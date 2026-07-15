using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.WebUtilities;
using Threads.Api.Common.Pagination;
using Threads.Api.UnitTests.TestSupport;

namespace Threads.Api.UnitTests.Common.Pagination;

public class CursorEncoderTests
{
    [Fact]
    public void EncodeAndDecode_ShouldPreserveCursorValues()
    {
        // Arrange
        var lastDate = new DateTime(2026, 7, 15, 12, 34, 56, DateTimeKind.Utc).AddTicks(1234567);
        Guid lastId = ValidatorTestData.PrimaryId;

        // Act
        string encodedCursor = CursorEncoder.Encode(lastDate, lastId);
        Cursor? decodedCursor = CursorEncoder.Decode(encodedCursor);

        // Assert
        decodedCursor.Should().NotBeNull();
        decodedCursor!.LastDate.Should().Be(lastDate);
        decodedCursor.LastId.Should().Be(lastId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Decode_ShouldReturnNull_WhenCursorIsEmpty(string? encodedCursor)
    {
        // Arrange

        // Act
        Cursor? result = CursorEncoder.Decode(encodedCursor!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Decode_ShouldReturnNull_WhenCursorIsNotValidBase64Url()
    {
        // Arrange
        const string encodedCursor = "%not-base64url%";

        // Act
        Cursor? result = CursorEncoder.Decode(encodedCursor);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Decode_ShouldReturnNull_WhenDecodedValueIsNotValidJson()
    {
        // Arrange
        byte[] invalidJson = Encoding.UTF8.GetBytes("not-json");
        string encodedCursor = Base64UrlTextEncoder.Encode(invalidJson);

        // Act
        Cursor? result = CursorEncoder.Decode(encodedCursor);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Encode_ShouldReturnBase64UrlCompatibleValue()
    {
        // Arrange
        var lastDate = new DateTime(2026, 7, 15, 12, 34, 56, DateTimeKind.Utc);
        Guid lastId = ValidatorTestData.PrimaryId;

        // Act
        string encodedCursor = CursorEncoder.Encode(lastDate, lastId);

        // Assert
        encodedCursor.Should().NotContain("+");
        encodedCursor.Should().NotContain("/");
        encodedCursor.Should().NotContain("=");
    }
}
