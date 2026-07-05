namespace Threads.Api.IntegrationTests.Posts.TestSupport;

internal static class PostTestData
{
    public const string ValidPassword = "Test123!";

    public const string AliceUsername = "alice";
    public const string AlicePrivateUsername = "alicePrivate";
    public const string BobPublicUsername = "bobPublic";
    public const string CarolPublicFollowedUsername = "carolPublicFollowed";
    public const string DinaPrivateFollowedUsername = "dinaPrivateFollowed";
    public const string ErinPrivatePendingUsername = "erinPrivatePending";
    public const string FrankPrivateStrangerUsername = "frankPrivateStranger";
    public const string HenryUsername = "henry";
    public const string BulkUsernamePrefix = "bulk";
    public const string SameTimestampUsernamePrefix = "sameTimestamp";

    public const string AliceOwnPostContent = "Alice own post";
    public const string AlicePrivateOwnPostContent = "Alice private own post";
    public const string BobPublicPostContent = "Bob public post";
    public const string CarolPublicFollowedPostContent = "Carol public followed post";
    public const string DinaPrivateFollowedPostContent = "Dina private followed post";
    public const string ErinPrivatePendingPostContent = "Erin private pending post";
    public const string FrankPrivateStrangerPostContent = "Frank private stranger post";
    public const string DeletedPublicPostContent = "Deleted public post";
    public const string BulkPostContentPrefix = "Bulk public post";
    public const string SameTimestampLowerPostContent = "Same timestamp lower id";
    public const string SameTimestampHigherPostContent = "Same timestamp higher id";

    public const string ValidContent = "Valid content";
    public const string OtherValidContent = "Other valid content";

    public const int MaxContentLength = 600;

    public static readonly DateTime BaseTime = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    public static readonly Guid AuthSmokePostId = Guid.Parse(
        "11111111-1111-1111-1111-111111111111"
    );

    public static readonly Guid LowerTieBreakerPostId = Guid.Parse(
        "11111111-1111-1111-1111-111111111111"
    );

    public static readonly Guid HigherTieBreakerPostId = Guid.Parse(
        "ffffffff-ffff-ffff-ffff-ffffffffffff"
    );

    public static string ContentOfLength(int length) => new('a', length);

    public static string MaxLengthContent => ContentOfLength(MaxContentLength);
}
