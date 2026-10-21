using FluentAssertions;

namespace Threads.Api.UnitTests.Auth.RefreshToken;

public class RefreshTokenStateTests
{
    [Fact]
    public void IsActive_ShouldBeTrue_WhenTokenIsNotExpiredAndNotRevoked()
    {
        // Arrange
        var token = RefreshTokenStateFactory.CreateToken(expiresAtUtc: DateTime.UtcNow.AddDays(1));

        // Act
        var isExpired = token.IsExpired;
        var isRevoked = token.IsRevoked;
        var isActive = token.IsActive;

        // Assert
        isExpired.Should().BeFalse();
        isRevoked.Should().BeFalse();
        isActive.Should().BeTrue();
    }

    [Fact]
    public void IsActive_ShouldBeFalse_WhenTokenIsExpired()
    {
        // Arrange
        var token = RefreshTokenStateFactory.CreateToken(expiresAtUtc: DateTime.UtcNow.AddDays(-1));

        // Act
        var isExpired = token.IsExpired;
        var isRevoked = token.IsRevoked;
        var isActive = token.IsActive;

        // Assert
        isExpired.Should().BeTrue();
        isRevoked.Should().BeFalse();
        isActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_ShouldBeFalse_WhenTokenIsRevoked()
    {
        // Arrange
        var token = RefreshTokenStateFactory.CreateToken(
            expiresAtUtc: DateTime.UtcNow.AddDays(1),
            revokedAtUtc: DateTime.UtcNow
        );

        // Act
        var isExpired = token.IsExpired;
        var isRevoked = token.IsRevoked;
        var isActive = token.IsActive;

        // Assert
        isExpired.Should().BeFalse();
        isRevoked.Should().BeTrue();
        isActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_ShouldBeFalse_WhenTokenIsExpiredAndRevoked()
    {
        // Arrange
        var token = RefreshTokenStateFactory.CreateToken(
            expiresAtUtc: DateTime.UtcNow.AddDays(-1),
            revokedAtUtc: DateTime.UtcNow
        );

        // Act
        var isExpired = token.IsExpired;
        var isRevoked = token.IsRevoked;
        var isActive = token.IsActive;

        // Assert
        isExpired.Should().BeTrue();
        isRevoked.Should().BeTrue();
        isActive.Should().BeFalse();
    }
}
