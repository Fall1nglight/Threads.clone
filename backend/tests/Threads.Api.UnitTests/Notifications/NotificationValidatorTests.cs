using FluentAssertions;
using FluentValidation.Results;
using Threads.Api.Features.Notifications;
using Threads.Api.Features.Notifications.Endpoints;

namespace Threads.Api.UnitTests.Notifications;

public class NotificationValidatorTests
{
    [Fact]
    public void GetNotificationsValidator_ShouldPass_WhenFiltersAreValid()
    {
        // Arrange
        var validator = new GetNotifications.GetNotificationsValidator();
        var request = NotificationRequestFactory.CreateGetNotificationsRequest(
            NotificationCategory.Follow,
            NotificationStatus.Unseen
        );

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void GetNotificationsValidator_ShouldFail_WhenCategoryIsUndefined()
    {
        // Arrange
        var validator = new GetNotifications.GetNotificationsValidator();
        var request = NotificationRequestFactory.CreateGetNotificationsRequest(
            category: (NotificationCategory)int.MaxValue
        );

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.Category));
    }

    [Fact]
    public void GetNotificationsValidator_ShouldFail_WhenStatusIsUndefined()
    {
        // Arrange
        var validator = new GetNotifications.GetNotificationsValidator();
        var request = NotificationRequestFactory.CreateGetNotificationsRequest(
            status: (NotificationStatus)int.MaxValue
        );

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(request.Status));
    }

    [Fact]
    public void MarkNotificationAsSeenValidator_ShouldPass_WhenNotificationIdIsNotEmpty()
    {
        // Arrange
        var validator = new MarkNotificationAsSeen.MarkNotificationAsSeenValidator();
        var request = NotificationRequestFactory.CreateMarkNotificationAsSeenRequest();

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void MarkNotificationAsSeenValidator_ShouldFail_WhenNotificationIdIsEmpty()
    {
        // Arrange
        var validator = new MarkNotificationAsSeen.MarkNotificationAsSeenValidator();
        var request = NotificationRequestFactory.CreateMarkNotificationAsSeenRequest(Guid.Empty);

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(error => error.PropertyName == nameof(request.NotificationId));
    }
}
