using Microsoft.AspNetCore.SignalR.Client;
using ADPA.Shared.DTOs;

namespace ADPA.Web.Services;

public class SignalRService : IAsyncDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SignalRService> _logger;
    private HubConnection? _hubConnection;

    public event Action<Guid, ProcessingStatus>? OnDocumentStatusUpdated;
    public event Action<string>? OnNotificationReceived;
    
    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public SignalRService(IConfiguration configuration, ILogger<SignalRService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync()
    {
        if (_hubConnection != null)
            return;

        var hubUrl = _configuration["ApiBaseUrl"]?.TrimEnd('/') + "/hubs/processing";
        
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<Guid, ProcessingStatus>("DocumentStatusUpdated", (documentId, status) =>
        {
            OnDocumentStatusUpdated?.Invoke(documentId, status);
        });

        _hubConnection.On<string>("NotificationReceived", (message) =>
        {
            OnNotificationReceived?.Invoke(message);
        });

        try
        {
            await _hubConnection.StartAsync();
            _logger.LogInformation("SignalR connection established");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start SignalR connection");
        }
    }

    public async Task StopAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }
}