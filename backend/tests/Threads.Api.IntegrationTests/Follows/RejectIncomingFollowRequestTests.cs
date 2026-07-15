using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Data.Follows;
using Threads.Api.Data.Users;
using Threads.Api.IntegrationTests.Follows.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Follows;

public class RejectIncomingFollowRequestTests : FollowIntegrationTestBase
{
    public RejectIncomingFollowRequestTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task RejectIncomingFollowRequest_ShouldDeletePendingRelationship()
    {
        // Arrange
        var seed = await CreateRelationshipAsync(FollowStatus.Pending);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.PostAsync(
            FollowTestRoutes.RejectIncoming(seed.OtherUser.Id),
            null
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        Db.ChangeTracker.Clear();

        (await Db.Follows.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task RejectIncomingFollowRequest_ShouldReturnNotFound_WhenRelationshipDoesNotExist()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var follower = await userSeeder.CreateUserAsync(username: IntegrationTestData.BobUsername);
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.PostAsync(FollowTestRoutes.RejectIncoming(follower.Id), null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RejectIncomingFollowRequest_ShouldReturnNotFound_WhenRelationshipIsReversed()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var followSeeder = CreateFollowSeeder();
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var otherUser = await userSeeder.CreateUserAsync(username: IntegrationTestData.BobUsername);
        await followSeeder.CreateFollowAsync(requester, otherUser, FollowStatus.Pending);
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.PostAsync(FollowTestRoutes.RejectIncoming(otherUser.Id), null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Db.Follows.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task RejectIncomingFollowRequest_ShouldReturnNotFound_WhenRelationshipIsAccepted()
    {
        // Arrange
        var seed = await CreateRelationshipAsync(FollowStatus.Accepted);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.PostAsync(
            FollowTestRoutes.RejectIncoming(seed.OtherUser.Id),
            null
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Db.Follows.CountAsync()).Should().Be(1);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RejectIncomingFollowRequest_ShouldReturnNotFound_WhenUsersAreBlocked(
        bool requesterBlocksFollower
    )
    {
        // Arrange
        var seed = await CreateRelationshipAsync(FollowStatus.Pending);
        var blockSeeder = CreateBlockSeeder();
        await blockSeeder.CreateUserBlockAsync(
            requesterBlocksFollower ? seed.Requester : seed.OtherUser,
            requesterBlocksFollower ? seed.OtherUser : seed.Requester
        );
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.PostAsync(
            FollowTestRoutes.RejectIncoming(seed.OtherUser.Id),
            null
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Db.Follows.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task RejectIncomingFollowRequest_ShouldReturnBadRequest_WhenFollowerIdIsEmpty()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.PostAsync(FollowTestRoutes.RejectIncoming(Guid.Empty), null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<IncomingRelationshipSeed> CreateRelationshipAsync(FollowStatus status)
    {
        var userSeeder = CreateUserSeeder();
        var followSeeder = CreateFollowSeeder();
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var otherUser = await userSeeder.CreateUserAsync(username: IntegrationTestData.BobUsername);
        await followSeeder.CreateFollowAsync(otherUser, requester, status);

        return new(requester, otherUser);
    }

    private sealed record IncomingRelationshipSeed(User Requester, User OtherUser);
}
