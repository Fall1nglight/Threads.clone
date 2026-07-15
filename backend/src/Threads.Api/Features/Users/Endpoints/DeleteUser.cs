using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Features.Users.Endpoints;

public class DeleteUser : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapDelete("/{userId}", Handle)
            .WithValidation<Request>()
            .WithSummary("Deletes the current user profile");
    }

    public record Request(Guid UserId);

    public class DeleteUserValidator : AbstractValidator<Request>
    {
        public DeleteUserValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
        }
    }

    private static async Task<Results<NoContent, ForbidHttpResult>> Handle(
        [AsParameters] Request request,
        AppDbContext db,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        var currentUserId = claimsPrincipal.GetUserId();
        if (request.UserId != currentUserId)
            return TypedResults.Forbid();

        var user = await db.Users.FirstOrDefaultAsync(
            user => user.Id == currentUserId,
            cancellationToken
        );

        if (user == null)
            return TypedResults.NoContent();

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        await db
            .RefreshTokens.Where(token => token.UserId == currentUserId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(token => token.ReplacedByTokenId, (Guid?)null),
                cancellationToken
            );

        await db
            .Follows.Where(follow =>
                follow.FollowerId == currentUserId || follow.FollowedId == currentUserId
            )
            .ExecuteDeleteAsync(cancellationToken);

        await db
            .UserBlocks.Where(block =>
                block.BlockerId == currentUserId || block.BlockedId == currentUserId
            )
            .ExecuteDeleteAsync(cancellationToken);

        await db
            .PostLikes.Where(like => like.UserId == currentUserId)
            .ExecuteDeleteAsync(cancellationToken);

        await db
            .Comments.Where(comment => comment.UserId == currentUserId)
            .ExecuteDeleteAsync(cancellationToken);

        await db
            .RefreshTokens.Where(token => token.UserId == currentUserId)
            .ExecuteDeleteAsync(cancellationToken);

        db.Users.Remove(user);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
