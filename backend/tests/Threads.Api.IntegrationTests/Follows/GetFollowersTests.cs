using System.Net;
using FluentAssertions;
using Threads.Api.Common.Pagination;
using Threads.Api.Features.Follows;
using Threads.Api.IntegrationTests.Follows.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Follows;

public class GetFollowersTests : IntegrationTestBase
{
    public GetFollowersTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task GetFollowers_ShouldReturnEmptyPage_WhenUserHasNoFollowers()
    {
        // Arrange
        var requester = await Seeders.Users.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.GetAsync(FollowTestRoutes.FollowersFor(requester.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<FollowUserDto>>(response);
        responseBody.Items.Should().BeEmpty();
        responseBody.Cursor.Should().BeNull();
        responseBody.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task GetFollowers_ShouldReturnTargetUsersFollowersWithCurrentUsersFollowStatus()
    {
        // Arrange
        var seed = await Scenarios.FollowRelationships.SeedListGraphAsync();
        var viewer = await Seeders.Users.CreateUserAsync(username: "followers-viewer");
        await Seeders.Follows.CreateFollowAsync(
            viewer,
            seed.IncomingAccepted,
            Threads.Api.Data.Follows.FollowStatus.Pending
        );
        var client = await CreateAuthenticatedClientAsync(viewer);

        // Act
        var response = await client.GetAsync(FollowTestRoutes.FollowersFor(seed.Requester.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<FollowUserDto>>(response);
        responseBody.Items.Should().ContainSingle();
        responseBody.Items[0].Id.Should().Be(seed.IncomingAccepted.Id);
        responseBody.Items[0].Username.Should().Be(seed.IncomingAccepted.UserName);
        responseBody.Items[0].CreatedAtUtc.Should().Be(IntegrationTestData.BaseTime.AddMinutes(3));
        responseBody
            .Items[0]
            .FollowStatusWithCurrentUser.Should()
            .Be(Threads.Api.Data.Follows.FollowStatus.Pending);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetFollowers_ShouldReturnFollowersToAnonymousRequesterRegardlessOfProfilePrivacy(
        bool isPrivate
    )
    {
        // Arrange
        var targetUser = await Seeders.Users.CreateUserAsync(
            username: "anonymous-followers-target",
            isPrivate: isPrivate
        );
        var follower = await Seeders.Users.CreateUserAsync(username: "anonymous-follower");
        await Seeders.Follows.CreateFollowAsync(
            follower,
            targetUser,
            Threads.Api.Data.Follows.FollowStatus.Accepted
        );
        var client = CreateAnonymousClient();

        // Act
        var response = await client.GetAsync(FollowTestRoutes.FollowersFor(targetUser.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<FollowUserDto>>(response);
        responseBody.Items.Should().ContainSingle();
        responseBody.Items[0].Id.Should().Be(follower.Id);
        responseBody.Items[0].FollowStatusWithCurrentUser.Should().BeNull();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetFollowers_ShouldReturnNotFound_WhenRequesterAndTargetUserHaveBlockRelationship(
        bool requesterBlocksTarget
    )
    {
        // Arrange
        var requester = await Seeders.Users.CreateUserAsync(username: "blocked-followers-viewer");
        var targetUser = await Seeders.Users.CreateUserAsync(username: "blocked-followers-target");
        var follower = await Seeders.Users.CreateUserAsync(username: "blocked-follower");
        await Seeders.Follows.CreateFollowAsync(
            follower,
            targetUser,
            Threads.Api.Data.Follows.FollowStatus.Accepted
        );
        await Seeders.Blocks.CreateUserBlockAsync(
            blocker: requesterBlocksTarget ? requester : targetUser,
            blocked: requesterBlocksTarget ? targetUser : requester
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.GetAsync(FollowTestRoutes.FollowersFor(targetUser.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetFollowers_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var client = CreateAnonymousClient();

        // Act
        var response = await client.GetAsync(FollowTestRoutes.FollowersFor(Guid.NewGuid()));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetFollowers_ShouldReturnNextPageInDescendingOrder()
    {
        // Arrange
        var seed = await Scenarios.FollowRelationships.SeedBulkAsync(FollowListKind.Followers, 5);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var route = FollowTestRoutes.FollowersFor(seed.Requester.Id);
        var firstResponse = await client.GetAsync($"{route}?pageSize=2");
        var firstPage = await ReadJsonAsync<PagedResponse<FollowUserDto>>(firstResponse);
        var secondResponse = await client.GetAsync(
            $"{route}?pageSize=2&cursor={Uri.EscapeDataString(firstPage.Cursor!)}"
        );

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        firstPage.Items.Select(item => item.Id).Should().Equal(seed.Users[4].Id, seed.Users[3].Id);

        var secondPage = await ReadJsonAsync<PagedResponse<FollowUserDto>>(secondResponse);
        secondPage.Items.Select(item => item.Id).Should().Equal(seed.Users[2].Id, seed.Users[1].Id);
    }

    [Fact]
    public async Task GetFollowers_ShouldReturnBadRequest_WhenUserIdIsEmpty()
    {
        // Arrange
        var requester = await Seeders.Users.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.GetAsync(FollowTestRoutes.FollowersFor(Guid.Empty));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
