using System.Net;
using FluentAssertions;
using Threads.Api.IntegrationTests.Infrastructure;
using Threads.Api.IntegrationTests.Notifications.TestSupport;

namespace Threads.Api.IntegrationTests.Notifications;

public class NotificationAuthorizationTests : IntegrationTestBase
{
    private const string NotificationId = "11111111-1111-1111-1111-111111111111";

    public NotificationAuthorizationTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Theory]
    [InlineData("GET", NotificationTestRoutes.Base)]
    [InlineData("GET", NotificationTestRoutes.UnseenCount)]
    [InlineData("POST", $"{NotificationTestRoutes.Base}/{NotificationId}/seen")]
    public async Task NotificationEndpoints_ShouldRejectAnonymousRequests(
        string method,
        string path
    )
    {
        // Arrange
        var client = CreateAnonymousClient();
        using var request = new HttpRequestMessage(new HttpMethod(method), path);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
