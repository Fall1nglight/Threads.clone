using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Threads.Api.IntegrationTests.Blocks.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Blocks;

public class BlockUserTests : BlockIntegrationTestBase
{
    public BlockUserTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task BlockUser_ShouldCreateBlockAndRemoveFollowsInBothDirections()
    {
        // Arrange
        var scenarioSeeder = CreateBlockRelationshipScenarioSeeder();
        var seed = await scenarioSeeder.SeedFollowCleanupAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Requester);
        var before = DateTime.UtcNow;

        // Act
        var response = await client.PostAsync(BlockTestRoutes.ForTarget(seed.Target.Id), null);
        var after = DateTime.UtcNow;

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        Db.ChangeTracker.Clear();

        var persistedBlock = await Db.UserBlocks.SingleAsync();
        persistedBlock.BlockerId.Should().Be(seed.Requester.Id);
        persistedBlock.BlockedId.Should().Be(seed.Target.Id);
        persistedBlock.CreatedAtUtc.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        (await Db.Follows.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task BlockUser_ShouldRemainIdempotentAndRemoveFollows_WhenBlockAlreadyExists()
    {
        // Arrange
        var scenarioSeeder = CreateBlockRelationshipScenarioSeeder();
        var seed = await scenarioSeeder.SeedFollowCleanupAsync(blockAlreadyExists: true);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.PostAsync(BlockTestRoutes.ForTarget(seed.Target.Id), null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        Db.ChangeTracker.Clear();

        (await Db.UserBlocks.CountAsync()).Should().Be(1);
        (await Db.Follows.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task BlockUser_ShouldReturnBadRequest_WhenTargetIsRequester()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.PostAsync(BlockTestRoutes.ForTarget(requester.Id), null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Db.UserBlocks.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task BlockUser_ShouldReturnNotFound_WhenTargetDoesNotExist()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.PostAsync(BlockTestRoutes.ForTarget(Guid.NewGuid()), null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Db.UserBlocks.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task BlockUser_ShouldReturnBadRequest_WhenTargetIdIsEmpty()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.PostAsync(BlockTestRoutes.ForTarget(Guid.Empty), null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Db.UserBlocks.CountAsync()).Should().Be(0);
    }
}
