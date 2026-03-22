using OAuthority;

namespace OAuthority.Providers;

/// <summary>
/// Creates <see cref="IOAuthProvider"/> instances from a <see cref="Provider"/> enum value
/// and a <see cref="ProviderConfig"/>.
/// </summary>
public static class OAuthProviderFactory
{
    /// <summary>
    /// Creates an <see cref="IOAuthProvider"/> for the specified <paramref name="provider"/>.
    /// </summary>
    /// <exception cref="NotSupportedException">
    /// Thrown for <see cref="Provider.Generic"/> — use <see cref="CreateAsync"/> instead,
    /// as the Generic provider requires fetching an OIDC discovery document.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when required <see cref="ProviderConfig"/> fields are missing for the chosen provider.
    /// </exception>
    public static IOAuthProvider Create(Provider provider, ProviderConfig config) => provider switch
    {
        Provider.Google    => new GoogleOAuthProvider(config),
        Provider.Microsoft => new MicrosoftOAuthProvider(config),
        Provider.GitHub    => new GitHubOAuthProvider(config),
        Provider.Apple     => new AppleOAuthProvider(config),
        Provider.Facebook  => new FacebookOAuthProvider(config),
        Provider.Discord   => new DiscordOAuthProvider(config),
        Provider.Slack     => new SlackOAuthProvider(config),
        Provider.Spotify   => new SpotifyOAuthProvider(config),
        Provider.Keycloak  => new KeycloakOAuthProvider(config),
        Provider.Auth0     => new Auth0OAuthProvider(config),
        Provider.Okta      => new OktaOAuthProvider(config),
        Provider.GitLab    => new GitLabOAuthProvider(config),
        Provider.Twitter   => new TwitterOAuthProvider(config),
        Provider.LinkedIn  => new LinkedInOAuthProvider(config),
        Provider.Twitch    => new TwitchOAuthProvider(config),
        Provider.Amazon    => new AmazonOAuthProvider(config),
        Provider.Generic   => throw new NotSupportedException(
            $"Use {nameof(CreateAsync)} for {nameof(Provider.Generic)} — it requires fetching an OIDC discovery document."),
        _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, "Unknown provider."),
    };

    /// <summary>
    /// Creates an <see cref="IOAuthProvider"/> for the specified <paramref name="provider"/>,
    /// fetching the OIDC discovery document when <see cref="Provider.Generic"/> is used.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when required <see cref="ProviderConfig"/> fields are missing for the chosen provider.
    /// </exception>
    public static async Task<IOAuthProvider> CreateAsync(
        Provider provider,
        ProviderConfig config,
        CancellationToken cancellationToken = default)
    {
        if (provider == Provider.Generic)
            return await GenericOAuthProvider.CreateAsync(config, cancellationToken);

        return Create(provider, config);
    }
}
