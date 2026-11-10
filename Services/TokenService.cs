using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TeachingAPI.Dtos;
using TeachingAPI.Models;
using TeachingAPI.Settings;

namespace TeachingAPI.Services;

/// <summary>Creates signed JWT access tokens.</summary>
public class TokenService
{
    private readonly JwtSettings _settings;

    // IOptions<JwtSettings> is injected by the DI container (see Program.cs).
    public TokenService(IOptions<JwtSettings> options) => _settings = options.Value;

    public TokenResponse CreateToken(User user)
    {
        var expires = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SigningKey));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            Expires = expires,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),

            // Claims = facts about the user, stored inside the token.
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = user.Id.ToString(),
                ["name"] = user.Username,
                ["role"] = user.Role
            }
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);
        return new TokenResponse(token, expires);
    }
}
