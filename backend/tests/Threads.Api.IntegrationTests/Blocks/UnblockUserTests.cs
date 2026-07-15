using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Threads.Api.IntegrationTests.Blocks.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Blocks;

public class UnblockUserTests : BlockIntegrationTestBase
{
    public UnblockUserTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task UnblockUser_ShouldRemoveRequesterBlockOnly()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var blockSeeder = CreateBlockSeeder();
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var target = await userSeeder.CreateUserAsync(username: IntegrationTestData.BobUsername);
        await blockSeeder.CreateUserBlockAsync(requester, target);
        await blockSeeder.CreateUserBlockAsync(target, requester);
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.DeleteAsync(BlockTestRoutes.ForTarget(target.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        Db.ChangeTracker.Clear();

        var remainingBlock = await Db.UserBlocks.SingleAsync();
        remainingBlock.BlockerId.Should().Be(target.Id);
        remainingBlock.BlockedId.Should().Be(requester.Id);
    }

    [Fact]
    public async Task UnblockUser_ShouldReturnNoContent_WhenBlockDoesNotExist()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var target = await userSeeder.CreateUserAsync(username: IntegrationTestData.BobUsername);
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.DeleteAsync(BlockTestRoutes.ForTarget(target.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await Db.UserBlocks.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task UnblockUser_ShouldReturnNoContent_WhenTargetDoesNotExist()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.DeleteAsync(BlockTestRoutes.ForTarget(Guid.NewGuid()));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UnblockUser_ShouldReturnBadRequest_WhenTargetIdIsEmpty()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.DeleteAsync(BlockTestRoutes.ForTarget(Guid.Empty));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
