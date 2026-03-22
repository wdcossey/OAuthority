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
        var html =
            """
            <html><body style="display:flex;flex-direction:column;align-items:center;justify-content:center;height:100vh;margin:0;font-family:sans-serif;"><img src="data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSI2NCIgaGVpZ2h0PSI2NCIgdmlld0JveD0iMCAwIDI0IDI0IiBmaWxsPSJub25lIiBzdHJva2U9IiMyMmM1NWUiIHN0cm9rZS13aWR0aD0iMiIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIj48cGF0aCBkPSJNMjIgMTEuMDhWMTJhMTAgMTAgMCAxIDEtNS45My05LjE0Ii8+PHBvbHlsaW5lIHBvaW50cz0iMjIgNCAxMiAxNC4wMSA5IDExLjAxIi8+PC9zdmc+" alt="Success" style="margin-bottom:20px;"><h2>Authentication complete. You may close this window.</h2></body></html>
            """u8.ToArray();
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