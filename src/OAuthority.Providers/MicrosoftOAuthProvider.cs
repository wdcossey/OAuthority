using OAuthority;

namespace OAuthority.Providers;

internal sealed class MicrosoftOAuthProvider(ProviderConfig config) : IOAuthProvider
{
    private static readonly IReadOnlyList<string> DefaultScopes = ["openid", "email", "profile", "offline_access"];

    private readonly string _tenant = config.TenantId ?? "common";

    public string Name => "Microsoft";

    public OAuthOptions Options => new OAuthOptions
    {
        AuthorizationEndpoint = $"https://login.microsoftonline.com/{_tenant}/oauth2/v2.0/authorize",
        TokenEndpoint = $"https://login.microsoftonline.com/{_tenant}/oauth2/v2.0/token",
        ClientId = config.ClientId,
        ClientSecret = config.ClientSecret,
        RedirectUri = config.RedirectUri ?? "http://localhost:5000/callback",
        Scopes = config.Scopes ?? DefaultScopes,
        UsePkce = config.UsePkce,
        ExtraParameters = config.ExtraParameters ?? new Dictionary<string, string>(),
    };
}
