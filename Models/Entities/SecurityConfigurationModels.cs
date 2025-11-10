#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADPA.Models.Entities
{
    /// <summary>
    /// Central security configuration entity for managing system-wide security settings
    /// </summary>
    [Table("SecurityConfigurations")]
    public class SecurityConfiguration
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ConfigurationType { get; set; } = string.Empty;

        [Required]
        public string ConfigurationData { get; set; } = string.Empty;

        [StringLength(20)]
        public string Version { get; set; } = "1.0";

        public bool IsActive { get; set; } = true;

        public bool IsDefault { get; set; } = false;

        public int Priority { get; set; } = 100;

        [StringLength(50)]
        public string Environment { get; set; } = "Production";

        public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;

        public DateTime? EffectiveTo { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(255)]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime? LastModified { get; set; }

        [StringLength(255)]
        public string? LastModifiedBy { get; set; }

        [StringLength(50)]
        public string ApprovalStatus { get; set; } = "Draft"; // Draft, Pending, Approved, Rejected

        [StringLength(255)]
        public string? ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        [StringLength(1000)]
        public string? Tags { get; set; }

        // Navigation properties
        public virtual ICollection<SecurityPolicyRule> PolicyRules { get; set; } = new List<SecurityPolicyRule>();
        public virtual ICollection<SecurityConfigurationAudit> AuditLogs { get; set; } = new List<SecurityConfigurationAudit>();
    }

    /// <summary>
    /// Security policy rule entity for defining security policies and enforcement rules
    /// </summary>
    [Table("SecurityPolicyRules")]
    public class SecurityPolicyRule
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid SecurityConfigurationId { get; set; }

        [Required]
        [StringLength(200)]
        public string RuleName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string PolicyType { get; set; } = string.Empty; // Authentication, Authorization, Encryption, Validation, etc.

        [Required]
        public string RuleDefinition { get; set; } = string.Empty;

        [StringLength(20)]
        public string Severity { get; set; } = "Medium"; // Critical, High, Medium, Low, Info

        [StringLength(50)]
        public string Action { get; set; } = "Log"; // Block, Allow, Log, Alert, Quarantine

        public bool IsEnabled { get; set; } = true;

        public int Priority { get; set; } = 100;

        public string? Conditions { get; set; }

        public string? Parameters { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(255)]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime? LastModified { get; set; }

        [StringLength(255)]
        public string? ModifiedBy { get; set; }

        // Navigation properties
        [ForeignKey("SecurityConfigurationId")]
        public virtual SecurityConfiguration SecurityConfiguration { get; set; } = null!;
        public virtual ICollection<PolicyRuleViolation> Violations { get; set; } = new List<PolicyRuleViolation>();
    }

    /// <summary>
    /// Security hardening guideline entity for managing security hardening recommendations
    /// </summary>
    [Table("SecurityHardeningGuidelines")]
    public class SecurityHardeningGuideline
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty; // Network, System, Application, Database, etc.

        [StringLength(20)]
        public string Severity { get; set; } = "Medium"; // Critical, High, Medium, Low

        [Required]
        public string Implementation { get; set; } = string.Empty;

        public string? Validation { get; set; }

        public bool IsRequired { get; set; } = true;

        public bool IsAutomated { get; set; } = false;

        public string? AutomationScript { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? ApplicableFrameworks { get; set; } // JSON array of frameworks

        public string? Prerequisites { get; set; }

        public string? RiskMitigation { get; set; }

        public int EstimatedEffort { get; set; } = 0; // in hours

        [StringLength(20)]
        public string Version { get; set; } = "1.0";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(255)]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime? LastModified { get; set; }

        [StringLength(255)]
        public string? ModifiedBy { get; set; }

        // Navigation properties
        public virtual ICollection<HardeningImplementation> Implementations { get; set; } = new List<HardeningImplementation>();
    }

    /// <summary>
    /// Hardening implementation tracking entity for monitoring hardening guideline implementation
    /// </summary>
    [Table("HardeningImplementations")]
    public class HardeningImplementation
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid GuidelineId { get; set; }

        [Required]
        [StringLength(200)]
        public string SystemComponent { get; set; } = string.Empty;

        [StringLength(50)]
        public string Status { get; set; } = "NotStarted"; // NotStarted, InProgress, Completed, Failed, Skipped

        public DateTime? StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        [StringLength(255)]
        public string? ImplementedBy { get; set; }

        public string? Notes { get; set; }

        public string? ValidationResults { get; set; }

        public bool IsValidated { get; set; } = false;

        public DateTime? LastValidated { get; set; }

        [StringLength(255)]
        public string? ValidatedBy { get; set; }

        public DateTime NextReviewDate { get; set; } = DateTime.UtcNow.AddMonths(3);

        public string? Evidence { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastModified { get; set; }

        // Navigation properties
        [ForeignKey("GuidelineId")]
        public virtual SecurityHardeningGuideline Guideline { get; set; } = null!;
    }

    /// <summary>
    /// Vulnerability scan entity for managing security vulnerability assessments
    /// </summary>
    [Table("VulnerabilityScans")]
    public class VulnerabilityScan
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string ScanName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ScanType { get; set; } = string.Empty; // Network, Web, Code, Infrastructure, etc.

        [Required]
        [StringLength(200)]
        public string TargetSystem { get; set; } = string.Empty;

        public string? ScanConfiguration { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Scheduled"; // Scheduled, Running, Completed, Failed, Cancelled

        public DateTime ScheduledAt { get; set; } = DateTime.UtcNow;

        public DateTime? StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public string? ScanResults { get; set; }

        public int CriticalCount { get; set; } = 0;

        public int HighCount { get; set; } = 0;

        public int MediumCount { get; set; } = 0;

        public int LowCount { get; set; } = 0;

        public int InfoCount { get; set; } = 0;

        [Column(TypeName = "decimal(5,2)")]
        public double OverallScore { get; set; } = 0.0;

        [StringLength(255)]
        public string? ExecutedBy { get; set; }

        [StringLength(100)]
        public string? ScanTool { get; set; }

        [StringLength(50)]
        public string? ScannerVersion { get; set; }

        public bool IsRecurring { get; set; } = false;

        public string? RecurrenceSchedule { get; set; }

        public DateTime? NextScanDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<VulnerabilityFinding> Findings { get; set; } = new List<VulnerabilityFinding>();
    }

    /// <summary>
    /// Vulnerability finding entity for detailed vulnerability information
    /// </summary>
    [Table("VulnerabilityFindings")]
    public class VulnerabilityFinding
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ScanId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [StringLength(20)]
        public string Severity { get; set; } = "Medium"; // Critical, High, Medium, Low, Info

        [StringLength(50)]
        public string? VulnerabilityId { get; set; } // CVE, CWE, etc.

        [Column(TypeName = "decimal(3,1)")]
        public double CVSSScore { get; set; } = 0.0;

        [StringLength(200)]
        public string? CVSSVector { get; set; }

        [StringLength(200)]
        public string? AffectedComponent { get; set; }

        [StringLength(500)]
        public string? Location { get; set; }

        public string? Evidence { get; set; }

        public string? Recommendation { get; set; }

        public string? References { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Open"; // Open, InProgress, Resolved, Closed, FalsePositive

        [StringLength(255)]
        public string? AssignedTo { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime IdentifiedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ResolvedAt { get; set; }

        public string? ResolutionNotes { get; set; }

        public bool IsFalsePositive { get; set; } = false;

        public string? FalsePositiveJustification { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastModified { get; set; }

        // Navigation properties
        [ForeignKey("ScanId")]
        public virtual VulnerabilityScan Scan { get; set; } = null!;
        public virtual ICollection<VulnerabilityRemediationAction> RemediationActions { get; set; } = new List<VulnerabilityRemediationAction>();
    }

    /// <summary>
    /// Vulnerability remediation action entity for tracking remediation efforts
    /// </summary>
    [Table("VulnerabilityRemediationActions")]
    public class VulnerabilityRemediationAction
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid FindingId { get; set; }

        [Required]
        [StringLength(100)]
        public string ActionType { get; set; } = string.Empty; // Patch, Configuration, Mitigation, etc.

        [Required]
        public string ActionDescription { get; set; } = string.Empty;

        public DateTime ActionDate { get; set; } = DateTime.UtcNow;

        [StringLength(255)]
        public string? ActionBy { get; set; }

        public string? Evidence { get; set; }

        public bool IsEffective { get; set; } = true;

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("FindingId")]
        public virtual VulnerabilityFinding Finding { get; set; } = null!;
    }

    /// <summary>
    /// Security configuration metric entity for tracking security KPIs and metrics
    /// </summary>
    [Table("SecurityConfigurationMetrics")]
    public class SecurityConfigurationMetric
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string MetricName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string MetricType { get; set; } = string.Empty; // Count, Percentage, Score, Time, etc.

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty; // Security, Compliance, Performance, etc.

        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public double Value { get; set; }

        [StringLength(20)]
        public string? Unit { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public double? Target { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public double? Threshold { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Normal"; // Normal, Warning, Critical

        public DateTime MeasuredAt { get; set; } = DateTime.UtcNow;

        public DateTime PeriodStart { get; set; }

        public DateTime PeriodEnd { get; set; }

        [StringLength(100)]
        public string? DataSource { get; set; }

        [StringLength(200)]
        public string? CalculationMethod { get; set; }

        [StringLength(500)]
        public string? Tags { get; set; }

        [StringLength(50)]
        public string Environment { get; set; } = "Production";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<SecurityMetricHistory> History { get; set; } = new List<SecurityMetricHistory>();
    }

    /// <summary>
    /// Security metric history entity for tracking metric changes over time
    /// </summary>
    [Table("SecurityMetricHistory")]
    public class SecurityMetricHistory
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid MetricId { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public double PreviousValue { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public double NewValue { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public double ChangeAmount { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public double ChangePercentage { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        [StringLength(255)]
        public string? ChangedBy { get; set; }

        public string? ChangeReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("MetricId")]
        public virtual SecurityConfigurationMetric Metric { get; set; } = null!;
    }

    /// <summary>
    /// Security configuration audit entity for tracking configuration changes
    /// </summary>
    [Table("SecurityConfigurationAudits")]
    public class SecurityConfigurationAudit
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid SecurityConfigurationId { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty; // Created, Updated, Deleted, Approved, Rejected

        public string? OldValues { get; set; }

        public string? NewValues { get; set; }

        [StringLength(500)]
        public string? ChangeDescription { get; set; }

        [StringLength(100)]
        public string? ChangeReason { get; set; }

        public DateTime AuditDate { get; set; } = DateTime.UtcNow;

        [StringLength(255)]
        public string AuditedBy { get; set; } = string.Empty;

        [StringLength(100)]
        public string? IPAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        public string? AdditionalData { get; set; }

        // Navigation properties
        [ForeignKey("SecurityConfigurationId")]
        public virtual SecurityConfiguration SecurityConfiguration { get; set; } = null!;
    }

    /// <summary>
    /// Policy rule violation entity for tracking security policy violations
    /// </summary>
    [Table("PolicyRuleViolations")]
    public class PolicyRuleViolation
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid PolicyRuleId { get; set; }

        [Required]
        [StringLength(200)]
        public string ViolationType { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [StringLength(20)]
        public string Severity { get; set; } = "Medium";

        [StringLength(100)]
        public string? AffectedResource { get; set; }

        [StringLength(100)]
        public string? AffectedUser { get; set; }

        [StringLength(100)]
        public string? SourceIP { get; set; }

        public string? ViolationData { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Open"; // Open, Investigating, Resolved, Closed, FalsePositive

        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ResolvedAt { get; set; }

        [StringLength(255)]
        public string? AssignedTo { get; set; }

        public string? ResolutionNotes { get; set; }

        [StringLength(100)]
        public string? ActionTaken { get; set; }

        public bool IsRecurring { get; set; } = false;

        public int OccurrenceCount { get; set; } = 1;

        public DateTime? LastOccurrence { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("PolicyRuleId")]
        public virtual SecurityPolicyRule PolicyRule { get; set; } = null!;
    }
}