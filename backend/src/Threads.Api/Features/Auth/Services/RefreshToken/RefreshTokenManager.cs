using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Tokens;

namespace Threads.Api.Features.Auth.Services.RefreshToken;

public class RefreshTokenManager : IRefreshTokenManager
{
    private readonly AppDbContext _db;
    private readonly IRefreshTokenProvider _refreshTokenProvider;
    private readonly IOptions<RefreshTokenOptions> _options;

    public RefreshTokenManager(
        AppDbContext db,
        IRefreshTokenProvider refreshTokenProvider,
        IOptions<RefreshTokenOptions> options
    )
    {
        _db = db;
        _refreshTokenProvider = refreshTokenProvider;
        _options = options;
    }

    public async Task<string> GenerateAsync(Guid userId, CancellationToken cancellationToken)
    {
        // limits simultaneously active devices/tokens per user
        await RevokeExcessTokensAsync(
            userId,
            _options.Value.MaxActiveTokensPerUser,
            cancellationToken
        );

        var plainToken = _refreshTokenProvider.GeneratePlainToken();

        var token = new Data.Tokens.RefreshToken
        {
            UserId = userId,
            TokenHash = _refreshTokenProvider.Hash(plainToken),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_options.Value.LifetimeInDays),
        };

        await _db.RefreshTokens.AddAsync(token, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return plainToken;
    }

    public async Task<TokenRotationResult> RotateAsync(
        string plainToken,
        CancellationToken cancellationToken
    )
    {
        var providedTokenHash = _refreshTokenProvider.Hash(plainToken);

        var tokenToRotate = await _db
            .RefreshTokens.Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == providedTokenHash, cancellationToken);

        switch (tokenToRotate)
        {
            case null:
                return TokenRotationResult.Failure();

            // indicates token theft
            case { IsActive: false, RevocationReason: RevocationReason.ReplacedByNewToken }:
                await InvalidateUserTokens(tokenToRotate.UserId, cancellationToken);
                return TokenRotationResult.Failure();

            case { IsActive: false }:
                return TokenRotationResult.Failure();
        }

        var user = tokenToRotate.User;
        var newPlainToken = _refreshTokenProvider.GeneratePlainToken();
        var newTokenHash = _refreshTokenProvider.Hash(newPlainToken);

        var newToken = new Data.Tokens.RefreshToken
        {
            TokenHash = newTokenHash,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_options.Value.LifetimeInDays),
            UserId = tokenToRotate.UserId,
        };

        await _db.RefreshTokens.AddAsync(newToken, cancellationToken);

        tokenToRotate.RevokedAtUtc = DateTime.UtcNow;
        tokenToRotate.RevocationReason = RevocationReason.ReplacedByNewToken;
        tokenToRotate.ReplacedByToken = newToken;

        await _db.SaveChangesAsync(cancellationToken);
        return TokenRotationResult.Success(newPlainToken, user);
    }

    public async Task LogoutSingleAsync(
        Guid userId,
        string plainToken,
        CancellationToken cancellationToken
    )
    {
        var tokenHash = _refreshTokenProvider.Hash(plainToken);

        var token = await _db.RefreshTokens.FirstOrDefaultAsync(
            rt => rt.TokenHash == tokenHash && rt.UserId == userId,
            cancellationToken
        );

        if (token == null)
            return;

        if (!token.IsActive)
            return;

        token.RevokedAtUtc = DateTime.UtcNow;
        token.RevocationReason = RevocationReason.Logout;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task LogoutGlobalAsync(Guid userId, CancellationToken cancellationToken)
    {
        await _db
            .RefreshTokens.Where(rt =>
                rt.UserId == userId && rt.RevokedAtUtc == null && rt.ExpiresAtUtc > DateTime.UtcNow
            )
            .ExecuteUpdateAsync(
                builder =>
                    builder
                        .SetProperty(token => token.RevokedAtUtc, DateTime.UtcNow)
                        .SetProperty(
                            token => token.RevocationReason,
                            RevocationReason.GlobalLogout
                        ),
                cancellationToken
            );
    }

    // helper methods
    private async Task InvalidateUserTokens(Guid userId, CancellationToken cancellationToken)
    {
        await _db
            .RefreshTokens.Where(rt =>
                rt.UserId == userId && rt.RevokedAtUtc == null && rt.ExpiresAtUtc > DateTime.UtcNow
            )
            .ExecuteUpdateAsync(
                builder =>
                    builder
                        .SetProperty(token => token.RevokedAtUtc, DateTime.UtcNow)
                        .SetProperty(token => token.RevocationReason, RevocationReason.Compromised),
                cancellationToken
            );

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task RevokeExcessTokensAsync(
        Guid userId,
        int limit,
        CancellationToken cancellationToken
    )
    {
        var activeTokens = await _db
            .RefreshTokens.Where(rt =>
                rt.UserId == userId && rt.RevokedAtUtc == null && rt.ExpiresAtUtc > DateTime.UtcNow
            )
            .ToListAsync(cancellationToken);

        if (activeTokens.Count < limit)
            return;

        var tokensToRevoke = activeTokens
            .OrderBy(rt => rt.ExpiresAtUtc)
            .Take(activeTokens.Count - limit + 1);

        foreach (var tokenToRevoke in tokensToRevoke)
        {
            tokenToRevoke.RevokedAtUtc = DateTime.UtcNow;
            tokenToRevoke.RevocationReason = RevocationReason.DeviceLimitExceeded;
        }
    }
}
