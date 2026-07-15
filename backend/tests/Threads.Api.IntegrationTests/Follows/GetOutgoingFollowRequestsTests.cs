using System.Net;
using FluentAssertions;
using Threads.Api.Common.Pagination;
using Threads.Api.Features.Follows;
using Threads.Api.IntegrationTests.Follows.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Follows;

public class GetOutgoingFollowRequestsTests : FollowIntegrationTestBase
{
    public GetOutgoingFollowRequestsTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task GetOutgoingFollowRequests_ShouldReturnEmptyPage_WhenNoRequestsExist()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var requester = await userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.GetAsync(FollowTestRoutes.OutgoingRequests);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<FollowUserDto>>(response);
        responseBody.Items.Should().BeEmpty();
        responseBody.Cursor.Should().BeNull();
        responseBody.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task GetOutgoingFollowRequests_ShouldReturnOnlyUnblockedOutgoingPendingUsers()
    {
        // Arrange
        var scenarioSeeder = CreateFollowRelationshipScenarioSeeder();
        var seed = await scenarioSeeder.SeedListGraphAsync();
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.GetAsync(FollowTestRoutes.OutgoingRequests);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<FollowUserDto>>(response);
        responseBody.Items.Should().ContainSingle();
        responseBody.Items[0].Id.Should().Be(seed.OutgoingPending.Id);
        responseBody.Items[0].Username.Should().Be(seed.OutgoingPending.UserName);
        responseBody.Items[0].IsPrivate.Should().Be(seed.OutgoingPending.IsPrivate);
        responseBody.Items[0].CreatedAtUtc.Should().Be(IntegrationTestData.BaseTime.AddMinutes(2));
    }

    [Fact]
    public async Task GetOutgoingFollowRequests_ShouldReturnNextPageInDescendingOrder()
    {
        // Arrange
        var scenarioSeeder = CreateFollowRelationshipScenarioSeeder();
        var seed = await scenarioSeeder.SeedBulkAsync(FollowListKind.OutgoingRequests, 5);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var firstResponse = await client.GetAsync(
            $"{FollowTestRoutes.OutgoingRequests}?pageSize=2"
        );
        var firstPage = await ReadJsonAsync<PagedResponse<FollowUserDto>>(firstResponse);
        var secondResponse = await client.GetAsync(
            $"{FollowTestRoutes.OutgoingRequests}?pageSize=2&cursor={Uri.EscapeDataString(firstPage.Cursor!)}"
        );

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        firstPage.Items.Select(item => item.Id).Should().Equal(seed.Users[4].Id, seed.Users[3].Id);

        var secondPage = await ReadJsonAsync<PagedResponse<FollowUserDto>>(secondResponse);
        secondPage.Items.Select(item => item.Id).Should().Equal(seed.Users[2].Id, seed.Users[1].Id);
    }
}
