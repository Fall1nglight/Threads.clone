using System.Net;
using FluentAssertions;
using Threads.Api.IntegrationTests.Follows.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Follows;

public class FollowAuthorizationTests : FollowIntegrationTestBase
{
    private const string UserId = "11111111-1111-1111-1111-111111111111";

    public FollowAuthorizationTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Theory]
    [InlineData("POST", $"{FollowTestRoutes.Base}/{UserId}")]
    [InlineData("POST", $"{FollowTestRoutes.IncomingRequests}/{UserId}/accept")]
    [InlineData("POST", $"{FollowTestRoutes.IncomingRequests}/{UserId}/reject")]
    [InlineData("GET", FollowTestRoutes.IncomingRequests)]
    [InlineData("GET", FollowTestRoutes.OutgoingRequests)]
    [InlineData("GET", FollowTestRoutes.Followers)]
    [InlineData("GET", FollowTestRoutes.Following)]
    [InlineData("DELETE", $"{FollowTestRoutes.Followers}/{UserId}")]
    [InlineData("DELETE", $"{FollowTestRoutes.Following}/{UserId}")]
    public async Task FollowEndpoints_ShouldRejectAnonymousRequests(string method, string path)
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
