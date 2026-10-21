using System.Net;
using FluentAssertions;
using Threads.Api.Features.Posts;
using Threads.Api.IntegrationTests.Infrastructure;
using Threads.Api.IntegrationTests.Posts.TestSupport;

namespace Threads.Api.IntegrationTests.Posts;

public class GetPostTests : IntegrationTestBase
{
    public GetPostTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task GetPost_ShouldReturnPublicPost_WhenRequesterIsAnonymous()
    {
        // Arrange
        var seed = await Scenarios.PostVisibility.SeedAsync();
        var client = CreateAnonymousClient();

        // Act
        var response = await client.GetAsync($"posts/{seed.BobPublicPost.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PostDto>(response);

        responseBody.Id.Should().Be(seed.BobPublicPost.Id);
        responseBody.User.Id.Should().Be(seed.BobPublic.Id);
        responseBody.User.Username.Should().Be(seed.BobPublic.UserName);
        responseBody.Content.Should().Be(seed.BobPublicPost.Content);
        responseBody.IsLikedByCurrentUser.Should().BeFalse();
        responseBody.CreatedAtUtc.Should().Be(seed.BobPublicPost.CreatedAtUtc);
        responseBody.UpdatedAtUtc.Should().Be(seed.BobPublicPost.UpdatedAtUtc);
    }

    [Fact]
    public async Task GetPost_ShouldReturnNotFound_WhenRequesterIsAnonymousAndPostIsPrivate()
    {
        // Arrange
        var seed = await Scenarios.PostVisibility.SeedAsync();
        var client = CreateAnonymousClient();

        // Act
        var response = await client.GetAsync($"posts/{seed.DinaPrivateFollowedPost.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPost_ShouldReturnPrivatePost_WhenRequesterIsOwner()
    {
        // Arrange
        var seed = await Scenarios.PostVisibility.SeedAsync();
        var client = await CreateAuthenticatedClientAsync(seed.AlicePrivate);

        // Act
        var response = await client.GetAsync($"posts/{seed.AlicePrivateOwnPost.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPost_ShouldReturnPrivatePost_WhenRequesterIsAcceptedFollower()
    {
        // Arrange
        var seed = await Scenarios.PostVisibility.SeedAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Alice);

        // Act
        var response = await client.GetAsync($"posts/{seed.DinaPrivateFollowedPost.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPost_ShouldReturnNotFound_WhenRequesterIsPendingFollower()
    {
        // Arrange
        var seed = await Scenarios.PostVisibility.SeedAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Alice);

        // Act
        var response = await client.GetAsync($"posts/{seed.ErinPrivatePendingPost.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPost_ShouldReturnNotFound_WhenRequesterIsStranger()
    {
        // Arrange
        var seed = await Scenarios.PostVisibility.SeedAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Henry);

        // Act
        var response = await client.GetAsync($"posts/{seed.DinaPrivateFollowedPost.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetPost_ShouldReturnNotFound_WhenRequesterAndPostOwnerHaveBlockRelationship(
        bool requesterBlocksOwner
    )
    {
        // Arrange
        var seed = await Scenarios.PostVisibility.SeedAsync();

        await Seeders.Blocks.CreateUserBlockAsync(
            blocker: requesterBlocksOwner ? seed.Alice : seed.BobPublic,
            blocked: requesterBlocksOwner ? seed.BobPublic : seed.Alice
        );

        var client = await CreateAuthenticatedClientAsync(seed.Alice);

        // Act
        var response = await client.GetAsync($"posts/{seed.BobPublicPost.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPost_ShouldExcludeBlockedUsersFromLikeAndCommentCounts()
    {
        // Arrange
        var seed = await Scenarios.PostEngagementVisibility.SeedAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.GetAsync($"posts/{seed.TargetPost.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PostDto>(response);

        responseBody.LikeCount.Should().Be(1);
        responseBody.CommentCount.Should().Be(1);
        responseBody.IsLikedByCurrentUser.Should().BeTrue();
    }

    [Fact]
    public async Task GetPost_ShouldReturnNotFound_WhenPostDoesNotExist()
    {
        // Arrange
        var seed = await Scenarios.PostVisibility.SeedAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Alice);

        // Act
        var response = await client.GetAsync($"posts/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPost_ShouldReturnNotFound_WhenPostIsSoftDeleted()
    {
        // Arrange
        var seed = await Scenarios.PostVisibility.SeedAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Alice);

        // Act
        var response = await client.GetAsync($"posts/{seed.DeletedPublicPost.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPost_ShouldReturnBadRequest_WhenPostIdIsEmpty()
    {
        // Arrange
        var seed = await Scenarios.PostVisibility.SeedAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Alice);

        // Act
        var response = await client.GetAsync($"posts/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
