using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ADPA.Services;

namespace ADPA.Controllers;

/// <summary>
/// Advanced document processing controller for Phase 2 Enhanced Processing capabilities
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdvancedProcessingController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly ILogger<AdvancedProcessingController> _logger;

    public AdvancedProcessingController(
        IDocumentService documentService,
        ILogger<AdvancedProcessingController> logger)
    {
        _documentService = documentService;
        _logger = logger;
    }

    /// <summary>
    /// Process document using Phase 2 Enhanced Processing capabilities
    /// </summary>
    /// <param name="documentId">Document ID to process</param>
    /// <returns>Advanced processing result with extracted text and metadata</returns>
    [HttpPost("{documentId:guid}/process-advanced")]
    [ProducesResponseType(typeof(ProcessingResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ProcessDocumentAdvanced(Guid documentId)
    {
        try
        {
            _logger.LogInformation("🚀 Advanced processing request received for document {DocumentId}", documentId);

            // Validate document ID
            if (documentId == Guid.Empty)
            {
                return BadRequest("Invalid document ID: Document ID cannot be empty");
            }

            // Process document with advanced capabilities
            var result = await _documentService.ProcessDocumentWithAdvancedServiceAsync(documentId);

            var response = new ProcessingResultResponse
            {
                Id = result.Id,
                DocumentId = result.DocumentId,
                ProcessingType = result.ProcessingType ?? string.Empty,
                ExtractedText = result.ExtractedText,
                Metadata = result.Metadata,
                ConfidenceScore = result.ConfidenceScore,
                ProcessingTimeMs = result.ProcessingTimeMs,
                ProcessingVersion = result.ProcessingVersion ?? "2.0",
                ErrorMessage = result.ErrorMessage,
                CreatedAt = result.CreatedAt
            };

            _logger.LogInformation("✅ Advanced processing completed for document {DocumentId} in {ProcessingTime}ms", 
                documentId, result.ProcessingTimeMs);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("⚠️ Document not found: {DocumentId}", documentId);
            return NotFound($"Document not found: {ex.Message}");
        }
        catch (NotSupportedException ex)
        {
            _logger.LogWarning("⚠️ Unsupported format for advanced processing: {DocumentId}", documentId);
            return UnprocessableEntity($"Format not supported for advanced processing: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Advanced processing failed for document {DocumentId}", documentId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Advanced processing failed");
        }
    }

    /// <summary>
    /// Get supported formats for Phase 2 Enhanced Processing
    /// </summary>
    /// <returns>List of supported content types and their capabilities</returns>
    [HttpGet("supported-formats")]
    [ProducesResponseType(typeof(SupportedFormatsResponse), StatusCodes.Status200OK)]
    [AllowAnonymous] // Allow anonymous access to check supported formats
    public IActionResult GetSupportedFormats()
    {
        try
        {
            var supportedFormats = new SupportedFormatsResponse
            {
                Formats = new List<FormatCapability>
                {
                    new FormatCapability
                    {
                        ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                        Extension = ".docx",
                        Description = "Microsoft Word Document",
                        Capabilities = new List<string>
                        {
                            "Text Extraction",
                            "Metadata Extraction",
                            "Table Processing", 
                            "Paragraph Structure Analysis",
                            "Document Properties"
                        }
                    },
                    new FormatCapability
                    {
                        ContentType = "application/pdf",
                        Extension = ".pdf",
                        Description = "Portable Document Format",
                        Capabilities = new List<string>
                        {
                            "Text Extraction",
                            "Metadata Extraction",
                            "Page-by-Page Processing",
                            "Document Information",
                            "Creation/Modification Dates"
                        }
                    },
                    new FormatCapability
                    {
                        ContentType = "text/plain",
                        Extension = ".txt",
                        Description = "Plain Text Document",
                        Capabilities = new List<string>
                        {
                            "Encoding Detection",
                            "Content Structure Analysis",
                            "Line Count Analysis",
                            "Character Statistics"
                        }
                    },
                    new FormatCapability
                    {
                        ContentType = "text/csv",
                        Extension = ".csv",
                        Description = "Comma-Separated Values",
                        Capabilities = new List<string>
                        {
                            "CSV Parsing",
                            "Header Detection",
                            "Column Analysis",
                            "Data Structure Recognition"
                        }
                    }
                }
            };

            return Ok(supportedFormats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to get supported formats");
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve supported formats");
        }
    }
}

/// <summary>
/// Processing result response model
/// </summary>
public class ProcessingResultResponse
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public string ProcessingType { get; set; } = string.Empty;
    public string? ExtractedText { get; set; }
    public string? Metadata { get; set; }
    public double? ConfidenceScore { get; set; }
    public int ProcessingTimeMs { get; set; }
    public string ProcessingVersion { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Supported formats response model
/// </summary>
public class SupportedFormatsResponse
{
    public List<FormatCapability> Formats { get; set; } = new();
}

/// <summary>
/// Format capability information
/// </summary>
public class FormatCapability
{
    public string ContentType { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Capabilities { get; set; } = new();
}