using System.Net;
using FluentAssertions;
using Threads.Api.Common.Pagination;
using Threads.Api.Data.Notifications;
using Threads.Api.Features.Notifications;
using Threads.Api.IntegrationTests.Infrastructure;
using Threads.Api.IntegrationTests.Notifications.TestSupport;

namespace Threads.Api.IntegrationTests.Notifications;

public class GetNotificationsTests : IntegrationTestBase
{
    public GetNotificationsTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task GetNotifications_ShouldReturnOnlyCurrentUsersNotificationsInDescendingOrder()
    {
        // Arrange
        var userSeeder = Seeders.Users;
        var notificationSeeder = Seeders.Notifications;
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var actor = await userSeeder.CreateUserAsync(username: IntegrationTestData.BobUsername);
        var otherRecipient = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.CarolUsername
        );
        var older = await notificationSeeder.CreateNotificationAsync(
            actor,
            requester,
            NotificationType.Followed,
            IntegrationTestData.BaseTime
        );
        var newer = await notificationSeeder.CreateNotificationAsync(
            actor,
            requester,
            NotificationType.PostLiked,
            IntegrationTestData.BaseTime.AddMinutes(1),
            NotificationSubjectKind.Post,
            Guid.NewGuid()
        );
        await notificationSeeder.CreateNotificationAsync(
            actor,
            otherRecipient,
            NotificationType.Followed
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.GetAsync(NotificationTestRoutes.Base);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await ReadJsonAsync<PagedResponse<NotificationDto>>(response);
        responseBody.Items.Select(item => item.Id).Should().Equal(newer.Id, older.Id);
        responseBody.Items.Should().OnlyContain(item => item.TriggeredByUser.Id == actor.Id);
    }

    [Theory]
    [InlineData("Follow", NotificationType.Followed)]
    [InlineData("PostLiked", NotificationType.PostLiked)]
    [InlineData("PostCommented", NotificationType.PostCommented)]
    public async Task GetNotifications_ShouldFilterByCategory(
        string category,
        NotificationType expectedType
    )
    {
        // Arrange
        var userSeeder = Seeders.Users;
        var notificationSeeder = Seeders.Notifications;
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var actor = await userSeeder.CreateUserAsync(username: IntegrationTestData.BobUsername);
        await notificationSeeder.CreateNotificationAsync(
            actor,
            requester,
            NotificationType.Followed
        );
        await notificationSeeder.CreateNotificationAsync(
            actor,
            requester,
            NotificationType.PostLiked,
            IntegrationTestData.BaseTime.AddMinutes(1),
            NotificationSubjectKind.Post,
            Guid.NewGuid()
        );
        await notificationSeeder.CreateNotificationAsync(
            actor,
            requester,
            NotificationType.PostCommented,
            IntegrationTestData.BaseTime.AddMinutes(2),
            NotificationSubjectKind.Post,
            Guid.NewGuid()
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.GetAsync($"{NotificationTestRoutes.Base}?category={category}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await ReadJsonAsync<PagedResponse<NotificationDto>>(response);
        responseBody.Items.Should().ContainSingle().Which.Type.Should().Be(expectedType);
    }

    [Fact]
    public async Task GetNotifications_ShouldReturnEveryFollowType_WhenFollowCategoryIsRequested()
    {
        // Arrange
        var userSeeder = Seeders.Users;
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var actor = await userSeeder.CreateUserAsync(username: IntegrationTestData.BobUsername);
        var expectedTypes = new[]
        {
            NotificationType.Followed,
            NotificationType.FollowRequestAccepted,
            NotificationType.FollowRequestReceived,
        };

        foreach (var (type, index) in expectedTypes.Select((type, index) => (type, index)))
        {
            await Seeders.Notifications.CreateNotificationAsync(
                actor,
                requester,
                type,
                IntegrationTestData.BaseTime.AddMinutes(index)
            );
        }

        await Seeders.Notifications.CreateNotificationAsync(
            actor,
            requester,
            NotificationType.PostLiked,
            IntegrationTestData.BaseTime.AddMinutes(3),
            NotificationSubjectKind.Post,
            Guid.NewGuid()
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.GetAsync(
            $"{NotificationTestRoutes.Base}?category={NotificationCategory.Follow}"
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await ReadJsonAsync<PagedResponse<NotificationDto>>(response);
        responseBody.Items.Select(item => item.Type).Should().BeEquivalentTo(expectedTypes);
    }

    [Theory]
    [InlineData("Seen", true)]
    [InlineData("Unseen", false)]
    public async Task GetNotifications_ShouldFilterByStatus(string status, bool expectSeen)
    {
        // Arrange
        var userSeeder = Seeders.Users;
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var actor = await userSeeder.CreateUserAsync(username: IntegrationTestData.BobUsername);
        var unseen = await Seeders.Notifications.CreateNotificationAsync(
            actor,
            requester,
            NotificationType.Followed
        );
        var seen = await Seeders.Notifications.CreateNotificationAsync(
            actor,
            requester,
            NotificationType.FollowRequestAccepted,
            IntegrationTestData.BaseTime.AddMinutes(1),
            seenAtUtc: IntegrationTestData.BaseTime.AddMinutes(2)
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.GetAsync($"{NotificationTestRoutes.Base}?status={status}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await ReadJsonAsync<PagedResponse<NotificationDto>>(response);
        responseBody.Items.Should().ContainSingle();
        responseBody.Items[0].Id.Should().Be(expectSeen ? seen.Id : unseen.Id);
        responseBody.Items[0].SeenAtUtc.HasValue.Should().Be(expectSeen);
    }

    [Theory]
    [InlineData("?category=999")]
    [InlineData("?status=999")]
    public async Task GetNotifications_ShouldReturnBadRequest_WhenFilterIsInvalid(
        string queryString
    )
    {
        // Arrange
        var requester = await Seeders.Users.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.GetAsync($"{NotificationTestRoutes.Base}{queryString}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetNotifications_ShouldPaginateAndExcludeBlockedActors()
    {
        // Arrange
        var userSeeder = Seeders.Users;
        var notificationSeeder = Seeders.Notifications;
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var actor = await userSeeder.CreateUserAsync(username: IntegrationTestData.BobUsername);
        var blockedActor = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.CarolUsername
        );
        await Seeders.Blocks.CreateUserBlockAsync(requester, blockedActor);
        for (var index = 0; index < 3; index++)
        {
            await notificationSeeder.CreateNotificationAsync(
                actor,
                requester,
                NotificationType.Followed,
                IntegrationTestData.BaseTime.AddMinutes(index)
            );
        }
        await notificationSeeder.CreateNotificationAsync(
            blockedActor,
            requester,
            NotificationType.Followed,
            IntegrationTestData.BaseTime.AddMinutes(4)
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var firstResponse = await client.GetAsync($"{NotificationTestRoutes.Base}?pageSize=2");
        var firstPage = await ReadJsonAsync<PagedResponse<NotificationDto>>(firstResponse);
        var secondResponse = await client.GetAsync(
            $"{NotificationTestRoutes.Base}?pageSize=2&cursor={Uri.EscapeDataString(firstPage.Cursor!)}"
        );
        var secondPage = await ReadJsonAsync<PagedResponse<NotificationDto>>(secondResponse);

        // Assert
        firstPage.Items.Should().HaveCount(2);
        secondPage.Items.Should().ContainSingle();
        firstPage
            .Items.Concat(secondPage.Items)
            .Should()
            .OnlyContain(item => item.TriggeredByUser.Id == actor.Id);
    }
}
