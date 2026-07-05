using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Threads.Api.Features.Posts.Endpoints;
using Threads.Api.IntegrationTests.Infrastructure;
using Threads.Api.IntegrationTests.Posts.TestSupport;

namespace Threads.Api.IntegrationTests.Posts;

public class PostAuthorizationTests : PostIntegrationTestBase
{
    public PostAuthorizationTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task GetAnonymousFeed_ShouldAllowAnonymousRequests()
    {
        // Arrange
        var client = CreateAnonymousClient();

        // Act
        var response = await client.GetAsync(PostTestRoutes.AnonymousFeed);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData(PostTestRoutes.GlobalFeed)]
    [InlineData(PostTestRoutes.MyFeed)]
    public async Task ProtectedGetFeedEndpoints_ShouldRejectAnonymousRequests(string path)
    {
        // Arrange
        var client = CreateAnonymousClient();

        // Act
        var response = await client.GetAsync(path);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedGetPostEndpoint_ShouldRejectAnonymousRequests()
    {
        // Arrange
        var client = CreateAnonymousClient();

        // Act
        var response = await client.GetAsync($"/posts/{PostTestData.AuthSmokePostId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedDeletePostEndpoint_ShouldRejectAnonymousRequests()
    {
        // Arrange
        var client = CreateAnonymousClient();

        // Act
        var response = await client.DeleteAsync($"/posts/{PostTestData.AuthSmokePostId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedCreatePostEndpoint_ShouldRejectAnonymousRequests()
    {
        // Arrange
        var client = CreateAnonymousClient();
        var requestBody = CreatePostRequest(PostTestData.ValidContent);

        // Act
        var response = await client.PostAsJsonAsync(PostTestRoutes.Base, requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedUpdatePostEndpoint_ShouldRejectAnonymousRequests()
    {
        // Arrange
        var client = CreateAnonymousClient();

        // Act
        var response = await client.PutAsync($"/posts/{PostTestData.AuthSmokePostId}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static CreatePost.Request CreatePostRequest(string? content) =>
        new(content ?? string.Empty);
}
