using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechRoad.API.Models.DTOs;
using TechRoad.API.Services;

namespace TechRoad.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Register([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (success, message, response) = _authService.Register(request);
        if (!success)
        {
            return BadRequest(new { message });
        }

        return Ok(response);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (success, message, response) = _authService.Login(request);
        if (!success)
        {
            return Unauthorized(new { message });
        }

        return Ok(response);
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetCurrentUser()
    {
        var username = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized(new { message = "Invalid user token claims." });
        }

        var profile = _authService.GetProfile(username);
        if (profile == null)
        {
            return NotFound(new { message = "User not found." });
        }

        return Ok(profile);
    }
}