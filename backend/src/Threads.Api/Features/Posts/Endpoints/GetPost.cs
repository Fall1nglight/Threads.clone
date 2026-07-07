using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Features.Posts.Endpoints;

public class GetPost : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapGet("/{id}", Handle)
            .WithValidation<Request>()
            .WithSummary("Retrieves a single post");
    }

    public record Request(Guid Id);

    public class GetPostValidator : AbstractValidator<Request>
    {
        public GetPostValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }

    private static async Task<Results<Ok<PostDto>, NotFound, ForbidHttpResult>> Handle(
        [AsParameters] Request request,
        AppDbContext db,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        var post = await db
            .Posts.Where(p => p.Id == request.Id)
            .Include(p => p.User)
            .FirstOrDefaultAsync(cancellationToken);

        if (post == null)
            return TypedResults.NotFound();

        var userId = claimsPrincipal.GetUserId();

        bool canViewPost = await post.CanBeViewedByUserAsync(db, userId, cancellationToken);
        if (!canViewPost)
            return TypedResults.Forbid();

        PostDto response = await db
            .Posts.Where(p => p.Id == post.Id)
            .ToDto(db, userId)
            .FirstAsync(cancellationToken);

        return TypedResults.Ok(response);
    }
}
