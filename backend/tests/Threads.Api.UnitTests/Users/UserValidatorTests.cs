using FluentAssertions;
using FluentValidation.Results;
using Threads.Api.Features.Users.Endpoints;
using Threads.Api.UnitTests.TestSupport;

namespace Threads.Api.UnitTests.Users;

public class UserValidatorTests
{
    [Fact]
    public void GetUserValidator_ShouldPass_WhenUserIdIsNotEmpty()
    {
        // Arrange
        var validator = new GetUser.GetUserValidator();
        var request = CreateGetUserRequest(userId: ValidatorTestData.PrimaryId);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void GetUserValidator_ShouldFail_WhenUserIdIsEmpty()
    {
        // Arrange
        var validator = new GetUser.GetUserValidator();
        var request = CreateGetUserRequest(userId: Guid.Empty);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.UserId));
    }

    [Fact]
    public void DeleteUserValidator_ShouldPass_WhenUserIdIsNotEmpty()
    {
        // Arrange
        var validator = new DeleteUser.DeleteUserValidator();
        var request = CreateDeleteUserRequest(userId: ValidatorTestData.PrimaryId);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void DeleteUserValidator_ShouldFail_WhenUserIdIsEmpty()
    {
        // Arrange
        var validator = new DeleteUser.DeleteUserValidator();
        var request = CreateDeleteUserRequest(userId: Guid.Empty);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.UserId));
    }

    private static GetUser.Request CreateGetUserRequest(Guid? userId = null) =>
        new(userId ?? ValidatorTestData.PrimaryId);

    private static DeleteUser.Request CreateDeleteUserRequest(Guid? userId = null) =>
        new(userId ?? ValidatorTestData.PrimaryId);
}
