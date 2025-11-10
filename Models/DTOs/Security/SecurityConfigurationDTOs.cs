#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ADPA.Models.DTOs.Security
{
    // Security Configuration DTOs

    public class SecurityConfigurationDto
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        public string ConfigurationType { get; set; } = string.Empty;
        
        public string ConfigurationData { get; set; } = string.Empty;
        
        public string Version { get; set; } = "1.0";
        
        public bool IsActive { get; set; }
        
        public bool IsDefault { get; set; }
        
        public int Priority { get; set; }
        
        public string Environment { get; set; } = "Production";
        
        public DateTime EffectiveFrom { get; set; }
        
        public DateTime? EffectiveTo { get; set; }
        
        public string CreatedBy { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; }
        
        public string? LastModifiedBy { get; set; }
        
        public DateTime? LastModified { get; set; }
        
        public string ApprovalStatus { get; set; } = "Draft";
        
        public string? ApprovedBy { get; set; }
        
        public DateTime? ApprovedAt { get; set; }
        
        public string? Tags { get; set; }
        
        public List<SecurityPolicyRuleDto> PolicyRules { get; set; } = new();
    }

    public class CreateSecurityConfigurationRequest
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public string ConfigurationType { get; set; } = string.Empty;
        
        [Required]
        public string ConfigurationData { get; set; } = string.Empty;
        
        public string Version { get; set; } = "1.0";
        
        public int Priority { get; set; } = 100;
        
        public string Environment { get; set; } = "Production";
        
        public DateTime? EffectiveFrom { get; set; }
        
        public DateTime? EffectiveTo { get; set; }
        
        public string? Tags { get; set; }
    }

    public class UpdateSecurityConfigurationRequest
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public string ConfigurationData { get; set; } = string.Empty;
        
        public string Version { get; set; } = "1.0";
        
        public bool IsActive { get; set; } = true;
        
        public int Priority { get; set; } = 100;
        
        public string Environment { get; set; } = "Production";
        
        public DateTime? EffectiveFrom { get; set; }
        
        public DateTime? EffectiveTo { get; set; }
        
        public string? Tags { get; set; }
    }

    // Security Policy Rule DTOs

    public class SecurityPolicyRuleDto
    {
        public Guid Id { get; set; }
        
        public Guid SecurityConfigurationId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string RuleName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        public string PolicyType { get; set; } = string.Empty;
        
        public string RuleDefinition { get; set; } = string.Empty;
        
        public string Severity { get; set; } = "Medium";
        
        public string Action { get; set; } = "Log";
        
        public bool IsEnabled { get; set; } = true;
        
        public int Priority { get; set; } = 100;
        
        public string? Conditions { get; set; }
        
        public string? Parameters { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public string CreatedBy { get; set; } = string.Empty;
        
        public DateTime? LastModified { get; set; }
        
        public string? ModifiedBy { get; set; }
    }

    public class CreatePolicyRuleRequest
    {
        public Guid SecurityConfigurationId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string RuleName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public string PolicyType { get; set; } = string.Empty;
        
        [Required]
        public string RuleDefinition { get; set; } = string.Empty;
        
        public string Severity { get; set; } = "Medium";
        
        public string Action { get; set; } = "Log";
        
        public int Priority { get; set; } = 100;
        
        public string? Conditions { get; set; }
        
        public string? Parameters { get; set; }
    }

    // Security Hardening Guidelines DTOs

    public class SecurityHardeningGuidelineDto
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public string Category { get; set; } = string.Empty;
        
        public string Severity { get; set; } = "Medium";
        
        public string Implementation { get; set; } = string.Empty;
        
        public string? Validation { get; set; }
        
        public bool IsRequired { get; set; } = true;
        
        public bool IsAutomated { get; set; } = false;
        
        public string? AutomationScript { get; set; }
        
        public string[]? ApplicableFrameworks { get; set; }
        
        public string? Prerequisites { get; set; }
        
        public string? RiskMitigation { get; set; }
        
        public int EstimatedEffort { get; set; }
        
        public string Version { get; set; } = "1.0";
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; }
        
        public string CreatedBy { get; set; } = string.Empty;
        
        public List<HardeningImplementationDto> Implementations { get; set; } = new();
    }

    public class CreateHardeningGuidelineRequest
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public string Category { get; set; } = string.Empty;
        
        [Required]
        public string Severity { get; set; } = "Medium";
        
        [Required]
        public string Implementation { get; set; } = string.Empty;
        
        public string? Validation { get; set; }
        
        public bool IsRequired { get; set; } = true;
        
        public bool IsAutomated { get; set; } = false;
        
        public string? AutomationScript { get; set; }
        
        public string[]? ApplicableFrameworks { get; set; }
        
        public string? Prerequisites { get; set; }
        
        public string? RiskMitigation { get; set; }
        
        public int EstimatedEffort { get; set; }
    }

    public class HardeningImplementationDto
    {
        public Guid Id { get; set; }
        
        public Guid GuidelineId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string SystemComponent { get; set; } = string.Empty;
        
        public string Status { get; set; } = "NotStarted";
        
        public DateTime? StartedAt { get; set; }
        
        public DateTime? CompletedAt { get; set; }
        
        public string? ImplementedBy { get; set; }
        
        public string? Notes { get; set; }
        
        public string? ValidationResults { get; set; }
        
        public bool IsValidated { get; set; } = false;
        
        public DateTime? LastValidated { get; set; }
        
        public string? ValidatedBy { get; set; }
        
        public DateTime NextReviewDate { get; set; }
        
        public string? Evidence { get; set; }
    }

    public class CreateImplementationRequest
    {
        public Guid GuidelineId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string SystemComponent { get; set; } = string.Empty;
        
        public DateTime NextReviewDate { get; set; }
    }

    public class UpdateImplementationStatusRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty;
        
        public string? Notes { get; set; }
        
        public string? ValidationResults { get; set; }
        
        public string? Evidence { get; set; }
    }

    // Vulnerability Scanning DTOs

    public class VulnerabilityScanDto
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string ScanName { get; set; } = string.Empty;
        
        public string ScanType { get; set; } = string.Empty;
        
        public string TargetSystem { get; set; } = string.Empty;
        
        public string? ScanConfiguration { get; set; }
        
        public string Status { get; set; } = "Scheduled";
        
        public DateTime ScheduledAt { get; set; }
        
        public DateTime? StartedAt { get; set; }
        
        public DateTime? CompletedAt { get; set; }
        
        public string? ScanResults { get; set; }
        
        public int CriticalCount { get; set; } = 0;
        
        public int HighCount { get; set; } = 0;
        
        public int MediumCount { get; set; } = 0;
        
        public int LowCount { get; set; } = 0;
        
        public int InfoCount { get; set; } = 0;
        
        public double OverallScore { get; set; } = 0.0;
        
        public string? ExecutedBy { get; set; }
        
        public string? ScanTool { get; set; }
        
        public string? ScannerVersion { get; set; }
        
        public bool IsRecurring { get; set; } = false;
        
        public string? RecurrenceSchedule { get; set; }
        
        public DateTime? NextScanDate { get; set; }
        
        public List<VulnerabilityFindingDto> Findings { get; set; } = new();
    }

    public class CreateVulnerabilityScanRequest
    {
        [Required]
        [StringLength(200)]
        public string ScanName { get; set; } = string.Empty;
        
        [Required]
        public string ScanType { get; set; } = string.Empty;
        
        [Required]
        public string TargetSystem { get; set; } = string.Empty;
        
        public string? ScanConfiguration { get; set; }
        
        public DateTime? ScheduledAt { get; set; }
        
        public string? ScanTool { get; set; }
        
        public bool IsRecurring { get; set; } = false;
        
        public string? RecurrenceSchedule { get; set; }
    }

    public class VulnerabilityFindingDto
    {
        public Guid Id { get; set; }
        
        public Guid ScanId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public string Severity { get; set; } = "Medium";
        
        public string? VulnerabilityId { get; set; }
        
        public double CVSSScore { get; set; } = 0.0;
        
        public string? CVSSVector { get; set; }
        
        public string? AffectedComponent { get; set; }
        
        public string? Location { get; set; }
        
        public string? Evidence { get; set; }
        
        public string? Recommendation { get; set; }
        
        public string? References { get; set; }
        
        public string Status { get; set; } = "Open";
        
        public string? AssignedTo { get; set; }
        
        public DateTime? DueDate { get; set; }
        
        public DateTime IdentifiedAt { get; set; }
        
        public DateTime? ResolvedAt { get; set; }
        
        public string? ResolutionNotes { get; set; }
        
        public bool IsFalsePositive { get; set; } = false;
        
        public string? FalsePositiveJustification { get; set; }
        
        public List<VulnerabilityRemediationActionDto> RemediationActions { get; set; } = new();
    }

    public class CreateVulnerabilityFindingRequest
    {
        public Guid ScanId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public string Severity { get; set; } = "Medium";
        
        public string? VulnerabilityId { get; set; }
        
        public double CVSSScore { get; set; } = 0.0;
        
        public string? CVSSVector { get; set; }
        
        public string? AffectedComponent { get; set; }
        
        public string? Location { get; set; }
        
        public string? Evidence { get; set; }
        
        public string? Recommendation { get; set; }
        
        public string? References { get; set; }
        
        public string? AssignedTo { get; set; }
        
        public DateTime? DueDate { get; set; }
    }

    public class UpdateFindingStatusRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty;
        
        public string? Notes { get; set; }
        
        public bool IsFalsePositive { get; set; } = false;
        
        public string? FalsePositiveJustification { get; set; }
    }

    public class VulnerabilityRemediationActionDto
    {
        public Guid Id { get; set; }
        
        public Guid FindingId { get; set; }
        
        public string ActionType { get; set; } = string.Empty;
        
        public string ActionDescription { get; set; } = string.Empty;
        
        public DateTime ActionDate { get; set; }
        
        public string? ActionBy { get; set; }
        
        public string? Evidence { get; set; }
        
        public bool IsEffective { get; set; } = true;
        
        public string? Notes { get; set; }
    }

    public class CreateRemediationActionRequest
    {
        public Guid FindingId { get; set; }
        
        [Required]
        public string ActionType { get; set; } = string.Empty;
        
        [Required]
        public string ActionDescription { get; set; } = string.Empty;
        
        public string? Evidence { get; set; }
        
        public bool IsEffective { get; set; } = true;
        
        public string? Notes { get; set; }
    }

    // Security Metrics DTOs

    public class SecurityMetricDto
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string MetricName { get; set; } = string.Empty;
        
        public string MetricType { get; set; } = string.Empty;
        
        public string Category { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public double Value { get; set; }
        
        public string? Unit { get; set; }
        
        public double? Target { get; set; }
        
        public double? Threshold { get; set; }
        
        public string Status { get; set; } = "Normal";
        
        public DateTime MeasuredAt { get; set; }
        
        public DateTime PeriodStart { get; set; }
        
        public DateTime PeriodEnd { get; set; }
        
        public string? DataSource { get; set; }
        
        public string? CalculationMethod { get; set; }
        
        public string? Tags { get; set; }
        
        public string Environment { get; set; } = "Production";
    }

    public class CreateSecurityMetricRequest
    {
        [Required]
        [StringLength(200)]
        public string MetricName { get; set; } = string.Empty;
        
        [Required]
        public string MetricType { get; set; } = string.Empty;
        
        [Required]
        public string Category { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        [Required]
        public double Value { get; set; }
        
        public string? Unit { get; set; }
        
        public double? Target { get; set; }
        
        public double? Threshold { get; set; }
        
        public DateTime? MeasuredAt { get; set; }
        
        public DateTime PeriodStart { get; set; }
        
        public DateTime PeriodEnd { get; set; }
        
        public string? DataSource { get; set; }
        
        public string? CalculationMethod { get; set; }
        
        public string? Tags { get; set; }
        
        public string Environment { get; set; } = "Production";
    }

    // Dashboard and Reporting DTOs

    public class SecurityDashboardDto
    {
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        
        public Dictionary<string, object> ConfigurationMetrics { get; set; } = new();
        
        public Dictionary<string, object> PolicyMetrics { get; set; } = new();
        
        public Dictionary<string, object> HardeningMetrics { get; set; } = new();
        
        public Dictionary<string, object> VulnerabilityMetrics { get; set; } = new();
        
        public Dictionary<string, double> SecurityKPIs { get; set; } = new();
        
        public Dictionary<string, object> SystemStatus { get; set; } = new();
    }

    public class SecurityReportRequest
    {
        [Required]
        public string ReportType { get; set; } = string.Empty;
        
        [Required]
        public DateTime FromDate { get; set; }
        
        [Required]
        public DateTime ToDate { get; set; }
        
        public string[]? IncludeCategories { get; set; }
        
        public string Format { get; set; } = "JSON"; // JSON, PDF, Excel
        
        public bool IncludeCharts { get; set; } = false;
        
        public string? Environment { get; set; }
    }

    // Orchestration DTOs

    public class SecurityOrchestrationRequest
    {
        [Required]
        public string OrchestrationName { get; set; } = string.Empty;
        
        public Dictionary<string, object> Parameters { get; set; } = new();
        
        public bool IsScheduled { get; set; } = false;
        
        public DateTime? ScheduledAt { get; set; }
        
        public string? RecurrencePattern { get; set; }
    }

    public class SecurityOrchestrationResponse
    {
        public bool Success { get; set; }
        
        public string Message { get; set; } = string.Empty;
        
        public Dictionary<string, object>? Results { get; set; }
        
        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
        
        public TimeSpan ExecutionTime { get; set; }
        
        public List<string> Errors { get; set; } = new();
    }

    // Security Metric History DTOs

    public class SecurityMetricHistoryDto
    {
        public Guid Id { get; set; }
        public Guid ConfigurationId { get; set; }
        public string MetricName { get; set; } = string.Empty;
        public string MetricType { get; set; } = string.Empty;
        public double Value { get; set; }
        public DateTime RecordedAt { get; set; }
        public string? Tags { get; set; }
    }

    // Security Configuration Audit DTOs

    public class SecurityConfigurationAuditDto
    {
        public Guid Id { get; set; }
        public Guid ConfigurationId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string ChangedBy { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
        public string? Reason { get; set; }
    }

    // Policy Rule Violation DTOs

    public class PolicyRuleViolationDto
    {
        public Guid Id { get; set; }
        public Guid PolicyRuleId { get; set; }
        public string ViolationType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DetectedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolutionNotes { get; set; }
    }

    // Security Metrics Dashboard DTOs

    public class SecurityMetricsDashboard
    {
        public Guid ConfigurationId { get; set; }
        public Dictionary<string, double> Metrics { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public string Environment { get; set; } = string.Empty;
    }

    // Security Report Response DTOs

    public class SecurityReportResponse
    {
        public string ReportId { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public Dictionary<string, object> Data { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public string GeneratedBy { get; set; } = string.Empty;
    }

    // Generic Response DTOs

    public class SecurityConfigurationResponse<T>
    {
        public bool Success { get; set; }
        
        public T? Data { get; set; }
        
        public string Message { get; set; } = string.Empty;
        
        public List<string> Errors { get; set; } = new();
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class PaginatedSecurityResponse<T>
    {
        public bool Success { get; set; }
        
        public List<T> Data { get; set; } = new();
        
        public int TotalCount { get; set; }
        
        public int PageNumber { get; set; }
        
        public int PageSize { get; set; }
        
        public int TotalPages { get; set; }
        
        public string Message { get; set; } = string.Empty;
        
        public List<string> Errors { get; set; } = new();
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}