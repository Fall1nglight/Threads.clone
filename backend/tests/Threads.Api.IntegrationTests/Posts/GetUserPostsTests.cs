using System.Net;
using FluentAssertions;
using Threads.Api.Common.Pagination;
using Threads.Api.Features.Posts;
using Threads.Api.IntegrationTests.Infrastructure;
using Threads.Api.IntegrationTests.Posts.TestSupport;

namespace Threads.Api.IntegrationTests.Posts;

public class GetUserPostsTests : IntegrationTestBase
{
    public GetUserPostsTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task GetUserPosts_ShouldReturnOnlyTargetUsersActivePostsInDescendingOrder_WhenAnonymous()
    {
        // Arrange
        var seed = await Scenarios.PostPagination.SeedSameTimestampPostsAsync();
        var otherUser = await Seeders.Users.CreateUserAsync(
            username: IntegrationTestData.BobUsername
        );
        var otherPost = await Seeders.Posts.CreatePostAsync(
            otherUser,
            IntegrationTestData.OtherValidContent,
            IntegrationTestData.BaseTime.AddHours(2)
        );
        var deletedPost = await Seeders.Posts.CreatePostAsync(
            seed.User,
            IntegrationTestData.ValidContent,
            IntegrationTestData.BaseTime.AddHours(3),
            isDeleted: true
        );
        var newerPost = await Seeders.Posts.CreatePostAsync(
            seed.User,
            "Newest active post",
            IntegrationTestData.BaseTime.AddHours(4)
        );
        var client = CreateAnonymousClient();

        // Act
        var response = await client.GetAsync(PostTestRoutes.UserPosts(seed.User.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<PostDto>>(response);
        responseBody
            .Items.Select(post => post.Id)
            .Should()
            .Equal(newerPost.Id, seed.HigherIdPost.Id, seed.LowerIdPost.Id);
        responseBody.Items.Should().OnlyContain(post => post.User.Id == seed.User.Id);
        responseBody.Items.Should().NotContain(post => post.Id == otherPost.Id);
        responseBody.Items.Should().NotContain(post => post.Id == deletedPost.Id);
        responseBody.Items.Should().OnlyContain(post => !post.IsLikedByCurrentUser);
    }

    [Fact]
    public async Task GetUserPosts_ShouldReturnEmptyPage_WhenRequesterIsAnonymousAndUserIsPrivate()
    {
        // Arrange
        var seed = await Scenarios.PostVisibility.SeedAsync();
        var client = CreateAnonymousClient();

        // Act
        var response = await client.GetAsync(PostTestRoutes.UserPosts(seed.DinaPrivateFollowed.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await ReadJsonAsync<PagedResponse<PostDto>>(response);
        responseBody.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserPosts_ShouldReturnPrivatePosts_WhenRequesterIsOwnerOrAcceptedFollower()
    {
        // Arrange
        var seed = await Scenarios.PostVisibility.SeedAsync();
        var ownerClient = await CreateAuthenticatedClientAsync(seed.AlicePrivate);
        var followerClient = await CreateAuthenticatedClientAsync(seed.Alice);

        // Act
        var ownerResponse = await ownerClient.GetAsync(
            PostTestRoutes.UserPosts(seed.AlicePrivate.Id)
        );
        var followerResponse = await followerClient.GetAsync(
            PostTestRoutes.UserPosts(seed.DinaPrivateFollowed.Id)
        );

        // Assert
        ownerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        followerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var ownerPage = await ReadJsonAsync<PagedResponse<PostDto>>(ownerResponse);
        ownerPage.Items.Should().ContainSingle().Which.Id.Should().Be(seed.AlicePrivateOwnPost.Id);

        var followerPage = await ReadJsonAsync<PagedResponse<PostDto>>(followerResponse);
        followerPage
            .Items.Should()
            .ContainSingle()
            .Which.Id.Should()
            .Be(seed.DinaPrivateFollowedPost.Id);
    }

    [Fact]
    public async Task GetUserPosts_ShouldReturnEmptyPage_WhenRequesterCannotViewPrivateUser()
    {
        // Arrange
        var seed = await Scenarios.PostVisibility.SeedAsync();
        var pendingFollowerClient = await CreateAuthenticatedClientAsync(seed.Alice);
        var strangerClient = await CreateAuthenticatedClientAsync(seed.Henry);

        // Act
        var pendingResponse = await pendingFollowerClient.GetAsync(
            PostTestRoutes.UserPosts(seed.ErinPrivatePending.Id)
        );
        var strangerResponse = await strangerClient.GetAsync(
            PostTestRoutes.UserPosts(seed.DinaPrivateFollowed.Id)
        );

        // Assert
        pendingResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        strangerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var pendingPage = await ReadJsonAsync<PagedResponse<PostDto>>(pendingResponse);
        var strangerPage = await ReadJsonAsync<PagedResponse<PostDto>>(strangerResponse);
        pendingPage.Items.Should().BeEmpty();
        strangerPage.Items.Should().BeEmpty();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetUserPosts_ShouldReturnEmptyPage_WhenRequesterAndOwnerAreBlocked(
        bool requesterBlocksOwner
    )
    {
        // Arrange
        var seed = await Scenarios.PostVisibility.SeedAsync();
        await Seeders.Blocks.CreateUserBlockAsync(
            requesterBlocksOwner ? seed.Alice : seed.BobPublic,
            requesterBlocksOwner ? seed.BobPublic : seed.Alice
        );
        var client = await CreateAuthenticatedClientAsync(seed.Alice);

        // Act
        var response = await client.GetAsync(PostTestRoutes.UserPosts(seed.BobPublic.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await ReadJsonAsync<PagedResponse<PostDto>>(response);
        responseBody.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserPosts_ShouldReturnNextPage_WhenCursorIsProvided()
    {
        // Arrange
        var seed = await Scenarios.PostPagination.SeedBulkPublicPostsAsync(5);
        var route = PostTestRoutes.UserPosts(seed.User.Id);
        var client = CreateAnonymousClient();

        // Act
        var firstResponse = await client.GetAsync($"{route}?pageSize=2");
        var firstPage = await ReadJsonAsync<PagedResponse<PostDto>>(firstResponse);
        var secondResponse = await client.GetAsync(
            $"{route}?pageSize=2&cursor={Uri.EscapeDataString(firstPage.Cursor!)}"
        );

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        firstPage.Items.Should().HaveCount(2);

        var secondPage = await ReadJsonAsync<PagedResponse<PostDto>>(secondResponse);
        secondPage.Items.Should().HaveCount(2);
        firstPage
            .Items.Select(post => post.Id)
            .Should()
            .NotIntersectWith(secondPage.Items.Select(post => post.Id));
    }

    [Fact]
    public async Task GetUserPosts_ShouldReturnEmptyPage_WhenUserDoesNotExist()
    {
        // Arrange
        var client = CreateAnonymousClient();

        // Act
        var response = await client.GetAsync(PostTestRoutes.UserPosts(Guid.NewGuid()));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await ReadJsonAsync<PagedResponse<PostDto>>(response);
        responseBody.Items.Should().BeEmpty();
        responseBody.Cursor.Should().BeNull();
        responseBody.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserPosts_ShouldReturnLikeStateForAuthenticatedRequesterOnly()
    {
        // Arrange
        var seed = await Scenarios.PostVisibility.SeedAsync();
        await Seeders.Likes.CreatePostLikeAsync(seed.Alice, seed.BobPublicPost);
        var anonymousClient = CreateAnonymousClient();
        var authenticatedClient = await CreateAuthenticatedClientAsync(seed.Alice);
        var route = PostTestRoutes.UserPosts(seed.BobPublic.Id);

        // Act
        var anonymousResponse = await anonymousClient.GetAsync(route);
        var authenticatedResponse = await authenticatedClient.GetAsync(route);

        // Assert
        var anonymousPage = await ReadJsonAsync<PagedResponse<PostDto>>(anonymousResponse);
        var authenticatedPage = await ReadJsonAsync<PagedResponse<PostDto>>(authenticatedResponse);

        anonymousPage
            .Items.Single(post => post.Id == seed.BobPublicPost.Id)
            .IsLikedByCurrentUser.Should()
            .BeFalse();
        authenticatedPage
            .Items.Single(post => post.Id == seed.BobPublicPost.Id)
            .IsLikedByCurrentUser.Should()
            .BeTrue();
    }

    [Fact]
    public async Task GetUserPosts_ShouldReturnBadRequest_WhenUserIdIsEmpty()
    {
        // Arrange
        var client = CreateAnonymousClient();

        // Act
        var response = await client.GetAsync(PostTestRoutes.UserPosts(Guid.Empty));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
