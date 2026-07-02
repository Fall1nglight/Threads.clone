using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Common.Pagination;
using Threads.Api.Features.Posts;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Posts;

public class DeletePostTests : IntegrationTestBase
{
    public DeletePostTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task DeletePost_ShouldSoftDeletePost_WhenRequesterIsOwner()
    {
        var seeder = CreatePostSeeder();
        var alice = await seeder.CreateUserAsync("alice");
        var post = await seeder.CreatePostAsync(alice, "Post to delete");
        var client = await CreateAuthenticatedClientAsync(alice);

        var response = await client.DeleteAsync($"/posts/{post.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        Db.ChangeTracker.Clear();
        var persisted = await Db.Posts.IgnoreQueryFilters().SingleAsync(p => p.Id == post.Id);
        persisted.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeletePost_ShouldHideDeletedPostFromFeedsAndSingleRead()
    {
        var seeder = CreatePostSeeder();
        var alice = await seeder.CreateUserAsync("alice");
        var post = await seeder.CreatePostAsync(alice, "Post to delete");
        var client = await CreateAuthenticatedClientAsync(alice);

        var deleteResponse = await client.DeleteAsync($"/posts/{post.Id}");
        var getResponse = await client.GetAsync($"/posts/{post.Id}");
        var feedResponse = await client.GetAsync(PostTestRoutes.GlobalFeed);

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        feedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var feed = await ReadJsonAsync<PagedResponse<PostDto>>(feedResponse);
        feed.Items.Select(item => item.Id).Should().NotContain(post.Id);
    }

    [Fact]
    public async Task DeletePost_ShouldForbidDelete_WhenRequesterIsNotOwner()
    {
        var seeder = CreatePostSeeder();
        var alice = await seeder.CreateUserAsync("alice");
        var henry = await seeder.CreateUserAsync("henry");
        var post = await seeder.CreatePostAsync(alice, "Post to keep");
        var client = await CreateAuthenticatedClientAsync(henry);

        var response = await client.DeleteAsync($"/posts/{post.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        Db.ChangeTracker.Clear();
        var persisted = await Db.Posts.SingleAsync(p => p.Id == post.Id);
        persisted.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task DeletePost_ShouldReturnNoContent_WhenPostDoesNotExist()
    {
        var seeder = CreatePostSeeder();
        var alice = await seeder.CreateUserAsync("alice");
        var client = await CreateAuthenticatedClientAsync(alice);

        var response = await client.DeleteAsync($"/posts/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeletePost_ShouldReturnNoContent_WhenPostIsAlreadyDeleted()
    {
        var seeder = CreatePostSeeder();
        var alice = await seeder.CreateUserAsync("alice");
        var post = await seeder.CreatePostAsync(alice, "Already deleted", isDeleted: true);
        var client = await CreateAuthenticatedClientAsync(alice);

        var response = await client.DeleteAsync($"/posts/{post.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        Db.ChangeTracker.Clear();
        var persisted = await Db.Posts.IgnoreQueryFilters().SingleAsync(p => p.Id == post.Id);
        persisted.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeletePost_ShouldReturnBadRequest_WhenIdIsEmpty()
    {
        var seeder = CreatePostSeeder();
        var alice = await seeder.CreateUserAsync("alice");
        var client = await CreateAuthenticatedClientAsync(alice);

        var response = await client.DeleteAsync($"/posts/{Guid.Empty}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
