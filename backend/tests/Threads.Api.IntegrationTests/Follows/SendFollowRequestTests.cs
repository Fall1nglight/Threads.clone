using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Data.Follows;
using Threads.Api.Features.Follows;
using Threads.Api.IntegrationTests.Follows.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Follows;

public class SendFollowRequestTests : FollowIntegrationTestBase
{
    public SendFollowRequestTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Theory]
    [InlineData(false, FollowStatus.Accepted)]
    [InlineData(true, FollowStatus.Pending)]
    public async Task SendFollowRequest_ShouldCreateRelationshipForTargetPrivacy(
        bool isPrivate,
        FollowStatus expectedStatus
    )
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var target = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.BobUsername,
            isPrivate: isPrivate
        );
        var client = await CreateAuthenticatedClientAsync(requester);
        var before = DateTime.UtcNow;

        // Act
        var response = await client.PostAsync(FollowTestRoutes.ForTarget(target.Id), null);
        var after = DateTime.UtcNow;

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<FollowStatusResponse>(response);
        responseBody.Status.Should().Be(expectedStatus);

        Db.ChangeTracker.Clear();

        var persistedFollow = await Db.Follows.SingleAsync();
        persistedFollow.FollowerId.Should().Be(requester.Id);
        persistedFollow.FollowedId.Should().Be(target.Id);
        persistedFollow.Status.Should().Be(expectedStatus);
        persistedFollow.CreatedAtUtc.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Theory]
    [InlineData(FollowStatus.Pending)]
    [InlineData(FollowStatus.Accepted)]
    public async Task SendFollowRequest_ShouldReturnStoredStatus_WhenRelationshipAlreadyExists(
        FollowStatus status
    )
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var followSeeder = CreateFollowSeeder();
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var target = await userSeeder.CreateUserAsync(username: IntegrationTestData.BobUsername);
        await followSeeder.CreateFollowAsync(requester, target, status);
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.PostAsync(FollowTestRoutes.ForTarget(target.Id), null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<FollowStatusResponse>(response);
        responseBody.Status.Should().Be(status);
        (await Db.Follows.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task SendFollowRequest_ShouldReturnBadRequest_WhenTargetIsRequester()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.PostAsync(FollowTestRoutes.ForTarget(requester.Id), null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Db.Follows.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task SendFollowRequest_ShouldReturnNotFound_WhenTargetDoesNotExist()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.PostAsync(FollowTestRoutes.ForTarget(Guid.NewGuid()), null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Db.Follows.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task SendFollowRequest_ShouldReturnBadRequest_WhenTargetIdIsEmpty()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.PostAsync(FollowTestRoutes.ForTarget(Guid.Empty), null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Db.Follows.CountAsync()).Should().Be(0);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SendFollowRequest_ShouldReturnNotFound_WhenUsersAreBlocked(
        bool requesterBlocksTarget
    )
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var blockSeeder = CreateBlockSeeder();
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var target = await userSeeder.CreateUserAsync(username: IntegrationTestData.BobUsername);
        await blockSeeder.CreateUserBlockAsync(
            requesterBlocksTarget ? requester : target,
            requesterBlocksTarget ? target : requester
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.PostAsync(FollowTestRoutes.ForTarget(target.Id), null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Db.Follows.CountAsync()).Should().Be(0);
    }
}
