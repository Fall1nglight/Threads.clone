using FluentAssertions;
using FluentValidation.Results;
using Threads.Api.Features.Follows.Endpoints;
using Threads.Api.UnitTests.TestSupport;

namespace Threads.Api.UnitTests.Follows;

public class FollowValidatorTests
{
    [Fact]
    public void SendFollowRequestValidator_ShouldPass_WhenTargetUserIdIsNotEmpty()
    {
        // Arrange
        var validator = new SendFollowRequest.SendFollowRequestValidator();
        var request = CreateSendFollowRequest(targetUserId: ValidatorTestData.PrimaryId);

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
        var request = CreateSendFollowRequest(targetUserId: Guid.Empty);

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
        var request = CreateAcceptIncomingFollowRequest(followerId: ValidatorTestData.PrimaryId);

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
        var request = CreateAcceptIncomingFollowRequest(followerId: Guid.Empty);

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
        var request = CreateRejectIncomingFollowRequest(followerId: ValidatorTestData.PrimaryId);

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
        var request = CreateRejectIncomingFollowRequest(followerId: Guid.Empty);

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
        var request = CreateDeleteFollowingRequest(followedId: ValidatorTestData.PrimaryId);

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
        var request = CreateDeleteFollowingRequest(followedId: Guid.Empty);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.FollowedId));
    }

    [Fact]
    public void RemoveFollowerValidator_ShouldPass_WhenFollowerIdIsNotEmpty()
    {
        // Arrange
        var validator = new RemoveFollower.RemoveFollowerValidator();
        var request = CreateRemoveFollowerRequest(followerId: ValidatorTestData.PrimaryId);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void RemoveFollowerValidator_ShouldFail_WhenFollowerIdIsEmpty()
    {
        // Arrange
        var validator = new RemoveFollower.RemoveFollowerValidator();
        var request = CreateRemoveFollowerRequest(followerId: Guid.Empty);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.FollowerId));
    }

    private static SendFollowRequest.Request CreateSendFollowRequest(Guid? targetUserId = null) =>
        new(targetUserId ?? ValidatorTestData.PrimaryId);

    private static AcceptIncomingFollowRequest.Request CreateAcceptIncomingFollowRequest(
        Guid? followerId = null
    ) => new(followerId ?? ValidatorTestData.PrimaryId);

    private static RejectIncomingFollowRequest.Request CreateRejectIncomingFollowRequest(
        Guid? followerId = null
    ) => new(followerId ?? ValidatorTestData.PrimaryId);

    private static DeleteFollowing.Request CreateDeleteFollowingRequest(Guid? followedId = null) =>
        new(followedId ?? ValidatorTestData.PrimaryId);

    private static RemoveFollower.Request CreateRemoveFollowerRequest(Guid? followerId = null) =>
        new(followerId ?? ValidatorTestData.PrimaryId);
}
