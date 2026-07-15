using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Features.Comments.Endpoints;
using Threads.Api.IntegrationTests.Comments.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Comments;

public class UpdateCommentTests : CommentIntegrationTestBase
{
    public UpdateCommentTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task UpdateComment_ShouldUpdateContentAndTimestamp_WhenRequesterIsOwner()
    {
        // Arrange
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
        var client = await CreateAuthenticatedClientAsync(owner);
        var requestBody = CreateUpdateCommentRequest(IntegrationTestData.OtherValidContent);
        var before = DateTime.UtcNow;

        // Act
        var response = await client.PutAsJsonAsync(
            CommentTestRoutes.ForComment(post.Id, comment.Id),
            requestBody
        );
        var after = DateTime.UtcNow;

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        Db.ChangeTracker.Clear();

        var persistedComment = await Db.Comments.SingleAsync();
        persistedComment.Content.Should().Be(IntegrationTestData.OtherValidContent);
        persistedComment.CreatedAtUtc.Should().Be(IntegrationTestData.BaseTime);
        persistedComment.UpdatedAtUtc.Should().NotBeNull();
        persistedComment.UpdatedAtUtc!.Value.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public async Task UpdateComment_ShouldReturnForbidden_WhenRequesterIsNotOwner()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var postSeeder = CreatePostSeeder();
        var commentSeeder = CreateCommentSeeder();
        var owner = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.HenryUsername
        );
        var post = await postSeeder.CreatePostAsync(owner, IntegrationTestData.ValidContent);
        var comment = await commentSeeder.CreateCommentAsync(
            owner,
            post,
            IntegrationTestData.ValidContent
        );
        var client = await CreateAuthenticatedClientAsync(requester);
        var requestBody = CreateUpdateCommentRequest(IntegrationTestData.OtherValidContent);

        // Act
        var response = await client.PutAsJsonAsync(
            CommentTestRoutes.ForComment(post.Id, comment.Id),
            requestBody
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        Db.ChangeTracker.Clear();

        var persistedComment = await Db.Comments.SingleAsync();
        persistedComment.Content.Should().Be(IntegrationTestData.ValidContent);
        persistedComment.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task UpdateComment_ShouldReturnNotFound_WhenCommentDoesNotExist()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var postSeeder = CreatePostSeeder();
        var owner = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var post = await postSeeder.CreatePostAsync(owner, IntegrationTestData.ValidContent);
        var client = await CreateAuthenticatedClientAsync(owner);
        var requestBody = CreateUpdateCommentRequest(IntegrationTestData.OtherValidContent);

        // Act
        var response = await client.PutAsJsonAsync(
            CommentTestRoutes.ForComment(post.Id, Guid.NewGuid()),
            requestBody
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateComment_ShouldReturnNotFound_WhenCommentBelongsToDifferentPost()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var postSeeder = CreatePostSeeder();
        var commentSeeder = CreateCommentSeeder();
        var owner = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var post = await postSeeder.CreatePostAsync(owner, IntegrationTestData.ValidContent);
        var otherPost = await postSeeder.CreatePostAsync(
            owner,
            IntegrationTestData.OtherValidContent
        );
        var comment = await commentSeeder.CreateCommentAsync(
            owner,
            post,
            IntegrationTestData.ValidContent
        );
        var client = await CreateAuthenticatedClientAsync(owner);
        var requestBody = CreateUpdateCommentRequest(IntegrationTestData.OtherValidContent);

        // Act
        var response = await client.PutAsJsonAsync(
            CommentTestRoutes.ForComment(otherPost.Id, comment.Id),
            requestBody
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        Db.ChangeTracker.Clear();

        (await Db.Comments.SingleAsync()).Content.Should().Be(IntegrationTestData.ValidContent);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UpdateComment_ShouldReturnBadRequest_WhenRouteIdIsEmpty(bool emptyPostId)
    {
        // Arrange
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
        var client = await CreateAuthenticatedClientAsync(owner);
        var requestBody = CreateUpdateCommentRequest(IntegrationTestData.OtherValidContent);

        // Act
        var response = await client.PutAsJsonAsync(
            CommentTestRoutes.ForComment(
                emptyPostId ? Guid.Empty : post.Id,
                emptyPostId ? comment.Id : Guid.Empty
            ),
            requestBody
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateComment_ShouldAcceptContentAtMaximumLength()
    {
        // Arrange
        var seed = await CreateOwnedCommentAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Owner);
        var requestBody = CreateUpdateCommentRequest(IntegrationTestData.MaxLengthContent);

        // Act
        var response = await client.PutAsJsonAsync(
            CommentTestRoutes.ForComment(seed.PostId, seed.CommentId),
            requestBody
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task UpdateComment_ShouldReturnBadRequest_WhenContentIsEmpty(string? content)
    {
        // Arrange
        var seed = await CreateOwnedCommentAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Owner);
        var requestBody = CreateUpdateCommentRequest(content);

        // Act
        var response = await client.PutAsJsonAsync(
            CommentTestRoutes.ForComment(seed.PostId, seed.CommentId),
            requestBody
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await AssertCommentUnchangedAsync();
    }

    [Theory]
    [InlineData(601)]
    [InlineData(999)]
    public async Task UpdateComment_ShouldReturnBadRequest_WhenContentIsTooLong(int length)
    {
        // Arrange
        var seed = await CreateOwnedCommentAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Owner);
        var requestBody = CreateUpdateCommentRequest(IntegrationTestData.ContentOfLength(length));

        // Act
        var response = await client.PutAsJsonAsync(
            CommentTestRoutes.ForComment(seed.PostId, seed.CommentId),
            requestBody
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await AssertCommentUnchangedAsync();
    }

    [Fact]
    public async Task UpdateComment_ShouldReturnBadRequest_WhenContentIsMissing()
    {
        // Arrange
        var seed = await CreateOwnedCommentAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Owner);
        var requestBody = new { };

        // Act
        var response = await client.PutAsJsonAsync(
            CommentTestRoutes.ForComment(seed.PostId, seed.CommentId),
            requestBody
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await AssertCommentUnchangedAsync();
    }

    [Fact]
    public async Task UpdateComment_ShouldReturnBadRequest_WhenJsonContainsUnknownProperty()
    {
        // Arrange
        var seed = await CreateOwnedCommentAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Owner);
        var requestBody = new
        {
            content = IntegrationTestData.OtherValidContent,
            unexpected = true,
        };

        // Act
        var response = await client.PutAsJsonAsync(
            CommentTestRoutes.ForComment(seed.PostId, seed.CommentId),
            requestBody
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await AssertCommentUnchangedAsync();
    }

    [Fact]
    public async Task UpdateComment_ShouldReturnBadRequest_WhenJsonIsMalformed()
    {
        // Arrange
        var seed = await CreateOwnedCommentAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Owner);
        using var requestBody = new StringContent(
            "{\"content\":\"Valid content\"",
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await client.PutAsync(
            CommentTestRoutes.ForComment(seed.PostId, seed.CommentId),
            requestBody
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await AssertCommentUnchangedAsync();
    }

    private async Task<OwnedCommentSeed> CreateOwnedCommentAsync()
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

    private async Task AssertCommentUnchangedAsync()
    {
        Db.ChangeTracker.Clear();

        var persistedComment = await Db.Comments.SingleAsync();
        persistedComment.Content.Should().Be(IntegrationTestData.ValidContent);
        persistedComment.UpdatedAtUtc.Should().BeNull();
    }

    private static UpdateComment.Body CreateUpdateCommentRequest(string? content) =>
        new(content ?? string.Empty);

    private sealed record OwnedCommentSeed(
        Threads.Api.Data.Users.User Owner,
        Guid PostId,
        Guid CommentId
    );
}
