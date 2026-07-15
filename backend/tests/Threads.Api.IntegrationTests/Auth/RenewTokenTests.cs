using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Threads.Api.Data.Tokens;
using Threads.Api.Features.Auth.Endpoints;
using Threads.Api.Features.Auth.Services.RefreshToken;
using Threads.Api.IntegrationTests.Auth.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Auth;

public class RenewTokenTests : AuthIntegrationTestBase
{
    public RenewTokenTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task RenewToken_ShouldRotateActiveToken()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var tokenSeeder = CreateTokenSeeder();
        var alice = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var activeToken = await tokenSeeder.CreateActiveTokenAsync(alice);
        var client = CreateAnonymousClient();
        var requestBody = new RenewToken.Request(activeToken.PlainToken);

        // Act
        var response = await client.PostAsJsonAsync(AuthTestRoutes.RenewToken, requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<RenewToken.Response>(response);
        responseBody.AccessToken.Should().NotBeNullOrWhiteSpace();
        responseBody.RefreshToken.Should().NotBeNullOrWhiteSpace();
        responseBody.RefreshToken.Should().NotBe(activeToken.PlainToken);
        responseBody.RefreshToken.Should().HaveLength(44);

        Db.ChangeTracker.Clear();

        var oldToken = await Db
            .RefreshTokens.Include(rt => rt.ReplacedByToken)
            .SingleAsync(rt => rt.Id == activeToken.Entity.Id);

        var refreshTokenProvider = Services.GetRequiredService<IRefreshTokenProvider>();
        var newTokenHash = refreshTokenProvider.Hash(responseBody.RefreshToken);
        var newToken = await Db.RefreshTokens.SingleAsync(rt => rt.TokenHash == newTokenHash);

        oldToken.IsRevoked.Should().BeTrue();
        oldToken.RevocationReason.Should().Be(RevocationReason.ReplacedByNewToken);
        oldToken.ReplacedByTokenId.Should().Be(newToken.Id);

        newToken.UserId.Should().Be(alice.Id);
        newToken.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task RenewToken_ShouldReturnAccessTokenThatCanAuthorizeProtectedAuthEndpoint()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var tokenSeeder = CreateTokenSeeder();
        var alice = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var activeToken = await tokenSeeder.CreateActiveTokenAsync(alice);
        var client = CreateAnonymousClient();
        var requestBody = new RenewToken.Request(activeToken.PlainToken);

        // Act
        var renewResponse = await client.PostAsJsonAsync(AuthTestRoutes.RenewToken, requestBody);

        // Assert
        renewResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<RenewToken.Response>(renewResponse);
        var authorizedClient = CreateAuthorizedClientWithBearerToken(responseBody.AccessToken);

        var response = await authorizedClient.PostAsync(AuthTestRoutes.LogoutAll, null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RenewToken_ShouldReturnBadRequest_WhenTokenIsUnknown()
    {
        // Arrange
        var client = CreateAnonymousClient();
        var unknownToken = RefreshTokenTestSeeder.CreatePlainToken(AuthTestData.UnknownTokenSeed);
        var requestBody = new RenewToken.Request(unknownToken);

        // Act
        var response = await client.PostAsJsonAsync(AuthTestRoutes.RenewToken, requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RenewToken_ShouldReturnBadRequest_WhenTokenIsExpired()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var tokenSeeder = CreateTokenSeeder();
        var alice = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var expiredToken = await tokenSeeder.CreateExpiredTokenAsync(alice);
        var client = CreateAnonymousClient();
        var requestBody = new RenewToken.Request(expiredToken.PlainToken);

        // Act
        var response = await client.PostAsJsonAsync(AuthTestRoutes.RenewToken, requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        Db.ChangeTracker.Clear();

        var persisted = await Db.RefreshTokens.SingleAsync(rt => rt.Id == expiredToken.Entity.Id);
        persisted.RevokedAtUtc.Should().BeNull();
        persisted.RevocationReason.Should().BeNull();
    }

    [Fact]
    public async Task RenewToken_ShouldReturnBadRequest_WhenTokenIsAlreadyRevoked()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var tokenSeeder = CreateTokenSeeder();
        var alice = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var revokedToken = await tokenSeeder.CreateRevokedTokenAsync(
            alice,
            RevocationReason.Logout
        );
        var client = CreateAnonymousClient();
        var requestBody = new RenewToken.Request(revokedToken.PlainToken);

        // Act
        var response = await client.PostAsJsonAsync(AuthTestRoutes.RenewToken, requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        Db.ChangeTracker.Clear();

        var persisted = await Db.RefreshTokens.SingleAsync(rt => rt.Id == revokedToken.Entity.Id);
        persisted.RevocationReason.Should().Be(RevocationReason.Logout);
    }

    [Fact]
    public async Task RenewToken_ShouldCompromiseActiveTokens_WhenReplacedTokenIsReused()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var tokenSeeder = CreateTokenSeeder();
        var refreshTokenProvider = Services.GetRequiredService<IRefreshTokenProvider>();
        var alice = await userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var activeToken = await tokenSeeder.CreateActiveTokenAsync(alice);
        var client = CreateAnonymousClient();
        var requestBody = new RenewToken.Request(activeToken.PlainToken);

        // Act
        var firstResponse = await client.PostAsJsonAsync(AuthTestRoutes.RenewToken, requestBody);
        var firstResponseBody = await ReadJsonAsync<RenewToken.Response>(firstResponse);
        var secondResponse = await client.PostAsJsonAsync(AuthTestRoutes.RenewToken, requestBody);

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        Db.ChangeTracker.Clear();

        var rotatedTokenHash = refreshTokenProvider.Hash(firstResponseBody.RefreshToken);
        var rotatedToken = await Db.RefreshTokens.SingleAsync(rt =>
            rt.TokenHash == rotatedTokenHash
        );

        rotatedToken.IsRevoked.Should().BeTrue();
        rotatedToken.RevocationReason.Should().Be(RevocationReason.Compromised);
    }

    [Fact]
    public async Task RenewToken_ShouldReturnBadRequest_WhenTokenLengthIsInvalid()
    {
        // Arrange
        var client = CreateAnonymousClient();
        var requestBody = new RenewToken.Request("short");

        // Act
        var response = await client.PostAsJsonAsync(AuthTestRoutes.RenewToken, requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RenewToken_ShouldReturnBadRequest_WhenJsonContainsUnknownProperty()
    {
        // Arrange
        var client = CreateAnonymousClient();

        // Act
        var response = await client.PostAsJsonAsync(
            AuthTestRoutes.RenewToken,
            new
            {
                refreshToken = RefreshTokenTestSeeder.CreatePlainToken(
                    AuthTestData.UnknownTokenSeed
                ),
                unexpected = true,
            }
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RenewToken_ShouldReturnBadRequest_WhenJsonIsMalformed()
    {
        // Arrange
        var client = CreateAnonymousClient();
        using var content = new StringContent(
            "{\"refreshToken\":\"value\"",
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await client.PostAsync(AuthTestRoutes.RenewToken, content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
