using System.Collections.Concurrent;
using Microsoft.AspNetCore.Identity;
using TeachingAPI.Models;

namespace TeachingAPI.Services;

/// <summary>In-memory user store with hashed passwords.</summary>
public class InMemoryUserService : IUserService
{
    // Usernames are case-insensitive: "Bob" and "bob" are the same user.
    private readonly ConcurrentDictionary<string, User> _users = new(StringComparer.OrdinalIgnoreCase);
    private readonly PasswordHasher<User> _hasher = new();   // salted + slow hashing, built into ASP.NET Core
    private int _lastId;

    public InMemoryUserService()
    {
        // Demo admin so you can log in right away. Do NOT do this in production.
        Register("admin", "Admin123!", "Admin");
    }

    public User? Register(string username, string password, string role = "User")
    {
        var user = new User { Id = Interlocked.Increment(ref _lastId), Username = username, Role = role };
        user.PasswordHash = _hasher.HashPassword(user, password);

        // TryAdd returns false if the key already exists -> username taken.
        return _users.TryAdd(username, user) ? user : null;
    }

    public User? ValidateCredentials(string username, string password)
    {
        if (!_users.TryGetValue(username, out var user))
            return null;

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result == PasswordVerificationResult.Failed ? null : user;
    }
}
