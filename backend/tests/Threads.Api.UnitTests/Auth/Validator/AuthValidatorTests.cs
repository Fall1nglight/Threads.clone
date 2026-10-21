using FluentAssertions;
using FluentValidation.Results;
using Threads.Api.Features.Auth.Endpoints;

namespace Threads.Api.UnitTests.Auth.Validator;

public class AuthValidatorTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SignupValidator_ShouldPass_WhenRequestIsValid(bool isPrivate)
    {
        // Arrange
        var validator = new Signup.SignupValidator();
        var request = AuthRequestFactory.CreateSignupRequest(
            isPrivate: isPrivate,
            bio: new string('a', 300)
        );

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [MemberData(
        nameof(AuthValidatorTestData.InvalidSignupRequests),
        MemberType = typeof(AuthValidatorTestData)
    )]
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
        var request = AuthRequestFactory.CreateLoginRequest();

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [MemberData(
        nameof(AuthValidatorTestData.InvalidLoginRequests),
        MemberType = typeof(AuthValidatorTestData)
    )]
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
        var request = AuthRequestFactory.CreateRenewRefreshTokenRequest();

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [MemberData(
        nameof(AuthValidatorTestData.InvalidRefreshTokenRequests),
        MemberType = typeof(AuthValidatorTestData)
    )]
    public void RenewTokenValidator_ShouldFail_WhenRefreshTokenIsInvalid(string? refreshToken)
    {
        // Arrange
        var validator = new RenewToken.RenewTokenValidator();
        var request = AuthRequestFactory.CreateRenewRefreshTokenRequest(refreshToken);

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
        var request = AuthRequestFactory.CreateLogoutRequest();

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [MemberData(
        nameof(AuthValidatorTestData.InvalidRefreshTokenRequests),
        MemberType = typeof(AuthValidatorTestData)
    )]
    public void LogoutValidator_ShouldFail_WhenRefreshTokenIsInvalid(string? refreshToken)
    {
        // Arrange
        var validator = new Logout.LogoutValidator();
        var request = AuthRequestFactory.CreateLogoutRequest(plainToken: refreshToken);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(request.RefreshToken));
    }
}
