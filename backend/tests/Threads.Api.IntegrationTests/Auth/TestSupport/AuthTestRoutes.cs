namespace Threads.Api.IntegrationTests.Auth.TestSupport;

internal static class AuthTestRoutes
{
    public const string Base = "/auth";
    public const string Signup = $"{Base}/signup";
    public const string Login = $"{Base}/login";
    public const string RenewToken = $"{Base}/renew-token";
    public const string Logout = $"{Base}/logout";
    public const string LogoutAll = $"{Base}/logout-all";
}
