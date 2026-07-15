using Threads.Api.Data.Comments;
using Threads.Api.Data.Posts;
using Threads.Api.Data.Users;
using Threads.Api.IntegrationTests.Auth.TestSupport;
using Threads.Api.IntegrationTests.Blocks.TestSupport;
using Threads.Api.IntegrationTests.Comments.TestSupport;
using Threads.Api.IntegrationTests.Follows.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;
using Threads.Api.IntegrationTests.Likes.TestSupport;
using Threads.Api.IntegrationTests.Posts.TestSupport;

namespace Threads.Api.IntegrationTests.Users.TestSupport;

public sealed class UserDeletionScenarioSeeder
{
    private readonly BlockTestSeeder _blockSeeder;
    private readonly CommentTestSeeder _commentSeeder;
    private readonly FollowTestSeeder _followSeeder;
    private readonly LikeTestSeeder _likeSeeder;
    private readonly PostTestSeeder _postSeeder;
    private readonly RefreshTokenTestSeeder _refreshTokenSeeder;
    private readonly UserTestSeeder _userSeeder;

    public UserDeletionScenarioSeeder(
        UserTestSeeder userSeeder,
        PostTestSeeder postSeeder,
        FollowTestSeeder followSeeder,
        BlockTestSeeder blockSeeder,
        LikeTestSeeder likeSeeder,
        CommentTestSeeder commentSeeder,
        RefreshTokenTestSeeder refreshTokenSeeder
    )
    {
        _userSeeder = userSeeder;
        _postSeeder = postSeeder;
        _followSeeder = followSeeder;
        _blockSeeder = blockSeeder;
        _likeSeeder = likeSeeder;
        _commentSeeder = commentSeeder;
        _refreshTokenSeeder = refreshTokenSeeder;
    }

    public async Task<UserDeletionSeed> SeedAsync()
    {
        var alice = await _userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var bob = await _userSeeder.CreateUserAsync(username: IntegrationTestData.BobUsername);
        var carol = await _userSeeder.CreateUserAsync(username: IntegrationTestData.CarolUsername);

        var alicePost = await _postSeeder.CreatePostAsync(alice, UserTestData.AlicePostContent);
        var bobPost = await _postSeeder.CreatePostAsync(bob, UserTestData.BobPostContent);

        await _followSeeder.CreateFollowAsync(alice, bob);
        await _followSeeder.CreateFollowAsync(bob, alice);

        await _blockSeeder.CreateUserBlockAsync(blocker: alice, blocked: bob);
        await _blockSeeder.CreateUserBlockAsync(blocker: bob, blocked: alice);
        await _blockSeeder.CreateUserBlockAsync(blocker: bob, blocked: carol);

        await _likeSeeder.CreatePostLikeAsync(alice, bobPost);
        await _likeSeeder.CreatePostLikeAsync(bob, alicePost);

        var aliceComment = await _commentSeeder.CreateCommentAsync(
            alice,
            bobPost,
            UserTestData.CommentContent
        );
        var bobComment = await _commentSeeder.CreateCommentAsync(
            bob,
            alicePost,
            UserTestData.CommentContent
        );
        var tokenChain = await _refreshTokenSeeder.CreateTokenChainAsync(alice);

        return new UserDeletionSeed(
            alice,
            bob,
            carol,
            alicePost,
            bobPost,
            aliceComment,
            bobComment,
            tokenChain
        );
    }
}

public sealed record UserDeletionSeed(
    User Alice,
    User Bob,
    User Carol,
    Post AlicePost,
    Post BobPost,
    Comment AliceComment,
    Comment BobComment,
    RefreshTokenChainSeed TokenChain
);
