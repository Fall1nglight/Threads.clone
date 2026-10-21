namespace Threads.Api.Features.Auth.Services.RefreshToken;

public class RefreshTokenOptions
{
    public required int LifetimeInDays { get; init; }
    public required int MaxActiveTokensPerUser { get; init; }
}
