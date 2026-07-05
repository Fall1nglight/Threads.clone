namespace Threads.Api.IntegrationTests.Posts.TestSupport;

internal static class PostTestRoutes
{
    public const string Base = "/posts";
    public const string AnonymousFeed = $"{Base}/anonymous-feed";
    public const string GlobalFeed = $"{Base}/global-feed";
    public const string MyFeed = $"{Base}/my-feed";
}
