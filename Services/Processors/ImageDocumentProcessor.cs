using ADPA.Services;
using ADPA.Services.Intelligence;

namespace ADPA.Services.Processors;

/// <summary>
/// Image document processor that uses OCR to extract text from image files
/// Handles JPG, PNG, TIFF, BMP, GIF, and WebP formats
/// </summary>
public class ImageDocumentProcessor : IDocumentFormatProcessor
{
    private readonly ILogger<ImageDocumentProcessor> _logger;
    private readonly IOcrService _ocrService;

    public ImageDocumentProcessor(ILogger<ImageDocumentProcessor> logger, IOcrService ocrService)
    {
        _logger = logger;
        _ocrService = ocrService;
    }

    /// <summary>
    /// Extract text from image using OCR technology
    /// </summary>
    public async Task<string> ExtractTextAsync(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        
        try
        {
            _logger.LogInformation("🖼️ Processing image file: {FileName}", fileName);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Image file not found: {filePath}");
            }

            // Validate file format
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            if (!IsValidImageFormat(extension))
            {
                _logger.LogWarning("⚠️ Unsupported image format: {Extension}", extension);
                return string.Empty;
            }

            // Use OCR service to extract text
            var ocrResult = await _ocrService.ExtractTextAsync(filePath, new OcrOptions
            {
                Language = "eng",
                DetectOrientation = true,
                PreprocessImage = true
            });

            if (ocrResult.Success)
            {
                _logger.LogInformation("✅ OCR completed for {FileName}. Confidence: {Confidence}%, Words: {WordCount}", 
                    fileName, ocrResult.ConfidenceScore, ocrResult.WordCount);
                
                if (ocrResult.Warnings != null && ocrResult.Warnings.Length > 0)
                {
                    foreach (var warning in ocrResult.Warnings)
                    {
                        _logger.LogWarning("⚠️ OCR Warning: {Warning}", warning);
                    }
                }
                
                return ocrResult.ExtractedText ?? string.Empty;
            }
            else
            {
                _logger.LogError("❌ OCR failed for {FileName}: {Error}", fileName, ocrResult.ErrorMessage);
                return string.Empty;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Image processing failed for file: {FileName}", fileName);
            throw;
        }
    }

    /// <summary>
    /// Extract metadata from image file
    /// </summary>
    public async Task<DocumentMetadata> ExtractMetadataAsync(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        
        try
        {
            _logger.LogInformation("📊 Extracting metadata from image: {FileName}", fileName);

            var metadata = new DocumentMetadata();

            // Basic file information
            if (File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                metadata.CreatedDate = fileInfo.CreationTimeUtc;
                metadata.ModifiedDate = fileInfo.LastWriteTimeUtc;
                metadata.PageCount = 1; // Images are single page

                // Add basic image metadata
                metadata.CustomProperties["ProcessorType"] = "ImageDocumentProcessor";
                metadata.CustomProperties["ProcessorVersion"] = "3.0";
                metadata.CustomProperties["FileSize"] = fileInfo.Length.ToString();
                metadata.CustomProperties["FileExtension"] = Path.GetExtension(filePath).ToLowerInvariant();
            }

            // Attempt OCR for text statistics
            try
            {
                var ocrResult = await _ocrService.ExtractTextAsync(filePath, new OcrOptions
                {
                    Language = "eng",
                    DetectOrientation = true,
                    PreprocessImage = true
                });

                if (ocrResult.Success && !string.IsNullOrEmpty(ocrResult.ExtractedText))
                {
                    metadata.CustomProperties["WordCount"] = ocrResult.WordCount.ToString();
                    metadata.CustomProperties["CharacterCount"] = ocrResult.ExtractedText.Length.ToString();
                    metadata.CustomProperties["OcrConfidence"] = ocrResult.ConfidenceScore.ToString("F1");
                    metadata.CustomProperties["OcrTextQuality"] = ocrResult.TextQuality;
                    metadata.CustomProperties["OcrDetectedLanguage"] = ocrResult.DetectedLanguage ?? "Unknown";
                    metadata.CustomProperties["OcrLineCount"] = ocrResult.LineCount.ToString();
                    
                    if (ocrResult.Warnings != null && ocrResult.Warnings.Length > 0)
                    {
                        metadata.CustomProperties["OcrWarnings"] = string.Join("; ", ocrResult.Warnings);
                    }
                }
                else
                {
                    metadata.CustomProperties["OcrStatus"] = "No text detected";
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ OCR metadata extraction failed for {FileName}", fileName);
                metadata.CustomProperties["OcrError"] = ex.Message;
            }

            _logger.LogInformation("✅ Image metadata extraction completed for: {FileName}", fileName);
            return metadata;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Image metadata extraction failed for file: {FileName}", fileName);
            throw;
        }
    }

    /// <summary>
    /// Check if file extension is a valid image format
    /// </summary>
    private bool IsValidImageFormat(string extension)
    {
        var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".tiff", ".tif", ".bmp", ".gif", ".webp" };
        return validExtensions.Contains(extension);
    }
}