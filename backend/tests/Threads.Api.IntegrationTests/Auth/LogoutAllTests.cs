using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Data.Tokens;
using Threads.Api.IntegrationTests.Auth.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Auth;

public class LogoutAllTests : AuthIntegrationTestBase
{
    public LogoutAllTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task LogoutAll_ShouldRejectAnonymousRequests()
    {
        // Arrange
        var client = CreateAnonymousClient();

        // Act
        var response = await client.PostAsync(AuthTestRoutes.LogoutAll, null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LogoutAll_ShouldRevokeOnlyAuthenticatedUsersActiveTokens()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var tokenSeeder = CreateTokenSeeder();

        var alice = await userSeeder.CreateUserAsync(username: AuthTestData.AliceUsername);
        var henry = await userSeeder.CreateUserAsync(username: AuthTestData.HenryUsername);
        var client = await CreateAuthenticatedClientAsync(alice);

        var aliceActiveToken = await tokenSeeder.CreateActiveTokenAsync(
            alice,
            tokenSeed: AuthTestData.ActiveTokenSeed
        );

        var aliceSecondActiveToken = await tokenSeeder.CreateActiveTokenAsync(
            alice,
            tokenSeed: AuthTestData.SecondActiveTokenSeed
        );

        var aliceExpiredToken = await tokenSeeder.CreateExpiredTokenAsync(
            alice,
            tokenSeed: AuthTestData.ExpiredTokenSeed
        );

        var aliceAlreadyRevokedToken = await tokenSeeder.CreateRevokedTokenAsync(
            alice,
            RevocationReason.Logout,
            tokenSeed: AuthTestData.RevokedTokenSeed
        );

        var henryActiveToken = await tokenSeeder.CreateActiveTokenAsync(
            henry,
            tokenSeed: AuthTestData.OtherUserActiveTokenSeed
        );

        // Act
        var response = await client.PostAsync(AuthTestRoutes.LogoutAll, null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        Db.ChangeTracker.Clear();

        var aliceActive = await FindRefreshTokenById(aliceActiveToken.Entity.Id);
        var aliceSecondActive = await FindRefreshTokenById(aliceSecondActiveToken.Entity.Id);
        var aliceExpired = await FindRefreshTokenById(aliceExpiredToken.Entity.Id);
        var aliceAlreadyRevoked = await FindRefreshTokenById(aliceAlreadyRevokedToken.Entity.Id);
        var henryActive = await FindRefreshTokenById(henryActiveToken.Entity.Id);

        aliceActive.IsRevoked.Should().BeTrue();
        aliceActive.RevocationReason.Should().Be(RevocationReason.GlobalLogout);
        aliceSecondActive.IsRevoked.Should().BeTrue();
        aliceSecondActive.RevocationReason.Should().Be(RevocationReason.GlobalLogout);
        aliceExpired.RevokedAtUtc.Should().BeNull();
        aliceExpired.RevocationReason.Should().BeNull();
        aliceAlreadyRevoked.RevocationReason.Should().Be(RevocationReason.Logout);
        henryActive.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task LogoutAll_ShouldReturnOk_WhenUserHasNoRefreshTokens()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var alice = await userSeeder.CreateUserAsync(username: AuthTestData.AliceUsername);
        var client = await CreateAuthenticatedClientAsync(alice);

        // Act
        var response = await client.PostAsync(AuthTestRoutes.LogoutAll, null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await Db.RefreshTokens.CountAsync()).Should().Be(0);
    }

    private async Task<RefreshToken> FindRefreshTokenById(Guid id)
    {
        return await Db.RefreshTokens.SingleAsync(rt => rt.Id == id);
    }
}
