using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Common.Extensions;
using Threads.Api.Common.Pagination;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Features.Blocks;
using Threads.Api.Features.Posts;

namespace Threads.Api.Features.Likes.Endpoints;

public class GetPostLikes : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapGet("/{postId}/likes", Handle)
            .WithValidation<Request>()
            .WithSummary("Retrieves users who liked a post");
    }

    public record Request(Guid PostId, string? Cursor, int PageSize = 20);

    public class GetPostLikesValidator : AbstractValidator<Request>
    {
        public GetPostLikesValidator()
        {
            RuleFor(x => x.PostId).NotEmpty();
        }
    }

    private static async Task<Results<Ok<PagedResponse<PostLikeUserDto>>, NotFound>> Handle(
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
            .FirstOrDefaultAsync(cancellationToken);

        if (post == null)
            return TypedResults.NotFound();

        var pagedRequest = new PagedRequest(request.Cursor, request.PageSize);
        var response = await db
            .PostLikes.Where(like => like.PostId == post.Id)
            .WhereOwnerHasNoBlockRelationshipWith(currentUserId, db)
            .ToPostLikeUserDto()
            .ToPagedResponse(pagedRequest, cancellationToken);

        return TypedResults.Ok(response);
    }
}
