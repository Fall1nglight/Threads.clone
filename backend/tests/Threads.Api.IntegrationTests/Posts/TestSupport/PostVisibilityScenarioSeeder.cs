using Threads.Api.Data.Follows;
using Threads.Api.Data.Posts;
using Threads.Api.Data.Users;
using Threads.Api.IntegrationTests.Follows.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;
using Threads.Api.IntegrationTests.Users.TestSupport;

namespace Threads.Api.IntegrationTests.Posts.TestSupport;

public sealed class PostVisibilityScenarioSeeder
{
    private readonly FollowTestSeeder _followSeeder;
    private readonly PostTestSeeder _postSeeder;
    private readonly UserTestSeeder _userSeeder;

    public PostVisibilityScenarioSeeder(
        UserTestSeeder userSeeder,
        PostTestSeeder postSeeder,
        FollowTestSeeder followSeeder
    )
    {
        _userSeeder = userSeeder;
        _postSeeder = postSeeder;
        _followSeeder = followSeeder;
    }

    public async Task<PostVisibilitySeed> SeedAsync()
    {
        var alice = await _userSeeder.CreateUserAsync(username: IntegrationTestData.AliceUsername);
        var alicePrivate = await _userSeeder.CreateUserAsync(
            username: PostTestData.AlicePrivateUsername,
            isPrivate: true
        );
        var bobPublic = await _userSeeder.CreateUserAsync(username: PostTestData.BobPublicUsername);
        var carolPublicFollowed = await _userSeeder.CreateUserAsync(
            username: PostTestData.CarolPublicFollowedUsername
        );
        var dinaPrivateFollowed = await _userSeeder.CreateUserAsync(
            username: PostTestData.DinaPrivateFollowedUsername,
            isPrivate: true
        );
        var erinPrivatePending = await _userSeeder.CreateUserAsync(
            username: PostTestData.ErinPrivatePendingUsername,
            isPrivate: true
        );
        var frankPrivateStranger = await _userSeeder.CreateUserAsync(
            username: PostTestData.FrankPrivateStrangerUsername,
            isPrivate: true
        );
        var henry = await _userSeeder.CreateUserAsync(username: IntegrationTestData.HenryUsername);

        await _followSeeder.CreateFollowAsync(alice, carolPublicFollowed);
        await _followSeeder.CreateFollowAsync(alice, dinaPrivateFollowed);
        await _followSeeder.CreateFollowAsync(
            alice,
            erinPrivatePending,
            status: FollowStatus.Pending
        );

        var posts = await _postSeeder.CreatePostsAsync(
            new List<PostSeedData>
            {
                new(
                    alice,
                    PostTestData.AliceOwnPostContent,
                    IntegrationTestData.BaseTime.AddMinutes(1)
                ),
                new(
                    alicePrivate,
                    PostTestData.AlicePrivateOwnPostContent,
                    IntegrationTestData.BaseTime.AddMinutes(2)
                ),
                new(
                    bobPublic,
                    PostTestData.BobPublicPostContent,
                    IntegrationTestData.BaseTime.AddMinutes(3)
                ),
                new(
                    carolPublicFollowed,
                    PostTestData.CarolPublicFollowedPostContent,
                    IntegrationTestData.BaseTime.AddMinutes(4)
                ),
                new(
                    dinaPrivateFollowed,
                    PostTestData.DinaPrivateFollowedPostContent,
                    IntegrationTestData.BaseTime.AddMinutes(5)
                ),
                new(
                    erinPrivatePending,
                    PostTestData.ErinPrivatePendingPostContent,
                    IntegrationTestData.BaseTime.AddMinutes(6)
                ),
                new(
                    frankPrivateStranger,
                    PostTestData.FrankPrivateStrangerPostContent,
                    IntegrationTestData.BaseTime.AddMinutes(7)
                ),
                new(
                    bobPublic,
                    PostTestData.DeletedPublicPostContent,
                    IntegrationTestData.BaseTime.AddMinutes(8),
                    IsDeleted: true
                ),
            }
        );

        return new PostVisibilitySeed(
            alice,
            alicePrivate,
            bobPublic,
            carolPublicFollowed,
            dinaPrivateFollowed,
            erinPrivatePending,
            frankPrivateStranger,
            henry,
            posts[0],
            posts[1],
            posts[2],
            posts[3],
            posts[4],
            posts[5],
            posts[6],
            posts[7]
        );
    }
}

public sealed record PostVisibilitySeed(
    User Alice,
    User AlicePrivate,
    User BobPublic,
    User CarolPublicFollowed,
    User DinaPrivateFollowed,
    User ErinPrivatePending,
    User FrankPrivateStranger,
    User Henry,
    Post AliceOwnPost,
    Post AlicePrivateOwnPost,
    Post BobPublicPost,
    Post CarolPublicFollowedPost,
    Post DinaPrivateFollowedPost,
    Post ErinPrivatePendingPost,
    Post FrankPrivateStrangerPost,
    Post DeletedPublicPost
);
