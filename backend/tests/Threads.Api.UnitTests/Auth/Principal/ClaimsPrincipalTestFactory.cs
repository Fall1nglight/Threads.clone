using System.Security.Claims;

namespace Threads.Api.UnitTests.Auth;

internal static class ClaimsPrincipalTestFactory
{
    public static string GetUserIdErrorMessage => "Failed to read userId";

    public static ClaimsPrincipal CreateWithUserId(Guid userId)
    {
        return CreateWithNameIdentifier(userId.ToString());
    }

    public static ClaimsPrincipal CreateWithMalformedUserId()
    {
        return CreateWithNameIdentifier("not-a-guid");
    }

    public static ClaimsPrincipal CreateWithoutUserId()
    {
        return new ClaimsPrincipal(new ClaimsIdentity(authenticationType: "Test"));
    }

    private static ClaimsPrincipal CreateWithNameIdentifier(string value)
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, value)],
            authenticationType: "Test"
        );

        return new ClaimsPrincipal(identity);
    }
}
