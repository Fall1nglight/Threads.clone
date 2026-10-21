namespace Threads.Api.IntegrationTests.Follows.TestSupport;

internal static class FollowTestRoutes
{
    public const string MeBase = "me";
    public const string UsersBase = "users";
    public const string IncomingRequests = $"{MeBase}/follow-requests/incoming";
    public const string OutgoingRequests = $"{MeBase}/follow-requests/outgoing";

    public static string SendFollowRequest(Guid targetUserId) =>
        $"{MeBase}/following/{targetUserId}";

    public static string AcceptIncoming(Guid followerId) =>
        $"{IncomingRequests}/{followerId}/accept";

    public static string RejectIncoming(Guid followerId) =>
        $"{IncomingRequests}/{followerId}/reject";

    public static string CancelOutgoing(Guid followedId) =>
        $"{OutgoingRequests}/{followedId}/cancel";

    public static string DeleteFollower(Guid followerId) => $"{MeBase}/followers/{followerId}";

    public static string DeleteFollowing(Guid followedId) => $"{MeBase}/following/{followedId}";

    public static string FollowersFor(Guid userId) => $"{UsersBase}/{userId}/followers";

    public static string FollowingFor(Guid userId) => $"{UsersBase}/{userId}/following";
}
