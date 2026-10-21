using System.Net;
using FluentAssertions;
using Threads.Api.Common.Pagination;
using Threads.Api.Features.Follows;
using Threads.Api.IntegrationTests.Follows.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Follows;

public class GetFollowingTests : IntegrationTestBase
{
    public GetFollowingTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task GetFollowing_ShouldReturnEmptyPage_WhenUserFollowsNobody()
    {
        // Arrange
        var requester = await Seeders.Users.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.GetAsync(FollowTestRoutes.FollowingFor(requester.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<FollowUserDto>>(response);
        responseBody.Items.Should().BeEmpty();
        responseBody.Cursor.Should().BeNull();
        responseBody.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task GetFollowing_ShouldReturnTargetUsersFollowingWithCurrentUsersFollowStatus()
    {
        // Arrange
        var seed = await Scenarios.FollowRelationships.SeedListGraphAsync();
        var viewer = await Seeders.Users.CreateUserAsync(username: "following-viewer");
        await Seeders.Follows.CreateFollowAsync(
            viewer,
            seed.OutgoingAccepted,
            Threads.Api.Data.Follows.FollowStatus.Accepted
        );
        var client = await CreateAuthenticatedClientAsync(viewer);

        // Act
        var response = await client.GetAsync(FollowTestRoutes.FollowingFor(seed.Requester.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<FollowUserDto>>(response);
        responseBody.Items.Should().ContainSingle();
        responseBody.Items[0].Id.Should().Be(seed.OutgoingAccepted.Id);
        responseBody.Items[0].Username.Should().Be(seed.OutgoingAccepted.UserName);
        responseBody.Items[0].IsPrivate.Should().Be(seed.OutgoingAccepted.IsPrivate);
        responseBody.Items[0].CreatedAtUtc.Should().Be(IntegrationTestData.BaseTime.AddMinutes(4));
        responseBody
            .Items[0]
            .FollowStatusWithCurrentUser.Should()
            .Be(Threads.Api.Data.Follows.FollowStatus.Accepted);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetFollowing_ShouldReturnFollowingToAnonymousRequesterRegardlessOfProfilePrivacy(
        bool isPrivate
    )
    {
        // Arrange
        var targetUser = await Seeders.Users.CreateUserAsync(
            username: "anonymous-following-target",
            isPrivate: isPrivate
        );
        var followedUser = await Seeders.Users.CreateUserAsync(username: "anonymous-followed-user");
        await Seeders.Follows.CreateFollowAsync(
            targetUser,
            followedUser,
            Threads.Api.Data.Follows.FollowStatus.Accepted
        );
        var client = CreateAnonymousClient();

        // Act
        var response = await client.GetAsync(FollowTestRoutes.FollowingFor(targetUser.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<FollowUserDto>>(response);
        responseBody.Items.Should().ContainSingle();
        responseBody.Items[0].Id.Should().Be(followedUser.Id);
        responseBody.Items[0].FollowStatusWithCurrentUser.Should().BeNull();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetFollowing_ShouldReturnNotFound_WhenRequesterAndTargetUserHaveBlockRelationship(
        bool requesterBlocksTarget
    )
    {
        // Arrange
        var requester = await Seeders.Users.CreateUserAsync(username: "blocked-following-viewer");
        var targetUser = await Seeders.Users.CreateUserAsync(username: "blocked-following-target");
        var followedUser = await Seeders.Users.CreateUserAsync(username: "blocked-followed-user");
        await Seeders.Follows.CreateFollowAsync(
            targetUser,
            followedUser,
            Threads.Api.Data.Follows.FollowStatus.Accepted
        );
        await Seeders.Blocks.CreateUserBlockAsync(
            blocker: requesterBlocksTarget ? requester : targetUser,
            blocked: requesterBlocksTarget ? targetUser : requester
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.GetAsync(FollowTestRoutes.FollowingFor(targetUser.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetFollowing_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var client = CreateAnonymousClient();

        // Act
        var response = await client.GetAsync(FollowTestRoutes.FollowingFor(Guid.NewGuid()));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetFollowing_ShouldReturnNextPageInDescendingOrder()
    {
        // Arrange
        var seed = await Scenarios.FollowRelationships.SeedBulkAsync(FollowListKind.Following, 5);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var route = FollowTestRoutes.FollowingFor(seed.Requester.Id);
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
    public async Task GetFollowing_ShouldUseDefaultPageSize()
    {
        // Arrange
        var seed = await Scenarios.FollowRelationships.SeedBulkAsync(FollowListKind.Following, 21);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.GetAsync(FollowTestRoutes.FollowingFor(seed.Requester.Id));

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
        var seed = await Scenarios.FollowRelationships.SeedBulkAsync(FollowListKind.Following, 2);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.GetAsync(
            $"{FollowTestRoutes.FollowingFor(seed.Requester.Id)}?pageSize={pageSize}"
        );

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
        var seed = await Scenarios.FollowRelationships.SeedBulkAsync(FollowListKind.Following, 101);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var response = await client.GetAsync(
            $"{FollowTestRoutes.FollowingFor(seed.Requester.Id)}?pageSize={pageSize}"
        );

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
        var seed = await Scenarios.FollowRelationships.SeedBulkAsync(FollowListKind.Following, 3);
        var client = await CreateAuthenticatedClientAsync(seed.Requester);

        // Act
        var route = FollowTestRoutes.FollowingFor(seed.Requester.Id);
        var firstResponse = await client.GetAsync($"{route}?pageSize=2");
        var invalidCursorResponse = await client.GetAsync(
            $"{route}?pageSize=2&cursor=not-a-valid-cursor"
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

    [Fact]
    public async Task GetFollowing_ShouldReturnBadRequest_WhenUserIdIsEmpty()
    {
        // Arrange
        var requester = await Seeders.Users.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.GetAsync(FollowTestRoutes.FollowingFor(Guid.Empty));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
