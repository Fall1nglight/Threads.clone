using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Features.Posts.Endpoints;

public class UpdatePost : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapPut("/{postId}", Handle).WithValidation<Request>().WithSummary("Updates a post");
    }

    public record Request(Guid PostId, [FromBody] Body Body);

    public record Body(string Content);

    public class UpdatePostValidator : AbstractValidator<Request>
    {
        public UpdatePostValidator()
        {
            RuleFor(x => x.PostId).NotEmpty();
            RuleFor(x => x.Body.Content).NotEmpty().MaximumLength(600);
        }
    }

    private static async Task<Results<NoContent, NotFound, ForbidHttpResult>> Handle(
        [AsParameters] Request request,
        ClaimsPrincipal claimsPrincipal,
        AppDbContext db,
        CancellationToken cancellationToken
    )
    {
        var post = await db.Posts.FirstOrDefaultAsync(
            post => post.Id == request.PostId,
            cancellationToken
        );
        if (post == null)
            return TypedResults.NotFound();

        var currentUserId = claimsPrincipal.GetUserId();
        if (post.UserId != currentUserId)
            return TypedResults.Forbid();

        post.Content = request.Body.Content;
        post.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
