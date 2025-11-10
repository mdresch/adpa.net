using System.ComponentModel.DataAnnotations;

namespace ADPA.Models.DTOs;

/// <summary>
/// DTO for user registration
/// </summary>
public class UserRegistrationDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;
}

/// <summary>
/// DTO for user login
/// </summary>
public class UserLoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// DTO for user authentication response
/// </summary>
public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expires { get; set; }
    public UserDto User { get; set; } = null!;
}

/// <summary>
/// DTO for user information
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}