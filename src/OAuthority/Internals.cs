using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace OAuthority;

internal static class PkceHelper
{
    public static string GenerateCodeVerifier()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public static string GenerateCodeChallenge(string codeVerifier)
    {
        var bytes = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}

internal static class TokenResponseParser
{
    public static OAuthResult Parse(string json)
    {
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var accessToken = root.GetProperty("access_token").GetString()
            ?? throw new OAuthException("Missing access_token in token response.");

        var expiresIn = root.TryGetProperty("expires_in", out var exp)
            ? (int?)exp.GetInt32()
            : null;

        var scopes = root.TryGetProperty("scope", out var scope)
            ? (scope.GetString() ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries)
            : Array.Empty<string>();

        return new OAuthResult
        {
            AccessToken = accessToken,
            RefreshToken = root.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null,
            IdToken = root.TryGetProperty("id_token", out var idt) ? idt.GetString() : null,
            TokenType = root.TryGetProperty("token_type", out var tt) ? tt.GetString() : null,
            ExpiresAt = expiresIn.HasValue
                ? DateTimeOffset.UtcNow.AddSeconds(expiresIn.Value)
                : null,
            Scopes = scopes,
        };
    }
}