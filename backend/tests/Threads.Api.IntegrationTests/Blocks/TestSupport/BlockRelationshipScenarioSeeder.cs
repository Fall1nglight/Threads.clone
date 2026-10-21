using Threads.Api.Data.Blocks;
using Threads.Api.Data.Follows;
using Threads.Api.Data.Users;
using Threads.Api.IntegrationTests.Follows.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;
using Threads.Api.IntegrationTests.Users.TestSupport;

namespace Threads.Api.IntegrationTests.Blocks.TestSupport;

public sealed class BlockRelationshipScenarioSeeder
{
    private const string BulkUsernamePrefix = "blocked-user";

    private readonly UserTestSeeder _userSeeder;
    private readonly BlockTestSeeder _blockSeeder;
    private readonly FollowTestSeeder _followSeeder;

    public BlockRelationshipScenarioSeeder(
        UserTestSeeder userSeeder,
        BlockTestSeeder blockSeeder,
        FollowTestSeeder followSeeder
    )
    {
        _userSeeder = userSeeder;
        _blockSeeder = blockSeeder;
        _followSeeder = followSeeder;
    }

    public async Task<BlockFollowCleanupSeed> SeedFollowCleanupAsync(
        bool blockAlreadyExists = false
    )
    {
        var requester = await _userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var target = await _userSeeder.CreateUserAsync(username: IntegrationTestData.BobUsername);
        var outgoingFollow = await _followSeeder.CreateFollowAsync(
            requester,
            target,
            FollowStatus.Accepted
        );
        var incomingFollow = await _followSeeder.CreateFollowAsync(
            target,
            requester,
            FollowStatus.Pending
        );
        UserBlock? existingBlock = null;

        if (blockAlreadyExists)
            existingBlock = await _blockSeeder.CreateUserBlockAsync(requester, target);

        return new(requester, target, outgoingFollow, incomingFollow, existingBlock);
    }

    public async Task<BlockedUsersBulkSeed> SeedBlockedUsersAsync(int count)
    {
        var requester = await _userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var users = new List<User>(count);

        for (var index = 0; index < count; index++)
        {
            users.Add(
                await _userSeeder.CreateUserAsync(
                    username: $"{BulkUsernamePrefix}-{index:D3}",
                    bio: $"Blocked user {index}"
                )
            );
        }

        var blocks = await _blockSeeder.CreateUserBlocksAsync(
            users
                .Select(
                    (user, index) =>
                        new UserBlockSeedData(
                            requester,
                            user,
                            IntegrationTestData.BaseTime.AddMinutes(index)
                        )
                )
                .ToList()
        );

        return new(requester, users, blocks);
    }
}

public sealed record BlockFollowCleanupSeed(
    User Requester,
    User Target,
    Follow OutgoingFollow,
    Follow IncomingFollow,
    UserBlock? ExistingBlock
);

public sealed record BlockedUsersBulkSeed(
    User Requester,
    IReadOnlyList<User> Users,
    IReadOnlyList<UserBlock> Blocks
);
