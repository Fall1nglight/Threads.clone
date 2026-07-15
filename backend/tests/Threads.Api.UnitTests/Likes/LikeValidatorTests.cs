using FluentAssertions;
using FluentValidation.Results;
using Threads.Api.Features.Likes.Endpoints;
using Threads.Api.UnitTests.TestSupport;

namespace Threads.Api.UnitTests.Likes;

public class LikeValidatorTests
{
    [Fact]
    public void LikePostValidator_ShouldPass_WhenPostIdIsNotEmpty()
    {
        // Arrange
        var validator = new LikePost.LikePostValidator();
        var request = CreateLikePostRequest(postId: ValidatorTestData.PrimaryId);

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
        var request = CreateLikePostRequest(postId: Guid.Empty);

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
        var request = CreateUnlikePostRequest(postId: ValidatorTestData.PrimaryId);

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
        var request = CreateUnlikePostRequest(postId: Guid.Empty);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.PostId));
    }

    [Fact]
    public void GetPostLikesValidator_ShouldPass_WhenPostIdIsNotEmpty()
    {
        // Arrange
        var validator = new GetPostLikes.GetPostLikesValidator();
        var request = CreateGetPostLikesRequest(postId: ValidatorTestData.PrimaryId);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void GetPostLikesValidator_ShouldFail_WhenPostIdIsEmpty()
    {
        // Arrange
        var validator = new GetPostLikes.GetPostLikesValidator();
        var request = CreateGetPostLikesRequest(postId: Guid.Empty);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.PostId));
    }

    private static LikePost.Request CreateLikePostRequest(Guid? postId = null) =>
        new(postId ?? ValidatorTestData.PrimaryId);

    private static UnlikePost.Request CreateUnlikePostRequest(Guid? postId = null) =>
        new(postId ?? ValidatorTestData.PrimaryId);

    private static GetPostLikes.Request CreateGetPostLikesRequest(Guid? postId = null) =>
        new(postId ?? ValidatorTestData.PrimaryId, Cursor: null);
}
