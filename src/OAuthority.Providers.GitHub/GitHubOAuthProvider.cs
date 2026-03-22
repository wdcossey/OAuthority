using OAuthority;

namespace OAuthority.Providers;

public record GitHubOAuthOptions
{
    public required string ClientId { get; init; }
    public string? ClientSecret { get; init; }
    public IReadOnlyList<string> Scopes { get; init; } = ["read:user", "user:email"];
    public int LocalPort { get; init; } = 5000;
}

public class GitHubOAuthProvider(GitHubOAuthOptions options) : IOAuthProvider
{
    public string Name => "GitHub";

    public OAuthOptions Options { get; } = new OAuthOptions
    {
        AuthorizationEndpoint = "https://github.com/login/oauth/authorize",
        TokenEndpoint = "https://github.com/login/oauth/access_token",
        ClientId = options.ClientId,
        ClientSecret = options.ClientSecret,
        RedirectUri = $"http://localhost:{options.LocalPort}/callback",
        Scopes = options.Scopes,
        UsePkce = false, // GitHub doesn't support PKCE yet
    };
}