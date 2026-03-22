using OAuthority;
using PhotinoNET;

namespace OAuthority.Browser.Photino;

/// <summary>
/// Embedded browser handler using Photino (WebKit on Linux/macOS, WebView2 on Windows).
///
/// Applies known fixes:
///   - Sets a valid WebKit user agent (fixes Monaco and other UA-sniffing JS libraries on Linux)
///   - Intercepts redirect URI via NavigationStarting without requiring a local HTTP server
/// </summary>
public class PhinoBrowserHandler : IBrowserHandler
{
    private readonly PhinoBrowserOptions _options;

    public PhinoBrowserHandler(PhinoBrowserOptions? options = null)
    {
        _options = options ?? new PhinoBrowserOptions();
    }

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
                var window = new PhotinoWindow()
                    .SetTitle(_options.Title)
                    .SetWidth(_options.Width)
                    .SetHeight(_options.Height)
                    .SetResizable(_options.Resizable)
                    .SetDevToolsEnabled(_options.ShowDevTools)
                    // FIX: null = use default WebKit UA which includes Mozilla/5.0, AppleWebKit, KHTML
                    // Setting a bare product-name UA (e.g. "Photino WebView") breaks coordinate
                    // handling in JS libraries like Monaco that use UA sniffing for browser detection.
                    // See: https://github.com/tryphotino/photino.NET/issues/???
                    .SetUserAgent(null);

                window.RegisterWebMessageReceivedHandler((sender, message) => { });

                window.NavigationStarting += (sender, url) =>
                {
                    if (url != null && url.StartsWith(redirectUri, StringComparison.OrdinalIgnoreCase))
                    {
                        tcs.TrySetResult(url);
                        ((PhotinoWindow)sender!).Close();
                    }
                };

                window.Load(authorizationUrl);
                window.WaitForClose();
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();

        return tcs.Task;
    }
}

public record PhinoBrowserOptions
{
    public string Title { get; init; } = "Sign In";
    public int Width { get; init; } = 800;
    public int Height { get; init; } = 600;
    public bool Resizable { get; init; } = true;
    public bool ShowDevTools { get; init; } = false;
}