using FluentAssertions;
using Threads.Api.Data.Tokens;

namespace Threads.Api.UnitTests.Auth;

public class RefreshTokenStateTests
{
    [Fact]
    public void IsActive_ShouldBeTrue_WhenTokenIsNotExpiredAndNotRevoked()
    {
        // Arrange
        var token = CreateToken(expiresAtUtc: DateTime.UtcNow.AddDays(1));

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
        var token = CreateToken(expiresAtUtc: DateTime.UtcNow.AddDays(-1));

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
        var token = CreateToken(
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
        var token = CreateToken(
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

    private static RefreshToken CreateToken(DateTime expiresAtUtc, DateTime? revokedAtUtc = null)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TokenHash = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
            ExpiresAtUtc = expiresAtUtc,
            RevokedAtUtc = revokedAtUtc,
        };
    }
}
