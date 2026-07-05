using Microsoft.AspNetCore.Identity;
using Threads.Api.Data.Users;

namespace Threads.Api.IntegrationTests.Auth.Infrastructure;

public sealed class AuthTestUserSeeder
{
    private readonly UserManager<User> _userManager;

    public AuthTestUserSeeder(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<User> CreateUserAsync(
        string username,
        string? email = null,
        string password = AuthTestData.ValidPassword,
        bool isPrivate = false,
        string? bio = null
    )
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = username,
            Email = email ?? $"{username}@example.test",
            Bio = bio,
            IsPrivate = isPrivate,
            CreatedAtUtc = DateTime.UtcNow,
        };

        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
            return user;

        var errors = string.Join(" | ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"Failed to create test user '{username}': {errors}");
    }
}
