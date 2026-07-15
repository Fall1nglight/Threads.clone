using System.Net;
using FluentAssertions;
using Threads.Api.IntegrationTests.Infrastructure;
using Threads.Api.IntegrationTests.Likes.TestSupport;

namespace Threads.Api.IntegrationTests.Likes;

public class LikeAuthorizationTests : LikeIntegrationTestBase
{
    private const string PostId = "11111111-1111-1111-1111-111111111111";

    public LikeAuthorizationTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Theory]
    [InlineData("POST")]
    [InlineData("DELETE")]
    [InlineData("GET")]
    public async Task LikeEndpoints_ShouldRejectAnonymousRequests(string method)
    {
        // Arrange
        var client = CreateAnonymousClient();
        using var request = new HttpRequestMessage(
            new HttpMethod(method),
            $"/posts/{PostId}/likes"
        );

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
