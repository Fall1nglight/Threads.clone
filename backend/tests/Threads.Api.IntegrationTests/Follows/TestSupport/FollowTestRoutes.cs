namespace Threads.Api.IntegrationTests.Follows.TestSupport;

internal static class FollowTestRoutes
{
    public const string Base = "/follows";
    public const string IncomingRequests = $"{Base}/requests/incoming";
    public const string OutgoingRequests = $"{Base}/requests/outgoing";
    public const string Followers = $"{Base}/followers";
    public const string Following = $"{Base}/following";

    public static string ForTarget(Guid targetUserId) => $"{Base}/{targetUserId}";

    public static string AcceptIncoming(Guid followerId) =>
        $"{IncomingRequests}/{followerId}/accept";

    public static string RejectIncoming(Guid followerId) =>
        $"{IncomingRequests}/{followerId}/reject";

    public static string RemoveFollower(Guid followerId) => $"{Followers}/{followerId}";

    public static string DeleteFollowing(Guid followedId) => $"{Following}/{followedId}";
}
