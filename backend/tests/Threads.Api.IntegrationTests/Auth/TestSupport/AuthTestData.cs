namespace Threads.Api.IntegrationTests.Auth.TestSupport;

internal static class AuthTestData
{
    public const string WrongPassword = "WrongPassword123!";

    public const string ValidEmail = "valid@example.test";
    public const string InvalidEmail = "invalid-email";

    public const string HenryEmail = "henry@example.test";

    public const int ActiveTokenSeed = 1;
    public const int SecondActiveTokenSeed = 2;
    public const int ExpiredTokenSeed = 3;
    public const int RevokedTokenSeed = 4;
    public const int OtherUserActiveTokenSeed = 5;
    public const int UnknownTokenSeed = 99;
}
