using ADPA.Models.DTOs;
using ADPA.Models.Entities;
using ADPA.Data.Repositories;
using System.Collections.Concurrent;

namespace ADPA.Services.Analytics;

/// <summary>
/// Phase 3: Analytics Engine Service
/// Provides comprehensive analytics, metrics, and KPI calculations for document processing
/// </summary>
public interface IAnalyticsService
{
    // Core Analytics
    Task<AnalyticsSummary> GetAnalyticsSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<DocumentProcessingMetrics> GetDocumentProcessingMetricsAsync(TimeSpan period);
    Task<PerformanceAnalytics> GetPerformanceAnalyticsAsync(TimeSpan period);
    Task<TrendAnalysis> GetTrendAnalysisAsync(TimeSpan period, TrendType trendType);
    
    // Predictive Analytics
    Task<PredictiveAnalytics> GetPredictiveAnalyticsAsync();
    Task<List<CustomKpi>> GetCustomKpisAsync(Guid? userId = null);
    Task<CustomKpi> CreateCustomKpiAsync(CustomKpiDefinition definition);
    
    // Real-time Metrics
    Task<RealTimeMetrics> GetRealTimeMetricsAsync();
    Task RecordProcessingEventAsync(ProcessingEvent processingEvent);
    
    // Historical Analysis
    Task<HistoricalAnalysis> GetHistoricalAnalysisAsync(DateTime fromDate, DateTime toDate);
    Task<List<ProcessingEvent>> GetProcessingEventsAsync(DateTime fromDate, DateTime toDate, string? eventType = null);
    
    // Export and Reporting
    Task<byte[]> ExportAnalyticsToExcelAsync(AnalyticsExportRequest request);
    Task<AnalyticsReport> GenerateAnalyticsReportAsync(AnalyticsReportRequest request);
}

public class AnalyticsService : IAnalyticsService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly EfProcessingResultRepository _processingResultRepository;
    private readonly ILogger<AnalyticsService> _logger;
    
    // In-memory cache for real-time metrics (would use Redis in production)
    private readonly ConcurrentDictionary<string, object> _metricsCache = new();
    private readonly ConcurrentQueue<ProcessingEvent> _recentEvents = new();
    private readonly Timer _metricsTimer;

    public AnalyticsService(
        IDocumentRepository documentRepository,
        EfProcessingResultRepository processingResultRepository,
        ILogger<AnalyticsService> logger)
    {
        _documentRepository = documentRepository;
        _processingResultRepository = processingResultRepository;
        _logger = logger;
        
        // Initialize metrics collection timer (updates every 30 seconds)
        _metricsTimer = new Timer(UpdateRealTimeMetrics, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        
        _logger.LogInformation("🔧 Analytics Service initialized with real-time metrics collection");
    }

    public async Task<AnalyticsSummary> GetAnalyticsSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            _logger.LogInformation("📊 Generating analytics summary for period {FromDate} to {ToDate}", 
                fromDate?.ToString("yyyy-MM-dd") ?? "all-time", 
                toDate?.ToString("yyyy-MM-dd") ?? "now");

            var startDate = fromDate ?? DateTime.UtcNow.AddDays(-30);
            var endDate = toDate ?? DateTime.UtcNow;

            // Get all documents in the period
            var allDocuments = await _documentRepository.GetAllAsync();
            var filteredDocuments = allDocuments
                .Where(d => d.CreatedAt >= startDate && d.CreatedAt <= endDate)
                .ToList();

            // Get processing results
            var allProcessingResults = await _processingResultRepository.GetAllAsync();
            var filteredResults = allProcessingResults
                .Where(pr => pr.CreatedAt >= startDate && pr.CreatedAt <= endDate)
                .ToList();

            // Calculate summary metrics
            var summary = new AnalyticsSummary
            {
                PeriodStart = startDate,
                PeriodEnd = endDate,
                GeneratedAt = DateTime.UtcNow,
                
                // Document Metrics
                TotalDocuments = filteredDocuments.Count,
                ProcessedDocuments = filteredDocuments.Count(d => d.Status == ProcessingStatus.Completed),
                FailedDocuments = filteredDocuments.Count(d => d.Status == ProcessingStatus.Failed),
                PendingDocuments = filteredDocuments.Count(d => d.Status == ProcessingStatus.Pending),
                ProcessingDocuments = filteredDocuments.Count(d => d.Status == ProcessingStatus.Processing),
                
                // Processing Performance
                AverageProcessingTime = CalculateAverageProcessingTime(filteredResults),
                TotalProcessingTime = filteredResults.Sum(pr => pr.ProcessingTimeMs),
                ProcessingSuccessRate = filteredDocuments.Count > 0 
                    ? (double)filteredDocuments.Count(d => d.Status == ProcessingStatus.Completed) / filteredDocuments.Count * 100
                    : 0,
                
                // File Type Distribution
                FileTypeDistribution = filteredDocuments
                    .GroupBy(d => d.ContentType)
                    .ToDictionary(g => g.Key, g => g.Count()),
                
                // Daily Processing Volume
                DailyProcessingVolume = filteredDocuments
                    .GroupBy(d => d.CreatedAt.Date)
                    .ToDictionary(g => g.Key, g => g.Count()),
                
                // Quality Metrics
                AverageConfidenceScore = filteredResults.Any(pr => pr.ConfidenceScore.HasValue)
                    ? filteredResults.Where(pr => pr.ConfidenceScore.HasValue).Average(pr => pr.ConfidenceScore!.Value)
                    : 0,
                
                // Error Analysis
                ErrorDistribution = filteredResults
                    .Where(pr => !string.IsNullOrEmpty(pr.ErrorMessage))
                    .GroupBy(pr => pr.ErrorMessage!)
                    .ToDictionary(g => g.Key, g => g.Count()),
                
                // Language Detection
                LanguageDistribution = filteredDocuments
                    .Where(d => !string.IsNullOrEmpty(d.DetectedLanguage))
                    .GroupBy(d => d.DetectedLanguage!)
                    .ToDictionary(g => g.Key, g => g.Count()),
                
                // Size Analysis
                TotalDataProcessed = filteredDocuments.Sum(d => d.FileSize),
                AverageFileSize = filteredDocuments.Any() ? (long)filteredDocuments.Average(d => d.FileSize) : 0,
                LargestFile = filteredDocuments.Any() ? filteredDocuments.Max(d => d.FileSize) : 0,
                SmallestFile = filteredDocuments.Any() ? filteredDocuments.Min(d => d.FileSize) : 0
            };

            _logger.LogInformation("✅ Analytics summary generated: {TotalDocuments} documents, {ProcessingRate}% success rate", 
                summary.TotalDocuments, Math.Round(summary.ProcessingSuccessRate, 2));

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to generate analytics summary");
            throw;
        }
    }

    public async Task<DocumentProcessingMetrics> GetDocumentProcessingMetricsAsync(TimeSpan period)
    {
        try
        {
            _logger.LogInformation("📈 Calculating document processing metrics for period {Period}", period);

            var startDate = DateTime.UtcNow.Subtract(period);
            var endDate = DateTime.UtcNow;

            var allDocuments = await _documentRepository.GetAllAsync();
            var filteredDocuments = allDocuments
                .Where(d => d.CreatedAt >= startDate && d.CreatedAt <= endDate)
                .ToList();

            var metrics = new DocumentProcessingMetrics
            {
                PeriodStart = startDate,
                PeriodEnd = endDate,
                GeneratedAt = DateTime.UtcNow,
                
                // Volume Metrics
                TotalDocumentsProcessed = filteredDocuments.Count,
                DocumentsPerHour = CalculateDocumentsPerHour(filteredDocuments, period),
                DocumentsPerDay = CalculateDocumentsPerDay(filteredDocuments, period),
                PeakProcessingHour = CalculatePeakProcessingHour(filteredDocuments),
                
                // Status Distribution
                StatusDistribution = filteredDocuments
                    .GroupBy(d => d.Status)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                
                // Processing Efficiency
                AverageProcessingLatency = CalculateAverageLatency(filteredDocuments),
                ProcessingThroughput = CalculateThroughput(filteredDocuments, period),
                
                // Quality Metrics
                ProcessingAccuracy = CalculateProcessingAccuracy(filteredDocuments),
                RetryRate = CalculateRetryRate(filteredDocuments),
                
                // Resource Utilization
                AverageMemoryUsage = GetAverageMemoryUsage(period),
                AverageCpuUsage = GetAverageCpuUsage(period),
                
                // Trend Analysis
                VolumeGrowthRate = CalculateVolumeGrowthRate(filteredDocuments, period),
                PerformanceTrend = CalculatePerformanceTrend(filteredDocuments)
            };

            _logger.LogInformation("✅ Document processing metrics calculated: {TotalDocuments} docs, {DocsPerHour} docs/hour", 
                metrics.TotalDocumentsProcessed, Math.Round(metrics.DocumentsPerHour, 2));

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to calculate document processing metrics");
            throw;
        }
    }

    public async Task<PerformanceAnalytics> GetPerformanceAnalyticsAsync(TimeSpan period)
    {
        try
        {
            _logger.LogInformation("⚡ Generating performance analytics for period {Period}", period);

            var startDate = DateTime.UtcNow.Subtract(period);
            var endDate = DateTime.UtcNow;

            var allResults = await _processingResultRepository.GetAllAsync();
            var filteredResults = allResults
                .Where(pr => pr.CreatedAt >= startDate && pr.CreatedAt <= endDate)
                .ToList();

            var analytics = new PerformanceAnalytics
            {
                PeriodStart = startDate,
                PeriodEnd = endDate,
                GeneratedAt = DateTime.UtcNow,
                
                // Response Time Analytics
                AverageResponseTime = filteredResults.Any() ? filteredResults.Average(pr => pr.ProcessingTimeMs) : 0,
                MedianResponseTime = CalculateMedian(filteredResults.Select(pr => pr.ProcessingTimeMs)),
                P95ResponseTime = CalculatePercentile(filteredResults.Select(pr => pr.ProcessingTimeMs), 95),
                P99ResponseTime = CalculatePercentile(filteredResults.Select(pr => pr.ProcessingTimeMs), 99),
                
                // Throughput Analytics
                TotalProcessingTime = filteredResults.Sum(pr => pr.ProcessingTimeMs),
                ProcessingOperationsPerSecond = CalculateOperationsPerSecond(filteredResults, period),
                DataThroughputMBPerSecond = CalculateDataThroughput(filteredResults, period),
                
                // Error Rate Analytics
                TotalErrors = filteredResults.Count(pr => !string.IsNullOrEmpty(pr.ErrorMessage)),
                ErrorRate = filteredResults.Count > 0 
                    ? (double)filteredResults.Count(pr => !string.IsNullOrEmpty(pr.ErrorMessage)) / filteredResults.Count * 100
                    : 0,
                ErrorsByType = filteredResults
                    .Where(pr => !string.IsNullOrEmpty(pr.ErrorMessage))
                    .GroupBy(pr => pr.ProcessingType)
                    .ToDictionary(g => g.Key, g => g.Count()),
                
                // Performance by Processing Type
                PerformanceByType = filteredResults
                    .GroupBy(pr => pr.ProcessingType)
                    .ToDictionary(g => g.Key, g => new ProcessingTypePerformance
                    {
                        ProcessingType = g.Key,
                        Count = g.Count(),
                        AverageTime = g.Average(pr => pr.ProcessingTimeMs),
                        SuccessRate = (double)g.Count(pr => string.IsNullOrEmpty(pr.ErrorMessage)) / g.Count() * 100
                    }),
                
                // Resource Utilization
                SystemResourceUsage = GetSystemResourceUsage(period),
                
                // Scalability Metrics
                ConcurrentProcessingCapacity = GetConcurrentProcessingCapacity(),
                LoadDistribution = GetLoadDistribution(filteredResults)
            };

            _logger.LogInformation("✅ Performance analytics generated: {AvgResponseTime}ms avg response, {ErrorRate}% error rate", 
                Math.Round(analytics.AverageResponseTime, 2), Math.Round(analytics.ErrorRate, 2));

            return analytics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to generate performance analytics");
            throw;
        }
    }

    public async Task<RealTimeMetrics> GetRealTimeMetricsAsync()
    {
        try
        {
            var now = DateTime.UtcNow;
            var oneHourAgo = now.AddHours(-1);

            // Get recent processing events
            var recentEvents = _recentEvents.ToArray()
                .Where(e => e.Timestamp >= oneHourAgo)
                .ToList();

            // Get current system metrics
            var currentMetrics = new RealTimeMetrics
            {
                Timestamp = now,
                
                // Current Processing Status
                ActiveProcessingJobs = GetActiveProcessingJobs(),
                QueuedDocuments = await GetQueuedDocumentsCount(),
                ProcessingThroughputLastHour = recentEvents.Count,
                
                // System Health
                SystemHealth = GetSystemHealthStatus(),
                CpuUsagePercent = GetCurrentCpuUsage(),
                MemoryUsagePercent = GetCurrentMemoryUsage(),
                AvailableDiskSpaceGB = GetAvailableDiskSpace(),
                
                // Performance Indicators
                AverageResponseTimeLast5Min = CalculateRecentAverageResponseTime(recentEvents, TimeSpan.FromMinutes(5)),
                ErrorRateLast5Min = CalculateRecentErrorRate(recentEvents, TimeSpan.FromMinutes(5)),
                
                // Active Users and Sessions
                ActiveUsers = GetActiveUsersCount(),
                ActiveSessions = GetActiveSessionsCount(),
                
                // Recent Activity Summary
                RecentActivitySummary = recentEvents
                    .GroupBy(e => e.EventType)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return currentMetrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to get real-time metrics");
            throw;
        }
    }

    public async Task RecordProcessingEventAsync(ProcessingEvent processingEvent)
    {
        try
        {
            processingEvent.Timestamp = DateTime.UtcNow;
            _recentEvents.Enqueue(processingEvent);
            
            // Keep only last 10,000 events to prevent memory issues
            while (_recentEvents.Count > 10000)
            {
                _recentEvents.TryDequeue(out _);
            }
            
            // Update real-time metrics cache
            UpdateMetricsCache(processingEvent);
            
            await Task.CompletedTask; // For future database persistence
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to record processing event");
        }
    }

    // Helper methods for calculations
    private double CalculateAverageProcessingTime(List<ProcessingResult> results)
    {
        return results.Any() ? results.Average(pr => pr.ProcessingTimeMs) : 0;
    }

    private double CalculateDocumentsPerHour(List<Document> documents, TimeSpan period)
    {
        if (!documents.Any()) return 0;
        var hours = period.TotalHours;
        return hours > 0 ? documents.Count / hours : documents.Count;
    }

    private double CalculateDocumentsPerDay(List<Document> documents, TimeSpan period)
    {
        if (!documents.Any()) return 0;
        var days = period.TotalDays;
        return days > 0 ? documents.Count / days : documents.Count;
    }

    private string CalculatePeakProcessingHour(List<Document> documents)
    {
        if (!documents.Any()) return "N/A";
        
        var hourlyDistribution = documents
            .GroupBy(d => d.CreatedAt.Hour)
            .ToDictionary(g => g.Key, g => g.Count());
        
        var peakHour = hourlyDistribution.OrderByDescending(kvp => kvp.Value).FirstOrDefault();
        return $"{peakHour.Key:D2}:00 ({peakHour.Value} documents)";
    }

    private double CalculateMedian(IEnumerable<int> values)
    {
        var sortedValues = values.OrderBy(v => v).ToList();
        if (!sortedValues.Any()) return 0;
        
        var count = sortedValues.Count;
        if (count % 2 == 0)
        {
            return (sortedValues[count / 2 - 1] + sortedValues[count / 2]) / 2.0;
        }
        return sortedValues[count / 2];
    }

    private double CalculatePercentile(IEnumerable<int> values, int percentile)
    {
        var sortedValues = values.OrderBy(v => v).ToList();
        if (!sortedValues.Any()) return 0;
        
        var index = (percentile / 100.0) * (sortedValues.Count - 1);
        var lower = (int)Math.Floor(index);
        var upper = (int)Math.Ceiling(index);
        
        if (lower == upper) return sortedValues[lower];
        
        var weight = index - lower;
        return sortedValues[lower] * (1 - weight) + sortedValues[upper] * weight;
    }

    // System metrics methods (would integrate with actual system monitoring in production)
    private double GetCurrentCpuUsage() => Random.Shared.NextDouble() * 100; // Mock implementation
    private double GetCurrentMemoryUsage() => Random.Shared.NextDouble() * 100; // Mock implementation
    private double GetAvailableDiskSpace() => Random.Shared.NextDouble() * 1000; // Mock implementation
    private string GetSystemHealthStatus() => "Healthy"; // Mock implementation
    private int GetActiveProcessingJobs() => Random.Shared.Next(0, 10); // Mock implementation
    private async Task<int> GetQueuedDocumentsCount()
    {
        var documents = await _documentRepository.GetAllAsync();
        return documents.Count(d => d.Status == ProcessingStatus.Pending);
    }
    private int GetActiveUsersCount() => Random.Shared.Next(1, 50); // Mock implementation
    private int GetActiveSessionsCount() => Random.Shared.Next(1, 100); // Mock implementation

    // Additional helper methods would be implemented here for full functionality
    private double CalculateAverageLatency(List<Document> documents) => 0; // Placeholder
    private double CalculateThroughput(List<Document> documents, TimeSpan period) => 0; // Placeholder
    private double CalculateProcessingAccuracy(List<Document> documents) => 0; // Placeholder
    private double CalculateRetryRate(List<Document> documents) => 0; // Placeholder
    private double GetAverageMemoryUsage(TimeSpan period) => 0; // Placeholder
    private double GetAverageCpuUsage(TimeSpan period) => 0; // Placeholder
    private double CalculateVolumeGrowthRate(List<Document> documents, TimeSpan period) => 0; // Placeholder
    private string CalculatePerformanceTrend(List<Document> documents) => "Stable"; // Placeholder
    
    private double CalculateOperationsPerSecond(List<ProcessingResult> results, TimeSpan period)
    {
        return results.Count / period.TotalSeconds;
    }
    
    private double CalculateDataThroughput(List<ProcessingResult> results, TimeSpan period) => 0; // Placeholder
    private Dictionary<string, object> GetSystemResourceUsage(TimeSpan period) => new(); // Placeholder
    private int GetConcurrentProcessingCapacity() => 10; // Placeholder
    private Dictionary<string, int> GetLoadDistribution(List<ProcessingResult> results) => new(); // Placeholder
    
    private double CalculateRecentAverageResponseTime(List<ProcessingEvent> events, TimeSpan timeFrame)
    {
        var recentEvents = events.Where(e => e.Timestamp >= DateTime.UtcNow.Subtract(timeFrame));
        return recentEvents.Any() ? recentEvents.Average(e => e.ProcessingTimeMs ?? 0) : 0;
    }
    
    private double CalculateRecentErrorRate(List<ProcessingEvent> events, TimeSpan timeFrame)
    {
        var recentEvents = events.Where(e => e.Timestamp >= DateTime.UtcNow.Subtract(timeFrame)).ToList();
        if (!recentEvents.Any()) return 0;
        return (double)recentEvents.Count(e => e.IsError) / recentEvents.Count * 100;
    }
    
    private void UpdateMetricsCache(ProcessingEvent processingEvent)
    {
        // Update cached metrics based on the new event
        // This would be implemented based on specific caching strategy
    }
    
    private void UpdateRealTimeMetrics(object? state)
    {
        try
        {
            // Periodic update of real-time metrics
            // This would collect system metrics and update cache
            _logger.LogDebug("📊 Updating real-time metrics cache");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to update real-time metrics");
        }
    }

    // Placeholder implementations for remaining interface methods
    public Task<TrendAnalysis> GetTrendAnalysisAsync(TimeSpan period, TrendType trendType)
    {
        // Implementation would analyze trends based on historical data
        return Task.FromResult(new TrendAnalysis
        {
            PeriodStart = DateTime.UtcNow.Subtract(period),
            PeriodEnd = DateTime.UtcNow,
            TrendType = trendType,
            TrendDirection = "Upward",
            TrendStrength = 0.75,
            DataPoints = new Dictionary<DateTime, double>()
        });
    }

    public Task<PredictiveAnalytics> GetPredictiveAnalyticsAsync()
    {
        // Implementation would use ML models for predictions
        return Task.FromResult(new PredictiveAnalytics
        {
            GeneratedAt = DateTime.UtcNow,
            PredictionHorizon = TimeSpan.FromDays(30),
            Predictions = new Dictionary<string, double>()
        });
    }

    public Task<List<CustomKpi>> GetCustomKpisAsync(Guid? userId = null)
    {
        // Implementation would retrieve custom KPIs from database
        return Task.FromResult(new List<CustomKpi>());
    }

    public Task<CustomKpi> CreateCustomKpiAsync(CustomKpiDefinition definition)
    {
        // Implementation would create and store custom KPI
        return Task.FromResult(new CustomKpi
        {
            Id = Guid.NewGuid(),
            Name = definition.Name,
            CreatedAt = DateTime.UtcNow
        });
    }

    public Task<HistoricalAnalysis> GetHistoricalAnalysisAsync(DateTime fromDate, DateTime toDate)
    {
        // Implementation would analyze historical trends
        return Task.FromResult(new HistoricalAnalysis
        {
            PeriodStart = fromDate,
            PeriodEnd = toDate,
            GeneratedAt = DateTime.UtcNow
        });
    }

    public Task<List<ProcessingEvent>> GetProcessingEventsAsync(DateTime fromDate, DateTime toDate, string? eventType = null)
    {
        // Implementation would retrieve events from storage
        var events = _recentEvents.ToArray()
            .Where(e => e.Timestamp >= fromDate && e.Timestamp <= toDate)
            .Where(e => eventType == null || e.EventType == eventType)
            .ToList();
        
        return Task.FromResult(events);
    }

    public Task<byte[]> ExportAnalyticsToExcelAsync(AnalyticsExportRequest request)
    {
        // Implementation would generate Excel file with analytics data
        return Task.FromResult(new byte[0]); // Placeholder
    }

    public Task<AnalyticsReport> GenerateAnalyticsReportAsync(AnalyticsReportRequest request)
    {
        // Implementation would generate comprehensive analytics report
        return Task.FromResult(new AnalyticsReport
        {
            Id = Guid.NewGuid(),
            GeneratedAt = DateTime.UtcNow,
            ReportType = request.ReportType,
            Title = $"Analytics Report - {request.ReportType}"
        });
    }

    public void Dispose()
    {
        _metricsTimer?.Dispose();
    }
}