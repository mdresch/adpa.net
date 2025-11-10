using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADPA.Models.Entities
{
    // Additional Compliance Enums (ViolationSeverity is in AuditModels.cs)
    public enum ViolationSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum DataSubjectRights
    {
        Access,
        Rectification,
        Erasure,
        Portability,
        Restriction,
        Objection
    }

    public enum RetentionAction
    {
        Retain,
        Archive,
        Delete,
        Anonymize,
        Review
    }

    public enum PolicyType
    {
        DataProtection,
        AccessControl,
        DataRetention,
        IncidentResponse,
        PrivacyNotice,
        ConsentManagement,
        DataProcessing,
        ThirdPartyRisk,
        BusinessContinuity,
        Custom
    }

    // Core Compliance Policy Model
    [Table("CompliancePolicies")]
    public class CompliancePolicy
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public ComplianceFramework Framework { get; set; }

        public PolicyType PolicyType { get; set; }

        [Required]
        public string PolicyContent { get; set; }

        public string RequirementReference { get; set; }

        public bool IsActive { get; set; }

        public bool IsMandatory { get; set; }

        public DateTime EffectiveDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? LastReviewDate { get; set; }

        public string ReviewedBy { get; set; }

        public int Version { get; set; }

        // Navigation Properties
        public virtual ICollection<ComplianceViolation> Violations { get; set; }
        public virtual ICollection<PolicyControl> Controls { get; set; }
        public virtual ICollection<ComplianceAssessment> Assessments { get; set; }
    }

    // Policy Controls and Requirements
    [Table("PolicyControls")]
    public class PolicyControl
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid PolicyId { get; set; }

        [Required]
        [StringLength(200)]
        public string ControlName { get; set; }

        public string Description { get; set; }

        [Required]
        public string ImplementationGuidance { get; set; }

        public string ValidationCriteria { get; set; }

        public bool IsAutomated { get; set; }

        public int CheckFrequencyDays { get; set; }

        public ComplianceStatus Status { get; set; }

        public DateTime? LastChecked { get; set; }

        public DateTime? NextCheckDue { get; set; }

        public string ResponsibleRole { get; set; }

        public string Evidence { get; set; }

        // Navigation Properties
        [ForeignKey("PolicyId")]
        public virtual CompliancePolicy Policy { get; set; }
        public virtual ICollection<ControlAssessment> Assessments { get; set; }
    }

    // Compliance Violations and Incidents
    [Table("ComplianceViolations")]
    public class ComplianceViolation
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid PolicyId { get; set; }

        public Guid? ControlId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public ViolationSeverity Severity { get; set; }

        public ComplianceFramework Framework { get; set; }

        public DateTime DetectedAt { get; set; }

        public string DetectedBy { get; set; }

        public string DetectionMethod { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public string ResolvedBy { get; set; }

        public string ResolutionNotes { get; set; }

        public ComplianceStatus Status { get; set; }

        // Additional properties expected by audit services
        [StringLength(100)]
        public string ControlName { get; set; } = string.Empty;

        [StringLength(100)]
        public string ViolationType { get; set; } = string.Empty;

        [StringLength(500)]
        public string RemediationAction { get; set; } = string.Empty;

        public DateTime? RemediationDue { get; set; }

        public string AffectedEntity { get; set; }

        public string AffectedData { get; set; }

        public int? AffectedRecordCount { get; set; }

        public bool RequiresNotification { get; set; }

        public DateTime? NotificationDeadline { get; set; }

        public bool IsNotified { get; set; }

        public DateTime? NotifiedAt { get; set; }

        public string RiskAssessment { get; set; }

        public string RemediationPlan { get; set; }

        public decimal? PotentialFine { get; set; }

        public string Currency { get; set; }

        // Navigation Properties
        [ForeignKey("PolicyId")]
        public virtual CompliancePolicy Policy { get; set; }
        
        [ForeignKey("ControlId")]
        public virtual PolicyControl Control { get; set; }
        public virtual ICollection<ViolationAction> Actions { get; set; }
    }

    // Data Subject Rights Management (GDPR/CCPA)
    [Table("DataSubjectRequests")]
    public class DataSubjectRequest
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string RequestNumber { get; set; }

        [Required]
        public DataSubjectRights RequestType { get; set; }

        [Required]
        [StringLength(200)]
        public string SubjectName { get; set; }

        [Required]
        [EmailAddress]
        public string SubjectEmail { get; set; }

        public string SubjectPhone { get; set; }

        public string IdentityVerification { get; set; }

        public bool IsVerified { get; set; }

        [Required]
        public string RequestDescription { get; set; }

        public DateTime RequestedAt { get; set; }

        public DateTime? ResponseDeadline { get; set; }

        public ComplianceStatus Status { get; set; }

        public string AssignedTo { get; set; }

        public DateTime? CompletedAt { get; set; }

        public string ResponseMethod { get; set; }

        public string ResponseDetails { get; set; }

        public bool IsComplexRequest { get; set; }

        public int ExtensionDays { get; set; }

        public string ExtensionReason { get; set; }

        public string DataSources { get; set; }

        public string ProcessingLegalBasis { get; set; }

        public decimal ProcessingCost { get; set; }

        public bool IsFeeCharged { get; set; }

        // Navigation Properties
        public virtual ICollection<DataSubjectAction> Actions { get; set; }
    }

    // Data Retention and Lifecycle Management
    [Table("DataRetentionPolicies")]
    public class DataRetentionPolicy
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public string DataCategory { get; set; }

        public ComplianceFramework ApplicableFramework { get; set; }

        public int RetentionPeriodDays { get; set; }

        public string RetentionCriteria { get; set; }

        [Required]
        public RetentionAction RetentionAction { get; set; }

        public string LegalBasis { get; set; }

        public bool IsActive { get; set; }

        public DateTime EffectiveDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public string ResponsibleRole { get; set; }

        public bool RequiresApproval { get; set; }

        public string ApprovalWorkflow { get; set; }

        public DateTime CreatedAt { get; set; }

        public string CreatedBy { get; set; }

        // Navigation Properties
        public virtual ICollection<DataRetentionRecord> RetentionRecords { get; set; }
    }

    // Privacy Impact Assessments (DPIA)
    [Table("PrivacyImpactAssessments")]
    public class PrivacyImpactAssessment
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string ProjectName { get; set; }

        public string ProjectDescription { get; set; }

        public string DataController { get; set; }

        public string DataProcessor { get; set; }

        public DateTime AssessmentDate { get; set; }

        public string ConductedBy { get; set; }

        public ComplianceStatus Status { get; set; }

        public string DataTypes { get; set; }

        public string ProcessingPurpose { get; set; }

        public string LegalBasis { get; set; }

        public string DataSources { get; set; }

        public string DataRecipients { get; set; }

        public bool InvolvesSpecialCategories { get; set; }

        public bool InvolvesChildren { get; set; }

        public bool InvolvesProfiling { get; set; }

        public bool InvolvesAutomatedDecisions { get; set; }

        public string PrivacyRisks { get; set; }

        public string RiskMitigation { get; set; }

        public int RiskScore { get; set; }

        public string Recommendations { get; set; }

        public DateTime? ReviewDate { get; set; }

        public bool RequiresMonitoring { get; set; }

        public string MonitoringPlan { get; set; }

        // Navigation Properties
        public virtual ICollection<PIAStakeholder> Stakeholders { get; set; }
        public virtual ICollection<PIARisk> Risks { get; set; }
    }

    // Compliance Assessments and Audits
    [Table("ComplianceAssessments")]
    public class ComplianceAssessment
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string AssessmentName { get; set; }

        [Required]
        public ComplianceFramework Framework { get; set; }

        public Guid? PolicyId { get; set; }

        public DateTime AssessmentDate { get; set; }

        public string AssessorName { get; set; }

        public string AssessorOrganization { get; set; }

        public ComplianceStatus OverallStatus { get; set; }

        public int ComplianceScore { get; set; }

        public string Methodology { get; set; }

        public string Scope { get; set; }

        public string KeyFindings { get; set; }

        public string Recommendations { get; set; }

        public DateTime? NextAssessmentDue { get; set; }

        public string CertificationStatus { get; set; }

        public DateTime? CertificationExpiry { get; set; }

        public string ExecutiveSummary { get; set; }

        public string DetailedReport { get; set; }

        // Navigation Properties
        [ForeignKey("PolicyId")]
        public virtual CompliancePolicy Policy { get; set; }
        public virtual ICollection<ControlAssessment> ControlAssessments { get; set; }
        public virtual ICollection<AssessmentFinding> Findings { get; set; }
    }

    // Supporting Models
    [Table("ViolationActions")]
    public class ViolationAction
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ViolationId { get; set; }

        [Required]
        [StringLength(200)]
        public string ActionType { get; set; }

        [Required]
        public string Description { get; set; }

        public string AssignedTo { get; set; }

        public DateTime DueDate { get; set; }

        public ComplianceStatus Status { get; set; }

        public DateTime? CompletedAt { get; set; }

        public string CompletionNotes { get; set; }

        [ForeignKey("ViolationId")]
        public virtual ComplianceViolation Violation { get; set; }
    }

    [Table("DataSubjectActions")]
    public class DataSubjectAction
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid RequestId { get; set; }

        [Required]
        [StringLength(200)]
        public string ActionType { get; set; }

        public string Description { get; set; }

        public DateTime ActionDate { get; set; }

        public string PerformedBy { get; set; }

        public string Result { get; set; }

        [ForeignKey("RequestId")]
        public virtual DataSubjectRequest Request { get; set; }
    }

    [Table("DataRetentionRecords")]
    public class DataRetentionRecord
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid PolicyId { get; set; }

        [Required]
        public string DataIdentifier { get; set; }

        public string DataLocation { get; set; }

        public DateTime DataCreated { get; set; }

        public DateTime RetentionExpiry { get; set; }

        public RetentionAction ScheduledAction { get; set; }

        public ComplianceStatus Status { get; set; }

        public DateTime? ActionTaken { get; set; }

        public string ActionResult { get; set; }

        [ForeignKey("PolicyId")]
        public virtual DataRetentionPolicy Policy { get; set; }
    }

    [Table("ControlAssessments")]
    public class ControlAssessment
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid AssessmentId { get; set; }

        [Required]
        public Guid ControlId { get; set; }

        public ComplianceStatus Status { get; set; }

        public int Score { get; set; }

        public string Evidence { get; set; }

        public string Findings { get; set; }

        public string Recommendations { get; set; }

        [ForeignKey("AssessmentId")]
        public virtual ComplianceAssessment Assessment { get; set; }

        [ForeignKey("ControlId")]
        public virtual PolicyControl Control { get; set; }
    }

    [Table("AssessmentFindings")]
    public class AssessmentFinding
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid AssessmentId { get; set; }

        [Required]
        [StringLength(200)]
        public string FindingType { get; set; }

        public ViolationSeverity Severity { get; set; }

        [Required]
        public string Description { get; set; }

        public string Recommendation { get; set; }

        public ComplianceStatus Status { get; set; }

        [ForeignKey("AssessmentId")]
        public virtual ComplianceAssessment Assessment { get; set; }
    }

    [Table("PIAStakeholders")]
    public class PIAStakeholder
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid PIAId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        public string Role { get; set; }

        public string Organization { get; set; }

        public string ContactInfo { get; set; }

        [ForeignKey("PIAId")]
        public virtual PrivacyImpactAssessment PIA { get; set; }
    }

    [Table("PIARisks")]
    public class PIARisk
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid PIAId { get; set; }

        [Required]
        [StringLength(200)]
        public string RiskDescription { get; set; }

        public ViolationSeverity Severity { get; set; }

        public int Likelihood { get; set; }

        public int Impact { get; set; }

        public int RiskScore { get; set; }

        public string Mitigation { get; set; }

        public string ResidualRisk { get; set; }

        [ForeignKey("PIAId")]
        public virtual PrivacyImpactAssessment PIA { get; set; }
    }

    // Compliance Configuration
    public class ComplianceConfiguration
    {
        public bool EnableGDPRCompliance { get; set; } = true;
        public bool EnableHIPAACompliance { get; set; } = false;
        public bool EnableSOXCompliance { get; set; } = false;
        public bool EnablePCICompliance { get; set; } = false;
        public bool EnableAutomaticViolationDetection { get; set; } = true;
        public bool EnableDataSubjectRights { get; set; } = true;
        public bool EnableDataRetentionManagement { get; set; } = true;
        public bool EnablePrivacyImpactAssessments { get; set; } = true;
        
        public int DataSubjectRequestTimeoutDays { get; set; } = 30;
        public int BreachNotificationHours { get; set; } = 72;
        public int ComplianceAssessmentFrequencyDays { get; set; } = 365;
        public int DataRetentionCheckFrequencyDays { get; set; } = 30;
        
        public string DefaultDataController { get; set; } = string.Empty;
        public string DefaultDataProtectionOfficer { get; set; } = string.Empty;
        public string ComplianceEmail { get; set; } = string.Empty;
        public string OrganizationName { get; set; } = string.Empty;
        public string OrganizationAddress { get; set; } = string.Empty;
        
        public Dictionary<string, string> FrameworkSettings { get; set; } = new Dictionary<string, string>();
    }
}