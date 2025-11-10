using System.ComponentModel.DataAnnotations;

namespace ADPA.Models.DTOs;

/// <summary>
/// DTO for document upload
/// </summary>
public class DocumentUploadDto
{
    [Required]
    public IFormFile File { get; set; } = null!;
    
    [StringLength(500)]
    public string? Description { get; set; }
}

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
/// Data Transfer Object for creating new data processing requests (Legacy)
/// </summary>
public class CreateDataRequestDto
{
    /// <summary>
    /// The raw data to be processed
    /// </summary>
    public string RawData { get; set; } = string.Empty;
}

/// <summary>
/// Data Transfer Object for data processing responses (Legacy)
/// </summary>
public class DataResponseDto
{
    public int Id { get; set; }
    public string RawData { get; set; } = string.Empty;
    public string ProcessedResult { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Health check response DTO
/// </summary>
public class HealthCheckResponseDto
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Version { get; set; } = string.Empty;
    public Dictionary<string, object> Details { get; set; } = new();
}