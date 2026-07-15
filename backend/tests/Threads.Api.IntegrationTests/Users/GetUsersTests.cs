using System.Net;
using FluentAssertions;
using Threads.Api.Common.Pagination;
using Threads.Api.Features.Users;
using Threads.Api.IntegrationTests.Infrastructure;
using Threads.Api.IntegrationTests.Users.TestSupport;

namespace Threads.Api.IntegrationTests.Users;

public class GetUsersTests : UserIntegrationTestBase
{
    public GetUsersTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task GetUsers_ShouldReturnEmptyPage_WhenNoUsersExist()
    {
        // Arrange
        var client = CreateAnonymousClient();

        // Act
        var response = await client.GetAsync(UserTestRoutes.Base);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<UserProfileDto>>(response);
        responseBody.Items.Should().BeEmpty();
        responseBody.Cursor.Should().BeNull();
        responseBody.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task GetUsers_ShouldReturnPublicAndPrivateUsersOrderedByCreatedAtThenIdDescending()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var alice = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername,
            createdAtUtc: IntegrationTestData.BaseTime
        );
        var bob = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.BobUsername,
            isPrivate: true,
            createdAtUtc: IntegrationTestData.BaseTime.AddMinutes(1)
        );
        var carol = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.CarolUsername,
            createdAtUtc: IntegrationTestData.BaseTime.AddMinutes(2)
        );
        var client = CreateAnonymousClient();

        // Act
        var response = await client.GetAsync($"{UserTestRoutes.Base}?pageSize=3");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<UserProfileDto>>(response);
        responseBody.Items.Select(user => user.Id).Should().Equal(carol.Id, bob.Id, alice.Id);
        responseBody.Items.Single(user => user.Id == bob.Id).IsPrivate.Should().BeTrue();
    }

    [Fact]
    public async Task GetUsers_ShouldExcludeUsersWithBlockRelationshipWithRequester()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var blockSeeder = CreateBlockSeeder();
        var alice = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername,
            createdAtUtc: IntegrationTestData.BaseTime
        );
        var bob = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.BobUsername,
            createdAtUtc: IntegrationTestData.BaseTime.AddMinutes(1)
        );
        var carol = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.CarolUsername,
            createdAtUtc: IntegrationTestData.BaseTime.AddMinutes(2)
        );
        var dave = await userSeeder.CreateUserAsync(
            username: UserTestData.DaveUsername,
            createdAtUtc: IntegrationTestData.BaseTime.AddMinutes(3)
        );

        await blockSeeder.CreateUserBlockAsync(blocker: alice, blocked: bob);
        await blockSeeder.CreateUserBlockAsync(blocker: carol, blocked: alice);

        var client = await CreateAuthenticatedClientAsync(alice);

        // Act
        var response = await client.GetAsync($"{UserTestRoutes.Base}?pageSize=4");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<UserProfileDto>>(response);

        responseBody.Items.Select(user => user.Id).Should().Equal(dave.Id, alice.Id);
        responseBody.Items.Select(user => user.Id).Should().NotContain([bob.Id, carol.Id]);
        responseBody.Cursor.Should().BeNull();
        responseBody.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task GetUsers_ShouldReturnNextPage_WhenCursorIsProvided()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        await userSeeder.SeedBulkUsersAsync(5);
        var client = CreateAnonymousClient();

        // Act
        var firstResponse = await client.GetAsync($"{UserTestRoutes.Base}?pageSize=2");
        var firstPage = await ReadJsonAsync<PagedResponse<UserProfileDto>>(firstResponse);
        var secondResponse = await client.GetAsync(
            $"{UserTestRoutes.Base}?pageSize=2&cursor={Uri.EscapeDataString(firstPage.Cursor!)}"
        );

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondPage = await ReadJsonAsync<PagedResponse<UserProfileDto>>(secondResponse);
        secondPage.Items.Should().HaveCount(2);
        secondPage.HasMore.Should().BeTrue();
        secondPage.Cursor.Should().NotBeNullOrWhiteSpace();

        firstPage
            .Items.Select(user => user.Id)
            .Should()
            .NotIntersectWith(secondPage.Items.Select(user => user.Id));
    }
}
