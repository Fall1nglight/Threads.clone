using Threads.Api.Data.Blocks;

namespace Threads.Api.Features.Blocks;

public static class BlockDtoExtensions
{
    public static IQueryable<BlockedUserDto> ToBlockedUserDto(this IQueryable<UserBlock> blocks)
    {
        return blocks.Select(block => new BlockedUserDto
        {
            Id = block.Blocked.Id,
            Username = block.Blocked.UserName!,
            IsPrivate = block.Blocked.IsPrivate,
            Bio = block.Blocked.Bio,
            CreatedAtUtc = block.CreatedAtUtc,
        });
    }
}
