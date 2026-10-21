using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Data.Follows;
using Threads.Api.Data.Notifications;
using Threads.Api.IntegrationTests.Follows.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Follows;

public class CancelOutgoingFollowRequestTests : IntegrationTestBase
{
    public CancelOutgoingFollowRequestTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task CancelOutgoingFollowRequest_ShouldDeletePendingRelationshipAndMatchingNotificationOnly()
    {
        // Arrange
        var userSeeder = Seeders.Users;
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var target = await userSeeder.CreateUserAsync(username: IntegrationTestData.BobUsername);
        var otherTarget = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.CarolUsername
        );

        await Seeders.Follows.CreateFollowAsync(requester, target, FollowStatus.Pending);
        var otherFollow = await Seeders.Follows.CreateFollowAsync(
            requester,
            otherTarget,
            FollowStatus.Pending
        );
        await Seeders.Notifications.CreateNotificationAsync(
            requester,
            target,
            NotificationType.FollowRequestReceived
        );
        var otherNotification = await Seeders.Notifications.CreateNotificationAsync(
            requester,
            otherTarget,
            NotificationType.FollowRequestReceived,
            IntegrationTestData.BaseTime.AddMinutes(1)
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.PostAsync(FollowTestRoutes.CancelOutgoing(target.Id), null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        Db.ChangeTracker.Clear();

        var persistedFollow = await Db.Follows.SingleAsync();
        persistedFollow.FollowerId.Should().Be(otherFollow.FollowerId);
        persistedFollow.FollowedId.Should().Be(otherFollow.FollowedId);

        var persistedNotification = await Db.Notifications.SingleAsync();
        persistedNotification.Id.Should().Be(otherNotification.Id);
    }

    [Fact]
    public async Task CancelOutgoingFollowRequest_ShouldReturnNoContent_WhenRelationshipDoesNotExist()
    {
        // Arrange
        var requester = await Seeders.Users.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.PostAsync(
            FollowTestRoutes.CancelOutgoing(Guid.NewGuid()),
            null
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task CancelOutgoingFollowRequest_ShouldKeepAcceptedRelationship()
    {
        // Arrange
        var seed = await SeedRelationshipAsync(FollowStatus.Accepted, requesterIsFollower: true);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.PostAsync(
            FollowTestRoutes.CancelOutgoing(seed.OtherUser.Id),
            null
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await Db.Follows.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task CancelOutgoingFollowRequest_ShouldKeepReversedPendingRelationship()
    {
        // Arrange
        var seed = await SeedRelationshipAsync(FollowStatus.Pending, requesterIsFollower: false);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.PostAsync(
            FollowTestRoutes.CancelOutgoing(seed.OtherUser.Id),
            null
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await Db.Follows.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task CancelOutgoingFollowRequest_ShouldKeepAnotherUsersPendingRelationship()
    {
        // Arrange
        var userSeeder = Seeders.Users;
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var otherRequester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.BobUsername
        );
        var target = await userSeeder.CreateUserAsync(username: IntegrationTestData.CarolUsername);
        await Seeders.Follows.CreateFollowAsync(otherRequester, target, FollowStatus.Pending);
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.PostAsync(FollowTestRoutes.CancelOutgoing(target.Id), null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await Db.Follows.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task CancelOutgoingFollowRequest_ShouldRejectAnonymousRequests()
    {
        // Arrange
        var client = CreateAnonymousClient();

        // Act
        var response = await client.PostAsync(
            FollowTestRoutes.CancelOutgoing(Guid.NewGuid()),
            null
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CancelOutgoingFollowRequest_ShouldReturnBadRequest_WhenFollowedIdIsEmpty()
    {
        // Arrange
        var requester = await Seeders.Users.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.PostAsync(FollowTestRoutes.CancelOutgoing(Guid.Empty), null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<RelationshipSeed> SeedRelationshipAsync(
        FollowStatus status,
        bool requesterIsFollower
    )
    {
        var userSeeder = Seeders.Users;
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var otherUser = await userSeeder.CreateUserAsync(username: IntegrationTestData.BobUsername);

        await Seeders.Follows.CreateFollowAsync(
            requesterIsFollower ? requester : otherUser,
            requesterIsFollower ? otherUser : requester,
            status
        );

        return new(requester, otherUser);
    }

    private sealed record RelationshipSeed(
        Threads.Api.Data.Users.User Requester,
        Threads.Api.Data.Users.User OtherUser
    );
}
