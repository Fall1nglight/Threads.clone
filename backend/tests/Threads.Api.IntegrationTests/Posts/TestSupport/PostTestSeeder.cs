using Microsoft.AspNetCore.Identity;
using Threads.Api.Data.Follows;
using Threads.Api.Data.Posts;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Users;

namespace Threads.Api.IntegrationTests.Posts.TestSupport;

public sealed class PostTestSeeder
{
    private readonly AppDbContext _db;
    private readonly UserManager<User> _userManager;

    public PostTestSeeder(AppDbContext db, UserManager<User> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<PostVisibilitySeed> SeedVisibilityGraphAsync()
    {
        var alice = await CreateUserAsync(username: PostTestData.AliceUsername);

        var alicePrivate = await CreateUserAsync(
            username: PostTestData.AlicePrivateUsername,
            isPrivate: true
        );

        var bobPublic = await CreateUserAsync(username: PostTestData.BobPublicUsername);
        var carolPublicFollowed = await CreateUserAsync(
            username: PostTestData.CarolPublicFollowedUsername
        );

        var dinaPrivateFollowed = await CreateUserAsync(
            username: PostTestData.DinaPrivateFollowedUsername,
            isPrivate: true
        );

        var erinPrivatePending = await CreateUserAsync(
            username: PostTestData.ErinPrivatePendingUsername,
            isPrivate: true
        );

        var frankPrivateStranger = await CreateUserAsync(
            username: PostTestData.FrankPrivateStrangerUsername,
            isPrivate: true
        );

        var henry = await CreateUserAsync(username: PostTestData.HenryUsername);

        await CreateFollowAsync(alice, carolPublicFollowed, FollowStatus.Accepted);
        await CreateFollowAsync(alice, dinaPrivateFollowed, FollowStatus.Accepted);
        await CreateFollowAsync(alice, erinPrivatePending, FollowStatus.Pending);

        var aliceOwnPost = await CreatePostAsync(
            owner: alice,
            content: PostTestData.AliceOwnPostContent,
            createdAtUtc: PostTestData.BaseTime.AddMinutes(1)
        );

        var alicePrivateOwnPost = await CreatePostAsync(
            owner: alicePrivate,
            content: PostTestData.AlicePrivateOwnPostContent,
            createdAtUtc: PostTestData.BaseTime.AddMinutes(2)
        );

        var bobPublicPost = await CreatePostAsync(
            owner: bobPublic,
            content: PostTestData.BobPublicPostContent,
            createdAtUtc: PostTestData.BaseTime.AddMinutes(3)
        );

        var carolPublicFollowedPost = await CreatePostAsync(
            owner: carolPublicFollowed,
            content: PostTestData.CarolPublicFollowedPostContent,
            createdAtUtc: PostTestData.BaseTime.AddMinutes(4)
        );

        var dinaPrivateFollowedPost = await CreatePostAsync(
            owner: dinaPrivateFollowed,
            content: PostTestData.DinaPrivateFollowedPostContent,
            createdAtUtc: PostTestData.BaseTime.AddMinutes(5)
        );

        var erinPrivatePendingPost = await CreatePostAsync(
            owner: erinPrivatePending,
            content: PostTestData.ErinPrivatePendingPostContent,
            createdAtUtc: PostTestData.BaseTime.AddMinutes(6)
        );

        var frankPrivateStrangerPost = await CreatePostAsync(
            owner: frankPrivateStranger,
            content: PostTestData.FrankPrivateStrangerPostContent,
            createdAtUtc: PostTestData.BaseTime.AddMinutes(7)
        );

        var deletedPublicPost = await CreatePostAsync(
            owner: bobPublic,
            content: PostTestData.DeletedPublicPostContent,
            createdAtUtc: PostTestData.BaseTime.AddMinutes(8),
            isDeleted: true
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
            aliceOwnPost,
            alicePrivateOwnPost,
            bobPublicPost,
            carolPublicFollowedPost,
            dinaPrivateFollowedPost,
            erinPrivatePendingPost,
            frankPrivateStrangerPost,
            deletedPublicPost
        );
    }

    public async Task<(User User, List<Post> Posts)> SeedBulkPublicPostsAsync(int count)
    {
        var user = await CreateUserAsync(
            username: $"{PostTestData.BulkUsernamePrefix}-{Guid.NewGuid():N}"
        );
        var posts = new List<Post>();

        for (var i = 0; i < count; i++)
        {
            var post = new Post
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Content = $"{PostTestData.BulkPostContentPrefix} {i}",
                CreatedAtUtc = PostTestData.BaseTime.AddMinutes(i),
            };

            posts.Add(post);
        }

        await _db.Posts.AddRangeAsync(posts);
        await _db.SaveChangesAsync();

        return (user, posts);
    }

    public async Task<(
        User User,
        Post LowerIdPost,
        Post HigherIdPost
    )> SeedSameTimestampPostsAsync()
    {
        var user = await CreateUserAsync(
            username: $"{PostTestData.SameTimestampUsernamePrefix}-{Guid.NewGuid():N}"
        );

        var timestamp = PostTestData.BaseTime.AddHours(1);

        var lowerIdPost = new Post
        {
            Id = PostTestData.LowerTieBreakerPostId,
            UserId = user.Id,
            Content = PostTestData.SameTimestampLowerPostContent,
            CreatedAtUtc = timestamp,
        };

        var higherIdPost = new Post
        {
            Id = PostTestData.HigherTieBreakerPostId,
            UserId = user.Id,
            Content = PostTestData.SameTimestampHigherPostContent,
            CreatedAtUtc = timestamp,
        };

        await _db.Posts.AddRangeAsync(lowerIdPost, higherIdPost);
        await _db.SaveChangesAsync();

        return (user, lowerIdPost, higherIdPost);
    }

    public async Task<User> CreateUserAsync(string username, bool isPrivate = false)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = username,
            Email = $"{username}@example.test",
            Bio = null,
            IsPrivate = isPrivate,
            CreatedAtUtc = DateTime.UtcNow,
        };

        var result = await _userManager.CreateAsync(user, PostTestData.ValidPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(" | ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException(
                $"Failed to create test user '{username}': {errors}"
            );
        }

        return user;
    }

    public async Task<Post> CreatePostAsync(
        User owner,
        string content,
        DateTime? createdAtUtc = null,
        bool isDeleted = false,
        Guid? id = null
    )
    {
        var post = new Post
        {
            Id = id ?? Guid.NewGuid(),
            UserId = owner.Id,
            Content = content,
            CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow,
            IsDeleted = isDeleted,
        };

        await _db.Posts.AddAsync(post);
        await _db.SaveChangesAsync();
        return post;
    }

    public async Task CreateFollowAsync(User follower, User followed, FollowStatus status)
    {
        var follow = new Follow
        {
            FollowerId = follower.Id,
            FollowedId = followed.Id,
            Status = status,
            CreatedAtUtc = DateTime.UtcNow,
        };

        await _db.Follows.AddAsync(follow);
        await _db.SaveChangesAsync();
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
