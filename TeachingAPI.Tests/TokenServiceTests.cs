using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using TeachingAPI.Models;
using TeachingAPI.Services;
using TeachingAPI.Settings;

namespace TeachingAPI.Tests;

public class TokenServiceTests
{
    private static readonly JwtSettings Settings = new()
    {
        Issuer = "Iss",
        Audience = "Aud",
        SigningKey = "unit-test-signing-key-with-at-least-32-chars!!",
        ExpiryMinutes = 30
    };

    private static TokenService Create() => new(Options.Create(Settings));

    [Fact]
    public void CreateToken_ContainsExpectedClaims()
    {
        var user = new User { Id = 7, Username = "eve", Role = "Admin" };

        var response = Create().CreateToken(user);
        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(response.Token);

        Assert.Equal("7", jwt.GetClaim("sub").Value);
        Assert.Equal("eve", jwt.GetClaim("name").Value);
        Assert.Equal("Admin", jwt.GetClaim("role").Value);
        Assert.Equal("Iss", jwt.Issuer);
        Assert.Contains("Aud", jwt.Audiences);
    }

    [Fact]
    public void CreateToken_ExpiryMatchesSettings()
    {
        var before = DateTime.UtcNow.AddMinutes(30);
        var response = Create().CreateToken(new User { Id = 1, Username = "u", Role = "User" });
        var after = DateTime.UtcNow.AddMinutes(30);

        Assert.InRange(response.ExpiresAtUtc, before, after);
    }
}
