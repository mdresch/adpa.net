using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADPA.Models.Entities;

/// <summary>
/// Document entity representing uploaded files
/// </summary>
public class Document
{
    /// <summary>
    /// Unique identifier for the document
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// ID of the user who uploaded this document
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Original filename of the uploaded document
    /// </summary>
    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Size of the file in bytes
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// MIME type of the file
    /// </summary>
    [Required]
    [StringLength(100)]
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Path to the file in blob storage
    /// </summary>
    [StringLength(500)]
    public string? BlobPath { get; set; }

    /// <summary>
    /// Processing status of the document
    /// </summary>
    [Required]
    public ProcessingStatus Status { get; set; } = ProcessingStatus.Pending;

    /// <summary>
    /// MD5 hash of the file for duplicate detection
    /// </summary>
    [StringLength(32)]
    public string? FileHash { get; set; }

    /// <summary>
    /// Number of pages (for multi-page documents)
    /// </summary>
    public int? PageCount { get; set; }

    /// <summary>
    /// Detected language of the document
    /// </summary>
    [StringLength(10)]
    public string? DetectedLanguage { get; set; }

    /// <summary>
    /// When the document was uploaded
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When processing was completed (if applicable)
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Navigation property to the user who uploaded this document
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Navigation property to the processing results
    /// </summary>
    public virtual ICollection<ProcessingResult> ProcessingResults { get; set; } = new List<ProcessingResult>();
}

/// <summary>
/// Document processing status enumeration
/// </summary>
public enum ProcessingStatus
{
    /// <summary>
    /// Document is waiting to be processed
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Document is currently being processed
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Document processing completed successfully
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Document processing failed
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Document processing was cancelled
    /// </summary>
    Cancelled = 4
}