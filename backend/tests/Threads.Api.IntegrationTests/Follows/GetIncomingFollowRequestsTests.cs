using System.Net;
using FluentAssertions;
using Threads.Api.Common.Pagination;
using Threads.Api.Features.Follows;
using Threads.Api.IntegrationTests.Follows.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Follows;

public class GetIncomingFollowRequestsTests : FollowIntegrationTestBase
{
    public GetIncomingFollowRequestsTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task GetIncomingFollowRequests_ShouldReturnEmptyPage_WhenNoRequestsExist()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.GetAsync(FollowTestRoutes.IncomingRequests);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<FollowUserDto>>(response);
        responseBody.Items.Should().BeEmpty();
        responseBody.Cursor.Should().BeNull();
        responseBody.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task GetIncomingFollowRequests_ShouldReturnOnlyUnblockedIncomingPendingUsers()
    {
        // Arrange
        var scenarioSeeder = CreateFollowRelationshipScenarioSeeder();
        var seed = await scenarioSeeder.SeedListGraphAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.GetAsync(FollowTestRoutes.IncomingRequests);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<FollowUserDto>>(response);
        responseBody.Items.Should().ContainSingle();
        responseBody.Items[0].Id.Should().Be(seed.IncomingPending.Id);
        responseBody.Items[0].Username.Should().Be(seed.IncomingPending.UserName);
        responseBody.Items[0].IsPrivate.Should().Be(seed.IncomingPending.IsPrivate);
        responseBody.Items[0].CreatedAtUtc.Should().Be(IntegrationTestData.BaseTime.AddMinutes(1));
    }

    [Fact]
    public async Task GetIncomingFollowRequests_ShouldReturnNextPageInDescendingOrder()
    {
        // Arrange
        var scenarioSeeder = CreateFollowRelationshipScenarioSeeder();
        var seed = await scenarioSeeder.SeedBulkAsync(FollowListKind.IncomingRequests, 5);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var firstResponse = await client.GetAsync(
            $"{FollowTestRoutes.IncomingRequests}?pageSize=2"
        );
        var firstPage = await ReadJsonAsync<PagedResponse<FollowUserDto>>(firstResponse);
        var secondResponse = await client.GetAsync(
            $"{FollowTestRoutes.IncomingRequests}?pageSize=2&cursor={Uri.EscapeDataString(firstPage.Cursor!)}"
        );

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        firstPage.Items.Select(item => item.Id).Should().Equal(seed.Users[4].Id, seed.Users[3].Id);

        var secondPage = await ReadJsonAsync<PagedResponse<FollowUserDto>>(secondResponse);
        secondPage.Items.Select(item => item.Id).Should().Equal(seed.Users[2].Id, seed.Users[1].Id);
    }
}
