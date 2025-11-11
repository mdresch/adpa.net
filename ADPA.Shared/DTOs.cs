using System.ComponentModel.DataAnnotations;

namespace ADPA.Shared.DTOs;

/// <summary>
/// DTO for document information
/// </summary>
public class DocumentDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? PageCount { get; set; }
    public string? DetectedLanguage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public List<ProcessingResultDto> ProcessingResults { get; set; } = new();
}

/// <summary>
/// DTO for processing results
/// </summary>
public class ProcessingResultDto
{
    public Guid Id { get; set; }
    public string ProcessingType { get; set; } = string.Empty;
    public string? ExtractedText { get; set; }
    public string? Metadata { get; set; }
    public string? Analytics { get; set; }
    public double? ConfidenceScore { get; set; }
    public int ProcessingTimeMs { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for user authentication
/// </summary>
public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// DTO for user registration
/// </summary>
public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;
}

/// <summary>
/// DTO for authentication response
/// </summary>
public class AuthResponseDto
{
    public bool Success { get; set; }
    public string Token { get; set; } = string.Empty;
    public string? Message { get; set; }
    public UserDto? User { get; set; }
}

/// <summary>
/// DTO for user information
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Processing status enumeration
/// </summary>
public enum ProcessingStatus
{
    Pending,
    InProgress,
    Completed,
    Failed,
    Cancelled
}