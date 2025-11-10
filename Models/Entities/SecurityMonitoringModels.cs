using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace ADPA.Models.Entities;

/// <summary>
/// Phase 5.5: Security Monitoring & Threat Detection Models
/// Real-time security monitoring, anomaly detection, and threat intelligence
/// </summary>

/// <summary>
/// Security incident tracking and management
/// </summary>
public class SecurityIncident
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string IncidentNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Incident classification
    public IncidentSeverity Severity { get; set; } = IncidentSeverity.Medium;
    public IncidentType Type { get; set; }
    public IncidentStatus Status { get; set; } = IncidentStatus.Open;
    public IncidentCategory Category { get; set; }
    
    // Incident details
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Source { get; set; }
    public string? SourceIp { get; set; }
    public string? TargetResource { get; set; }
    public string? AffectedUser { get; set; }
    
    // Assessment and impact
    public RiskLevel RiskLevel { get; set; } = RiskLevel.Medium;
    public string ImpactAssessment { get; set; } = string.Empty;
    public List<string> AffectedSystems { get; set; } = new();
    public List<string> CompromisedData { get; set; } = new();
    
    // Response and resolution
    public string? AssignedTo { get; set; }
    public string? ResponseTeam { get; set; }
    public DateTime? ResponseStartTime { get; set; }
    public DateTime? ResolutionTime { get; set; }
    public string? ResolutionSummary { get; set; }
    
    // External references
    public List<Guid> RelatedAuditLogIds { get; set; } = new();
    public List<Guid> RelatedThreatIds { get; set; } = new();
    public List<string> ExternalReferences { get; set; } = new();
    
    // Timeline and evidence
    public string IncidentTimeline { get; set; } = "[]"; // JSON array
    public string EvidenceData { get; set; } = "{}"; // JSON object
    public List<string> AttachedFiles { get; set; } = new();
    
    // Metrics
    public TimeSpan? DetectionTime { get; set; }
    public TimeSpan? ResponseTime { get; set; }
    public TimeSpan? ResolutionDuration { get; set; }
    
    // Communication
    public bool IsPubliclyDisclosed { get; set; } = false;
    public string? PublicStatement { get; set; }
    public List<string> NotifiedParties { get; set; } = new();
    
    public List<IncidentTimelineEntry> GetTimeline()
    {
        try
        {
            return JsonSerializer.Deserialize<List<IncidentTimelineEntry>>(IncidentTimeline) ?? new();
        }
        catch
        {
            return new List<IncidentTimelineEntry>();
        }
    }

    public void SetTimeline(List<IncidentTimelineEntry> timeline)
    {
        IncidentTimeline = JsonSerializer.Serialize(timeline);
    }

    public Dictionary<string, object> GetEvidenceData()
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(EvidenceData) ?? new();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    public void SetEvidenceData(Dictionary<string, object> evidence)
    {
        EvidenceData = JsonSerializer.Serialize(evidence);
    }
}

/// <summary>
/// Threat intelligence and indicators
/// </summary>
public class ThreatIndicator
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    
    // Indicator details
    public ThreatIndicatorType Type { get; set; }
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ThreatConfidenceLevel Confidence { get; set; } = ThreatConfidenceLevel.Medium;
    
    // Threat classification
    public ThreatSeverity Severity { get; set; } = ThreatSeverity.Medium;
    public List<string> ThreatTypes { get; set; } = new();
    public List<string> AttackVectors { get; set; } = new();
    public List<string> TargetSectors { get; set; } = new();
    
    // Intelligence sources
    public string Source { get; set; } = string.Empty;
    public string? SourceReliability { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? MitreAttackId { get; set; }
    
    // Contextual information
    public string? Campaign { get; set; }
    public string? Actor { get; set; }
    public string? Malware { get; set; }
    public Dictionary<string, string> Attributes { get; set; } = new();
    
    // Operational status
    public bool IsActive { get; set; } = true;
    public bool IsBlocked { get; set; } = false;
    public int MatchCount { get; set; } = 0;
    public DateTime? LastSeen { get; set; }
    
    // Relationships
    public List<Guid> RelatedIndicators { get; set; } = new();
    public List<Guid> RelatedIncidents { get; set; } = new();
    
    // Additional metadata
    public string AdditionalData { get; set; } = "{}";
    
    public Dictionary<string, object> GetAdditionalData()
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(AdditionalData) ?? new();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    public void SetAdditionalData(Dictionary<string, object> data)
    {
        AdditionalData = JsonSerializer.Serialize(data);
    }
}

/// <summary>
/// Security anomaly detection and scoring
/// </summary>
public class SecurityAnomaly
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Anomaly identification
    public string AnomalyType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AnomalySeverity Severity { get; set; } = AnomalySeverity.Medium;
    public double AnomalyScore { get; set; }
    public double ConfidenceLevel { get; set; }
    
    // Source information
    public string Source { get; set; } = string.Empty;
    public string? SourceEntity { get; set; }
    public string? SourceIp { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    
    // Detection details
    public string DetectionAlgorithm { get; set; } = string.Empty;
    public string? BaselineProfile { get; set; }
    public string ExpectedBehavior { get; set; } = string.Empty;
    public string ActualBehavior { get; set; } = string.Empty;
    
    // Anomaly data
    public string AnomalyData { get; set; } = "{}";
    public string StatisticalData { get; set; } = "{}";
    public List<string> AffectedResources { get; set; } = new();
    
    // Investigation status
    public AnomalyStatus Status { get; set; } = AnomalyStatus.New;
    public string? InvestigatedBy { get; set; }
    public DateTime? InvestigationStarted { get; set; }
    public string? InvestigationNotes { get; set; }
    public bool IsFalsePositive { get; set; } = false;
    
    // Related entities
    public Guid? RelatedIncidentId { get; set; }
    public SecurityIncident? RelatedIncident { get; set; }
    public List<Guid> RelatedAuditLogIds { get; set; } = new();
    public List<Guid> RelatedAnomalyIds { get; set; } = new();
    
    // Response actions
    public bool RequiresResponse { get; set; } = false;
    public List<string> RecommendedActions { get; set; } = new();
    public string? AutomaticResponse { get; set; }
    
    public Dictionary<string, object> GetAnomalyData()
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(AnomalyData) ?? new();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    public void SetAnomalyData(Dictionary<string, object> data)
    {
        AnomalyData = JsonSerializer.Serialize(data);
    }

    public Dictionary<string, object> GetStatisticalData()
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(StatisticalData) ?? new();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    public void SetStatisticalData(Dictionary<string, object> data)
    {
        StatisticalData = JsonSerializer.Serialize(data);
    }
}

/// <summary>
/// Security monitoring rules and policies
/// </summary>
public class SecurityMonitoringRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; } = true;
    
    // Rule configuration
    public MonitoringRuleType Type { get; set; }
    public RuleSeverity Severity { get; set; } = RuleSeverity.Medium;
    public string RuleQuery { get; set; } = string.Empty;
    public string? RuleConditions { get; set; }
    public TimeSpan TimeWindow { get; set; } = TimeSpan.FromMinutes(15);
    public int Threshold { get; set; } = 1;
    
    // Detection settings
    public bool EnableRealTimeDetection { get; set; } = true;
    public bool EnableBatchDetection { get; set; } = false;
    public TimeSpan BatchInterval { get; set; } = TimeSpan.FromHours(1);
    
    // Response configuration
    public List<MonitoringAction> Actions { get; set; } = new();
    public bool AutoCreateIncident { get; set; } = false;
    public IncidentSeverity? DefaultIncidentSeverity { get; set; }
    
    // Suppression and tuning
    public bool EnableSuppression { get; set; } = false;
    public TimeSpan? SuppressionWindow { get; set; }
    public List<string> SuppressionKeys { get; set; } = new();
    
    // Management
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    
    // Statistics
    public int TriggerCount { get; set; } = 0;
    public DateTime? LastTriggered { get; set; }
    public double AverageTriggerRate { get; set; } = 0;
    public int FalsePositiveCount { get; set; } = 0;
    
    // Rule metadata
    public List<string> DataSources { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public string? MitreMapping { get; set; }
    public string AdditionalConfig { get; set; } = "{}";
    
    public Dictionary<string, object> GetAdditionalConfig()
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(AdditionalConfig) ?? new();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    public void SetAdditionalConfig(Dictionary<string, object> config)
    {
        AdditionalConfig = JsonSerializer.Serialize(config);
    }
}

/// <summary>
/// Security alert generated by monitoring rules
/// </summary>
public class SecurityAlert
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Alert identification
    public string AlertName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; } = AlertSeverity.Medium;
    public AlertStatus Status { get; set; } = AlertStatus.New;
    
    // Source rule
    public Guid? MonitoringRuleId { get; set; }
    public SecurityMonitoringRule? MonitoringRule { get; set; }
    public string? RuleName { get; set; }
    
    // Alert data
    public string AlertData { get; set; } = "{}";
    public List<Guid> TriggeredAuditLogIds { get; set; } = new();
    public List<string> AffectedEntities { get; set; } = new();
    
    // Investigation
    public string? AssignedTo { get; set; }
    public DateTime? AssignedAt { get; set; }
    public string? InvestigationNotes { get; set; }
    public AlertDisposition Disposition { get; set; } = AlertDisposition.UnderInvestigation;
    
    // Response tracking
    public DateTime? AcknowledgedAt { get; set; }
    public string? AcknowledgedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedBy { get; set; }
    public string? ResolutionReason { get; set; }
    
    // Escalation
    public bool IsEscalated { get; set; } = false;
    public DateTime? EscalatedAt { get; set; }
    public string? EscalatedTo { get; set; }
    public string? EscalationReason { get; set; }
    
    // Related entities
    public Guid? RelatedIncidentId { get; set; }
    public SecurityIncident? RelatedIncident { get; set; }
    public List<Guid> RelatedAlertIds { get; set; } = new();
    
    public Dictionary<string, object> GetAlertData()
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(AlertData) ?? new();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    public void SetAlertData(Dictionary<string, object> data)
    {
        AlertData = JsonSerializer.Serialize(data);
    }
}

/// <summary>
/// Security dashboard metrics and KPIs
/// </summary>
public class SecurityMetric
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string MetricName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public MetricType Type { get; set; } = MetricType.Counter;
    
    // Metric values
    public double Value { get; set; }
    public string? Unit { get; set; }
    public Dictionary<string, string> Labels { get; set; } = new();
    
    // Aggregation period
    public TimeSpan? AggregationPeriod { get; set; }
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
    
    // Metadata
    public string? Description { get; set; }
    public string Source { get; set; } = string.Empty;
    public string AdditionalData { get; set; } = "{}";
    
    public Dictionary<string, object> GetAdditionalData()
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(AdditionalData) ?? new();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    public void SetAdditionalData(Dictionary<string, object> data)
    {
        AdditionalData = JsonSerializer.Serialize(data);
    }
}

#region Supporting Classes and Enums

/// <summary>
/// Incident timeline entry
/// </summary>
public class IncidentTimelineEntry
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Event { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Actor { get; set; }
    public string? Source { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}

/// <summary>
/// Security monitoring action
/// </summary>
public class MonitoringAction
{
    public MonitoringActionType Type { get; set; }
    public string Target { get; set; } = string.Empty;
    public Dictionary<string, string> Parameters { get; set; } = new();
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// Incident severity levels
/// </summary>
public enum IncidentSeverity
{
    Critical = 1,
    High = 2,
    Medium = 3,
    Low = 4,
    Informational = 5
}

/// <summary>
/// Types of security incidents
/// </summary>
public enum IncidentType
{
    UnauthorizedAccess,
    DataBreach,
    Malware,
    DenialOfService,
    Phishing,
    SocialEngineering,
    InsiderThreat,
    SystemCompromise,
    DataLoss,
    PolicyViolation,
    Other
}

/// <summary>
/// Incident status tracking
/// </summary>
public enum IncidentStatus
{
    Open,
    InProgress,
    UnderInvestigation,
    Escalated,
    Resolved,
    Closed,
    Reopened
}

/// <summary>
/// Incident categories
/// </summary>
public enum IncidentCategory
{
    Security,
    Privacy,
    Compliance,
    Operational,
    Technical,
    Business
}

/// <summary>
/// Risk assessment levels
/// </summary>
public enum RiskLevel
{
    VeryLow = 1,
    Low = 2,
    Medium = 3,
    High = 4,
    VeryHigh = 5,
    Critical = 6
}

/// <summary>
/// Threat indicator types
/// </summary>
public enum ThreatIndicatorType
{
    IpAddress,
    Domain,
    Url,
    EmailAddress,
    FileHash,
    FileName,
    Registry,
    Process,
    Service,
    UserAgent,
    Certificate,
    Other
}

/// <summary>
/// Threat confidence levels
/// </summary>
public enum ThreatConfidenceLevel
{
    VeryLow = 1,
    Low = 2,
    Medium = 3,
    High = 4,
    VeryHigh = 5
}

/// <summary>
/// Threat severity levels
/// </summary>
public enum ThreatSeverity
{
    Critical = 1,
    High = 2,
    Medium = 3,
    Low = 4,
    Informational = 5
}

/// <summary>
/// Security anomaly severity
/// </summary>
public enum AnomalySeverity
{
    Critical = 1,
    High = 2,
    Medium = 3,
    Low = 4
}

/// <summary>
/// Anomaly investigation status
/// </summary>
public enum AnomalyStatus
{
    New,
    UnderInvestigation,
    Verified,
    FalsePositive,
    Resolved,
    Suppressed
}

/// <summary>
/// Monitoring rule types
/// </summary>
public enum MonitoringRuleType
{
    Threshold,
    Anomaly,
    Correlation,
    Pattern,
    Behavioral,
    Statistical,
    MachineLearning
}

/// <summary>
/// Rule severity levels
/// </summary>
public enum RuleSeverity
{
    Critical = 1,
    High = 2,
    Medium = 3,
    Low = 4,
    Informational = 5
}

/// <summary>
/// Security alert severity
/// </summary>
public enum AlertSeverity
{
    Critical = 1,
    High = 2,
    Medium = 3,
    Low = 4,
    Informational = 5
}

/// <summary>
/// Alert status tracking
/// </summary>
public enum AlertStatus
{
    New,
    Acknowledged,
    InProgress,
    Resolved,
    Closed,
    Suppressed
}

/// <summary>
/// Alert investigation disposition
/// </summary>
public enum AlertDisposition
{
    UnderInvestigation,
    TruePositive,
    FalsePositive,
    BenignPositive,
    Undetermined
}

/// <summary>
/// Monitoring action types
/// </summary>
public enum MonitoringActionType
{
    Email,
    Webhook,
    CreateIncident,
    CreateAlert,
    BlockIp,
    DisableUser,
    Quarantine,
    Log,
    Dashboard
}

/// <summary>
/// Security metric types
/// </summary>
public enum MetricType
{
    Counter,
    Gauge,
    Histogram,
    Rate,
    Percentage
}

#endregion