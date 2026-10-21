using Microsoft.AspNetCore.Identity;
using Threads.Api.Data.Users;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Users.TestSupport;

public sealed class UserTestSeeder
{
    private readonly UserManager<User> _userManager;

    public UserTestSeeder(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<User> CreateUserAsync(
        string username,
        string? email = null,
        string password = IntegrationTestData.ValidPassword,
        bool isPrivate = false,
        string? bio = null,
        DateTime? createdAtUtc = null,
        DateTime? updatedAtUtc = null
    )
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = username,
            Email = email ?? $"{username}@example.test",
            Bio = bio,
            IsPrivate = isPrivate,
            CreatedAtUtc = createdAtUtc ?? IntegrationTestData.BaseTime,
            UpdatedAtUtc = updatedAtUtc,
        };

        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
            return user;

        var errors = string.Join(" | ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"Failed to create test user '{username}': {errors}");
    }

    public async Task<List<User>> SeedBulkUsersAsync(int count)
    {
        var users = new List<User>();

        for (var i = 0; i < count; i++)
        {
            var user = await CreateUserAsync(
                username: $"{UserTestData.BulkUsernamePrefix}_{i:D3}",
                createdAtUtc: IntegrationTestData.BaseTime.AddMinutes(i)
            );

            users.Add(user);
        }

        return users;
    }
}
