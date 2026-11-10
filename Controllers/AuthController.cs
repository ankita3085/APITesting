using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachingAPI.Dtos;
using TeachingAPI.Services;

namespace TeachingAPI.Controllers;

// [ApiController] turns on API-friendly behaviour: automatic model validation (400 errors),
// automatic binding of JSON bodies, etc.
[ApiController]
// [controller] is replaced by the class name minus "Controller" -> route is /api/auth
[Route("api/[controller]")]
// This controller is public: login/register must work before you have a token.
[AllowAnonymous]
[Authorize] // This controller is public: login/register must work before you have a token.
public class AuthController : ControllerBase
{
    private readonly IUserService _users;
    private readonly TokenService _tokens;

    // Constructor injection: ASP.NET Core sees what we need and supplies it from the DI container.
    public AuthController(IUserService users, TokenService tokens)
    {
        _users = users;
        _tokens = tokens;
    }

    /// <summary>POST /api/auth/register - create a new (non-admin) user.</summary>
    [HttpPost("register")]
    public IActionResult Register(CredentialsRequest request)
    {
        var user = _users.Register(request.Username, request.Password);
        if (user is null)
            return Conflict(new { message = "Username already exists." });   // 409

        return StatusCode(StatusCodes.Status201Created, new { user.Id, user.Username, user.Role });
    }

    /// <summary>POST /api/auth/login - exchange username + password for a JWT.</summary>
    [HttpPost("login")]
    public ActionResult<TokenResponse> Login(CredentialsRequest request)
    {
        var user = _users.ValidateCredentials(request.Username, request.Password);
        if (user is null)
            return Unauthorized(new { message = "Invalid username or password." });   // 401

        return _tokens.CreateToken(user);   // 200 with the token as JSON
    }
}
