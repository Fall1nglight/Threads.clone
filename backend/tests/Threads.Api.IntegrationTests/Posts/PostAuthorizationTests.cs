using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Posts;

public class PostAuthorizationTests : IntegrationTestBase
{
    private static readonly Guid AuthSmokePostId = Guid.Parse(
        "11111111-1111-1111-1111-111111111111"
    );

    public PostAuthorizationTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task GetAnonymousFeed_ShouldAllowAnonymousRequests()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync(PostTestRoutes.AnonymousFeed);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData(PostTestRoutes.GlobalFeed)]
    [InlineData(PostTestRoutes.MyFeed)]
    public async Task ProtectedGetFeedEndpoints_ShouldRejectAnonymousRequests(string path)
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync(path);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedGetPostEndpoint_ShouldRejectAnonymousRequests()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync($"/posts/{AuthSmokePostId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedDeletePostEndpoint_ShouldRejectAnonymousRequests()
    {
        var client = CreateAnonymousClient();

        var response = await client.DeleteAsync($"/posts/{AuthSmokePostId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedCreatePostEndpoint_ShouldRejectAnonymousRequests()
    {
        var client = CreateAnonymousClient();
        var body = JsonContent.Create(new { content = "Valid content" });

        var response = await client.PostAsync(PostTestRoutes.Create, body);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedUpdatePostEndpoint_ShouldRejectAnonymousRequests()
    {
        var client = CreateAnonymousClient();
        var body = JsonContent.Create(new { content = "Valid content" });

        var response = await client.PutAsync($"/posts/{AuthSmokePostId}", body);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
