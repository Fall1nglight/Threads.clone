using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Features.Notifications.Endpoints;

namespace Threads.Api.Features.Notifications;

public class NotificationRouter : IEndpointRouter
{
    public static void MapRouter(IEndpointRouteBuilder builder)
    {
        var protectedGroup = builder.MapGroup("/notifications").RequireAuthorization();
        protectedGroup
            .MapEndpoint<GetNotifications>()
            .MapEndpoint<GetUnseenNotificationCount>()
            .MapEndpoint<MarkNotificationAsSeen>();
    }
}
