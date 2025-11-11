namespace ADPA.Web.Services;

public interface IAnalyticsService
{
    Task<Dictionary<string, object>> GetAnalyticsSummaryAsync();
    Task<Dictionary<string, object>> GetRealtimeMetricsAsync();
    Task<Dictionary<string, object>> GetPerformanceAnalyticsAsync();
}

public class AnalyticsService : IAnalyticsService
{
    private readonly ApiService _apiService;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(ApiService apiService, ILogger<AnalyticsService> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<Dictionary<string, object>> GetAnalyticsSummaryAsync()
    {
        try
        {
            return await _apiService.GetAsync<Dictionary<string, object>>("/analytics/summary") 
                ?? new Dictionary<string, object>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get analytics summary");
            return new Dictionary<string, object>();
        }
    }

    public async Task<Dictionary<string, object>> GetRealtimeMetricsAsync()
    {
        try
        {
            return await _apiService.GetAsync<Dictionary<string, object>>("/analytics/realtime") 
                ?? new Dictionary<string, object>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get realtime metrics");
            return new Dictionary<string, object>();
        }
    }

    public async Task<Dictionary<string, object>> GetPerformanceAnalyticsAsync()
    {
        try
        {
            return await _apiService.GetAsync<Dictionary<string, object>>("/analytics/performance") 
                ?? new Dictionary<string, object>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get performance analytics");
            return new Dictionary<string, object>();
        }
    }
}