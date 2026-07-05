using System.Net;
using FluentAssertions;
using Threads.Api.Common.Pagination;
using Threads.Api.Features.Posts;
using Threads.Api.IntegrationTests.Infrastructure;
using Threads.Api.IntegrationTests.Posts.TestSupport;

namespace Threads.Api.IntegrationTests.Posts;

public class PostFeedTests : PostIntegrationTestBase
{
    public PostFeedTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task GetAnonymousFeed_ShouldReturnEmptyPage_WhenNoPostsExist()
    {
        // Arrange
        var client = CreateAnonymousClient();

        // Act
        var response = await client.GetAsync(PostTestRoutes.AnonymousFeed);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<PostDto>>(response);
        responseBody.Items.Should().BeEmpty();
        responseBody.Cursor.Should().BeNull();
        responseBody.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task GetAnonymousFeed_ShouldReturnOnlyPublicActivePosts()
    {
        // Arrange
        var seeder = CreatePostSeeder();
        var seed = await seeder.SeedVisibilityGraphAsync();
        var client = CreateAnonymousClient();

        // Act
        var response = await client.GetAsync(PostTestRoutes.AnonymousFeed);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<PostDto>>(response);
        var postIds = responseBody.Items.Select(post => post.Id).ToList();

        postIds.Should().Contain(seed.AliceOwnPost.Id);
        postIds.Should().Contain(seed.BobPublicPost.Id);
        postIds.Should().Contain(seed.CarolPublicFollowedPost.Id);
        postIds.Should().NotContain(seed.AlicePrivateOwnPost.Id);
        postIds.Should().NotContain(seed.DinaPrivateFollowedPost.Id);
        postIds.Should().NotContain(seed.ErinPrivatePendingPost.Id);
        postIds.Should().NotContain(seed.FrankPrivateStrangerPost.Id);
        postIds.Should().NotContain(seed.DeletedPublicPost.Id);
    }

    [Fact]
    public async Task GetGlobalFeed_ShouldReturnPostsVisibleToAuthenticatedUser()
    {
        // Arrange
        var seeder = CreatePostSeeder();
        var seed = await seeder.SeedVisibilityGraphAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Alice);

        // Act
        var response = await client.GetAsync(PostTestRoutes.GlobalFeed);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<PostDto>>(response);
        var postIds = responseBody.Items.Select(post => post.Id).ToList();

        postIds.Should().Contain(seed.AliceOwnPost.Id);
        postIds.Should().Contain(seed.BobPublicPost.Id);
        postIds.Should().Contain(seed.CarolPublicFollowedPost.Id);
        postIds.Should().Contain(seed.DinaPrivateFollowedPost.Id);

        postIds.Should().NotContain(seed.AlicePrivateOwnPost.Id);
        postIds.Should().NotContain(seed.ErinPrivatePendingPost.Id);
        postIds.Should().NotContain(seed.FrankPrivateStrangerPost.Id);
        postIds.Should().NotContain(seed.DeletedPublicPost.Id);

        postIds.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task GetMyFeed_ShouldReturnOwnAndAcceptedFollowedPostsOnly()
    {
        // Arrange
        var seeder = CreatePostSeeder();
        var seed = await seeder.SeedVisibilityGraphAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Alice);

        // Act
        var response = await client.GetAsync(PostTestRoutes.MyFeed);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<PostDto>>(response);
        var postIds = responseBody.Items.Select(post => post.Id).ToList();

        postIds.Should().Contain(seed.AliceOwnPost.Id);
        postIds.Should().Contain(seed.CarolPublicFollowedPost.Id);
        postIds.Should().Contain(seed.DinaPrivateFollowedPost.Id);
        postIds.Should().NotContain(seed.BobPublicPost.Id);
        postIds.Should().NotContain(seed.AlicePrivateOwnPost.Id);
        postIds.Should().NotContain(seed.ErinPrivatePendingPost.Id);
        postIds.Should().NotContain(seed.FrankPrivateStrangerPost.Id);
        postIds.Should().NotContain(seed.DeletedPublicPost.Id);
    }

    [Fact]
    public async Task GetAnonymousFeed_ShouldOrderByCreatedAtThenIdDescending()
    {
        // Arrange
        var seeder = CreatePostSeeder();
        var seed = await seeder.SeedSameTimestampPostsAsync();
        var client = CreateAnonymousClient();

        // Act
        var response = await client.GetAsync(PostTestRoutes.AnonymousFeed);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<PostDto>>(response);

        responseBody
            .Items.Select(post => post.Id)
            .Take(2)
            .Should()
            .Equal(seed.HigherIdPost.Id, seed.LowerIdPost.Id);
    }

    [Fact]
    public async Task GetAnonymousFeed_ShouldUseDefaultPageSize()
    {
        // Arrange
        var seeder = CreatePostSeeder();
        await seeder.SeedBulkPublicPostsAsync(25);
        var client = CreateAnonymousClient();

        // Act
        var response = await client.GetAsync(PostTestRoutes.AnonymousFeed);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<PostDto>>(response);
        responseBody.Items.Should().HaveCount(20);
        responseBody.Cursor.Should().NotBeNullOrWhiteSpace();
        responseBody.HasMore.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task GetAnonymousFeed_ShouldClampPageSizeToMinimum(int pageSize)
    {
        // Arrange
        var seeder = CreatePostSeeder();
        await seeder.SeedBulkPublicPostsAsync(2);
        var client = CreateAnonymousClient();

        // Act
        var response = await client.GetAsync($"{PostTestRoutes.AnonymousFeed}?pageSize={pageSize}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<PostDto>>(response);
        responseBody.Items.Should().HaveCount(1);
        responseBody.Cursor.Should().NotBeNullOrWhiteSpace();
        responseBody.HasMore.Should().BeTrue();
    }

    [Theory]
    [InlineData(101)]
    [InlineData(500)]
    public async Task GetAnonymousFeed_ShouldClampPageSizeToMaximum(int pageSize)
    {
        // Arrange
        var seeder = CreatePostSeeder();
        await seeder.SeedBulkPublicPostsAsync(105);
        var client = CreateAnonymousClient();

        // Act
        var response = await client.GetAsync($"{PostTestRoutes.AnonymousFeed}?pageSize={pageSize}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<PostDto>>(response);
        responseBody.Items.Should().HaveCount(100);
        responseBody.Cursor.Should().NotBeNullOrWhiteSpace();
        responseBody.HasMore.Should().BeTrue();
    }

    [Fact]
    public async Task GetAnonymousFeed_ShouldReturnNextPage_WhenCursorIsProvided()
    {
        // Arrange
        var seeder = CreatePostSeeder();
        await seeder.SeedBulkPublicPostsAsync(25);
        var client = CreateAnonymousClient();

        // Act
        var firstResponse = await client.GetAsync($"{PostTestRoutes.AnonymousFeed}?pageSize=10");
        var firstPage = await ReadJsonAsync<PagedResponse<PostDto>>(firstResponse);
        var secondResponse = await client.GetAsync(
            $"{PostTestRoutes.AnonymousFeed}?pageSize=10&cursor={Uri.EscapeDataString(firstPage.Cursor!)}"
        );

        // Assert

        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondPage = await ReadJsonAsync<PagedResponse<PostDto>>(secondResponse);
        secondPage.Items.Should().HaveCount(10);
        secondPage.Cursor.Should().NotBeNullOrWhiteSpace();
        secondPage.HasMore.Should().BeTrue();

        firstPage
            .Items.Select(post => post.Id)
            .Should()
            .NotIntersectWith(secondPage.Items.Select(post => post.Id));
    }

    [Fact]
    public async Task GetAnonymousFeed_ShouldTreatInvalidCursorAsFirstPage()
    {
        // Arrange
        var seeder = CreatePostSeeder();
        await seeder.SeedBulkPublicPostsAsync(5);
        var client = CreateAnonymousClient();

        // Act
        var firstResponse = await client.GetAsync($"{PostTestRoutes.AnonymousFeed}?pageSize=3");
        var invalidCursorResponse = await client.GetAsync(
            $"{PostTestRoutes.AnonymousFeed}?pageSize=3&cursor=not-a-valid-cursor"
        );

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        invalidCursorResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var firstPage = await ReadJsonAsync<PagedResponse<PostDto>>(firstResponse);
        var invalidCursorPage = await ReadJsonAsync<PagedResponse<PostDto>>(invalidCursorResponse);

        invalidCursorPage
            .Items.Select(post => post.Id)
            .Should()
            .Equal(firstPage.Items.Select(post => post.Id));
    }
}
