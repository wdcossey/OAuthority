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
    private const string NativeLib = "OAuthority.Native";

    private readonly NativeBrowserOptions _options;

    public NativeBrowserHandler(NativeBrowserOptions? options = null)
    {
        _options = options ?? new NativeBrowserOptions();
    }

    // Returns a heap-allocated string; caller must free via OAuthority_Free.
    // Using IntPtr to control lifetime and avoid automatic marshaling.
    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern IntPtr OAuthority_Browse(
        string title,
        string authUrl,
        string redirectUri,
        int width,
        int height);

    [DllImport(NativeLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void OAuthority_Free(IntPtr ptr);

    public Task<string> InvokeAsync(
        string authorizationUrl,
        string redirectUri,
        CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<string>();
        cancellationToken.Register(() => tcs.TrySetCanceled());

        var thread = new Thread(() =>
        {
            try
            {
                var resultPtr = OAuthority_Browse(
                    _options.Title,
                    authorizationUrl,
                    redirectUri,
                    _options.Width,
                    _options.Height);

                if (resultPtr == IntPtr.Zero)
                {
                    tcs.TrySetException(new OAuthException("Native browser was closed without completing authentication."));
                    return;
                }

                try
                {
                    var result = Marshal.PtrToStringAnsi(resultPtr)
                        ?? throw new OAuthException("Native browser returned an empty redirect URL.");
                    tcs.TrySetResult(result);
                }
                finally
                {
                    OAuthority_Free(resultPtr);
                }
            }
            catch (DllNotFoundException ex)
            {
                tcs.TrySetException(new InvalidOperationException(
                    $"Native browser library '{NativeLib}' not found. " +
                    "Build the native library from src/OAuthority.Browser.Native/Linux|Windows|macOS first.", ex));
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        });

        // GTK requires its event loop on a dedicated thread.
        // STA is only needed (and supported) on Windows for COM/WebView2 interop.
        if (OperatingSystem.IsWindows())
            thread.SetApartmentState(ApartmentState.STA);

        thread.IsBackground = true;
        thread.Start();

        return tcs.Task;
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