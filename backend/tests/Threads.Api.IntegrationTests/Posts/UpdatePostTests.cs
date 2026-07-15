using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Features.Posts.Endpoints;
using Threads.Api.IntegrationTests.Infrastructure;
using Threads.Api.IntegrationTests.Posts.TestSupport;

namespace Threads.Api.IntegrationTests.Posts;

public class UpdatePostTests : PostIntegrationTestBase
{
    public UpdatePostTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task UpdatePost_ShouldUpdatePost_WhenRequesterIsOwner()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var postSeeder = CreatePostSeeder();
        var alice = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var post = await postSeeder.CreatePostAsync(
            owner: alice,
            content: IntegrationTestData.ValidContent,
            createdAtUtc: IntegrationTestData.BaseTime
        );

        var client = await CreateAuthenticatedClientAsync(alice);
        var before = DateTime.UtcNow;
        var requestBody = CreateUpdatePostRequest(IntegrationTestData.OtherValidContent);

        // Act
        var response = await client.PutAsJsonAsync($"/posts/{post.Id}", requestBody);
        var after = DateTime.UtcNow;

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        Db.ChangeTracker.Clear();

        var persisted = await Db.Posts.SingleAsync(p => p.Id == post.Id);
        persisted.Content.Should().Be(IntegrationTestData.OtherValidContent);
        persisted.UserId.Should().Be(alice.Id);
        persisted.CreatedAtUtc.Should().Be(IntegrationTestData.BaseTime);
        persisted.UpdatedAtUtc.Should().NotBeNull();
        persisted.UpdatedAtUtc.Value.Should().BeOnOrAfter(before);
        persisted.UpdatedAtUtc.Value.Should().BeOnOrBefore(after);
    }

    [Fact]
    public async Task UpdatePost_ShouldForbidUpdate_WhenRequesterIsNotOwner()
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
        var requestBody = CreateUpdatePostRequest(IntegrationTestData.OtherValidContent);

        // Act
        var response = await client.PutAsJsonAsync($"/posts/{post.Id}", requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        Db.ChangeTracker.Clear();

        var persisted = await Db.Posts.SingleAsync(p => p.Id == post.Id);
        persisted.Content.Should().Be(IntegrationTestData.ValidContent);
        persisted.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task UpdatePost_ShouldReturnNotFound_WhenPostDoesNotExist()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var postSeeder = CreatePostSeeder();
        var alice = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var client = await CreateAuthenticatedClientAsync(alice);
        var requestBody = CreateUpdatePostRequest(IntegrationTestData.OtherValidContent);

        // Act
        var response = await client.PutAsJsonAsync($"/posts/{Guid.NewGuid()}", requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdatePost_ShouldReturnNotFound_WhenPostIsSoftDeleted()
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
        var requestBody = CreateUpdatePostRequest(IntegrationTestData.OtherValidContent);

        // Act
        var response = await client.PutAsJsonAsync($"/posts/{post.Id}", requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdatePost_ShouldReturnBadRequest_WhenPostIdIsEmpty()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var postSeeder = CreatePostSeeder();
        var alice = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var client = await CreateAuthenticatedClientAsync(alice);
        var requestBody = CreateUpdatePostRequest(IntegrationTestData.OtherValidContent);

        // Act
        var response = await client.PutAsJsonAsync($"/posts/{Guid.Empty}", requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdatePost_ShouldAcceptContentAtMaximumLength()
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
        var requestBody = CreateUpdatePostRequest(IntegrationTestData.MaxLengthContent);

        // Act
        var response = await client.PutAsJsonAsync($"/posts/{post.Id}", requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task UpdatePost_ShouldReturnBadRequest_WhenContentIsEmpty(string? content)
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
        var requestBody = CreateUpdatePostRequest(content);

        // Act
        var response = await client.PutAsJsonAsync($"/posts/{post.Id}", requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        Db.ChangeTracker.Clear();

        var persisted = await Db.Posts.SingleAsync(p => p.Id == post.Id);
        persisted.Content.Should().Be(IntegrationTestData.ValidContent);
        persisted.UpdatedAtUtc.Should().BeNull();
    }

    [Theory]
    [InlineData(601)]
    [InlineData(666)]
    [InlineData(999)]
    public async Task UpdatePost_ShouldReturnBadRequest_WhenContentIsTooLong(int length)
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
        var requestBody = CreateUpdatePostRequest(IntegrationTestData.ContentOfLength(length));

        // Act
        var response = await client.PutAsJsonAsync($"/posts/{post.Id}", requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        Db.ChangeTracker.Clear();

        var persisted = await Db.Posts.SingleAsync(p => p.Id == post.Id);
        persisted.Content.Should().Be(IntegrationTestData.ValidContent);
        persisted.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task UpdatePost_ShouldReturnBadRequest_WhenJsonContainsUnknownProperty()
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
        var requestBody = new
        {
            content = IntegrationTestData.OtherValidContent,
            unexpected = true,
        };

        // Act
        var response = await client.PutAsJsonAsync($"/posts/{post.Id}", requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdatePost_ShouldReturnBadRequest_WhenJsonIsMalformed()
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
        using var requestBody = new StringContent(
            "{\"content\":\"Valid content\"",
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await client.PutAsync($"/posts/{post.Id}", requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static UpdatePost.Body CreateUpdatePostRequest(string? content) =>
        new(content ?? string.Empty);
}
