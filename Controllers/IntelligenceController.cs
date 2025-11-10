using ADPA.Services.Intelligence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ADPA.Controllers;

/// <summary>
/// Phase 3: Intelligence & Classification API Controller
/// Provides AI-powered document analysis including OCR, classification, and text analysis
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IntelligenceController : ControllerBase
{
    private readonly IIntelligenceService _intelligenceService;
    private readonly IOcrService _ocrService;
    private readonly IDocumentClassificationService _classificationService;
    private readonly ITextAnalysisService _textAnalysisService;
    private readonly ILogger<IntelligenceController> _logger;

    public IntelligenceController(
        IIntelligenceService intelligenceService,
        IOcrService ocrService,
        IDocumentClassificationService classificationService,
        ITextAnalysisService textAnalysisService,
        ILogger<IntelligenceController> logger)
    {
        _intelligenceService = intelligenceService;
        _ocrService = ocrService;
        _classificationService = classificationService;
        _textAnalysisService = textAnalysisService;
        _logger = logger;
    }

    /// <summary>
    /// Process comprehensive intelligence analysis for a document
    /// Combines OCR, classification, and text analysis
    /// </summary>
    /// <param name="documentId">Document ID to process</param>
    /// <param name="options">Processing options</param>
    /// <returns>Comprehensive intelligence analysis results</returns>
    [HttpPost("{documentId}/analyze")]
    public async Task<ActionResult<IntelligenceResult>> AnalyzeDocumentAsync(
        Guid documentId, 
        [FromBody] IntelligenceProcessingOptions? options = null)
    {
        try
        {
            _logger.LogInformation("🧠 Starting intelligence analysis for document: {DocumentId}", documentId);

            var result = await _intelligenceService.ProcessDocumentIntelligenceAsync(documentId, options);
            
            if (result.Success)
            {
                _logger.LogInformation("✅ Intelligence analysis completed for document: {DocumentId}. Confidence: {Confidence}%", 
                    documentId, result.ConfidenceScore);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("⚠️ Intelligence analysis failed for document: {DocumentId}. Error: {Error}", 
                    documentId, result.ErrorMessage);
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Intelligence analysis error for document: {DocumentId}", documentId);
            return StatusCode(500, new { error = "Internal server error during intelligence analysis", details = ex.Message });
        }
    }

    /// <summary>
    /// Extract text from image using OCR
    /// </summary>
    /// <param name="file">Image file to process</param>
    /// <param name="options">OCR processing options</param>
    /// <returns>OCR extraction results</returns>
    [HttpPost("ocr")]
    public async Task<ActionResult<OcrResult>> ExtractTextFromImageAsync(
        IFormFile file,
        [FromQuery] string? language = "eng",
        [FromQuery] bool detectOrientation = true,
        [FromQuery] bool preprocessImage = true)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "No file provided" });
            }

            _logger.LogInformation("📷 Starting OCR processing for file: {FileName}", file.FileName);

            // Save uploaded file temporarily
            var tempPath = Path.GetTempFileName();
            try
            {
                using (var stream = new FileStream(tempPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var ocrOptions = new OcrOptions
                {
                    Language = language ?? "eng",
                    DetectOrientation = detectOrientation,
                    PreprocessImage = preprocessImage
                };

                var result = await _ocrService.ExtractTextAsync(tempPath, ocrOptions);
                
                if (result.Success)
                {
                    _logger.LogInformation("✅ OCR completed for file: {FileName}. Confidence: {Confidence}%, Words: {WordCount}", 
                        file.FileName, result.ConfidenceScore, result.WordCount);
                }
                else
                {
                    _logger.LogWarning("⚠️ OCR failed for file: {FileName}. Error: {Error}", 
                        file.FileName, result.ErrorMessage);
                }

                return Ok(result);
            }
            finally
            {
                // Clean up temporary file
                if (System.IO.File.Exists(tempPath))
                {
                    System.IO.File.Delete(tempPath);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ OCR processing error for file: {FileName}", file?.FileName ?? "unknown");
            return StatusCode(500, new { error = "Internal server error during OCR processing", details = ex.Message });
        }
    }

    /// <summary>
    /// Classify document type using ML
    /// </summary>
    /// <param name="text">Text content to classify</param>
    /// <returns>Document classification results</returns>
    [HttpPost("classify")]
    public async Task<ActionResult<DocumentClassification>> ClassifyDocumentAsync([FromBody] string text)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return BadRequest(new { error = "No text content provided" });
            }

            _logger.LogInformation("📋 Starting document classification. Text length: {Length}", text.Length);

            var result = await _classificationService.ClassifyAsync(text);
            
            _logger.LogInformation("✅ Document classification completed. Predicted: {Category}, Confidence: {Confidence}%", 
                result.PrimaryCategory, result.PrimaryConfidence);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Document classification error");
            return StatusCode(500, new { error = "Internal server error during document classification", details = ex.Message });
        }
    }

    /// <summary>
    /// Perform advanced text analysis
    /// </summary>
    /// <param name="text">Text content to analyze</param>
    /// <param name="options">Analysis options</param>
    /// <returns>Text analysis results</returns>
    [HttpPost("analyze-text")]
    public async Task<ActionResult<TextAnalysisResult>> AnalyzeTextAsync(
        [FromBody] string text,
        [FromQuery] bool detectLanguage = true,
        [FromQuery] bool extractEntities = true,
        [FromQuery] bool analyzeSentiment = true)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return BadRequest(new { error = "No text content provided" });
            }

            _logger.LogInformation("🔬 Starting text analysis. Text length: {Length}", text.Length);

            var options = new TextAnalysisOptions
            {
                DetectLanguage = detectLanguage,
                ExtractEntities = extractEntities,
                AnalyzeSentiment = analyzeSentiment
            };

            var result = await _textAnalysisService.AnalyzeAsync(text, options);
            
            _logger.LogInformation("✅ Text analysis completed. Language: {Language}, Analysis: {Success}", 
                result.LanguageDetection?.DetectedLanguage ?? "Unknown", 
                result.Success);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Text analysis error");
            return StatusCode(500, new { error = "Internal server error during text analysis", details = ex.Message });
        }
    }

    /// <summary>
    /// Get intelligence service capabilities and status
    /// </summary>
    /// <returns>Service capabilities information</returns>
    [HttpGet("capabilities")]
    public ActionResult<object> GetCapabilities()
    {
        try
        {
            var capabilities = new
            {
                Version = "3.0",
                Phase = "Intelligence & Classification",
                Features = new[]
                {
                    "OCR Text Extraction (Tesseract)",
                    "ML-based Document Classification",
                    "Language Detection",
                    "Entity Extraction",
                    "Sentiment Analysis",
                    "Comprehensive Intelligence Analysis"
                },
                SupportedImageFormats = new[]
                {
                    "image/jpeg", "image/png", "image/tiff", "image/bmp", "image/gif", "image/webp"
                },
                DocumentCategories = new[]
                {
                    "Invoice", "Contract", "Report", "Resume", "Legal Document",
                    "Technical Documentation", "Marketing Material", "Email",
                    "Memo", "Other"
                },
                LanguageSupport = new[]
                {
                    "English (eng)", "Spanish (spa)", "French (fra)", "German (deu)",
                    "Italian (ita)", "Portuguese (por)", "Dutch (nld)", "Auto-detect"
                }
            };

            return Ok(capabilities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error retrieving intelligence capabilities");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }
}