using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Features.Follows.Endpoints;

public class DeleteFollowing : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapDelete("/following/{followedId}", Handle)
            .WithValidation<Request>()
            .WithSummary("Deletes an outgoing follow relationship");
    }

    public record Request(Guid FollowedId);

    public class DeleteFollowingValidator : AbstractValidator<Request>
    {
        public DeleteFollowingValidator()
        {
            RuleFor(x => x.FollowedId).NotEmpty();
        }
    }

    private static async Task<NoContent> Handle(
        [AsParameters] Request request,
        AppDbContext db,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        var followerId = claimsPrincipal.GetUserId();
        var follow = await db.Follows.FirstOrDefaultAsync(
            follow => follow.FollowerId == followerId && follow.FollowedId == request.FollowedId,
            cancellationToken
        );

        if (follow == null)
            return TypedResults.NoContent();

        db.Follows.Remove(follow);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
