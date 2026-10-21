using Threads.Api.Data.Follows;
using Threads.Api.Data.Users;
using Threads.Api.IntegrationTests.Blocks.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;
using Threads.Api.IntegrationTests.Users.TestSupport;

namespace Threads.Api.IntegrationTests.Follows.TestSupport;

public sealed class FollowRelationshipScenarioSeeder
{
    private readonly UserTestSeeder _userSeeder;
    private readonly FollowTestSeeder _followSeeder;
    private readonly BlockTestSeeder _blockSeeder;

    public FollowRelationshipScenarioSeeder(
        UserTestSeeder userSeeder,
        FollowTestSeeder followSeeder,
        BlockTestSeeder blockSeeder
    )
    {
        _userSeeder = userSeeder;
        _followSeeder = followSeeder;
        _blockSeeder = blockSeeder;
    }

    public async Task<FollowListGraphSeed> SeedListGraphAsync()
    {
        var requester = await CreateUserAsync(IntegrationTestData.AliceUsername);
        var incomingPending = await CreateUserAsync(IntegrationTestData.BobUsername);
        var outgoingPending = await CreateUserAsync(IntegrationTestData.CarolUsername);
        var incomingAccepted = await CreateUserAsync(IntegrationTestData.HenryUsername);
        var outgoingAccepted = await CreateUserAsync("dina-followed");
        var blockedUsers = new List<User>();

        for (var index = 0; index < 8; index++)
            blockedUsers.Add(await CreateUserAsync($"blocked-follow-{index}"));

        await _followSeeder.CreateFollowsAsync(
            new List<FollowSeedData>
            {
                new(incomingPending, requester, FollowStatus.Pending, Time(1)),
                new(requester, outgoingPending, FollowStatus.Pending, Time(2)),
                new(incomingAccepted, requester, FollowStatus.Accepted, Time(3)),
                new(requester, outgoingAccepted, FollowStatus.Accepted, Time(4)),
                new(blockedUsers[0], requester, FollowStatus.Pending, Time(5)),
                new(blockedUsers[1], requester, FollowStatus.Pending, Time(6)),
                new(requester, blockedUsers[2], FollowStatus.Pending, Time(7)),
                new(requester, blockedUsers[3], FollowStatus.Pending, Time(8)),
                new(blockedUsers[4], requester, FollowStatus.Accepted, Time(9)),
                new(blockedUsers[5], requester, FollowStatus.Accepted, Time(10)),
                new(requester, blockedUsers[6], FollowStatus.Accepted, Time(11)),
                new(requester, blockedUsers[7], FollowStatus.Accepted, Time(12)),
            }
        );

        await _blockSeeder.CreateUserBlocksAsync(
            new List<UserBlockSeedData>
            {
                new(requester, blockedUsers[0]),
                new(blockedUsers[1], requester),
                new(requester, blockedUsers[2]),
                new(blockedUsers[3], requester),
                new(requester, blockedUsers[4]),
                new(blockedUsers[5], requester),
                new(requester, blockedUsers[6]),
                new(blockedUsers[7], requester),
            }
        );

        return new(requester, incomingPending, outgoingPending, incomingAccepted, outgoingAccepted);
    }

    public async Task<FollowBulkSeed> SeedBulkAsync(FollowListKind kind, int count)
    {
        var requester = await CreateUserAsync($"bulk-requester-{kind}");
        var users = new List<User>(count);

        for (var index = 0; index < count; index++)
            users.Add(await CreateUserAsync($"bulk-follow-{kind}-{index:D3}"));

        var status = kind is FollowListKind.IncomingRequests or FollowListKind.OutgoingRequests
            ? FollowStatus.Pending
            : FollowStatus.Accepted;
        var follows = await _followSeeder.CreateFollowsAsync(
            users
                .Select(
                    (user, index) =>
                        kind is FollowListKind.IncomingRequests or FollowListKind.Followers
                            ? new FollowSeedData(user, requester, status, Time(index))
                            : new FollowSeedData(requester, user, status, Time(index))
                )
                .ToList()
        );

        return new(requester, users, follows);
    }

    private Task<User> CreateUserAsync(string username) =>
        _userSeeder.CreateUserAsync(username: username);

    private static DateTime Time(int minutes) => IntegrationTestData.BaseTime.AddMinutes(minutes);
}

public enum FollowListKind
{
    IncomingRequests,
    OutgoingRequests,
    Followers,
    Following,
}

public sealed record FollowListGraphSeed(
    User Requester,
    User IncomingPending,
    User OutgoingPending,
    User IncomingAccepted,
    User OutgoingAccepted
);

public sealed record FollowBulkSeed(
    User Requester,
    IReadOnlyList<User> Users,
    IReadOnlyList<Follow> Follows
);
