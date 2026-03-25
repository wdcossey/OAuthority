using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;

namespace OAuthority;

/// <summary>
/// Abstracts an OAuth2/OIDC identity provider.
/// </summary>
public interface IOAuthProvider
{
    string Name { get; }
    OAuthOptions Options { get; }
}

/// <summary>
/// Abstraction for the browser used to show the authorization flow.
/// Implement this to provide a custom browser experience.
/// </summary>
public interface IBrowserHandler
{
    /// <summary>
    /// Opens the authorization URL and waits until a navigation matching
    /// <paramref name="redirectUri"/> is detected.
    /// </summary>
    /// <param name="authorizationUrl">The full authorization URL to open.</param>
    /// <param name="redirectUri">The redirect URI prefix to intercept.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The full redirect URL including query string (contains the auth code).</returns>
    Task<string> InvokeAsync(
        string authorizationUrl,
        string redirectUri,
        CancellationToken cancellationToken = default);
}


/// <summary>
/// Main entry point for OAuthority authentication flows.
/// </summary>
public class OAuthorityClient(IOAuthProvider provider, IBrowserHandler browserHandler)
{
    private readonly IOAuthProvider _provider = provider;
    private readonly IBrowserHandler _browserHandler = browserHandler;

    /// <summary>
    /// Runs the full OAuth2 authorization code flow with optional PKCE.
    /// </summary>
    public async Task<OAuthResult> AuthenticateAsync(CancellationToken cancellationToken = default)
    {
        var options = _provider.Options;

        // Generate PKCE if enabled
        string? codeVerifier = null;
        string? codeChallenge = null;
        if (options.UsePkce)
        {
            codeVerifier = PkceHelper.GenerateCodeVerifier();
            codeChallenge = PkceHelper.GenerateCodeChallenge(codeVerifier);
        }

        // Build authorization URL
        var state = Guid.NewGuid().ToString("N");
        var authUrl = BuildAuthorizationUrl(options, state, codeChallenge);

        // Open browser and wait for redirect
        var redirectUrl = await _browserHandler.InvokeAsync(authUrl, options.RedirectUri, cancellationToken);

        // Parse the authorization code from the redirect
        var code = ParseAuthorizationCode(redirectUrl, state);

        // Exchange code for tokens
        return await ExchangeCodeForTokenAsync(options, code, codeVerifier, cancellationToken);
    }

    private static string BuildAuthorizationUrl(OAuthOptions options, string state, string? codeChallenge)
    {
        var query = new Dictionary<string, string>
        {
            ["response_type"] = "code",
            ["client_id"] = options.ClientId,
            ["redirect_uri"] = options.RedirectUri,
            ["scope"] = string.Join(" ", options.Scopes),
            ["state"] = state,
        };

        if (codeChallenge != null)
        {
            query["code_challenge"] = codeChallenge;
            query["code_challenge_method"] = "S256";
        }

        foreach (var (key, value) in options.ExtraParameters)
            query[key] = value;

        var queryString = string.Join("&", query.Select(kvp =>
            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        return $"{options.AuthorizationEndpoint}?{queryString}";
    }

    private static string ParseAuthorizationCode(string redirectUrl, string expectedState)
    {
        var uri = new Uri(redirectUrl);
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

        var error = query["error"];
        if (error != null)
            throw new OAuthException($"Authorization failed: {error}", error, query["error_description"]);

        var state = query["state"];
        if (state != expectedState)
            throw new OAuthException("State mismatch — possible CSRF attack.");

        var code = query["code"]
            ?? throw new OAuthException("No authorization code in redirect response.");

        return code;
    }

    private static async Task<OAuthResult> ExchangeCodeForTokenAsync(
        OAuthOptions options,
        string code,
        string? codeVerifier,
        CancellationToken cancellationToken)
    {
        using var http = new HttpClient();

        var body = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = options.RedirectUri,
            ["client_id"] = options.ClientId,
        };

        if (options.ClientSecret != null)
            body["client_secret"] = options.ClientSecret;

        if (codeVerifier != null)
            body["code_verifier"] = codeVerifier;

        var request = new HttpRequestMessage(HttpMethod.Post, options.TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(body)
        };

        request.Headers.Add("Accept", MediaTypeNames.Application.Json);

        var response = await http.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return TokenResponseParser.Parse(json);
    }
}