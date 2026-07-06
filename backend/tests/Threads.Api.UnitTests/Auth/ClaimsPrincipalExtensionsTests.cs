using System.Security.Claims;
using FluentAssertions;
using Threads.Api.Common.Extensions;

namespace Threads.Api.UnitTests.Auth;

public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetUserId_ShouldReturnUserId_WhenClaimIsValid()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        ClaimsPrincipal principal = ClaimsPrincipalTestFactory.CreateWithUserId(userId);

        // Act
        Guid result = principal.GetUserId();

        // Assert
        result.Should().Be(userId);
    }

    [Fact]
    public void GetUserId_ShouldThrow_WhenClaimIsMissing()
    {
        // Arrange
        ClaimsPrincipal principal = ClaimsPrincipalTestFactory.CreateWithoutUserId();

        // Act
        Func<Guid> act = principal.GetUserId;

        // Assert
        act.Should()
            .Throw<Exception>()
            .WithMessage(ClaimsPrincipalTestFactory.GetUserIdErrorMessage);
    }

    [Fact]
    public void GetUserId_ShouldThrow_WhenClaimIsMalformed()
    {
        // Arrange
        ClaimsPrincipal principal = ClaimsPrincipalTestFactory.CreateWithMalformedUserId();

        // Act
        Func<Guid> act = principal.GetUserId;

        // Assert
        act.Should()
            .Throw<Exception>()
            .WithMessage(ClaimsPrincipalTestFactory.GetUserIdErrorMessage);
    }
}
