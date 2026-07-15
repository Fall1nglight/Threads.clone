namespace Threads.Api.IntegrationTests.Likes.TestSupport;

internal static class LikeTestRoutes
{
    public static string ForPost(Guid postId) => $"/posts/{postId}/likes";
}
