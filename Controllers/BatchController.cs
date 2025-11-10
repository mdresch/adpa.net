using ADPA.Services.Batch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ADPA.Controllers;

/// <summary>
/// Phase 4: Batch Processing API Controller
/// Handles batch upload and processing of multiple documents
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BatchController : ControllerBase
{
    private readonly IBatchProcessingService _batchService;
    private readonly ILogger<BatchController> _logger;

    public BatchController(IBatchProcessingService batchService, ILogger<BatchController> logger)
    {
        _batchService = batchService;
        _logger = logger;
    }

    /// <summary>
    /// Upload multiple files and create batch processing job
    /// </summary>
    /// <param name="files">Files to upload</param>
    /// <param name="jobName">Name for the batch job</param>
    /// <param name="processImmediately">Start processing immediately</param>
    /// <param name="enableOcr">Enable OCR processing</param>
    /// <param name="enableClassification">Enable document classification</param>
    /// <param name="enableTextAnalysis">Enable text analysis</param>
    /// <returns>Batch processing job information</returns>
    [HttpPost("upload")]
    public async Task<ActionResult<BatchProcessingJob>> UploadBatchAsync(
        [FromForm] List<IFormFile> files,
        [FromForm] string jobName = "Batch Upload",
        [FromForm] bool processImmediately = true,
        [FromForm] bool enableOcr = true,
        [FromForm] bool enableClassification = true,
        [FromForm] bool enableTextAnalysis = true)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("📤 Batch upload request from user {UserId}: {FileCount} files for job '{JobName}'", 
                userId, files?.Count ?? 0, jobName);

            if (files == null || files.Count == 0)
            {
                return BadRequest(new { error = "No files provided" });
            }

            if (files.Count > 50) // Reasonable limit
            {
                return BadRequest(new { error = "Too many files. Maximum 50 files per batch." });
            }

            var request = new BatchUploadRequest
            {
                JobName = jobName,
                Files = files,
                ProcessImmediately = processImmediately,
                Options = new Dictionary<string, string>
                {
                    ["enableOcr"] = enableOcr.ToString(),
                    ["enableClassification"] = enableClassification.ToString(),
                    ["enableTextAnalysis"] = enableTextAnalysis.ToString()
                }
            };

            var job = await _batchService.UploadBatchAsync(userId, request);

            _logger.LogInformation("✅ Batch upload created: {JobId} - {JobName} with {FileCount} files", 
                job.Id, job.JobName, files.Count);

            return Ok(job);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Batch upload failed");
            return StatusCode(500, new { error = "Batch upload failed", details = ex.Message });
        }
    }

    /// <summary>
    /// Create a batch job from existing documents
    /// </summary>
    /// <param name="request">Batch job creation request</param>
    /// <returns>Batch processing job information</returns>
    [HttpPost("create-job")]
    public async Task<ActionResult<BatchProcessingJob>> CreateBatchJobAsync([FromBody] CreateBatchJobRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("📦 Creating batch job '{JobName}' for user {UserId} with {DocumentCount} documents", 
                request.JobName, userId, request.DocumentIds.Count);

            if (request.DocumentIds == null || request.DocumentIds.Count == 0)
            {
                return BadRequest(new { error = "No document IDs provided" });
            }

            var options = new BatchProcessingOptions
            {
                EnableOcr = request.EnableOcr,
                EnableClassification = request.EnableClassification,
                EnableTextAnalysis = request.EnableTextAnalysis,
                StopOnFirstError = request.StopOnFirstError,
                MaxConcurrentProcessing = Math.Min(request.MaxConcurrentProcessing, 10) // Limit concurrency
            };

            var job = await _batchService.CreateBatchJobAsync(userId, request.JobName, request.DocumentIds, options);

            _logger.LogInformation("✅ Batch job created: {JobId} - {JobName}", job.Id, job.JobName);

            return Ok(job);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to create batch job");
            return StatusCode(500, new { error = "Failed to create batch job", details = ex.Message });
        }
    }

    /// <summary>
    /// Start processing a batch job
    /// </summary>
    /// <param name="jobId">Batch job ID</param>
    /// <returns>Success status</returns>
    [HttpPost("{jobId}/start")]
    public async Task<ActionResult> StartBatchProcessingAsync(Guid jobId)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("🚀 Starting batch processing for job {JobId} by user {UserId}", jobId, userId);

            var job = await _batchService.GetBatchJobAsync(jobId);
            if (job == null)
            {
                return NotFound(new { error = "Batch job not found" });
            }

            if (job.UserId != userId)
            {
                return Forbid("You can only start your own batch jobs");
            }

            // Start processing in background
            _ = Task.Run(() => _batchService.StartBatchProcessingAsync(jobId));

            _logger.LogInformation("✅ Batch processing started for job {JobId}", jobId);

            return Ok(new { message = "Batch processing started", jobId = jobId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to start batch processing for job {JobId}", jobId);
            return StatusCode(500, new { error = "Failed to start batch processing", details = ex.Message });
        }
    }

    /// <summary>
    /// Cancel a batch job
    /// </summary>
    /// <param name="jobId">Batch job ID</param>
    /// <returns>Success status</returns>
    [HttpPost("{jobId}/cancel")]
    public async Task<ActionResult> CancelBatchJobAsync(Guid jobId)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("🚫 Cancelling batch job {JobId} by user {UserId}", jobId, userId);

            var job = await _batchService.GetBatchJobAsync(jobId);
            if (job == null)
            {
                return NotFound(new { error = "Batch job not found" });
            }

            if (job.UserId != userId)
            {
                return Forbid("You can only cancel your own batch jobs");
            }

            await _batchService.CancelBatchJobAsync(jobId);

            _logger.LogInformation("✅ Batch job cancelled: {JobId}", jobId);

            return Ok(new { message = "Batch job cancelled", jobId = jobId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to cancel batch job {JobId}", jobId);
            return StatusCode(500, new { error = "Failed to cancel batch job", details = ex.Message });
        }
    }

    /// <summary>
    /// Get batch job status and progress
    /// </summary>
    /// <param name="jobId">Batch job ID</param>
    /// <returns>Batch job information</returns>
    [HttpGet("{jobId}")]
    public async Task<ActionResult<BatchProcessingJob>> GetBatchJobAsync(Guid jobId)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            var job = await _batchService.GetBatchJobAsync(jobId);
            if (job == null)
            {
                return NotFound(new { error = "Batch job not found" });
            }

            if (job.UserId != userId)
            {
                return Forbid("You can only view your own batch jobs");
            }

            return Ok(job);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to get batch job {JobId}", jobId);
            return StatusCode(500, new { error = "Failed to get batch job", details = ex.Message });
        }
    }

    /// <summary>
    /// Get all batch jobs for the current user
    /// </summary>
    /// <returns>List of batch jobs</returns>
    [HttpGet("my-jobs")]
    public async Task<ActionResult<IEnumerable<BatchProcessingJob>>> GetMyBatchJobsAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("📋 Getting batch jobs for user {UserId}", userId);

            var jobs = await _batchService.GetUserBatchJobsAsync(userId);

            return Ok(jobs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to get batch jobs for user");
            return StatusCode(500, new { error = "Failed to get batch jobs", details = ex.Message });
        }
    }

    /// <summary>
    /// Get active batch jobs (admin only)
    /// </summary>
    /// <returns>List of active batch jobs</returns>
    [HttpGet("active")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<ActionResult<IEnumerable<BatchProcessingJob>>> GetActiveBatchJobsAsync()
    {
        try
        {
            _logger.LogInformation("📊 Getting active batch jobs");

            var jobs = await _batchService.GetActiveBatchJobsAsync();

            return Ok(jobs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to get active batch jobs");
            return StatusCode(500, new { error = "Failed to get active batch jobs", details = ex.Message });
        }
    }

    /// <summary>
    /// Get batch processing capabilities and limits
    /// </summary>
    /// <returns>Batch processing capabilities</returns>
    [HttpGet("capabilities")]
    public ActionResult<object> GetBatchCapabilities()
    {
        try
        {
            var capabilities = new
            {
                MaxFilesPerBatch = 50,
                MaxConcurrentProcessing = 10,
                SupportedFeatures = new[]
                {
                    "Bulk File Upload",
                    "Real-time Progress Tracking", 
                    "OCR Processing",
                    "Document Classification",
                    "Text Analysis",
                    "Batch Result Aggregation",
                    "Error Handling & Recovery"
                },
                SupportedFileTypes = new[]
                {
                    "PDF", "DOC", "DOCX", "TXT", "CSV",
                    "JPG", "JPEG", "PNG", "TIFF", "BMP", "GIF"
                },
                ProcessingOptions = new
                {
                    EnableOcr = "Enable OCR for image documents",
                    EnableClassification = "Enable ML-based document classification", 
                    EnableTextAnalysis = "Enable advanced text analysis",
                    StopOnFirstError = "Stop processing if any document fails",
                    MaxConcurrentProcessing = "Maximum parallel document processing"
                }
            };

            return Ok(capabilities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to get batch capabilities");
            return StatusCode(500, new { error = "Failed to get batch capabilities", details = ex.Message });
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
/// Request model for creating batch jobs from existing documents
/// </summary>
public class CreateBatchJobRequest
{
    public string JobName { get; set; } = string.Empty;
    public List<Guid> DocumentIds { get; set; } = new();
    public bool EnableOcr { get; set; } = true;
    public bool EnableClassification { get; set; } = true;
    public bool EnableTextAnalysis { get; set; } = true;
    public bool StopOnFirstError { get; set; } = false;
    public int MaxConcurrentProcessing { get; set; } = 3;
}