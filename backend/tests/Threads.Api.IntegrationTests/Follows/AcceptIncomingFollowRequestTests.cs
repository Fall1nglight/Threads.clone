using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Data.Follows;
using Threads.Api.Data.Notifications;
using Threads.Api.Data.Users;
using Threads.Api.IntegrationTests.Follows.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Follows;

public class AcceptIncomingFollowRequestTests : IntegrationTestBase
{
    public AcceptIncomingFollowRequestTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task AcceptIncomingFollowRequest_ShouldAcceptPendingRelationship()
    {
        // Arrange
        var seed = await CreateRelationshipAsync(FollowStatus.Pending);
        var receivedNotification = await Seeders.Notifications.CreateNotificationAsync(
            seed.OtherUser,
            seed.Requester,
            NotificationType.FollowRequestReceived
        );
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.PostAsync(
            FollowTestRoutes.AcceptIncoming(seed.OtherUser.Id),
            null
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        Db.ChangeTracker.Clear();

        (await Db.Follows.SingleAsync()).Status.Should().Be(FollowStatus.Accepted);

        var notifications = await Db
            .Notifications.OrderBy(item => item.TriggeredAtUtc)
            .ToListAsync();
        notifications.Should().HaveCount(2);
        var followedNotification = notifications.Single(item => item.Id == receivedNotification.Id);
        followedNotification.Type.Should().Be(NotificationType.Followed);
        followedNotification.TriggeredAtUtc.Should().Be(receivedNotification.TriggeredAtUtc);
        notifications
            .Should()
            .ContainSingle(item =>
                item.Type == NotificationType.FollowRequestAccepted
                && item.TriggeredByUserId == seed.Requester.Id
                && item.NotifiedUserId == seed.OtherUser.Id
            );
    }

    [Fact]
    public async Task AcceptIncomingFollowRequest_ShouldReturnNotFound_WhenRelationshipDoesNotExist()
    {
        // Arrange
        var userSeeder = Seeders.Users;
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var follower = await userSeeder.CreateUserAsync(username: IntegrationTestData.BobUsername);
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.PostAsync(FollowTestRoutes.AcceptIncoming(follower.Id), null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AcceptIncomingFollowRequest_ShouldReturnNotFound_WhenRelationshipIsReversed()
    {
        // Arrange
        var userSeeder = Seeders.Users;
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var otherUser = await userSeeder.CreateUserAsync(username: IntegrationTestData.BobUsername);
        await Seeders.Follows.CreateFollowAsync(requester, otherUser, FollowStatus.Pending);
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.PostAsync(FollowTestRoutes.AcceptIncoming(otherUser.Id), null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Db.Follows.SingleAsync()).Status.Should().Be(FollowStatus.Pending);
    }

    [Fact]
    public async Task AcceptIncomingFollowRequest_ShouldReturnNotFound_WhenRelationshipIsAccepted()
    {
        // Arrange
        var seed = await CreateRelationshipAsync(FollowStatus.Accepted);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.PostAsync(
            FollowTestRoutes.AcceptIncoming(seed.OtherUser.Id),
            null
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Db.Follows.SingleAsync()).Status.Should().Be(FollowStatus.Accepted);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AcceptIncomingFollowRequest_ShouldReturnNotFound_WhenUsersAreBlocked(
        bool requesterBlocksFollower
    )
    {
        // Arrange
        var seed = await CreateRelationshipAsync(FollowStatus.Pending);
        await Seeders.Blocks.CreateUserBlockAsync(
            requesterBlocksFollower ? seed.Requester : seed.OtherUser,
            requesterBlocksFollower ? seed.OtherUser : seed.Requester
        );
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.PostAsync(
            FollowTestRoutes.AcceptIncoming(seed.OtherUser.Id),
            null
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        Db.ChangeTracker.Clear();

        (await Db.Follows.SingleAsync()).Status.Should().Be(FollowStatus.Pending);
    }

    [Fact]
    public async Task AcceptIncomingFollowRequest_ShouldReturnBadRequest_WhenFollowerIdIsEmpty()
    {
        // Arrange
        var requester = await Seeders.Users.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.PostAsync(FollowTestRoutes.AcceptIncoming(Guid.Empty), null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<IncomingRelationshipSeed> CreateRelationshipAsync(FollowStatus status)
    {
        var userSeeder = Seeders.Users;
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var otherUser = await userSeeder.CreateUserAsync(username: IntegrationTestData.BobUsername);
        await Seeders.Follows.CreateFollowAsync(otherUser, requester, status);

        return new(requester, otherUser);
    }

    private sealed record IncomingRelationshipSeed(User Requester, User OtherUser);
}
