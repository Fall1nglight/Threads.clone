using FluentAssertions;
using FluentValidation.Results;
using Threads.Api.Features.Posts.Endpoints;
using Threads.Api.UnitTests.TestSupport;

namespace Threads.Api.UnitTests.Posts;

public class PostValidatorTests
{
    [Fact]
    public void CreatePostValidator_ShouldPass_WhenContentIsValid()
    {
        // Arrange
        var validator = new CreatePost.CreatePostValidator();
        var request = PostRequestFactory.CreatePostRequest();

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreatePostValidator_ShouldPass_WhenContentIsExactlyMaxLength()
    {
        // Arrange
        var validator = new CreatePost.CreatePostValidator();
        var request = PostRequestFactory.CreatePostRequest(
            content: new string('a', ValidatorTestData.MaxContentLength)
        );

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [MemberData(
        nameof(ValidatorTestData.EmptyContentValues),
        MemberType = typeof(ValidatorTestData)
    )]
    public void CreatePostValidator_ShouldFail_WhenContentIsEmpty(string? content)
    {
        // Arrange
        var validator = new CreatePost.CreatePostValidator();
        var request = PostRequestFactory.CreatePostRequest(content: content);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.Content));
    }

    [Theory]
    [MemberData(
        nameof(ValidatorTestData.TooLongContentLengths),
        MemberType = typeof(ValidatorTestData)
    )]
    public void CreatePostValidator_ShouldFail_WhenContentIsTooLong(int length)
    {
        // Arrange
        var validator = new CreatePost.CreatePostValidator();
        var request = PostRequestFactory.CreatePostRequest(content: new string('a', length));

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.Content));
    }

    [Fact]
    public void UpdatePostValidator_ShouldPass_WhenRequestIsValid()
    {
        // Arrange
        var validator = new UpdatePost.UpdatePostValidator();
        var request = PostRequestFactory.CreateUpdateRequest();

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpdatePostValidator_ShouldPass_WhenContentIsExactlyMaxLength()
    {
        // Arrange
        var validator = new UpdatePost.UpdatePostValidator();
        var request = PostRequestFactory.CreateUpdateRequest(
            content: new string('a', ValidatorTestData.MaxContentLength)
        );

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpdatePostValidator_ShouldFail_WhenPostIdIsEmpty()
    {
        // Arrange
        var validator = new UpdatePost.UpdatePostValidator();
        var request = PostRequestFactory.CreateUpdateRequest(postId: Guid.Empty);

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
    public void UpdatePostValidator_ShouldFail_WhenContentIsEmpty(string? content)
    {
        // Arrange
        var validator = new UpdatePost.UpdatePostValidator();
        var request = PostRequestFactory.CreateUpdateRequest(content: content);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(error =>
                error.PropertyName
                == $"{nameof(UpdatePost.Request.Body)}.{nameof(UpdatePost.Body.Content)}"
            );
    }

    [Theory]
    [MemberData(
        nameof(ValidatorTestData.TooLongContentLengths),
        MemberType = typeof(ValidatorTestData)
    )]
    public void UpdatePostValidator_ShouldFail_WhenContentIsTooLong(int length)
    {
        // Arrange
        var validator = new UpdatePost.UpdatePostValidator();
        var request = PostRequestFactory.CreateUpdateRequest(content: new string('a', length));

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(error =>
                error.PropertyName
                == $"{nameof(UpdatePost.Request.Body)}.{nameof(UpdatePost.Body.Content)}"
            );
    }

    [Fact]
    public void GetPostValidator_ShouldPass_WhenPostIdIsNotEmpty()
    {
        // Arrange
        var validator = new GetPost.GetPostValidator();
        var request = PostRequestFactory.CreateGetRequest();

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void GetPostValidator_ShouldFail_WhenPostIdIsEmpty()
    {
        // Arrange
        var validator = new GetPost.GetPostValidator();
        var request = PostRequestFactory.CreateGetRequest(postId: Guid.Empty);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.PostId));
    }

    [Fact]
    public void DeletePostValidator_ShouldPass_WhenPostIdIsNotEmpty()
    {
        // Arrange
        var validator = new DeletePost.DeletePostValidator();
        var request = PostRequestFactory.CreateDeleteRequest();

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void DeletePostValidator_ShouldFail_WhenPostIdIsEmpty()
    {
        // Arrange
        var validator = new DeletePost.DeletePostValidator();
        var request = PostRequestFactory.CreateDeleteRequest(postId: Guid.Empty);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.PostId));
    }

    [Fact]
    public void GetUserPostsValidator_ShouldPass_WhenUserIdIsNotEmpty()
    {
        // Arrange
        var validator = new GetUserPosts.GetUserPostsValidator();
        var request = PostRequestFactory.CreateGetUserPostsRequest();

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void GetUserPostsValidator_ShouldFail_WhenUserIdIsEmpty()
    {
        // Arrange
        var validator = new GetUserPosts.GetUserPostsValidator();
        var request = PostRequestFactory.CreateGetUserPostsRequest(userId: Guid.Empty);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.UserId));
    }
}
