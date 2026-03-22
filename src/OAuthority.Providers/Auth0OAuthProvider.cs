using OAuthority;

namespace OAuthority.Providers;

/// <remarks>
/// Requires <see cref="ProviderConfig.TenantId"/> to be set to your Auth0 domain,
/// e.g. <c>your-app.us.auth0.com</c>.
/// </remarks>
internal sealed class Auth0OAuthProvider(ProviderConfig config) : IOAuthProvider
{
    private static readonly IReadOnlyList<string> DefaultScopes = ["openid", "email", "profile"];

    private readonly string _domain = config.TenantId
        ?? throw new ArgumentException("ProviderConfig.TenantId is required for Auth0 (set to your Auth0 domain, e.g. your-app.us.auth0.com).", nameof(config));

    public string Name => "Auth0";

    public OAuthOptions Options => new OAuthOptions
    {
        AuthorizationEndpoint = $"https://{_domain}/authorize",
        TokenEndpoint = $"https://{_domain}/oauth/token",
        ClientId = config.ClientId,
        ClientSecret = config.ClientSecret,
        RedirectUri = config.RedirectUri ?? "http://localhost:5000/callback",
        Scopes = config.Scopes ?? DefaultScopes,
        UsePkce = config.UsePkce,
        ExtraParameters = config.ExtraParameters ?? new Dictionary<string, string>(),
    };
}
