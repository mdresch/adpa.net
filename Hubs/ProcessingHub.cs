using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace ADPA.Hubs;

/// <summary>
/// SignalR Hub for real-time document processing notifications
/// Phase 4: Advanced Features - Real-time Processing Updates
/// </summary>
[Authorize]
public class ProcessingHub : Hub
{
    private readonly ILogger<ProcessingHub> _logger;

    public ProcessingHub(ILogger<ProcessingHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handle client connection
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        _logger.LogInformation("🔌 User {UserId} connected to ProcessingHub: {ConnectionId}", userId, Context.ConnectionId);
        
        // Join user to their personal notification group
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            _logger.LogDebug("👥 Added connection {ConnectionId} to group User_{UserId}", Context.ConnectionId, userId);
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Handle client disconnection
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        _logger.LogInformation("🔌 User {UserId} disconnected from ProcessingHub: {ConnectionId}", userId, Context.ConnectionId);
        
        if (exception != null)
        {
            _logger.LogWarning(exception, "⚠️ User {UserId} disconnected with error", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Subscribe to document processing updates
    /// </summary>
    /// <param name="documentId">Document ID to monitor</param>
    [HubMethodName("SubscribeToDocument")]
    public async Task SubscribeToDocumentAsync(string documentId)
    {
        try
        {
            var userId = Context.UserIdentifier;
            _logger.LogInformation("📋 User {UserId} subscribing to document {DocumentId} updates", userId, documentId);
            
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Document_{documentId}");
            
            // Notify client of successful subscription
            await Clients.Caller.SendAsync("DocumentSubscribed", new
            {
                DocumentId = documentId,
                Message = "Successfully subscribed to document updates",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error subscribing to document {DocumentId}", documentId);
            await Clients.Caller.SendAsync("Error", new
            {
                Message = "Failed to subscribe to document updates",
                Details = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Unsubscribe from document processing updates
    /// </summary>
    /// <param name="documentId">Document ID to stop monitoring</param>
    [HubMethodName("UnsubscribeFromDocument")]
    public async Task UnsubscribeFromDocumentAsync(string documentId)
    {
        try
        {
            var userId = Context.UserIdentifier;
            _logger.LogInformation("📋 User {UserId} unsubscribing from document {DocumentId} updates", userId, documentId);
            
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Document_{documentId}");
            
            // Notify client of successful unsubscription
            await Clients.Caller.SendAsync("DocumentUnsubscribed", new
            {
                DocumentId = documentId,
                Message = "Successfully unsubscribed from document updates",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error unsubscribing from document {DocumentId}", documentId);
            await Clients.Caller.SendAsync("Error", new
            {
                Message = "Failed to unsubscribe from document updates",
                Details = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Join system notifications group for admin users
    /// </summary>
    [HubMethodName("JoinSystemNotifications")]
    public async Task JoinSystemNotificationsAsync()
    {
        try
        {
            var userId = Context.UserIdentifier;
            _logger.LogInformation("🔔 User {UserId} joining system notifications", userId);
            
            // Note: In production, check user roles/permissions here
            await Groups.AddToGroupAsync(Context.ConnectionId, "SystemNotifications");
            
            await Clients.Caller.SendAsync("SystemNotificationsJoined", new
            {
                Message = "Successfully joined system notifications",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error joining system notifications");
            await Clients.Caller.SendAsync("Error", new
            {
                Message = "Failed to join system notifications",
                Details = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Leave system notifications group
    /// </summary>
    [HubMethodName("LeaveSystemNotifications")]
    public async Task LeaveSystemNotificationsAsync()
    {
        try
        {
            var userId = Context.UserIdentifier;
            _logger.LogInformation("🔔 User {UserId} leaving system notifications", userId);
            
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SystemNotifications");
            
            await Clients.Caller.SendAsync("SystemNotificationsLeft", new
            {
                Message = "Successfully left system notifications",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error leaving system notifications");
            await Clients.Caller.SendAsync("Error", new
            {
                Message = "Failed to leave system notifications",
                Details = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}