using System.Net;
using System.Text.Json;
using FluentAssertions;
using Threads.Api.Features.Users;
using Threads.Api.IntegrationTests.Infrastructure;
using Threads.Api.IntegrationTests.Users.TestSupport;

namespace Threads.Api.IntegrationTests.Users;

public class GetUserTests : UserIntegrationTestBase
{
    public GetUserTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task GetUser_ShouldReturnUserProfile_WhenUserExists()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var alice = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername,
            isPrivate: true,
            bio: IntegrationTestData.AliceBio,
            createdAtUtc: IntegrationTestData.BaseTime,
            updatedAtUtc: IntegrationTestData.BaseTime.AddMinutes(1)
        );
        var client = CreateAnonymousClient();

        // Act
        var response = await client.GetAsync($"{UserTestRoutes.Base}/{alice.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);

        document
            .RootElement.EnumerateObject()
            .Select(property => property.Name)
            .Should()
            .BeEquivalentTo("id", "username", "isPrivate", "createdAtUtc", "updatedAtUtc", "bio");

        var responseBody = JsonSerializer.Deserialize<UserProfileDto>(json, JsonOptions);
        responseBody.Should().NotBeNull();
        responseBody!.Id.Should().Be(alice.Id);
        responseBody.Username.Should().Be(alice.UserName);
        responseBody.IsPrivate.Should().BeTrue();
        responseBody.Bio.Should().Be(alice.Bio);
        responseBody.CreatedAtUtc.Should().Be(alice.CreatedAtUtc);
        responseBody.UpdatedAtUtc.Should().Be(alice.UpdatedAtUtc);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetUser_ShouldReturnNotFound_WhenRequesterAndTargetUserHaveBlockRelationship(
        bool requesterBlocksTarget
    )
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var blockSeeder = CreateBlockSeeder();
        var alice = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var bob = await userSeeder.CreateUserAsync(username: IntegrationTestData.BobUsername);

        await blockSeeder.CreateUserBlockAsync(
            blocker: requesterBlocksTarget ? alice : bob,
            blocked: requesterBlocksTarget ? bob : alice
        );

        var client = await CreateAuthenticatedClientAsync(alice);

        // Act
        var response = await client.GetAsync($"{UserTestRoutes.Base}/{bob.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var client = CreateAnonymousClient();

        // Act
        var response = await client.GetAsync($"{UserTestRoutes.Base}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUser_ShouldReturnBadRequest_WhenIdIsEmpty()
    {
        // Arrange
        var client = CreateAnonymousClient();

        // Act
        var response = await client.GetAsync($"{UserTestRoutes.Base}/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
