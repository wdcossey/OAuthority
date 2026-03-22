using OAuthority;

namespace OAuthority.Providers;

internal sealed class TwitchOAuthProvider(ProviderConfig config) : IOAuthProvider
{
    private static readonly IReadOnlyList<string> DefaultScopes = ["user:read:email"];

    public string Name => "Twitch";

    public OAuthOptions Options { get; } = new OAuthOptions
    {
        AuthorizationEndpoint = "https://id.twitch.tv/oauth2/authorize",
        TokenEndpoint = "https://id.twitch.tv/oauth2/token",
        ClientId = config.ClientId,
        ClientSecret = config.ClientSecret,
        RedirectUri = config.RedirectUri ?? "http://localhost:5000/callback",
        Scopes = config.Scopes ?? DefaultScopes,
        UsePkce = config.UsePkce,
        ExtraParameters = config.ExtraParameters ?? new Dictionary<string, string>(),
    };
}
