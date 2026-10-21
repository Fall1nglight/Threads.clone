using Threads.Api.Features.Auth.Services.JwtProvider;

namespace Threads.Api.UnitTests.Auth.JWT;

internal static class JwtOptionsFactory
{
    private const string JwtIssuer = "ThreadsApi.UnitTests";
    private const string JwtAudience = "ThreadsApi.UnitTests";
    private const int JwtExpirationInMinutes = 60;
    private const string JwtSecretKey = "unit-test-secret-key-with-enough-bytes-for-hmac";

    public static JwtOptions CreateJwtOptions()
    {
        return new JwtOptions
        {
            Issuer = JwtIssuer,
            Audience = JwtAudience,
            ExpirationInMinutes = JwtExpirationInMinutes,
            SecretKey = JwtSecretKey,
        };
    }
}
