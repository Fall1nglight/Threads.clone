namespace Threads.Api.Features.Auth.Services.RefreshToken;

public interface IRefreshTokenProvider
{
    string GeneratePlainToken();
    string Hash(string plainToken);
}
