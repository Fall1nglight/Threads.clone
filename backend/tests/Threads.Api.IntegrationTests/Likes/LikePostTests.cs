using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Features.Posts;
using Threads.Api.IntegrationTests.Infrastructure;
using Threads.Api.IntegrationTests.Likes.TestSupport;

namespace Threads.Api.IntegrationTests.Likes;

public class LikePostTests : LikeIntegrationTestBase
{
    public LikePostTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task LikePost_ShouldCreateLikeAndUpdatePostDto_WhenPostIsPublic()
    {
        // Arrange
        var scenarioSeeder = CreatePostVisibilityScenarioSeeder();
        var seed = await scenarioSeeder.SeedAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Alice);
        var before = DateTime.UtcNow;

        // Act
        var response = await client.PostAsync(LikeTestRoutes.ForPost(seed.BobPublicPost.Id), null);
        var after = DateTime.UtcNow;

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        Db.ChangeTracker.Clear();

        var persistedLike = await Db.PostLikes.SingleAsync();
        persistedLike.PostId.Should().Be(seed.BobPublicPost.Id);
        persistedLike.UserId.Should().Be(seed.Alice.Id);
        persistedLike.CreatedAtUtc.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);

        var getResponse = await client.GetAsync($"/posts/{seed.BobPublicPost.Id}");
        var responseBody = await ReadJsonAsync<PostDto>(getResponse);
        responseBody.LikeCount.Should().Be(1);
        responseBody.IsLikedByCurrentUser.Should().BeTrue();
    }

    [Fact]
    public async Task LikePost_ShouldCreateLike_WhenRequesterOwnsPrivatePost()
    {
        // Arrange
        var scenarioSeeder = CreatePostVisibilityScenarioSeeder();
        var seed = await scenarioSeeder.SeedAsync();
        var client = await CreateAuthenticatedClientAsync(seed.AlicePrivate);

        // Act
        var response = await client.PostAsync(
            LikeTestRoutes.ForPost(seed.AlicePrivateOwnPost.Id),
            null
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await Db.PostLikes.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task LikePost_ShouldCreateLike_WhenPrivateOwnerIsAcceptedFollowed()
    {
        // Arrange
        var scenarioSeeder = CreatePostVisibilityScenarioSeeder();
        var seed = await scenarioSeeder.SeedAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Alice);

        // Act
        var response = await client.PostAsync(
            LikeTestRoutes.ForPost(seed.DinaPrivateFollowedPost.Id),
            null
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await Db.PostLikes.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task LikePost_ShouldBeIdempotent_WhenLikeAlreadyExists()
    {
        // Arrange
        var scenarioSeeder = CreatePostVisibilityScenarioSeeder();
        var seed = await scenarioSeeder.SeedAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Alice);

        // Act
        var firstResponse = await client.PostAsync(
            LikeTestRoutes.ForPost(seed.BobPublicPost.Id),
            null
        );
        var secondResponse = await client.PostAsync(
            LikeTestRoutes.ForPost(seed.BobPublicPost.Id),
            null
        );

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await Db.PostLikes.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task LikePost_ShouldReturnNotFound_WhenPostDoesNotExist()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.PostAsync(LikeTestRoutes.ForPost(Guid.NewGuid()), null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Db.PostLikes.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task LikePost_ShouldReturnNotFound_WhenPostIsNotVisible()
    {
        // Arrange
        var scenarioSeeder = CreatePostVisibilityScenarioSeeder();
        var seed = await scenarioSeeder.SeedAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Alice);

        // Act
        var pendingResponse = await client.PostAsync(
            LikeTestRoutes.ForPost(seed.ErinPrivatePendingPost.Id),
            null
        );
        var strangerResponse = await client.PostAsync(
            LikeTestRoutes.ForPost(seed.FrankPrivateStrangerPost.Id),
            null
        );
        var deletedResponse = await client.PostAsync(
            LikeTestRoutes.ForPost(seed.DeletedPublicPost.Id),
            null
        );

        // Assert
        pendingResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        strangerResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        deletedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Db.PostLikes.CountAsync()).Should().Be(0);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task LikePost_ShouldReturnNotFound_WhenRequesterAndOwnerAreBlocked(
        bool requesterBlocksOwner
    )
    {
        // Arrange
        var scenarioSeeder = CreatePostVisibilityScenarioSeeder();
        var blockSeeder = CreateBlockSeeder();
        var seed = await scenarioSeeder.SeedAsync();
        await blockSeeder.CreateUserBlockAsync(
            requesterBlocksOwner ? seed.Alice : seed.BobPublic,
            requesterBlocksOwner ? seed.BobPublic : seed.Alice
        );
        var client = await CreateAuthenticatedClientAsync(seed.Alice);

        // Act
        var response = await client.PostAsync(LikeTestRoutes.ForPost(seed.BobPublicPost.Id), null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Db.PostLikes.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task LikePost_ShouldReturnBadRequest_WhenPostIdIsEmpty()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.PostAsync(LikeTestRoutes.ForPost(Guid.Empty), null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Db.PostLikes.CountAsync()).Should().Be(0);
    }
}
