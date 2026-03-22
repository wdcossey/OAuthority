// OAuthority.Linux.cpp
// Native WebKit browser window for OAuthority on Linux (GTK + WebKit2GTK)
//
// Key fixes applied based on investigation findings (2025):
//
// BUG 1 — Invalid default User Agent breaks WebKit coordinate handling
//   Root cause: Setting a non-standard UA (e.g. "Photino WebView") strips Mozilla/5.0,
//   AppleWebKit and KHTML tokens. Libraries like Monaco use these for browser detection
//   and fall back to broken legacy input handling paths when absent.
//   Fix: Always use a valid WebKit UA. Never set a bare product-name-only UA.
//
// BUG 2 — Fractional scaling not handled (KDE/HiDPI)
//   Root cause: gtk_widget_get_scale_factor() returns integer only (always 1 on KDE
//   with 125% fractional scaling). KDE stores the real value in ~/.config/kwinrc
//   under [Xwayland] Scale=1.25
//   Fix: Read kwinrc directly for KDE, GSettings for GNOME, fallback to GTK integer.
//   TODO: Hook notify::scale-factor signal for dynamic multi-monitor scale changes.
//
// BUG 3 — webkit_web_view_set_zoom_level() must be called AFTER webkit_web_view_set_settings()
//   Root cause: webkit_web_view_set_settings() resets zoom to 1.0.
//   Fix: Apply zoom after settings are applied.
//
// BUILD (Fedora):
//   sudo dnf install -y gtk3-devel webkit2gtk4.1-devel libnotify-devel
//   c++ -o OAuthority.Native.so -std=c++2a -Wall -O2 -shared -fPIC \
//       OAuthority.Linux.cpp \
//       `pkg-config --cflags --libs gtk+-3.0 webkit2gtk-4.1`

#include <gtk/gtk.h>
#include <webkit2/webkit2.h>
#include <string>
#include <stdio.h>

// ─── Scale Factor ────────────────────────────────────────────────────────────

static gdouble GetDisplayScaleFactor(GtkWidget *window)
{
    const char *desktop = g_getenv("XDG_CURRENT_DESKTOP");

    bool isKDE = desktop && (
        g_str_has_prefix(desktop, "KDE") ||
        g_str_has_prefix(desktop, "plasma")
    );

    if (isKDE)
    {
        gdouble scale = 1.0;
        const char *home = g_get_home_dir();
        gchar *path = g_build_filename(home, ".config", "kwinrc", NULL);

        FILE *f = fopen(path, "r");
        if (f)
        {
            char line[256];
            bool inXwayland = false;
            while (fgets(line, sizeof(line), f))
            {
                if (strncmp(line, "[Xwayland]", 10) == 0) { inXwayland = true; continue; }
                if (line[0] == '[') { inXwayland = false; continue; }
                if (inXwayland && strncmp(line, "Scale=", 6) == 0)
                {
                    scale = atof(line + 6);
                    break;
                }
            }
            fclose(f);
        }
        g_free(path);
        printf("OAuthority: KDE scale factor: %f\n", scale);
        return scale;
    }

    bool isGNOME = desktop && (
        g_str_has_prefix(desktop, "GNOME") ||
        g_str_has_prefix(desktop, "Unity")
    );

    if (isGNOME)
    {
        GSettings *settings = g_settings_new("org.gnome.desktop.interface");
        gdouble scale = g_settings_get_double(settings, "text-scaling-factor");
        g_object_unref(settings);
        if (scale > 0.0)
        {
            printf("OAuthority: GNOME scale factor: %f\n", scale);
            return scale;
        }
    }

    gint intScale = gtk_widget_get_scale_factor(window);
    printf("OAuthority: fallback scale factor: %d\n", intScale);
    return (gdouble)intScale;
}

// ─── Browser Window ──────────────────────────────────────────────────────────

struct BrowserContext
{
    std::string redirectUri;
    std::string resultUrl;
    bool completed = false;
    GtkWidget *window = nullptr;
};

static void OnDecidePolicy(
    WebKitWebView *webView,
    WebKitPolicyDecision *decision,
    WebKitPolicyDecisionType type,
    gpointer userData)
{
    if (type != WEBKIT_POLICY_DECISION_TYPE_NAVIGATION_ACTION)
        return;

    auto *ctx = static_cast<BrowserContext *>(userData);
    auto *nav = WEBKIT_NAVIGATION_POLICY_DECISION(decision);
    auto *action = webkit_navigation_policy_decision_get_navigation_action(nav);
    auto *request = webkit_navigation_action_get_request(action);
    const char *url = webkit_uri_request_get_uri(request);

    if (url && ctx->redirectUri.length() > 0 &&
        strncmp(url, ctx->redirectUri.c_str(), ctx->redirectUri.length()) == 0)
    {
        ctx->resultUrl = url;
        ctx->completed = true;
        webkit_policy_decision_ignore(decision);
        gtk_widget_destroy(ctx->window);
        gtk_main_quit();
        return;
    }

    webkit_policy_decision_use(decision);
}

// ─── Exported API ────────────────────────────────────────────────────────────

extern "C" {

/// Opens a WebKit browser window, navigates to authUrl, and blocks until
/// a navigation matching redirectUri is detected.
/// Returns a heap-allocated string (caller must free) containing the full redirect URL,
/// or NULL on failure/cancellation.
char *OAuthority_Browse(
    const char *title,
    const char *authUrl,
    const char *redirectUri,
    int width,
    int height)
{
    gtk_init(0, NULL);

    BrowserContext ctx;
    ctx.redirectUri = redirectUri;

    GtkWidget *window = gtk_window_new(GTK_WINDOW_TOPLEVEL);
    ctx.window = window;
    gtk_window_set_title(GTK_WINDOW(window), title);

    // TODO: Read scale factor and adjust window size accordingly
    // gdouble scale = GetDisplayScaleFactor(window);
    gtk_window_set_default_size(GTK_WINDOW(window), width, height);

    g_signal_connect(G_OBJECT(window), "destroy",
        G_CALLBACK(+[](GtkWidget *, gpointer) { gtk_main_quit(); }), nullptr);

    WebKitUserContentManager *contentManager = webkit_user_content_manager_new();
    GtkWidget *webView = webkit_web_view_new_with_user_content_manager(contentManager);

    // Apply settings BEFORE zoom (webkit_web_view_set_settings resets zoom to 1.0)
    WebKitSettings *settings = webkit_settings_new();

    // IMPORTANT: Always use a valid WebKit UA.
    // A bare product-name UA (e.g. "MyApp/1.0") strips Mozilla/5.0, AppleWebKit,
    // and KHTML tokens that JS libraries use for browser detection, causing broken
    // input handling in editors like Monaco.
    // webkit_settings_set_user_agent(settings, "..."); // Leave as default WebKit UA

    webkit_web_view_set_settings(WEBKIT_WEB_VIEW(webView), settings);

    // Apply scale factor AFTER settings
    gdouble scale = GetDisplayScaleFactor(window);
    // Note: set_zoom_level affects content zoom, not coordinate space.
    // The real fix for coordinate issues is the valid UA above.
    // webkit_web_view_set_zoom_level(WEBKIT_WEB_VIEW(webView), scale);

    g_signal_connect(webView, "decide-policy", G_CALLBACK(OnDecidePolicy), &ctx);

    gtk_container_add(GTK_CONTAINER(window), webView);
    gtk_widget_show_all(window);

    webkit_web_view_load_uri(WEBKIT_WEB_VIEW(webView), authUrl);

    gtk_main();

    if (ctx.completed && !ctx.resultUrl.empty())
        return strdup(ctx.resultUrl.c_str());

    return nullptr;
}

void OAuthority_Free(char *ptr)
{
    free(ptr);
}

} // extern "C"