using System.Net;
using FluentAssertions;
using Threads.Api.Common.Pagination;
using Threads.Api.Data.Follows;
using Threads.Api.Features.Users;
using Threads.Api.IntegrationTests.Follows.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;
using Threads.Api.IntegrationTests.Users.TestSupport;

namespace Threads.Api.IntegrationTests.Users;

public class SearchUsersTests : IntegrationTestBase
{
    public SearchUsersTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task SearchUsers_ShouldReturnCaseInsensitiveSubstringMatchesOrderedByCreatedAtThenIdDescending()
    {
        // Arrange
        var userSeeder = Seeders.Users;
        var olderMatch = await userSeeder.CreateUserAsync(
            username: "prefixAlice",
            createdAtUtc: IntegrationTestData.BaseTime
        );
        var newerMatch = await userSeeder.CreateUserAsync(
            username: "ALICE_suffix",
            isPrivate: true,
            createdAtUtc: IntegrationTestData.BaseTime.AddMinutes(1)
        );
        await userSeeder.CreateUserAsync(
            username: IntegrationTestData.BobUsername,
            createdAtUtc: IntegrationTestData.BaseTime.AddMinutes(2)
        );
        var client = CreateAnonymousClient();

        // Act
        var response = await client.GetAsync($"{UserTestRoutes.SearchUsers}?username=alice");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<UserProfileDto>>(response);
        responseBody.Items.Select(user => user.Id).Should().Equal(newerMatch.Id, olderMatch.Id);
        responseBody.Items.Single(user => user.Id == newerMatch.Id).IsPrivate.Should().BeTrue();
        responseBody.Cursor.Should().BeNull();
        responseBody.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task SearchUsers_ShouldTreatUnderscoreAsLiteralCharacter()
    {
        // Arrange
        var userSeeder = Seeders.Users;
        var literalMatch = await userSeeder.CreateUserAsync(username: "alpha_user");
        await userSeeder.CreateUserAsync(username: "alphaxuser");
        var client = CreateAnonymousClient();

        // Act
        var response = await client.GetAsync($"{UserTestRoutes.SearchUsers}?username=alpha_");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<UserProfileDto>>(response);
        responseBody.Items.Select(user => user.Id).Should().Equal(literalMatch.Id);
    }

    [Fact]
    public async Task SearchUsers_ShouldReturnEmptyPage_WhenNoUsersMatch()
    {
        // Arrange
        await Seeders.Users.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var client = CreateAnonymousClient();

        // Act
        var response = await client.GetAsync($"{UserTestRoutes.SearchUsers}?username=missing");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<UserProfileDto>>(response);
        responseBody.Items.Should().BeEmpty();
        responseBody.Cursor.Should().BeNull();
        responseBody.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task SearchUsers_ShouldReturnNextMatchingPage_WhenCursorIsProvided()
    {
        // Arrange
        await Seeders.Users.SeedBulkUsersAsync(5);
        var client = CreateAnonymousClient();

        // Act
        var firstResponse = await client.GetAsync(
            $"{UserTestRoutes.SearchUsers}?username=bulk&pageSize=2"
        );
        var firstPage = await ReadJsonAsync<PagedResponse<UserProfileDto>>(firstResponse);
        var secondResponse = await client.GetAsync(
            $"{UserTestRoutes.SearchUsers}?username=bulk&pageSize=2&cursor={Uri.EscapeDataString(firstPage.Cursor!)}"
        );

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        firstPage.Items.Should().HaveCount(2);
        firstPage.HasMore.Should().BeTrue();
        firstPage.Cursor.Should().NotBeNullOrWhiteSpace();
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

    [Fact]
    public async Task SearchUsers_ShouldExcludeUsersWithBlockRelationshipWithRequester()
    {
        // Arrange
        var userSeeder = Seeders.Users;
        var blockSeeder = Seeders.Blocks;
        var requester = await userSeeder.CreateUserAsync(username: "requester");
        var visible = await userSeeder.CreateUserAsync(username: "match_visible");
        var blocked = await userSeeder.CreateUserAsync(username: "match_blocked");
        var blocker = await userSeeder.CreateUserAsync(username: "match_blocker");

        await blockSeeder.CreateUserBlockAsync(blocker: requester, blocked: blocked);
        await blockSeeder.CreateUserBlockAsync(blocker: blocker, blocked: requester);

        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.GetAsync($"{UserTestRoutes.SearchUsers}?username=match_");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<UserProfileDto>>(response);
        responseBody.Items.Select(user => user.Id).Should().Equal(visible.Id);
        responseBody.Items.Select(user => user.Id).Should().NotContain([blocked.Id, blocker.Id]);
    }

    [Fact]
    public async Task SearchUsers_ShouldReturnFollowDataForAuthenticatedRequester()
    {
        // Arrange
        var userSeeder = Seeders.Users;
        var followSeeder = Seeders.Follows;
        var requester = await userSeeder.CreateUserAsync(username: "requester");
        var target = await userSeeder.CreateUserAsync(username: "target_user");
        var followedByTarget = await userSeeder.CreateUserAsync(username: "other_user");

        await followSeeder.CreateFollowAsync(requester, target, FollowStatus.Accepted);
        await followSeeder.CreateFollowAsync(target, followedByTarget, FollowStatus.Accepted);

        var client = await CreateAuthenticatedClientAsync(requester);

        // Act
        var response = await client.GetAsync($"{UserTestRoutes.SearchUsers}?username=target");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<PagedResponse<UserProfileDto>>(response);
        var targetResult = responseBody.Items.Should().ContainSingle().Subject;
        targetResult.Id.Should().Be(target.Id);
        targetResult.FollowerCount.Should().Be(1);
        targetResult.FollowingCount.Should().Be(1);
        targetResult.FollowStatusWithCurrentUser.Should().Be(FollowStatus.Accepted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("?username=")]
    [InlineData("?username=ab")]
    [InlineData("?username=abcdefghijklmnopqrstuvwxyzabcde")]
    [InlineData("?username=user%20name")]
    [InlineData("?username=%40user")]
    [InlineData("?username=user-name")]
    public async Task SearchUsers_ShouldReturnBadRequest_WhenUsernameIsInvalid(string queryString)
    {
        // Arrange
        var client = CreateAnonymousClient();

        // Act
        var response = await client.GetAsync($"{UserTestRoutes.SearchUsers}{queryString}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("user123")]
    [InlineData("user_name")]
    [InlineData("user.name")]
    public async Task SearchUsers_ShouldAcceptValidUsernameSearchValue(string username)
    {
        // Arrange
        var client = CreateAnonymousClient();

        // Act
        var response = await client.GetAsync(
            $"{UserTestRoutes.SearchUsers}?username={Uri.EscapeDataString(username)}"
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
