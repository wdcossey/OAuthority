using OAuthority;

namespace OAuthority.Providers;

/// <remarks>
/// For self-hosted GitLab, set <see cref="ProviderConfig.DiscoveryUrl"/> to your GitLab server base URL
/// (e.g. <c>https://gitlab.mycompany.com</c>). Defaults to <c>https://gitlab.com</c>.
/// </remarks>
internal sealed class GitLabOAuthProvider(ProviderConfig config) : IOAuthProvider
{
    private static readonly IReadOnlyList<string> DefaultScopes = ["read_user", "openid", "email"];

    private readonly string _baseUrl = (config.DiscoveryUrl ?? "https://gitlab.com").TrimEnd('/');

    public string Name => "GitLab";

    public OAuthOptions Options => new OAuthOptions
    {
        AuthorizationEndpoint = $"{_baseUrl}/oauth/authorize",
        TokenEndpoint = $"{_baseUrl}/oauth/token",
        ClientId = config.ClientId,
        ClientSecret = config.ClientSecret,
        RedirectUri = config.RedirectUri ?? "http://localhost:5000/callback",
        Scopes = config.Scopes ?? DefaultScopes,
        UsePkce = config.UsePkce,
        ExtraParameters = config.ExtraParameters ?? new Dictionary<string, string>(),
    };
}
