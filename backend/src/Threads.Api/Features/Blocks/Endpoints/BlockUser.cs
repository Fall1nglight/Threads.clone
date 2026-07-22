using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Common.Extensions;
using Threads.Api.Data.Blocks;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Features.Follows;

namespace Threads.Api.Features.Blocks.Endpoints;

public class BlockUser : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapPost("/{targetUserId}", Handle)
            .WithValidation<Request>()
            .WithSummary("Blocks a user");
    }

    public record Request(Guid TargetUserId);

    public class BlockUserValidator : AbstractValidator<Request>
    {
        public BlockUserValidator()
        {
            RuleFor(x => x.TargetUserId).NotEmpty();
        }
    }

    private static async Task<Results<NoContent, NotFound, BadRequest>> Handle(
        [AsParameters] Request request,
        AppDbContext db,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        var currentUserId = claimsPrincipal.GetUserId();
        if (request.TargetUserId == currentUserId)
            return TypedResults.BadRequest();

        bool targetUserExists = await db.Users.AnyAsync(
            user => user.Id == request.TargetUserId,
            cancellationToken
        );

        if (!targetUserExists)
            return TypedResults.NotFound();

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        bool blockExists = await db.UserBlocks.AnyAsync(
            block => block.BlockerId == currentUserId && block.BlockedId == request.TargetUserId,
            cancellationToken
        );

        if (!blockExists)
        {
            var userBlock = new UserBlock
            {
                BlockerId = currentUserId,
                BlockedId = request.TargetUserId,
                CreatedAtUtc = DateTime.UtcNow,
            };

            await db.UserBlocks.AddAsync(userBlock, cancellationToken);
        }

        await db
            .Follows.WhereBetween(currentUserId, request.TargetUserId)
            .ExecuteDeleteAsync(cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
