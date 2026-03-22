using OAuthority;

namespace OAuthority.Providers;

/// <remarks>
/// Requires <see cref="ProviderConfig.TenantId"/> to be set to the full Keycloak realm URL,
/// e.g. <c>https://keycloak.example.com/realms/myrealm</c>.
/// </remarks>
internal sealed class KeycloakOAuthProvider(ProviderConfig config) : IOAuthProvider
{
    private static readonly IReadOnlyList<string> DefaultScopes = ["openid", "email", "profile"];

    private readonly string _realmUrl = config.TenantId
        ?? throw new ArgumentException("ProviderConfig.TenantId is required for Keycloak (set to the full realm URL, e.g. https://keycloak.example.com/realms/myrealm).", nameof(config));

    public string Name => "Keycloak";

    public OAuthOptions Options => new OAuthOptions
    {
        AuthorizationEndpoint = $"{_realmUrl}/protocol/openid-connect/auth",
        TokenEndpoint = $"{_realmUrl}/protocol/openid-connect/token",
        ClientId = config.ClientId,
        ClientSecret = config.ClientSecret,
        RedirectUri = config.RedirectUri ?? "http://localhost:5000/callback",
        Scopes = config.Scopes ?? DefaultScopes,
        UsePkce = config.UsePkce,
        ExtraParameters = config.ExtraParameters ?? new Dictionary<string, string>(),
    };
}
