using System.Net;
using FluentAssertions;
using Threads.Api.Data.Notifications;
using Threads.Api.Features.Notifications;
using Threads.Api.IntegrationTests.Infrastructure;
using Threads.Api.IntegrationTests.Notifications.TestSupport;

namespace Threads.Api.IntegrationTests.Notifications;

public class GetUnseenNotificationCountTests : IntegrationTestBase
{
    public GetUnseenNotificationCountTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task GetUnseenNotificationCount_ShouldReturnZeroWhenThereAreNoNotifications()
    {
        // Arrange
        var requester = await Seeders.Users.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.GetAsync(NotificationTestRoutes.UnseenCount);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await ReadJsonAsync<UnseenNotificationCountDto>(response);
        responseBody.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetUnseenNotificationCount_ShouldCountOnlyCurrentUsersVisibleUnseenNotifications()
    {
        // Arrange
        var userSeeder = Seeders.Users;
        var notificationSeeder = Seeders.Notifications;
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var visibleActor = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.BobUsername
        );
        var blockedActor = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.CarolUsername
        );
        var blockingActor = await userSeeder.CreateUserAsync(username: "dave");
        var otherRecipient = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.HenryUsername
        );

        await notificationSeeder.CreateNotificationAsync(
            visibleActor,
            requester,
            NotificationType.Followed
        );
        await notificationSeeder.CreateNotificationAsync(
            visibleActor,
            requester,
            NotificationType.PostLiked,
            subjectKind: NotificationSubjectKind.Post,
            subjectId: Guid.NewGuid()
        );
        await notificationSeeder.CreateNotificationAsync(
            visibleActor,
            requester,
            NotificationType.PostCommented,
            subjectKind: NotificationSubjectKind.Post,
            subjectId: Guid.NewGuid(),
            seenAtUtc: IntegrationTestData.BaseTime.AddMinutes(1)
        );
        await notificationSeeder.CreateNotificationAsync(
            visibleActor,
            otherRecipient,
            NotificationType.Followed
        );
        await notificationSeeder.CreateNotificationAsync(
            blockedActor,
            requester,
            NotificationType.Followed
        );
        await notificationSeeder.CreateNotificationAsync(
            blockingActor,
            requester,
            NotificationType.Followed
        );
        await Seeders.Blocks.CreateUserBlockAsync(requester, blockedActor);
        await Seeders.Blocks.CreateUserBlockAsync(blockingActor, requester);
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.GetAsync(NotificationTestRoutes.UnseenCount);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await ReadJsonAsync<UnseenNotificationCountDto>(response);
        responseBody.Count.Should().Be(2);
    }
}
