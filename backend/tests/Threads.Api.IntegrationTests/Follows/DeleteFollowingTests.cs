using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Data.Follows;
using Threads.Api.IntegrationTests.Follows.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Follows;

public class DeleteFollowingTests : FollowIntegrationTestBase
{
    public DeleteFollowingTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Theory]
    [InlineData(FollowStatus.Pending)]
    [InlineData(FollowStatus.Accepted)]
    public async Task DeleteFollowing_ShouldDeleteOutgoingRelationship(FollowStatus status)
    {
        // Arrange
        var seed = await SeedRelationshipAsync(status, outgoing: true);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.DeleteAsync(FollowTestRoutes.DeleteFollowing(seed.OtherUserId));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await Db.Follows.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task DeleteFollowing_ShouldReturnNoContent_WhenRelationshipDoesNotExist()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.DeleteAsync(FollowTestRoutes.DeleteFollowing(Guid.NewGuid()));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteFollowing_ShouldKeepReversedRelationship()
    {
        // Arrange
        var seed = await SeedRelationshipAsync(FollowStatus.Accepted, outgoing: false);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.DeleteAsync(FollowTestRoutes.DeleteFollowing(seed.OtherUserId));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await Db.Follows.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task DeleteFollowing_ShouldReturnBadRequest_WhenFollowedIdIsEmpty()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.DeleteAsync(FollowTestRoutes.DeleteFollowing(Guid.Empty));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<RelationshipSeed> SeedRelationshipAsync(FollowStatus status, bool outgoing)
    {
        var userSeeder = CreateUserSeeder();
        var followSeeder = CreateFollowSeeder();
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var otherUser = await userSeeder.CreateUserAsync(username: IntegrationTestData.BobUsername);
        await followSeeder.CreateFollowAsync(
            outgoing ? requester : otherUser,
            outgoing ? otherUser : requester,
            status
        );

        return new(requester, otherUser.Id);
    }

    private sealed record RelationshipSeed(Threads.Api.Data.Users.User Requester, Guid OtherUserId);
}
