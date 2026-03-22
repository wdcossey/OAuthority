using OAuthority;

namespace OAuthority.Providers;

internal sealed class GoogleOAuthProvider(ProviderConfig config) : IOAuthProvider
{
    private static readonly IReadOnlyList<string> DefaultScopes = ["openid", "email", "profile"];

    public string Name => "Google";

    public OAuthOptions Options { get; } = new OAuthOptions
    {
        AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth",
        TokenEndpoint = "https://oauth2.googleapis.com/token",
        ClientId = config.ClientId,
        ClientSecret = config.ClientSecret,
        RedirectUri = config.RedirectUri ?? "http://localhost:5000/callback",
        Scopes = config.Scopes ?? DefaultScopes,
        UsePkce = config.UsePkce,
        ExtraParameters = Merge(new Dictionary<string, string>
        {
            ["access_type"] = "offline",
            ["prompt"] = "consent",
        }, config.ExtraParameters),
    };

    private static IReadOnlyDictionary<string, string> Merge(
        Dictionary<string, string> defaults,
        IReadOnlyDictionary<string, string>? overrides)
    {
        if (overrides is null) return defaults;
        foreach (var (k, v) in overrides) defaults[k] = v;
        return defaults;
    }
}
