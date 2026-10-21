using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Threads.Api.Data.Users;

namespace Threads.Api.Features.Auth.Services.JwtProvider;

public class JwtProvider : IJwtProvider
{
    private readonly UserManager<User> _userManager;
    private readonly IOptions<JwtOptions> _options;

    public JwtProvider(UserManager<User> userManager, IOptions<JwtOptions> options)
    {
        _userManager = userManager;
        _options = options;
    }

    public async Task<string> GenerateAsync(User user)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Value.SecretKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var expiration = DateTime.UtcNow.AddMinutes(_options.Value.ExpirationInMinutes);

        var roles = await _userManager.GetRolesAsync(user);

        List<Claim> claims =
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Name, user.UserName!),
            .. roles.Select(r => new Claim(ClaimTypes.Role, r)),
        ];

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(claims),
            Audience = _options.Value.Audience,
            Issuer = _options.Value.Issuer,
            SigningCredentials = credentials,
            Expires = expiration,
        };

        return new JsonWebTokenHandler().CreateToken(tokenDescriptor);
    }
}
