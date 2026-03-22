using OAuthority;

namespace OAuthority.Providers;

internal sealed class FacebookOAuthProvider(ProviderConfig config) : IOAuthProvider
{
    private static readonly IReadOnlyList<string> DefaultScopes = ["public_profile", "email"];

    public string Name => "Facebook";

    public OAuthOptions Options { get; } = new OAuthOptions
    {
        AuthorizationEndpoint = "https://www.facebook.com/v21.0/dialog/oauth",
        TokenEndpoint = "https://graph.facebook.com/v21.0/oauth/access_token",
        ClientId = config.ClientId,
        ClientSecret = config.ClientSecret,
        RedirectUri = config.RedirectUri ?? "http://localhost:5000/callback",
        Scopes = config.Scopes ?? DefaultScopes,
        UsePkce = config.UsePkce,
        ExtraParameters = config.ExtraParameters ?? new Dictionary<string, string>(),
    };
}
