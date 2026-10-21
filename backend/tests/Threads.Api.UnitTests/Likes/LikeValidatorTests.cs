using FluentAssertions;
using FluentValidation.Results;
using Threads.Api.Features.Likes.Endpoints;

namespace Threads.Api.UnitTests.Likes;

public class LikeValidatorTests
{
    [Fact]
    public void LikePostValidator_ShouldPass_WhenPostIdIsNotEmpty()
    {
        // Arrange
        var validator = new LikePost.LikePostValidator();
        var request = LikeRequestFactory.CreateLikePostRequest();

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void LikePostValidator_ShouldFail_WhenPostIdIsEmpty()
    {
        // Arrange
        var validator = new LikePost.LikePostValidator();
        var request = LikeRequestFactory.CreateLikePostRequest(postId: Guid.Empty);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.PostId));
    }

    [Fact]
    public void UnlikePostValidator_ShouldPass_WhenPostIdIsNotEmpty()
    {
        // Arrange
        var validator = new UnlikePost.UnlikePostValidator();
        var request = LikeRequestFactory.CreateUnlikePostRequest();

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UnlikePostValidator_ShouldFail_WhenPostIdIsEmpty()
    {
        // Arrange
        var validator = new UnlikePost.UnlikePostValidator();
        var request = LikeRequestFactory.CreateUnlikePostRequest(postId: Guid.Empty);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.PostId));
    }

    [Fact]
    public void GetLikesValidator_ShouldPass_WhenPostIdIsNotEmpty()
    {
        // Arrange
        var validator = new GetLikes.GetLikesValidator();
        var request = LikeRequestFactory.CreateGetLikesRequest();

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void GetLikesValidator_ShouldFail_WhenPostIdIsEmpty()
    {
        // Arrange
        var validator = new GetLikes.GetLikesValidator();
        var request = LikeRequestFactory.CreateGetLikesRequest(postId: Guid.Empty);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.PostId));
    }
}
