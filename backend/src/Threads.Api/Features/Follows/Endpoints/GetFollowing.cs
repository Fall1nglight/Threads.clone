using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Common.Extensions;
using Threads.Api.Common.Pagination;
using Threads.Api.Data.Follows;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Features.Users;

namespace Threads.Api.Features.Follows.Endpoints;

public class GetFollowing : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapGet("/{userId}/following", Handle)
            .WithValidation<Request>()
            .WithSummary("Retrieves followed users");
    }

    public record Request(Guid UserId, string? Cursor, int PageSize = PaginationDefaults.PageSize)
        : PagedRequest(Cursor, PageSize);

    public class GetFollowingValidator : AbstractValidator<Request>
    {
        public GetFollowingValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
        }
    }

    private static async Task<Results<Ok<PagedResponse<FollowUserDto>>, NotFound>> Handle(
        [AsParameters] Request request,
        AppDbContext db,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        var loggedInUserId = claimsPrincipal.TryGetUserId();

        var isUserVisible = await db
            .Users.Where(x => x.Id == request.UserId)
            .WhereVisibleTo(loggedInUserId, db)
            .AnyAsync(cancellationToken);

        if (!isUserVisible)
            return TypedResults.NotFound();

        var users = await db
            .Follows.Where(follow =>
                follow.FollowerId == request.UserId && follow.Status == FollowStatus.Accepted
            )
            .WhereParticipantsHaveNoBlockRelationship(db)
            .ToFollowedDto(loggedInUserId, db)
            .ToPagedResponse(request, cancellationToken);

        return TypedResults.Ok(users);
    }
}
