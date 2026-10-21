namespace Threads.Api.Features.Auth.Services.RefreshToken;

public interface IRefreshTokenManager
{
    Task<string> GenerateAsync(Guid userId, CancellationToken cancellationToken);

    Task<TokenRotationResult> RotateAsync(string plainToken, CancellationToken cancellationToken);

    Task LogoutSingleAsync(Guid userId, string plainToken, CancellationToken cancellationToken);

    Task LogoutGlobalAsync(Guid userId, CancellationToken cancellationToken);
}
