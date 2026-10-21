using System.Security.Cryptography;
using System.Text;

namespace Threads.Api.Features.Auth.Services.RefreshToken;

public class RefreshTokenProvider : IRefreshTokenProvider
{
    public string GeneratePlainToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    public string Hash(string plainToken)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(plainToken);
        var hashBytes = SHA256.HashData(tokenBytes);
        return Convert.ToBase64String(hashBytes);
    }
}
