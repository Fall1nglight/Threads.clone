using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Features.Comments;
using Threads.Api.Features.Comments.Endpoints;
using Threads.Api.IntegrationTests.Comments.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Comments;

public class CreateCommentTests : CommentIntegrationTestBase
{
    public CreateCommentTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task CreateComment_ShouldCreateCommentAndReturnDto_WhenPostIsPublic()
    {
        // Arrange
        var scenarioSeeder = CreatePostVisibilityScenarioSeeder();
        var seed = await scenarioSeeder.SeedAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Alice);
        var requestBody = CreateCommentRequest(IntegrationTestData.ValidContent);
        var before = DateTime.UtcNow;

        // Act
        var response = await client.PostAsJsonAsync(
            CommentTestRoutes.ForPost(seed.BobPublicPost.Id),
            requestBody
        );
        var after = DateTime.UtcNow;

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseBody = await ReadJsonAsync<CommentDto>(response);
        response.Headers.Location.Should().NotBeNull();
        response
            .Headers.Location!.ToString()
            .Should()
            .Be(CommentTestRoutes.ForComment(seed.BobPublicPost.Id, responseBody.Id));
        responseBody.PostId.Should().Be(seed.BobPublicPost.Id);
        responseBody.User.Id.Should().Be(seed.Alice.Id);
        responseBody.User.Username.Should().Be(seed.Alice.UserName);
        responseBody.Content.Should().Be(IntegrationTestData.ValidContent);
        responseBody.CreatedAtUtc.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        responseBody.UpdatedAtUtc.Should().BeNull();

        Db.ChangeTracker.Clear();

        var persistedComment = await Db.Comments.SingleAsync();
        persistedComment.Id.Should().Be(responseBody.Id);
        persistedComment.PostId.Should().Be(seed.BobPublicPost.Id);
        persistedComment.UserId.Should().Be(seed.Alice.Id);
        persistedComment.Content.Should().Be(IntegrationTestData.ValidContent);
    }

    [Fact]
    public async Task CreateComment_ShouldCreateComment_WhenRequesterOwnsPrivatePost()
    {
        // Arrange
        var scenarioSeeder = CreatePostVisibilityScenarioSeeder();
        var seed = await scenarioSeeder.SeedAsync();
        var client = await CreateAuthenticatedClientAsync(seed.AlicePrivate);
        var requestBody = CreateCommentRequest(IntegrationTestData.ValidContent);

        // Act
        var response = await client.PostAsJsonAsync(
            CommentTestRoutes.ForPost(seed.AlicePrivateOwnPost.Id),
            requestBody
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        (await Db.Comments.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task CreateComment_ShouldCreateComment_WhenPrivateOwnerIsAcceptedFollowed()
    {
        // Arrange
        var scenarioSeeder = CreatePostVisibilityScenarioSeeder();
        var seed = await scenarioSeeder.SeedAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Alice);
        var requestBody = CreateCommentRequest(IntegrationTestData.ValidContent);

        // Act
        var response = await client.PostAsJsonAsync(
            CommentTestRoutes.ForPost(seed.DinaPrivateFollowedPost.Id),
            requestBody
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        (await Db.Comments.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task CreateComment_ShouldAcceptContentAtMaximumLength()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var postSeeder = CreatePostSeeder();
        var user = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var post = await postSeeder.CreatePostAsync(user, IntegrationTestData.ValidContent);
        var client = await CreateAuthenticatedClientAsync(user);
        var requestBody = CreateCommentRequest(IntegrationTestData.MaxLengthContent);

        // Act
        var response = await client.PostAsJsonAsync(
            CommentTestRoutes.ForPost(post.Id),
            requestBody
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task CreateComment_ShouldReturnBadRequest_WhenContentIsEmpty(string? content)
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var postSeeder = CreatePostSeeder();
        var user = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var post = await postSeeder.CreatePostAsync(user, IntegrationTestData.ValidContent);
        var client = await CreateAuthenticatedClientAsync(user);
        var requestBody = CreateCommentRequest(content);

        // Act
        var response = await client.PostAsJsonAsync(
            CommentTestRoutes.ForPost(post.Id),
            requestBody
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Db.Comments.CountAsync()).Should().Be(0);
    }

    [Theory]
    [InlineData(601)]
    [InlineData(999)]
    public async Task CreateComment_ShouldReturnBadRequest_WhenContentIsTooLong(int length)
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var postSeeder = CreatePostSeeder();
        var user = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var post = await postSeeder.CreatePostAsync(user, IntegrationTestData.ValidContent);
        var client = await CreateAuthenticatedClientAsync(user);
        var requestBody = CreateCommentRequest(IntegrationTestData.ContentOfLength(length));

        // Act
        var response = await client.PostAsJsonAsync(
            CommentTestRoutes.ForPost(post.Id),
            requestBody
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Db.Comments.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task CreateComment_ShouldReturnBadRequest_WhenPostIdIsEmpty()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var user = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var client = await CreateAuthenticatedClientAsync(user);
        var requestBody = CreateCommentRequest(IntegrationTestData.ValidContent);

        // Act
        var response = await client.PostAsJsonAsync(
            CommentTestRoutes.ForPost(Guid.Empty),
            requestBody
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Db.Comments.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task CreateComment_ShouldReturnNotFound_WhenPostDoesNotExist()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var user = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var client = await CreateAuthenticatedClientAsync(user);
        var requestBody = CreateCommentRequest(IntegrationTestData.ValidContent);

        // Act
        var response = await client.PostAsJsonAsync(
            CommentTestRoutes.ForPost(Guid.NewGuid()),
            requestBody
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Db.Comments.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task CreateComment_ShouldReturnNotFound_WhenPostIsNotVisible()
    {
        // Arrange
        var scenarioSeeder = CreatePostVisibilityScenarioSeeder();
        var seed = await scenarioSeeder.SeedAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Alice);
        var requestBody = CreateCommentRequest(IntegrationTestData.ValidContent);

        // Act
        var pendingResponse = await client.PostAsJsonAsync(
            CommentTestRoutes.ForPost(seed.ErinPrivatePendingPost.Id),
            requestBody
        );
        var strangerResponse = await client.PostAsJsonAsync(
            CommentTestRoutes.ForPost(seed.FrankPrivateStrangerPost.Id),
            requestBody
        );
        var deletedResponse = await client.PostAsJsonAsync(
            CommentTestRoutes.ForPost(seed.DeletedPublicPost.Id),
            requestBody
        );

        // Assert
        pendingResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        strangerResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        deletedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Db.Comments.CountAsync()).Should().Be(0);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateComment_ShouldReturnNotFound_WhenRequesterAndOwnerAreBlocked(
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
        var requestBody = CreateCommentRequest(IntegrationTestData.ValidContent);

        // Act
        var response = await client.PostAsJsonAsync(
            CommentTestRoutes.ForPost(seed.BobPublicPost.Id),
            requestBody
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Db.Comments.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task CreateComment_ShouldReturnBadRequest_WhenContentIsMissing()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var postSeeder = CreatePostSeeder();
        var user = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var post = await postSeeder.CreatePostAsync(user, IntegrationTestData.ValidContent);
        var client = await CreateAuthenticatedClientAsync(user);
        var requestBody = new { };

        // Act
        var response = await client.PostAsJsonAsync(
            CommentTestRoutes.ForPost(post.Id),
            requestBody
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Db.Comments.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task CreateComment_ShouldReturnBadRequest_WhenJsonContainsUnknownProperty()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var postSeeder = CreatePostSeeder();
        var user = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var post = await postSeeder.CreatePostAsync(user, IntegrationTestData.ValidContent);
        var client = await CreateAuthenticatedClientAsync(user);
        var requestBody = new { content = IntegrationTestData.ValidContent, unexpected = true };

        // Act
        var response = await client.PostAsJsonAsync(
            CommentTestRoutes.ForPost(post.Id),
            requestBody
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Db.Comments.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task CreateComment_ShouldReturnBadRequest_WhenJsonIsMalformed()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var postSeeder = CreatePostSeeder();
        var user = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var post = await postSeeder.CreatePostAsync(user, IntegrationTestData.ValidContent);
        var client = await CreateAuthenticatedClientAsync(user);
        using var requestBody = new StringContent(
            "{\"content\":\"Valid content\"",
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await client.PostAsync(CommentTestRoutes.ForPost(post.Id), requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Db.Comments.CountAsync()).Should().Be(0);
    }

    private static CreateComment.Body CreateCommentRequest(string? content) =>
        new(content ?? string.Empty);
}
