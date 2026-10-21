using Threads.Api.Data.Users;

namespace Threads.Api.Features.Auth.Services.JwtProvider;

public interface IJwtProvider
{
    Task<string> GenerateAsync(User user);
}
