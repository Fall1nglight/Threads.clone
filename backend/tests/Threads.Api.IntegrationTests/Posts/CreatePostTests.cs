using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Features.Posts;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Posts;

public class CreatePostTests : IntegrationTestBase
{
    public CreatePostTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task CreatePost_ShouldCreatePostForAuthenticatedUser()
    {
        var seeder = CreatePostSeeder();
        var alice = await seeder.CreateUserAsync("alice");
        var client = await CreateAuthenticatedClientAsync(alice);
        var before = DateTime.UtcNow;
        var postContent = "Hello from Alice";

        var response = await client.PostAsJsonAsync(PostTestRoutes.Create, new { content = postContent });
        var after = DateTime.UtcNow;

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var body = await ReadJsonAsync<PostDto>(response);
        body.User.Id.Should().Be(alice.Id);
        body.User.Username.Should().Be(alice.UserName);
        body.Content.Should().Be(postContent);
        body.UpdatedAtUtc.Should().BeNull();
        body.CreatedAtUtc.Should().BeOnOrAfter(before);
        body.CreatedAtUtc.Should().BeOnOrBefore(after);
        response.Headers.Location.ToString().Should().Be($"/posts/{body.Id}");

        Db.ChangeTracker.Clear();
        var persisted = await Db.Posts.SingleAsync(post => post.Id == body.Id);
        persisted.UserId.Should().Be(alice.Id);
        persisted.Content.Should().Be(postContent);
        persisted.UpdatedAtUtc.Should().BeNull();
        persisted.CreatedAtUtc.Should().BeOnOrAfter(before);
        persisted.CreatedAtUtc.Should().BeOnOrBefore(after);
    }

    [Fact]
    public async Task CreatePost_ShouldAllowCreatedPostToBeRead()
    {
        var seeder = CreatePostSeeder();
        var alice = await seeder.CreateUserAsync("alice");
        var client = await CreateAuthenticatedClientAsync(alice);
        var postContent = "Readable post";
        var createResponse = await client.PostAsJsonAsync(
            PostTestRoutes.Create,
            new { content = postContent }
        );
        var createdPost = await ReadJsonAsync<PostDto>(createResponse);

        var getResponse = await client.GetAsync($"/posts/{createdPost.Id}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetchedPost = await ReadJsonAsync<PostDto>(getResponse);
        fetchedPost.Id.Should().Be(createdPost.Id);
        fetchedPost.Content.Should().Be(postContent);
    }

    [Fact]
    public async Task CreatePost_ShouldAcceptContentAtMaximumLength()
    {
        var seeder = CreatePostSeeder();
        var alice = await seeder.CreateUserAsync("alice");
        var client = await CreateAuthenticatedClientAsync(alice);

        var response = await client.PostAsJsonAsync(
            PostTestRoutes.Create,
            new { content = new string('a', 600) }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task CreatePost_ShouldReturnBadRequest_WhenContentIsEmpty(string? content)
    {
        var seeder = CreatePostSeeder();
        var alice = await seeder.CreateUserAsync("alice");
        var client = await CreateAuthenticatedClientAsync(alice);

        var response = await client.PostAsJsonAsync(PostTestRoutes.Create, new { content });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Db.Posts.CountAsync()).Should().Be(0);
    }

    [Theory]
    [InlineData(601)]
    [InlineData(666)]
    [InlineData(999)]
    public async Task CreatePost_ShouldReturnBadRequest_WhenContentIsTooLong(int length)
    {
        var seeder = CreatePostSeeder();
        var alice = await seeder.CreateUserAsync("alice");
        var client = await CreateAuthenticatedClientAsync(alice);

        var response = await client.PostAsJsonAsync(
            PostTestRoutes.Create,
            new { content = new string('a', length) }
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Db.Posts.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task CreatePost_ShouldReturnBadRequest_WhenContentIsMissing()
    {
        var seeder = CreatePostSeeder();
        var alice = await seeder.CreateUserAsync("alice");
        var client = await CreateAuthenticatedClientAsync(alice);

        var response = await client.PostAsJsonAsync(PostTestRoutes.Create, new { });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Db.Posts.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task CreatePost_ShouldReturnBadRequest_WhenJsonContainsUnknownProperty()
    {
        var seeder = CreatePostSeeder();
        var alice = await seeder.CreateUserAsync("alice");
        var client = await CreateAuthenticatedClientAsync(alice);

        var response = await client.PostAsJsonAsync(
            PostTestRoutes.Create,
            new { content = "Valid content", unexpected = true }
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Db.Posts.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task CreatePost_ShouldReturnBadRequest_WhenJsonIsMalformed()
    {
        var seeder = CreatePostSeeder();
        var alice = await seeder.CreateUserAsync("alice");
        var client = await CreateAuthenticatedClientAsync(alice);
        using var content = new StringContent(
            "{\"content\":\"Valid content\"",
            Encoding.UTF8,
            "application/json"
        );

        var response = await client.PostAsync(PostTestRoutes.Create, content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Db.Posts.CountAsync()).Should().Be(0);
    }
}
