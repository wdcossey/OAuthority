using OAuthority;

namespace OAuthority.Providers;

/// <remarks>
/// Requires <see cref="ProviderConfig.TenantId"/> to be set to your Okta domain,
/// e.g. <c>your-org.okta.com</c>.
/// </remarks>
internal sealed class OktaOAuthProvider(ProviderConfig config) : IOAuthProvider
{
    private static readonly IReadOnlyList<string> DefaultScopes = ["openid", "email", "profile"];

    private readonly string _domain = config.TenantId
        ?? throw new ArgumentException("ProviderConfig.TenantId is required for Okta (set to your Okta domain, e.g. your-org.okta.com).", nameof(config));

    public string Name => "Okta";

    public OAuthOptions Options => new OAuthOptions
    {
        AuthorizationEndpoint = $"https://{_domain}/oauth2/v1/authorize",
        TokenEndpoint = $"https://{_domain}/oauth2/v1/token",
        ClientId = config.ClientId,
        ClientSecret = config.ClientSecret,
        RedirectUri = config.RedirectUri ?? "http://localhost:5000/callback",
        Scopes = config.Scopes ?? DefaultScopes,
        UsePkce = config.UsePkce,
        ExtraParameters = config.ExtraParameters ?? new Dictionary<string, string>(),
    };
}
