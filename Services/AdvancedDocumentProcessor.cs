using ADPA.Models.Entities;
using ADPA.Services.Processors;
using ADPA.Services.Intelligence;
using System.Text;

namespace ADPA.Services;

/// <summary>
/// Enhanced document processor with format-specific implementations
/// Supports Word, PDF, and text extraction with metadata analysis
/// </summary>
public interface IAdvancedDocumentProcessor
{
    Task<DocumentProcessingResult> ProcessDocumentAsync(string filePath, string fileName, string contentType);
    Task<string> ExtractTextAsync(string filePath, string contentType);
    Task<DocumentMetadata> ExtractMetadataAsync(string filePath, string contentType);
    bool IsFormatSupported(string contentType);
}

/// <summary>
/// Advanced document processor implementation for Phase 2
/// </summary>
public class AdvancedDocumentProcessor : IAdvancedDocumentProcessor
{
    private readonly ILogger<AdvancedDocumentProcessor> _logger;
    private readonly Dictionary<string, IDocumentFormatProcessor> _processors;
    private readonly IServiceProvider _serviceProvider;

    public AdvancedDocumentProcessor(
        ILogger<AdvancedDocumentProcessor> logger, 
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        
        // Initialize processors with dependency injection
        _processors = new Dictionary<string, IDocumentFormatProcessor>();
        InitializeProcessors();
    }

    /// <summary>
    /// Initialize document processors with dependency injection support
    /// </summary>
    private void InitializeProcessors()
    {
        try
        {
            // Traditional document processors
            var wordProcessor = new WordDocumentProcessor(_serviceProvider.GetRequiredService<ILogger<WordDocumentProcessor>>());
            var pdfProcessor = new PdfDocumentProcessor(_serviceProvider.GetRequiredService<ILogger<PdfDocumentProcessor>>());
            var textProcessor = new TextDocumentProcessor(_serviceProvider.GetRequiredService<ILogger<TextDocumentProcessor>>());
            
            _processors.Add("application/vnd.openxmlformats-officedocument.wordprocessingml.document", wordProcessor);
            _processors.Add("application/pdf", pdfProcessor);
            _processors.Add("text/plain", textProcessor);
            _processors.Add("text/csv", textProcessor); // CSV uses text processor

            // Phase 3: Add image processors with OCR support
            try
            {
                var ocrService = _serviceProvider.GetService<IOcrService>();
                if (ocrService != null)
                {
                    var imageProcessor = new ImageDocumentProcessor(
                        _serviceProvider.GetRequiredService<ILogger<ImageDocumentProcessor>>(), 
                        ocrService);

                    // Add support for image formats
                    _processors.Add("image/jpeg", imageProcessor);
                    _processors.Add("image/jpg", imageProcessor);
                    _processors.Add("image/png", imageProcessor);
                    _processors.Add("image/tiff", imageProcessor);
                    _processors.Add("image/bmp", imageProcessor);
                    _processors.Add("image/gif", imageProcessor);
                    _processors.Add("image/webp", imageProcessor);

                    _logger.LogInformation("✅ Phase 3 Image processing with OCR enabled");
                }
                else
                {
                    _logger.LogWarning("⚠️ OCR service not available, image processing disabled");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Failed to initialize OCR image processing");
            }

            _logger.LogInformation("🚀 Advanced Document Processor initialized with {ProcessorCount} format processors", _processors.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to initialize document processors");
            throw;
        }
    }

    /// <summary>
    /// Check if the content type is supported for processing
    /// </summary>
    public bool IsFormatSupported(string contentType)
    {
        return _processors.ContainsKey(contentType);
    }

    /// <summary>
    /// Process document with format-specific implementation
    /// </summary>
    public async Task<DocumentProcessingResult> ProcessDocumentAsync(string filePath, string fileName, string contentType)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("🔄 Starting advanced processing for {FileName} ({ContentType})", fileName, contentType);

            if (!IsFormatSupported(contentType))
            {
                throw new NotSupportedException($"Content type '{contentType}' is not supported for advanced processing");
            }

            var processor = _processors[contentType];
            
            // Extract text and metadata in parallel
            var textTask = processor.ExtractTextAsync(filePath);
            var metadataTask = processor.ExtractMetadataAsync(filePath);
            
            await Task.WhenAll(textTask, metadataTask);
            
            var extractedText = await textTask;
            var metadata = await metadataTask;

            stopwatch.Stop();

            var result = new DocumentProcessingResult
            {
                Success = true,
                ExtractedText = extractedText,
                Metadata = metadata,
                ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds,
                ProcessorType = processor.GetType().Name,
                WordCount = CountWords(extractedText),
                CharacterCount = extractedText?.Length ?? 0,
                ConfidenceScore = CalculateConfidenceScore(extractedText, metadata)
            };

            _logger.LogInformation("✅ Advanced processing completed for {FileName} in {ProcessingTime}ms", 
                fileName, result.ProcessingTimeMs);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "❌ Advanced processing failed for {FileName}", fileName);
            
            return new DocumentProcessingResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds
            };
        }
    }

    /// <summary>
    /// Extract text from document using format-specific processor
    /// </summary>
    public async Task<string> ExtractTextAsync(string filePath, string contentType)
    {
        if (!IsFormatSupported(contentType))
        {
            throw new NotSupportedException($"Content type '{contentType}' is not supported");
        }

        var processor = _processors[contentType];
        return await processor.ExtractTextAsync(filePath);
    }

    /// <summary>
    /// Extract metadata from document using format-specific processor
    /// </summary>
    public async Task<DocumentMetadata> ExtractMetadataAsync(string filePath, string contentType)
    {
        if (!IsFormatSupported(contentType))
        {
            throw new NotSupportedException($"Content type '{contentType}' is not supported");
        }

        var processor = _processors[contentType];
        return await processor.ExtractMetadataAsync(filePath);
    }

    /// <summary>
    /// Count words in extracted text
    /// </summary>
    private int CountWords(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        return text.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    /// <summary>
    /// Calculate confidence score based on text quality and metadata
    /// </summary>
    private double CalculateConfidenceScore(string? extractedText, DocumentMetadata metadata)
    {
        double score = 0.5; // Base score

        // Text quality factors
        if (!string.IsNullOrWhiteSpace(extractedText))
        {
            score += 0.3; // Has text content

            // Bonus for reasonable text length
            if (extractedText.Length > 10)
                score += 0.1;

            // Bonus for structured content (paragraphs, sentences)
            if (extractedText.Contains('\n') || extractedText.Contains('.'))
                score += 0.1;
        }

        // Metadata quality factors
        if (metadata.PageCount > 0)
            score += 0.05;

        if (!string.IsNullOrWhiteSpace(metadata.Title))
            score += 0.05;

        if (!string.IsNullOrWhiteSpace(metadata.Author))
            score += 0.05;

        return Math.Min(1.0, score); // Cap at 1.0
    }
}

/// <summary>
/// Document processing result with enhanced information
/// </summary>
public class DocumentProcessingResult
{
    public bool Success { get; set; }
    public string? ExtractedText { get; set; }
    public DocumentMetadata? Metadata { get; set; }
    public string? ErrorMessage { get; set; }
    public int ProcessingTimeMs { get; set; }
    public string? ProcessorType { get; set; }
    public int WordCount { get; set; }
    public int CharacterCount { get; set; }
    public double ConfidenceScore { get; set; }
}

/// <summary>
/// Document metadata extracted during processing
/// </summary>
public class DocumentMetadata
{
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? Subject { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public string? Creator { get; set; }
    public string? Producer { get; set; }
    public int PageCount { get; set; }
    public Dictionary<string, object> CustomProperties { get; set; } = new();
}

/// <summary>
/// Base interface for format-specific document processors
/// </summary>
public interface IDocumentFormatProcessor
{
    Task<string> ExtractTextAsync(string filePath);
    Task<DocumentMetadata> ExtractMetadataAsync(string filePath);
}