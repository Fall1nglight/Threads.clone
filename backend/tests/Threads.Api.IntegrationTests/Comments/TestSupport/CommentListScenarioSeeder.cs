using Threads.Api.Data.Comments;
using Threads.Api.Data.Posts;
using Threads.Api.Data.Users;
using Threads.Api.IntegrationTests.Blocks.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;
using Threads.Api.IntegrationTests.Posts.TestSupport;
using Threads.Api.IntegrationTests.Users.TestSupport;

namespace Threads.Api.IntegrationTests.Comments.TestSupport;

public sealed class CommentListScenarioSeeder
{
    private const string BlockedCommenterUsername = "blocked-commenter";
    private const string BlockingCommenterUsername = "blocking-commenter";

    private readonly UserTestSeeder _userSeeder;
    private readonly PostTestSeeder _postSeeder;
    private readonly CommentTestSeeder _commentSeeder;
    private readonly BlockTestSeeder _blockSeeder;

    public CommentListScenarioSeeder(
        UserTestSeeder userSeeder,
        PostTestSeeder postSeeder,
        CommentTestSeeder commentSeeder,
        BlockTestSeeder blockSeeder
    )
    {
        _userSeeder = userSeeder;
        _postSeeder = postSeeder;
        _commentSeeder = commentSeeder;
        _blockSeeder = blockSeeder;
    }

    public async Task<CommentListSeed> SeedAsync()
    {
        var requester = await _userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var postOwner = await _userSeeder.CreateUserAsync(
            username: IntegrationTestData.BobUsername
        );
        var visibleCommenter = await _userSeeder.CreateUserAsync(
            username: IntegrationTestData.CarolUsername
        );
        var blockedCommenter = await _userSeeder.CreateUserAsync(
            username: BlockedCommenterUsername
        );
        var blockingCommenter = await _userSeeder.CreateUserAsync(
            username: BlockingCommenterUsername
        );
        var targetPost = await _postSeeder.CreatePostAsync(
            postOwner,
            IntegrationTestData.ValidContent
        );
        var otherPost = await _postSeeder.CreatePostAsync(
            postOwner,
            IntegrationTestData.OtherValidContent
        );
        var comments = await _commentSeeder.CreateCommentsAsync(
            new List<CommentSeedData>
            {
                new(
                    requester,
                    targetPost,
                    "Requester comment",
                    IntegrationTestData.BaseTime.AddMinutes(1)
                ),
                new(
                    visibleCommenter,
                    targetPost,
                    "Visible comment",
                    IntegrationTestData.BaseTime.AddMinutes(2)
                ),
                new(
                    blockedCommenter,
                    targetPost,
                    "Blocked commenter comment",
                    IntegrationTestData.BaseTime.AddMinutes(3)
                ),
                new(
                    blockingCommenter,
                    targetPost,
                    "Blocking commenter comment",
                    IntegrationTestData.BaseTime.AddMinutes(4)
                ),
                new(
                    visibleCommenter,
                    otherPost,
                    "Other post comment",
                    IntegrationTestData.BaseTime.AddMinutes(5)
                ),
            }
        );

        await _blockSeeder.CreateUserBlockAsync(requester, blockedCommenter);
        await _blockSeeder.CreateUserBlockAsync(blockingCommenter, requester);

        return new(
            requester,
            targetPost,
            comments[0],
            comments[1],
            comments[2],
            comments[3],
            comments[4]
        );
    }

    public async Task<CommentBulkSeed> SeedBulkCommentsAsync(int count)
    {
        var requester = await _userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var postOwner = await _userSeeder.CreateUserAsync(
            username: IntegrationTestData.BobUsername
        );
        var post = await _postSeeder.CreatePostAsync(postOwner, IntegrationTestData.ValidContent);
        var comments = await _commentSeeder.CreateCommentsAsync(
            Enumerable
                .Range(0, count)
                .Select(index => new CommentSeedData(
                    requester,
                    post,
                    $"Comment {index}",
                    IntegrationTestData.BaseTime.AddMinutes(index)
                ))
                .ToList()
        );

        return new(requester, post, comments);
    }
}

public sealed record CommentListSeed(
    User Requester,
    Post Post,
    Comment RequesterComment,
    Comment VisibleComment,
    Comment BlockedComment,
    Comment BlockingComment,
    Comment OtherPostComment
);

public sealed record CommentBulkSeed(User Requester, Post Post, IReadOnlyList<Comment> Comments);
