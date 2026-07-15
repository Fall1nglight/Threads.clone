namespace Threads.Api.IntegrationTests.Infrastructure;

internal static class IntegrationTestData
{
    public const string ValidPassword = "Test123!";

    public const string AliceUsername = "alice";
    public const string AliceEmail = "alice@example.test";
    public const string AliceBio = "Hello from Alice";

    public const string BobUsername = "bob";
    public const string CarolUsername = "carol";
    public const string HenryUsername = "henry";

    public const string ValidContent = "Valid content";
    public const string OtherValidContent = "Other valid content";

    public const int MaxContentLength = 600;

    public static readonly DateTime BaseTime = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    public static string ContentOfLength(int length) => new('a', length);

    public static string MaxLengthContent => ContentOfLength(MaxContentLength);
}
