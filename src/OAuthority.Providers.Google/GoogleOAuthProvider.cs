using OAuthority;

namespace OAuthority.Providers;

public record GoogleOAuthOptions
{
    public required string ClientId { get; init; }
    public string? ClientSecret { get; init; }
    public IReadOnlyList<string> Scopes { get; init; } = ["openid", "email", "profile"];
    public int LocalPort { get; init; } = 5000;
}

public class GoogleOAuthProvider(GoogleOAuthOptions options) : IOAuthProvider
{
    public string Name => "Google";

    public OAuthOptions Options { get; } = new OAuthOptions
    {
        AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth",
        TokenEndpoint = "https://oauth2.googleapis.com/token",
        ClientId = options.ClientId,
        ClientSecret = options.ClientSecret,
        RedirectUri = $"http://localhost:{options.LocalPort}/callback",
        Scopes = options.Scopes,
        UsePkce = true,
        ExtraParameters = new Dictionary<string, string>
        {
            ["access_type"] = "offline",
            ["prompt"] = "consent",
        }
    };
}