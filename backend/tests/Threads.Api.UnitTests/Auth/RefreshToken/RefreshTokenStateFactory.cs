namespace Threads.Api.UnitTests.Auth.RefreshToken;

public static class RefreshTokenStateFactory
{
    public static Data.Tokens.RefreshToken CreateToken(
        DateTime expiresAtUtc,
        DateTime? revokedAtUtc = null
    )
    {
        return new Data.Tokens.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TokenHash = RefreshTokenTestData.PlainToken,
            ExpiresAtUtc = expiresAtUtc,
            RevokedAtUtc = revokedAtUtc,
        };
    }
}
