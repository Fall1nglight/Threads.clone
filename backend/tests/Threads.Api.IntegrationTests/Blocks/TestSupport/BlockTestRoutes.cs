namespace Threads.Api.IntegrationTests.Blocks.TestSupport;

internal static class BlockTestRoutes
{
    public const string Base = "/blocks";

    public static string ForTarget(Guid targetUserId) => $"{Base}/{targetUserId}";
}
