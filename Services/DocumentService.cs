using ADPA.Models.DTOs;
using ADPA.Models.Entities;
using ADPA.Data.Repositories;
using ADPA.Services.Notifications;

namespace ADPA.Services;

/// <summary>
/// Interface for document service
/// </summary>
public interface IDocumentService
{
    Task<DocumentDto> UploadDocumentAsync(Guid userId, DocumentUploadDto uploadDto);
    Task<DocumentDto?> GetDocumentAsync(Guid id);
    Task<IEnumerable<DocumentDto>> GetUserDocumentsAsync(Guid userId);
    Task<IEnumerable<DocumentDto>> GetAllDocumentsAsync();
    Task<bool> DeleteDocumentAsync(Guid id);
    Task ProcessPendingDocumentsAsync();
    Task<ProcessingResult> ProcessDocumentWithAdvancedServiceAsync(Guid documentId);
}

/// <summary>
/// Service for handling document operations
/// </summary>
public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileProcessingService _fileProcessingService;
    private readonly IAdvancedDocumentProcessor _advancedProcessor;
    private readonly EfProcessingResultRepository _processingResultRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(
        IDocumentRepository documentRepository, 
        IFileProcessingService fileProcessingService,
        IAdvancedDocumentProcessor advancedProcessor,
        EfProcessingResultRepository processingResultRepository,
        INotificationService notificationService,
        ILogger<DocumentService> logger)
    {
        _documentRepository = documentRepository;
        _fileProcessingService = fileProcessingService;
        _advancedProcessor = advancedProcessor;
        _processingResultRepository = processingResultRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Upload and process a document
    /// </summary>
    public async Task<DocumentDto> UploadDocumentAsync(Guid userId, DocumentUploadDto uploadDto)
    {
        try
        {
            // Validate file
            if (uploadDto.File == null || uploadDto.File.Length == 0)
            {
                throw new ArgumentException("File is required and cannot be empty");
            }

            using var fileStream = uploadDto.File.OpenReadStream();

            // Enhanced file validation using the new file processing service
            var isValid = await _fileProcessingService.ValidateFileAsync(fileStream, uploadDto.File.FileName, uploadDto.File.Length);
            if (!isValid)
            {
                throw new ArgumentException("File validation failed. Please check file format and size.");
            }

            // Generate file hash for duplicate detection
            var fileHash = await _fileProcessingService.GenerateFileHashAsync(fileStream);
            
            // Check for duplicate
            var existingDoc = await _documentRepository.GetByHashAsync(fileHash);
            if (existingDoc != null)
            {
                _logger.LogWarning("Duplicate file detected: {FileName}", uploadDto.File.FileName);
                return MapToDocumentDto(existingDoc);
            }

            // Detect file format
            var detectedFormat = _fileProcessingService.DetectFileFormat(uploadDto.File.FileName, uploadDto.File.ContentType);
            
            // Create document entity
            var document = new Document
            {
                UserId = userId,
                FileName = uploadDto.File.FileName,
                FileSize = uploadDto.File.Length,
                ContentType = uploadDto.File.ContentType,
                FileHash = fileHash,
                Status = ProcessingStatus.Pending,
                BlobPath = $"documents/{detectedFormat}/{Guid.NewGuid()}/{uploadDto.File.FileName}",
                DetectedLanguage = null // Will be determined during processing
            };

            // Save document
            var createdDocument = await _documentRepository.CreateAsync(document);

            _logger.LogInformation("📄 Document uploaded: {DocumentId} - {FileName} ({Format})", 
                createdDocument.Id, createdDocument.FileName, detectedFormat);

            // Start processing asynchronously with enhanced processing
            _ = Task.Run(() => ProcessDocumentWithEnhancedServiceAsync(createdDocument.Id));

            return MapToDocumentDto(createdDocument);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document: {FileName}", uploadDto.File?.FileName);
            throw;
        }
    }

    /// <summary>
    /// Get document by ID
    /// </summary>
    public async Task<DocumentDto?> GetDocumentAsync(Guid id)
    {
        var document = await _documentRepository.GetByIdAsync(id);
        return document != null ? MapToDocumentDto(document) : null;
    }

    /// <summary>
    /// Get documents for a specific user
    /// </summary>
    public async Task<IEnumerable<DocumentDto>> GetUserDocumentsAsync(Guid userId)
    {
        var documents = await _documentRepository.GetByUserIdAsync(userId);
        return documents.Select(MapToDocumentDto);
    }

    /// <summary>
    /// Get all documents (admin function)
    /// </summary>
    public async Task<IEnumerable<DocumentDto>> GetAllDocumentsAsync()
    {
        var documents = await _documentRepository.GetAllAsync();
        return documents.Select(MapToDocumentDto);
    }

    /// <summary>
    /// Delete document
    /// </summary>
    public async Task<bool> DeleteDocumentAsync(Guid id)
    {
        try
        {
            var deleted = await _documentRepository.DeleteAsync(id);
            if (deleted)
            {
                _logger.LogInformation("Document deleted: {DocumentId}", id);
            }
            return deleted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document: {DocumentId}", id);
            throw;
        }
    }

    /// <summary>
    /// Process all pending documents
    /// </summary>
    public async Task ProcessPendingDocumentsAsync()
    {
        var pendingDocuments = await _documentRepository.GetByStatusAsync(ProcessingStatus.Pending);
        
        foreach (var document in pendingDocuments)
        {
            _ = Task.Run(() => ProcessDocumentAsync(document.Id));
        }

        _logger.LogInformation("Started processing {Count} pending documents", pendingDocuments.Count());
    }

    /// <summary>
    /// Process individual document
    /// </summary>
    private async Task ProcessDocumentAsync(Guid documentId)
    {
        try
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null)
            {
                _logger.LogWarning("Document not found for processing: {DocumentId}", documentId);
                return;
            }

            // Update status to processing
            document.Status = ProcessingStatus.Processing;
            await _documentRepository.UpdateAsync(document);

            // Send real-time notification - processing started
            await _notificationService.NotifyProcessingStartedAsync(document.Id, document.UserId, document.FileName);

            _logger.LogInformation("Processing document: {DocumentId} - {FileName}", documentId, document.FileName);

            // Simulate document processing based on content type
            await SimulateDocumentProcessing(document);

            // Update status to completed
            document.Status = ProcessingStatus.Completed;
            document.ProcessedAt = DateTime.UtcNow;
            
            // Detect language and page count (simulated)
            document.DetectedLanguage = "en";
            document.PageCount = document.ContentType.Contains("pdf") ? Random.Shared.Next(1, 20) : 1;

            await _documentRepository.UpdateAsync(document);

            // Send real-time notification - processing completed
            var processingTime = DateTime.UtcNow - document.CreatedAt;
            await _notificationService.NotifyProcessingCompletedAsync(document.Id, document.UserId, document.FileName, processingTime);

            _logger.LogInformation("Document processing completed: {DocumentId}", documentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing document: {DocumentId}", documentId);
            
            // Update status to failed
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document != null)
            {
                document.Status = ProcessingStatus.Failed;
                document.ProcessedAt = DateTime.UtcNow;
                await _documentRepository.UpdateAsync(document);

                // Send real-time notification - processing failed
                await _notificationService.NotifyProcessingErrorAsync(document.Id, document.UserId, 
                    "Document processing failed", ex.Message);
            }
        }
    }

    /// <summary>
    /// Simulate document processing based on file type
    /// </summary>
    private async Task SimulateDocumentProcessing(Document document)
    {
        // Simulate processing time based on file type and size
        var processingTime = document.ContentType switch
        {
            var ct when ct.Contains("pdf") => Math.Max(2000, document.FileSize / 10000), // PDF processing
            var ct when ct.Contains("image") => Math.Max(1500, document.FileSize / 15000), // Image OCR
            var ct when ct.Contains("text") => Math.Max(500, document.FileSize / 50000), // Text processing
            _ => 1000 // Default processing time
        };

        await Task.Delay((int)Math.Min(processingTime, 5000)); // Cap at 5 seconds for demo
    }

    /// <summary>
    /// Calculate file hash for duplicate detection
    /// </summary>
    private async Task<string> CalculateFileHashAsync(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hashBytes = await md5.ComputeHashAsync(stream);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    /// <summary>
    /// Enhanced document processing using the new FileProcessingService
    /// </summary>
    private async Task ProcessDocumentWithEnhancedServiceAsync(Guid documentId)
    {
        try
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null)
            {
                _logger.LogWarning("Document not found for enhanced processing: {DocumentId}", documentId);
                return;
            }

            // Update status to processing
            document.Status = ProcessingStatus.Processing;
            await _documentRepository.UpdateAsync(document);

            _logger.LogInformation("🔄 Enhanced processing started: {DocumentId} - {FileName}", documentId, document.FileName);

            // Simulate file stream (in real implementation, retrieve from blob storage)
            using var fileStream = new MemoryStream();
            
            // Process the document using the enhanced file processing service
            var processingResult = await _fileProcessingService.ProcessFileAsync(fileStream, document.FileName, document.ContentType);

            if (processingResult.Success)
            {
                // Update document with processing results
                document.Status = ProcessingStatus.Completed;
                document.ProcessedAt = DateTime.UtcNow;
                document.DetectedLanguage = DetectLanguage(processingResult.ExtractedText);
                document.PageCount = EstimatePageCount(processingResult.ExtractedText, processingResult.DetectedFormat.ToString());

                // Create processing result entry
                var processResult = new ProcessingResult
                {
                    DocumentId = document.Id,
                    ProcessingType = processingResult.DetectedFormat.ToString(),
                    ExtractedText = processingResult.ExtractedText,
                    ProcessingTimeMs = (int)processingResult.ProcessingTimeMs,
                    ConfidenceScore = processingResult.ConfidenceScore,
                    Metadata = System.Text.Json.JsonSerializer.Serialize(processingResult.Metadata),
                    ProcessingVersion = "2.0"
                };

                // In a real implementation, you would save this to a ProcessingResult repository
                // For now, just log the result
                _logger.LogInformation("✅ Enhanced processing completed: {DocumentId} - Confidence: {Confidence}%", 
                    documentId, Math.Round(processingResult.ConfidenceScore * 100, 1));
            }
            else
            {
                // Handle processing failure
                document.Status = ProcessingStatus.Failed;
                _logger.LogError("❌ Enhanced processing failed: {DocumentId} - {Error}", documentId, processingResult.ErrorMessage);
            }

            await _documentRepository.UpdateAsync(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Enhanced processing error: {DocumentId}", documentId);
            
            // Update document status to failed
            try
            {
                var document = await _documentRepository.GetByIdAsync(documentId);
                if (document != null)
                {
                    document.Status = ProcessingStatus.Failed;
                    await _documentRepository.UpdateAsync(document);
                }
            }
            catch (Exception updateEx)
            {
                _logger.LogError(updateEx, "Failed to update document status after processing error: {DocumentId}", documentId);
            }
        }
    }

    /// <summary>
    /// Detect document language (simplified implementation)
    /// </summary>
    private static string DetectLanguage(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return "unknown";

        // Simple language detection based on common words
        var englishWords = new[] { "the", "and", "is", "in", "to", "of", "a", "that", "it", "with" };
        var words = text.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var englishWordCount = words.Count(word => englishWords.Contains(word));
        
        return englishWordCount > words.Length * 0.1 ? "en" : "unknown";
    }

    /// <summary>
    /// Process document using advanced processor (Phase 2 Enhanced Processing)
    /// </summary>
    public async Task<ProcessingResult> ProcessDocumentWithAdvancedServiceAsync(Guid documentId)
    {
        try
        {
            _logger.LogInformation("🚀 Starting advanced processing for document {DocumentId}", documentId);

            // Get document from repository
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null)
            {
                throw new ArgumentException($"Document with ID {documentId} not found");
            }

            // Check if format is supported for advanced processing
            if (!_advancedProcessor.IsFormatSupported(document.ContentType))
            {
                throw new NotSupportedException($"Advanced processing not supported for content type: {document.ContentType}");
            }

            // Update document status to processing
            document.Status = ProcessingStatus.Processing;
            await _documentRepository.UpdateAsync(document);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                // Process document with advanced processor
                var processingResult = await _advancedProcessor.ProcessDocumentAsync(
                    document.BlobPath ?? string.Empty, 
                    document.FileName, 
                    document.ContentType);

                stopwatch.Stop();

                if (processingResult.Success)
                {
                    // Create processing result entity
                    var result = new ProcessingResult
                    {
                        DocumentId = documentId,
                        ProcessingType = "AdvancedExtraction",
                        ExtractedText = processingResult.ExtractedText,
                        Metadata = System.Text.Json.JsonSerializer.Serialize(processingResult.Metadata),
                        ConfidenceScore = processingResult.ConfidenceScore,
                        ProcessingTimeMs = processingResult.ProcessingTimeMs,
                        ProcessingVersion = "2.0",
                        CreatedAt = DateTime.UtcNow
                    };

                    // Save processing result
                    await _processingResultRepository.CreateAsync(result);

                    // Update document with processing results
                    document.Status = ProcessingStatus.Completed;
                    document.ProcessedAt = DateTime.UtcNow;
                    if (processingResult.Metadata != null)
                    {
                        document.PageCount = processingResult.Metadata.PageCount > 0 ? 
                            processingResult.Metadata.PageCount : document.PageCount;
                        
                        // Extract language from metadata if available
                        if (processingResult.Metadata.CustomProperties.TryGetValue("DetectedLanguage", out var language))
                        {
                            document.DetectedLanguage = language.ToString();
                        }
                    }

                    await _documentRepository.UpdateAsync(document);

                    _logger.LogInformation("✅ Advanced processing completed successfully for document {DocumentId} in {ProcessingTime}ms", 
                        documentId, processingResult.ProcessingTimeMs);

                    return result;
                }
                else
                {
                    // Processing failed
                    var result = new ProcessingResult
                    {
                        DocumentId = documentId,
                        ProcessingType = "AdvancedExtraction",
                        ErrorMessage = processingResult.ErrorMessage,
                        ProcessingTimeMs = processingResult.ProcessingTimeMs,
                        ProcessingVersion = "2.0",
                        CreatedAt = DateTime.UtcNow
                    };

                    await _processingResultRepository.CreateAsync(result);

                    document.Status = ProcessingStatus.Failed;
                    await _documentRepository.UpdateAsync(document);

                    _logger.LogError("❌ Advanced processing failed for document {DocumentId}: {ErrorMessage}", 
                        documentId, processingResult.ErrorMessage);

                    return result;
                }
            }
            catch (Exception processingEx)
            {
                stopwatch.Stop();
                
                // Create error result
                var errorResult = new ProcessingResult
                {
                    DocumentId = documentId,
                    ProcessingType = "AdvancedExtraction",
                    ErrorMessage = processingEx.Message,
                    ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds,
                    ProcessingVersion = "2.0",
                    CreatedAt = DateTime.UtcNow
                };

                await _processingResultRepository.CreateAsync(errorResult);

                document.Status = ProcessingStatus.Failed;
                await _documentRepository.UpdateAsync(document);

                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Advanced processing failed for document {DocumentId}", documentId);
            throw;
        }
    }

    /// <summary>
    /// Estimate page count based on content and format
    /// </summary>
    private static int EstimatePageCount(string? text, string format)
    {
        if (string.IsNullOrEmpty(text))
            return 1;

        return format.ToUpperInvariant() switch
        {
            "PDF" => Math.Max(1, text.Length / 2000), // ~2000 chars per page
            "DOCX" => Math.Max(1, text.Length / 1500), // ~1500 chars per page
            "TXT" => Math.Max(1, text.Split('\n').Length / 50), // ~50 lines per page
            _ => 1
        };
    }

    /// <summary>
    /// Map document entity to DTO
    /// </summary>
    private static DocumentDto MapToDocumentDto(Document document)
    {
        return new DocumentDto
        {
            Id = document.Id,
            FileName = document.FileName,
            FileSize = document.FileSize,
            ContentType = document.ContentType,
            Status = document.Status.ToString(),
            PageCount = document.PageCount,
            DetectedLanguage = document.DetectedLanguage,
            CreatedAt = document.CreatedAt,
            ProcessedAt = document.ProcessedAt,
            ProcessingResults = new List<ProcessingResultDto>() // TODO: Implement processing results
        };
    }
}