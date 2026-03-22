using System.Text.Json;
using OAuthority;

namespace OAuthority.Providers;

/// <remarks>
/// Resolves endpoints from an OIDC discovery document at
/// <c>{DiscoveryUrl}/.well-known/openid-configuration</c>.
/// Use <see cref="OAuthProviderFactory.CreateAsync"/> to instantiate this provider.
/// </remarks>
internal sealed class GenericOAuthProvider(ProviderConfig config, string authorizationEndpoint, string tokenEndpoint) : IOAuthProvider
{
    private static readonly IReadOnlyList<string> DefaultScopes = ["openid", "email", "profile"];

    public string Name => "Generic";

    public OAuthOptions Options { get; } = new OAuthOptions
    {
        AuthorizationEndpoint = authorizationEndpoint,
        TokenEndpoint = tokenEndpoint,
        ClientId = config.ClientId,
        ClientSecret = config.ClientSecret,
        RedirectUri = config.RedirectUri ?? "http://localhost:5000/callback",
        Scopes = config.Scopes ?? DefaultScopes,
        UsePkce = config.UsePkce,
        ExtraParameters = config.ExtraParameters ?? new Dictionary<string, string>(),
    };

    internal static async Task<GenericOAuthProvider> CreateAsync(
        ProviderConfig config,
        CancellationToken cancellationToken = default)
    {
        var discoveryUrl = config.DiscoveryUrl
            ?? throw new ArgumentException("ProviderConfig.DiscoveryUrl is required for the Generic OIDC provider.", nameof(config));

        var metadataUrl = $"{discoveryUrl.TrimEnd('/')}/.well-known/openid-configuration";

        using var http = new HttpClient();
        var json = await http.GetStringAsync(metadataUrl, cancellationToken);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var authEndpoint = root.GetProperty("authorization_endpoint").GetString()
            ?? throw new OAuthException("OIDC discovery document missing 'authorization_endpoint'.");

        var tokenEndpoint = root.GetProperty("token_endpoint").GetString()
            ?? throw new OAuthException("OIDC discovery document missing 'token_endpoint'.");

        return new GenericOAuthProvider(config, authEndpoint, tokenEndpoint);
    }
}
