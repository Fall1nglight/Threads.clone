using Microsoft.AspNetCore.Identity;
using Threads.Api.Data.Follows;
using Threads.Api.Data.Posts;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Users;

namespace Threads.Api.IntegrationTests.Posts;

public sealed class PostTestSeeder
{
    private static readonly DateTime BaseTime = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid LowerTieBreakerPostId = Guid.Parse(
        "11111111-1111-1111-1111-111111111111"
    );
    private static readonly Guid HigherTieBreakerPostId = Guid.Parse(
        "ffffffff-ffff-ffff-ffff-ffffffffffff"
    );

    private readonly AppDbContext _db;
    private readonly UserManager<User> _userManager;

    public PostTestSeeder(AppDbContext db, UserManager<User> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<PostVisibilitySeed> SeedVisibilityGraphAsync()
    {
        // create test users
        var alice = await CreateUserAsync("alice");
        var alicePrivate = await CreateUserAsync("alicePrivate", isPrivate: true);
        var bobPublic = await CreateUserAsync("bobPublic");
        var carolPublicFollowed = await CreateUserAsync("carolPublicFollowed");
        var dinaPrivateFollowed = await CreateUserAsync("dinaPrivateFollowed", isPrivate: true);
        var erinPrivatePending = await CreateUserAsync("erinPrivatePending", isPrivate: true);
        var frankPrivateStranger = await CreateUserAsync("frankPrivateStranger", isPrivate: true);
        var henry = await CreateUserAsync("henry");

        // create follows
        // alice -> carolPublic, dinaPrivate, erinPrivate
        await CreateFollowAsync(alice, carolPublicFollowed, FollowStatus.Accepted);
        await CreateFollowAsync(alice, dinaPrivateFollowed, FollowStatus.Accepted);
        await CreateFollowAsync(alice, erinPrivatePending, FollowStatus.Pending);

        // create posts - alice
        var aliceOwnPost = await CreatePostAsync(alice, "Alice own post", BaseTime.AddMinutes(1));

        // create posts - alice (private)
        var alicePrivateOwnPost = await CreatePostAsync(
            alicePrivate,
            "Alice private own post",
            BaseTime.AddMinutes(2)
        );

        // create posts - bob
        var bobPublicPost = await CreatePostAsync(
            bobPublic,
            "Bob public post",
            BaseTime.AddMinutes(3)
        );

        // create posts - carol
        var carolPublicFollowedPost = await CreatePostAsync(
            carolPublicFollowed,
            "Carol public followed post",
            BaseTime.AddMinutes(4)
        );

        // create posts - dina
        var dinaPrivateFollowedPost = await CreatePostAsync(
            dinaPrivateFollowed,
            "Dina private followed post",
            BaseTime.AddMinutes(5)
        );

        // create posts - erin
        var erinPrivatePendingPost = await CreatePostAsync(
            erinPrivatePending,
            "Erin private pending post",
            BaseTime.AddMinutes(6)
        );

        // create posts - frank
        var frankPrivateStrangerPost = await CreatePostAsync(
            frankPrivateStranger,
            "Frank private stranger post",
            BaseTime.AddMinutes(7)
        );

        var deletedPublicPost = await CreatePostAsync(
            bobPublic,
            "Deleted public post",
            BaseTime.AddMinutes(8),
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
        var user = await CreateUserAsync($"bulk-{Guid.NewGuid():N}");
        var posts = new List<Post>();

        for (var i = 0; i < count; i++)
        {
            var post = new Post
            {
                // todo | id-t itt is reviewzni
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Content = $"Bulk public post {i}",
                CreatedAtUtc = BaseTime.AddMinutes(i),
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
        var user = await CreateUserAsync($"sameTimestamp-{Guid.NewGuid():N}");
        var timestamp = BaseTime.AddHours(1);

        var lowerIdPost = new Post
        {
            Id = LowerTieBreakerPostId,
            UserId = user.Id,
            Content = "Same timestamp lower id",
            CreatedAtUtc = timestamp,
        };

        var higherIdPost = new Post
        {
            Id = HigherTieBreakerPostId,
            UserId = user.Id,
            Content = "Same timestamp higher id",
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
            // todo | ezt ellenőrizni, hogy kell-e
            Id = Guid.NewGuid(),
            UserName = username,
            Email = $"{username}@example.test",
            Bio = null,
            IsPrivate = isPrivate,
            CreatedAtUtc = DateTime.UtcNow,
        };

        var result = await _userManager.CreateAsync(user, "Test123!");
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
        // todo | itt is az id-t megnézni újra
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
