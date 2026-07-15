using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Data.Follows;
using Threads.Api.IntegrationTests.Follows.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Follows;

public class RemoveFollowerTests : FollowIntegrationTestBase
{
    public RemoveFollowerTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task RemoveFollower_ShouldDeleteAcceptedIncomingRelationship()
    {
        // Arrange
        var seed = await SeedRelationshipAsync(FollowStatus.Accepted, incoming: true);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.DeleteAsync(FollowTestRoutes.RemoveFollower(seed.OtherUserId));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await Db.Follows.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task RemoveFollower_ShouldKeepPendingIncomingRelationship()
    {
        // Arrange
        var seed = await SeedRelationshipAsync(FollowStatus.Pending, incoming: true);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.DeleteAsync(FollowTestRoutes.RemoveFollower(seed.OtherUserId));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await Db.Follows.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task RemoveFollower_ShouldReturnNoContent_WhenRelationshipDoesNotExist()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.DeleteAsync(FollowTestRoutes.RemoveFollower(Guid.NewGuid()));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveFollower_ShouldKeepReversedRelationship()
    {
        // Arrange
        var seed = await SeedRelationshipAsync(FollowStatus.Accepted, incoming: false);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.DeleteAsync(FollowTestRoutes.RemoveFollower(seed.OtherUserId));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await Db.Follows.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task RemoveFollower_ShouldReturnBadRequest_WhenFollowerIdIsEmpty()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.DeleteAsync(FollowTestRoutes.RemoveFollower(Guid.Empty));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<RelationshipSeed> SeedRelationshipAsync(FollowStatus status, bool incoming)
    {
        var userSeeder = CreateUserSeeder();
        var followSeeder = CreateFollowSeeder();
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var otherUser = await userSeeder.CreateUserAsync(username: IntegrationTestData.BobUsername);
        await followSeeder.CreateFollowAsync(
            incoming ? otherUser : requester,
            incoming ? requester : otherUser,
            status
        );

        return new(requester, otherUser.Id);
    }

    private sealed record RelationshipSeed(Threads.Api.Data.Users.User Requester, Guid OtherUserId);
}
