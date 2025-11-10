using ADPA.Models;
using ADPA.Models.DTOs;
using System.Collections.Concurrent;

namespace ADPA.Services;

/// <summary>
/// Interface for data processing service
/// </summary>
public interface IDataProcessingService
{
    Task<DataResponseDto> CreateDataProcessingRequestAsync(CreateDataRequestDto request);
    Task<IEnumerable<DataResponseDto>> GetAllDataAsync();
    Task<DataResponseDto?> GetDataByIdAsync(int id);
    Task ProcessPendingDataAsync();
}

/// <summary>
/// Service for handling data processing operations
/// </summary>
public class DataProcessingService : IDataProcessingService
{
    private readonly ILogger<DataProcessingService> _logger;
    private readonly ConcurrentDictionary<int, ProcessedData> _dataStore;
    private int _nextId;

    public DataProcessingService(ILogger<DataProcessingService> logger)
    {
        _logger = logger;
        _dataStore = new ConcurrentDictionary<int, ProcessedData>();
        _nextId = 1;
    }

    /// <summary>
    /// Creates a new data processing request
    /// </summary>
    public async Task<DataResponseDto> CreateDataProcessingRequestAsync(CreateDataRequestDto request)
    {
        var id = Interlocked.Increment(ref _nextId);
        var processedData = new ProcessedData
        {
            Id = id,
            RawData = request.RawData,
            Status = ProcessingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _dataStore.TryAdd(id, processedData);

        _logger.LogInformation("Created new data processing request with ID: {Id}", processedData.Id);

        // Start processing asynchronously
        _ = Task.Run(() => ProcessDataAsync(processedData.Id));

        return MapToDto(processedData);
    }

    /// <summary>
    /// Gets all processed data
    /// </summary>
    public Task<IEnumerable<DataResponseDto>> GetAllDataAsync()
    {
        var data = _dataStore.Values
            .OrderByDescending(x => x.CreatedAt)
            .ToList();

        return Task.FromResult(data.Select(MapToDto));
    }

    /// <summary>
    /// Gets processed data by ID
    /// </summary>
    public Task<DataResponseDto?> GetDataByIdAsync(int id)
    {
        _dataStore.TryGetValue(id, out var data);
        var result = data != null ? MapToDto(data) : null;
        return Task.FromResult(result);
    }

    /// <summary>
    /// Processes pending data items
    /// </summary>
    public async Task ProcessPendingDataAsync()
    {
        var pendingItems = _dataStore.Values
            .Where(x => x.Status == ProcessingStatus.Pending)
            .ToList();

        var tasks = pendingItems.Select(item => ProcessDataAsync(item.Id));
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Processes individual data item
    /// </summary>
    private async Task ProcessDataAsync(int dataId)
    {
        try
        {
            if (!_dataStore.TryGetValue(dataId, out var data)) return;

            // Update status to processing
            data.Status = ProcessingStatus.Processing;
            _dataStore.TryUpdate(dataId, data, data);

            _logger.LogInformation("Starting processing for data ID: {Id}", dataId);

            // Simulate data processing (replace with actual processing logic)
            await SimulateDataProcessing(data);

            // Update with results
            data.Status = ProcessingStatus.Completed;
            data.CompletedAt = DateTime.UtcNow;
            _dataStore.TryUpdate(dataId, data, data);

            _logger.LogInformation("Completed processing for data ID: {Id}", dataId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing data ID: {Id}", dataId);
            
            if (_dataStore.TryGetValue(dataId, out var data))
            {
                data.Status = ProcessingStatus.Failed;
                data.ErrorMessage = ex.Message;
                data.CompletedAt = DateTime.UtcNow;
                _dataStore.TryUpdate(dataId, data, data);
            }
        }
    }

    /// <summary>
    /// Simulates data processing logic
    /// </summary>
    private async Task SimulateDataProcessing(ProcessedData data)
    {
        // Simulate processing delay
        await Task.Delay(2000);

        // Simple processing: convert to uppercase and add timestamp
        data.ProcessedResult = $"PROCESSED: {data.RawData.ToUpper()} - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
    }

    /// <summary>
    /// Maps entity to DTO
    /// </summary>
    private static DataResponseDto MapToDto(ProcessedData data)
    {
        return new DataResponseDto
        {
            Id = data.Id,
            RawData = data.RawData,
            ProcessedResult = data.ProcessedResult,
            Status = data.Status.ToString(),
            CreatedAt = data.CreatedAt,
            CompletedAt = data.CompletedAt,
            ErrorMessage = data.ErrorMessage
        };
    }
}