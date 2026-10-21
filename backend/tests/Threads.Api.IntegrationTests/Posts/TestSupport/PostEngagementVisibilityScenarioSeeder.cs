using Threads.Api.Data.Posts;
using Threads.Api.Data.Users;
using Threads.Api.IntegrationTests.Blocks.TestSupport;
using Threads.Api.IntegrationTests.Comments.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;
using Threads.Api.IntegrationTests.Likes.TestSupport;

namespace Threads.Api.IntegrationTests.Posts.TestSupport;

public sealed class PostEngagementVisibilityScenarioSeeder
{
    private readonly BlockTestSeeder _blockSeeder;
    private readonly CommentTestSeeder _commentSeeder;
    private readonly LikeTestSeeder _likeSeeder;
    private readonly PostVisibilityScenarioSeeder _visibilityScenarioSeeder;

    public PostEngagementVisibilityScenarioSeeder(
        PostVisibilityScenarioSeeder visibilityScenarioSeeder,
        BlockTestSeeder blockSeeder,
        LikeTestSeeder likeSeeder,
        CommentTestSeeder commentSeeder
    )
    {
        _visibilityScenarioSeeder = visibilityScenarioSeeder;
        _blockSeeder = blockSeeder;
        _likeSeeder = likeSeeder;
        _commentSeeder = commentSeeder;
    }

    public async Task<PostEngagementVisibilitySeed> SeedAsync()
    {
        var visibility = await _visibilityScenarioSeeder.SeedAsync();

        await _blockSeeder.CreateUserBlockAsync(
            blocker: visibility.Alice,
            blocked: visibility.Henry
        );
        await _blockSeeder.CreateUserBlockAsync(
            blocker: visibility.CarolPublicFollowed,
            blocked: visibility.Alice
        );

        await _likeSeeder.CreatePostLikeAsync(visibility.Alice, visibility.BobPublicPost);
        await _likeSeeder.CreatePostLikeAsync(visibility.Henry, visibility.BobPublicPost);
        await _likeSeeder.CreatePostLikeAsync(
            visibility.CarolPublicFollowed,
            visibility.BobPublicPost
        );

        await _commentSeeder.CreateCommentAsync(
            visibility.Alice,
            visibility.BobPublicPost,
            IntegrationTestData.ValidContent
        );
        await _commentSeeder.CreateCommentAsync(
            visibility.Henry,
            visibility.BobPublicPost,
            IntegrationTestData.ValidContent
        );
        await _commentSeeder.CreateCommentAsync(
            visibility.CarolPublicFollowed,
            visibility.BobPublicPost,
            IntegrationTestData.ValidContent
        );

        return new PostEngagementVisibilitySeed(visibility.Alice, visibility.BobPublicPost);
    }
}

public sealed record PostEngagementVisibilitySeed(User Requester, Post TargetPost);
