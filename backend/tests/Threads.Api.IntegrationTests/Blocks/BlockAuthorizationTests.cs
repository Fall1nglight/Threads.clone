using System.Net;
using FluentAssertions;
using Threads.Api.IntegrationTests.Blocks.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Blocks;

public class BlockAuthorizationTests : BlockIntegrationTestBase
{
    private const string TargetId = "11111111-1111-1111-1111-111111111111";

    public BlockAuthorizationTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Theory]
    [InlineData("POST", $"{BlockTestRoutes.Base}/{TargetId}")]
    [InlineData("DELETE", $"{BlockTestRoutes.Base}/{TargetId}")]
    [InlineData("GET", BlockTestRoutes.Base)]
    public async Task BlockEndpoints_ShouldRejectAnonymousRequests(string method, string path)
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
