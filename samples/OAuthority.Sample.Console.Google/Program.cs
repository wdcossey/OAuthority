using OAuthority;
using OAuthority.Browser;
using OAuthority.Providers;

// Read credentials from env vars or command-line args
var clientId     = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID")     ?? args.ElementAtOrDefault(0);
var clientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET") ?? args.ElementAtOrDefault(1);

if (string.IsNullOrWhiteSpace(clientId))
{
    Console.Error.WriteLine("Google Client ID is required.");
    Console.Error.WriteLine("  Set GOOGLE_CLIENT_ID env var, or pass as first argument.");
    return 1;
}

if (string.IsNullOrWhiteSpace(clientSecret))
{
    Console.Error.WriteLine("Google Client Secret is required.");
    Console.Error.WriteLine("  Set GOOGLE_CLIENT_SECRET env var, or pass as second argument.");
    return 1;
}

const int port = 5001;

var provider = new GoogleOAuthProvider(new GoogleOAuthOptions
{
    ClientId     = clientId,
    ClientSecret = clientSecret,
    LocalPort    = port,
    Scopes       = ["openid", "email", "profile"],
});

var browser = new SystemBrowserHandler(port);
var client  = new OAuthorityClient(provider, browser);

Console.WriteLine("Opening browser for Google authentication...");
Console.WriteLine($"Listening for redirect on http://localhost:{port}/");
Console.WriteLine();

try
{
    var result = await client.AuthenticateAsync();

    Console.WriteLine("Authentication successful!");
    Console.WriteLine($"  Token type:    {result.TokenType}");
    Console.WriteLine($"  Access token:  {result.AccessToken[..8]}...");
    Console.WriteLine($"  ID token:      {(result.IdToken != null ? result.IdToken[..8] + "..." : "not present")}");
    Console.WriteLine($"  Refresh token: {(result.RefreshToken != null ? result.RefreshToken[..8] + "..." : "not present")}");
    Console.WriteLine($"  Expires at:    {result.ExpiresAt?.ToString("u") ?? "not specified"}");
    if (result.Scopes.Count > 0)
        Console.WriteLine($"  Scopes:        {string.Join(", ", result.Scopes)}");
}
catch (OAuthException ex)
{
    Console.Error.WriteLine($"Authentication failed: {ex.Message}");
    if (ex.ErrorDescription != null)
        Console.Error.WriteLine($"  Details: {ex.ErrorDescription}");
    return 1;
}

return 0;