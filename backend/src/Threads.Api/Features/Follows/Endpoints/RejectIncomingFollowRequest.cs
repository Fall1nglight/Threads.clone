using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Common.Extensions;
using Threads.Api.Data.Follows;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Features.Follows.Endpoints;

public class RejectIncomingFollowRequest : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapPost("/requests/incoming/{followerId}/reject", Handle)
            .WithValidation<Request>()
            .WithSummary("Rejects an incoming follow request");
    }

    public record Request(Guid FollowerId);

    public class RejectIncomingFollowRequestValidator : AbstractValidator<Request>
    {
        public RejectIncomingFollowRequestValidator()
        {
            RuleFor(x => x.FollowerId).NotEmpty();
        }
    }

    private static async Task<Results<NoContent, NotFound>> Handle(
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
                && follow.Status == FollowStatus.Pending,
            cancellationToken
        );

        if (follow == null)
            return TypedResults.NotFound();

        db.Follows.Remove(follow);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
