using System.Net;
using FluentAssertions;
using Threads.Api.Common.Pagination;
using Threads.Api.Features.Likes;
using Threads.Api.IntegrationTests.Infrastructure;
using Threads.Api.IntegrationTests.Likes.TestSupport;

namespace Threads.Api.IntegrationTests.Likes;

public class GetPostLikesTests : LikeIntegrationTestBase
{
    public GetPostLikesTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task GetPostLikes_ShouldReturnEmptyPage_WhenPostHasNoLikes()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var postSeeder = CreatePostSeeder();
        var owner = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var post = await postSeeder.CreatePostAsync(owner, IntegrationTestData.ValidContent);
        var client = await CreateAuthenticatedClientAsync(owner);

        // Act
        var response = await client.GetAsync(LikeTestRoutes.ForPost(post.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<PostLikeUserDto>>(response);
        responseBody.Items.Should().BeEmpty();
        responseBody.Cursor.Should().BeNull();
        responseBody.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task GetPostLikes_ShouldReturnVisibleLikersWithCompleteDtoInDescendingOrder()
    {
        // Arrange
        var scenarioSeeder = CreateLikeListScenarioSeeder();
        var seed = await scenarioSeeder.SeedAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.GetAsync(LikeTestRoutes.ForPost(seed.Post.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<PostLikeUserDto>>(response);
        responseBody
            .Items.Select(item => item.Id)
            .Should()
            .Equal(seed.VisibleLiker.Id, seed.Requester.Id);

        var visibleLiker = responseBody.Items[0];
        visibleLiker.Username.Should().Be(seed.VisibleLiker.UserName);
        visibleLiker.IsPrivate.Should().Be(seed.VisibleLiker.IsPrivate);
        visibleLiker.CreatedAtUtc.Should().Be(seed.VisibleLike.CreatedAtUtc);
        responseBody.Cursor.Should().BeNull();
        responseBody.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task GetPostLikes_ShouldReturnNextPage_WhenCursorIsProvided()
    {
        // Arrange
        var scenarioSeeder = CreateLikeListScenarioSeeder();
        var seed = await scenarioSeeder.SeedBulkAsync(5);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);
        var route = LikeTestRoutes.ForPost(seed.Post.Id);

        // Act
        var firstResponse = await client.GetAsync($"{route}?pageSize=2");
        var firstPage = await ReadJsonAsync<PagedResponse<PostLikeUserDto>>(firstResponse);
        var secondResponse = await client.GetAsync(
            $"{route}?pageSize=2&cursor={Uri.EscapeDataString(firstPage.Cursor!)}"
        );

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        firstPage.Items.Select(item => item.Id).Should().Equal(seed.Users[4].Id, seed.Users[3].Id);

        var secondPage = await ReadJsonAsync<PagedResponse<PostLikeUserDto>>(secondResponse);
        secondPage.Items.Select(item => item.Id).Should().Equal(seed.Users[2].Id, seed.Users[1].Id);
    }

    [Fact]
    public async Task GetPostLikes_ShouldReturnNotFound_WhenPostDoesNotExist()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.GetAsync(LikeTestRoutes.ForPost(Guid.NewGuid()));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPostLikes_ShouldReturnNotFound_WhenPostIsNotVisible()
    {
        // Arrange
        var scenarioSeeder = CreatePostVisibilityScenarioSeeder();
        var seed = await scenarioSeeder.SeedAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Alice);

        // Act
        var pendingResponse = await client.GetAsync(
            LikeTestRoutes.ForPost(seed.ErinPrivatePendingPost.Id)
        );
        var strangerResponse = await client.GetAsync(
            LikeTestRoutes.ForPost(seed.FrankPrivateStrangerPost.Id)
        );
        var deletedResponse = await client.GetAsync(
            LikeTestRoutes.ForPost(seed.DeletedPublicPost.Id)
        );

        // Assert
        pendingResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        strangerResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        deletedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetPostLikes_ShouldReturnNotFound_WhenRequesterAndOwnerAreBlocked(
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
        var response = await client.GetAsync(LikeTestRoutes.ForPost(seed.BobPublicPost.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPostLikes_ShouldReturnBadRequest_WhenPostIdIsEmpty()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.GetAsync(LikeTestRoutes.ForPost(Guid.Empty));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetPostLikes_ShouldUseDefaultPageSize()
    {
        // Arrange
        var scenarioSeeder = CreateLikeListScenarioSeeder();
        var seed = await scenarioSeeder.SeedBulkAsync(21);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.GetAsync(LikeTestRoutes.ForPost(seed.Post.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<PostLikeUserDto>>(response);
        responseBody.Items.Should().HaveCount(20);
        responseBody.Cursor.Should().NotBeNullOrWhiteSpace();
        responseBody.HasMore.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task GetPostLikes_ShouldClampPageSizeToMinimum(int pageSize)
    {
        // Arrange
        var scenarioSeeder = CreateLikeListScenarioSeeder();
        var seed = await scenarioSeeder.SeedBulkAsync(2);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.GetAsync(
            $"{LikeTestRoutes.ForPost(seed.Post.Id)}?pageSize={pageSize}"
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<PostLikeUserDto>>(response);
        responseBody.Items.Should().ContainSingle();
        responseBody.HasMore.Should().BeTrue();
    }

    [Theory]
    [InlineData(101)]
    [InlineData(500)]
    public async Task GetPostLikes_ShouldClampPageSizeToMaximum(int pageSize)
    {
        // Arrange
        var scenarioSeeder = CreateLikeListScenarioSeeder();
        var seed = await scenarioSeeder.SeedBulkAsync(101);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.GetAsync(
            $"{LikeTestRoutes.ForPost(seed.Post.Id)}?pageSize={pageSize}"
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<PostLikeUserDto>>(response);
        responseBody.Items.Should().HaveCount(100);
        responseBody.HasMore.Should().BeTrue();
    }

    [Fact]
    public async Task GetPostLikes_ShouldTreatInvalidCursorAsFirstPage()
    {
        // Arrange
        var scenarioSeeder = CreateLikeListScenarioSeeder();
        var seed = await scenarioSeeder.SeedBulkAsync(3);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);
        var route = LikeTestRoutes.ForPost(seed.Post.Id);

        // Act
        var firstResponse = await client.GetAsync($"{route}?pageSize=2");
        var invalidCursorResponse = await client.GetAsync(
            $"{route}?pageSize=2&cursor=not-a-valid-cursor"
        );

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        invalidCursorResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var firstPage = await ReadJsonAsync<PagedResponse<PostLikeUserDto>>(firstResponse);
        var invalidCursorPage = await ReadJsonAsync<PagedResponse<PostLikeUserDto>>(
            invalidCursorResponse
        );
        invalidCursorPage
            .Items.Select(item => item.Id)
            .Should()
            .Equal(firstPage.Items.Select(item => item.Id));
    }
}
