using System.Net;
using FluentAssertions;
using Threads.Api.Common.Pagination;
using Threads.Api.Features.Comments;
using Threads.Api.IntegrationTests.Comments.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Comments;

public class GetPostCommentsTests : CommentIntegrationTestBase
{
    public GetPostCommentsTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task GetPostComments_ShouldReturnEmptyPage_WhenPostHasNoComments()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var postSeeder = CreatePostSeeder();
        var user = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var post = await postSeeder.CreatePostAsync(user, IntegrationTestData.ValidContent);
        var client = await CreateAuthenticatedClientAsync(user);

        // Act
        var response = await client.GetAsync(CommentTestRoutes.ForPost(post.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<CommentDto>>(response);
        responseBody.Items.Should().BeEmpty();
        responseBody.Cursor.Should().BeNull();
        responseBody.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task GetPostComments_ShouldReturnVisibleAuthorsAndTargetPostCommentsOnly()
    {
        // Arrange
        var scenarioSeeder = CreateCommentListScenarioSeeder();
        var seed = await scenarioSeeder.SeedAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.GetAsync(CommentTestRoutes.ForPost(seed.Post.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<CommentDto>>(response);
        responseBody
            .Items.Select(comment => comment.Id)
            .Should()
            .Equal(seed.VisibleComment.Id, seed.RequesterComment.Id);
        responseBody.Items.Should().NotContain(comment => comment.Id == seed.BlockedComment.Id);
        responseBody.Items.Should().NotContain(comment => comment.Id == seed.BlockingComment.Id);
        responseBody.Items.Should().NotContain(comment => comment.Id == seed.OtherPostComment.Id);

        var visibleComment = responseBody.Items[0];
        visibleComment.PostId.Should().Be(seed.Post.Id);
        visibleComment.User.Id.Should().Be(seed.VisibleComment.UserId);
        visibleComment.Content.Should().Be(seed.VisibleComment.Content);
        visibleComment.CreatedAtUtc.Should().Be(seed.VisibleComment.CreatedAtUtc);
        visibleComment.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task GetPostComments_ShouldReturnNextPage_WhenCursorIsProvided()
    {
        // Arrange
        var scenarioSeeder = CreateCommentListScenarioSeeder();
        var seed = await scenarioSeeder.SeedBulkCommentsAsync(5);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);
        var route = CommentTestRoutes.ForPost(seed.Post.Id);

        // Act
        var firstResponse = await client.GetAsync($"{route}?pageSize=2");
        var firstPage = await ReadJsonAsync<PagedResponse<CommentDto>>(firstResponse);
        var secondResponse = await client.GetAsync(
            $"{route}?pageSize=2&cursor={Uri.EscapeDataString(firstPage.Cursor!)}"
        );

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondPage = await ReadJsonAsync<PagedResponse<CommentDto>>(secondResponse);
        firstPage.Items.Should().HaveCount(2);
        secondPage.Items.Should().HaveCount(2);
        firstPage
            .Items.Select(item => item.Id)
            .Should()
            .NotIntersectWith(secondPage.Items.Select(item => item.Id));
    }

    [Fact]
    public async Task GetPostComments_ShouldReturnNotFound_WhenPostDoesNotExist()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var user = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var client = await CreateAuthenticatedClientAsync(user);

        // Act
        var response = await client.GetAsync(CommentTestRoutes.ForPost(Guid.NewGuid()));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPostComments_ShouldReturnNotFound_WhenPostIsNotVisible()
    {
        // Arrange
        var scenarioSeeder = CreatePostVisibilityScenarioSeeder();
        var seed = await scenarioSeeder.SeedAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Alice);

        // Act
        var pendingResponse = await client.GetAsync(
            CommentTestRoutes.ForPost(seed.ErinPrivatePendingPost.Id)
        );
        var strangerResponse = await client.GetAsync(
            CommentTestRoutes.ForPost(seed.FrankPrivateStrangerPost.Id)
        );
        var deletedResponse = await client.GetAsync(
            CommentTestRoutes.ForPost(seed.DeletedPublicPost.Id)
        );

        // Assert
        pendingResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        strangerResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        deletedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetPostComments_ShouldReturnNotFound_WhenRequesterAndOwnerAreBlocked(
        bool requesterBlocksOwner
    )
    {
        // Arrange
        var scenarioSeeder = CreatePostVisibilityScenarioSeeder();
        var blockSeeder = CreateBlockSeeder();
        var seed = await scenarioSeeder.SeedAsync();
        await blockSeeder.CreateUserBlockAsync(
            requesterBlocksOwner ? seed.Alice : seed.BobPublic,
            requesterBlocksOwner ? seed.BobPublic : seed.Alice
        );
        var client = await CreateAuthenticatedClientAsync(seed.Alice);

        // Act
        var response = await client.GetAsync(CommentTestRoutes.ForPost(seed.BobPublicPost.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPostComments_ShouldReturnBadRequest_WhenPostIdIsEmpty()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var user = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var client = await CreateAuthenticatedClientAsync(user);

        // Act
        var response = await client.GetAsync(CommentTestRoutes.ForPost(Guid.Empty));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetPostComments_ShouldUseDefaultPageSize()
    {
        // Arrange
        var scenarioSeeder = CreateCommentListScenarioSeeder();
        var seed = await scenarioSeeder.SeedBulkCommentsAsync(21);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.GetAsync(CommentTestRoutes.ForPost(seed.Post.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<CommentDto>>(response);
        responseBody.Items.Should().HaveCount(20);
        responseBody.Cursor.Should().NotBeNullOrWhiteSpace();
        responseBody.HasMore.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task GetPostComments_ShouldClampPageSizeToMinimum(int pageSize)
    {
        // Arrange
        var scenarioSeeder = CreateCommentListScenarioSeeder();
        var seed = await scenarioSeeder.SeedBulkCommentsAsync(2);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.GetAsync(
            $"{CommentTestRoutes.ForPost(seed.Post.Id)}?pageSize={pageSize}"
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<CommentDto>>(response);
        responseBody.Items.Should().ContainSingle();
        responseBody.HasMore.Should().BeTrue();
    }

    [Theory]
    [InlineData(101)]
    [InlineData(500)]
    public async Task GetPostComments_ShouldClampPageSizeToMaximum(int pageSize)
    {
        // Arrange
        var scenarioSeeder = CreateCommentListScenarioSeeder();
        var seed = await scenarioSeeder.SeedBulkCommentsAsync(101);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.GetAsync(
            $"{CommentTestRoutes.ForPost(seed.Post.Id)}?pageSize={pageSize}"
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<CommentDto>>(response);
        responseBody.Items.Should().HaveCount(100);
        responseBody.HasMore.Should().BeTrue();
    }

    [Fact]
    public async Task GetPostComments_ShouldTreatInvalidCursorAsFirstPage()
    {
        // Arrange
        var scenarioSeeder = CreateCommentListScenarioSeeder();
        var seed = await scenarioSeeder.SeedBulkCommentsAsync(3);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);
        var route = CommentTestRoutes.ForPost(seed.Post.Id);

        // Act
        var firstResponse = await client.GetAsync($"{route}?pageSize=2");
        var invalidCursorResponse = await client.GetAsync(
            $"{route}?pageSize=2&cursor=not-a-valid-cursor"
        );

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        invalidCursorResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var firstPage = await ReadJsonAsync<PagedResponse<CommentDto>>(firstResponse);
        var invalidCursorPage = await ReadJsonAsync<PagedResponse<CommentDto>>(
            invalidCursorResponse
        );
        invalidCursorPage
            .Items.Select(item => item.Id)
            .Should()
            .Equal(firstPage.Items.Select(item => item.Id));
    }
}
