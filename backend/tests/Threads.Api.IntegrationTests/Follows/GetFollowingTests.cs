using System.Net;
using FluentAssertions;
using Threads.Api.Common.Pagination;
using Threads.Api.Features.Follows;
using Threads.Api.IntegrationTests.Follows.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Follows;

public class GetFollowingTests : FollowIntegrationTestBase
{
    public GetFollowingTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task GetFollowing_ShouldReturnEmptyPage_WhenRequesterFollowsNobody()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.GetAsync(FollowTestRoutes.Following);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<FollowUserDto>>(response);
        responseBody.Items.Should().BeEmpty();
        responseBody.Cursor.Should().BeNull();
        responseBody.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task GetFollowing_ShouldReturnOnlyUnblockedOutgoingAcceptedUsers()
    {
        // Arrange
        var scenarioSeeder = CreateFollowRelationshipScenarioSeeder();
        var seed = await scenarioSeeder.SeedListGraphAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.GetAsync(FollowTestRoutes.Following);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<FollowUserDto>>(response);
        responseBody.Items.Should().ContainSingle();
        responseBody.Items[0].Id.Should().Be(seed.OutgoingAccepted.Id);
        responseBody.Items[0].Username.Should().Be(seed.OutgoingAccepted.UserName);
        responseBody.Items[0].IsPrivate.Should().Be(seed.OutgoingAccepted.IsPrivate);
        responseBody.Items[0].CreatedAtUtc.Should().Be(IntegrationTestData.BaseTime.AddMinutes(4));
    }

    [Fact]
    public async Task GetFollowing_ShouldReturnNextPageInDescendingOrder()
    {
        // Arrange
        var scenarioSeeder = CreateFollowRelationshipScenarioSeeder();
        var seed = await scenarioSeeder.SeedBulkAsync(FollowListKind.Following, 5);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var firstResponse = await client.GetAsync($"{FollowTestRoutes.Following}?pageSize=2");
        var firstPage = await ReadJsonAsync<PagedResponse<FollowUserDto>>(firstResponse);
        var secondResponse = await client.GetAsync(
            $"{FollowTestRoutes.Following}?pageSize=2&cursor={Uri.EscapeDataString(firstPage.Cursor!)}"
        );

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        firstPage.Items.Select(item => item.Id).Should().Equal(seed.Users[4].Id, seed.Users[3].Id);

        var secondPage = await ReadJsonAsync<PagedResponse<FollowUserDto>>(secondResponse);
        secondPage.Items.Select(item => item.Id).Should().Equal(seed.Users[2].Id, seed.Users[1].Id);
    }

    [Fact]
    public async Task GetFollowing_ShouldUseDefaultPageSize()
    {
        // Arrange
        var scenarioSeeder = CreateFollowRelationshipScenarioSeeder();
        var seed = await scenarioSeeder.SeedBulkAsync(FollowListKind.Following, 21);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.GetAsync(FollowTestRoutes.Following);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<FollowUserDto>>(response);
        responseBody.Items.Should().HaveCount(20);
        responseBody.Cursor.Should().NotBeNullOrWhiteSpace();
        responseBody.HasMore.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task GetFollowing_ShouldClampPageSizeToMinimum(int pageSize)
    {
        // Arrange
        var scenarioSeeder = CreateFollowRelationshipScenarioSeeder();
        var seed = await scenarioSeeder.SeedBulkAsync(FollowListKind.Following, 2);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.GetAsync($"{FollowTestRoutes.Following}?pageSize={pageSize}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<FollowUserDto>>(response);
        responseBody.Items.Should().ContainSingle();
        responseBody.HasMore.Should().BeTrue();
    }

    [Theory]
    [InlineData(101)]
    [InlineData(500)]
    public async Task GetFollowing_ShouldClampPageSizeToMaximum(int pageSize)
    {
        // Arrange
        var scenarioSeeder = CreateFollowRelationshipScenarioSeeder();
        var seed = await scenarioSeeder.SeedBulkAsync(FollowListKind.Following, 101);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.GetAsync($"{FollowTestRoutes.Following}?pageSize={pageSize}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<FollowUserDto>>(response);
        responseBody.Items.Should().HaveCount(100);
        responseBody.HasMore.Should().BeTrue();
    }

    [Fact]
    public async Task GetFollowing_ShouldTreatInvalidCursorAsFirstPage()
    {
        // Arrange
        var scenarioSeeder = CreateFollowRelationshipScenarioSeeder();
        var seed = await scenarioSeeder.SeedBulkAsync(FollowListKind.Following, 3);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var firstResponse = await client.GetAsync($"{FollowTestRoutes.Following}?pageSize=2");
        var invalidCursorResponse = await client.GetAsync(
            $"{FollowTestRoutes.Following}?pageSize=2&cursor=not-a-valid-cursor"
        );

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        invalidCursorResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var firstPage = await ReadJsonAsync<PagedResponse<FollowUserDto>>(firstResponse);
        var invalidCursorPage = await ReadJsonAsync<PagedResponse<FollowUserDto>>(
            invalidCursorResponse
        );
        invalidCursorPage
            .Items.Select(item => item.Id)
            .Should()
            .Equal(firstPage.Items.Select(item => item.Id));
    }
}
