using TeachingAPI.Models;

namespace TeachingAPI.Services;

public interface IUserService
{
    /// <summary>Creates a user. Returns null if the username is already taken.</summary>
    User? Register(string username, string password, string role = "User");

    /// <summary>Returns the user if username + password match, otherwise null.</summary>
    User? ValidateCredentials(string username, string password);
}
