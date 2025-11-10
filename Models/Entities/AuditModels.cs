using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace ADPA.Models.Entities;

/// <summary>
/// Phase 5.4: Comprehensive Audit System Models
/// Complete audit logging, compliance reporting, and data lineage tracking
/// </summary>

/// <summary>
/// Base audit log entry for all system activities
/// </summary>
public class AuditLogEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string EventType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public AuditSeverity Severity { get; set; } = AuditSeverity.Information;
    
    // User context
    public Guid? UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    
    // Request context
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    
    // Event details
    public string Action { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Result information
    public bool WasSuccessful { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    
    // Additional data (stored as JSON)
    public string Metadata { get; set; } = "{}";
    public string BeforeState { get; set; } = "{}";
    public string AfterState { get; set; } = "{}";
    
    // Risk assessment
    public AuditRiskLevel RiskLevel { get; set; } = AuditRiskLevel.Low;
    
    // Compliance fields
    public SecurityClassification? SecurityClassification { get; set; }
    public bool IsPersonalData { get; set; } = false;
    public bool IsSensitiveOperation { get; set; } = false;
    public string? ComplianceFramework { get; set; }
    public List<string> ComplianceTags { get; set; } = new();
    
    // Integrity protection
    public string IntegrityHash { get; set; } = string.Empty;
    public DateTime IntegrityTimestamp { get; set; } = DateTime.UtcNow;
    
    // Performance metrics
    public long? ExecutionTimeMs { get; set; }
    public int? ResultCount { get; set; }
    
    // Geographic/Location information
    public string? GeographicLocation { get; set; }
    public string? DataCenter { get; set; }
    
    // Helper methods for metadata
    public Dictionary<string, object> GetMetadata()
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(Metadata) ?? new();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    public void SetMetadata(Dictionary<string, object> metadata)
    {
        Metadata = JsonSerializer.Serialize(metadata);
    }

    public Dictionary<string, object> GetBeforeState()
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(BeforeState) ?? new();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    public void SetBeforeState(Dictionary<string, object> beforeState)
    {
        BeforeState = JsonSerializer.Serialize(beforeState);
    }

    public Dictionary<string, object> GetAfterState()
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(AfterState) ?? new();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    public void SetAfterState(Dictionary<string, object> afterState)
    {
        AfterState = JsonSerializer.Serialize(afterState);
    }
}

/// <summary>
/// Data lineage entry for tracking data flow and transformations (GDPR compliance)
/// </summary>
public class DataLineageEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Data identification
    public string DataType { get; set; } = string.Empty;
    public string DataId { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    public string DataDestination { get; set; } = string.Empty;
    
    // Database context fields
    public string TableName { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string Classification { get; set; } = string.Empty;
    public List<string> PersonalDataTypes { get; set; } = new();
    public List<string> ApplicableRegulations { get; set; } = new();
    
    // Operation details
    public DataOperation Operation { get; set; }
    public string OperationDescription { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    
    // Legal basis (GDPR)
    public string? LegalBasis { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public DateTime? ConsentTimestamp { get; set; }
    public string? ConsentVersion { get; set; }
    
    // Data classification
    public bool IsPersonalData { get; set; } = false;
    public bool IsSensitivePersonalData { get; set; } = false;
    public SecurityClassification SecurityClassification { get; set; } = SecurityClassification.Public;
    
    // Retention information
    public DateTime? RetentionExpiry { get; set; }
    public bool IsSubjectToRightOfErasure { get; set; } = false;
    
    // Processing location
    public string ProcessingLocation { get; set; } = string.Empty;
    public bool IsCrossBorderTransfer { get; set; } = false;
    public string? TransferMechanism { get; set; }
    
    // Technical details
    public string ProcessingMethod { get; set; } = string.Empty;
    public string? EncryptionMethod { get; set; }
    public string? DataFormat { get; set; }
    
    // Lineage chain
    public Guid? ParentLineageId { get; set; }
    public DataLineageEntry? ParentLineage { get; set; }
    public List<DataLineageEntry> ChildLineages { get; set; } = new();
    
    // Additional metadata
    public string AdditionalMetadata { get; set; } = "{}";
    
    public Dictionary<string, object> GetAdditionalMetadata()
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(AdditionalMetadata) ?? new();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    public void SetAdditionalMetadata(Dictionary<string, object> metadata)
    {
        AdditionalMetadata = JsonSerializer.Serialize(metadata);
    }
}

/// <summary>
/// Compliance audit entry for regulatory compliance tracking
/// </summary>
public class ComplianceAuditEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Service expected properties
    public string EventType { get; set; } = string.Empty;
    public string ControlId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Regulation { get; set; } = string.Empty;
    public string AssessmentResult { get; set; } = string.Empty;
    public string RemediationAction { get; set; } = string.Empty;
    public DateTime? RemediationDate { get; set; }
    public string ControlDescription { get; set; } = string.Empty;
    
    // Compliance framework
    public ComplianceFramework Framework { get; set; }
    public string Requirement { get; set; } = string.Empty;
    public string Control { get; set; } = string.Empty;
    public string Evidence { get; set; } = string.Empty;
    
    // Compliance status
    public ComplianceStatus Status { get; set; }
    public string? NonComplianceReason { get; set; }
    public ComplianceSeverity Severity { get; set; } = ComplianceSeverity.Low;
    
    // Assessment details
    public string AssessedBy { get; set; } = string.Empty;
    public DateTime AssessmentDate { get; set; } = DateTime.UtcNow;
    public string AssessmentMethod { get; set; } = string.Empty;
    
    // Remediation information
    public string? RemediationPlan { get; set; }
    public DateTime? RemediationDeadline { get; set; }
    public string? RemediationOwner { get; set; }
    public ComplianceStatus? RemediationStatus { get; set; }
    
    // Risk assessment
    public ComplianceRiskLevel RiskLevel { get; set; } = ComplianceRiskLevel.Low;
    public string RiskDescription { get; set; } = string.Empty;
    public decimal? PotentialFineAmount { get; set; }
    
    // Related audit log entry
    public Guid? RelatedAuditLogId { get; set; }
    public AuditLogEntry? RelatedAuditLog { get; set; }
    
    // Additional compliance data
    public string ComplianceData { get; set; } = "{}";
    
    public Dictionary<string, object> GetComplianceData()
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(ComplianceData) ?? new();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    public void SetComplianceData(Dictionary<string, object> data)
    {
        ComplianceData = JsonSerializer.Serialize(data);
    }
}

/// <summary>
/// Audit alert rule configuration
/// </summary>
public class AuditAlertRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; } = true;
    
    // Rule conditions (stored as JSON)
    public string Conditions { get; set; } = "[]";
    
    // Alert configuration
    public AlertFrequency Frequency { get; set; } = AlertFrequency.Immediate;
    public int Threshold { get; set; } = 1;
    public TimeSpan TimeWindow { get; set; } = TimeSpan.FromMinutes(5);
    
    // Actions (stored as JSON)
    public string Actions { get; set; } = "[]";
    
    // Severity and priority
    public AuditSeverity AlertSeverity { get; set; } = AuditSeverity.Warning;
    public int Priority { get; set; } = 5;
    
    // Management fields
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    
    // Statistics
    public int TriggerCount { get; set; } = 0;
    public DateTime? LastTriggered { get; set; }
    public bool IsCurrentlyTriggered { get; set; } = false;
    
    public List<AuditAlertCondition> GetConditions()
    {
        try
        {
            return JsonSerializer.Deserialize<List<AuditAlertCondition>>(Conditions) ?? new();
        }
        catch
        {
            return new List<AuditAlertCondition>();
        }
    }

    public void SetConditions(List<AuditAlertCondition> conditions)
    {
        Conditions = JsonSerializer.Serialize(conditions);
    }

    public List<AlertAction> GetActions()
    {
        try
        {
            return JsonSerializer.Deserialize<List<AlertAction>>(Actions) ?? new();
        }
        catch
        {
            return new List<AlertAction>();
        }
    }

    public void SetActions(List<AlertAction> actions)
    {
        Actions = JsonSerializer.Serialize(actions);
    }
}

/// <summary>
/// System-wide audit configuration
/// </summary>
public class AuditConfiguration
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ConfigurationName { get; set; } = "Default";
    public string Name { get; set; } = "Default"; // Alias for service compatibility
    public bool IsActive { get; set; } = true;
    
    // Service expected properties
    public bool IsEnabled { get; set; } = true;
    public TimeSpan RetentionPeriod { get; set; } = TimeSpan.FromDays(2555); // 7 years
    public string? StorageConnectionString { get; set; }
    public AuditStorageType StorageType { get; set; } = AuditStorageType.Database;
    public AuditSeverity MinimumSeverity { get; set; } = AuditSeverity.Information;
    public List<string> ExcludedEventTypes { get; set; } = new();
    public List<string> IncludedEventTypes { get; set; } = new();
    
    // Logging configuration
    public bool EnableAuditLogging { get; set; } = true;
    public bool EnableDataLineageTracking { get; set; } = true;
    public bool EnableComplianceAuditing { get; set; } = true;
    public bool EnableIntegrityProtection { get; set; } = true;
    
    // Retention settings
    public TimeSpan DefaultRetentionPeriod { get; set; } = TimeSpan.FromDays(2555); // 7 years default
    public TimeSpan ArchivalPeriod { get; set; } = TimeSpan.FromDays(365); // 1 year
    public bool AutoArchive { get; set; } = true;
    public bool AutoPurge { get; set; } = false;
    
    // Performance settings
    public bool EnableAsynchronousLogging { get; set; } = true;
    public int BatchSize { get; set; } = 100;
    public TimeSpan BatchFlushInterval { get; set; } = TimeSpan.FromSeconds(30);
    
    // Alert settings
    public bool EnableRealTimeAlerts { get; set; } = true;
    public bool EnableAnomalyDetection { get; set; } = true;
    public AuditSeverity MinimumAlertSeverity { get; set; } = AuditSeverity.Warning;
    
    // Compliance settings
    public List<ComplianceFramework> EnabledFrameworks { get; set; } = new();
    public bool RequireExplicitConsent { get; set; } = true;
    public bool EnableAutomaticComplianceReports { get; set; } = true;
    
    // Security settings
    public bool RequireIntegrityValidation { get; set; } = true;
    public string IntegrityAlgorithm { get; set; } = "HMAC-SHA256";
    public bool EncryptSensitiveData { get; set; } = true;
    
    // Export settings
    public List<ReportFormat> SupportedExportFormats { get; set; } = new() 
    { 
        ReportFormat.PDF, 
        ReportFormat.CSV, 
        ReportFormat.Excel, 
        ReportFormat.JSON 
    };
    
    // Management fields
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    
    // Additional configuration (stored as JSON)
    public string AdditionalSettings { get; set; } = "{}";
    
    public Dictionary<string, object> GetAdditionalSettings()
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(AdditionalSettings) ?? new();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    public void SetAdditionalSettings(Dictionary<string, object> settings)
    {
        AdditionalSettings = JsonSerializer.Serialize(settings);
    }
}

#region Enums and Supporting Classes

/// <summary>
/// Audit severity levels
/// </summary>
public enum AuditSeverity
{
    Trace = 0,
    Debug = 1,
    Information = 2,
    Warning = 3,
    Error = 4,
    Critical = 5,
    Fatal = 6
}

/// <summary>
/// Risk levels for audit events
/// </summary>
public enum AuditRiskLevel
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

/// <summary>
/// Data operations for lineage tracking
/// </summary>
public enum DataOperation
{
    Create,
    Read,
    Update,
    Delete,
    Export,
    Import,
    Transform,
    Aggregate,
    Copy,
    Move,
    Archive,
    Purge,
    Backup,
    Restore,
    Encrypt,
    Decrypt,
    Anonymize,
    Pseudonymize,
    Share,
    Access
}

/// <summary>
/// Compliance frameworks
/// </summary>
public enum ComplianceFramework
{
    GDPR,
    HIPAA,
    SOC2,
    PCI_DSS,
    ISO27001,
    NIST_CSF,
    CCPA,
    FedRAMP,
    SOX,
    FERPA,
    COPPA
}

/// <summary>
/// Compliance status values
/// </summary>
public enum ComplianceStatus
{
    Compliant,
    NonCompliant,
    PartiallyCompliant,
    NotApplicable,
    UnderReview,
    Remediated,
    Accepted,
    Pending,
    Unknown
}

/// <summary>
/// Compliance severity levels
/// </summary>
public enum ComplianceSeverity
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

/// <summary>
/// Compliance risk levels
/// </summary>
public enum ComplianceRiskLevel
{
    VeryLow = 1,
    Low = 2,
    Medium = 3,
    High = 4,
    VeryHigh = 5,
    Critical = 6
}

/// <summary>
/// Alert frequencies
/// </summary>
public enum AlertFrequency
{
    Immediate,
    Every5Minutes,
    Every15Minutes,
    Hourly,
    Daily,
    Weekly
}

/// <summary>
/// Report export formats
/// </summary>
public enum ReportFormat
{
    PDF,
    CSV,
    Excel,
    JSON,
    XML
}

/// <summary>
/// Security classification levels
/// </summary>
public enum SecurityClassification
{
    Public = 1,
    Internal = 2,
    Confidential = 3,
    Restricted = 4,
    TopSecret = 5
}

/// <summary>
/// Audit alert condition
/// </summary>
public class AuditAlertCondition
{
    public string Field { get; set; } = string.Empty;
    public ComparisonOperator Operator { get; set; }
    public string Value { get; set; } = string.Empty;
    public bool IsNegated { get; set; } = false;
    public bool CaseSensitive { get; set; } = false;
}

/// <summary>
/// Alert action configuration
/// </summary>
public class AlertAction
{
    public AlertActionType Type { get; set; }
    public string Target { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string> Parameters { get; set; } = new();
}

/// <summary>
/// Comparison operators for alert conditions
/// </summary>
public enum ComparisonOperator
{
    Equals,
    NotEquals,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    Contains,
    StartsWith,
    EndsWith,
    Matches, // Regex
    In,
    NotIn
}

/// <summary>
/// Alert action types
/// </summary>
public enum AlertActionType
{
    Email,
    SMS,
    Slack,
    Teams,
    Webhook,
    Log,
    Database,
    PushNotification
}

/// <summary>
/// Audit storage types
/// </summary>
public enum AuditStorageType
{
    Database,
    FileSystem,
    BlobStorage,
    EventHub,
    ServiceBus
}

/// <summary>
/// Compliance overall status for reports
/// </summary>
public enum ComplianceOverallStatus
{
    Compliant,
    NonCompliant,
    PartiallyCompliant,
    InProgress,
    NotAssessed,
    FullyCompliant,
    MostlyCompliant
}

/// <summary>
/// Compliance control status for individual controls
/// </summary>
public enum ComplianceControlStatus
{
    Implemented,
    NotImplemented,
    PartiallyImplemented,
    InProgress,
    NotApplicable,
    RequiresReview
}

/// <summary>
/// Alert operators for conditions
/// </summary>
public enum AlertOperator
{
    Equals,
    NotEquals,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    Contains,
    StartsWith,
    EndsWith,
    Matches, // Regex
    In,
    NotIn
}

#endregion