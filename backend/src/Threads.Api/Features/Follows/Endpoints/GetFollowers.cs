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

public class GetFollowers : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapGet("/{userId}/followers", Handle)
            .WithValidation<Request>()
            .WithSummary("Retrieves followers");
    }

    public record Request(Guid UserId, string? Cursor, int PageSize = PaginationDefaults.PageSize)
        : PagedRequest(Cursor, PageSize);

    public class GetFollowersValidator : AbstractValidator<Request>
    {
        public GetFollowersValidator()
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
                follow.FollowedId == request.UserId && follow.Status == FollowStatus.Accepted
            )
            // this may be redundant because follow relationships cancel when blocking happens
            .WhereParticipantsHaveNoBlockRelationship(db)
            .ToFollowerDto(loggedInUserId, db)
            .ToPagedResponse(request, cancellationToken);

        return TypedResults.Ok(users);
    }
}
