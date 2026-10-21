using Threads.Api.Data.Users;

namespace Threads.Api.Data.Tokens;

public class RefreshToken
{
    public Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required string TokenHash { get; init; }

    public DateTime? RevokedAtUtc { get; set; }
    public RevocationReason? RevocationReason { get; set; }
    public RefreshToken? ReplacedByToken { get; set; }
    public Guid? ReplacedByTokenId { get; set; }

    public required DateTime ExpiresAtUtc { get; set; }
    public User User { get; init; } = null!;

    public bool IsExpired => ExpiresAtUtc <= DateTime.UtcNow;
    public bool IsRevoked => RevokedAtUtc != null;
    public bool IsActive => !IsRevoked && !IsExpired;
}
