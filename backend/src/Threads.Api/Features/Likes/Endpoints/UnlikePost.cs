using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Features.Likes.Endpoints;

public class UnlikePost : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapDelete("/{postId}/likes", Handle)
            .WithValidation<Request>()
            .WithSummary("Unlikes a post");
    }

    public record Request(Guid PostId);

    public class UnlikePostValidator : AbstractValidator<Request>
    {
        public UnlikePostValidator()
        {
            RuleFor(x => x.PostId).NotEmpty();
        }
    }

    private static async Task<NoContent> Handle(
        [AsParameters] Request request,
        AppDbContext db,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();
        var postLike = await db.PostLikes.FirstOrDefaultAsync(
            like => like.PostId == request.PostId && like.UserId == userId,
            cancellationToken
        );

        if (postLike == null)
            return TypedResults.NoContent();

        db.PostLikes.Remove(postLike);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
