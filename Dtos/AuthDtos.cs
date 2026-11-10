using System.ComponentModel.DataAnnotations;

namespace TeachingAPI.Dtos;

/// <summary>Body of POST /api/auth/register and POST /api/auth/login.</summary>
public class CredentialsRequest
{
    [Required, StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}

/// <summary>Returned after a successful login. The client sends the token back in the
/// "Authorization: Bearer {token}" header on every later request.</summary>
public record TokenResponse(string Token, DateTime ExpiresAtUtc);
