using ADPA.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ADPA.Models.DTOs.Compliance
{
    // Policy Management DTOs
    public class CompliancePolicyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ComplianceFramework Framework { get; set; }
        public PolicyType PolicyType { get; set; }
        public string PolicyContent { get; set; }
        public string RequirementReference { get; set; }
        public bool IsActive { get; set; }
        public bool IsMandatory { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int Version { get; set; }
        public ComplianceStatus Status { get; set; }
        public int ControlCount { get; set; }
        public int ViolationCount { get; set; }
    }

    public class CreateCompliancePolicyRequest
    {
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

        public bool IsMandatory { get; set; }

        public DateTime EffectiveDate { get; set; }

        public DateTime? ExpiryDate { get; set; }
    }

    public class UpdateCompliancePolicyRequest
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public string PolicyContent { get; set; }

        public string RequirementReference { get; set; }

        public DateTime EffectiveDate { get; set; }

        public DateTime? ExpiryDate { get; set; }
    }

    // Control Management DTOs
    public class PolicyControlDto
    {
        public Guid Id { get; set; }
        public Guid PolicyId { get; set; }
        public string PolicyName { get; set; }
        public string ControlName { get; set; }
        public string Description { get; set; }
        public bool IsAutomated { get; set; }
        public int CheckFrequencyDays { get; set; }
        public ComplianceStatus Status { get; set; }
        public DateTime? LastChecked { get; set; }
        public DateTime? NextCheckDue { get; set; }
        public string ResponsibleRole { get; set; }
        public string Evidence { get; set; }
    }

    public class CreatePolicyControlRequest
    {
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

        public int CheckFrequencyDays { get; set; } = 30;

        public string ResponsibleRole { get; set; }
    }

    // Violation Management DTOs
    public class ComplianceViolationDto
    {
        public Guid Id { get; set; }
        public string PolicyName { get; set; }
        public string ControlName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public ViolationSeverity Severity { get; set; }
        public ComplianceFramework Framework { get; set; }
        public DateTime DetectedAt { get; set; }
        public string DetectedBy { get; set; }
        public ComplianceStatus Status { get; set; }
        public string AffectedEntity { get; set; }
        public int? AffectedRecordCount { get; set; }
        public bool RequiresNotification { get; set; }
        public DateTime? NotificationDeadline { get; set; }
        public bool IsNotified { get; set; }
        public decimal? PotentialFine { get; set; }
        public string Currency { get; set; }
        public int ActionCount { get; set; }
        public int DaysOpen { get; set; }
    }

    public class CreateViolationRequest
    {
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

        public string AffectedEntity { get; set; }

        public string AffectedData { get; set; }

        public int? AffectedRecordCount { get; set; }

        public string RiskAssessment { get; set; }

        public decimal? PotentialFine { get; set; }

        public string Currency { get; set; }
    }

    public class ResolveViolationRequest
    {
        [Required]
        public string ResolutionNotes { get; set; }

        public string RemediationActions { get; set; }

        public bool PreventRecurrence { get; set; }
    }

    // Data Subject Rights DTOs
    public class DataSubjectRequestDto
    {
        public Guid Id { get; set; }
        public string RequestNumber { get; set; }
        public DataSubjectRights RequestType { get; set; }
        public string SubjectName { get; set; }
        public string SubjectEmail { get; set; }
        public string RequestDescription { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ResponseDeadline { get; set; }
        public ComplianceStatus Status { get; set; }
        public string AssignedTo { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsVerified { get; set; }
        public bool IsComplexRequest { get; set; }
        public int ExtensionDays { get; set; }
        public int DaysRemaining { get; set; }
    }

    public class CreateDataSubjectRequestRequest
    {
        [Required]
        public DataSubjectRights RequestType { get; set; }

        [Required]
        [StringLength(200)]
        public string SubjectName { get; set; }

        [Required]
        [EmailAddress]
        public string SubjectEmail { get; set; }

        public string SubjectPhone { get; set; }

        [Required]
        public string RequestDescription { get; set; }

        public string IdentityVerification { get; set; }

        public bool IsComplexRequest { get; set; }

        public string ProcessingLegalBasis { get; set; }
    }

    public class ProcessDataSubjectRequestRequest
    {
        public string ResponseMethod { get; set; } = "Email";

        public string ResponseDetails { get; set; }

        public bool RequiresExtension { get; set; }

        public int ExtensionDays { get; set; }

        public string ExtensionReason { get; set; }
    }

    // Data Retention DTOs
    public class DataRetentionPolicyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DataCategory { get; set; }
        public ComplianceFramework ApplicableFramework { get; set; }
        public int RetentionPeriodDays { get; set; }
        public RetentionAction RetentionAction { get; set; }
        public string LegalBasis { get; set; }
        public bool IsActive { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string ResponsibleRole { get; set; }
        public int ActiveRecords { get; set; }
        public int ExpiringRecords { get; set; }
    }

    public class CreateRetentionPolicyRequest
    {
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

        public DateTime EffectiveDate { get; set; }

        public string ResponsibleRole { get; set; }
    }

    public class DataRetentionRecordDto
    {
        public Guid Id { get; set; }
        public string PolicyName { get; set; }
        public string DataIdentifier { get; set; }
        public string DataLocation { get; set; }
        public DateTime DataCreated { get; set; }
        public DateTime RetentionExpiry { get; set; }
        public RetentionAction ScheduledAction { get; set; }
        public ComplianceStatus Status { get; set; }
        public DateTime? ActionTaken { get; set; }
        public string ActionResult { get; set; }
        public int DaysUntilExpiry { get; set; }
    }

    // Privacy Impact Assessment DTOs
    public class PrivacyImpactAssessmentDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string ProjectName { get; set; }
        public string ProjectDescription { get; set; }
        public string DataController { get; set; }
        public DateTime AssessmentDate { get; set; }
        public string ConductedBy { get; set; }
        public ComplianceStatus Status { get; set; }
        public bool InvolvesSpecialCategories { get; set; }
        public bool InvolvesChildren { get; set; }
        public bool InvolvesProfiling { get; set; }
        public int RiskScore { get; set; }
        public DateTime? ReviewDate { get; set; }
        public bool RequiresMonitoring { get; set; }
        public int StakeholderCount { get; set; }
        public int RiskCount { get; set; }
    }

    public class CreatePIARequest
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string ProjectName { get; set; }

        public string ProjectDescription { get; set; }

        public string DataController { get; set; }

        public string DataProcessor { get; set; }

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
    }

    // Compliance Assessment DTOs
    public class ComplianceAssessmentDto
    {
        public Guid Id { get; set; }
        public string AssessmentName { get; set; }
        public ComplianceFramework Framework { get; set; }
        public string PolicyName { get; set; }
        public DateTime AssessmentDate { get; set; }
        public string AssessorName { get; set; }
        public string AssessorOrganization { get; set; }
        public ComplianceStatus OverallStatus { get; set; }
        public int ComplianceScore { get; set; }
        public string Methodology { get; set; }
        public string Scope { get; set; }
        public DateTime? NextAssessmentDue { get; set; }
        public string CertificationStatus { get; set; }
        public DateTime? CertificationExpiry { get; set; }
        public int ControlAssessmentCount { get; set; }
        public int FindingCount { get; set; }
    }

    public class CreateAssessmentRequest
    {
        [Required]
        [StringLength(200)]
        public string AssessmentName { get; set; }

        [Required]
        public ComplianceFramework Framework { get; set; }

        public Guid? PolicyId { get; set; }

        public string AssessorOrganization { get; set; }

        public string Methodology { get; set; }

        public string Scope { get; set; }

        public DateTime? NextAssessmentDue { get; set; }
    }

    // Search and Filter DTOs
    public class ComplianceSearchRequest
    {
        public ComplianceFramework? Framework { get; set; }
        public ComplianceStatus? Status { get; set; }
        public ViolationSeverity? Severity { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SortBy { get; set; } = "CreatedAt";
        public string SortDirection { get; set; } = "desc";
    }

    public class ViolationSearchRequest : ComplianceSearchRequest
    {
        public bool? RequiresNotification { get; set; }
        public bool? IsNotified { get; set; }
        public string AffectedEntity { get; set; }
        public int? MinAffectedRecords { get; set; }
        public int? MaxAffectedRecords { get; set; }
    }

    public class DataSubjectRequestSearchRequest
    {
        public DataSubjectRights? RequestType { get; set; }
        public ComplianceStatus? Status { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsOverdue { get; set; }
        public string AssignedTo { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // Dashboard and Reporting DTOs
    public class ComplianceDashboardData
    {
        public ComplianceOverview Overview { get; set; }
        public List<ComplianceFrameworkStatus> FrameworkStatuses { get; set; }
        public List<ViolationTrendData> ViolationTrends { get; set; }
        public List<DataSubjectRequestSummary> DataSubjectRequestSummary { get; set; }
        public List<UpcomingDeadline> UpcomingDeadlines { get; set; }
        public ComplianceMetrics Metrics { get; set; }
    }

    public class ComplianceOverview
    {
        public int TotalPolicies { get; set; }
        public int ActivePolicies { get; set; }
        public int TotalViolations { get; set; }
        public int ActiveViolations { get; set; }
        public int CriticalViolations { get; set; }
        public int PendingDataSubjectRequests { get; set; }
        public int OverdueRequests { get; set; }
        public int ComplianceScore { get; set; }
        public ComplianceStatus OverallStatus { get; set; }
    }

    public class ComplianceFrameworkStatus
    {
        public ComplianceFramework Framework { get; set; }
        public string FrameworkName { get; set; }
        public bool IsEnabled { get; set; }
        public ComplianceStatus Status { get; set; }
        public int Score { get; set; }
        public int PolicyCount { get; set; }
        public int ViolationCount { get; set; }
        public DateTime? LastAssessment { get; set; }
        public DateTime? NextAssessment { get; set; }
    }

    public class ViolationTrendData
    {
        public DateTime Date { get; set; }
        public int TotalViolations { get; set; }
        public int CriticalViolations { get; set; }
        public int HighViolations { get; set; }
        public int MediumViolations { get; set; }
        public int LowViolations { get; set; }
        public int ResolvedViolations { get; set; }
    }

    public class DataSubjectRequestSummary
    {
        public DataSubjectRights RequestType { get; set; }
        public int PendingCount { get; set; }
        public int InProgressCount { get; set; }
        public int CompletedCount { get; set; }
        public int OverdueCount { get; set; }
        public double AverageProcessingDays { get; set; }
    }

    public class UpcomingDeadline
    {
        public string Type { get; set; }
        public string Title { get; set; }
        public DateTime Deadline { get; set; }
        public int DaysRemaining { get; set; }
        public ViolationSeverity Priority { get; set; }
        public string AssignedTo { get; set; }
        public Guid EntityId { get; set; }
    }

    public class ComplianceMetrics
    {
        public double AverageViolationResolutionDays { get; set; }
        public double AverageDataSubjectResponseDays { get; set; }
        public double PolicyComplianceRate { get; set; }
        public double AutomatedControlRate { get; set; }
        public int TotalDataRetentionRecords { get; set; }
        public int ExpiringDataRecords { get; set; }
        public int PrivacyImpactAssessments { get; set; }
        public decimal PotentialFines { get; set; }
        public string Currency { get; set; }
    }

    // Breach Reporting DTOs
    public class BreachReportRequest
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public ViolationSeverity Severity { get; set; }

        public ComplianceFramework Framework { get; set; } = ComplianceFramework.GDPR;

        public string AffectedEntity { get; set; }

        public string AffectedData { get; set; }

        public int AffectedRecordCount { get; set; }

        public string RiskAssessment { get; set; }

        public string ImmediateActions { get; set; }

        public bool RequiresRegulatoryNotification { get; set; }

        public bool RequiresDataSubjectNotification { get; set; }

        public decimal? EstimatedFine { get; set; }

        public string Currency { get; set; } = "USD";
    }

    public class BreachNotificationRequest
    {
        [Required]
        public Guid ViolationId { get; set; }

        public bool NotifyRegulatoryAuthorities { get; set; }

        public bool NotifyDataSubjects { get; set; }

        public string NotificationMethod { get; set; }

        public string AdditionalDetails { get; set; }
    }

    // Configuration DTOs
    public class ComplianceConfigurationDto
    {
        public bool EnableGDPRCompliance { get; set; }
        public bool EnableHIPAACompliance { get; set; }
        public bool EnableSOXCompliance { get; set; }
        public bool EnablePCICompliance { get; set; }
        public bool EnableAutomaticViolationDetection { get; set; }
        public bool EnableDataSubjectRights { get; set; }
        public bool EnableDataRetentionManagement { get; set; }
        public bool EnablePrivacyImpactAssessments { get; set; }

        public int DataSubjectRequestTimeoutDays { get; set; }
        public int BreachNotificationHours { get; set; }
        public int ComplianceAssessmentFrequencyDays { get; set; }
        public int DataRetentionCheckFrequencyDays { get; set; }

        public string DefaultDataController { get; set; }
        public string DefaultDataProtectionOfficer { get; set; }
        public string ComplianceEmail { get; set; }
        public string OrganizationName { get; set; }
        public string OrganizationAddress { get; set; }

        public Dictionary<string, string> FrameworkSettings { get; set; }
    }
}