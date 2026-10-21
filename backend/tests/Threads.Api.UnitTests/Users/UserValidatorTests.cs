using FluentAssertions;
using FluentValidation.Results;
using Threads.Api.Features.Users.Endpoints;

namespace Threads.Api.UnitTests.Users;

public class UserValidatorTests
{
    [Fact]
    public void GetUserValidator_ShouldPass_WhenUserIdIsNotEmpty()
    {
        // Arrange
        var validator = new GetUser.GetUserValidator();
        var request = UserRequestFactory.CreateGetUserRequest();

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
        var request = UserRequestFactory.CreateGetUserRequest(userId: Guid.Empty);

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
        var request = UserRequestFactory.CreateDeleteUserRequest();

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
        var request = UserRequestFactory.CreateDeleteUserRequest(userId: Guid.Empty);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.UserId));
    }

    [Fact]
    public void SearchUsersValidator_ShouldPass_WhenUsernameIsValid()
    {
        // Arrange
        var validator = new SearchUsers.SearchUsersValidator();
        var request = UserRequestFactory.CreateSearchUsersRequest();

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("ab")]
    [InlineData("bad-name")]
    public void SearchUsersValidator_ShouldFail_WhenUsernameIsInvalid(string? username)
    {
        // Arrange
        var validator = new SearchUsers.SearchUsersValidator();
        var request = UserRequestFactory.CreateSearchUsersRequest(username);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.Username));
    }

    [Fact]
    public void SearchUsersValidator_ShouldFail_WhenUsernameIsTooLong()
    {
        // Arrange
        var validator = new SearchUsers.SearchUsersValidator();
        var request = UserRequestFactory.CreateSearchUsersRequest(new string('a', 31));

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.Username));
    }
}
