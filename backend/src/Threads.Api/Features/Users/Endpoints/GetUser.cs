using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Features.Users.Endpoints;

public class GetUser : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapGet("/{userId}", Handle)
            .WithValidation<Request>()
            .WithSummary("Retrieves a single user profile");
    }

    public record Request(Guid UserId);

    public class GetUserValidator : AbstractValidator<Request>
    {
        public GetUserValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
        }
    }

    private static async Task<Results<Ok<UserProfileDto>, NotFound>> Handle(
        [AsParameters] Request request,
        AppDbContext db,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        var usersQuery = db.Users.Where(user => user.Id == request.UserId);
        Guid? currentUserId = null;

        if (claimsPrincipal.Identity?.IsAuthenticated == true)
        {
            currentUserId = claimsPrincipal.GetUserId();
            usersQuery = usersQuery.WhereVisibleTo(currentUserId.Value, db);
        }

        var user = await usersQuery
            .ToUserProfileDto(currentUserId, db)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
            return TypedResults.NotFound();

        return TypedResults.Ok(user);
    }
}
