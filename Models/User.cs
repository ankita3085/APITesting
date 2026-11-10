namespace TeachingAPI.Models;

/// <summary>An API user. We never store plain-text passwords, only a hash.</summary>
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>"Admin" or "User". Used for role-based authorization.</summary>
    public string Role { get; set; } = "User";
}
