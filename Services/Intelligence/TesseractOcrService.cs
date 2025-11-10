using ADPA.Services.Intelligence;
using Tesseract;
using System.Drawing;

namespace ADPA.Services.Intelligence;

/// <summary>
/// OCR Service implementation using Tesseract engine
/// Extracts text from images with confidence scoring and quality metrics
/// </summary>
public class TesseractOcrService : IOcrService, IDisposable
{
    private readonly ILogger<TesseractOcrService> _logger;
    private readonly string _tessDataPath;
    private TesseractEngine? _engine;
    private bool _disposed = false;

    // Supported image formats for OCR processing
    private readonly string[] _supportedFormats = {
        "image/jpeg", "image/jpg", "image/png", "image/tiff", 
        "image/bmp", "image/gif", "image/webp"
    };

    public TesseractOcrService(ILogger<TesseractOcrService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _tessDataPath = configuration["OCR:TesseractDataPath"] ?? "./tessdata";
        
        // Initialize Tesseract engine on first use
        InitializeEngine();
    }

    /// <summary>
    /// Extract text from image using OCR
    /// </summary>
    public async Task<OcrResult> ExtractTextAsync(string imagePath, OcrOptions? options = null)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("🔍 Starting OCR text extraction for image: {ImagePath}", imagePath);

            // Validate image file exists
            if (!File.Exists(imagePath))
            {
                return new OcrResult
                {
                    Success = false,
                    ErrorMessage = $"Image file not found: {imagePath}"
                };
            }

            // Initialize options
            options ??= new OcrOptions();

            // Ensure Tesseract engine is initialized
            if (_engine == null)
            {
                InitializeEngine(options.Language);
            }

            // Process image
            var result = await ProcessImageAsync(imagePath, options);
            
            stopwatch.Stop();
            result.ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds;

            _logger.LogInformation("✅ OCR extraction completed in {ProcessingTime}ms with confidence {Confidence:F2}", 
                result.ProcessingTimeMs, result.ConfidenceScore);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "❌ OCR extraction failed for image: {ImagePath}", imagePath);
            
            return new OcrResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds
            };
        }
    }

    /// <summary>
    /// Check if image format is supported for OCR
    /// </summary>
    public bool IsImageFormatSupported(string contentType)
    {
        return _supportedFormats.Contains(contentType.ToLowerInvariant());
    }

    /// <summary>
    /// Check if OCR service is available and operational
    /// </summary>
    public async Task<bool> IsServiceAvailableAsync()
    {
        try
        {
            await Task.Run(() =>
            {
                if (_engine == null)
                {
                    InitializeEngine();
                }
            });
            
            return _engine != null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ OCR service availability check failed");
            return false;
        }
    }

    /// <summary>
    /// Initialize Tesseract engine with specified language
    /// </summary>
    private void InitializeEngine(string language = "eng")
    {
        try
        {
            _logger.LogInformation("🚀 Initializing Tesseract OCR engine with language: {Language}", language);

            // Ensure tessdata directory exists
            if (!Directory.Exists(_tessDataPath))
            {
                _logger.LogWarning("⚠️ Tessdata directory not found: {TessDataPath}. Creating directory...", _tessDataPath);
                Directory.CreateDirectory(_tessDataPath);
            }

            // Initialize engine
            _engine = new TesseractEngine(_tessDataPath, language, EngineMode.Default);
            
            _logger.LogInformation("✅ Tesseract OCR engine initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to initialize Tesseract OCR engine");
            throw new InvalidOperationException($"Failed to initialize OCR engine: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Process image and extract text using Tesseract
    /// </summary>
    private async Task<OcrResult> ProcessImageAsync(string imagePath, OcrOptions options)
    {
        return await Task.Run(() =>
        {
            try
            {
                // Load and preprocess image if needed
                using var preprocessedImage = options.PreprocessImage 
                    ? PreprocessImage(imagePath, options)
                    : Pix.LoadFromFile(imagePath);

                if (preprocessedImage == null)
                {
                    return new OcrResult
                    {
                        Success = false,
                        ErrorMessage = "Failed to load or preprocess image"
                    };
                }

                // Set page segmentation mode
                _engine!.DefaultPageSegMode = (PageSegMode)options.PageSegmentationMode;

                // Process image with Tesseract
                using var page = _engine.Process(preprocessedImage);
                
                // Extract text and metadata
                var extractedText = page.GetText();
                var confidence = page.GetMeanConfidence();
                
                // Get additional OCR details
                var wordCount = CountWords(extractedText);
                var lineCount = CountLines(extractedText);
                
                // Detect language if available
                string? detectedLanguage = null;
                try
                {
                    using var iter = page.GetIterator();
                    iter.Begin();
                    var ocrLanguage = iter.GetText(PageIteratorLevel.TextLine);
                    detectedLanguage = !string.IsNullOrEmpty(ocrLanguage) ? options.Language : null;
                }
                catch
                {
                    // Language detection failed, continue without it
                }

                // Calculate text quality score
                var textQuality = CalculateTextQuality(extractedText, confidence);

                // Generate warnings based on quality
                var warnings = GenerateQualityWarnings(confidence, textQuality, extractedText);

                return new OcrResult
                {
                    Success = true,
                    ExtractedText = extractedText,
                    ConfidenceScore = confidence,
                    DetectedLanguage = detectedLanguage,
                    WordCount = wordCount,
                    LineCount = lineCount,
                    TextQuality = textQuality,
                    Warnings = warnings.ToArray()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Image processing failed");
                return new OcrResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        });
    }

    /// <summary>
    /// Preprocess image to improve OCR accuracy
    /// </summary>
    private Pix? PreprocessImage(string imagePath, OcrOptions options)
    {
        try
        {
            var originalImage = Pix.LoadFromFile(imagePath);
            if (originalImage == null) return null;

            var processedImage = originalImage;

            // Apply scaling if specified
            if (Math.Abs(options.ImageScale - 1.0) > 0.001)
            {
                var scaledImage = originalImage.Scale((float)options.ImageScale, (float)options.ImageScale);
                if (processedImage != originalImage) processedImage.Dispose();
                processedImage = scaledImage;
            }

            // Convert to grayscale for better OCR results
            if (processedImage.Depth > 8)
            {
                var grayImage = processedImage.ConvertRGBToGray(0.299f, 0.587f, 0.114f);
                if (processedImage != originalImage) processedImage.Dispose();
                processedImage = grayImage;
            }

            // Apply simple thresholding to improve contrast
            var thresholdedImage = processedImage.BinarizeOtsuAdaptiveThreshold(2000, 2000, 0, 0, 0.0f);
            if (processedImage != originalImage) processedImage.Dispose();

            if (originalImage != thresholdedImage) originalImage.Dispose();

            return thresholdedImage;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Image preprocessing failed, using original image");
            return Pix.LoadFromFile(imagePath);
        }
    }

    /// <summary>
    /// Count words in extracted text
    /// </summary>
    private static int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        
        return text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    /// <summary>
    /// Count lines in extracted text
    /// </summary>
    private static int CountLines(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        
        return text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    /// <summary>
    /// Calculate overall text quality score
    /// </summary>
    private static double CalculateTextQuality(string text, float confidence)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0.0;

        // Base score from OCR confidence
        double qualityScore = confidence / 100.0;

        // Adjust based on text characteristics
        var wordCount = CountWords(text);
        var charCount = text.Length;
        
        // Penalize very short extractions
        if (wordCount < 5)
        {
            qualityScore *= 0.7;
        }

        // Penalize text with too many special characters (potential OCR errors)
        var specialCharRatio = text.Count(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c)) / (double)charCount;
        if (specialCharRatio > 0.3)
        {
            qualityScore *= 0.8;
        }

        // Boost score for reasonable word-to-character ratio
        var avgWordLength = charCount / (double)wordCount;
        if (avgWordLength >= 3 && avgWordLength <= 10)
        {
            qualityScore *= 1.1;
        }

        return Math.Min(1.0, qualityScore);
    }

    /// <summary>
    /// Generate quality warnings based on OCR results
    /// </summary>
    private static List<string> GenerateQualityWarnings(float confidence, double textQuality, string extractedText)
    {
        var warnings = new List<string>();

        if (confidence < 60)
        {
            warnings.Add("Low OCR confidence detected. Text extraction may be inaccurate.");
        }

        if (textQuality < 0.5)
        {
            warnings.Add("Poor text quality detected. Consider using a higher resolution image.");
        }

        if (string.IsNullOrWhiteSpace(extractedText))
        {
            warnings.Add("No text detected in image. Image may not contain readable text.");
        }

        var wordCount = CountWords(extractedText);
        if (wordCount < 3)
        {
            warnings.Add("Very little text detected. Image quality or content may be insufficient for OCR.");
        }

        return warnings;
    }

    /// <summary>
    /// Dispose of Tesseract engine resources
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _engine?.Dispose();
            _engine = null;
            _disposed = true;
        }
    }
}