using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Threads.Api.Common.Extensions;
using Threads.Api.Data.Posts;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Features.Posts.Endpoints;

public class CreatePost : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapPost("", Handle).WithValidation<Request>().WithSummary("Creates a post");
    }

    public record Request(string Content);

    public class CreatePostValidator : AbstractValidator<Request>
    {
        public CreatePostValidator()
        {
            RuleFor(x => x.Content).NotEmpty().MaximumLength(600);
        }
    }

    private static async Task<Created<PostDto>> Handle(
        [FromBody] Request request,
        AppDbContext db,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();

        var post = new Post
        {
            UserId = userId,
            Content = request.Content,
            CreatedAtUtc = DateTime.UtcNow,
        };

        await db.Posts.AddAsync(post, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await db.Entry(post).Reference(p => p.User).LoadAsync(cancellationToken);

        PostDto response = post.ToDto();

        return TypedResults.Created($"/posts/{post.Id}", response);
    }
}
