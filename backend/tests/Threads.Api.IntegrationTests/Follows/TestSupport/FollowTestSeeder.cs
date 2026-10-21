using Threads.Api.Data.Follows;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Users;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Follows.TestSupport;

public sealed class FollowTestSeeder
{
    private readonly AppDbContext _db;

    public FollowTestSeeder(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Follow> CreateFollowAsync(
        User follower,
        User followed,
        FollowStatus status = FollowStatus.Accepted,
        DateTime? createdAtUtc = null
    )
    {
        var follow = new Follow
        {
            FollowerId = follower.Id,
            FollowedId = followed.Id,
            Status = status,
            CreatedAtUtc = createdAtUtc ?? IntegrationTestData.BaseTime,
        };

        await _db.Follows.AddAsync(follow);
        await _db.SaveChangesAsync();
        return follow;
    }

    public async Task<IReadOnlyList<Follow>> CreateFollowsAsync(
        IReadOnlyCollection<FollowSeedData> seeds
    )
    {
        var follows = seeds
            .Select(seed => new Follow
            {
                FollowerId = seed.Follower.Id,
                FollowedId = seed.Followed.Id,
                Status = seed.Status,
                CreatedAtUtc = seed.CreatedAtUtc ?? IntegrationTestData.BaseTime,
            })
            .ToList();

        await _db.Follows.AddRangeAsync(follows);
        await _db.SaveChangesAsync();
        return follows;
    }
}

public sealed record FollowSeedData(
    User Follower,
    User Followed,
    FollowStatus Status = FollowStatus.Accepted,
    DateTime? CreatedAtUtc = null
);
