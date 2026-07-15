using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Threads.Api.IntegrationTests.Infrastructure;
using Threads.Api.IntegrationTests.Likes.TestSupport;

namespace Threads.Api.IntegrationTests.Likes;

public class UnlikePostTests : LikeIntegrationTestBase
{
    public UnlikePostTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task UnlikePost_ShouldRemoveRequesterLikeOnly()
    {
        // Arrange
        var scenarioSeeder = CreatePostVisibilityScenarioSeeder();
        var likeSeeder = CreateLikeSeeder();
        var seed = await scenarioSeeder.SeedAsync();
        await likeSeeder.CreatePostLikeAsync(seed.Alice, seed.BobPublicPost);
        await likeSeeder.CreatePostLikeAsync(seed.Henry, seed.BobPublicPost);
        var client = await CreateAuthenticatedClientAsync(seed.Alice);

        // Act
        var response = await client.DeleteAsync(LikeTestRoutes.ForPost(seed.BobPublicPost.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        Db.ChangeTracker.Clear();

        (await Db.PostLikes.AnyAsync(like => like.UserId == seed.Alice.Id)).Should().BeFalse();
        (await Db.PostLikes.AnyAsync(like => like.UserId == seed.Henry.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task UnlikePost_ShouldReturnNoContent_WhenRequesterHasNotLikedPost()
    {
        // Arrange
        var scenarioSeeder = CreatePostVisibilityScenarioSeeder();
        var seed = await scenarioSeeder.SeedAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Alice);

        // Act
        var response = await client.DeleteAsync(LikeTestRoutes.ForPost(seed.BobPublicPost.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await Db.PostLikes.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task UnlikePost_ShouldReturnNoContent_WhenPostDoesNotExist()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.DeleteAsync(LikeTestRoutes.ForPost(Guid.NewGuid()));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UnlikePost_ShouldReturnBadRequest_WhenPostIdIsEmpty()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.DeleteAsync(LikeTestRoutes.ForPost(Guid.Empty));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
