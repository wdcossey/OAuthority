namespace OAuthority;

/// <summary>
/// Options for an OAuth2/OIDC authentication flow.
/// </summary>
public record OAuthOptions
{
    /// <summary>The authorization endpoint URI.</summary>
    public required string AuthorizationEndpoint { get; init; }

    /// <summary>The token endpoint URI.</summary>
    public required string TokenEndpoint { get; init; }

    /// <summary>The client ID registered with the identity provider.</summary>
    public required string ClientId { get; init; }

    /// <summary>The redirect URI to intercept after authentication.</summary>
    public required string RedirectUri { get; init; }

    /// <summary>Requested OAuth2 scopes.</summary>
    public IReadOnlyList<string> Scopes { get; init; } = [];

    /// <summary>Optional client secret (not recommended for desktop apps — use PKCE).</summary>
    public string? ClientSecret { get; init; }

    /// <summary>Use PKCE (Proof Key for Code Exchange). Recommended for desktop apps.</summary>
    public bool UsePkce { get; init; } = true;

    /// <summary>Optional extra query parameters to append to the authorization URL.</summary>
    public IReadOnlyDictionary<string, string> ExtraParameters { get; init; }
        = new Dictionary<string, string>();
}

/// <summary>
/// The result of a successful OAuth2 authentication flow.
/// </summary>
public record OAuthResult
{
    public required string AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public string? IdToken { get; init; }
    public string? TokenType { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
    public IReadOnlyList<string> Scopes { get; init; } = [];

    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value <= DateTimeOffset.UtcNow;
}

/// <summary>
/// Exception thrown when an OAuth2 authentication flow fails.
/// </summary>
public class OAuthException(string message, string? error = null, string? errorDescription = null)
    : Exception(message)
{
    public string? Error { get; } = error;
    public string? ErrorDescription { get; } = errorDescription;
}