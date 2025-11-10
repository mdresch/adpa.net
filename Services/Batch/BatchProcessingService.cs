using ADPA.Models.DTOs;
using ADPA.Models.Entities;
using ADPA.Services.Notifications;
using System.Collections.Concurrent;

namespace ADPA.Services.Batch;

/// <summary>
/// Batch processing status
/// </summary>
public enum BatchProcessingStatus
{
    Queued,
    InProgress,
    Completed,
    Failed,
    Cancelled
}

/// <summary>
/// Batch processing job information
/// </summary>
public class BatchProcessingJob
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string JobName { get; set; } = string.Empty;
    public BatchProcessingStatus Status { get; set; } = BatchProcessingStatus.Queued;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<Guid> DocumentIds { get; set; } = new();
    public List<BatchDocumentResult> Results { get; set; } = new();
    public int TotalDocuments => DocumentIds.Count;
    public int ProcessedDocuments => Results.Count;
    public int SuccessfulDocuments => Results.Count(r => r.Success);
    public int FailedDocuments => Results.Count(r => !r.Success);
    public double ProgressPercentage => TotalDocuments > 0 ? (double)ProcessedDocuments / TotalDocuments * 100 : 0;
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Individual document result within a batch
/// </summary>
public class BatchDocumentResult
{
    public Guid DocumentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan ProcessingTime { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Batch upload request
/// </summary>
public class BatchUploadRequest
{
    public string JobName { get; set; } = string.Empty;
    public List<IFormFile> Files { get; set; } = new();
    public bool ProcessImmediately { get; set; } = true;
    public Dictionary<string, string> Options { get; set; } = new();
}

/// <summary>
/// Batch processing options
/// </summary>
public class BatchProcessingOptions
{
    public bool EnableOcr { get; set; } = true;
    public bool EnableClassification { get; set; } = true;
    public bool EnableTextAnalysis { get; set; } = true;
    public bool StopOnFirstError { get; set; } = false;
    public int MaxConcurrentProcessing { get; set; } = 3;
    public TimeSpan MaxProcessingTime { get; set; } = TimeSpan.FromMinutes(30);
    public Dictionary<string, object> CustomOptions { get; set; } = new();
}

/// <summary>
/// Interface for batch processing service
/// </summary>
public interface IBatchProcessingService
{
    /// <summary>
    /// Create a new batch processing job
    /// </summary>
    Task<BatchProcessingJob> CreateBatchJobAsync(Guid userId, string jobName, List<Guid> documentIds, BatchProcessingOptions? options = null);
    
    /// <summary>
    /// Upload multiple files and create batch job
    /// </summary>
    Task<BatchProcessingJob> UploadBatchAsync(Guid userId, BatchUploadRequest request);
    
    /// <summary>
    /// Start processing a batch job
    /// </summary>
    Task StartBatchProcessingAsync(Guid jobId);
    
    /// <summary>
    /// Cancel a batch job
    /// </summary>
    Task CancelBatchJobAsync(Guid jobId);
    
    /// <summary>
    /// Get batch job status
    /// </summary>
    Task<BatchProcessingJob?> GetBatchJobAsync(Guid jobId);
    
    /// <summary>
    /// Get all batch jobs for a user
    /// </summary>
    Task<IEnumerable<BatchProcessingJob>> GetUserBatchJobsAsync(Guid userId);
    
    /// <summary>
    /// Get active batch jobs
    /// </summary>
    Task<IEnumerable<BatchProcessingJob>> GetActiveBatchJobsAsync();
}

/// <summary>
/// Batch processing service implementation
/// Phase 4: Advanced Features - Batch Processing
/// </summary>
public class BatchProcessingService : IBatchProcessingService
{
    private readonly IDocumentService _documentService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<BatchProcessingService> _logger;
    private readonly ConcurrentDictionary<Guid, BatchProcessingJob> _batchJobs;
    private readonly SemaphoreSlim _processingLock;

    public BatchProcessingService(
        IDocumentService documentService,
        INotificationService notificationService,
        ILogger<BatchProcessingService> logger)
    {
        _documentService = documentService;
        _notificationService = notificationService;
        _logger = logger;
        _batchJobs = new ConcurrentDictionary<Guid, BatchProcessingJob>();
        _processingLock = new SemaphoreSlim(1, 1);
    }

    /// <summary>
    /// Create a new batch processing job
    /// </summary>
    public async Task<BatchProcessingJob> CreateBatchJobAsync(Guid userId, string jobName, List<Guid> documentIds, BatchProcessingOptions? options = null)
    {
        try
        {
            _logger.LogInformation("📦 Creating batch job '{JobName}' for user {UserId} with {DocumentCount} documents", 
                jobName, userId, documentIds.Count);

            var job = new BatchProcessingJob
            {
                UserId = userId,
                JobName = jobName,
                DocumentIds = documentIds,
                Status = BatchProcessingStatus.Queued
            };

            if (options != null)
            {
                job.Metadata["ProcessingOptions"] = options;
            }

            _batchJobs[job.Id] = job;

            // Send notification about batch job creation
            await _notificationService.SendToUserAsync(userId, "BatchJobCreated", new
            {
                JobId = job.Id,
                JobName = job.JobName,
                DocumentCount = job.TotalDocuments,
                Status = job.Status.ToString(),
                CreatedAt = job.CreatedAt
            });

            _logger.LogInformation("✅ Batch job created: {JobId} - {JobName}", job.Id, job.JobName);
            return job;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to create batch job '{JobName}' for user {UserId}", jobName, userId);
            throw;
        }
    }

    /// <summary>
    /// Upload multiple files and create batch job
    /// </summary>
    public async Task<BatchProcessingJob> UploadBatchAsync(Guid userId, BatchUploadRequest request)
    {
        try
        {
            _logger.LogInformation("📤 Starting batch upload '{JobName}' for user {UserId} with {FileCount} files", 
                request.JobName, userId, request.Files.Count);

            var documentIds = new List<Guid>();

            // Upload each file
            foreach (var file in request.Files)
            {
                try
                {
                    var uploadDto = new DocumentUploadDto
                    {
                        File = file,
                        // Add any additional upload options from request.Options
                    };

                    var document = await _documentService.UploadDocumentAsync(userId, uploadDto);
                    documentIds.Add(document.Id);

                    _logger.LogDebug("✅ Uploaded file {FileName} as document {DocumentId}", file.FileName, document.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Failed to upload file {FileName} in batch upload", file.FileName);
                    // Continue with other files unless StopOnFirstError is enabled
                }
            }

            if (documentIds.Count == 0)
            {
                throw new InvalidOperationException("No files were successfully uploaded");
            }

            // Create batch job
            var options = new BatchProcessingOptions
            {
                EnableOcr = request.Options.ContainsKey("enableOcr") && bool.Parse(request.Options["enableOcr"]),
                EnableClassification = request.Options.ContainsKey("enableClassification") && bool.Parse(request.Options["enableClassification"]),
                EnableTextAnalysis = request.Options.ContainsKey("enableTextAnalysis") && bool.Parse(request.Options["enableTextAnalysis"])
            };

            var job = await CreateBatchJobAsync(userId, request.JobName, documentIds, options);

            // Start processing if requested
            if (request.ProcessImmediately)
            {
                _ = Task.Run(() => StartBatchProcessingAsync(job.Id));
            }

            _logger.LogInformation("✅ Batch upload completed: {JobId} - {JobName} with {DocumentCount} documents", 
                job.Id, job.JobName, documentIds.Count);

            return job;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Batch upload failed for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Start processing a batch job
    /// </summary>
    public async Task StartBatchProcessingAsync(Guid jobId)
    {
        await _processingLock.WaitAsync();
        try
        {
            if (!_batchJobs.TryGetValue(jobId, out var job))
            {
                _logger.LogWarning("⚠️ Batch job not found: {JobId}", jobId);
                return;
            }

            if (job.Status != BatchProcessingStatus.Queued)
            {
                _logger.LogWarning("⚠️ Batch job {JobId} is not in queued status: {Status}", jobId, job.Status);
                return;
            }

            _logger.LogInformation("🚀 Starting batch processing: {JobId} - {JobName}", jobId, job.JobName);

            job.Status = BatchProcessingStatus.InProgress;
            job.StartedAt = DateTime.UtcNow;

            // Send notification about batch processing start
            await _notificationService.SendToUserAsync(job.UserId, "BatchProcessingStarted", new
            {
                JobId = job.Id,
                JobName = job.JobName,
                Status = job.Status.ToString(),
                StartedAt = job.StartedAt
            });

            var options = job.Metadata.ContainsKey("ProcessingOptions") 
                ? (BatchProcessingOptions)job.Metadata["ProcessingOptions"] 
                : new BatchProcessingOptions();

            var successCount = 0;
            var failCount = 0;

            // Process documents with controlled concurrency
            var semaphore = new SemaphoreSlim(options.MaxConcurrentProcessing, options.MaxConcurrentProcessing);
            var tasks = job.DocumentIds.Select(async documentId =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var startTime = DateTime.UtcNow;
                    var result = await ProcessSingleDocumentAsync(documentId, job, options);
                    result.ProcessingTime = DateTime.UtcNow - startTime;

                    lock (job.Results)
                    {
                        job.Results.Add(result);
                        if (result.Success) successCount++; else failCount++;
                    }

                    // Send progress update
                    await _notificationService.SendToUserAsync(job.UserId, "BatchProgressUpdate", new
                    {
                        JobId = job.Id,
                        Progress = job.ProgressPercentage,
                        ProcessedCount = job.ProcessedDocuments,
                        TotalCount = job.TotalDocuments,
                        SuccessCount = successCount,
                        FailCount = failCount,
                        LastProcessedFile = result.FileName
                    });

                    return result;
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);

            // Complete the batch job
            job.Status = successCount > 0 ? BatchProcessingStatus.Completed : BatchProcessingStatus.Failed;
            job.CompletedAt = DateTime.UtcNow;

            // Send final notification
            await _notificationService.SendToUserAsync(job.UserId, "BatchProcessingCompleted", new
            {
                JobId = job.Id,
                JobName = job.JobName,
                Status = job.Status.ToString(),
                CompletedAt = job.CompletedAt,
                TotalProcessingTime = job.CompletedAt - job.StartedAt,
                TotalDocuments = job.TotalDocuments,
                SuccessfulDocuments = job.SuccessfulDocuments,
                FailedDocuments = job.FailedDocuments,
                OverallSuccess = job.SuccessfulDocuments > 0
            });

            _logger.LogInformation("✅ Batch processing completed: {JobId} - Success: {SuccessCount}, Failed: {FailCount}", 
                jobId, job.SuccessfulDocuments, job.FailedDocuments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Batch processing failed: {JobId}", jobId);

            if (_batchJobs.TryGetValue(jobId, out var job))
            {
                job.Status = BatchProcessingStatus.Failed;
                job.ErrorMessage = ex.Message;
                job.CompletedAt = DateTime.UtcNow;

                await _notificationService.SendToUserAsync(job.UserId, "BatchProcessingFailed", new
                {
                    JobId = job.Id,
                    JobName = job.JobName,
                    Error = ex.Message,
                    FailedAt = job.CompletedAt
                });
            }
        }
        finally
        {
            _processingLock.Release();
        }
    }

    /// <summary>
    /// Process a single document within a batch
    /// </summary>
    private async Task<BatchDocumentResult> ProcessSingleDocumentAsync(Guid documentId, BatchProcessingJob job, BatchProcessingOptions options)
    {
        try
        {
            _logger.LogDebug("🔄 Processing document {DocumentId} in batch {JobId}", documentId, job.Id);

            // Get document info
            var document = await _documentService.GetDocumentAsync(documentId);
            if (document == null)
            {
                return new BatchDocumentResult
                {
                    DocumentId = documentId,
                    FileName = "Unknown",
                    Success = false,
                    ErrorMessage = "Document not found"
                };
            }

            // Process the document using advanced processing
            var result = await _documentService.ProcessDocumentWithAdvancedServiceAsync(documentId);

            return new BatchDocumentResult
            {
                DocumentId = documentId,
                FileName = document.FileName,
                Success = result != null,
                ErrorMessage = result == null ? "Processing failed" : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to process document {DocumentId} in batch {JobId}", documentId, job.Id);
            return new BatchDocumentResult
            {
                DocumentId = documentId,
                FileName = "Error",
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Cancel a batch job
    /// </summary>
    public async Task CancelBatchJobAsync(Guid jobId)
    {
        try
        {
            if (_batchJobs.TryGetValue(jobId, out var job))
            {
                job.Status = BatchProcessingStatus.Cancelled;
                job.CompletedAt = DateTime.UtcNow;

                await _notificationService.SendToUserAsync(job.UserId, "BatchJobCancelled", new
                {
                    JobId = job.Id,
                    JobName = job.JobName,
                    CancelledAt = job.CompletedAt
                });

                _logger.LogInformation("🚫 Batch job cancelled: {JobId} - {JobName}", jobId, job.JobName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to cancel batch job: {JobId}", jobId);
        }
    }

    /// <summary>
    /// Get batch job status
    /// </summary>
    public async Task<BatchProcessingJob?> GetBatchJobAsync(Guid jobId)
    {
        _batchJobs.TryGetValue(jobId, out var job);
        return await Task.FromResult(job);
    }

    /// <summary>
    /// Get all batch jobs for a user
    /// </summary>
    public async Task<IEnumerable<BatchProcessingJob>> GetUserBatchJobsAsync(Guid userId)
    {
        var userJobs = _batchJobs.Values.Where(j => j.UserId == userId).OrderByDescending(j => j.CreatedAt);
        return await Task.FromResult(userJobs);
    }

    /// <summary>
    /// Get active batch jobs
    /// </summary>
    public async Task<IEnumerable<BatchProcessingJob>> GetActiveBatchJobsAsync()
    {
        var activeJobs = _batchJobs.Values.Where(j => j.Status == BatchProcessingStatus.InProgress || j.Status == BatchProcessingStatus.Queued);
        return await Task.FromResult(activeJobs);
    }
}