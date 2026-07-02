using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Users;
using Threads.Api.Features.Auth.Services.JwtProvider;
using Threads.Api.IntegrationTests.Posts;

namespace Threads.Api.IntegrationTests.Infrastructure;

[Collection("Integration tests")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected readonly CustomWebApplicationFactory Factory;
    protected readonly AppDbContext Db;
    protected readonly IServiceProvider Services;

    private readonly IServiceScope _scope;

    public IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        _scope = Factory.Services.CreateScope();
        Services = _scope.ServiceProvider;
        Db = Services.GetRequiredService<AppDbContext>();
    }

    public async Task InitializeAsync()
    {
        await ResetDatabaseAsync();
    }

    protected HttpClient CreateAnonymousClient()
    {
        return Factory.CreateClient();
    }

    protected async Task<HttpClient> CreateAuthenticatedClientAsync(User user)
    {
        var jwtProvider = Services.GetRequiredService<IJwtProvider>();
        var accessToken = await jwtProvider.GenerateAsync(user);

        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            JwtBearerDefaults.AuthenticationScheme,
            accessToken
        );

        return client;
    }

    protected PostTestSeeder CreatePostSeeder()
    {
        var userManager = Services.GetRequiredService<UserManager<User>>();
        return new PostTestSeeder(Db, userManager);
    }

    protected async Task<T> ReadJsonAsync<T>(HttpResponseMessage response)
    {
        var body = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        body.Should().NotBeNull();
        return body;
    }

    private async Task ResetDatabaseAsync()
    {
        Db.ChangeTracker.Clear();

        await Db.RefreshTokens.ExecuteDeleteAsync();
        await Db.Posts.IgnoreQueryFilters().ExecuteDeleteAsync();
        await Db.Follows.ExecuteDeleteAsync();
        await Db.UserTokens.ExecuteDeleteAsync();
        await Db.UserLogins.ExecuteDeleteAsync();
        await Db.UserClaims.ExecuteDeleteAsync();
        await Db.UserRoles.ExecuteDeleteAsync();
        await Db.RoleClaims.ExecuteDeleteAsync();
        await Db.Roles.ExecuteDeleteAsync();
        await Db.Users.ExecuteDeleteAsync();

        Db.ChangeTracker.Clear();
    }

    public Task DisposeAsync()
    {
        _scope.Dispose();
        return Task.CompletedTask;
    }
}
