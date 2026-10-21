using Threads.Api.Features.Auth.Endpoints;

namespace Threads.Api.UnitTests.Auth.Validator;

public static class AuthRequestFactory
{
    public static Login.Request CreateLoginRequest(
        string? email = AuthValidatorTestData.ValidEmail,
        string? password = AuthValidatorTestData.ValidPassword
    )
    {
        return new(email!, password!);
    }

    public static Signup.Request CreateSignupRequest(
        string? email = AuthValidatorTestData.ValidEmail,
        string? username = AuthValidatorTestData.ValidUsername,
        string? password = AuthValidatorTestData.ValidPassword,
        bool isPrivate = false,
        string? bio = null
    )
    {
        return new(email!, username!, password!, isPrivate, bio);
    }

    public static RenewToken.Request CreateRenewRefreshTokenRequest(
        string? plainToken = AuthValidatorTestData.ValidRefreshToken
    )
    {
        return new(plainToken!);
    }

    public static Logout.Request CreateLogoutRequest(
        string? plainToken = AuthValidatorTestData.ValidRefreshToken
    )
    {
        return new(plainToken!);
    }
}
