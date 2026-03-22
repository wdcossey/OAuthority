# OAuthority

Cross-platform OAuth2/OIDC authentication library for .NET — supports desktop (Blazor, Avalonia, WPF), CLI, and TUI applications.

> ⚠️ Early alpha — API is unstable and subject to change.

## Packages

| Package | Description |
|---|---|
| `OAuthority` | Core abstractions, OAuth2 flow, PKCE |
| `OAuthority.Browser` | Browser handler abstraction + `Process.Start` fallback |
| `OAuthority.Browser.Native` | Embedded WebKit (Linux/macOS) / WebView2 (Windows) |
| `OAuthority.Browser.Photino` | Photino-based embedded browser |
| `OAuthority.Providers.Google` | Google OAuth2/OIDC provider |
| `OAuthority.Providers.Microsoft` | Microsoft/Entra OAuth2/OIDC provider |
| `OAuthority.Providers.GitHub` | GitHub OAuth2 provider |

## Quick Start

### System browser (simplest, works everywhere)

```csharp
var client = new OAuthorityClient(
    new GoogleOAuthProvider(new GoogleOAuthOptions
    {
        ClientId = "your-client-id",
        Scopes = ["openid", "email", "profile"],
    }),
    new SystemBrowserHandler(port: 5000)
);

var result = await client.AuthenticateAsync();
Console.WriteLine($"Access token: {result.AccessToken}");
```

### Embedded browser (Photino)

```csharp
var client = new OAuthorityClient(
    new GoogleOAuthProvider(new GoogleOAuthOptions
    {
        ClientId = "your-client-id",
    }),
    new PhinoBrowserHandler(new PhinoBrowserOptions
    {
        Title = "Sign in with Google",
        Width = 500,
        Height = 700,
    })
);

var result = await client.AuthenticateAsync();
```

## Supported Providers

- Google (`OAuthority.Providers.Google`)
- Microsoft / Entra ID (`OAuthority.Providers.Microsoft`)
- GitHub (`OAuthority.Providers.GitHub`)
- Custom — implement `IOAuthProvider`

## Building the Native Library (Linux)

```bash
cd src/OAuthority.Browser.Native/Linux
sudo dnf install -y gtk3-devel webkit2gtk4.1-devel   # Fedora
# sudo apt-get install libgtk-3-dev libwebkit2gtk-4.1-dev  # Debian/Ubuntu
make build
```

## Known Issues & Fixes

### Fractional scaling (KDE HiDPI)

`gtk_widget_get_scale_factor()` returns an integer and cannot represent fractional
scaling (e.g. 125%). KDE stores the real value in `~/.config/kwinrc` under
`[Xwayland] Scale=1.25`.

**Fix:** `OAuthority.Browser.Native/Linux/OAuthority.Linux.cpp` reads kwinrc directly
for KDE environments and GSettings for GNOME.

**TODO:** Hook `notify::scale-factor` signal for dynamic per-monitor scale changes.

---

### NVIDIA + Wayland + WebKit compositing (Fedora/KDE)

WebKit's GPU process fails to create GBM buffers on RTX 5080 with NVIDIA 580.x drivers.

**Workaround:**
```bash
WEBKIT_DISABLE_COMPOSITING_MODE=1 GDK_BACKEND=x11 dotnet run
```

Expected to resolve with future NVIDIA driver updates.

## License

Apache 2.0 — see [LICENSE](LICENSE)