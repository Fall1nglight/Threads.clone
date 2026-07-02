using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Posts;

public class UpdatePostTests : IntegrationTestBase
{
    public UpdatePostTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task UpdatePost_ShouldUpdatePost_WhenRequesterIsOwner()
    {
        var seeder = CreatePostSeeder();
        var alice = await seeder.CreateUserAsync("alice");
        var originalCreatedAt = new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc);
        var post = await seeder.CreatePostAsync(alice, "Original content", originalCreatedAt);
        var client = await CreateAuthenticatedClientAsync(alice);
        var before = DateTime.UtcNow;

        var response = await client.PutAsJsonAsync(
            $"/posts/{post.Id}",
            new { content = "Updated content" }
        );
        var after = DateTime.UtcNow;

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        Db.ChangeTracker.Clear();
        var persisted = await Db.Posts.SingleAsync(p => p.Id == post.Id);
        persisted.Content.Should().Be("Updated content");
        persisted.UserId.Should().Be(alice.Id);
        persisted.CreatedAtUtc.Should().Be(originalCreatedAt);
        persisted.UpdatedAtUtc.Should().NotBeNull();
        persisted.UpdatedAtUtc.Value.Should().BeOnOrAfter(before);
        persisted.UpdatedAtUtc.Value.Should().BeOnOrBefore(after);
    }

    [Fact]
    public async Task UpdatePost_ShouldForbidUpdate_WhenRequesterIsNotOwner()
    {
        var seeder = CreatePostSeeder();
        var alice = await seeder.CreateUserAsync("alice");
        var henry = await seeder.CreateUserAsync("henry");
        var post = await seeder.CreatePostAsync(alice, "Original content");
        var client = await CreateAuthenticatedClientAsync(henry);

        var response = await client.PutAsJsonAsync(
            $"/posts/{post.Id}",
            new { content = "Attempted update" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        Db.ChangeTracker.Clear();
        var persisted = await Db.Posts.SingleAsync(p => p.Id == post.Id);
        persisted.Content.Should().Be("Original content");
        persisted.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task UpdatePost_ShouldReturnNotFound_WhenPostDoesNotExist()
    {
        var seeder = CreatePostSeeder();
        var alice = await seeder.CreateUserAsync("alice");
        var client = await CreateAuthenticatedClientAsync(alice);

        var response = await client.PutAsJsonAsync(
            $"/posts/{Guid.NewGuid()}",
            new { content = "Updated content" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdatePost_ShouldReturnNotFound_WhenPostIsSoftDeleted()
    {
        var seeder = CreatePostSeeder();
        var alice = await seeder.CreateUserAsync("alice");
        var post = await seeder.CreatePostAsync(alice, "Deleted content", isDeleted: true);
        var client = await CreateAuthenticatedClientAsync(alice);

        var response = await client.PutAsJsonAsync(
            $"/posts/{post.Id}",
            new { content = "Updated content" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdatePost_ShouldReturnBadRequest_WhenIdIsEmpty()
    {
        var seeder = CreatePostSeeder();
        var alice = await seeder.CreateUserAsync("alice");
        var client = await CreateAuthenticatedClientAsync(alice);

        var response = await client.PutAsJsonAsync(
            $"/posts/{Guid.Empty}",
            new { content = "Updated content" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdatePost_ShouldAcceptContentAtMaximumLength()
    {
        var seeder = CreatePostSeeder();
        var alice = await seeder.CreateUserAsync("alice");
        var post = await seeder.CreatePostAsync(alice, "Original content");
        var client = await CreateAuthenticatedClientAsync(alice);

        var response = await client.PutAsJsonAsync(
            $"/posts/{post.Id}",
            new { content = new string('a', 600) }
        );

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task UpdatePost_ShouldReturnBadRequest_WhenContentIsEmpty(string? content)
    {
        var seeder = CreatePostSeeder();
        var alice = await seeder.CreateUserAsync("alice");
        var post = await seeder.CreatePostAsync(alice, "Original content");
        var client = await CreateAuthenticatedClientAsync(alice);

        var response = await client.PutAsJsonAsync($"/posts/{post.Id}", new { content });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        Db.ChangeTracker.Clear();
        var persisted = await Db.Posts.SingleAsync(p => p.Id == post.Id);
        persisted.Content.Should().Be("Original content");
        persisted.UpdatedAtUtc.Should().BeNull();
    }

    [Theory]
    [InlineData(601)]
    [InlineData(666)]
    [InlineData(999)]
    public async Task UpdatePost_ShouldReturnBadRequest_WhenContentIsTooLong(int length)
    {
        var seeder = CreatePostSeeder();
        var alice = await seeder.CreateUserAsync("alice");
        var post = await seeder.CreatePostAsync(alice, "Original content");
        var client = await CreateAuthenticatedClientAsync(alice);

        var response = await client.PutAsJsonAsync(
            $"/posts/{post.Id}",
            new { content = new string('a', length) }
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        Db.ChangeTracker.Clear();
        var persisted = await Db.Posts.SingleAsync(p => p.Id == post.Id);
        persisted.Content.Should().Be("Original content");
        persisted.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task UpdatePost_ShouldReturnBadRequest_WhenJsonContainsUnknownProperty()
    {
        var seeder = CreatePostSeeder();
        var alice = await seeder.CreateUserAsync("alice");
        var post = await seeder.CreatePostAsync(alice, "Original content");
        var client = await CreateAuthenticatedClientAsync(alice);

        var response = await client.PutAsJsonAsync(
            $"/posts/{post.Id}",
            new { content = "Updated content", unexpected = true }
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdatePost_ShouldReturnBadRequest_WhenJsonIsMalformed()
    {
        var seeder = CreatePostSeeder();
        var alice = await seeder.CreateUserAsync("alice");
        var post = await seeder.CreatePostAsync(alice, "Original content");
        var client = await CreateAuthenticatedClientAsync(alice);
        using var content = new StringContent(
            "{\"content\":\"Updated content\"",
            Encoding.UTF8,
            "application/json"
        );

        var response = await client.PutAsync($"/posts/{post.Id}", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
