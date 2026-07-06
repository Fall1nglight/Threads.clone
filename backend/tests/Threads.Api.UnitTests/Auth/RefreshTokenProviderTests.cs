using FluentAssertions;
using Threads.Api.Features.Auth.Services.RefreshToken;

namespace Threads.Api.UnitTests.Auth;

public class RefreshTokenProviderTests
{
    [Fact]
    public void GeneratePlainToken_ShouldReturnFortyFourCharacterToken()
    {
        // Arrange
        var provider = new RefreshTokenProvider();

        // Act
        var token = provider.GeneratePlainToken();

        // Assert
        token.Should().HaveLength(44);
    }

    [Fact]
    public void GeneratePlainToken_ShouldReturnDifferentTokensAcrossCalls()
    {
        // Arrange
        var provider = new RefreshTokenProvider();

        // Act
        var firstToken = provider.GeneratePlainToken();
        var secondToken = provider.GeneratePlainToken();

        // Assert
        secondToken.Should().NotBe(firstToken);
    }

    [Fact]
    public void Hash_ShouldBeDeterministic()
    {
        // Arrange
        var provider = new RefreshTokenProvider();
        const string plainToken = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

        // Act
        var firstHash = provider.Hash(plainToken);
        var secondHash = provider.Hash(plainToken);

        // Assert
        secondHash.Should().Be(firstHash);
    }

    [Fact]
    public void Hash_ShouldNotReturnPlainToken()
    {
        // Arrange
        var provider = new RefreshTokenProvider();
        const string plainToken = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

        // Act
        var hash = provider.Hash(plainToken);

        // Assert
        hash.Should().NotBe(plainToken);
        hash.Should().HaveLength(44);
    }
}
