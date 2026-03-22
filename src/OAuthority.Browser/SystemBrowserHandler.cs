using System.Diagnostics;
using System.Net;
using OAuthority;

namespace OAuthority.Browser;

/// <summary>
/// Opens the authorization URL in the system default browser and listens
/// for the redirect on a local HTTP listener. Suitable for redirect URIs
/// like http://localhost:{port}/callback
/// </summary>
public class SystemBrowserHandler : IBrowserHandler
{
    private readonly int _port;

    public SystemBrowserHandler(int port = 5000)
    {
        _port = port;
    }

    public async Task<string> InvokeAsync(
        string authorizationUrl,
        string redirectUri,
        CancellationToken cancellationToken = default)
    {
        using var listener = new HttpListener();
        listener.Prefixes.Add($"http://localhost:{_port}/");
        listener.Start();

        // Open the system browser
        OpenBrowser(authorizationUrl);

        // Wait for the redirect callback
        var context = await listener.GetContextAsync().WaitAsync(cancellationToken);
        var request = context.Request;

        // Acknowledge to the browser
        var response = context.Response;
        var html = "<html><body><h2>Authentication complete. You may close this window.</h2></body></html>"u8.ToArray();
        response.ContentLength64 = html.Length;
        response.ContentType = "text/html";
        await response.OutputStream.WriteAsync(html, cancellationToken);
        response.Close();

        return request.Url?.ToString()
            ?? throw new OAuthException("No redirect URL received.");
    }

    private static void OpenBrowser(string url)
    {
        if (OperatingSystem.IsWindows())
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        else if (OperatingSystem.IsLinux())
        {
            Process.Start("xdg-open", url);
        }
        else if (OperatingSystem.IsMacOS())
        {
            Process.Start("open", url);
        }
    }
}