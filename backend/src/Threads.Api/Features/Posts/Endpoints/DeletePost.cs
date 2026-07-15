using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Features.Posts.Endpoints;

public class DeletePost : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapDelete("/{postId}", Handle)
            .WithValidation<Request>()
            .WithSummary("Deletes a post");
    }

    public record Request(Guid PostId);

    public class DeletePostValidator : AbstractValidator<Request>
    {
        public DeletePostValidator()
        {
            RuleFor(x => x.PostId).NotEmpty();
        }
    }

    private static async Task<Results<NoContent, ForbidHttpResult>> Handle(
        [AsParameters] Request request,
        AppDbContext db,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        var post = await db.Posts.FirstOrDefaultAsync(
            post => post.Id == request.PostId,
            cancellationToken
        );
        if (post == null)
            return TypedResults.NoContent();

        var currentUserId = claimsPrincipal.GetUserId();
        if (post.UserId != currentUserId)
            return TypedResults.Forbid();

        post.IsDeleted = true;
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
