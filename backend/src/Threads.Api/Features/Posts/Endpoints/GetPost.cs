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
            .MapGet("/{postId}", Handle)
            .WithValidation<Request>()
            .WithSummary("Retrieves a single post");
    }

    public record Request(Guid PostId);

    public class GetPostValidator : AbstractValidator<Request>
    {
        public GetPostValidator()
        {
            RuleFor(x => x.PostId).NotEmpty();
        }
    }

    private static async Task<Results<Ok<PostDto>, NotFound, ForbidHttpResult>> Handle(
        [AsParameters] Request request,
        AppDbContext db,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        var currentUserId = claimsPrincipal.GetUserId();

        var post = await db
            .Posts.Where(post => post.Id == request.PostId)
            .WhereVisibleTo(currentUserId, db)
            .ToDto(currentUserId, db)
            .FirstOrDefaultAsync(cancellationToken);

        if (post == null)
            return TypedResults.NotFound();

        return TypedResults.Ok(post);
    }
}
