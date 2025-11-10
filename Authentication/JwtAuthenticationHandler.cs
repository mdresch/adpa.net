using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using ADPA.Services;
using Microsoft.Extensions.Options;

namespace ADPA.Authentication;

/// <summary>
/// Custom JWT authentication handler for ADPA API
/// </summary>
public class JwtAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IAuthService _authService;
    private readonly ILogger<JwtAuthenticationHandler> _logger;

    public JwtAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IAuthService authService) 
        : base(options, logger, encoder, clock)
    {
        _authService = authService;
        _logger = logger.CreateLogger<JwtAuthenticationHandler>();
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if Authorization header exists
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return AuthenticateResult.NoResult();
        }

        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return AuthenticateResult.Fail("Invalid authorization header format");
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();
        
        try
        {
            // For now, we'll implement a simple token validation
            // In a real implementation, you'd validate JWT tokens properly
            var isValid = await ValidateTokenAsync(token);
            
            if (!isValid)
            {
                return AuthenticateResult.Fail("Invalid or expired token");
            }

            // Extract user info from token (simplified implementation)
            var userClaims = await ExtractClaimsFromToken(token);
            
            var identity = new ClaimsIdentity(userClaims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            _logger.LogInformation("✅ User authenticated successfully: {UserId}", 
                userClaims.FirstOrDefault(c => c.Type == "sub")?.Value);

            return AuthenticateResult.Success(ticket);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "❌ Authentication failed for token: {Token}", token.Substring(0, Math.Min(10, token.Length)));
            return AuthenticateResult.Fail("Token validation failed");
        }
    }

    /// <summary>
    /// Validate the provided token
    /// </summary>
    private async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            // Simple token validation - in production use proper JWT validation
            if (string.IsNullOrEmpty(token) || token.Length < 10)
                return false;

            // For demo purposes, we'll accept tokens that start with "valid_"
            // In production, implement proper JWT signature validation
            if (token.StartsWith("demo_token_") || token.StartsWith("valid_"))
                return true;

            // Try to validate with auth service
            var result = await _authService.ValidateTokenAsync(token);
            return result;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Extract claims from the token
    /// </summary>
    private async Task<IEnumerable<Claim>> ExtractClaimsFromToken(string token)
    {
        var claims = new List<Claim>();

        try
        {
            // Simple token parsing - in production, parse actual JWT claims
            if (token.StartsWith("demo_token_"))
            {
                var userId = token.Replace("demo_token_", "");
                claims.Add(new Claim("sub", userId));
                claims.Add(new Claim("name", "Demo User"));
                claims.Add(new Claim("email", "demo@adpa.local"));
                claims.Add(new Claim("role", "User"));
            }
            else if (token.StartsWith("admin_token_"))
            {
                var userId = token.Replace("admin_token_", "");
                claims.Add(new Claim("sub", userId));
                claims.Add(new Claim("name", "Admin User"));
                claims.Add(new Claim("email", "admin@adpa.local"));
                claims.Add(new Claim("role", "Admin"));
            }
            else
            {
                // Default claims for valid tokens
                claims.Add(new Claim("sub", Guid.NewGuid().ToString()));
                claims.Add(new Claim("name", "API User"));
                claims.Add(new Claim("role", "User"));
            }

            claims.Add(new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()));
            claims.Add(new Claim("exp", DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds().ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract claims from token");
            // Return minimal claims
            claims.Add(new Claim("sub", "anonymous"));
            claims.Add(new Claim("role", "Guest"));
        }

        return claims;
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 401;
        Response.ContentType = "application/json";
        
        var errorResponse = new
        {
            success = false,
            message = "Authentication required",
            timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(errorResponse);
        await Response.WriteAsync(json);
    }

    protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 403;
        Response.ContentType = "application/json";
        
        var errorResponse = new
        {
            success = false,
            message = "Access forbidden - insufficient permissions",
            timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(errorResponse);
        await Response.WriteAsync(json);
    }
}