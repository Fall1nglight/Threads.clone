using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Threads.Api.IntegrationTests.Comments.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Comments;

public class DeleteCommentTests : CommentIntegrationTestBase
{
    public DeleteCommentTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task DeleteComment_ShouldRemoveComment_WhenRequesterIsOwner()
    {
        // Arrange
        var seed = await CreateCommentAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Owner);

        // Act
        var response = await client.DeleteAsync(
            CommentTestRoutes.ForComment(seed.PostId, seed.CommentId)
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        Db.ChangeTracker.Clear();

        (await Db.Comments.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task DeleteComment_ShouldReturnForbidden_WhenRequesterIsNotOwner()
    {
        // Arrange
        var seed = await CreateCommentAsync();
        var userSeeder = CreateUserSeeder();
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.HenryUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.DeleteAsync(
            CommentTestRoutes.ForComment(seed.PostId, seed.CommentId)
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await Db.Comments.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task DeleteComment_ShouldReturnNoContent_WhenCommentDoesNotExist()
    {
        // Arrange
        var seed = await CreateCommentAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Owner);

        // Act
        var response = await client.DeleteAsync(
            CommentTestRoutes.ForComment(seed.PostId, Guid.NewGuid())
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await Db.Comments.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task DeleteComment_ShouldReturnNoContent_WhenCommentBelongsToDifferentPost()
    {
        // Arrange
        var seed = await CreateCommentAsync();
        var postSeeder = CreatePostSeeder();
        var otherPost = await postSeeder.CreatePostAsync(
            seed.Owner,
            IntegrationTestData.OtherValidContent
        );
        var client = await CreateAuthenticatedClientAsync(seed.Owner);

        // Act
        var response = await client.DeleteAsync(
            CommentTestRoutes.ForComment(otherPost.Id, seed.CommentId)
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await Db.Comments.CountAsync()).Should().Be(1);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeleteComment_ShouldReturnBadRequest_WhenRouteIdIsEmpty(bool emptyPostId)
    {
        // Arrange
        var seed = await CreateCommentAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Owner);

        // Act
        var response = await client.DeleteAsync(
            CommentTestRoutes.ForComment(
                emptyPostId ? Guid.Empty : seed.PostId,
                emptyPostId ? seed.CommentId : Guid.Empty
            )
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Db.Comments.CountAsync()).Should().Be(1);
    }

    private async Task<CommentSeed> CreateCommentAsync()
    {
        var userSeeder = CreateUserSeeder();
        var postSeeder = CreatePostSeeder();
        var commentSeeder = CreateCommentSeeder();
        var owner = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var post = await postSeeder.CreatePostAsync(owner, IntegrationTestData.ValidContent);
        var comment = await commentSeeder.CreateCommentAsync(
            owner,
            post,
            IntegrationTestData.ValidContent
        );

        return new(owner, post.Id, comment.Id);
    }

    private sealed record CommentSeed(
        Threads.Api.Data.Users.User Owner,
        Guid PostId,
        Guid CommentId
    );
}
