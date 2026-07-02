using System.Net;
using FluentAssertions;
using Threads.Api.Features.Posts;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Posts;

public class GetPostTests : IntegrationTestBase
{
    public GetPostTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task GetPost_ShouldReturnPublicPost_WhenUserIsAuthenticated()
    {
        var seeder = CreatePostSeeder();
        var seed = await seeder.SeedVisibilityGraphAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Alice);

        var response = await client.GetAsync($"/posts/{seed.BobPublicPost.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadJsonAsync<PostDto>(response);
        body.Id.Should().Be(seed.BobPublicPost.Id);
        body.User.Id.Should().Be(seed.BobPublic.Id);
        body.User.Username.Should().Be(seed.BobPublic.UserName);
        body.Content.Should().Be(seed.BobPublicPost.Content);
        body.CreatedAtUtc.Should().Be(seed.BobPublicPost.CreatedAtUtc);
        body.UpdatedAtUtc.Should().Be(seed.BobPublicPost.UpdatedAtUtc);
    }

    [Fact]
    public async Task GetPost_ShouldReturnPrivatePost_WhenRequesterIsOwner()
    {
        var seeder = CreatePostSeeder();
        var seed = await seeder.SeedVisibilityGraphAsync();
        var client = await CreateAuthenticatedClientAsync(seed.AlicePrivate);

        var response = await client.GetAsync($"/posts/{seed.AlicePrivateOwnPost.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPost_ShouldReturnPrivatePost_WhenRequesterIsAcceptedFollower()
    {
        var seeder = CreatePostSeeder();
        var seed = await seeder.SeedVisibilityGraphAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Alice);

        var response = await client.GetAsync($"/posts/{seed.DinaPrivateFollowedPost.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPost_ShouldForbidPrivatePost_WhenRequesterIsPendingFollower()
    {
        var seeder = CreatePostSeeder();
        var seed = await seeder.SeedVisibilityGraphAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Alice);

        var response = await client.GetAsync($"/posts/{seed.ErinPrivatePendingPost.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPost_ShouldForbidPrivatePost_WhenRequesterIsStranger()
    {
        var seeder = CreatePostSeeder();
        var seed = await seeder.SeedVisibilityGraphAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Henry);

        var response = await client.GetAsync($"/posts/{seed.DinaPrivateFollowedPost.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPost_ShouldReturnNotFound_WhenPostDoesNotExist()
    {
        var seeder = CreatePostSeeder();
        var seed = await seeder.SeedVisibilityGraphAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Alice);

        var response = await client.GetAsync($"/posts/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPost_ShouldReturnNotFound_WhenPostIsSoftDeleted()
    {
        var seeder = CreatePostSeeder();
        var seed = await seeder.SeedVisibilityGraphAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Alice);

        var response = await client.GetAsync($"/posts/{seed.DeletedPublicPost.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPost_ShouldReturnBadRequest_WhenIdIsEmpty()
    {
        var seeder = CreatePostSeeder();
        var seed = await seeder.SeedVisibilityGraphAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Alice);

        var response = await client.GetAsync($"/posts/{Guid.Empty}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
