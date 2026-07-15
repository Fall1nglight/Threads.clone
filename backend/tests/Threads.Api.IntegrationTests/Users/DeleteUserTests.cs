using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Threads.Api.IntegrationTests.Infrastructure;
using Threads.Api.IntegrationTests.Users.TestSupport;

namespace Threads.Api.IntegrationTests.Users;

public class DeleteUserTests : UserIntegrationTestBase
{
    public DeleteUserTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task DeleteUser_ShouldReturnUnauthorized_WhenRequesterIsAnonymous()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var alice = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var client = CreateAnonymousClient();

        // Act
        var response = await client.DeleteAsync($"{UserTestRoutes.Base}/{alice.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteUser_ShouldForbidDelete_WhenRequesterTargetsAnotherUser()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var alice = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var bob = await userSeeder.CreateUserAsync(username: IntegrationTestData.BobUsername);
        var client = await CreateAuthenticatedClientAsync(alice);

        // Act
        var response = await client.DeleteAsync($"{UserTestRoutes.Base}/{bob.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        Db.ChangeTracker.Clear();

        var persisted = await Db.Users.SingleAsync(user => user.Id == bob.Id);
        persisted.UserName.Should().Be(bob.UserName);
    }

    [Fact]
    public async Task DeleteUser_ShouldReturnBadRequest_WhenIdIsEmpty()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var alice = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var client = await CreateAuthenticatedClientAsync(alice);

        // Act
        var response = await client.DeleteAsync($"{UserTestRoutes.Base}/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteUser_ShouldHardDeleteCurrentUserAndDependentData()
    {
        // Arrange
        var scenarioSeeder = CreateUserDeletionScenarioSeeder();
        var seed = await scenarioSeeder.SeedAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Alice);

        // Act
        var response = await client.DeleteAsync($"{UserTestRoutes.Base}/{seed.Alice.Id}");
        var getResponse = await client.GetAsync($"{UserTestRoutes.Base}/{seed.Alice.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        Db.ChangeTracker.Clear();

        (await Db.Users.AnyAsync(user => user.Id == seed.Alice.Id)).Should().BeFalse();
        (await Db.Users.AnyAsync(user => user.Id == seed.Bob.Id)).Should().BeTrue();

        (await Db.Posts.IgnoreQueryFilters().AnyAsync(post => post.Id == seed.AlicePost.Id))
            .Should()
            .BeFalse();
        (await Db.Posts.AnyAsync(post => post.Id == seed.BobPost.Id)).Should().BeTrue();

        (
            await Db.Follows.AnyAsync(follow =>
                follow.FollowerId == seed.Alice.Id || follow.FollowedId == seed.Alice.Id
            )
        )
            .Should()
            .BeFalse();

        (
            await Db.UserBlocks.AnyAsync(block =>
                block.BlockerId == seed.Alice.Id || block.BlockedId == seed.Alice.Id
            )
        )
            .Should()
            .BeFalse();

        (
            await Db.UserBlocks.AnyAsync(block =>
                block.BlockerId == seed.Bob.Id && block.BlockedId == seed.Carol.Id
            )
        )
            .Should()
            .BeTrue();

        (await Db.PostLikes.AnyAsync(like => like.UserId == seed.Alice.Id)).Should().BeFalse();
        (await Db.PostLikes.AnyAsync(like => like.PostId == seed.AlicePost.Id)).Should().BeFalse();

        (await Db.Comments.AnyAsync(comment => comment.Id == seed.AliceComment.Id))
            .Should()
            .BeFalse();
        (await Db.Comments.AnyAsync(comment => comment.Id == seed.BobComment.Id))
            .Should()
            .BeFalse();

        (
            await Db.RefreshTokens.AnyAsync(token =>
                token.Id == seed.TokenChain.ReplacedToken.Id
                || token.Id == seed.TokenChain.ReplacementToken.Id
            )
        )
            .Should()
            .BeFalse();
    }

    [Fact]
    public async Task DeleteUser_ShouldReturnNoContent_WhenCurrentUserNoLongerExists()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var alice = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var client = await CreateAuthenticatedClientAsync(alice);

        Db.Users.Remove(alice);
        await Db.SaveChangesAsync();

        // Act
        var response = await client.DeleteAsync($"{UserTestRoutes.Base}/{alice.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
