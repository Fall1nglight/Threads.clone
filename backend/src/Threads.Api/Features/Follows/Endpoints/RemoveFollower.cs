using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Common.Extensions;
using Threads.Api.Data.Follows;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Features.Follows.Endpoints;

public class RemoveFollower : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapDelete("/followers/{followerId}", Handle)
            .WithValidation<Request>()
            .WithSummary("Removes a follower");
    }

    public record Request(Guid FollowerId);

    public class RemoveFollowerValidator : AbstractValidator<Request>
    {
        public RemoveFollowerValidator()
        {
            RuleFor(x => x.FollowerId).NotEmpty();
        }
    }

    private static async Task<NoContent> Handle(
        [AsParameters] Request request,
        AppDbContext db,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        var followedId = claimsPrincipal.GetUserId();
        var follow = await db.Follows.FirstOrDefaultAsync(
            follow =>
                follow.FollowerId == request.FollowerId
                && follow.FollowedId == followedId
                && follow.Status == FollowStatus.Accepted,
            cancellationToken
        );

        if (follow == null)
            return TypedResults.NoContent();

        db.Follows.Remove(follow);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
