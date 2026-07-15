using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Threads.Api.Features.Comments.Endpoints;
using Threads.Api.IntegrationTests.Comments.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Comments;

public class CommentAuthorizationTests : CommentIntegrationTestBase
{
    private static readonly Guid PostId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid CommentId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public CommentAuthorizationTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task CreateComment_ShouldRejectAnonymousRequests()
    {
        // Arrange
        var client = CreateAnonymousClient();
        var requestBody = new CreateComment.Body(IntegrationTestData.ValidContent);

        // Act
        var response = await client.PostAsJsonAsync(CommentTestRoutes.ForPost(PostId), requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPostComments_ShouldRejectAnonymousRequests()
    {
        // Arrange
        var client = CreateAnonymousClient();

        // Act
        var response = await client.GetAsync(CommentTestRoutes.ForPost(PostId));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateComment_ShouldRejectAnonymousRequests()
    {
        // Arrange
        var client = CreateAnonymousClient();
        var requestBody = new UpdateComment.Body(IntegrationTestData.OtherValidContent);

        // Act
        var response = await client.PutAsJsonAsync(
            CommentTestRoutes.ForComment(PostId, CommentId),
            requestBody
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteComment_ShouldRejectAnonymousRequests()
    {
        // Arrange
        var client = CreateAnonymousClient();

        // Act
        var response = await client.DeleteAsync(CommentTestRoutes.ForComment(PostId, CommentId));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
