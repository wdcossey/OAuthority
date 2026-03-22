using OAuthority;

namespace OAuthority.Providers;

/// <remarks>
/// Sign in with Apple uses <c>response_mode=form_post</c>, meaning the authorization code is
/// delivered as an HTTP POST to the redirect URI rather than a GET with query parameters.
/// The <c>SystemBrowserHandler</c> HTTP listener must handle POST requests for this provider.
/// The <c>Photino</c> and <c>Native</c> browser handlers intercept navigation directly and
/// are not affected.
/// </remarks>
internal sealed class AppleOAuthProvider(ProviderConfig config) : IOAuthProvider
{
    private static readonly IReadOnlyList<string> DefaultScopes = ["openid", "name", "email"];

    public string Name => "Apple";

    public OAuthOptions Options { get; } = new OAuthOptions
    {
        AuthorizationEndpoint = "https://appleid.apple.com/auth/authorize",
        TokenEndpoint = "https://appleid.apple.com/auth/token",
        ClientId = config.ClientId,
        ClientSecret = config.ClientSecret,
        RedirectUri = config.RedirectUri ?? "http://localhost:5000/callback",
        Scopes = config.Scopes ?? DefaultScopes,
        UsePkce = config.UsePkce,
        ExtraParameters = Merge(new Dictionary<string, string>
        {
            ["response_mode"] = "form_post",
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
