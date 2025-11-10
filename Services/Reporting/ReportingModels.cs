namespace ADPA.Services.Reporting;

/// <summary>
/// Report types enumeration for standardized report categorization
/// </summary>
public enum ReportType
{
    Executive,
    Operational,
    Performance,
    Security,
    Financial,
    Technical,
    Compliance,
    Analytics,
    Custom
}

/// <summary>
/// Report format enumeration for export options
/// </summary>
public enum ReportFormat
{
    PDF,
    Excel,
    CSV,
    HTML,
    JSON
}

/// <summary>
/// Report template definition for standardized report generation
/// </summary>
public class ReportTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;
    public string Layout { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public Dictionary<string, object> Configuration { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsShared { get; set; } = false;
}

/// <summary>
/// Definition for creating a new report template
/// </summary>
public class ReportTemplateDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public Guid? CreatedBy { get; set; }
}

/// <summary>
/// Scheduled report configuration
/// </summary>
public class ScheduledReport
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string Schedule { get; set; } = string.Empty; // Cron expression or simple schedule
    public string CronExpression { get; set; } = string.Empty; // Alias for Schedule
    public List<string> Recipients { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid UserId { get; set; } // Alias for CreatedBy
    public bool IsActive { get; set; } = true;
    public DateTime? NextRunTime { get; set; }
    public DateTime? LastRunTime { get; set; }
    public DateTime? LastRunDate { get; set; } // Alias for LastRunTime
    public int RunCount { get; set; }
    public string? LastRunStatus { get; set; }
    public string Status { get; set; } = "Active";
}

/// <summary>
/// Definition for creating a scheduled report
/// </summary>
public class ScheduledReportDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string Schedule { get; set; } = string.Empty;
    public List<string> Recipients { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
    public Guid CreatedBy { get; set; }
}

/// <summary>
/// Report generation history record
/// </summary>
public class ReportHistory
{
    public Guid Id { get; set; }
    public Guid ReportId { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public DateTime GeneratedDate { get; set; } // Alias for GeneratedAt
    public Guid? UserId { get; set; }
    public long FileSize { get; set; }
    public long FileSizeBytes { get; set; } // Alias for FileSize
    public string Status { get; set; } = "Generated";
    public string? FilePath { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Report distribution record
/// </summary>
public class ReportDistribution
{
    public Guid Id { get; set; }
    public Guid ReportId { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public string DistributionType { get; set; } = string.Empty; // Email, Share, Download
    public string DeliveryMethod { get; set; } = string.Empty; // Alias for DistributionType
    public List<string> Recipients { get; set; } = new();
    public string Recipient { get; set; } = string.Empty; // Single recipient
    public DateTime DistributedAt { get; set; }
    public DateTime DeliveredAt { get; set; } // Alias for DistributedAt
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Report sharing configuration
/// </summary>
public class ReportShare
{
    public Guid Id { get; set; }
    public Guid ReportId { get; set; }
    public Guid SharedBy { get; set; }
    public List<Guid> SharedWith { get; set; } = new();
    public List<Guid> SharedWithUserIds { get; set; } = new(); // Alias for SharedWith
    public DateTime SharedAt { get; set; }
    public DateTime SharedDate { get; set; } // Alias for SharedAt
    public DateTime? ExpiresAt { get; set; }
    public string AccessLevel { get; set; } = "Read"; // Read, Download, Edit
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Report export configuration
/// </summary>
public class ReportExportConfiguration
{
    public string Format { get; set; } = "PDF"; // PDF, Excel, HTML, JSON
    public bool IncludeCharts { get; set; } = true;
    public bool IncludeRawData { get; set; } = false;
    public string Orientation { get; set; } = "Portrait"; // Portrait, Landscape
    public string PaperSize { get; set; } = "A4";
    public Dictionary<string, object> CustomOptions { get; set; } = new();
}

/// <summary>
/// Report subscription for automated delivery
/// </summary>
public class ReportSubscription
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty; // Daily, Weekly, Monthly
    public List<string> DeliveryMethods { get; set; } = new(); // Email, Dashboard, SMS
    public Dictionary<string, object> Filters { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastDelivered { get; set; }
}

/// <summary>
/// Report dashboard configuration
/// </summary>
public class ReportDashboard
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<DashboardReportWidget> Widgets { get; set; } = new();
    public Guid? UserId { get; set; }
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModified { get; set; }
}

/// <summary>
/// Dashboard widget for displaying reports
/// </summary>
public class DashboardReportWidget
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string VisualizationType { get; set; } = string.Empty; // Chart, Table, KPI, Gauge
    public Dictionary<string, object> Configuration { get; set; } = new();
    public int Position { get; set; }
    public string Size { get; set; } = "medium"; // small, medium, large
    public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromMinutes(15);
}

/// <summary>
/// Report approval workflow
/// </summary>
public class ReportApproval
{
    public Guid Id { get; set; }
    public Guid ReportId { get; set; }
    public Guid RequestedBy { get; set; }
    public List<Guid> Approvers { get; set; } = new();
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
    public DateTime RequestedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? Comments { get; set; }
    public Dictionary<string, object> ApprovalMetadata { get; set; } = new();
}

/// <summary>
/// Report audit log entry
/// </summary>
public class ReportAuditLog
{
    public Guid Id { get; set; }
    public Guid? ReportId { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty; // Generated, Downloaded, Shared, Deleted
    public string Details { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Report performance metrics
/// </summary>
public class ReportPerformanceMetrics
{
    public Guid Id { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public TimeSpan GenerationTime { get; set; }
    public long DataSize { get; set; }
    public int RecordCount { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string? PerformanceProfile { get; set; }
    public Dictionary<string, double> Benchmarks { get; set; } = new();
}

/// <summary>
/// Custom report field definition
/// </summary>
public class ReportFieldDefinition
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty; // String, Number, Date, Boolean
    public bool IsRequired { get; set; }
    public string? DefaultValue { get; set; }
    public List<string> ValidValues { get; set; } = new();
    public Dictionary<string, object> ValidationRules { get; set; } = new();
}

/// <summary>
/// Report data source configuration
/// </summary>
public class ReportDataSource
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty; // Database, API, File, Service
    public string ConnectionString { get; set; } = string.Empty;
    public Dictionary<string, object> Configuration { get; set; } = new();
    public List<ReportFieldDefinition> Fields { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Report query definition for data retrieval
/// </summary>
public class ReportQuery
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid DataSourceId { get; set; }
    public string Query { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public List<string> RequiredPermissions { get; set; } = new();
    public TimeSpan? CacheDuration { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastExecuted { get; set; }
}

/// <summary>
/// Report theme and styling configuration
/// </summary>
public class ReportTheme
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, string> Colors { get; set; } = new();
    public Dictionary<string, string> Fonts { get; set; } = new();
    public Dictionary<string, object> Styles { get; set; } = new();
    public string? LogoUrl { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Report data container for generated reports
/// </summary>
public class ReportData
{
    public Guid ReportId { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public ReportType Type { get; set; }
    public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
    public Guid GeneratedBy { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
    public List<ReportChart> Charts { get; set; } = new();
    public List<ReportTable> Tables { get; set; } = new();
    public ReportSummary Summary { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
    public string Status { get; set; } = "Generated";
    public TimeSpan GenerationTime { get; set; }
    public int PageCount { get; set; }
    public long FileSizeBytes { get; set; }
}

/// <summary>
/// Report chart configuration and data
/// </summary>
public class ReportChart
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "";
    public string Type { get; set; } = ""; // bar, line, pie, etc.
    public List<Dictionary<string, object>> Data { get; set; } = new();
    public Dictionary<string, object> Configuration { get; set; } = new();
    public string XAxisLabel { get; set; } = "";
    public string YAxisLabel { get; set; } = "";
    public int Order { get; set; }
    public bool IsVisible { get; set; } = true;
}

/// <summary>
/// Report table configuration and data
/// </summary>
public class ReportTable
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "";
    public List<string> Headers { get; set; } = new();
    public List<List<object>> Rows { get; set; } = new();
    public Dictionary<string, object> Formatting { get; set; } = new();
    public int Order { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsPaginated { get; set; }
    public int PageSize { get; set; } = 25;
}

/// <summary>
/// Report summary with key insights and metrics
/// </summary>
public class ReportSummary
{
    public List<string> KeyInsights { get; set; } = new();
    public Dictionary<string, object> KpiValues { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public string ExecutiveSummary { get; set; } = "";
    public Dictionary<string, decimal> TrendIndicators { get; set; } = new();
}

/// <summary>
/// Paged result wrapper for API responses
/// </summary>
/// <typeparam name="T">Type of items in the result</typeparam>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalItems { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}

/// <summary>
/// Report email request model
/// </summary>
public class ReportEmailRequest
{
    public ReportType ReportType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public List<string> Recipients { get; set; } = new();
    public string Subject { get; set; } = "";
    public string Body { get; set; } = "";
    public bool IncludeAttachment { get; set; } = true;
    public string AttachmentFormat { get; set; } = "PDF"; // PDF, Excel
}