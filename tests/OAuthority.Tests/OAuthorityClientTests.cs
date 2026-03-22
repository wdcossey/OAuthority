using OAuthority;
using NSubstitute;
using Xunit;

namespace OAuthority.Tests;

public class OAuthorityClientTests
{
    [Fact]
    public async Task AuthenticateAsync_CallsBrowserHandler_WithCorrectAuthUrl()
    {
        // Arrange
        var browserHandler = Substitute.For<IBrowserHandler>();
        var provider = new TestProvider();

        // Simulate a redirect response
        browserHandler
            .InvokeAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("http://localhost:5000/callback?code=test_code&state=placeholder"));

        var client = new OAuthorityClient(provider, browserHandler);

        // Act & Assert — token exchange will fail (no real server) but we can verify the browser was called
        await Assert.ThrowsAnyAsync<Exception>(() => client.AuthenticateAsync());

        await browserHandler.Received(1).InvokeAsync(
            Arg.Is<string>(url => url.Contains("response_type=code")),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public void PkceHelper_GeneratesUniqueVerifiers()
    {
        var v1 = PkceHelperAccessor.GenerateCodeVerifier();
        var v2 = PkceHelperAccessor.GenerateCodeVerifier();
        Assert.NotEqual(v1, v2);
    }

    [Fact]
    public void PkceHelper_ChallengeIsDeterministic()
    {
        var verifier = PkceHelperAccessor.GenerateCodeVerifier();
        var c1 = PkceHelperAccessor.GenerateCodeChallenge(verifier);
        var c2 = PkceHelperAccessor.GenerateCodeChallenge(verifier);
        Assert.Equal(c1, c2);
    }

    private class TestProvider : IOAuthProvider
    {
        public string Name => "Test";
        public OAuthOptions Options { get; } = new OAuthOptions
        {
            AuthorizationEndpoint = "https://example.com/auth",
            TokenEndpoint = "https://example.com/token",
            ClientId = "test_client",
            RedirectUri = "http://localhost:5000/callback",
            Scopes = ["openid"],
        };
    }
}

// Expose internals for testing
internal static class PkceHelperAccessor
{
    public static string GenerateCodeVerifier() => PkceHelper.GenerateCodeVerifier();
    public static string GenerateCodeChallenge(string v) => PkceHelper.GenerateCodeChallenge(v);
}