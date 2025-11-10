using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ADPA.Services.Security;
using AuthenticationService = ADPA.Services.Security.IAuthenticationService;

namespace ADPA.Controllers;

/// <summary>
/// Phase 5: Enhanced Authentication Controller
/// Comprehensive authentication API with MFA, OAuth, biometric support, and security features
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthenticationController : ControllerBase
{
    private readonly AuthenticationService _authService;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(
        AuthenticationService authService,
        ILogger<AuthenticationController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate user with username/password and optional MFA
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.AuthenticateAsync(request);
            
            if (!response.IsSuccessful)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for user {Username}", request.Username);
            return StatusCode(500, new { message = "Authentication failed" });
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var response = await _authService.RefreshTokenAsync(request.RefreshToken);
            
            if (!response.IsSuccessful)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            return StatusCode(500, new { message = "Token refresh failed" });
        }
    }

    /// <summary>
    /// Logout current session
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout()
    {
        try
        {
            var sessionIdClaim = User.FindFirst("session_id")?.Value;
            if (Guid.TryParse(sessionIdClaim, out var sessionId))
            {
                await _authService.LogoutAsync(sessionId);
            }

            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout failed");
            return StatusCode(500, new { message = "Logout failed" });
        }
    }

    /// <summary>
    /// Logout all sessions for current user
    /// </summary>
    [HttpPost("logout-all")]
    [Authorize]
    public async Task<ActionResult> LogoutAllSessions()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                await _authService.LogoutAllSessionsAsync(userId);
            }

            return Ok(new { message = "All sessions logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout all sessions failed");
            return StatusCode(500, new { message = "Logout failed" });
        }
    }

    /// <summary>
    /// Change user password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user" });
            }

            var success = await _authService.ChangePasswordAsync(userId, request);
            
            if (!success)
            {
                return BadRequest(new { message = "Password change failed" });
            }

            return Ok(new { message = "Password changed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Password change failed for user {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return StatusCode(500, new { message = "Password change failed" });
        }
    }

    /// <summary>
    /// Request password reset
    /// </summary>
    [HttpPost("reset-password-request")]
    [AllowAnonymous]
    public async Task<ActionResult> RequestPasswordReset([FromBody] ResetPasswordRequest request)
    {
        try
        {
            var user = await _authService.GetUserByEmailAsync(request.Email);
            if (user != null)
            {
                var token = await _authService.GeneratePasswordResetTokenAsync(user.Id);
                // Send email with token (implementation depends on email service)
                // For security, always return success even if email doesn't exist
            }

            return Ok(new { message = "If the email exists, a reset link has been sent" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Password reset request failed for email {Email}", request.Email);
            return StatusCode(500, new { message = "Reset request failed" });
        }
    }

    /// <summary>
    /// Reset password with token
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult> ResetPassword([FromBody] SetNewPasswordRequest request)
    {
        try
        {
            var success = await _authService.ResetPasswordAsync(request);
            
            if (!success)
            {
                return BadRequest(new { message = "Invalid or expired reset token" });
            }

            return Ok(new { message = "Password reset successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Password reset failed");
            return StatusCode(500, new { message = "Password reset failed" });
        }
    }

    /// <summary>
    /// Setup multi-factor authentication device
    /// </summary>
    [HttpPost("mfa/setup")]
    [Authorize]
    public async Task<ActionResult<MfaSetupResponse>> SetupMfa([FromBody] MfaSetupRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user" });
            }

            var response = await _authService.SetupMfaDeviceAsync(userId, request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MFA setup failed for user {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return StatusCode(500, new { message = "MFA setup failed" });
        }
    }

    /// <summary>
    /// Verify MFA code
    /// </summary>
    [HttpPost("mfa/verify")]
    [Authorize]
    public async Task<ActionResult> VerifyMfa([FromBody] MfaVerifyRequest request)
    {
        try
        {
            var success = await _authService.VerifyMfaAsync(request);
            
            if (!success)
            {
                return BadRequest(new { message = "Invalid MFA code" });
            }

            return Ok(new { message = "MFA verified successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MFA verification failed");
            return StatusCode(500, new { message = "MFA verification failed" });
        }
    }

    /// <summary>
    /// Disable MFA device
    /// </summary>
    [HttpDelete("mfa/device/{deviceId}")]
    [Authorize]
    public async Task<ActionResult> DisableMfaDevice(Guid deviceId)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user" });
            }

            var success = await _authService.DisableMfaDeviceAsync(userId, deviceId);
            
            if (!success)
            {
                return BadRequest(new { message = "Failed to disable MFA device" });
            }

            return Ok(new { message = "MFA device disabled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MFA device disable failed for user {UserId}, device {DeviceId}", 
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value, deviceId);
            return StatusCode(500, new { message = "MFA device disable failed" });
        }
    }

    /// <summary>
    /// Generate MFA backup codes
    /// </summary>
    [HttpPost("mfa/backup-codes")]
    [Authorize]
    public async Task<ActionResult<List<string>>> GenerateBackupCodes()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user" });
            }

            var codes = await _authService.GenerateBackupCodesAsync(userId);
            return Ok(codes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Backup codes generation failed for user {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return StatusCode(500, new { message = "Backup codes generation failed" });
        }
    }

    /// <summary>
    /// Get user's active sessions
    /// </summary>
    [HttpGet("sessions")]
    [Authorize]
    public async Task<ActionResult<List<AuthenticationSession>>> GetActiveSessions()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user" });
            }

            var sessions = await _authService.GetUserSessionsAsync(userId);
            return Ok(sessions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get sessions failed for user {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return StatusCode(500, new { message = "Failed to get sessions" });
        }
    }

    /// <summary>
    /// Revoke specific session
    /// </summary>
    [HttpDelete("sessions/{sessionId}")]
    [Authorize]
    public async Task<ActionResult> RevokeSession(Guid sessionId)
    {
        try
        {
            var success = await _authService.LogoutAsync(sessionId);
            
            if (!success)
            {
                return BadRequest(new { message = "Failed to revoke session" });
            }

            return Ok(new { message = "Session revoked successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Session revocation failed for session {SessionId}", sessionId);
            return StatusCode(500, new { message = "Session revocation failed" });
        }
    }

    /// <summary>
    /// Create API key
    /// </summary>
    [HttpPost("api-keys")]
    [Authorize]
    public async Task<ActionResult<ApiKeyResponse>> CreateApiKey([FromBody] CreateApiKeyRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user" });
            }

            var (apiKey, keyInfo) = await _authService.CreateApiKeyAsync(userId, request.Name, request.Scopes, request.ExpiresAt);
            
            return Ok(new ApiKeyResponse
            {
                ApiKey = apiKey,
                KeyInfo = keyInfo
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API key creation failed for user {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return StatusCode(500, new { message = "API key creation failed" });
        }
    }

    /// <summary>
    /// Get user's API keys
    /// </summary>
    [HttpGet("api-keys")]
    [Authorize]
    public async Task<ActionResult<List<ApiKey>>> GetApiKeys()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user" });
            }

            var apiKeys = await _authService.GetUserApiKeysAsync(userId);
            return Ok(apiKeys);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get API keys failed for user {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return StatusCode(500, new { message = "Failed to get API keys" });
        }
    }

    /// <summary>
    /// Revoke API key
    /// </summary>
    [HttpDelete("api-keys/{keyId}")]
    [Authorize]
    public async Task<ActionResult> RevokeApiKey(Guid keyId)
    {
        try
        {
            var success = await _authService.RevokeApiKeyAsync(keyId);
            
            if (!success)
            {
                return BadRequest(new { message = "Failed to revoke API key" });
            }

            return Ok(new { message = "API key revoked successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API key revocation failed for key {KeyId}", keyId);
            return StatusCode(500, new { message = "API key revocation failed" });
        }
    }

    /// <summary>
    /// Get user security status
    /// </summary>
    [HttpGet("security-status")]
    [Authorize]
    public async Task<ActionResult<SecurityStatusResponse>> GetSecurityStatus()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user" });
            }

            var status = await _authService.GetSecurityStatusAsync(userId);
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get security status failed for user {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return StatusCode(500, new { message = "Failed to get security status" });
        }
    }

    /// <summary>
    /// Get login history
    /// </summary>
    [HttpGet("login-history")]
    [Authorize]
    public async Task<ActionResult<List<LoginAttempt>>> GetLoginHistory(
        [FromQuery] int pageSize = 50,
        [FromQuery] int pageNumber = 1)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user" });
            }

            var history = await _authService.GetLoginHistoryAsync(userId, pageSize, pageNumber);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get login history failed for user {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return StatusCode(500, new { message = "Failed to get login history" });
        }
    }

    /// <summary>
    /// Start OAuth authentication
    /// </summary>
    [HttpGet("oauth/{providerId}/login")]
    [AllowAnonymous]
    public async Task<ActionResult> OAuthLogin(string providerId, [FromQuery] string? returnUrl = null)
    {
        try
        {
            var state = await _authService.GenerateOAuthStateAsync(providerId);
            // Redirect to OAuth provider
            // Implementation depends on OAuth service configuration
            
            return Ok(new { message = "OAuth login initiated", state });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OAuth login failed for provider {ProviderId}", providerId);
            return StatusCode(500, new { message = "OAuth login failed" });
        }
    }

    /// <summary>
    /// Handle OAuth callback
    /// </summary>
    [HttpPost("oauth/{providerId}/callback")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> OAuthCallback(string providerId, [FromBody] OAuthCallbackRequest request)
    {
        try
        {
            var response = await _authService.AuthenticateWithOAuthAsync(providerId, request.Code, request.State);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OAuth callback failed for provider {ProviderId}", providerId);
            return StatusCode(500, new { message = "OAuth authentication failed" });
        }
    }

    /// <summary>
    /// Enroll biometric authentication
    /// </summary>
    [HttpPost("biometric/enroll")]
    [Authorize]
    public async Task<ActionResult> EnrollBiometric([FromBody] BiometricEnrollRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user" });
            }

            var success = await _authService.EnrollBiometricAsync(userId, request.BiometricType, request.EncodedData, request.DeviceId);
            
            if (!success)
            {
                return BadRequest(new { message = "Biometric enrollment failed" });
            }

            return Ok(new { message = "Biometric enrolled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Biometric enrollment failed for user {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return StatusCode(500, new { message = "Biometric enrollment failed" });
        }
    }

    /// <summary>
    /// Authenticate with biometric
    /// </summary>
    [HttpPost("biometric/authenticate")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> BiometricAuthenticate([FromBody] BiometricAuthRequest request)
    {
        try
        {
            var success = await _authService.AuthenticateWithBiometricAsync(request.UserId, request.BiometricType, request.EncodedData, request.DeviceId);
            
            if (!success)
            {
                return BadRequest(new { message = "Biometric authentication failed" });
            }

            // Create session and tokens for successful biometric auth
            var user = await _authService.GetUserAsync(request.UserId);
            if (user == null)
            {
                return BadRequest(new { message = "User not found" });
            }

            // Implementation would continue with session creation...
            return Ok(new LoginResponse { IsSuccessful = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Biometric authentication failed for user {UserId}", request.UserId);
            return StatusCode(500, new { message = "Biometric authentication failed" });
        }
    }
}

/// <summary>
/// Supporting request/response models
/// </summary>
public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class CreateApiKeyRequest
{
    public string Name { get; set; } = string.Empty;
    public List<string> Scopes { get; set; } = new();
    public DateTime? ExpiresAt { get; set; }
}

public class ApiKeyResponse
{
    public string ApiKey { get; set; } = string.Empty;
    public ApiKey KeyInfo { get; set; } = new();
}

public class OAuthCallbackRequest
{
    public string Code { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}

public class BiometricEnrollRequest
{
    public string BiometricType { get; set; } = string.Empty;
    public string EncodedData { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
}

public class BiometricAuthRequest
{
    public Guid UserId { get; set; }
    public string BiometricType { get; set; } = string.Empty;
    public string EncodedData { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
}