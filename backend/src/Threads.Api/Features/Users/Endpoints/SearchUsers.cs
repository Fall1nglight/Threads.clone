using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Common.Extensions;
using Threads.Api.Common.Pagination;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Features.Users.Endpoints;

public class SearchUsers : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapGet("/search", Handle)
            .WithValidation<Request>()
            .WithSummary("Searches user profiles by username");
    }

    public record Request(
        string? Username,
        string? Cursor,
        int PageSize = PaginationDefaults.PageSize
    ) : PagedRequest(Cursor, PageSize);

    public class SearchUsersValidator : AbstractValidator<Request>
    {
        public SearchUsersValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .MinimumLength(3)
                .MaximumLength(30)
                .Matches("^[a-zA-Z0-9_.]+$");
        }
    }

    private static async Task<Ok<PagedResponse<UserProfileDto>>> Handle(
        [AsParameters] Request request,
        AppDbContext db,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        var loggedInUserId = claimsPrincipal.TryGetUserId();
        var escapedUsername = EscapeLikePattern(request.Username!);
        var searchPattern = $"%{escapedUsername}%";

        var users = await db
            .Users.Where(user => EF.Functions.ILike(user.UserName!, searchPattern, "\\"))
            .WhereVisibleTo(loggedInUserId, db)
            .ToDto(loggedInUserId, db)
            .ToPagedResponse(request, cancellationToken);

        return TypedResults.Ok(users);
    }

    private static string EscapeLikePattern(string value)
    {
        return value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
    }
}
