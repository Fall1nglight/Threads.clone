namespace Threads.Api.Features.Auth.Services.JwtProvider;

public class JwtOptions
{
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public int ExpirationInMinutes { get; init; }
    public required string SecretKey { get; init; }
}
