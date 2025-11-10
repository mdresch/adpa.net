using ADPA.Models.Entities;

namespace ADPA.Models.DTOs;

/// <summary>
/// Phase 5.4: Audit System DTOs and Request/Response Models
/// </summary>

/// <summary>
/// Audit search request parameters
/// </summary>
public class AuditSearchRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<string> EventTypes { get; set; } = new();
    public List<AuditSeverity> Severities { get; set; } = new();
    public string? SearchTerm { get; set; }
    public Guid? UserId { get; set; }
    public List<string> UserIds { get; set; } = new();
    public string? Resource { get; set; }
    public List<string> Resources { get; set; } = new();
    public bool? WasSuccessful { get; set; }
    public string? IpAddress { get; set; }
    public SecurityClassification? SecurityClassification { get; set; }
    public bool? IsPersonalData { get; set; }
    public string? ComplianceFramework { get; set; }
    
    // Pagination
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    
    // Sorting
    public string SortBy { get; set; } = "Timestamp";
    public bool Descending { get; set; } = true;
}

/// <summary>
/// Audit search response with results and pagination
/// </summary>
public class AuditSearchResponse
{
    public List<AuditLogEntry> Results { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    public Dictionary<string, object> Aggregations { get; set; } = new();
}

/// <summary>
/// Audit trail analysis request
/// </summary>
public class AuditAnalysisRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<string>? EventTypes { get; set; }
    public List<AuditSeverity>? Severities { get; set; }
    public bool IncludePatternDetection { get; set; } = true;
    public bool IncludeAnomalyDetection { get; set; } = true;
    public bool IncludeRiskAssessment { get; set; } = true;
}

/// <summary>
/// Audit trail analysis result
/// </summary>
public class AuditAnalysisResult
{
    public TimeSpan AnalysisPeriod { get; set; }
    public int TotalEvents { get; set; }
    public Dictionary<string, int> EventTypeCounts { get; set; } = new();
    public Dictionary<AuditSeverity, int> SeverityCounts { get; set; } = new();
    public List<AuditPattern> DetectedPatterns { get; set; } = new();
    public List<AuditAnomaly> DetectedAnomalies { get; set; } = new();
    public AuditRiskAssessment RiskAssessment { get; set; } = new();
    public Dictionary<string, object> AdditionalMetrics { get; set; } = new();
    public DateTime AnalysisTimestamp { get; set; } = DateTime.UtcNow;

    // Additional properties expected by audit services
    public Dictionary<string, int> EventsByType { get; set; } = new();
    public Dictionary<AuditSeverity, int> EventsBySeverity { get; set; } = new();
    public Dictionary<string, int> EventsByUser { get; set; } = new();
    public List<AuditLogEntry> HighRiskEvents { get; set; } = new();
    public List<ComplianceViolation> SecurityViolations { get; set; } = new();
    public List<ComplianceViolation> ComplianceViolations { get; set; } = new();
}

/// <summary>
/// Compliance report generation request
/// </summary>
public class ComplianceReportRequest
{
    public ComplianceFramework Framework { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<string>? SpecificControls { get; set; }
    public List<ComplianceStatus>? StatusFilter { get; set; }
    public ReportFormat Format { get; set; } = ReportFormat.PDF;
    public string? RequestedBy { get; set; }
    public bool IncludeDetails { get; set; } = true;
    public bool IncludeRecommendations { get; set; } = true;
    public string? CustomTemplate { get; set; }
}

/// <summary>
/// Compliance report generation response
/// </summary>
public class ComplianceReportResponse
{
    public Guid ReportId { get; set; } = Guid.NewGuid();
    public ComplianceFramework Framework { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string GeneratedBy { get; set; } = string.Empty;
    public ComplianceReportSummary Summary { get; set; } = new();
    public List<ComplianceSection> Sections { get; set; } = new();
    public List<ComplianceViolation> Violations { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public byte[]? ReportData { get; set; }
    public string? ReportUrl { get; set; }
    public ReportFormat Format { get; set; }

    // Additional properties expected by audit services
    public string ReportPeriod { get; set; } = string.Empty;
    public Dictionary<string, ComplianceControlStatus> ControlStatuses { get; set; } = new();
    public ComplianceOverallStatus OverallStatus { get; set; }
}

/// <summary>
/// Audit integrity verification request
/// </summary>
public class AuditIntegrityRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool VerifyHashes { get; set; } = true;
    public bool VerifyTimestamps { get; set; } = true;
    public bool VerifySequence { get; set; } = true;
}

/// <summary>
/// Audit integrity verification result
/// </summary>
public class AuditIntegrityResult
{
    public bool IsValid { get; set; }
    public DateTime VerificationDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string VerifiedBy { get; set; } = string.Empty;
    public int TotalRecordsVerified { get; set; }
    public int IntegrityViolations { get; set; }
    public List<IntegrityViolation> Violations { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Data operation logging request
/// </summary>
public class LogDataOperationRequest
{
    public string DataType { get; set; } = string.Empty;
    public string DataId { get; set; } = string.Empty;
    public DataOperation Operation { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string? LegalBasis { get; set; }
    public Dictionary<string, object>? AdditionalMetadata { get; set; }
}

/// <summary>
/// Alert rule creation request
/// </summary>
public class CreateAlertRuleRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; } = true;
    public List<AuditAlertCondition> Conditions { get; set; } = new();
    public AlertFrequency Frequency { get; set; } = AlertFrequency.Immediate;
    public int Threshold { get; set; } = 1;
    public TimeSpan TimeWindow { get; set; } = TimeSpan.FromMinutes(5);
    public List<AlertAction> Actions { get; set; } = new();
    public AuditSeverity AlertSeverity { get; set; } = AuditSeverity.Warning;
}

/// <summary>
/// Alert rule update request
/// </summary>
public class UpdateAlertRuleRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; } = true;
    public List<AuditAlertCondition> Conditions { get; set; } = new();
    public AlertFrequency Frequency { get; set; } = AlertFrequency.Immediate;
    public int Threshold { get; set; } = 1;
    public TimeSpan TimeWindow { get; set; } = TimeSpan.FromMinutes(5);
    public List<AlertAction> Actions { get; set; } = new();
    public AuditSeverity AlertSeverity { get; set; } = AuditSeverity.Warning;
}

/// <summary>
/// Archive request
/// </summary>
public class ArchiveRequest
{
    public DateTime BeforeDate { get; set; }
    public List<string>? EventTypes { get; set; }
    public bool VerifyIntegrity { get; set; } = true;
    public string? ArchiveLocation { get; set; }
}

/// <summary>
/// Purge request
/// </summary>
public class PurgeRequest
{
    public DateTime BeforeDate { get; set; }
    public bool Force { get; set; } = false;
    public string? Reason { get; set; }
    public bool CreateBackup { get; set; } = true;
}

#region Supporting Models

/// <summary>
/// Audit pattern detection result
/// </summary>
public class AuditPattern
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string PatternType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Occurrences { get; set; }
    public double Confidence { get; set; }
    public DateTime FirstOccurrence { get; set; }
    public DateTime LastOccurrence { get; set; }
    public List<Guid> RelatedAuditLogIds { get; set; } = new();
    public AuditSeverity Severity { get; set; }
    public Dictionary<string, object> PatternData { get; set; } = new();

    // Additional properties expected by audit services
    public int Frequency { get; set; }
    public AuditRiskLevel RiskLevel { get; set; }
}

/// <summary>
/// Audit anomaly detection result
/// </summary>
public class AuditAnomaly
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string AnomalyType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double AnomalyScore { get; set; }
    public AuditSeverity Severity { get; set; }
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    public List<Guid> AffectedAuditLogIds { get; set; } = new();
    public Dictionary<string, object> AnomalyData { get; set; } = new();
    public string? RecommendedAction { get; set; }

    // Additional properties expected by audit services  
    public double Score { get; set; }
    public AuditRiskLevel RiskLevel { get; set; }
    public DateTime FirstDetected { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Risk assessment result
/// </summary>
public class AuditRiskAssessment
{
    public string OverallRiskLevel { get; set; } = "Low";
    public double RiskScore { get; set; }
    public List<string> RiskFactors { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public Dictionary<string, double> CategoryRisks { get; set; } = new();
}

/// <summary>
/// Compliance report summary
/// </summary>
public class ComplianceReportSummary
{
    public int TotalControls { get; set; }
    public int CompliantControls { get; set; }
    public int NonCompliantControls { get; set; }
    public int PartiallyCompliantControls { get; set; }
    public double CompliancePercentage { get; set; }
    public string OverallStatus { get; set; } = "Unknown";
    public List<string> CriticalIssues { get; set; } = new();
}

/// <summary>
/// Compliance report section
/// </summary>
public class ComplianceSection
{
    public string SectionName { get; set; } = string.Empty;
    public string Control { get; set; } = string.Empty;
    public ComplianceStatus Status { get; set; }
    public string Evidence { get; set; } = string.Empty;
    public string? NonComplianceReason { get; set; }
    public List<string> Recommendations { get; set; } = new();
    public DateTime LastAssessment { get; set; }
}

/// <summary>
/// Compliance violation details
/// </summary>
public class ComplianceViolation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public ComplianceFramework Framework { get; set; }
    public string Control { get; set; } = string.Empty;
    public string Requirement { get; set; } = string.Empty;
    public ComplianceSeverity Severity { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }
    public ComplianceStatus Status { get; set; }
    public string? RemediationPlan { get; set; }
    public DateTime? RemediationDeadline { get; set; }
    public string? RemediationOwner { get; set; }
    public decimal? PotentialFineAmount { get; set; }
}

/// <summary>
/// Integrity violation details
/// </summary>
public class IntegrityViolation
{
    public Guid AuditLogId { get; set; }
    public string ViolationType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }
    public string? ExpectedValue { get; set; }
    public string? ActualValue { get; set; }
}

/// <summary>
/// Compliance control status information
/// </summary>
public class ComplianceControlStatus
{
    public string ControlId { get; set; } = string.Empty;
    public string ControlName { get; set; } = string.Empty;
    public ComplianceStatus Status { get; set; }
    public DateTime LastAssessed { get; set; } = DateTime.UtcNow;
    public int TotalChecks { get; set; }
    public int PassedChecks { get; set; }
    public int FailedChecks { get; set; }
    public double CompliancePercentage { get; set; }
    public List<string> Issues { get; set; } = new();
    public string? ResponsibleOwner { get; set; }
}

#endregion