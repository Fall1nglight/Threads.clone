using Threads.Api.Data.Blocks;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Users;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Blocks.TestSupport;

public sealed class BlockTestSeeder
{
    private readonly AppDbContext _db;

    public BlockTestSeeder(AppDbContext db)
    {
        _db = db;
    }

    public async Task<UserBlock> CreateUserBlockAsync(
        User blocker,
        User blocked,
        DateTime? createdAtUtc = null
    )
    {
        var userBlock = new UserBlock
        {
            BlockerId = blocker.Id,
            BlockedId = blocked.Id,
            CreatedAtUtc = createdAtUtc ?? IntegrationTestData.BaseTime,
        };

        await _db.UserBlocks.AddAsync(userBlock);
        await _db.SaveChangesAsync();
        return userBlock;
    }

    public async Task<IReadOnlyList<UserBlock>> CreateUserBlocksAsync(
        IReadOnlyCollection<UserBlockSeedData> seeds
    )
    {
        var blocks = seeds
            .Select(seed => new UserBlock
            {
                BlockerId = seed.Blocker.Id,
                BlockedId = seed.Blocked.Id,
                CreatedAtUtc = seed.CreatedAtUtc ?? IntegrationTestData.BaseTime,
            })
            .ToList();

        await _db.UserBlocks.AddRangeAsync(blocks);
        await _db.SaveChangesAsync();
        return blocks;
    }
}

public sealed record UserBlockSeedData(User Blocker, User Blocked, DateTime? CreatedAtUtc = null);
