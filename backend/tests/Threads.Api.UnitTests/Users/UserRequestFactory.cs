using Threads.Api.Features.Users.Endpoints;
using Threads.Api.UnitTests.TestSupport;

namespace Threads.Api.UnitTests.Users;

public static class UserRequestFactory
{
    public static GetUser.Request CreateGetUserRequest(Guid? userId = null)
    {
        return new(userId ?? ValidatorTestData.ValidPrimaryGuid);
    }

    public static DeleteUser.Request CreateDeleteUserRequest(Guid? userId = null)
    {
        return new(userId ?? ValidatorTestData.ValidPrimaryGuid);
    }

    public static SearchUsers.Request CreateSearchUsersRequest(string? username = "alice")
    {
        return new(username, Cursor: null);
    }
}
