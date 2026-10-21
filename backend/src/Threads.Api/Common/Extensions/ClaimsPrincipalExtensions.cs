using System.Security.Claims;

namespace Threads.Api.Common.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal claimsPrincipal)
    {
        var claim = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (claim == null || !Guid.TryParse(claim, out var userId))
            throw new Exception("Failed to read userId");

        return userId;
    }

    public static Guid? TryGetUserId(this ClaimsPrincipal claimsPrincipal)
    {
        var claim = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (claim == null || !Guid.TryParse(claim, out var userId))
            return null;

        return userId;
    }
}
