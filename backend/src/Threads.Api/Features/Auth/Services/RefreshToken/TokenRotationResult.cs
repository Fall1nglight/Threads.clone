using System.Diagnostics.CodeAnalysis;
using Threads.Api.Data.Users;

namespace Threads.Api.Features.Auth.Services.RefreshToken;

public record TokenRotationResult
{
    [MemberNotNullWhen(true, nameof(User), nameof(PlainToken))]
    public bool Succeeded { get; init; }

    public string? PlainToken { get; init; }
    public User? User { get; init; }

    public static TokenRotationResult Success(string plainToken, User user) =>
        new()
        {
            Succeeded = true,
            PlainToken = plainToken,
            User = user,
        };

    public static TokenRotationResult Failure() => new() { Succeeded = false };
}
