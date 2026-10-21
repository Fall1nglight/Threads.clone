using Threads.Api.Features.Blocks.Endpoints;
using Threads.Api.UnitTests.TestSupport;

namespace Threads.Api.UnitTests.Blocks;

public static class BlockRequestFactory
{
    public static BlockUser.Request CreateBlockUserRequest(Guid? targetUserId = null)
    {
        return new(targetUserId ?? ValidatorTestData.ValidPrimaryGuid);
    }

    public static UnblockUser.Request CreateUnblockUserRequest(Guid? targetUserId = null)
    {
        return new(targetUserId ?? ValidatorTestData.ValidPrimaryGuid);
    }
}
