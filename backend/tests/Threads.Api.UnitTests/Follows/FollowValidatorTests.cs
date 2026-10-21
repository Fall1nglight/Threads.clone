using FluentAssertions;
using FluentValidation.Results;
using Threads.Api.Features.Follows.Endpoints;
using Threads.Api.Features.Follows.Endpoints.Incoming;
using Threads.Api.Features.Follows.Endpoints.Outgoing;

namespace Threads.Api.UnitTests.Follows;

public class FollowValidatorTests
{
    [Fact]
    public void SendFollowRequestValidator_ShouldPass_WhenTargetUserIdIsNotEmpty()
    {
        // Arrange
        var validator = new SendFollowRequest.SendFollowRequestValidator();
        var request = FollowRequestFactory.CreateSendFollowRequest();

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void SendFollowRequestValidator_ShouldFail_WhenTargetUserIdIsEmpty()
    {
        // Arrange
        var validator = new SendFollowRequest.SendFollowRequestValidator();
        var request = FollowRequestFactory.CreateSendFollowRequest(targetUserId: Guid.Empty);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.TargetUserId));
    }

    [Fact]
    public void AcceptIncomingFollowRequestValidator_ShouldPass_WhenFollowerIdIsNotEmpty()
    {
        // Arrange
        var validator = new AcceptIncomingFollowRequest.AcceptIncomingFollowRequestValidator();
        var request = FollowRequestFactory.CreateAcceptIncomingFollowRequest();

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void AcceptIncomingFollowRequestValidator_ShouldFail_WhenFollowerIdIsEmpty()
    {
        // Arrange
        var validator = new AcceptIncomingFollowRequest.AcceptIncomingFollowRequestValidator();
        var request = FollowRequestFactory.CreateAcceptIncomingFollowRequest(
            followerId: Guid.Empty
        );

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.FollowerId));
    }

    [Fact]
    public void RejectIncomingFollowRequestValidator_ShouldPass_WhenFollowerIdIsNotEmpty()
    {
        // Arrange
        var validator = new RejectIncomingFollowRequest.RejectIncomingFollowRequestValidator();
        var request = FollowRequestFactory.CreateRejectIncomingFollowRequest();

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void RejectIncomingFollowRequestValidator_ShouldFail_WhenFollowerIdIsEmpty()
    {
        // Arrange
        var validator = new RejectIncomingFollowRequest.RejectIncomingFollowRequestValidator();
        var request = FollowRequestFactory.CreateRejectIncomingFollowRequest(
            followerId: Guid.Empty
        );

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.FollowerId));
    }

    [Fact]
    public void DeleteFollowingValidator_ShouldPass_WhenFollowedIdIsNotEmpty()
    {
        // Arrange
        var validator = new DeleteFollowing.DeleteFollowingValidator();
        var request = FollowRequestFactory.CreateDeleteFollowingRequest();

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void DeleteFollowingValidator_ShouldFail_WhenFollowedIdIsEmpty()
    {
        // Arrange
        var validator = new DeleteFollowing.DeleteFollowingValidator();
        var request = FollowRequestFactory.CreateDeleteFollowingRequest(followedId: Guid.Empty);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.FollowedId));
    }

    [Fact]
    public void DeleteFollowerValidator_ShouldPass_WhenFollowerIdIsNotEmpty()
    {
        // Arrange
        var validator = new DeleteFollower.DeleteFollowerValidator();
        var request = FollowRequestFactory.CreateDeleteFollowerRequest();

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void DeleteFollowerValidator_ShouldFail_WhenFollowerIdIsEmpty()
    {
        // Arrange
        var validator = new DeleteFollower.DeleteFollowerValidator();
        var request = FollowRequestFactory.CreateDeleteFollowerRequest(followerId: Guid.Empty);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.FollowerId));
    }

    [Fact]
    public void GetFollowersValidator_ShouldPass_WhenUserIdIsNotEmpty()
    {
        // Arrange
        var validator = new GetFollowers.GetFollowersValidator();
        var request = FollowRequestFactory.CreateGetFollowersRequest();

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void GetFollowersValidator_ShouldFail_WhenUserIdIsEmpty()
    {
        // Arrange
        var validator = new GetFollowers.GetFollowersValidator();
        var request = FollowRequestFactory.CreateGetFollowersRequest(userId: Guid.Empty);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.UserId));
    }

    [Fact]
    public void GetFollowingValidator_ShouldPass_WhenUserIdIsNotEmpty()
    {
        // Arrange
        var validator = new GetFollowing.GetFollowingValidator();
        var request = FollowRequestFactory.CreateGetFollowingRequest();

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void GetFollowingValidator_ShouldFail_WhenUserIdIsEmpty()
    {
        // Arrange
        var validator = new GetFollowing.GetFollowingValidator();
        var request = FollowRequestFactory.CreateGetFollowingRequest(userId: Guid.Empty);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.UserId));
    }

    [Fact]
    public void CancelOutgoingFollowRequestValidator_ShouldPass_WhenFollowedIdIsNotEmpty()
    {
        // Arrange
        var validator = new CancelOutgoingFollowRequest.CancelOutgoingFollowRequestValidator();
        var request = FollowRequestFactory.CreateCancelOutgoingFollowRequest();

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CancelOutgoingFollowRequestValidator_ShouldFail_WhenFollowedIdIsEmpty()
    {
        // Arrange
        var validator = new CancelOutgoingFollowRequest.CancelOutgoingFollowRequestValidator();
        var request = FollowRequestFactory.CreateCancelOutgoingFollowRequest(
            followedId: Guid.Empty
        );

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.FollowedId));
    }
}
