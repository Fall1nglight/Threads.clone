using FluentAssertions;
using Threads.Api.Features.Posts.Endpoints;

namespace Threads.Api.UnitTests.Posts;

public class PostValidatorTests
{
    [Fact]
    public void CreatePostValidator_ShouldPass_WhenContentIsValid()
    {
        var validator = new CreatePost.CreatePostValidator();
        var request = new CreatePost.Request("Valid content");

        var result = validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreatePostValidator_ShouldPass_WhenContentIsExactlyMaxLength()
    {
        var validator = new CreatePost.CreatePostValidator();
        var request = new CreatePost.Request(new string('a', 600));

        var result = validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void CreatePostValidator_ShouldFail_WhenContentIsEmpty(string? content)
    {
        var validator = new CreatePost.CreatePostValidator();
        var request = new CreatePost.Request(content);

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.Content));
    }

    [Theory]
    [InlineData(601)]
    [InlineData(666)]
    [InlineData(999)]
    public void CreatePostValidator_ShouldFail_WhenContentIsTooLong(int length)
    {
        var validator = new CreatePost.CreatePostValidator();
        var request = new CreatePost.Request(new string('a', length));

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.Content));
    }

    [Fact]
    public void UpdatePostValidator_ShouldPass_WhenRequestIsValid()
    {
        var validator = new UpdatePost.UpdatePostValidator();
        var request = new UpdatePost.Request(
            Guid.NewGuid(),
            new UpdatePost.Body("Updated content")
        );

        var result = validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpdatePostValidator_ShouldPass_WhenContentIsExactlyMaxLength()
    {
        var validator = new UpdatePost.UpdatePostValidator();
        var request = new UpdatePost.Request(
            Guid.NewGuid(),
            new UpdatePost.Body(new string('a', 600))
        );

        var result = validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpdatePostValidator_ShouldFail_WhenIdIsEmpty()
    {
        var validator = new UpdatePost.UpdatePostValidator();
        var request = new UpdatePost.Request(Guid.Empty, new UpdatePost.Body("Updated content"));

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.Id));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void UpdatePostValidator_ShouldFail_WhenContentIsEmpty(string? content)
    {
        var validator = new UpdatePost.UpdatePostValidator();
        var request = new UpdatePost.Request(Guid.NewGuid(), new UpdatePost.Body(content));

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(error =>
                error.PropertyName
                == $"{nameof(UpdatePost.Request.Body)}.{nameof(UpdatePost.Body.Content)}"
            );
    }

    [Theory]
    [InlineData(601)]
    [InlineData(666)]
    [InlineData(999)]
    public void UpdatePostValidator_ShouldFail_WhenContentIsTooLong(int length)
    {
        var validator = new UpdatePost.UpdatePostValidator();
        var request = new UpdatePost.Request(
            Guid.NewGuid(),
            new UpdatePost.Body(new string('a', length))
        );

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(error =>
                error.PropertyName
                == $"{nameof(UpdatePost.Request.Body)}.{nameof(UpdatePost.Body.Content)}"
            );
    }

    [Fact]
    public void GetPostValidator_ShouldPass_WhenIdIsNotEmpty()
    {
        var validator = new GetPost.GetPostValidator();
        var request = new GetPost.Request(Guid.NewGuid());

        var result = validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void GetPostValidator_ShouldFail_WhenIdIsEmpty()
    {
        var validator = new GetPost.GetPostValidator();
        var request = new GetPost.Request(Guid.Empty);

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.Id));
    }

    [Fact]
    public void DeletePostValidator_ShouldPass_WhenIdIsNotEmpty()
    {
        var validator = new DeletePost.DeletePostValidator();
        var request = new DeletePost.Request(Guid.NewGuid());

        var result = validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void DeletePostValidator_ShouldFail_WhenIdIsEmpty()
    {
        var validator = new DeletePost.DeletePostValidator();
        var request = new DeletePost.Request(Guid.Empty);

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.Id));
    }
}
