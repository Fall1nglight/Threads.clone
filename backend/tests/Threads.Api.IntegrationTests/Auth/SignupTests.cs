using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Threads.Api.Features.Auth.Endpoints;
using Threads.Api.Features.Auth.Services.RefreshToken;
using Threads.Api.IntegrationTests.Auth.Infrastructure;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Auth;

public class SignupTests : AuthIntegrationTestBase
{
    public SignupTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task Signup_ShouldCreateUserAndRefreshToken_WhenRequestIsValid()
    {
        // Arrange
        var before = DateTime.UtcNow;
        var client = CreateAnonymousClient();
        var requestBody = CreateSignupRequest(
            username: AuthTestData.AliceUsername,
            email: AuthTestData.AliceEmail,
            isPrivate: true,
            bio: AuthTestData.AliceBio
        );

        // Act
        var response = await client.PostAsJsonAsync(AuthTestRoutes.Signup, requestBody);
        var after = DateTime.UtcNow;

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<Signup.Response>(response);

        responseBody.AccessToken.Should().NotBeNullOrWhiteSpace();
        responseBody.RefreshToken.Should().NotBeNullOrWhiteSpace();
        responseBody.RefreshToken.Should().HaveLength(44);

        Db.ChangeTracker.Clear();

        var persistedUser = await Db.Users.SingleAsync(u => u.Email == requestBody.Email);

        persistedUser.UserName.Should().Be(requestBody.Username);
        persistedUser.Bio.Should().Be(requestBody.Bio);
        persistedUser.IsPrivate.Should().BeTrue();
        persistedUser.CreatedAtUtc.Should().BeOnOrAfter(before);
        persistedUser.CreatedAtUtc.Should().BeOnOrBefore(after);
        persistedUser.UpdatedAtUtc.Should().BeNull();

        var refreshTokenProvider = Services.GetRequiredService<IRefreshTokenProvider>();
        var refreshTokenHash = refreshTokenProvider.Hash(responseBody.RefreshToken);
        var persistedToken = await Db.RefreshTokens.SingleAsync(rt =>
            rt.UserId == persistedUser.Id
        );

        persistedToken.TokenHash.Should().Be(refreshTokenHash);
        persistedToken.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Signup_ShouldReturnAccessTokenThatCanAuthorizeProtectedAuthEndpoint()
    {
        // Arrange
        var client = CreateAnonymousClient();
        var requestBody = CreateSignupRequest(
            username: AuthTestData.AliceUsername,
            email: AuthTestData.AliceEmail
        );

        // Act
        var signupResponse = await client.PostAsJsonAsync(AuthTestRoutes.Signup, requestBody);

        // Assert
        signupResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await ReadJsonAsync<Signup.Response>(signupResponse);
        var authorizedClient = CreateAuthorizedClientWithBearerToken(responseBody.AccessToken);

        var response = await authorizedClient.PostAsync(AuthTestRoutes.LogoutAll, null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Signup_ShouldReturnBadRequest_WhenEmailAlreadyExists()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var alice = await userSeeder.CreateUserAsync(
            username: AuthTestData.AliceUsername,
            email: AuthTestData.AliceEmail
        );
        var client = CreateAnonymousClient();
        var requestBody = CreateSignupRequest(email: alice.Email!);

        // Act
        var response = await client.PostAsJsonAsync(AuthTestRoutes.Signup, requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await ReadJsonAsync<ProblemDetails>(response);

        problem.Title.Should().Be("Signup failed");
        problem.Detail.Should().Be("The provided username or email is already in use");

        (await Db.Users.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Signup_ShouldReturnBadRequest_WhenUsernameAlreadyExists()
    {
        // Arrange
        var userSeeder = CreateUserSeeder();
        var alice = await userSeeder.CreateUserAsync(username: AuthTestData.AliceUsername);
        var client = CreateAnonymousClient();
        var requestBody = CreateSignupRequest(username: alice.UserName!);

        // Act
        var response = await client.PostAsJsonAsync(AuthTestRoutes.Signup, requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await ReadJsonAsync<ProblemDetails>(response);

        problem.Title.Should().Be("Signup failed");
        problem.Detail.Should().Be("The provided username or email is already in use");

        (await Db.Users.CountAsync()).Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(InvalidSignupRequests))]
    public async Task Signup_ShouldReturnBadRequest_WhenRequestIsInvalid(Signup.Request request)
    {
        // Arrange
        var client = CreateAnonymousClient();

        // Act
        var response = await client.PostAsJsonAsync(AuthTestRoutes.Signup, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        (await Db.Users.CountAsync()).Should().Be(0);
        (await Db.RefreshTokens.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Signup_ShouldReturnBadRequest_WhenJsonContainsUnknownProperty()
    {
        // Arrange
        var client = CreateAnonymousClient();

        // Act
        var response = await client.PostAsJsonAsync(
            AuthTestRoutes.Signup,
            new
            {
                email = AuthTestData.AliceEmail,
                username = AuthTestData.AliceUsername,
                password = AuthTestData.ValidPassword,
                isPrivate = false,
                bio = "Hello",
                unexpected = true,
            }
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Db.Users.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Signup_ShouldReturnBadRequest_WhenJsonIsMalformed()
    {
        // Arrange
        var client = CreateAnonymousClient();
        using var content = new StringContent(
            "{\"email\":\"alice@example.test\",\"username\":\"alice\"",
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await client.PostAsync(AuthTestRoutes.Signup, content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Db.Users.CountAsync()).Should().Be(0);
    }

    public static TheoryData<Signup.Request> InvalidSignupRequests =>
        [
            CreateSignupRequest(email: ""),
            CreateSignupRequest(email: "invalid-email"),
            CreateSignupRequest(email: $"{new string('a', 257)}@example.test"),
            CreateSignupRequest(username: ""),
            CreateSignupRequest(username: "ab"),
            CreateSignupRequest(username: new string('a', 31)),
            CreateSignupRequest(username: "bad-name"),
            CreateSignupRequest(password: ""),
            CreateSignupRequest(password: "Short1!"),
            CreateSignupRequest(password: "test123!"),
            CreateSignupRequest(password: "TEST123!"),
            CreateSignupRequest(password: "TestTest!"),
            CreateSignupRequest(password: "Test1234"),
            CreateSignupRequest(bio: new string('a', 301)),
        ];

    private static Signup.Request CreateSignupRequest(
        string email = AuthTestData.AliceEmail,
        string username = AuthTestData.AliceUsername,
        string password = AuthTestData.ValidPassword,
        bool isPrivate = false,
        string? bio = null
    )
    {
        return new Signup.Request(email, username, password, isPrivate, bio);
    }
}
