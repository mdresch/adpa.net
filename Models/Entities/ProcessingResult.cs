using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADPA.Models.Entities;

/// <summary>
/// Processing result entity containing extracted data and analysis
/// </summary>
public class ProcessingResult
{
    /// <summary>
    /// Unique identifier for the processing result
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// ID of the document this result belongs to
    /// </summary>
    [Required]
    public Guid DocumentId { get; set; }

    /// <summary>
    /// Type of processing performed
    /// </summary>
    [Required]
    [StringLength(50)]
    public string ProcessingType { get; set; } = string.Empty;

    /// <summary>
    /// Extracted text content from the document
    /// </summary>
    [Column(TypeName = "ntext")]
    public string? ExtractedText { get; set; }

    /// <summary>
    /// Document metadata as JSON
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? Metadata { get; set; }

    /// <summary>
    /// Analytics results as JSON
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? Analytics { get; set; }

    /// <summary>
    /// Confidence score of the processing (0-1)
    /// </summary>
    public double? ConfidenceScore { get; set; }

    /// <summary>
    /// Time taken to process in milliseconds
    /// </summary>
    public int ProcessingTimeMs { get; set; }

    /// <summary>
    /// Error message if processing failed
    /// </summary>
    [StringLength(1000)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Version of the processing engine used
    /// </summary>
    [StringLength(20)]
    public string? ProcessingVersion { get; set; }

    /// <summary>
    /// When this processing was performed
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property to the document
    /// </summary>
    [ForeignKey(nameof(DocumentId))]
    public virtual Document Document { get; set; } = null!;
}

/// <summary>
/// Processing type enumeration
/// </summary>
public enum ProcessingType
{
    TextExtraction,
    OCR,
    Classification,
    SentimentAnalysis,
    EntityExtraction,
    Summarization,
    Translation
}