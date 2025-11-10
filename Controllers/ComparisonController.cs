using ADPA.Services.Comparison;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ADPA.Controllers;

/// <summary>
/// Phase 4: Document Comparison API Controller
/// Provides intelligent document comparison and similarity analysis
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ComparisonController : ControllerBase
{
    private readonly IDocumentComparisonService _comparisonService;
    private readonly ILogger<ComparisonController> _logger;

    public ComparisonController(IDocumentComparisonService comparisonService, ILogger<ComparisonController> logger)
    {
        _comparisonService = comparisonService;
        _logger = logger;
    }

    /// <summary>
    /// Compare two documents by their IDs
    /// </summary>
    /// <param name="document1Id">First document ID</param>
    /// <param name="document2Id">Second document ID</param>
    /// <param name="comparisonType">Type of comparison to perform</param>
    /// <param name="ignoreWhitespace">Ignore whitespace differences</param>
    /// <param name="ignoreCase">Ignore case differences</param>
    /// <param name="ignorePunctuation">Ignore punctuation differences</param>
    /// <param name="similarityThreshold">Minimum similarity threshold</param>
    /// <returns>Document comparison results</returns>
    [HttpPost("compare-documents")]
    public async Task<ActionResult<DocumentComparisonResult>> CompareDocumentsAsync(
        [FromQuery] Guid document1Id,
        [FromQuery] Guid document2Id,
        [FromQuery] ComparisonType comparisonType = ComparisonType.Complete,
        [FromQuery] bool ignoreWhitespace = true,
        [FromQuery] bool ignoreCase = false,
        [FromQuery] bool ignorePunctuation = false,
        [FromQuery] double similarityThreshold = 0.1)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("🔍 Document comparison request from user {UserId}: {Document1Id} vs {Document2Id}", 
                userId, document1Id, document2Id);

            if (document1Id == document2Id)
            {
                return BadRequest(new { error = "Cannot compare a document with itself" });
            }

            var options = new ComparisonOptions
            {
                ComparisonType = comparisonType,
                IgnoreWhitespace = ignoreWhitespace,
                IgnoreCase = ignoreCase,
                IgnorePunctuation = ignorePunctuation,
                SimilarityThreshold = similarityThreshold
            };

            var result = await _comparisonService.CompareDocumentsAsync(document1Id, document2Id, options);

            _logger.LogInformation("✅ Document comparison completed: {ComparisonId}. Similarity: {Similarity}%", 
                result.ComparisonId, result.SimilarityScore * 100);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Document comparison failed: {Document1Id} vs {Document2Id}", document1Id, document2Id);
            return StatusCode(500, new { error = "Document comparison failed", details = ex.Message });
        }
    }

    /// <summary>
    /// Compare two text contents directly
    /// </summary>
    /// <param name="request">Content comparison request</param>
    /// <returns>Content comparison results</returns>
    [HttpPost("compare-content")]
    public async Task<ActionResult<DocumentComparisonResult>> CompareContentAsync([FromBody] ContentComparisonRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("📝 Content comparison request from user {UserId}: '{Name1}' vs '{Name2}'", 
                userId, request.Name1, request.Name2);

            if (string.IsNullOrEmpty(request.Content1) || string.IsNullOrEmpty(request.Content2))
            {
                return BadRequest(new { error = "Both content1 and content2 must be provided" });
            }

            var options = new ComparisonOptions
            {
                ComparisonType = request.ComparisonType,
                IgnoreWhitespace = request.IgnoreWhitespace,
                IgnoreCase = request.IgnoreCase,
                IgnorePunctuation = request.IgnorePunctuation,
                SimilarityThreshold = request.SimilarityThreshold,
                MaxDifferences = request.MaxDifferences
            };

            var result = await _comparisonService.CompareContentAsync(
                request.Content1, request.Content2, 
                request.Name1, request.Name2, 
                options);

            _logger.LogInformation("✅ Content comparison completed: {ComparisonId}. Similarity: {Similarity}%", 
                result.ComparisonId, result.SimilarityScore * 100);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Content comparison failed");
            return StatusCode(500, new { error = "Content comparison failed", details = ex.Message });
        }
    }

    /// <summary>
    /// Calculate similarity score between two texts
    /// </summary>
    /// <param name="text1">First text</param>
    /// <param name="text2">Second text</param>
    /// <returns>Similarity score between 0 and 1</returns>
    [HttpPost("similarity")]
    public async Task<ActionResult<double>> CalculateSimilarityAsync([FromBody] SimilarityRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("📊 Similarity calculation request from user {UserId}", userId);

            if (string.IsNullOrEmpty(request.Text1) || string.IsNullOrEmpty(request.Text2))
            {
                return BadRequest(new { error = "Both text1 and text2 must be provided" });
            }

            var similarity = await _comparisonService.CalculateSimilarityScoreAsync(request.Text1, request.Text2);

            _logger.LogInformation("✅ Similarity calculated: {Similarity}%", similarity * 100);

            return Ok(new 
            { 
                similarityScore = similarity,
                similarityPercentage = Math.Round(similarity * 100, 2),
                text1Length = request.Text1.Length,
                text2Length = request.Text2.Length
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Similarity calculation failed");
            return StatusCode(500, new { error = "Similarity calculation failed", details = ex.Message });
        }
    }

    /// <summary>
    /// Get comparison history for a document
    /// </summary>
    /// <param name="documentId">Document ID</param>
    /// <returns>List of comparison results involving this document</returns>
    [HttpGet("{documentId}/history")]
    public async Task<ActionResult<IEnumerable<DocumentComparisonResult>>> GetComparisonHistoryAsync(Guid documentId)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("📋 Comparison history request for document {DocumentId} by user {UserId}", documentId, userId);

            var history = await _comparisonService.GetComparisonHistoryAsync(documentId);

            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to get comparison history for document {DocumentId}", documentId);
            return StatusCode(500, new { error = "Failed to get comparison history", details = ex.Message });
        }
    }

    /// <summary>
    /// Get comparison service capabilities and supported features
    /// </summary>
    /// <returns>Comparison service capabilities</returns>
    [HttpGet("capabilities")]
    public ActionResult<object> GetComparisonCapabilities()
    {
        try
        {
            var capabilities = new
            {
                Version = "4.0",
                SupportedComparisonTypes = new[]
                {
                    "TextContent", "Structure", "Metadata", "Complete"
                },
                SupportedAlgorithms = new[]
                {
                    "Levenshtein Distance",
                    "Word-level Similarity", 
                    "Character-level Comparison",
                    "Statistical Analysis"
                },
                Features = new[]
                {
                    "Document-to-Document Comparison",
                    "Content-to-Content Comparison",
                    "Similarity Score Calculation",
                    "Difference Highlighting",
                    "Comparison History",
                    "Configurable Options",
                    "Statistical Analysis"
                },
                Options = new
                {
                    IgnoreWhitespace = "Ignore whitespace differences",
                    IgnoreCase = "Ignore case sensitivity",
                    IgnorePunctuation = "Ignore punctuation marks",
                    SimilarityThreshold = "Minimum similarity threshold (0.0 - 1.0)",
                    MaxDifferences = "Maximum number of differences to report"
                },
                Limits = new
                {
                    MaxContentLength = 1000000, // 1MB
                    MaxDifferences = 10000,
                    ProcessingTimeout = "30 seconds"
                }
            };

            return Ok(capabilities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to get comparison capabilities");
            return StatusCode(500, new { error = "Failed to get comparison capabilities", details = ex.Message });
        }
    }

    /// <summary>
    /// Get current user ID from claims
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }
        return userId;
    }
}

/// <summary>
/// Request model for content comparison
/// </summary>
public class ContentComparisonRequest
{
    public string Content1 { get; set; } = string.Empty;
    public string Content2 { get; set; } = string.Empty;
    public string Name1 { get; set; } = "Content 1";
    public string Name2 { get; set; } = "Content 2";
    public ComparisonType ComparisonType { get; set; } = ComparisonType.Complete;
    public bool IgnoreWhitespace { get; set; } = true;
    public bool IgnoreCase { get; set; } = false;
    public bool IgnorePunctuation { get; set; } = false;
    public double SimilarityThreshold { get; set; } = 0.1;
    public int MaxDifferences { get; set; } = 1000;
}

/// <summary>
/// Request model for similarity calculation
/// </summary>
public class SimilarityRequest
{
    public string Text1 { get; set; } = string.Empty;
    public string Text2 { get; set; } = string.Empty;
}