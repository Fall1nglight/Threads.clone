using Threads.Api.Features.Auth.Endpoints;

namespace Threads.Api.UnitTests.Auth.Validator;

internal static class AuthValidatorTestData
{
    public const string ValidEmail = "alice@example.test";
    public const string ValidUsername = "alice";
    public const string ValidPassword = "Test123!";
    public const string ValidRefreshToken = "fTzm7xPpnCuDC+vYOhr170XRhkJEonJx+UHQ/j1t2SY=";

    public static TheoryData<Signup.Request, string> InvalidSignupRequests
    {
        get
        {
            return new()
            {
                {
                    AuthRequestFactory.CreateSignupRequest(email: null),
                    nameof(Signup.Request.Email)
                },
                { AuthRequestFactory.CreateSignupRequest(email: ""), nameof(Signup.Request.Email) },
                {
                    AuthRequestFactory.CreateSignupRequest(email: "invalid-email"),
                    nameof(Signup.Request.Email)
                },
                {
                    AuthRequestFactory.CreateSignupRequest(
                        email: $"{new string('a', 257)}@example.test"
                    ),
                    nameof(Signup.Request.Email)
                },
                {
                    AuthRequestFactory.CreateSignupRequest(username: null),
                    nameof(Signup.Request.Username)
                },
                {
                    AuthRequestFactory.CreateSignupRequest(username: ""),
                    nameof(Signup.Request.Username)
                },
                {
                    AuthRequestFactory.CreateSignupRequest(username: "ab"),
                    nameof(Signup.Request.Username)
                },
                {
                    AuthRequestFactory.CreateSignupRequest(username: new string('a', 31)),
                    nameof(Signup.Request.Username)
                },
                {
                    AuthRequestFactory.CreateSignupRequest(username: "bad-name"),
                    nameof(Signup.Request.Username)
                },
                {
                    AuthRequestFactory.CreateSignupRequest(password: null),
                    nameof(Signup.Request.Password)
                },
                {
                    AuthRequestFactory.CreateSignupRequest(password: ""),
                    nameof(Signup.Request.Password)
                },
                {
                    AuthRequestFactory.CreateSignupRequest(password: "Short1!"),
                    nameof(Signup.Request.Password)
                },
                {
                    AuthRequestFactory.CreateSignupRequest(password: "test123!"),
                    nameof(Signup.Request.Password)
                },
                {
                    AuthRequestFactory.CreateSignupRequest(password: "TEST123!"),
                    nameof(Signup.Request.Password)
                },
                {
                    AuthRequestFactory.CreateSignupRequest(password: "TestTest!"),
                    nameof(Signup.Request.Password)
                },
                {
                    AuthRequestFactory.CreateSignupRequest(password: "Test1234"),
                    nameof(Signup.Request.Password)
                },
                {
                    AuthRequestFactory.CreateSignupRequest(password: $"A1!{new string('a', 98)}"),
                    nameof(Signup.Request.Password)
                },
                {
                    AuthRequestFactory.CreateSignupRequest(bio: new string('a', 301)),
                    nameof(Signup.Request.Bio)
                },
            };
        }
    }

    public static TheoryData<Login.Request, string> InvalidLoginRequests
    {
        get
        {
            return new()
            {
                { AuthRequestFactory.CreateLoginRequest(email: null), nameof(Login.Request.Email) },
                { AuthRequestFactory.CreateLoginRequest(email: ""), nameof(Login.Request.Email) },
                {
                    AuthRequestFactory.CreateLoginRequest(email: "invalid-email"),
                    nameof(Login.Request.Email)
                },
                {
                    AuthRequestFactory.CreateLoginRequest(
                        email: $"{new string('a', 257)}@example.test"
                    ),
                    nameof(Login.Request.Email)
                },
                {
                    AuthRequestFactory.CreateLoginRequest(password: null),
                    nameof(Login.Request.Password)
                },
                {
                    AuthRequestFactory.CreateLoginRequest(password: ""),
                    nameof(Login.Request.Password)
                },
                {
                    AuthRequestFactory.CreateLoginRequest(password: "Short1!"),
                    nameof(Login.Request.Password)
                },
                {
                    AuthRequestFactory.CreateLoginRequest(password: "test123!"),
                    nameof(Login.Request.Password)
                },
                {
                    AuthRequestFactory.CreateLoginRequest(password: "TEST123!"),
                    nameof(Login.Request.Password)
                },
                {
                    AuthRequestFactory.CreateLoginRequest(password: "TestTest!"),
                    nameof(Login.Request.Password)
                },
                {
                    AuthRequestFactory.CreateLoginRequest(password: "Test1234"),
                    nameof(Login.Request.Password)
                },
                {
                    AuthRequestFactory.CreateLoginRequest(password: $"A1!{new string('a', 98)}"),
                    nameof(Login.Request.Password)
                },
            };
        }
    }

    public static TheoryData<string?> InvalidRefreshTokenRequests
    {
        get { return [null, "", "short", new('a', 43), new('a', 45)]; }
    }
}
