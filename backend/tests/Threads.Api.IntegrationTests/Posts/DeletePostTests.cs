using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Common.Pagination;
using Threads.Api.Features.Posts;
using Threads.Api.IntegrationTests.Infrastructure;
using Threads.Api.IntegrationTests.Posts.TestSupport;

namespace Threads.Api.IntegrationTests.Posts;

public class DeletePostTests : PostIntegrationTestBase
{
    public DeletePostTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task DeletePost_ShouldSoftDeletePost_WhenRequesterIsOwner()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var postSeeder = CreatePostSeeder();
        var alice = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var post = await postSeeder.CreatePostAsync(
            owner: alice,
            content: IntegrationTestData.ValidContent
        );
        var client = await CreateAuthenticatedClientAsync(alice);

        // Act
        var response = await client.DeleteAsync($"/posts/{post.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        Db.ChangeTracker.Clear();

        var persisted = await Db.Posts.IgnoreQueryFilters().SingleAsync(p => p.Id == post.Id);
        persisted.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeletePost_ShouldHideDeletedPostFromFeedsAndSingleRead()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var postSeeder = CreatePostSeeder();
        var alice = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var post = await postSeeder.CreatePostAsync(
            owner: alice,
            content: IntegrationTestData.ValidContent
        );
        var client = await CreateAuthenticatedClientAsync(alice);

        // Act
        var deleteResponse = await client.DeleteAsync($"/posts/{post.Id}");
        var getResponse = await client.GetAsync($"/posts/{post.Id}");
        var feedResponse = await client.GetAsync(PostTestRoutes.GlobalFeed);

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        feedResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var feed = await ReadJsonAsync<PagedResponse<PostDto>>(feedResponse);
        feed.Items.Select(item => item.Id).Should().NotContain(post.Id);
    }

    [Fact]
    public async Task DeletePost_ShouldForbidDelete_WhenRequesterIsNotOwner()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var postSeeder = CreatePostSeeder();
        var alice = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var henry = await userSeeder.CreateUserAsync(username: IntegrationTestData.HenryUsername);
        var post = await postSeeder.CreatePostAsync(
            owner: alice,
            content: IntegrationTestData.ValidContent
        );
        var client = await CreateAuthenticatedClientAsync(henry);

        // Act
        var response = await client.DeleteAsync($"/posts/{post.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        Db.ChangeTracker.Clear();

        var persisted = await Db.Posts.SingleAsync(p => p.Id == post.Id);
        persisted.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task DeletePost_ShouldReturnNoContent_WhenPostDoesNotExist()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var postSeeder = CreatePostSeeder();
        var alice = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var client = await CreateAuthenticatedClientAsync(alice);

        // Act
        var response = await client.DeleteAsync($"/posts/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeletePost_ShouldReturnNoContent_WhenPostIsAlreadyDeleted()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var postSeeder = CreatePostSeeder();
        var alice = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var post = await postSeeder.CreatePostAsync(
            owner: alice,
            content: IntegrationTestData.ValidContent,
            isDeleted: true
        );
        var client = await CreateAuthenticatedClientAsync(alice);

        // Act
        var response = await client.DeleteAsync($"/posts/{post.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        Db.ChangeTracker.Clear();

        var persisted = await Db.Posts.IgnoreQueryFilters().SingleAsync(p => p.Id == post.Id);
        persisted.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeletePost_ShouldReturnBadRequest_WhenPostIdIsEmpty()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var postSeeder = CreatePostSeeder();
        var alice = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var client = await CreateAuthenticatedClientAsync(alice);

        // Act
        var response = await client.DeleteAsync($"/posts/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
