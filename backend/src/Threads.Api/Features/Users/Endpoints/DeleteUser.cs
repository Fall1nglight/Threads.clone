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
        var loggedInUserId = claimsPrincipal.GetUserId();
        if (request.UserId != loggedInUserId)
            return TypedResults.Forbid();

        var user = await db.Users.FirstOrDefaultAsync(
            user => user.Id == loggedInUserId,
            cancellationToken
        );

        if (user == null)
            return TypedResults.NoContent();

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        await db
            .RefreshTokens.Where(token => token.UserId == loggedInUserId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(token => token.ReplacedByTokenId, (Guid?)null),
                cancellationToken
            );

        await db
            .Follows.Where(follow =>
                follow.FollowerId == loggedInUserId || follow.FollowedId == loggedInUserId
            )
            .ExecuteDeleteAsync(cancellationToken);

        await db
            .UserBlocks.Where(block =>
                block.BlockerId == loggedInUserId || block.BlockedId == loggedInUserId
            )
            .ExecuteDeleteAsync(cancellationToken);

        await db
            .PostLikes.Where(like => like.UserId == loggedInUserId)
            .ExecuteDeleteAsync(cancellationToken);

        await db
            .Comments.Where(comment => comment.UserId == loggedInUserId)
            .ExecuteDeleteAsync(cancellationToken);

        await db
            .RefreshTokens.Where(token => token.UserId == loggedInUserId)
            .ExecuteDeleteAsync(cancellationToken);

        db.Users.Remove(user);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
