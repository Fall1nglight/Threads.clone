using System.Net;
using FluentAssertions;
using Threads.Api.Common.Pagination;
using Threads.Api.Features.Blocks;
using Threads.Api.IntegrationTests.Blocks.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Blocks;

public class GetBlockedUsersTests : BlockIntegrationTestBase
{
    public GetBlockedUsersTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task GetBlockedUsers_ShouldReturnEmptyPage_WhenRequesterHasNoBlocks()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.GetAsync(BlockTestRoutes.Base);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<BlockedUserDto>>(response);
        responseBody.Items.Should().BeEmpty();
        responseBody.Cursor.Should().BeNull();
        responseBody.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task GetBlockedUsers_ShouldReturnRequesterBlocksWithCompleteDtoInDescendingOrder()
    {
        // Arrange
        var scenarioSeeder = CreateBlockRelationshipScenarioSeeder();
        var userSeeder = CreateUserSeeder();
        var blockSeeder = CreateBlockSeeder();
        var seed = await scenarioSeeder.SeedBlockedUsersAsync(2);
        var reverseBlocker = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.HenryUsername
        );
        await blockSeeder.CreateUserBlockAsync(reverseBlocker, seed.Requester);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.GetAsync(BlockTestRoutes.Base);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<BlockedUserDto>>(response);
        responseBody
            .Items.Select(item => item.Id)
            .Should()
            .Equal(seed.Users[1].Id, seed.Users[0].Id);
        responseBody.Items.Should().NotContain(item => item.Id == reverseBlocker.Id);

        var newestItem = responseBody.Items[0];
        newestItem.Username.Should().Be(seed.Users[1].UserName);
        newestItem.IsPrivate.Should().Be(seed.Users[1].IsPrivate);
        newestItem.Bio.Should().Be(seed.Users[1].Bio);
        newestItem.CreatedAtUtc.Should().Be(seed.Blocks[1].CreatedAtUtc);
        responseBody.Cursor.Should().BeNull();
        responseBody.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task GetBlockedUsers_ShouldReturnNextPage_WhenCursorIsProvided()
    {
        // Arrange
        var scenarioSeeder = CreateBlockRelationshipScenarioSeeder();
        var seed = await scenarioSeeder.SeedBlockedUsersAsync(5);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var firstResponse = await client.GetAsync($"{BlockTestRoutes.Base}?pageSize=2");
        var firstPage = await ReadJsonAsync<PagedResponse<BlockedUserDto>>(firstResponse);
        var secondResponse = await client.GetAsync(
            $"{BlockTestRoutes.Base}?pageSize=2&cursor={Uri.EscapeDataString(firstPage.Cursor!)}"
        );

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondPage = await ReadJsonAsync<PagedResponse<BlockedUserDto>>(secondResponse);
        firstPage.Items.Should().HaveCount(2);
        secondPage.Items.Should().HaveCount(2);
        firstPage
            .Items.Select(item => item.Id)
            .Should()
            .NotIntersectWith(secondPage.Items.Select(item => item.Id));
    }

    [Fact]
    public async Task GetBlockedUsers_ShouldUseDefaultPageSize()
    {
        // Arrange
        var scenarioSeeder = CreateBlockRelationshipScenarioSeeder();
        var seed = await scenarioSeeder.SeedBlockedUsersAsync(21);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.GetAsync(BlockTestRoutes.Base);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<BlockedUserDto>>(response);
        responseBody.Items.Should().HaveCount(20);
        responseBody.Cursor.Should().NotBeNullOrWhiteSpace();
        responseBody.HasMore.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task GetBlockedUsers_ShouldClampPageSizeToMinimum(int pageSize)
    {
        // Arrange
        var scenarioSeeder = CreateBlockRelationshipScenarioSeeder();
        var seed = await scenarioSeeder.SeedBlockedUsersAsync(2);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.GetAsync($"{BlockTestRoutes.Base}?pageSize={pageSize}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<BlockedUserDto>>(response);
        responseBody.Items.Should().ContainSingle();
        responseBody.HasMore.Should().BeTrue();
    }

    [Theory]
    [InlineData(101)]
    [InlineData(500)]
    public async Task GetBlockedUsers_ShouldClampPageSizeToMaximum(int pageSize)
    {
        // Arrange
        var scenarioSeeder = CreateBlockRelationshipScenarioSeeder();
        var seed = await scenarioSeeder.SeedBlockedUsersAsync(101);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.GetAsync($"{BlockTestRoutes.Base}?pageSize={pageSize}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<BlockedUserDto>>(response);
        responseBody.Items.Should().HaveCount(100);
        responseBody.HasMore.Should().BeTrue();
    }

    [Fact]
    public async Task GetBlockedUsers_ShouldTreatInvalidCursorAsFirstPage()
    {
        // Arrange
        var scenarioSeeder = CreateBlockRelationshipScenarioSeeder();
        var seed = await scenarioSeeder.SeedBlockedUsersAsync(3);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var firstResponse = await client.GetAsync($"{BlockTestRoutes.Base}?pageSize=2");
        var invalidCursorResponse = await client.GetAsync(
            $"{BlockTestRoutes.Base}?pageSize=2&cursor=not-a-valid-cursor"
        );

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        invalidCursorResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var firstPage = await ReadJsonAsync<PagedResponse<BlockedUserDto>>(firstResponse);
        var invalidCursorPage = await ReadJsonAsync<PagedResponse<BlockedUserDto>>(
            invalidCursorResponse
        );
        invalidCursorPage
            .Items.Select(item => item.Id)
            .Should()
            .Equal(firstPage.Items.Select(item => item.Id));
    }
}
