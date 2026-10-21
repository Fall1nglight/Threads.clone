using FluentAssertions;
using FluentValidation.Results;
using Threads.Api.Features.Comments.Endpoints;
using Threads.Api.UnitTests.TestSupport;

namespace Threads.Api.UnitTests.Comments;

public class CommentValidatorTests
{
    [Fact]
    public void CreateCommentValidator_ShouldPass_WhenRequestIsValid()
    {
        // Arrange
        var validator = new CreateComment.CreateCommentValidator();
        var request = CommentRequestFactory.CreateCreateCommentRequest();

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateCommentValidator_ShouldPass_WhenContentIsExactlyMaxLength()
    {
        // Arrange
        var validator = new CreateComment.CreateCommentValidator();
        var request = CommentRequestFactory.CreateCreateCommentRequest(
            content: new string('a', ValidatorTestData.MaxContentLength)
        );

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateCommentValidator_ShouldFail_WhenPostIdIsEmpty()
    {
        // Arrange
        var validator = new CreateComment.CreateCommentValidator();
        var request = CommentRequestFactory.CreateCreateCommentRequest(postId: Guid.Empty);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.PostId));
    }

    [Theory]
    [MemberData(
        nameof(ValidatorTestData.EmptyContentValues),
        MemberType = typeof(ValidatorTestData)
    )]
    public void CreateCommentValidator_ShouldFail_WhenContentIsEmpty(string? content)
    {
        // Arrange
        var validator = new CreateComment.CreateCommentValidator();
        var request = CommentRequestFactory.CreateCreateCommentRequest(content: content);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(error => error.PropertyName == GetCreateCommentContentPropertyName());
    }

    [Theory]
    [MemberData(
        nameof(ValidatorTestData.TooLongContentLengths),
        MemberType = typeof(ValidatorTestData)
    )]
    public void CreateCommentValidator_ShouldFail_WhenContentIsTooLong(int length)
    {
        // Arrange
        var validator = new CreateComment.CreateCommentValidator();
        var request = CommentRequestFactory.CreateCreateCommentRequest(
            content: new string('a', length)
        );

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(error => error.PropertyName == GetCreateCommentContentPropertyName());
    }

    [Fact]
    public void UpdateCommentValidator_ShouldPass_WhenRequestIsValid()
    {
        // Arrange
        var validator = new UpdateComment.UpdateCommentValidator();
        var request = CommentRequestFactory.CreateUpdateCommentRequest();

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpdateCommentValidator_ShouldPass_WhenContentIsExactlyMaxLength()
    {
        // Arrange
        var validator = new UpdateComment.UpdateCommentValidator();
        var request = CommentRequestFactory.CreateUpdateCommentRequest(
            content: new string('a', ValidatorTestData.MaxContentLength)
        );

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpdateCommentValidator_ShouldFail_WhenPostIdIsEmpty()
    {
        // Arrange
        var validator = new UpdateComment.UpdateCommentValidator();
        var request = CommentRequestFactory.CreateUpdateCommentRequest(postId: Guid.Empty);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.PostId));
    }

    [Fact]
    public void UpdateCommentValidator_ShouldFail_WhenCommentIdIsEmpty()
    {
        // Arrange
        var validator = new UpdateComment.UpdateCommentValidator();
        var request = CommentRequestFactory.CreateUpdateCommentRequest(commentId: Guid.Empty);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.CommentId));
    }

    [Theory]
    [MemberData(
        nameof(ValidatorTestData.EmptyContentValues),
        MemberType = typeof(ValidatorTestData)
    )]
    public void UpdateCommentValidator_ShouldFail_WhenContentIsEmpty(string? content)
    {
        // Arrange
        var validator = new UpdateComment.UpdateCommentValidator();
        var request = CommentRequestFactory.CreateUpdateCommentRequest(content: content);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(error => error.PropertyName == GetUpdateCommentContentPropertyName());
    }

    [Theory]
    [MemberData(
        nameof(ValidatorTestData.TooLongContentLengths),
        MemberType = typeof(ValidatorTestData)
    )]
    public void UpdateCommentValidator_ShouldFail_WhenContentIsTooLong(int length)
    {
        // Arrange
        var validator = new UpdateComment.UpdateCommentValidator();
        var request = CommentRequestFactory.CreateUpdateCommentRequest(
            content: new string('a', length)
        );

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(error => error.PropertyName == GetUpdateCommentContentPropertyName());
    }

    [Fact]
    public void DeleteCommentValidator_ShouldPass_WhenRequestIsValid()
    {
        // Arrange
        var validator = new DeleteComment.DeleteCommentValidator();
        var request = CommentRequestFactory.CreateDeleteCommentRequest();

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void DeleteCommentValidator_ShouldFail_WhenPostIdIsEmpty()
    {
        // Arrange
        var validator = new DeleteComment.DeleteCommentValidator();
        var request = CommentRequestFactory.CreateDeleteCommentRequest(postId: Guid.Empty);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.PostId));
    }

    [Fact]
    public void DeleteCommentValidator_ShouldFail_WhenCommentIdIsEmpty()
    {
        // Arrange
        var validator = new DeleteComment.DeleteCommentValidator();
        var request = CommentRequestFactory.CreateDeleteCommentRequest(commentId: Guid.Empty);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.CommentId));
    }

    [Fact]
    public void GetCommentsValidator_ShouldPass_WhenPostIdIsNotEmpty()
    {
        // Arrange
        var validator = new GetComments.GetCommentsValidator();
        var request = CommentRequestFactory.CreateGetCommentsRequest();

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void GetCommentsValidator_ShouldFail_WhenPostIdIsEmpty()
    {
        // Arrange
        var validator = new GetComments.GetCommentsValidator();
        var request = CommentRequestFactory.CreateGetCommentsRequest(postId: Guid.Empty);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.PostId));
    }

    private static string GetCreateCommentContentPropertyName() =>
        $"{nameof(CreateComment.Request.Body)}.{nameof(CreateComment.Body.Content)}";

    private static string GetUpdateCommentContentPropertyName() =>
        $"{nameof(UpdateComment.Request.Body)}.{nameof(UpdateComment.Body.Content)}";
}
