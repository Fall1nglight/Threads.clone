using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Data.Notifications;
using Threads.Api.IntegrationTests.Infrastructure;
using Threads.Api.IntegrationTests.Notifications.TestSupport;

namespace Threads.Api.IntegrationTests.Notifications;

public class MarkNotificationAsSeenTests : IntegrationTestBase
{
    public MarkNotificationAsSeenTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task MarkNotificationAsSeen_ShouldSetTimestampAndRemainIdempotent()
    {
        // Arrange
        var userSeeder = Seeders.Users;
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var actor = await userSeeder.CreateUserAsync(username: IntegrationTestData.BobUsername);
        var notification = await Seeders.Notifications.CreateNotificationAsync(
            actor,
            requester,
            NotificationType.Followed
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var firstResponse = await client.PostAsync(
            NotificationTestRoutes.MarkAsSeen(notification.Id),
            null
        );
        var secondResponse = await client.PostAsync(
            NotificationTestRoutes.MarkAsSeen(notification.Id),
            null
        );

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        Db.ChangeTracker.Clear();
        (await Db.Notifications.SingleAsync()).SeenAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task MarkNotificationAsSeen_ShouldReturnNotFoundForAnotherUsersNotification()
    {
        // Arrange
        var userSeeder = Seeders.Users;
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var actor = await userSeeder.CreateUserAsync(username: IntegrationTestData.BobUsername);
        var owner = await userSeeder.CreateUserAsync(username: IntegrationTestData.CarolUsername);
        var notification = await Seeders.Notifications.CreateNotificationAsync(
            actor,
            owner,
            NotificationType.Followed
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.PostAsync(
            NotificationTestRoutes.MarkAsSeen(notification.Id),
            null
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        Db.ChangeTracker.Clear();
        (await Db.Notifications.SingleAsync()).SeenAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task MarkNotificationAsSeen_ShouldReturnNotFound_WhenNotificationDoesNotExist()
    {
        // Arrange
        var requester = await Seeders.Users.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.PostAsync(
            NotificationTestRoutes.MarkAsSeen(Guid.NewGuid()),
            null
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MarkNotificationAsSeen_ShouldReturnBadRequest_WhenNotificationIdIsEmpty()
    {
        // Arrange
        var requester = await Seeders.Users.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.PostAsync(NotificationTestRoutes.MarkAsSeen(Guid.Empty), null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
