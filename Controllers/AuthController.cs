using Microsoft.AspNetCore.Mvc;
using ADPA.Services;
using ADPA.Models.DTOs;

namespace ADPA.Controllers;

/// <summary>
/// Controller for authentication operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// User login
    /// </summary>
    /// <param name="loginDto">Login credentials</param>
    /// <returns>Authentication response with JWT token</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] UserLoginDto loginDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var authResponse = await _authService.LoginAsync(loginDto);
            if (authResponse == null)
            {
                return Unauthorized("Invalid email or password");
            }

            return Ok(authResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// User registration
    /// </summary>
    /// <param name="registrationDto">Registration details</param>
    /// <returns>Created user information</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<UserDto>> Register([FromBody] UserRegistrationDto registrationDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _authService.RegisterAsync(registrationDto);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get user information
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User information</returns>
    [HttpGet("user/{id}")]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        try
        {
            var user = await _authService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found");
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user: {UserId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Validate authentication token
    /// </summary>
    /// <returns>Token validation result</returns>
    [HttpPost("validate")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult> ValidateToken()
    {
        try
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader))
            {
                return Unauthorized("Authorization header missing");
            }

            var isValid = await _authService.ValidateTokenAsync(authHeader);
            if (!isValid)
            {
                return Unauthorized("Invalid token");
            }

            return Ok("Token is valid");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return StatusCode(500, "Internal server error");
        }
    }
}