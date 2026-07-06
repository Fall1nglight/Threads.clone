using Threads.Api.Features.Auth.Services.JwtProvider;
using Threads.Api.Features.Auth.Services.RefreshToken;

namespace Threads.Api.UnitTests.Auth;

internal static class OptionsTestFactory
{
    public static JwtOptions CreateJwtOptions()
    {
        return new JwtOptions
        {
            Issuer = "ThreadsApi.UnitTests",
            Audience = "ThreadsApi.UnitTests",
            ExpirationInMinutes = 60,
            SecretKey = "unit-test-secret-key-with-enough-bytes-for-hmac",
        };
    }

    public static RefreshTokenOptions CreateRefreshTokenOptions()
    {
        return new RefreshTokenOptions { LifetimeInDays = 7, MaxActiveTokensPerUser = 5 };
    }
}
