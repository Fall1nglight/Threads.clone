using FluentAssertions;
using FluentValidation.Results;
using Threads.Api.Features.Posts.Endpoints;

namespace Threads.Api.UnitTests.Posts;

public class PostValidatorTests
{
    private const string ValidContent = "Valid content";
    private const int MaxContentLength = 600;

    private static readonly Guid ValidPostId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public void CreatePostValidator_ShouldPass_WhenContentIsValid()
    {
        // Arrange
        var validator = new CreatePost.CreatePostValidator();
        var request = CreatePostRequest(content: ValidContent);

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
        var request = CreatePostRequest(content: new string('a', MaxContentLength));

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(EmptyContentValues))]
    public void CreatePostValidator_ShouldFail_WhenContentIsEmpty(string? content)
    {
        // Arrange
        var validator = new CreatePost.CreatePostValidator();
        var request = CreatePostRequest(content: content);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.Content));
    }

    [Theory]
    [MemberData(nameof(TooLongContentLengths))]
    public void CreatePostValidator_ShouldFail_WhenContentIsTooLong(int length)
    {
        // Arrange
        var validator = new CreatePost.CreatePostValidator();
        var request = CreatePostRequest(content: new string('a', length));

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
        var request = CreateUpdateRequest(postId: ValidPostId, content: ValidContent);

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
        var request = CreateUpdateRequest(
            postId: ValidPostId,
            content: new string('a', MaxContentLength)
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
        var request = CreateUpdateRequest(postId: Guid.Empty, content: ValidContent);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.PostId));
    }

    [Theory]
    [MemberData(nameof(EmptyContentValues))]
    public void UpdatePostValidator_ShouldFail_WhenContentIsEmpty(string? content)
    {
        // Arrange
        var validator = new UpdatePost.UpdatePostValidator();
        var request = CreateUpdateRequest(postId: ValidPostId, content: content);

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
    [MemberData(nameof(TooLongContentLengths))]
    public void UpdatePostValidator_ShouldFail_WhenContentIsTooLong(int length)
    {
        // Arrange
        var validator = new UpdatePost.UpdatePostValidator();
        var request = CreateUpdateRequest(postId: ValidPostId, content: new string('a', length));

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
        var request = CreateGetRequest(postId: ValidPostId);

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
        var request = CreateGetRequest(postId: Guid.Empty);

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
        var request = CreateDeleteRequest(postId: ValidPostId);

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
        var request = CreateDeleteRequest(postId: Guid.Empty);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.PostId));
    }

    public static TheoryData<string?> EmptyContentValues => [null, string.Empty, " "];

    public static TheoryData<int> TooLongContentLengths => [601, 666, 999];

    private static CreatePost.Request CreatePostRequest(string? content = ValidContent) =>
        new(content!);

    private static UpdatePost.Request CreateUpdateRequest(
        Guid? postId = null,
        string? content = ValidContent
    ) => new(postId ?? ValidPostId, CreateUpdateBody(content: content));

    private static UpdatePost.Body CreateUpdateBody(string? content = ValidContent) =>
        new(content!);

    private static GetPost.Request CreateGetRequest(Guid? postId = null) =>
        new(postId ?? ValidPostId);

    private static DeletePost.Request CreateDeleteRequest(Guid? postId = null) =>
        new(postId ?? ValidPostId);
}
