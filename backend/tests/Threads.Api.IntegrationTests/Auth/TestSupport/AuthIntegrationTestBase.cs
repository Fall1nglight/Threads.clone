using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
}
