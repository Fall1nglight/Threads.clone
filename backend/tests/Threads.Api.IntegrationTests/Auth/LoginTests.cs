using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Threads.Api.Data.Users;
using Threads.Api.Features.Auth.Endpoints;
using Threads.Api.Features.Auth.Services.RefreshToken;
using Threads.Api.IntegrationTests.Auth.Infrastructure;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Auth;

public class LoginTests : AuthIntegrationTestBase
{
    public LoginTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task Login_ShouldReturnTokensAndPersistRefreshToken_WhenCredentialsAreValid()
    {
        // Arrange
        var client = CreateAnonymousClient();
        var userSeeder = CreateUserSeeder();
        var alice = await userSeeder.CreateUserAsync(
            username: AuthTestData.AliceUsername,
            email: AuthTestData.AliceEmail
        );
        var requestBody = CreateLoginRequestFor(alice);

        // Act
        var response = await client.PostAsJsonAsync(AuthTestRoutes.Login, requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<Login.Response>(response);

        responseBody.AccessToken.Should().NotBeNullOrWhiteSpace();
        responseBody.RefreshToken.Should().NotBeNullOrWhiteSpace();
        responseBody.RefreshToken.Should().HaveLength(44);

        var refreshTokenProvider = Services.GetRequiredService<IRefreshTokenProvider>();
        var refreshTokenHash = refreshTokenProvider.Hash(responseBody.RefreshToken);

        var persistedToken = await Db.RefreshTokens.SingleAsync(rt => rt.UserId == alice.Id);

        persistedToken.TokenHash.Should().Be(refreshTokenHash);
        persistedToken.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Login_ShouldReturnAccessTokenThatCanAuthorizeProtectedAuthEndpoint()
    {
        // Arrange
        var client = CreateAnonymousClient();
        var userSeeder = CreateUserSeeder();
        var alice = await userSeeder.CreateUserAsync(
            username: AuthTestData.AliceUsername,
            email: AuthTestData.AliceEmail
        );
        var requestBody = CreateLoginRequestFor(alice);

        // Act
        var loginResponse = await client.PostAsJsonAsync(AuthTestRoutes.Login, requestBody);

        // Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<Login.Response>(loginResponse);
        var authorizedClient = CreateAuthorizedClientWithBearerToken(responseBody.AccessToken);

        var response = await authorizedClient.PostAsync(AuthTestRoutes.LogoutAll, null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenEmailDoesNotExist()
    {
        // Arrange
        var client = CreateAnonymousClient();
        var requestBody = CreateLoginRequest(
            email: "missing@example.test",
            password: AuthTestData.ValidPassword
        );

        // Act
        var response = await client.PostAsJsonAsync(AuthTestRoutes.Login, requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var problem = await ReadJsonAsync<ProblemDetails>(response);
        problem.Detail.Should().Be("The provided email is incorrect");
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsWrong()
    {
        // Arrange
        var client = CreateAnonymousClient();
        var userSeeder = CreateUserSeeder();
        var alice = await userSeeder.CreateUserAsync(
            username: AuthTestData.AliceUsername,
            email: AuthTestData.AliceEmail
        );
        var requestBody = CreateLoginRequestFor(alice, password: AuthTestData.WrongPassword);

        // Act
        var response = await client.PostAsJsonAsync(AuthTestRoutes.Login, requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var problem = await ReadJsonAsync<ProblemDetails>(response);
        problem.Detail.Should().Be("The provided password is incorrect");
        (await Db.RefreshTokens.CountAsync()).Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(InvalidLoginRequests))]
    public async Task Login_ShouldReturnBadRequest_WhenRequestIsInvalid(Login.Request request)
    {
        // Arrange
        var client = CreateAnonymousClient();

        // Act
        var response = await client.PostAsJsonAsync(AuthTestRoutes.Login, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Db.RefreshTokens.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenJsonContainsUnknownProperty()
    {
        // Arrange
        var client = CreateAnonymousClient();

        // Act
        var response = await client.PostAsJsonAsync(
            AuthTestRoutes.Login,
            new
            {
                email = AuthTestData.AliceEmail,
                password = AuthTestData.ValidPassword,
                unexpected = true,
            }
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenJsonIsMalformed()
    {
        // Arrange
        var client = CreateAnonymousClient();
        using var content = new StringContent(
            "{\"email\":\"alice@example.test\"",
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await client.PostAsync(AuthTestRoutes.Login, content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    public static TheoryData<Login.Request> InvalidLoginRequests =>
        [
            CreateLoginRequest(email: "", password: AuthTestData.ValidPassword),
            CreateLoginRequest(email: "invalid-email", password: AuthTestData.ValidPassword),
            CreateLoginRequest(
                email: $"{new string('a', 257)}@example.test",
                password: AuthTestData.ValidPassword
            ),
            CreateLoginRequest(email: AuthTestData.AliceEmail, password: ""),
            CreateLoginRequest(email: AuthTestData.AliceEmail, password: "Short1!"),
            CreateLoginRequest(email: AuthTestData.AliceEmail, password: "test123!"),
            CreateLoginRequest(email: AuthTestData.AliceEmail, password: "TEST123!"),
            CreateLoginRequest(email: AuthTestData.AliceEmail, password: "TestTest!"),
            CreateLoginRequest(email: AuthTestData.AliceEmail, password: "Test1234"),
        ];

    private static Login.Request CreateLoginRequestFor(
        User user,
        string password = AuthTestData.ValidPassword
    ) => new(user.Email!, password);

    private static Login.Request CreateLoginRequest(string email, string password) =>
        new(email, password);
}
