using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Threads.Api.Data.Users;
using Threads.Api.Features.Auth.Services.RefreshToken;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Auth.TestSupport;

public abstract class AuthIntegrationTestBase : IntegrationTestBase
{
    protected AuthIntegrationTestBase(CustomWebApplicationFactory factory)
        : base(factory) { }

    protected HttpClient CreateAuthorizedClientWithBearerToken(string accessToken)
    {
        var client = CreateAnonymousClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            JwtBearerDefaults.AuthenticationScheme,
            accessToken
        );

        return client;
    }

    protected AuthTestUserSeeder CreateUserSeeder()
    {
        var userManager = Services.GetRequiredService<UserManager<User>>();
        return new AuthTestUserSeeder(userManager);
    }

    protected AuthRefreshTokenSeeder CreateTokenSeeder()
    {
        var refreshTokenProvider = Services.GetRequiredService<IRefreshTokenProvider>();
        return new AuthRefreshTokenSeeder(Db, refreshTokenProvider);
    }
}
