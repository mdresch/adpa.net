namespace ADPA.Models;

/// <summary>
/// Represents data to be processed by the ADPA system
/// </summary>
public class ProcessedData
{
    public int Id { get; set; }
    
    /// <summary>
    /// The raw input data
    /// </summary>
    public string RawData { get; set; } = string.Empty;
    
    /// <summary>
    /// The processed result
    /// </summary>
    public string ProcessedResult { get; set; } = string.Empty;
    
    /// <summary>
    /// Processing status
    /// </summary>
    public ProcessingStatus Status { get; set; }
    
    /// <summary>
    /// Timestamp when processing was initiated
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Timestamp when processing was completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    
    /// <summary>
    /// Error message if processing failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Processing status enumeration
/// </summary>
public enum ProcessingStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3
}