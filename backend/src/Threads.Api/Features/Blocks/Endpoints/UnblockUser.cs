using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Features.Blocks.Endpoints;

public class UnblockUser : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapDelete("/{targetUserId}", Handle)
            .WithValidation<Request>()
            .WithSummary("Unblocks a user");
    }

    public record Request(Guid TargetUserId);

    public class UnblockUserValidator : AbstractValidator<Request>
    {
        public UnblockUserValidator()
        {
            RuleFor(x => x.TargetUserId).NotEmpty();
        }
    }

    private static async Task<NoContent> Handle(
        [AsParameters] Request request,
        AppDbContext db,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        var currentUserId = claimsPrincipal.GetUserId();
        var block = await db.UserBlocks.FirstOrDefaultAsync(
            block => block.BlockerId == currentUserId && block.BlockedId == request.TargetUserId,
            cancellationToken
        );

        if (block == null)
            return TypedResults.NoContent();

        db.UserBlocks.Remove(block);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
