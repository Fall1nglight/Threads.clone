using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Features.Posts;
using Threads.Api.Features.Posts.Endpoints;
using Threads.Api.IntegrationTests.Infrastructure;
using Threads.Api.IntegrationTests.Posts.TestSupport;

namespace Threads.Api.IntegrationTests.Posts;

public class CreatePostTests : PostIntegrationTestBase
{
    public CreatePostTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task CreatePost_ShouldCreatePostForAuthenticatedUser()
    {
        // Arrange
        var seeder = CreatePostSeeder();
        var alice = await seeder.CreateUserAsync(username: PostTestData.AliceUsername);
        var client = await CreateAuthenticatedClientAsync(alice);
        var before = DateTime.UtcNow;
        var requestBody = CreatePostRequest(PostTestData.ValidContent);

        // Act
        var response = await client.PostAsJsonAsync(PostTestRoutes.Base, requestBody);
        var after = DateTime.UtcNow;

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var responseBody = await ReadJsonAsync<PostDto>(response);

        responseBody.User.Id.Should().Be(alice.Id);
        responseBody.User.Username.Should().Be(alice.UserName);
        responseBody.Content.Should().Be(PostTestData.ValidContent);
        responseBody.UpdatedAtUtc.Should().BeNull();
        responseBody.CreatedAtUtc.Should().BeOnOrAfter(before);
        responseBody.CreatedAtUtc.Should().BeOnOrBefore(after);
        response.Headers.Location.ToString().Should().Be($"/posts/{responseBody.Id}");

        Db.ChangeTracker.Clear();

        var persisted = await Db.Posts.SingleAsync(post => post.Id == responseBody.Id);

        persisted.UserId.Should().Be(alice.Id);
        persisted.Content.Should().Be(PostTestData.ValidContent);
        persisted.UpdatedAtUtc.Should().BeNull();
        persisted.CreatedAtUtc.Should().BeOnOrAfter(before);
        persisted.CreatedAtUtc.Should().BeOnOrBefore(after);
    }

    [Fact]
    public async Task CreatePost_ShouldAllowCreatedPostToBeRead()
    {
        // Arrange
        var seeder = CreatePostSeeder();
        var alice = await seeder.CreateUserAsync(username: PostTestData.AliceUsername);
        var client = await CreateAuthenticatedClientAsync(alice);
        var requestBody = CreatePostRequest(PostTestData.ValidContent);
        var createResponse = await client.PostAsJsonAsync(PostTestRoutes.Base, requestBody);
        var createdPost = await ReadJsonAsync<PostDto>(createResponse);

        // Act
        var getResponse = await client.GetAsync($"/posts/{createdPost.Id}");

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetchedPost = await ReadJsonAsync<PostDto>(getResponse);
        fetchedPost.Id.Should().Be(createdPost.Id);
        fetchedPost.Content.Should().Be(PostTestData.ValidContent);
    }

    [Fact]
    public async Task CreatePost_ShouldAcceptContentAtMaximumLength()
    {
        // Arrange
        var seeder = CreatePostSeeder();
        var alice = await seeder.CreateUserAsync(username: PostTestData.AliceUsername);
        var client = await CreateAuthenticatedClientAsync(alice);
        var requestBody = CreatePostRequest(PostTestData.MaxLengthContent);

        // Act
        var response = await client.PostAsJsonAsync(PostTestRoutes.Base, requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task CreatePost_ShouldReturnBadRequest_WhenContentIsEmpty(string? content)
    {
        // Arrange
        var seeder = CreatePostSeeder();
        var alice = await seeder.CreateUserAsync(username: PostTestData.AliceUsername);
        var client = await CreateAuthenticatedClientAsync(alice);
        var requestBody = CreatePostRequest(content);

        // Act
        var response = await client.PostAsJsonAsync(PostTestRoutes.Base, requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Db.Posts.CountAsync()).Should().Be(0);
    }

    [Theory]
    [InlineData(601)]
    [InlineData(666)]
    [InlineData(999)]
    public async Task CreatePost_ShouldReturnBadRequest_WhenContentIsTooLong(int length)
    {
        // Arrange
        var seeder = CreatePostSeeder();
        var alice = await seeder.CreateUserAsync(username: PostTestData.AliceUsername);
        var client = await CreateAuthenticatedClientAsync(alice);
        var requestBody = CreatePostRequest(PostTestData.ContentOfLength(length));

        // Act
        var response = await client.PostAsJsonAsync(PostTestRoutes.Base, requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Db.Posts.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task CreatePost_ShouldReturnBadRequest_WhenContentIsMissing()
    {
        // Arrange
        var seeder = CreatePostSeeder();
        var alice = await seeder.CreateUserAsync(username: PostTestData.AliceUsername);
        var client = await CreateAuthenticatedClientAsync(alice);
        var requestBody = new { };

        // Act
        var response = await client.PostAsJsonAsync(PostTestRoutes.Base, requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Db.Posts.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task CreatePost_ShouldReturnBadRequest_WhenJsonContainsUnknownProperty()
    {
        // Arrange
        var seeder = CreatePostSeeder();
        var alice = await seeder.CreateUserAsync(username: PostTestData.AliceUsername);
        var client = await CreateAuthenticatedClientAsync(alice);
        var requestBody = new { content = PostTestData.ValidContent, unexpected = true };

        // Act
        var response = await client.PostAsJsonAsync(PostTestRoutes.Base, requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Db.Posts.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task CreatePost_ShouldReturnBadRequest_WhenJsonIsMalformed()
    {
        // Arrange
        var seeder = CreatePostSeeder();
        var alice = await seeder.CreateUserAsync(username: PostTestData.AliceUsername);
        var client = await CreateAuthenticatedClientAsync(alice);
        using var requestBody = new StringContent(
            "{\"content\":\"Valid content\"",
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await client.PostAsync(PostTestRoutes.Base, requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Db.Posts.CountAsync()).Should().Be(0);
    }

    private static CreatePost.Request CreatePostRequest(string? content) =>
        new(content ?? string.Empty);
}
