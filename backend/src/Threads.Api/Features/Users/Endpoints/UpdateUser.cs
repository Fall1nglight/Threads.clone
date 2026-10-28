using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Threads.Api.Common.Extensions;
using Threads.Api.Data.Follows;
using Threads.Api.Data.Notifications;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Data.Users;

namespace Threads.Api.Features.Users.Endpoints;

public class UpdateUser : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapPut("/{Id}", Handle).WithValidation<Request>().WithSummary("Update User");
    }

    public record Request(Guid Id, [FromBody] Body Body);

    public record Body(bool IsPrivate, string? Bio, string? CurrentPassword, string? NewPassword);

    public class UpdateUserValidator : AbstractValidator<Request>
    {
        public UpdateUserValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Body.Bio).MaximumLength(300);
        }
    }

    private static async Task<Results<NoContent, ForbidHttpResult>> Handle(
        [AsParameters] Request request,
        ClaimsPrincipal claimsPrincipal,
        AppDbContext db,
        UserManager<User> userManager,
        CancellationToken cancellationToken
    )
    {
        Guid loggedInUserId = claimsPrincipal.GetUserId();
        if (request.Id != loggedInUserId)
            return TypedResults.Forbid();

        IDbContextTransaction transaction = await db.Database.BeginTransactionAsync(
            cancellationToken
        );

        User? user = await userManager.FindByIdAsync(loggedInUserId.ToString());
        if (user == null)
            return TypedResults.Forbid();

        // private -> public
        if (user.IsPrivate && !request.Body.IsPrivate)
        {
            // updates PENDING follow requests to ACCEPTED
            await UpdateFollowRequests(db, cancellationToken, loggedInUserId);

            // updates FollowRequestReceived notifications to Followed
            await UpdateNotifications(db, cancellationToken, loggedInUserId);
        }

        user.IsPrivate = request.Body.IsPrivate;
        user.Bio = request.Body.Bio;
        user.UpdatedAtUtc = DateTime.UtcNow;

        await userManager.UpdateAsync(user);
        await transaction.CommitAsync(cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task UpdateFollowRequests(
        AppDbContext db,
        CancellationToken cancellationToken,
        Guid loggedInUserId
    )
    {
        await db
            .Follows.Where(x => x.FollowedId == loggedInUserId && x.Status == FollowStatus.Pending)
            .ExecuteUpdateAsync(
                builder => builder.SetProperty(follow => follow.Status, FollowStatus.Accepted),
                cancellationToken
            );
    }

    private static async Task UpdateNotifications(
        AppDbContext db,
        CancellationToken cancellationToken,
        Guid loggedInUserId
    )
    {
        await db
            .Notifications.Where(x =>
                x.NotifiedUserId == loggedInUserId
                && x.Type == NotificationType.FollowRequestReceived
            )
            .ExecuteUpdateAsync(
                builder =>
                    builder.SetProperty(
                        notification => notification.Type,
                        NotificationType.Followed
                    ),
                cancellationToken
            );
    }
}
