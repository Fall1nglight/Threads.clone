using FluentAssertions;
using FluentValidation.Results;
using Threads.Api.Features.Blocks.Endpoints;
using Threads.Api.UnitTests.TestSupport;

namespace Threads.Api.UnitTests.Blocks;

public class BlockValidatorTests
{
    [Fact]
    public void BlockUserValidator_ShouldPass_WhenTargetUserIdIsNotEmpty()
    {
        // Arrange
        var validator = new BlockUser.BlockUserValidator();
        var request = CreateBlockUserRequest(targetUserId: ValidatorTestData.PrimaryId);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void BlockUserValidator_ShouldFail_WhenTargetUserIdIsEmpty()
    {
        // Arrange
        var validator = new BlockUser.BlockUserValidator();
        var request = CreateBlockUserRequest(targetUserId: Guid.Empty);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.TargetUserId));
    }

    [Fact]
    public void UnblockUserValidator_ShouldPass_WhenTargetUserIdIsNotEmpty()
    {
        // Arrange
        var validator = new UnblockUser.UnblockUserValidator();
        var request = CreateUnblockUserRequest(targetUserId: ValidatorTestData.PrimaryId);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UnblockUserValidator_ShouldFail_WhenTargetUserIdIsEmpty()
    {
        // Arrange
        var validator = new UnblockUser.UnblockUserValidator();
        var request = CreateUnblockUserRequest(targetUserId: Guid.Empty);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.TargetUserId));
    }

    private static BlockUser.Request CreateBlockUserRequest(Guid? targetUserId = null) =>
        new(targetUserId ?? ValidatorTestData.PrimaryId);

    private static UnblockUser.Request CreateUnblockUserRequest(Guid? targetUserId = null) =>
        new(targetUserId ?? ValidatorTestData.PrimaryId);
}
