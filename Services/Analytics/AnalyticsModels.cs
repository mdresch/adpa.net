namespace ADPA.Services.Analytics;

/// <summary>
/// Comprehensive analytics summary for document processing
/// </summary>
public class AnalyticsSummary
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime GeneratedAt { get; set; }
    
    // Document Volume Metrics
    public int TotalDocuments { get; set; }
    public int ProcessedDocuments { get; set; }
    public int FailedDocuments { get; set; }
    public int PendingDocuments { get; set; }
    public int ProcessingDocuments { get; set; }
    
    // Processing Performance
    public double AverageProcessingTime { get; set; }
    public long TotalProcessingTime { get; set; }
    public double ProcessingSuccessRate { get; set; }
    public double SuccessRate { get; set; } // Alias for ProcessingSuccessRate
    public double ErrorRate { get; set; } // Calculated from failed documents
    
    // Distribution Analysis
    public Dictionary<string, int> FileTypeDistribution { get; set; } = new();
    public Dictionary<DateTime, int> DailyProcessingVolume { get; set; } = new();
    public Dictionary<string, int> ErrorDistribution { get; set; } = new();
    public Dictionary<string, int> LanguageDistribution { get; set; } = new();
    
    // Quality Metrics
    public double AverageConfidenceScore { get; set; }
    
    // Size Analysis
    public long TotalDataProcessed { get; set; }
    public long AverageFileSize { get; set; }
    public long LargestFile { get; set; }
    public long SmallestFile { get; set; }
}

/// <summary>
/// Document processing metrics and KPIs
/// </summary>
public class DocumentProcessingMetrics
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime GeneratedAt { get; set; }
    
    // Volume Metrics
    public int TotalDocumentsProcessed { get; set; }
    public double DocumentsPerHour { get; set; }
    public double DocumentsPerDay { get; set; }
    public string PeakProcessingHour { get; set; } = string.Empty;
    
    // Status Distribution
    public Dictionary<string, int> StatusDistribution { get; set; } = new();
    
    // Processing Efficiency
    public double AverageProcessingLatency { get; set; }
    public double ProcessingThroughput { get; set; }
    
    // Quality Metrics
    public double ProcessingAccuracy { get; set; }
    public double RetryRate { get; set; }
    
    // Resource Utilization
    public double AverageMemoryUsage { get; set; }
    public double AverageCpuUsage { get; set; }
    
    // Trend Analysis
    public double VolumeGrowthRate { get; set; }
    public string PerformanceTrend { get; set; } = string.Empty;
}

/// <summary>
/// Performance analytics and system metrics
/// </summary>
public class PerformanceAnalytics
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime GeneratedAt { get; set; }
    
    // Response Time Analytics
    public double AverageResponseTime { get; set; }
    public double MedianResponseTime { get; set; }
    public double P95ResponseTime { get; set; }
    public double P99ResponseTime { get; set; }
    
    // Throughput Analytics
    public long TotalProcessingTime { get; set; }
    public double ProcessingOperationsPerSecond { get; set; }
    public double RequestsPerSecond { get; set; } // Alias for ProcessingOperationsPerSecond
    public double DataThroughputMBPerSecond { get; set; }
    
    // Error Rate Analytics
    public int TotalErrors { get; set; }
    public double ErrorRate { get; set; }
    public Dictionary<string, int> ErrorsByType { get; set; } = new();
    
    // Performance by Processing Type
    public Dictionary<string, ProcessingTypePerformance> PerformanceByType { get; set; } = new();
    
    // Resource Utilization
    public Dictionary<string, object> SystemResourceUsage { get; set; } = new();
    
    // Scalability Metrics
    public int ConcurrentProcessingCapacity { get; set; }
    public Dictionary<string, int> LoadDistribution { get; set; } = new();
}

/// <summary>
/// Performance metrics for specific processing types
/// </summary>
public class ProcessingTypePerformance
{
    public string ProcessingType { get; set; } = string.Empty;
    public int Count { get; set; }
    public double AverageTime { get; set; }
    public double SuccessRate { get; set; }
}

/// <summary>
/// Real-time system metrics and current status
/// </summary>
public class RealTimeMetrics
{
    public DateTime Timestamp { get; set; }
    
    // Current Processing Status
    public int ActiveProcessingJobs { get; set; }
    public int QueuedDocuments { get; set; }
    public int ProcessingThroughputLastHour { get; set; }
    
    // System Health
    public string SystemHealth { get; set; } = string.Empty;
    public double CpuUsagePercent { get; set; }
    public double MemoryUsagePercent { get; set; }
    public double AvailableDiskSpaceGB { get; set; }
    
    // Performance Indicators
    public double AverageResponseTimeLast5Min { get; set; }
    public double ErrorRateLast5Min { get; set; }
    
    // Active Users and Sessions
    public int ActiveUsers { get; set; }
    public int ActiveSessions { get; set; }
    
    // Recent Activity Summary
    public Dictionary<string, int> RecentActivitySummary { get; set; } = new();
}

/// <summary>
/// Processing event for analytics tracking
/// </summary>
public class ProcessingEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; } = string.Empty;
    public Guid? DocumentId { get; set; }
    public Guid? UserId { get; set; }
    public int? ProcessingTimeMs { get; set; }
    public bool IsError { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Trend analysis for historical data
/// </summary>
public class TrendAnalysis
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime GeneratedAt { get; set; }
    public TrendType TrendType { get; set; }
    public string TrendDirection { get; set; } = string.Empty;
    public double TrendStrength { get; set; }
    public Dictionary<DateTime, double> DataPoints { get; set; } = new();
    public string Analysis { get; set; } = string.Empty;
    public List<string> Insights { get; set; } = new();
}

/// <summary>
/// Types of trends that can be analyzed
/// </summary>
public enum TrendType
{
    DocumentVolume,
    ProcessingTime,
    ErrorRate,
    UserActivity,
    SystemPerformance,
    FileTypeDistribution,
    LanguageDistribution
}

/// <summary>
/// Predictive analytics based on historical data
/// </summary>
public class PredictiveAnalytics
{
    public DateTime GeneratedAt { get; set; }
    public TimeSpan PredictionHorizon { get; set; }
    public Dictionary<string, double> Predictions { get; set; } = new();
    public double ConfidenceLevel { get; set; }
    public string Model { get; set; } = string.Empty;
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// Custom KPI definition
/// </summary>
public class CustomKpiDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Formula { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Custom KPI instance
/// </summary>
public class CustomKpi
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double CurrentValue { get; set; }
    public double? TargetValue { get; set; }
    public string Trend { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdated { get; set; }
    public Guid? UserId { get; set; }
}

/// <summary>
/// Historical analysis for long-term trends
/// </summary>
public class HistoricalAnalysis
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime GeneratedAt { get; set; }
    public Dictionary<string, TrendAnalysis> TrendAnalyses { get; set; } = new();
    public List<string> KeyInsights { get; set; } = new();
    public Dictionary<string, double> ComparisonMetrics { get; set; } = new();
}

/// <summary>
/// Analytics export request
/// </summary>
public class AnalyticsExportRequest
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<string> MetricsToInclude { get; set; } = new();
    public string ExportFormat { get; set; } = "Excel";
    public bool IncludeCharts { get; set; } = true;
    public Guid? UserId { get; set; }
}

/// <summary>
/// Analytics report request
/// </summary>
public class AnalyticsReportRequest
{
    public string ReportType { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
    public Guid? UserId { get; set; }
}

/// <summary>
/// Generated analytics report
/// </summary>
public class AnalyticsReport
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string ExecutiveSummary { get; set; } = string.Empty;
    public List<ReportSection> Sections { get; set; } = new();
    public Dictionary<string, object> Charts { get; set; } = new();
    public List<string> KeyFindings { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// Section within an analytics report
/// </summary>
public class ReportSection
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<object> Data { get; set; } = new();
    public string ChartType { get; set; } = string.Empty;
}

/// <summary>
/// Analytics dashboard widget configuration
/// </summary>
public class DashboardWidget
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string WidgetType { get; set; } = string.Empty;
    public Dictionary<string, object> Configuration { get; set; } = new();
    public int Position { get; set; }
    public string Size { get; set; } = "medium";
    public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromMinutes(5);
}

/// <summary>
/// Analytics dashboard configuration
/// </summary>
public class AnalyticsDashboard
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<DashboardWidget> Widgets { get; set; } = new();
    public Guid? UserId { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModified { get; set; }
}