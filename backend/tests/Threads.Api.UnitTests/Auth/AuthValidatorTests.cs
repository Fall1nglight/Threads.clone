using FluentAssertions;
using FluentValidation.Results;
using Threads.Api.Features.Auth.Endpoints;

namespace Threads.Api.UnitTests.Auth;

public class AuthValidatorTests
{
    private const string ValidEmail = "alice@example.test";
    private const string ValidUsername = "alice";
    private const string ValidPassword = "Test123!";
    private const string ValidRefreshToken = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SignupValidator_ShouldPass_WhenRequestIsValid(bool isPrivate)
    {
        // Arrange
        var validator = new Signup.SignupValidator();
        var request = CreateSignupRequest(isPrivate: isPrivate, bio: new string('a', 300));

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(InvalidSignupRequests))]
    public void SignupValidator_ShouldFail_WhenRequestIsInvalid(
        Signup.Request request,
        string propertyName
    )
    {
        // Arrange
        var validator = new Signup.SignupValidator();

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == propertyName);
    }

    [Fact]
    public void LoginValidator_ShouldPass_WhenRequestIsValid()
    {
        // Arrange
        var validator = new Login.LoginValidator();
        var request = CreateLoginRequest(email: ValidEmail, password: ValidPassword);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(InvalidLoginRequests))]
    public void LoginValidator_ShouldFail_WhenRequestIsInvalid(
        Login.Request request,
        string propertyName
    )
    {
        // Arrange
        var validator = new Login.LoginValidator();

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == propertyName);
    }

    [Fact]
    public void RenewTokenValidator_ShouldPass_WhenRefreshTokenIsValid()
    {
        // Arrange
        var validator = new RenewToken.RenewTokenValidator();
        var request = CreateRenewRefreshTokenRequest(plainToken: ValidRefreshToken);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(InvalidRefreshTokenRequests))]
    public void RenewTokenValidator_ShouldFail_WhenRefreshTokenIsInvalid(string? refreshToken)
    {
        // Arrange
        var validator = new RenewToken.RenewTokenValidator();
        var request = CreateRenewRefreshTokenRequest(refreshToken);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(request.RefreshToken));
    }

    [Fact]
    public void LogoutValidator_ShouldPass_WhenRefreshTokenIsValid()
    {
        // Arrange
        var validator = new Logout.LogoutValidator();
        var request = CreateLogoutRequest(plainToken: ValidRefreshToken);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(InvalidRefreshTokenRequests))]
    public void LogoutValidator_ShouldFail_WhenRefreshTokenIsInvalid(string? refreshToken)
    {
        // Arrange
        var validator = new Logout.LogoutValidator();
        var request = CreateLogoutRequest(plainToken: refreshToken);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(request.RefreshToken));
    }

    public static TheoryData<Signup.Request, string> InvalidSignupRequests =>
        new()
        {
            { CreateSignupRequest(email: null), nameof(Signup.Request.Email) },
            { CreateSignupRequest(email: ""), nameof(Signup.Request.Email) },
            { CreateSignupRequest(email: "invalid-email"), nameof(Signup.Request.Email) },
            {
                CreateSignupRequest(email: $"{new string('a', 257)}@example.test"),
                nameof(Signup.Request.Email)
            },
            { CreateSignupRequest(username: null), nameof(Signup.Request.Username) },
            { CreateSignupRequest(username: ""), nameof(Signup.Request.Username) },
            { CreateSignupRequest(username: "ab"), nameof(Signup.Request.Username) },
            { CreateSignupRequest(username: new string('a', 31)), nameof(Signup.Request.Username) },
            { CreateSignupRequest(username: "bad-name"), nameof(Signup.Request.Username) },
            { CreateSignupRequest(password: null), nameof(Signup.Request.Password) },
            { CreateSignupRequest(password: ""), nameof(Signup.Request.Password) },
            { CreateSignupRequest(password: "Short1!"), nameof(Signup.Request.Password) },
            { CreateSignupRequest(password: "test123!"), nameof(Signup.Request.Password) },
            { CreateSignupRequest(password: "TEST123!"), nameof(Signup.Request.Password) },
            { CreateSignupRequest(password: "TestTest!"), nameof(Signup.Request.Password) },
            { CreateSignupRequest(password: "Test1234"), nameof(Signup.Request.Password) },
            { CreateSignupRequest(bio: new string('a', 301)), nameof(Signup.Request.Bio) },
        };

    public static TheoryData<Login.Request, string> InvalidLoginRequests =>
        new()
        {
            { CreateLoginRequest(email: null), nameof(Login.Request.Email) },
            { CreateLoginRequest(email: ""), nameof(Login.Request.Email) },
            { CreateLoginRequest(email: "invalid-email"), nameof(Login.Request.Email) },
            {
                CreateLoginRequest(email: $"{new string('a', 257)}@example.test"),
                nameof(Login.Request.Email)
            },
            {
                CreateLoginRequest(email: "alice@example.test", password: null!),
                nameof(Login.Request.Password)
            },
            {
                CreateLoginRequest(email: "alice@example.test", password: ""),
                nameof(Login.Request.Password)
            },
            {
                CreateLoginRequest(email: "alice@example.test", password: "Short1!"),
                nameof(Login.Request.Password)
            },
            {
                CreateLoginRequest(email: "alice@example.test", password: "test123!"),
                nameof(Login.Request.Password)
            },
            {
                CreateLoginRequest(email: "alice@example.test", password: "TEST123!"),
                nameof(Login.Request.Password)
            },
            {
                CreateLoginRequest(email: "alice@example.test", password: "TestTest!"),
                nameof(Login.Request.Password)
            },
            {
                CreateLoginRequest(email: "alice@example.test", password: "Test1234"),
                nameof(Login.Request.Password)
            },
        };

    public static TheoryData<string?> InvalidRefreshTokenRequests =>
        [null, "", "short", new('a', 43), new('a', 45)];

    private static Login.Request CreateLoginRequest(
        string? email = ValidEmail,
        string? password = ValidPassword
    ) => new(email!, password!);

    private static Signup.Request CreateSignupRequest(
        string? email = ValidEmail,
        string? username = ValidUsername,
        string? password = ValidPassword,
        bool isPrivate = false,
        string? bio = null
    ) => new(email!, username!, password!, isPrivate, bio);

    private static RenewToken.Request CreateRenewRefreshTokenRequest(
        string? plainToken = ValidRefreshToken
    ) => new(plainToken!);

    private static Logout.Request CreateLogoutRequest(string? plainToken = ValidRefreshToken) =>
        new(plainToken!);
}
