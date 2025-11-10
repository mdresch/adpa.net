using ADPA.Models.DTOs;
using ADPA.Models.Entities;
using ADPA.Data.Repositories;

namespace ADPA.Services;

/// <summary>
/// Interface for authentication service
/// </summary>
public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(UserLoginDto loginDto);
    Task<UserDto> RegisterAsync(UserRegistrationDto registrationDto);
    Task<UserDto?> GetUserByIdAsync(Guid userId);
    Task<bool> ValidateTokenAsync(string token);
    string GenerateJwtToken(User user);
}

/// <summary>
/// Service for handling authentication operations
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository userRepository, ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate user and return JWT token
    /// </summary>
    public async Task<AuthResponseDto?> LoginAsync(UserLoginDto loginDto)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(loginDto.Email);
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("Login failed for email: {Email} - User not found or inactive", loginDto.Email);
                return null;
            }

            if (!VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed for email: {Email} - Invalid password", loginDto.Email);
                return null;
            }

            var token = GenerateJwtToken(user);
            var expires = DateTime.UtcNow.AddHours(8); // 8 hour token expiry

            _logger.LogInformation("User logged in successfully: {Email}", loginDto.Email);

            return new AuthResponseDto
            {
                Token = token,
                Expires = expires,
                User = MapToUserDto(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", loginDto.Email);
            throw;
        }
    }

    /// <summary>
    /// Register new user
    /// </summary>
    public async Task<UserDto> RegisterAsync(UserRegistrationDto registrationDto)
    {
        try
        {
            // Check if user already exists
            var existingUser = await _userRepository.GetByEmailAsync(registrationDto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with this email already exists");
            }

            var user = new User
            {
                Email = registrationDto.Email,
                PasswordHash = HashPassword(registrationDto.Password),
                DisplayName = registrationDto.DisplayName,
                Role = "User", // Default role
                IsActive = true
            };

            var createdUser = await _userRepository.CreateAsync(user);
            
            _logger.LogInformation("New user registered: {Email}", registrationDto.Email);
            
            return MapToUserDto(createdUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email: {Email}", registrationDto.Email);
            throw;
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user != null ? MapToUserDto(user) : null;
    }

    /// <summary>
    /// Validate JWT token (simplified implementation)
    /// </summary>
    public async Task<bool> ValidateTokenAsync(string token)
    {
        await Task.CompletedTask;
        // Simplified token validation - in production use proper JWT validation
        return !string.IsNullOrEmpty(token) && token.StartsWith("Bearer ");
    }

    /// <summary>
    /// Generate JWT token (simplified implementation)
    /// </summary>
    public string GenerateJwtToken(User user)
    {
        // Simplified JWT generation - in production use proper JWT library
        var tokenData = $"{user.Id}:{user.Email}:{user.Role}:{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}";
        var tokenBytes = System.Text.Encoding.UTF8.GetBytes(tokenData);
        return $"Bearer {Convert.ToBase64String(tokenBytes)}";
    }

    /// <summary>
    /// Hash password (simplified implementation)
    /// </summary>
    private static string HashPassword(string password)
    {
        // Simple hash implementation for demo purposes
        // In production, use BCrypt or similar
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"hash_{password}_salt"));
    }

    /// <summary>
    /// Verify password against hash
    /// </summary>
    private static bool VerifyPassword(string password, string hash)
    {
        var computedHash = HashPassword(password);
        return computedHash == hash;
    }

    /// <summary>
    /// Map User entity to UserDto
    /// </summary>
    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}