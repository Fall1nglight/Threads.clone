namespace Threads.Api.Data.Tokens;

public enum RevocationReason
{
    Logout = 0,
    GlobalLogout = 1,
    ReplacedByNewToken = 2,
    Compromised = 3,
    DeviceLimitExceeded = 4,
}
