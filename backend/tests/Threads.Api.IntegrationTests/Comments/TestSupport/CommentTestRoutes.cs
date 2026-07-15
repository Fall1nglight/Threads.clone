namespace Threads.Api.IntegrationTests.Comments.TestSupport;

internal static class CommentTestRoutes
{
    public static string ForPost(Guid postId) => $"/posts/{postId}/comments";

    public static string ForComment(Guid postId, Guid commentId) =>
        $"{ForPost(postId)}/{commentId}";
}
