using Threads.Api.Data.Shared;
using Threads.Api.Data.Tokens;
using Threads.Api.Data.Users;
using Threads.Api.Features.Auth.Services.RefreshToken;

namespace Threads.Api.IntegrationTests.Auth.TestSupport;

public sealed class RefreshTokenTestSeeder
{
    private readonly AppDbContext _db;
    private readonly IRefreshTokenProvider _refreshTokenProvider;

    public RefreshTokenTestSeeder(AppDbContext db, IRefreshTokenProvider refreshTokenProvider)
    {
        _db = db;
        _refreshTokenProvider = refreshTokenProvider;
    }

    public async Task<RefreshTokenSeed> CreateActiveTokenAsync(User user, int tokenSeed = 1)
    {
        return await CreateTokenAsync(
            user,
            tokenSeed,
            expiresAtUtc: DateTime.UtcNow.AddDays(7),
            revokedAtUtc: null,
            revocationReason: null
        );
    }

    public async Task<RefreshTokenSeed> CreateExpiredTokenAsync(User user, int tokenSeed = 2)
    {
        return await CreateTokenAsync(
            user,
            tokenSeed,
            expiresAtUtc: DateTime.UtcNow.AddDays(-1),
            revokedAtUtc: null,
            revocationReason: null
        );
    }

    public async Task<RefreshTokenSeed> CreateRevokedTokenAsync(
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

    public async Task<RefreshTokenChainSeed> CreateTokenChainAsync(User user)
    {
        var replacementToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = _refreshTokenProvider.Hash(CreatePlainToken(2)),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7),
        };

        await _db.RefreshTokens.AddAsync(replacementToken);
        await _db.SaveChangesAsync();

        var replacedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = _refreshTokenProvider.Hash(CreatePlainToken(1)),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7),
            RevokedAtUtc = DateTime.UtcNow,
            RevocationReason = RevocationReason.ReplacedByNewToken,
            ReplacedByTokenId = replacementToken.Id,
        };

        await _db.RefreshTokens.AddAsync(replacedToken);
        await _db.SaveChangesAsync();

        return new RefreshTokenChainSeed(replacedToken, replacementToken);
    }

    public static string CreatePlainToken(int tokenSeed)
    {
        var bytes = Enumerable.Repeat((byte)tokenSeed, 32).ToArray();
        return Convert.ToBase64String(bytes);
    }

    private async Task<RefreshTokenSeed> CreateTokenAsync(
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

        return new RefreshTokenSeed(plainToken, refreshToken);
    }
}

public sealed record RefreshTokenSeed(string PlainToken, RefreshToken Entity);

public sealed record RefreshTokenChainSeed(
    RefreshToken ReplacedToken,
    RefreshToken ReplacementToken
);
