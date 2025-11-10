using ADPA.Services;

namespace ADPA.Services.Intelligence;

/// <summary>
/// Core intelligence service interface for Phase 3 capabilities
/// Provides OCR, classification, and advanced text analysis
/// </summary>
public interface IIntelligenceService
{
    /// <summary>
    /// Process document with full intelligence capabilities (OCR + Classification + Analysis)
    /// </summary>
    Task<IntelligenceResult> ProcessDocumentIntelligenceAsync(Guid documentId, IntelligenceProcessingOptions? options = null);
    
    /// <summary>
    /// Extract text from image using OCR
    /// </summary>
    Task<OcrResult> ExtractTextFromImageAsync(string imagePath, OcrOptions? options = null);
    
    /// <summary>
    /// Classify document based on content
    /// </summary>
    Task<DocumentClassification> ClassifyDocumentAsync(string documentText, ClassificationOptions? options = null);
    
    /// <summary>
    /// Perform advanced text analysis (entities, sentiment, language detection)
    /// </summary>
    Task<TextAnalysisResult> AnalyzeTextAsync(string text, TextAnalysisOptions? options = null);
    
    /// <summary>
    /// Get supported image formats for OCR
    /// </summary>
    string[] GetSupportedImageFormats();
    
    /// <summary>
    /// Get available document classification categories
    /// </summary>
    Task<string[]> GetDocumentCategoriesAsync();
}

/// <summary>
/// OCR service interface for image text extraction
/// </summary>
public interface IOcrService
{
    Task<OcrResult> ExtractTextAsync(string imagePath, OcrOptions? options = null);
    bool IsImageFormatSupported(string contentType);
    Task<bool> IsServiceAvailableAsync();
}

/// <summary>
/// Document classification service interface
/// </summary>
public interface IDocumentClassificationService
{
    Task<DocumentClassification> ClassifyAsync(string documentText, ClassificationOptions? options = null);
    Task<string[]> GetAvailableCategoriesAsync();
    Task TrainModelAsync(IEnumerable<DocumentTrainingData> trainingData);
}

/// <summary>
/// Text analysis service interface
/// </summary>
public interface ITextAnalysisService
{
    Task<TextAnalysisResult> AnalyzeAsync(string text, TextAnalysisOptions? options = null);
    Task<LanguageDetectionResult> DetectLanguageAsync(string text);
    Task<EntityExtractionResult> ExtractEntitiesAsync(string text);
    Task<SentimentAnalysisResult> AnalyzeSentimentAsync(string text);
}

/// <summary>
/// Comprehensive intelligence processing result
/// </summary>
public class IntelligenceResult
{
    public Guid ProcessingId { get; set; } = Guid.NewGuid();
    public Guid DocumentId { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    
    // OCR Results (for image documents)
    public OcrResult? OcrResult { get; set; }
    
    // Classification Results
    public DocumentClassification? Classification { get; set; }
    
    // Text Analysis Results
    public TextAnalysisResult? TextAnalysis { get; set; }
    
    // Processing Metrics
    public int ProcessingTimeMs { get; set; }
    public double ConfidenceScore { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    
    // Enhanced Metadata
    public IntelligenceMetadata? Metadata { get; set; }
}

/// <summary>
/// OCR processing result with extracted text and confidence metrics
/// </summary>
public class OcrResult
{
    public bool Success { get; set; }
    public string? ExtractedText { get; set; }
    public double ConfidenceScore { get; set; }
    public int ProcessingTimeMs { get; set; }
    public string? ErrorMessage { get; set; }
    
    // OCR-specific details
    public string? DetectedLanguage { get; set; }
    public int WordCount { get; set; }
    public int LineCount { get; set; }
    public BoundingBox[]? WordBoundingBoxes { get; set; }
    
    // Quality metrics
    public double TextQuality { get; set; }
    public string[]? Warnings { get; set; }
}

/// <summary>
/// Document classification result with category and confidence
/// </summary>
public class DocumentClassification
{
    public bool Success { get; set; }
    public string? PrimaryCategory { get; set; }
    public double PrimaryConfidence { get; set; }
    public CategoryPrediction[]? AlternativeCategories { get; set; }
    public string? ErrorMessage { get; set; }
    
    // Classification details
    public string? ClassificationModel { get; set; }
    public DateTime ClassifiedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Features { get; set; } = new();
}

/// <summary>
/// Advanced text analysis result with multiple analysis types
/// </summary>
public class TextAnalysisResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    
    // Language Detection
    public LanguageDetectionResult? LanguageDetection { get; set; }
    
    // Entity Extraction
    public EntityExtractionResult? EntityExtraction { get; set; }
    
    // Sentiment Analysis
    public SentimentAnalysisResult? SentimentAnalysis { get; set; }
    
    // Content Statistics
    public TextStatistics? Statistics { get; set; }
    
    // Processing metadata
    public int ProcessingTimeMs { get; set; }
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Language detection result
/// </summary>
public class LanguageDetectionResult
{
    public string? DetectedLanguage { get; set; }
    public string? LanguageCode { get; set; }
    public double Confidence { get; set; }
    public LanguagePrediction[]? AlternativeLanguages { get; set; }
}

/// <summary>
/// Entity extraction result with named entities
/// </summary>
public class EntityExtractionResult
{
    public Entity[]? Entities { get; set; }
    public int EntityCount { get; set; }
    public Dictionary<string, int> EntityTypeCount { get; set; } = new();
}

/// <summary>
/// Sentiment analysis result
/// </summary>
public class SentimentAnalysisResult
{
    public string? OverallSentiment { get; set; } // Positive, Negative, Neutral
    public double ConfidenceScore { get; set; }
    public SentimentScore? Scores { get; set; }
}

/// <summary>
/// Processing options for intelligence operations
/// </summary>
public class IntelligenceProcessingOptions
{
    public bool EnableOcr { get; set; } = true;
    public bool EnableClassification { get; set; } = true;
    public bool EnableTextAnalysis { get; set; } = true;
    public OcrOptions? OcrOptions { get; set; }
    public ClassificationOptions? ClassificationOptions { get; set; }
    public TextAnalysisOptions? TextAnalysisOptions { get; set; }
}

/// <summary>
/// OCR processing options
/// </summary>
public class OcrOptions
{
    public string Language { get; set; } = "eng"; // Tesseract language code
    public int PageSegmentationMode { get; set; } = 3; // PSM_AUTO_OSD
    public int OcrEngineMode { get; set; } = 3; // OEM_DEFAULT
    public bool DetectOrientation { get; set; } = true;
    public double ImageScale { get; set; } = 1.0;
    public bool PreprocessImage { get; set; } = true;
}

/// <summary>
/// Document classification options
/// </summary>
public class ClassificationOptions
{
    public string[]? RestrictToCategories { get; set; }
    public double MinimumConfidence { get; set; } = 0.5;
    public int MaxAlternatives { get; set; } = 3;
    public bool UseCustomModel { get; set; } = false;
    public string? CustomModelPath { get; set; }
}

/// <summary>
/// Text analysis processing options
/// </summary>
public class TextAnalysisOptions
{
    public bool DetectLanguage { get; set; } = true;
    public bool ExtractEntities { get; set; } = true;
    public bool AnalyzeSentiment { get; set; } = true;
    public bool CalculateStatistics { get; set; } = true;
    public string[]? EntityTypes { get; set; }
}

/// <summary>
/// Supporting data models
/// </summary>
public class BoundingBox
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string? Text { get; set; }
    public double Confidence { get; set; }
}

public class CategoryPrediction
{
    public string Category { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public Dictionary<string, object> Features { get; set; } = new();
}

public class LanguagePrediction
{
    public string Language { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = string.Empty;
    public double Confidence { get; set; }
}

public class Entity
{
    public string Text { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int StartPosition { get; set; }
    public int EndPosition { get; set; }
    public double Confidence { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}

public class SentimentScore
{
    public double Positive { get; set; }
    public double Negative { get; set; }
    public double Neutral { get; set; }
}

public class TextStatistics
{
    public int CharacterCount { get; set; }
    public int WordCount { get; set; }
    public int SentenceCount { get; set; }
    public int ParagraphCount { get; set; }
    public double ReadabilityScore { get; set; }
    public string[]? TopKeywords { get; set; }
}

public class IntelligenceMetadata
{
    public string ProcessingVersion { get; set; } = "3.0";
    public string[]? ProcessingSteps { get; set; }
    public Dictionary<string, object> CustomProperties { get; set; } = new();
    public string? ModelVersions { get; set; }
}

public class DocumentTrainingData
{
    public string Text { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double Weight { get; set; } = 1.0;
}