using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Common.Extensions;
using Threads.Api.Data.Follows;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Features.Follows.Endpoints;

public class AcceptIncomingFollowRequest : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapPost("/requests/incoming/{followerId}/accept", Handle)
            .WithValidation<Request>()
            .WithSummary("Accepts an incoming follow request");
    }

    public record Request(Guid FollowerId);

    public class AcceptIncomingFollowRequestValidator : AbstractValidator<Request>
    {
        public AcceptIncomingFollowRequestValidator()
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
        var currentUserId = claimsPrincipal.GetUserId();

        var follow = await db
            .Follows.WhereParticipantsHaveNoBlockRelationship(db)
            .FirstOrDefaultAsync(
                follow =>
                    follow.FollowerId == request.FollowerId
                    && follow.FollowedId == currentUserId
                    && follow.Status == FollowStatus.Pending,
                cancellationToken
            );

        if (follow == null)
            return TypedResults.NotFound();

        follow.Status = FollowStatus.Accepted;
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
