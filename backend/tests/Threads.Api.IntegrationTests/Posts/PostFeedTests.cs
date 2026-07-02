using System.Net;
using FluentAssertions;
using Threads.Api.Common.Pagination;
using Threads.Api.Features.Posts;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Posts;

public class PostFeedTests : IntegrationTestBase
{
    public PostFeedTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task GetAnonymousFeed_ShouldReturnEmptyPage_WhenNoPostsExist()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync(PostTestRoutes.AnonymousFeed);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadJsonAsync<PagedResponse<PostDto>>(response);
        body.Items.Should().BeEmpty();
        body.Cursor.Should().BeNull();
        body.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task GetAnonymousFeed_ShouldReturnOnlyPublicActivePosts()
    {
        var seeder = CreatePostSeeder();
        var seed = await seeder.SeedVisibilityGraphAsync();
        var client = CreateAnonymousClient();

        var response = await client.GetAsync(PostTestRoutes.AnonymousFeed);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadJsonAsync<PagedResponse<PostDto>>(response);
        var ids = body.Items.Select(post => post.Id).ToList();

        ids.Should().Contain(seed.AliceOwnPost.Id);
        ids.Should().Contain(seed.BobPublicPost.Id);
        ids.Should().Contain(seed.CarolPublicFollowedPost.Id);
        ids.Should().NotContain(seed.AlicePrivateOwnPost.Id);
        ids.Should().NotContain(seed.DinaPrivateFollowedPost.Id);
        ids.Should().NotContain(seed.ErinPrivatePendingPost.Id);
        ids.Should().NotContain(seed.FrankPrivateStrangerPost.Id);
        ids.Should().NotContain(seed.DeletedPublicPost.Id);
    }

    [Fact]
    public async Task GetGlobalFeed_ShouldReturnPostsVisibleToAuthenticatedUser()
    {
        var seeder = CreatePostSeeder();
        var seed = await seeder.SeedVisibilityGraphAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Alice);

        var response = await client.GetAsync(PostTestRoutes.GlobalFeed);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadJsonAsync<PagedResponse<PostDto>>(response);
        var ids = body.Items.Select(post => post.Id).ToList();

        ids.Should().Contain(seed.AliceOwnPost.Id);
        ids.Should().Contain(seed.BobPublicPost.Id);
        ids.Should().Contain(seed.CarolPublicFollowedPost.Id);
        ids.Should().Contain(seed.DinaPrivateFollowedPost.Id);

        ids.Should().NotContain(seed.AlicePrivateOwnPost.Id);
        ids.Should().NotContain(seed.ErinPrivatePendingPost.Id);
        ids.Should().NotContain(seed.FrankPrivateStrangerPost.Id);
        ids.Should().NotContain(seed.DeletedPublicPost.Id);

        ids.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task GetMyFeed_ShouldReturnOwnAndAcceptedFollowedPostsOnly()
    {
        var seeder = CreatePostSeeder();
        var seed = await seeder.SeedVisibilityGraphAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Alice);

        var response = await client.GetAsync(PostTestRoutes.MyFeed);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadJsonAsync<PagedResponse<PostDto>>(response);
        var ids = body.Items.Select(post => post.Id).ToList();

        ids.Should().Contain(seed.AliceOwnPost.Id);
        ids.Should().Contain(seed.CarolPublicFollowedPost.Id);
        ids.Should().Contain(seed.DinaPrivateFollowedPost.Id);
        ids.Should().NotContain(seed.BobPublicPost.Id);
        ids.Should().NotContain(seed.AlicePrivateOwnPost.Id);
        ids.Should().NotContain(seed.ErinPrivatePendingPost.Id);
        ids.Should().NotContain(seed.FrankPrivateStrangerPost.Id);
        ids.Should().NotContain(seed.DeletedPublicPost.Id);
    }

    [Fact]
    public async Task GetAnonymousFeed_ShouldOrderByCreatedAtThenIdDescending()
    {
        var seeder = CreatePostSeeder();
        var seed = await seeder.SeedSameTimestampPostsAsync();
        var client = CreateAnonymousClient();

        var response = await client.GetAsync(PostTestRoutes.AnonymousFeed);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadJsonAsync<PagedResponse<PostDto>>(response);
        body.Items.Select(post => post.Id)
            .Take(2)
            .Should()
            .Equal(seed.HigherIdPost.Id, seed.LowerIdPost.Id);
    }

    [Fact]
    public async Task GetAnonymousFeed_ShouldUseDefaultPageSize()
    {
        var seeder = CreatePostSeeder();
        await seeder.SeedBulkPublicPostsAsync(25);
        var client = CreateAnonymousClient();

        var response = await client.GetAsync(PostTestRoutes.AnonymousFeed);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadJsonAsync<PagedResponse<PostDto>>(response);
        body.Items.Should().HaveCount(20);
        body.Cursor.Should().NotBeNullOrWhiteSpace();
        body.HasMore.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task GetAnonymousFeed_ShouldClampPageSizeToMinimum(int pageSize)
    {
        var seeder = CreatePostSeeder();
        await seeder.SeedBulkPublicPostsAsync(2);
        var client = CreateAnonymousClient();

        var response = await client.GetAsync($"{PostTestRoutes.AnonymousFeed}?pageSize={pageSize}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadJsonAsync<PagedResponse<PostDto>>(response);
        body.Items.Should().HaveCount(1);
        body.Cursor.Should().NotBeNullOrWhiteSpace();
        body.HasMore.Should().BeTrue();
    }

    [Theory]
    [InlineData(101)]
    [InlineData(500)]
    public async Task GetAnonymousFeed_ShouldClampPageSizeToMaximum(int pageSize)
    {
        var seeder = CreatePostSeeder();
        await seeder.SeedBulkPublicPostsAsync(105);
        var client = CreateAnonymousClient();

        var response = await client.GetAsync($"{PostTestRoutes.AnonymousFeed}?pageSize={pageSize}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadJsonAsync<PagedResponse<PostDto>>(response);
        body.Items.Should().HaveCount(100);
        body.Cursor.Should().NotBeNullOrWhiteSpace();
        body.HasMore.Should().BeTrue();
    }

    [Fact]
    public async Task GetAnonymousFeed_ShouldReturnNextPage_WhenCursorIsProvided()
    {
        var seeder = CreatePostSeeder();
        await seeder.SeedBulkPublicPostsAsync(25);
        var client = CreateAnonymousClient();

        var firstResponse = await client.GetAsync($"{PostTestRoutes.AnonymousFeed}?pageSize=10");
        var firstPage = await ReadJsonAsync<PagedResponse<PostDto>>(firstResponse);
        var secondResponse = await client.GetAsync(
            $"{PostTestRoutes.AnonymousFeed}?pageSize=10&cursor={Uri.EscapeDataString(firstPage.Cursor!)}"
        );

        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var secondPage = await ReadJsonAsync<PagedResponse<PostDto>>(secondResponse);
        firstPage
            .Items.Select(post => post.Id)
            .Should()
            .NotIntersectWith(secondPage.Items.Select(post => post.Id));
        secondPage.Items.Should().HaveCount(10);
        secondPage.Cursor.Should().NotBeNullOrWhiteSpace();
        secondPage.HasMore.Should().BeTrue();
    }

    [Fact]
    public async Task GetAnonymousFeed_ShouldTreatInvalidCursorAsFirstPage()
    {
        var seeder = CreatePostSeeder();
        await seeder.SeedBulkPublicPostsAsync(5);
        var client = CreateAnonymousClient();

        var firstResponse = await client.GetAsync($"{PostTestRoutes.AnonymousFeed}?pageSize=3");
        var invalidCursorResponse = await client.GetAsync(
            $"{PostTestRoutes.AnonymousFeed}?pageSize=3&cursor=not-a-valid-cursor"
        );

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
