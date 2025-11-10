using ADPA.Services.Analytics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ADPA.Controllers;

/// <summary>
/// Phase 3: Analytics API Controller
/// Provides comprehensive analytics, metrics, and KPI endpoints for business intelligence
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    /// <summary>
    /// Get comprehensive analytics summary for a specified period
    /// </summary>
    /// <param name="fromDate">Start date (optional, defaults to 30 days ago)</param>
    /// <param name="toDate">End date (optional, defaults to now)</param>
    /// <returns>Analytics summary with key metrics and insights</returns>
    [HttpGet("summary")]
    public async Task<ActionResult<AnalyticsSummary>> GetAnalyticsSummaryAsync(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("📊 Analytics summary request from user {UserId}: {FromDate} to {ToDate}", 
                userId, fromDate?.ToString("yyyy-MM-dd") ?? "30-days-ago", 
                toDate?.ToString("yyyy-MM-dd") ?? "now");

            var summary = await _analyticsService.GetAnalyticsSummaryAsync(fromDate, toDate);

            _logger.LogInformation("✅ Analytics summary generated: {TotalDocuments} documents analyzed", 
                summary.TotalDocuments);

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to get analytics summary");
            return StatusCode(500, new { error = "Failed to get analytics summary", details = ex.Message });
        }
    }

    /// <summary>
    /// Get document processing metrics for specified time period
    /// </summary>
    /// <param name="period">Time period in hours (default: 24 hours)</param>
    /// <returns>Document processing metrics and KPIs</returns>
    [HttpGet("processing-metrics")]
    public async Task<ActionResult<DocumentProcessingMetrics>> GetDocumentProcessingMetricsAsync(
        [FromQuery] int period = 24)
    {
        try
        {
            var userId = GetCurrentUserId();
            var timeSpan = TimeSpan.FromHours(period);
            
            _logger.LogInformation("📈 Processing metrics request from user {UserId} for {Period} hours", 
                userId, period);

            var metrics = await _analyticsService.GetDocumentProcessingMetricsAsync(timeSpan);

            _logger.LogInformation("✅ Processing metrics generated: {TotalDocuments} docs, {DocsPerHour} docs/hour", 
                metrics.TotalDocumentsProcessed, Math.Round(metrics.DocumentsPerHour, 2));

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to get processing metrics");
            return StatusCode(500, new { error = "Failed to get processing metrics", details = ex.Message });
        }
    }

    /// <summary>
    /// Get performance analytics including response times and throughput
    /// </summary>
    /// <param name="period">Time period in hours (default: 24 hours)</param>
    /// <returns>Performance analytics and system metrics</returns>
    [HttpGet("performance")]
    public async Task<ActionResult<PerformanceAnalytics>> GetPerformanceAnalyticsAsync(
        [FromQuery] int period = 24)
    {
        try
        {
            var userId = GetCurrentUserId();
            var timeSpan = TimeSpan.FromHours(period);
            
            _logger.LogInformation("⚡ Performance analytics request from user {UserId} for {Period} hours", 
                userId, period);

            var analytics = await _analyticsService.GetPerformanceAnalyticsAsync(timeSpan);

            _logger.LogInformation("✅ Performance analytics generated: {AvgResponseTime}ms avg, {ErrorRate}% errors", 
                Math.Round(analytics.AverageResponseTime, 2), Math.Round(analytics.ErrorRate, 2));

            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to get performance analytics");
            return StatusCode(500, new { error = "Failed to get performance analytics", details = ex.Message });
        }
    }

    /// <summary>
    /// Get real-time system metrics and current status
    /// </summary>
    /// <returns>Real-time metrics including active jobs, system health, and current performance</returns>
    [HttpGet("realtime")]
    public async Task<ActionResult<RealTimeMetrics>> GetRealTimeMetricsAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("📡 Real-time metrics request from user {UserId}", userId);

            var metrics = await _analyticsService.GetRealTimeMetricsAsync();

            _logger.LogInformation("✅ Real-time metrics retrieved: {ActiveJobs} active jobs, {SystemHealth} health", 
                metrics.ActiveProcessingJobs, metrics.SystemHealth);

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to get real-time metrics");
            return StatusCode(500, new { error = "Failed to get real-time metrics", details = ex.Message });
        }
    }

    /// <summary>
    /// Get trend analysis for specified metric type and period
    /// </summary>
    /// <param name="trendType">Type of trend to analyze</param>
    /// <param name="period">Time period in hours (default: 168 hours / 7 days)</param>
    /// <returns>Trend analysis with insights and predictions</returns>
    [HttpGet("trends")]
    public async Task<ActionResult<TrendAnalysis>> GetTrendAnalysisAsync(
        [FromQuery] TrendType trendType = TrendType.DocumentVolume,
        [FromQuery] int period = 168)
    {
        try
        {
            var userId = GetCurrentUserId();
            var timeSpan = TimeSpan.FromHours(period);
            
            _logger.LogInformation("📈 Trend analysis request from user {UserId}: {TrendType} for {Period} hours", 
                userId, trendType, period);

            var analysis = await _analyticsService.GetTrendAnalysisAsync(timeSpan, trendType);

            _logger.LogInformation("✅ Trend analysis completed: {TrendType} - {Direction} trend", 
                trendType, analysis.TrendDirection);

            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to get trend analysis");
            return StatusCode(500, new { error = "Failed to get trend analysis", details = ex.Message });
        }
    }

    /// <summary>
    /// Get predictive analytics and forecasts
    /// </summary>
    /// <returns>Predictive analytics with forecasts and recommendations</returns>
    [HttpGet("predictions")]
    public async Task<ActionResult<PredictiveAnalytics>> GetPredictiveAnalyticsAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("🔮 Predictive analytics request from user {UserId}", userId);

            var predictions = await _analyticsService.GetPredictiveAnalyticsAsync();

            _logger.LogInformation("✅ Predictive analytics generated: {PredictionCount} predictions", 
                predictions.Predictions.Count);

            return Ok(predictions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to get predictive analytics");
            return StatusCode(500, new { error = "Failed to get predictive analytics", details = ex.Message });
        }
    }

    /// <summary>
    /// Get custom KPIs for the current user
    /// </summary>
    /// <returns>List of custom KPIs with current values and trends</returns>
    [HttpGet("custom-kpis")]
    public async Task<ActionResult<List<CustomKpi>>> GetCustomKpisAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("📊 Custom KPIs request from user {UserId}", userId);

            var kpis = await _analyticsService.GetCustomKpisAsync(userId);

            _logger.LogInformation("✅ Custom KPIs retrieved: {Count} KPIs", kpis.Count);

            return Ok(kpis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to get custom KPIs");
            return StatusCode(500, new { error = "Failed to get custom KPIs", details = ex.Message });
        }
    }

    /// <summary>
    /// Create a new custom KPI
    /// </summary>
    /// <param name="definition">KPI definition with formula and parameters</param>
    /// <returns>Created custom KPI</returns>
    [HttpPost("custom-kpis")]
    public async Task<ActionResult<CustomKpi>> CreateCustomKpiAsync([FromBody] CustomKpiDefinition definition)
    {
        try
        {
            var userId = GetCurrentUserId();
            definition.UserId = userId;
            
            _logger.LogInformation("📝 Creating custom KPI '{Name}' for user {UserId}", definition.Name, userId);

            if (string.IsNullOrEmpty(definition.Name))
            {
                return BadRequest(new { error = "KPI name is required" });
            }

            if (string.IsNullOrEmpty(definition.Formula))
            {
                return BadRequest(new { error = "KPI formula is required" });
            }

            var kpi = await _analyticsService.CreateCustomKpiAsync(definition);

            _logger.LogInformation("✅ Custom KPI created: {KpiId} - {Name}", kpi.Id, kpi.Name);

            return Ok(kpi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to create custom KPI");
            return StatusCode(500, new { error = "Failed to create custom KPI", details = ex.Message });
        }
    }

    /// <summary>
    /// Get historical analysis for specified date range
    /// </summary>
    /// <param name="fromDate">Start date for analysis</param>
    /// <param name="toDate">End date for analysis</param>
    /// <returns>Historical analysis with trends and insights</returns>
    [HttpGet("historical")]
    public async Task<ActionResult<HistoricalAnalysis>> GetHistoricalAnalysisAsync(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("📚 Historical analysis request from user {UserId}: {FromDate} to {ToDate}", 
                userId, fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd"));

            if (fromDate >= toDate)
            {
                return BadRequest(new { error = "FromDate must be before ToDate" });
            }

            var timeSpan = toDate - fromDate;
            if (timeSpan.TotalDays > 365)
            {
                return BadRequest(new { error = "Analysis period cannot exceed 365 days" });
            }

            var analysis = await _analyticsService.GetHistoricalAnalysisAsync(fromDate, toDate);

            _logger.LogInformation("✅ Historical analysis completed: {InsightCount} insights generated", 
                analysis.KeyInsights.Count);

            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to get historical analysis");
            return StatusCode(500, new { error = "Failed to get historical analysis", details = ex.Message });
        }
    }

    /// <summary>
    /// Get processing events for analysis and debugging
    /// </summary>
    /// <param name="fromDate">Start date for events</param>
    /// <param name="toDate">End date for events</param>
    /// <param name="eventType">Optional event type filter</param>
    /// <returns>List of processing events</returns>
    [HttpGet("events")]
    public async Task<ActionResult<List<ProcessingEvent>>> GetProcessingEventsAsync(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] string? eventType = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("📋 Processing events request from user {UserId}: {FromDate} to {ToDate}, type: {EventType}", 
                userId, fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd"), eventType ?? "all");

            var events = await _analyticsService.GetProcessingEventsAsync(fromDate, toDate, eventType);

            _logger.LogInformation("✅ Processing events retrieved: {Count} events", events.Count);

            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to get processing events");
            return StatusCode(500, new { error = "Failed to get processing events", details = ex.Message });
        }
    }

    /// <summary>
    /// Export analytics data to Excel format
    /// </summary>
    /// <param name="request">Export configuration and parameters</param>
    /// <returns>Excel file with analytics data</returns>
    [HttpPost("export/excel")]
    public async Task<IActionResult> ExportAnalyticsToExcelAsync([FromBody] AnalyticsExportRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            request.UserId = userId;
            
            _logger.LogInformation("📊 Analytics Excel export request from user {UserId}: {FromDate} to {ToDate}", 
                userId, request.FromDate.ToString("yyyy-MM-dd"), request.ToDate.ToString("yyyy-MM-dd"));

            var excelData = await _analyticsService.ExportAnalyticsToExcelAsync(request);

            _logger.LogInformation("✅ Analytics Excel export completed: {Size} bytes", excelData.Length);

            return File(excelData, 
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Analytics_Export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to export analytics to Excel");
            return StatusCode(500, new { error = "Failed to export analytics", details = ex.Message });
        }
    }

    /// <summary>
    /// Record a processing event for analytics tracking
    /// </summary>
    /// <param name="processingEvent">Processing event details</param>
    /// <returns>Success confirmation</returns>
    [HttpPost("events")]
    public async Task<IActionResult> RecordProcessingEventAsync([FromBody] ProcessingEvent processingEvent)
    {
        try
        {
            var userId = GetCurrentUserId();
            processingEvent.UserId = userId;
            
            _logger.LogInformation("📝 Recording processing event from user {UserId}: {EventType}", 
                userId, processingEvent.EventType);

            await _analyticsService.RecordProcessingEventAsync(processingEvent);

            _logger.LogInformation("✅ Processing event recorded: {EventId}", processingEvent.Id);

            return Ok(new { message = "Processing event recorded successfully", eventId = processingEvent.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to record processing event");
            return StatusCode(500, new { error = "Failed to record processing event", details = ex.Message });
        }
    }

    /// <summary>
    /// Get analytics service capabilities and configuration
    /// </summary>
    /// <returns>Analytics service capabilities</returns>
    [HttpGet("capabilities")]
    public ActionResult<object> GetAnalyticsCapabilities()
    {
        try
        {
            var capabilities = new
            {
                Version = "3.0",
                AnalyticsTypes = new[]
                {
                    "Summary Analytics",
                    "Document Processing Metrics",
                    "Performance Analytics",
                    "Real-time Metrics",
                    "Trend Analysis",
                    "Predictive Analytics",
                    "Historical Analysis"
                },
                TrendTypes = Enum.GetNames(typeof(TrendType)),
                ExportFormats = new[]
                {
                    "Excel", "CSV", "JSON", "PDF"
                },
                Features = new[]
                {
                    "Comprehensive Analytics Dashboard",
                    "Custom KPI Creation and Tracking",
                    "Real-time Metrics and Monitoring",
                    "Trend Analysis and Forecasting", 
                    "Predictive Analytics and Insights",
                    "Historical Data Analysis",
                    "Performance Benchmarking",
                    "Export and Reporting Capabilities",
                    "Processing Event Tracking"
                },
                MetricCategories = new[]
                {
                    "Volume Metrics",
                    "Performance Metrics",
                    "Quality Metrics",
                    "Resource Utilization",
                    "Error Analysis",
                    "User Activity",
                    "System Health"
                },
                RealtimeCapabilities = new
                {
                    ActiveJobMonitoring = true,
                    SystemHealthTracking = true,
                    PerformanceMetrics = true,
                    ErrorRateMonitoring = true,
                    ThroughputTracking = true,
                    ResourceUtilization = true
                },
                Limits = new
                {
                    MaxAnalysisPeriod = "365 days",
                    MaxCustomKpis = 50,
                    MaxExportRecords = 100000,
                    RealtimeUpdateInterval = "30 seconds"
                }
            };

            return Ok(capabilities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to get analytics capabilities");
            return StatusCode(500, new { error = "Failed to get analytics capabilities", details = ex.Message });
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