namespace Threads.Api.UnitTests.TestSupport;

internal static class ValidatorTestData
{
    public const string ValidContent = "Valid content";
    public const int MaxContentLength = 600;

    public static readonly Guid PrimaryId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid SecondaryId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static TheoryData<string?> EmptyContentValues => [null, string.Empty, " "];

    public static TheoryData<int> TooLongContentLengths => [601, 666, 999];
}
