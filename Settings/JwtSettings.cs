namespace TeachingAPI.Settings;

/// <summary>
/// Strongly-typed view of the "Jwt" section in appsettings.json.
/// Binding config to a class (the "Options pattern") avoids magic strings all over the code.
/// </summary>
public class JwtSettings
{
    /// <summary>Who issues the token (this API).</summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>Who the token is meant for (the API's clients).</summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>Secret used to sign tokens. Must be at least 32 characters for HS256.
    /// In real apps keep it in user-secrets / environment variables / a vault, never in git.</summary>
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>How long a token stays valid.</summary>
    public int ExpiryMinutes { get; set; } = 60;
}
