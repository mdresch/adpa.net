using Microsoft.AspNetCore.SignalR;
using ADPA.Hubs;
using ADPA.Services.Workflow;

namespace ADPA.Services.Notifications;

/// <summary>
/// Real-time processing status for notifications
/// </summary>
public enum RealTimeProcessingStatus
{
    Queued,
    Started,
    InProgress,
    Completed,
    Failed,
    Cancelled
}

/// <summary>
/// Processing step types for detailed progress tracking
/// </summary>
public enum ProcessingStepType
{
    Upload,
    Validation,
    TextExtraction,
    MetadataExtraction,
    OcrProcessing,
    Classification,
    TextAnalysis,
    Intelligence,
    Finalization
}

/// <summary>
/// Real-time notification message
/// </summary>
public class ProcessingNotification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DocumentId { get; set; }
    public Guid UserId { get; set; }
    public RealTimeProcessingStatus Status { get; set; }
    public ProcessingStepType? Step { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public int ProgressPercentage { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public TimeSpan? ElapsedTime { get; set; }
    public TimeSpan? EstimatedTimeRemaining { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// System notification for admin users
/// </summary>
public class SystemNotification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string Severity { get; set; } = "Info"; // Info, Warning, Error
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Interface for real-time notification service
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Send processing notification to user and document subscribers
    /// </summary>
    Task SendProcessingNotificationAsync(ProcessingNotification notification);
    
    /// <summary>
    /// Send system notification to administrators
    /// </summary>
    Task SendSystemNotificationAsync(SystemNotification notification);
    
    /// <summary>
    /// Send notification to specific user
    /// </summary>
    Task SendToUserAsync(Guid userId, string method, object data);
    
    /// <summary>
    /// Send notification to document subscribers
    /// </summary>
    Task SendToDocumentSubscribersAsync(Guid documentId, string method, object data);
    
    /// <summary>
    /// Send notification to all system administrators
    /// </summary>
    Task SendToSystemAdminsAsync(string method, object data);
    
    /// <summary>
    /// Send workflow notification to relevant users
    /// </summary>
    Task SendWorkflowNotificationAsync(WorkflowNotification notification);
}

/// <summary>
/// Real-time notification service using SignalR
/// Phase 4: Advanced Features - Real-time Processing Updates
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IHubContext<ProcessingHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IHubContext<ProcessingHub> hubContext, ILogger<NotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Send processing notification to relevant users
    /// </summary>
    public async Task SendProcessingNotificationAsync(ProcessingNotification notification)
    {
        try
        {
            _logger.LogInformation("📢 Sending processing notification: Document {DocumentId}, Status {Status}, Step {Step}", 
                notification.DocumentId, notification.Status, notification.Step);

            // Send to user who owns the document
            await SendToUserAsync(notification.UserId, "ProcessingUpdate", notification);
            
            // Send to document subscribers
            await SendToDocumentSubscribersAsync(notification.DocumentId, "ProcessingUpdate", notification);
            
            _logger.LogDebug("✅ Processing notification sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send processing notification for document {DocumentId}", notification.DocumentId);
        }
    }

    /// <summary>
    /// Send system notification to administrators
    /// </summary>
    public async Task SendSystemNotificationAsync(SystemNotification notification)
    {
        try
        {
            _logger.LogInformation("📢 Sending system notification: Type {Type}, Severity {Severity}", 
                notification.Type, notification.Severity);

            await SendToSystemAdminsAsync("SystemNotification", notification);
            
            _logger.LogDebug("✅ System notification sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send system notification: {Type}", notification.Type);
        }
    }

    /// <summary>
    /// Send notification to specific user
    /// </summary>
    public async Task SendToUserAsync(Guid userId, string method, object data)
    {
        try
        {
            var groupName = $"User_{userId}";
            await _hubContext.Clients.Group(groupName).SendAsync(method, data);
            _logger.LogDebug("📤 Sent notification to user {UserId} via method {Method}", userId, method);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send notification to user {UserId}", userId);
        }
    }

    /// <summary>
    /// Send notification to document subscribers
    /// </summary>
    public async Task SendToDocumentSubscribersAsync(Guid documentId, string method, object data)
    {
        try
        {
            var groupName = $"Document_{documentId}";
            await _hubContext.Clients.Group(groupName).SendAsync(method, data);
            _logger.LogDebug("📤 Sent notification to document {DocumentId} subscribers via method {Method}", documentId, method);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send notification to document {DocumentId} subscribers", documentId);
        }
    }

    /// <summary>
    /// Send notification to system administrators
    /// </summary>
    public async Task SendToSystemAdminsAsync(string method, object data)
    {
        try
        {
            await _hubContext.Clients.Group("SystemNotifications").SendAsync(method, data);
            _logger.LogDebug("📤 Sent notification to system administrators via method {Method}", method);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send notification to system administrators");
        }
    }

    /// <summary>
    /// Send workflow notification to relevant users
    /// </summary>
    public async Task SendWorkflowNotificationAsync(WorkflowNotification notification)
    {
        try
        {
            _logger.LogInformation("🔄 Sending workflow notification: Workflow {WorkflowId}, Type {Type}", 
                notification.WorkflowId, notification.Type);

            // Send to specific user
            await SendToUserAsync(Guid.Parse(notification.UserId), "WorkflowUpdate", notification);
            
            // Send to document subscribers
            await SendToDocumentSubscribersAsync(notification.DocumentId, "WorkflowUpdate", notification);
            
            // Send to workflow subscribers
            var workflowGroupName = $"Workflow_{notification.WorkflowId}";
            await _hubContext.Clients.Group(workflowGroupName).SendAsync("WorkflowUpdate", notification);
            
            _logger.LogDebug("✅ Workflow notification sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send workflow notification for workflow {WorkflowId}", notification.WorkflowId);
        }
    }
}

/// <summary>
/// Helper extensions for common notification scenarios
/// </summary>
public static class NotificationExtensions
{
    /// <summary>
    /// Send document processing started notification
    /// </summary>
    public static async Task NotifyProcessingStartedAsync(this INotificationService service, Guid documentId, Guid userId, string fileName)
    {
        await service.SendProcessingNotificationAsync(new ProcessingNotification
        {
            DocumentId = documentId,
            UserId = userId,
            Status = RealTimeProcessingStatus.Started,
            Step = ProcessingStepType.Validation,
            Message = $"Processing started for '{fileName}'",
            ProgressPercentage = 5
        });
    }

    /// <summary>
    /// Send document processing completed notification
    /// </summary>
    public static async Task NotifyProcessingCompletedAsync(this INotificationService service, Guid documentId, Guid userId, string fileName, TimeSpan processingTime)
    {
        await service.SendProcessingNotificationAsync(new ProcessingNotification
        {
            DocumentId = documentId,
            UserId = userId,
            Status = RealTimeProcessingStatus.Completed,
            Step = ProcessingStepType.Finalization,
            Message = $"Processing completed for '{fileName}'",
            ProgressPercentage = 100,
            ElapsedTime = processingTime
        });
    }

    /// <summary>
    /// Send processing step update notification
    /// </summary>
    public static async Task NotifyProcessingStepAsync(this INotificationService service, Guid documentId, Guid userId, ProcessingStepType step, string message, int progress)
    {
        await service.SendProcessingNotificationAsync(new ProcessingNotification
        {
            DocumentId = documentId,
            UserId = userId,
            Status = RealTimeProcessingStatus.InProgress,
            Step = step,
            Message = message,
            ProgressPercentage = progress
        });
    }

    /// <summary>
    /// Send processing error notification
    /// </summary>
    public static async Task NotifyProcessingErrorAsync(this INotificationService service, Guid documentId, Guid userId, string error, string? details = null)
    {
        await service.SendProcessingNotificationAsync(new ProcessingNotification
        {
            DocumentId = documentId,
            UserId = userId,
            Status = RealTimeProcessingStatus.Failed,
            Message = error,
            Details = details
        });
    }

    /// <summary>
    /// Send system alert notification
    /// </summary>
    public static async Task NotifySystemAlertAsync(this INotificationService service, string type, string message, string severity = "Info", string? details = null)
    {
        await service.SendSystemNotificationAsync(new SystemNotification
        {
            Type = type,
            Message = message,
            Severity = severity,
            Details = details
        });
    }
}