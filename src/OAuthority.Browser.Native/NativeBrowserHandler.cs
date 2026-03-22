using System.Runtime.InteropServices;
using OAuthority;

namespace OAuthority.Browser.Native;

/// <summary>
/// Embedded browser handler using a native WebKit (Linux/macOS) or WebView2 (Windows) window.
/// Intercepts the redirect URI without requiring a local HTTP server.
///
/// NOTE: The native library (OAuthority.Native.so/.dll/.dylib) must be built from
/// src/OAuthority.Browser.Native/Linux|Windows|macOS before this handler can be used.
/// </summary>
public class NativeBrowserHandler : IBrowserHandler
{
    private readonly NativeBrowserOptions _options;

    public NativeBrowserHandler(NativeBrowserOptions? options = null)
    {
        _options = options ?? new NativeBrowserOptions();
    }

    public Task<string> InvokeAsync(
        string authorizationUrl,
        string redirectUri,
        CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<string>();

        cancellationToken.Register(() => tcs.TrySetCanceled());

        // TODO: P/Invoke into OAuthority.Native
        // The native library will:
        //   1. Create a GTK/Win32/Cocoa window with an embedded WebView
        //   2. Navigate to authorizationUrl
        //   3. Monitor navigations for a URL starting with redirectUri
        //   4. Close the window and return the full redirect URL
        //
        // See: src/OAuthority.Browser.Native/Linux/OAuthority.Linux.cpp
        //      src/OAuthority.Browser.Native/Windows/OAuthority.Windows.cpp
        //      src/OAuthority.Browser.Native/macOS/OAuthority.Mac.mm

        throw new NotImplementedException(
            "Native browser handler not yet implemented. " +
            "Build the native library from src/OAuthority.Browser.Native first.");
    }
}

public record NativeBrowserOptions
{
    public string Title { get; init; } = "Sign In";
    public int Width { get; init; } = 800;
    public int Height { get; init; } = 600;
    public bool Resizable { get; init; } = true;
    public bool ShowDevTools { get; init; } = false;
}