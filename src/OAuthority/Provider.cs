namespace OAuthority;

/// <summary>
/// Well-known OAuth2/OIDC identity providers.
/// </summary>
public enum Provider
{
    Google,
    Microsoft,
    GitHub,
    Apple,
    Facebook,
    Discord,
    Slack,
    Spotify,
    Keycloak,
    Auth0,
    Okta,
    GitLab,
    Twitter,
    LinkedIn,
    Twitch,
    Amazon,

    /// <summary>
    /// Generic OIDC provider. Requires <see cref="ProviderConfig.DiscoveryUrl"/> to be set.
    /// Use <c>OAuthProviderFactory.CreateAsync</c> to resolve endpoints from the discovery document.
    /// </summary>
    Generic,
}

/// <summary>
/// Unified configuration for any <see cref="Provider"/>.
/// </summary>
public record ProviderConfig
{
    /// <summary>The client ID registered with the identity provider.</summary>
    public required string ClientId { get; init; }

    /// <summary>Optional client secret.</summary>
    public string? ClientSecret { get; init; }

    /// <summary>
    /// Provider-specific tenant/realm/domain identifier.
    /// <list type="bullet">
    ///   <item><term>Microsoft</term><description>Tenant ID or name (e.g. <c>common</c>, <c>contoso.onmicrosoft.com</c>).</description></item>
    ///   <item><term>Auth0</term><description>Auth0 domain (e.g. <c>your-app.us.auth0.com</c>).</description></item>
    ///   <item><term>Okta</term><description>Okta domain (e.g. <c>your-org.okta.com</c>).</description></item>
    ///   <item><term>Keycloak</term><description>Full realm URL (e.g. <c>https://keycloak.example.com/realms/myrealm</c>).</description></item>
    /// </list>
    /// </summary>
    public string? TenantId { get; init; }

    /// <summary>
    /// Provider-specific base or discovery URL.
    /// <list type="bullet">
    ///   <item><term>Generic</term><description>OIDC discovery base URL — endpoints are resolved from <c>{DiscoveryUrl}/.well-known/openid-configuration</c>.</description></item>
    ///   <item><term>GitLab (self-hosted)</term><description>GitLab server base URL (defaults to <c>https://gitlab.com</c> if null).</description></item>
    /// </list>
    /// </summary>
    public string? DiscoveryUrl { get; init; }

    /// <summary>Requested OAuth2 scopes. If null, the provider's default scopes are used.</summary>
    public IReadOnlyList<string>? Scopes { get; init; }

    /// <summary>
    /// Full redirect URI (e.g. <c>http://localhost:5000/callback</c>).
    /// If null, defaults to <c>http://localhost:5000/callback</c>.
    /// </summary>
    public string? RedirectUri { get; init; }

    /// <summary>
    /// Use PKCE (Proof Key for Code Exchange). Defaults to <c>true</c>.
    /// Ignored for providers that do not support PKCE (GitHub, LinkedIn, Amazon).
    /// Always enforced for providers that require PKCE (Twitter/X).
    /// </summary>
    public bool UsePkce { get; init; } = true;

    /// <summary>Extra query parameters appended to the authorization URL, merged with provider defaults.</summary>
    public IReadOnlyDictionary<string, string>? ExtraParameters { get; init; }
}
