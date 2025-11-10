using ADPA.Models.Entities;

namespace ADPA.Models.DTOs;

/// <summary>
/// Phase 5.5: Security Monitoring DTOs and Request/Response Models
/// </summary>

/// <summary>
/// Incident search request parameters
/// </summary>
public class IncidentSearchRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<IncidentSeverity>? Severities { get; set; }
    public List<IncidentType>? Types { get; set; }
    public List<IncidentStatus>? Statuses { get; set; }
    public string? AssignedTo { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// Incident statistics summary
/// </summary>
public class IncidentStatistics
{
    public int TotalIncidents { get; set; }
    public int OpenIncidents { get; set; }
    public int ResolvedIncidents { get; set; }
    public int CriticalIncidents { get; set; }
    public int HighSeverityIncidents { get; set; }
    public double AverageResolutionTime { get; set; }
    public Dictionary<string, int> IncidentsByType { get; set; } = new();
    public object? Period { get; set; }
}

/// <summary>
/// Threat indicator search request
/// </summary>
public class ThreatIndicatorSearchRequest
{
    public bool IsActiveOnly { get; set; } = true;
    public List<ThreatIndicatorType>? Types { get; set; }
    public List<ThreatSeverity>? Severities { get; set; }
    public string? SearchValue { get; set; }
    public string? Source { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// Threat intelligence summary
/// </summary>
public class ThreatIntelligenceSummary
{
    public int TotalActiveIndicators { get; set; }
    public Dictionary<string, int> IndicatorsByType { get; set; } = new();
    public Dictionary<string, int> IndicatorsBySeverity { get; set; } = new();
    public int RecentMatches { get; set; }
    public int ExpiringIndicators { get; set; }
}

/// <summary>
/// Anomaly detection request
/// </summary>
public class AnomalyDetectionRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<AnomalyDetectionType> DetectionTypes { get; set; } = new();
    public double Sensitivity { get; set; } = 80.0;
    public bool IncludeResolved { get; set; } = false;
}

/// <summary>
/// Anomaly search request
/// </summary>
public class AnomalySearchRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<AnomalySeverity>? Severities { get; set; }
    public List<AnomalyStatus>? Statuses { get; set; }
    public double? MinimumScore { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// Anomaly statistics summary
/// </summary>
public class AnomalyStatistics
{
    public int TotalAnomalies { get; set; }
    public int VerifiedAnomalies { get; set; }
    public int FalsePositives { get; set; }
    public int UnderInvestigation { get; set; }
    public double AverageScore { get; set; }
    public Dictionary<string, int> AnomaliesBySeverity { get; set; } = new();
}

/// <summary>
/// Alert search request
/// </summary>
public class AlertSearchRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<AlertSeverity>? Severities { get; set; }
    public List<AlertStatus>? Statuses { get; set; }
    public string? AssignedTo { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// Alert statistics summary
/// </summary>
public class AlertStatistics
{
    public int TotalAlerts { get; set; }
    public int NewAlerts { get; set; }
    public int AcknowledgedAlerts { get; set; }
    public int ResolvedAlerts { get; set; }
    public int EscalatedAlerts { get; set; }
    public double AverageResolutionTime { get; set; }
    public Dictionary<string, int> AlertsBySeverity { get; set; } = new();
}

/// <summary>
/// Security dashboard data
/// </summary>
public class SecurityDashboardData
{
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public SecurityOverview Overview { get; set; } = new();
    public List<RecentIncident> RecentIncidents { get; set; } = new();
    public List<TopThreat> TopThreats { get; set; } = new();
    public List<SecurityMetricSummary> KeyMetrics { get; set; } = new();
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}

/// <summary>
/// Security metric search request
/// </summary>
public class MetricSearchRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? MetricName { get; set; }
    public string? Category { get; set; }
    public MetricType? Type { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// Threat hunting query
/// </summary>
public class ThreatHuntingQuery
{
    public string QueryName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Query { get; set; } = string.Empty;
    public List<string> DataSources { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Threat hunting result
/// </summary>
public class ThreatHuntingResult
{
    public Guid QueryId { get; set; } = Guid.NewGuid();
    public string QueryName { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan ExecutionTime { get; set; }
    public int ResultCount { get; set; }
    public List<ThreatHuntingMatch> Matches { get; set; } = new();
    public Dictionary<string, object> Statistics { get; set; } = new();
    public bool HasMoreResults { get; set; }
}

/// <summary>
/// Security pattern detection request
/// </summary>
public class PatternDetectionRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<PatternType> PatternTypes { get; set; } = new();
    public double MinimumConfidence { get; set; } = 0.7;
    public int MaxResults { get; set; } = 100;
}

/// <summary>
/// Risk assessment request
/// </summary>
public class RiskAssessmentRequest
{
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public DateTime AssessmentDate { get; set; } = DateTime.UtcNow;
    public List<string> RiskFactors { get; set; } = new();
    public Dictionary<string, object> Context { get; set; } = new();
}

/// <summary>
/// Risk assessment result
/// </summary>
public class RiskAssessmentResult
{
    public Guid AssessmentId { get; set; } = Guid.NewGuid();
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public DateTime AssessedAt { get; set; } = DateTime.UtcNow;
    public RiskLevel OverallRisk { get; set; }
    public double RiskScore { get; set; }
    public List<RiskFactor> IdentifiedRisks { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public Dictionary<string, double> CategoryScores { get; set; } = new();
}

/// <summary>
/// Security action configuration
/// </summary>
public class SecurityAction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ActionType { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public bool IsAutomatic { get; set; } = false;
}

/// <summary>
/// Security action execution result
/// </summary>
public class SecurityActionResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan ExecutionTime { get; set; }
    public Dictionary<string, object> ResultData { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

#region Supporting Classes

/// <summary>
/// Security overview summary
/// </summary>
public class SecurityOverview
{
    public string SecurityPosture { get; set; } = "Unknown";
    public int ActiveThreats { get; set; }
    public int OpenIncidents { get; set; }
    public int NewAlerts { get; set; }
    public int CriticalAnomalies { get; set; }
    public double ThreatLevel { get; set; }
}

/// <summary>
/// Recent incident summary
/// </summary>
public class RecentIncident
{
    public Guid Id { get; set; }
    public string IncidentNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public IncidentSeverity Severity { get; set; }
    public IncidentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? AssignedTo { get; set; }
}

/// <summary>
/// Top threat summary
/// </summary>
public class TopThreat
{
    public string ThreatType { get; set; } = string.Empty;
    public int Count { get; set; }
    public ThreatSeverity Severity { get; set; }
    public DateTime LastSeen { get; set; }
    public double TrendPercentage { get; set; }
}

/// <summary>
/// Security metric summary
/// </summary>
public class SecurityMetricSummary
{
    public string MetricName { get; set; } = string.Empty;
    public double CurrentValue { get; set; }
    public double PreviousValue { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string Trend { get; set; } = "Stable";
    public string Status { get; set; } = "Normal";
}

/// <summary>
/// Threat hunting match
/// </summary>
public class ThreatHuntingMatch
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; }
    public string Source { get; set; } = string.Empty;
    public double MatchScore { get; set; }
    public Dictionary<string, object> MatchData { get; set; } = new();
    public List<string> MatchReasons { get; set; } = new();
}

/// <summary>
/// Security pattern
/// </summary>
public class SecurityPattern
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string PatternName { get; set; } = string.Empty;
    public PatternType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public int Occurrences { get; set; }
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }
    public List<Guid> RelatedEntities { get; set; } = new();
}

/// <summary>
/// Risk factor details
/// </summary>
public class RiskFactor
{
    public string Factor { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RiskLevel Severity { get; set; }
    public double Impact { get; set; }
    public double Likelihood { get; set; }
    public string Mitigation { get; set; } = string.Empty;
}

/// <summary>
/// Monitoring rule statistics
/// </summary>
public class MonitoringRuleStatistics
{
    public Guid RuleId { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public int TotalTriggers { get; set; }
    public int FalsePositives { get; set; }
    public DateTime? LastTriggered { get; set; }
    public double AverageTriggerRate { get; set; }
    public double EffectivenessScore { get; set; }
}

#endregion

#region Enums

/// <summary>
/// Anomaly detection types
/// </summary>
public enum AnomalyDetectionType
{
    LoginAnomaly,
    DataAccessAnomaly,
    BehavioralAnomaly,
    NetworkAnomaly,
    SystemAnomaly,
    ApplicationAnomaly
}

/// <summary>
/// Security pattern types
/// </summary>
public enum PatternType
{
    AttackPattern,
    AccessPattern,
    BehavioralPattern,
    NetworkPattern,
    DataPattern,
    TimePattern
}

#endregion