using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Data.Follows;
using Threads.Api.Data.Notifications;
using Threads.Api.IntegrationTests.Follows.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Follows;

public class DeleteFollowingTests : IntegrationTestBase
{
    public DeleteFollowingTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task DeleteFollowing_ShouldDeleteAcceptedOutgoingRelationship()
    {
        // Arrange
        var seed = await SeedRelationshipAsync(FollowStatus.Accepted, outgoing: true);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.DeleteAsync(FollowTestRoutes.DeleteFollowing(seed.OtherUserId));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await Db.Follows.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task DeleteFollowing_ShouldReturnNoContent_WhenRelationshipDoesNotExist()
    {
        // Arrange
        var requester = await Seeders.Users.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.DeleteAsync(FollowTestRoutes.DeleteFollowing(Guid.NewGuid()));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteFollowing_ShouldKeepPendingRelationshipAndNotification()
    {
        // Arrange
        var seed = await SeedRelationshipAsync(FollowStatus.Pending, outgoing: true);
        var target = await Db.Users.SingleAsync(user => user.Id == seed.OtherUserId);
        await Seeders.Notifications.CreateNotificationAsync(
            seed.Requester,
            target,
            NotificationType.FollowRequestReceived
        );
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.DeleteAsync(FollowTestRoutes.DeleteFollowing(seed.OtherUserId));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await Db.Follows.CountAsync()).Should().Be(1);
        (await Db.Notifications.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task DeleteFollowing_ShouldKeepReversedRelationship()
    {
        // Arrange
        var seed = await SeedRelationshipAsync(FollowStatus.Accepted, outgoing: false);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.DeleteAsync(FollowTestRoutes.DeleteFollowing(seed.OtherUserId));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await Db.Follows.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task DeleteFollowing_ShouldReturnBadRequest_WhenFollowedIdIsEmpty()
    {
        // Arrange
        var requester = await Seeders.Users.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.DeleteAsync(FollowTestRoutes.DeleteFollowing(Guid.Empty));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<RelationshipSeed> SeedRelationshipAsync(FollowStatus status, bool outgoing)
    {
        var userSeeder = Seeders.Users;
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var otherUser = await userSeeder.CreateUserAsync(username: IntegrationTestData.BobUsername);
        await Seeders.Follows.CreateFollowAsync(
            outgoing ? requester : otherUser,
            outgoing ? otherUser : requester,
            status
        );

        return new(requester, otherUser.Id);
    }

    private sealed record RelationshipSeed(Threads.Api.Data.Users.User Requester, Guid OtherUserId);
}
