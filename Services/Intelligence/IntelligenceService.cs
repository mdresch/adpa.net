using ADPA.Services.Intelligence;

namespace ADPA.Services.Intelligence;

/// <summary>
/// Main Intelligence Service implementation that orchestrates OCR, Classification, and Text Analysis
/// Provides comprehensive AI-powered document processing capabilities for Phase 3
/// </summary>
public class IntelligenceService : IIntelligenceService
{
    private readonly ILogger<IntelligenceService> _logger;
    private readonly IOcrService _ocrService;
    private readonly IDocumentClassificationService _classificationService;
    private readonly ITextAnalysisService _textAnalysisService;
    private readonly IDocumentService _documentService;

    // Supported image formats for OCR processing
    private readonly string[] _supportedImageFormats = {
        "image/jpeg", "image/jpg", "image/png", "image/tiff", 
        "image/bmp", "image/gif", "image/webp"
    };

    public IntelligenceService(
        ILogger<IntelligenceService> logger,
        IOcrService ocrService,
        IDocumentClassificationService classificationService,
        ITextAnalysisService textAnalysisService,
        IDocumentService documentService)
    {
        _logger = logger;
        _ocrService = ocrService;
        _classificationService = classificationService;
        _textAnalysisService = textAnalysisService;
        _documentService = documentService;
        
        _logger.LogInformation("🧠 Intelligence Service initialized with full AI capabilities");
    }

    /// <summary>
    /// Process document with comprehensive intelligence capabilities
    /// </summary>
    public async Task<IntelligenceResult> ProcessDocumentIntelligenceAsync(Guid documentId, IntelligenceProcessingOptions? options = null)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("🧠 Starting intelligent processing for document {DocumentId}", documentId);

            options ??= new IntelligenceProcessingOptions();

            // Get document from repository
            var document = await _documentService.GetDocumentAsync(documentId);
            if (document == null)
            {
                return new IntelligenceResult
                {
                    DocumentId = documentId,
                    Success = false,
                    ErrorMessage = $"Document with ID {documentId} not found"
                };
            }

            var result = new IntelligenceResult
            {
                DocumentId = documentId,
                Success = true,
                Metadata = new IntelligenceMetadata
                {
                    ProcessingVersion = "3.0",
                    ProcessingSteps = new string[0],
                    CustomProperties = new Dictionary<string, object>
                    {
                        ["DocumentType"] = document.ContentType,
                        ["DocumentSize"] = document.FileSize,
                        ["OriginalFileName"] = document.FileName
                    }
                }
            };

            string documentText = string.Empty;

            // Step 1: Extract text (OCR for images, existing processing for documents)
            if (IsImageFormat(document.ContentType))
            {
                if (options.EnableOcr)
                {
                    _logger.LogInformation("🔍 Performing OCR on image document");
                    // OCR will be handled by document processor
                    result.OcrResult = new OcrResult { Success = false, ErrorMessage = "OCR processing moved to document processor" };
                    result.Metadata.ProcessingSteps = result.Metadata.ProcessingSteps.Append("OCR_EXTRACTION").ToArray();
                    
                    if (result.OcrResult.Success)
                    {
                        documentText = result.OcrResult.ExtractedText ?? string.Empty;
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ OCR extraction failed: {Error}", result.OcrResult.ErrorMessage);
                    }
                }
            }
            else
            {
                // For non-image documents, get text from existing processing
                _logger.LogInformation("📄 Retrieving text from processed document");
                // Get processing results from document
                var processingResults = document.ProcessingResults;
                var latestResult = processingResults.OrderByDescending(r => r.CreatedAt).FirstOrDefault();
                
                if (latestResult != null && !string.IsNullOrEmpty(latestResult.ExtractedText))
                {
                    documentText = latestResult.ExtractedText;
                    result.Metadata.ProcessingSteps = result.Metadata.ProcessingSteps.Append("TEXT_RETRIEVAL").ToArray();
                }
                else
                {
                    _logger.LogWarning("⚠️ No processed text found for document, attempting direct processing");
                    // Fallback: try to process the document directly
                    try
                    {
                        var processResult = await _documentService.ProcessDocumentWithAdvancedServiceAsync(documentId);
                        if (processResult != null && !string.IsNullOrEmpty(processResult.ExtractedText))
                        {
                            documentText = processResult.ExtractedText;
                            result.Metadata.ProcessingSteps = result.Metadata.ProcessingSteps.Append("DIRECT_PROCESSING").ToArray();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "⚠️ Direct document processing failed");
                    }
                }
            }

            // Step 2: Document Classification
            if (options.EnableClassification && !string.IsNullOrWhiteSpace(documentText))
            {
                _logger.LogInformation("🏷️ Performing document classification");
                try
                {
                    result.Classification = await _classificationService.ClassifyAsync(documentText, options.ClassificationOptions);
                    result.Metadata.ProcessingSteps = result.Metadata.ProcessingSteps.Append("CLASSIFICATION").ToArray();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Document classification failed");
                    result.Classification = new DocumentClassification
                    {
                        Success = false,
                        ErrorMessage = ex.Message
                    };
                }
            }

            // Step 3: Advanced Text Analysis
            if (options.EnableTextAnalysis && !string.IsNullOrWhiteSpace(documentText))
            {
                _logger.LogInformation("🔬 Performing advanced text analysis");
                try
                {
                    result.TextAnalysis = await _textAnalysisService.AnalyzeAsync(documentText, options.TextAnalysisOptions);
                    result.Metadata.ProcessingSteps = result.Metadata.ProcessingSteps.Append("TEXT_ANALYSIS").ToArray();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Text analysis failed");
                    result.TextAnalysis = new TextAnalysisResult
                    {
                        Success = false,
                        ErrorMessage = ex.Message
                    };
                }
            }

            // Step 4: Calculate overall confidence score
            result.ConfidenceScore = CalculateOverallConfidence(result);

            stopwatch.Stop();
            result.ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds;

            // Add processing metadata
            result.Metadata.CustomProperties["TotalProcessingTimeMs"] = result.ProcessingTimeMs;
            result.Metadata.CustomProperties["TextLength"] = documentText.Length;
            result.Metadata.CustomProperties["ProcessingStepsCount"] = result.Metadata.ProcessingSteps.Length;

            _logger.LogInformation("✅ Intelligence processing completed for document {DocumentId} in {ProcessingTime}ms with confidence {Confidence:F2}", 
                documentId, result.ProcessingTimeMs, result.ConfidenceScore);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "❌ Intelligence processing failed for document {DocumentId}", documentId);
            
            return new IntelligenceResult
            {
                DocumentId = documentId,
                Success = false,
                ErrorMessage = ex.Message,
                ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds
            };
        }
    }

    /// <summary>
    /// Extract text from image using OCR
    /// </summary>
    public async Task<OcrResult> ExtractTextFromImageAsync(string imagePath, OcrOptions? options = null)
    {
        try
        {
            _logger.LogInformation("🔍 OCR text extraction requested for: {ImagePath}", imagePath);
            return await _ocrService.ExtractTextAsync(imagePath, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ OCR text extraction failed");
            return new OcrResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Classify document based on content
    /// </summary>
    public async Task<DocumentClassification> ClassifyDocumentAsync(string documentText, ClassificationOptions? options = null)
    {
        try
        {
            _logger.LogInformation("🏷️ Document classification requested for text length: {TextLength}", documentText.Length);
            return await _classificationService.ClassifyAsync(documentText, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Document classification failed");
            return new DocumentClassification
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Perform advanced text analysis
    /// </summary>
    public async Task<TextAnalysisResult> AnalyzeTextAsync(string text, TextAnalysisOptions? options = null)
    {
        try
        {
            _logger.LogInformation("🔬 Text analysis requested for text length: {TextLength}", text.Length);
            return await _textAnalysisService.AnalyzeAsync(text, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Text analysis failed");
            return new TextAnalysisResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Get supported image formats for OCR
    /// </summary>
    public string[] GetSupportedImageFormats()
    {
        return _supportedImageFormats;
    }

    /// <summary>
    /// Get available document classification categories
    /// </summary>
    public async Task<string[]> GetDocumentCategoriesAsync()
    {
        try
        {
            return await _classificationService.GetAvailableCategoriesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Failed to get document categories");
            return Array.Empty<string>();
        }
    }

    /// <summary>
    /// Check if content type represents an image format
    /// </summary>
    private bool IsImageFormat(string contentType)
    {
        return _supportedImageFormats.Contains(contentType.ToLowerInvariant());
    }

    /// <summary>
    /// Calculate overall confidence score based on all processing results
    /// </summary>
    private static double CalculateOverallConfidence(IntelligenceResult result)
    {
        var confidenceScores = new List<double>();

        // Add OCR confidence if available
        if (result.OcrResult != null && result.OcrResult.Success)
        {
            confidenceScores.Add(result.OcrResult.ConfidenceScore);
        }

        // Add classification confidence if available
        if (result.Classification != null && result.Classification.Success)
        {
            confidenceScores.Add(result.Classification.PrimaryConfidence);
        }

        // Add text analysis confidence (derived from language detection if available)
        if (result.TextAnalysis != null && result.TextAnalysis.Success)
        {
            if (result.TextAnalysis.LanguageDetection != null)
            {
                confidenceScores.Add(result.TextAnalysis.LanguageDetection.Confidence);
            }
            else
            {
                confidenceScores.Add(0.8); // Default confidence for successful text analysis
            }
        }

        // Return average confidence or default if no scores available
        return confidenceScores.Count > 0 ? confidenceScores.Average() : 0.5;
    }
}