using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Data.Tokens;
using Threads.Api.Features.Auth.Endpoints;
using Threads.Api.IntegrationTests.Auth.Infrastructure;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Auth;

public class LogoutTests : AuthIntegrationTestBase
{
    public LogoutTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task Logout_ShouldRejectAnonymousRequests()
    {
        // Arrange
        var client = CreateAnonymousClient();

        // Act
        var response = await client.PostAsync(AuthTestRoutes.Logout, null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_ShouldRevokeMatchingActiveToken()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var tokenSeeder = CreateTokenSeeder();
        var alice = await userSeeder.CreateUserAsync(username: AuthTestData.AliceUsername);
        var activeToken = await tokenSeeder.CreateActiveTokenAsync(alice);
        var client = await CreateAuthenticatedClientAsync(alice);
        var requestBody = new Logout.Request(activeToken.PlainToken);

        // Act
        var response = await client.PostAsJsonAsync(AuthTestRoutes.Logout, requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        Db.ChangeTracker.Clear();

        var persisted = await Db.RefreshTokens.SingleAsync(rt => rt.Id == activeToken.Entity.Id);
        persisted.IsRevoked.Should().BeTrue();
        persisted.RevocationReason.Should().Be(RevocationReason.Logout);
    }

    [Fact]
    public async Task Logout_ShouldReturnOk_WhenTokenIsUnknown()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var tokenSeeder = CreateTokenSeeder();
        var alice = await userSeeder.CreateUserAsync(username: AuthTestData.AliceUsername);
        var existingToken = await tokenSeeder.CreateActiveTokenAsync(
            alice,
            tokenSeed: AuthTestData.ActiveTokenSeed
        );
        var client = await CreateAuthenticatedClientAsync(alice);
        var requestBody = new Logout.Request(
            AuthRefreshTokenSeeder.CreatePlainToken(AuthTestData.UnknownTokenSeed)
        );

        // Act
        var response = await client.PostAsJsonAsync(AuthTestRoutes.Logout, requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        Db.ChangeTracker.Clear();

        var persisted = await Db.RefreshTokens.SingleAsync(rt => rt.Id == existingToken.Entity.Id);
        persisted.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Logout_ShouldLeaveOtherUsersTokenUnchanged()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var tokenSeeder = CreateTokenSeeder();
        var alice = await userSeeder.CreateUserAsync(username: AuthTestData.AliceUsername);
        var henry = await userSeeder.CreateUserAsync(username: AuthTestData.HenryUsername);
        var henryToken = await tokenSeeder.CreateActiveTokenAsync(henry);
        var client = await CreateAuthenticatedClientAsync(alice);
        var requestBody = new Logout.Request(henryToken.PlainToken);

        // Act
        var response = await client.PostAsJsonAsync(AuthTestRoutes.Logout, requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        Db.ChangeTracker.Clear();

        var persisted = await Db.RefreshTokens.SingleAsync(rt => rt.Id == henryToken.Entity.Id);
        persisted.IsActive.Should().BeTrue();
        persisted.RevokedAtUtc.Should().BeNull();
        persisted.RevocationReason.Should().BeNull();
    }

    [Fact]
    public async Task Logout_ShouldLeaveExpiredTokenUnchanged()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var tokenSeeder = CreateTokenSeeder();
        var alice = await userSeeder.CreateUserAsync(username: AuthTestData.AliceUsername);
        var expiredToken = await tokenSeeder.CreateExpiredTokenAsync(alice);
        var client = await CreateAuthenticatedClientAsync(alice);
        var requestBody = new Logout.Request(expiredToken.PlainToken);

        // Act
        var response = await client.PostAsJsonAsync(AuthTestRoutes.Logout, requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        Db.ChangeTracker.Clear();

        var persisted = await Db.RefreshTokens.SingleAsync(rt => rt.Id == expiredToken.Entity.Id);
        persisted.RevokedAtUtc.Should().BeNull();
        persisted.RevocationReason.Should().BeNull();
    }

    [Theory]
    [InlineData(RevocationReason.Logout)]
    [InlineData(RevocationReason.GlobalLogout)]
    [InlineData(RevocationReason.Compromised)]
    public async Task Logout_ShouldLeaveAlreadyRevokedTokenUnchanged(RevocationReason reason)
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var tokenSeeder = CreateTokenSeeder();
        var alice = await userSeeder.CreateUserAsync(username: AuthTestData.AliceUsername);
        var revokedToken = await tokenSeeder.CreateRevokedTokenAsync(alice, reason);
        var client = await CreateAuthenticatedClientAsync(alice);
        var requestBody = new Logout.Request(revokedToken.PlainToken);

        // Act
        var response = await client.PostAsJsonAsync(AuthTestRoutes.Logout, requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        Db.ChangeTracker.Clear();

        var persisted = await Db.RefreshTokens.SingleAsync(rt => rt.Id == revokedToken.Entity.Id);
        persisted.RevocationReason.Should().Be(reason);
    }

    [Fact]
    public async Task Logout_ShouldReturnBadRequest_WhenTokenLengthIsInvalid()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var alice = await userSeeder.CreateUserAsync(username: AuthTestData.AliceUsername);
        var client = await CreateAuthenticatedClientAsync(alice);
        var requestBody = new Logout.Request("short");

        // Act
        var response = await client.PostAsJsonAsync(AuthTestRoutes.Logout, requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Logout_ShouldReturnBadRequest_WhenJsonContainsUnknownProperty()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var alice = await userSeeder.CreateUserAsync(username: AuthTestData.AliceUsername);
        var client = await CreateAuthenticatedClientAsync(alice);

        // Act
        var response = await client.PostAsJsonAsync(
            AuthTestRoutes.Logout,
            new
            {
                refreshToken = AuthRefreshTokenSeeder.CreatePlainToken(
                    AuthTestData.ActiveTokenSeed
                ),
                unexpected = true,
            }
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Logout_ShouldReturnBadRequest_WhenJsonIsMalformed()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var alice = await userSeeder.CreateUserAsync(username: AuthTestData.AliceUsername);
        var client = await CreateAuthenticatedClientAsync(alice);
        using var requestBody = new StringContent(
            "{\"refreshToken\":\"value\"",
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await client.PostAsync(AuthTestRoutes.Logout, requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
