using Threads.Api.Data.Shared;
using Threads.Api.Data.Tokens;
using Threads.Api.Data.Users;
using Threads.Api.Features.Auth.Services.RefreshToken;

namespace Threads.Api.IntegrationTests.Auth.Infrastructure;

public sealed class AuthRefreshTokenSeeder
{
    private readonly AppDbContext _db;
    private readonly IRefreshTokenProvider _refreshTokenProvider;

    public AuthRefreshTokenSeeder(AppDbContext db, IRefreshTokenProvider refreshTokenProvider)
    {
        _db = db;
        _refreshTokenProvider = refreshTokenProvider;
    }

    public async Task<AuthRefreshTokenSeed> CreateActiveTokenAsync(User user, int tokenSeed = 1)
    {
        return await CreateTokenAsync(
            user,
            tokenSeed,
            expiresAtUtc: DateTime.UtcNow.AddDays(7),
            revokedAtUtc: null,
            revocationReason: null
        );
    }

    public async Task<AuthRefreshTokenSeed> CreateExpiredTokenAsync(User user, int tokenSeed = 2)
    {
        return await CreateTokenAsync(
            user,
            tokenSeed,
            expiresAtUtc: DateTime.UtcNow.AddDays(-1),
            revokedAtUtc: null,
            revocationReason: null
        );
    }

    public async Task<AuthRefreshTokenSeed> CreateRevokedTokenAsync(
        User user,
        RevocationReason revocationReason,
        int tokenSeed = 3
    )
    {
        return await CreateTokenAsync(
            user,
            tokenSeed,
            expiresAtUtc: DateTime.UtcNow.AddDays(7),
            revokedAtUtc: DateTime.UtcNow.AddMinutes(-1),
            revocationReason
        );
    }

    public static string CreatePlainToken(int tokenSeed)
    {
        var bytes = Enumerable.Repeat((byte)tokenSeed, 32).ToArray();
        return Convert.ToBase64String(bytes);
    }

    private async Task<AuthRefreshTokenSeed> CreateTokenAsync(
        User user,
        int tokenSeed,
        DateTime expiresAtUtc,
        DateTime? revokedAtUtc,
        RevocationReason? revocationReason
    )
    {
        var plainToken = CreatePlainToken(tokenSeed);
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = _refreshTokenProvider.Hash(plainToken),
            ExpiresAtUtc = expiresAtUtc,
            RevokedAtUtc = revokedAtUtc,
            RevocationReason = revocationReason,
        };

        await _db.RefreshTokens.AddAsync(refreshToken);
        await _db.SaveChangesAsync();

        return new AuthRefreshTokenSeed(plainToken, refreshToken);
    }
}

public sealed record AuthRefreshTokenSeed(string PlainToken, RefreshToken Entity);
