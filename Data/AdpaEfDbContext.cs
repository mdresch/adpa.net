using ADPA.Models.Entities;

using Microsoft.EntityFrameworkCore;

namespace ADPA.Data;

/// <summary>
/// Entity Framework Core 10 Database Context for ADPA
/// Full EF Core implementation with SQL Server support
/// </summary>
public class AdpaEfDbContext : DbContext
{
    public AdpaEfDbContext(DbContextOptions<AdpaEfDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Users table
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Documents table
    /// </summary>
    public DbSet<Document> Documents { get; set; }

    /// <summary>
    /// Processing Results table
    /// </summary>
    public DbSet<ProcessingResult> ProcessingResults { get; set; }

    /// <summary>
    /// Audit Log Entries table (Phase 5.4)
    /// </summary>
    public DbSet<AuditLogEntry> AuditLogEntries { get; set; }

    /// <summary>
    /// Data Lineage Entries table (Phase 5.4)
    /// </summary>
    public DbSet<DataLineageEntry> DataLineageEntries { get; set; }

    /// <summary>
    /// Compliance Audit Entries table (Phase 5.4)
    /// </summary>
    public DbSet<ComplianceAuditEntry> ComplianceAuditEntries { get; set; }

    /// <summary>
    /// Audit Alert Rules table (Phase 5.4)
    /// </summary>
    public DbSet<AuditAlertRule> AuditAlertRules { get; set; }

    /// <summary>
    /// Audit Configurations table (Phase 5.4)
    /// </summary>
    public DbSet<AuditConfiguration> AuditConfigurations { get; set; }

    /// <summary>
    /// Security Incidents table (Phase 5.5)
    /// </summary>
    public DbSet<SecurityIncident> SecurityIncidents { get; set; }

    /// <summary>
    /// Threat Indicators table (Phase 5.5)
    /// </summary>
    public DbSet<ThreatIndicator> ThreatIndicators { get; set; }

    /// <summary>
    /// Security Anomalies table (Phase 5.5)
    /// </summary>
    public DbSet<SecurityAnomaly> SecurityAnomalies { get; set; }

    /// <summary>
    /// Security Monitoring Rules table (Phase 5.5)
    /// </summary>
    public DbSet<SecurityMonitoringRule> SecurityMonitoringRules { get; set; }

    /// <summary>
    /// Security Alerts table (Phase 5.5)
    /// </summary>
    public DbSet<SecurityAlert> SecurityAlerts { get; set; }

    /// <summary>
    /// Security Metrics table (Phase 5.5)
    /// </summary>
    public DbSet<SecurityMetric> SecurityMetrics { get; set; }

    // Compliance Framework Tables (Phase 5.6)
    
    /// <summary>
    /// Compliance Policies table (Phase 5.6)
    /// </summary>
    public DbSet<CompliancePolicy> CompliancePolicies { get; set; }

    /// <summary>
    /// Policy Controls table (Phase 5.6)
    /// </summary>
    public DbSet<PolicyControl> PolicyControls { get; set; }

    /// <summary>
    /// Compliance Violations table (Phase 5.6)
    /// </summary>
    public DbSet<ComplianceViolation> ComplianceViolations { get; set; }

    /// <summary>
    /// Data Subject Requests table (Phase 5.6)
    /// </summary>
    public DbSet<DataSubjectRequest> DataSubjectRequests { get; set; }

    /// <summary>
    /// Data Retention Policies table (Phase 5.6)
    /// </summary>
    public DbSet<DataRetentionPolicy> DataRetentionPolicies { get; set; }

    /// <summary>
    /// Data Retention Records table (Phase 5.6)
    /// </summary>
    public DbSet<DataRetentionRecord> DataRetentionRecords { get; set; }

    /// <summary>
    /// Privacy Impact Assessments table (Phase 5.6)
    /// </summary>
    public DbSet<PrivacyImpactAssessment> PrivacyImpactAssessments { get; set; }

    /// <summary>
    /// Compliance Assessments table (Phase 5.6)
    /// </summary>
    public DbSet<ComplianceAssessment> ComplianceAssessments { get; set; }

    /// <summary>
    /// Violation Actions table (Phase 5.6)
    /// </summary>
    public DbSet<ViolationAction> ViolationActions { get; set; }

    /// <summary>
    /// Data Subject Actions table (Phase 5.6)
    /// </summary>
    public DbSet<DataSubjectAction> DataSubjectActions { get; set; }

    /// <summary>
    /// Control Assessments table (Phase 5.6)
    /// </summary>
    public DbSet<ControlAssessment> ControlAssessments { get; set; }

    /// <summary>
    /// Assessment Findings table (Phase 5.6)
    /// </summary>
    public DbSet<AssessmentFinding> AssessmentFindings { get; set; }

    /// <summary>
    /// PIA Stakeholders table (Phase 5.6)
    /// </summary>
    public DbSet<PIAStakeholder> PIAStakeholders { get; set; }

    /// <summary>
    /// PIA Risks table (Phase 5.6)
    /// </summary>
    public DbSet<PIARisk> PIARisks { get; set; }

    // API Security & Threat Detection entities (Phase 5.7)

    /// <summary>
    /// Rate Limit Policies table (Phase 5.7)
    /// </summary>
    public DbSet<RateLimitPolicy> RateLimitPolicies { get; set; }

    /// <summary>
    /// Rate Limit Violations table (Phase 5.7)
    /// </summary>
    public DbSet<RateLimitViolation> RateLimitViolations { get; set; }

    /// <summary>
    /// Input Validation Rules table (Phase 5.7)
    /// </summary>
    public DbSet<InputValidationRule> InputValidationRules { get; set; }

    /// <summary>
    /// Validation Violations table (Phase 5.7)
    /// </summary>
    public DbSet<ValidationViolation> ValidationViolations { get; set; }

    /// <summary>
    /// API Security Events table (Phase 5.7)
    /// </summary>
    public DbSet<APISecurityEvent> APISecurityEvents { get; set; }

    /// <summary>
    /// API Versions table (Phase 5.7)
    /// </summary>
    public DbSet<APIVersion> APIVersions { get; set; }

    /// <summary>
    /// API Endpoints table (Phase 5.7)
    /// </summary>
    public DbSet<APIEndpoint> APIEndpoints { get; set; }

    /// <summary>
    /// Sanitization Rules table (Phase 5.7)
    /// </summary>
    public DbSet<SanitizationRule> SanitizationRules { get; set; }

    /// <summary>
    /// CORS Policies table (Phase 5.7)
    /// </summary>
    public DbSet<CORSPolicy> CORSPolicies { get; set; }

    /// <summary>
    /// Security Headers table (Phase 5.7)
    /// </summary>
    public DbSet<SecurityHeader> SecurityHeaders { get; set; }

    /// <summary>
    /// API Keys table (Phase 5.7)
    /// </summary>
    public DbSet<APIKey> APIKeys { get; set; }

    /// <summary>
    /// API Key Usage table (Phase 5.7)
    /// </summary>
    public DbSet<APIKeyUsage> APIKeyUsage { get; set; }

  // Security Configuration Management Tables (Phase 5.8) - Commented out until entity models are created
  
  // TODO: Uncomment these DbSets after creating the corresponding entity models in Models/Entities

  /// <summary>
  /// Security Configurations table (Phase 5.8)
  /// </summary>
  public DbSet<SecurityConfiguration> SecurityConfigurations { get; set; }

  /// <summary>
  /// Security Policy Rules table (Phase 5.8)
  /// </summary>
  public DbSet<SecurityPolicyRule> SecurityPolicyRules { get; set; }

  /// <summary>
  /// Security Hardening Guidelines table (Phase 5.8)
  /// </summary>
  public DbSet<SecurityHardeningGuideline> SecurityHardeningGuidelines { get; set; }

  /// <summary>
  /// Hardening Implementations table (Phase 5.8)
  /// </summary>
  public DbSet<HardeningImplementation> HardeningImplementations { get; set; }

  /// <summary>
  /// Vulnerability Scans table (Phase 5.8)
  /// </summary>
  public DbSet<VulnerabilityScan> VulnerabilityScans { get; set; }

  /// <summary>
  /// Vulnerability Findings table (Phase 5.8)
  /// </summary>
  public DbSet<VulnerabilityFinding> VulnerabilityFindings { get; set; }

  /// <summary>
  /// Vulnerability Remediation Actions table (Phase 5.8)
  /// </summary>
  public DbSet<VulnerabilityRemediationAction> VulnerabilityRemediationActions { get; set; }

  /// <summary>
  /// Security Configuration Metrics table (Phase 5.8)
  /// </summary>
  public DbSet<SecurityConfigurationMetric> SecurityConfigurationMetrics { get; set; }

  /// <summary>
  /// Security Metric History table (Phase 5.8)
  /// </summary>
  public DbSet<SecurityMetricHistory> SecurityMetricHistory { get; set; }

  /// <summary>
  /// Security Configuration Audits table (Phase 5.8)
  /// </summary>
  public DbSet<SecurityConfigurationAudit> SecurityConfigurationAudits { get; set; }

  /// <summary>
  /// Policy Rule Violations table (Phase 5.8)
  /// </summary>
  public DbSet<PolicyRuleViolation> PolicyRuleViolations { get; set; }

    /// <summary>
    /// Configure the database schema and relationships
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User entity configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email)
                  .IsRequired()
                  .HasMaxLength(255);
            entity.HasIndex(e => e.Email)
                  .IsUnique();
            entity.Property(e => e.DisplayName)
                  .HasMaxLength(100);
            entity.Property(e => e.Role)
                  .IsRequired()
                  .HasMaxLength(50)
                  .HasDefaultValue("User");
            entity.Property(e => e.IsActive)
                  .HasDefaultValue(true);
            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt)
                  .HasDefaultValueSql("GETUTCDATE()");
        });

        // Document entity configuration
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName)
                  .IsRequired()
                  .HasMaxLength(255);
            entity.Property(e => e.ContentType)
                  .IsRequired()
                  .HasMaxLength(100);
            entity.Property(e => e.FileSize)
                  .IsRequired();
            entity.Property(e => e.BlobPath)
                  .HasMaxLength(500);
            entity.Property(e => e.FileHash)
                  .HasMaxLength(32);
            entity.HasIndex(e => e.FileHash);
            entity.Property(e => e.Status)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.DetectedLanguage)
                  .HasMaxLength(10);
            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.ProcessedAt);

            // Foreign key relationship
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Documents)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Processing Result entity configuration
        modelBuilder.Entity<ProcessingResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProcessingType)
                  .IsRequired()
                  .HasMaxLength(50);
            entity.Property(e => e.ExtractedText)
                  .HasColumnType("ntext");
            entity.Property(e => e.Metadata)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.Analytics)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.ErrorMessage)
                  .HasMaxLength(1000);
            entity.Property(e => e.ProcessingVersion)
                  .HasMaxLength(20);
            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETUTCDATE()");

            // Foreign key relationship
            entity.HasOne(e => e.Document)
                  .WithMany(d => d.ProcessingResults)
                  .HasForeignKey(e => e.DocumentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Audit models (Phase 5.4)
        ConfigureAuditModels(modelBuilder);

        // Seed initial data
        SeedInitialData(modelBuilder);
    }

    /// <summary>
    /// Configure audit models (Phase 5.4)
    /// </summary>
    private void ConfigureAuditModels(ModelBuilder modelBuilder)
    {
        // AuditLogEntry configuration
        modelBuilder.Entity<AuditLogEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType)
                  .IsRequired()
                  .HasMaxLength(100);
            entity.Property(e => e.Category)
                  .IsRequired()
                  .HasMaxLength(100);
            entity.Property(e => e.Severity)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.UserName)
                  .HasMaxLength(100);
            entity.Property(e => e.UserRole)
                  .HasMaxLength(50);
            entity.Property(e => e.SessionId)
                  .HasMaxLength(100);
            entity.Property(e => e.IpAddress)
                  .HasMaxLength(45); // IPv6 support
            entity.Property(e => e.UserAgent)
                  .HasMaxLength(500);
            entity.Property(e => e.RequestId)
                  .HasMaxLength(100);
            entity.Property(e => e.Method)
                  .HasMaxLength(10);
            entity.Property(e => e.Endpoint)
                  .HasMaxLength(500);
            entity.Property(e => e.Action)
                  .HasMaxLength(100);
            entity.Property(e => e.Resource)
                  .HasMaxLength(100);
            entity.Property(e => e.ResourceId)
                  .HasMaxLength(100);
            entity.Property(e => e.Description)
                  .HasMaxLength(1000);
            entity.Property(e => e.ErrorMessage)
                  .HasMaxLength(2000);
            entity.Property(e => e.ErrorCode)
                  .HasMaxLength(50);
            entity.Property(e => e.Metadata)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.BeforeState)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.AfterState)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.SecurityClassification)
                  .HasConversion<string>();
            entity.Property(e => e.ComplianceFramework)
                  .HasMaxLength(50);
            entity.Property(e => e.IntegrityHash)
                  .HasMaxLength(128);
            entity.Property(e => e.GeographicLocation)
                  .HasMaxLength(100);
            entity.Property(e => e.DataCenter)
                  .HasMaxLength(100);

            // Indexes for performance
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.EventType);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.Severity);
            entity.HasIndex(e => e.WasSuccessful);
            entity.HasIndex(e => e.ResourceId);
        });

        // DataLineageEntry configuration
        modelBuilder.Entity<DataLineageEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DataType)
                  .IsRequired()
                  .HasMaxLength(100);
            entity.Property(e => e.DataId)
                  .IsRequired()
                  .HasMaxLength(100);
            entity.Property(e => e.DataSource)
                  .HasMaxLength(200);
            entity.Property(e => e.DataDestination)
                  .HasMaxLength(200);
            entity.Property(e => e.Operation)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.OperationDescription)
                  .HasMaxLength(500);
            entity.Property(e => e.UserName)
                  .HasMaxLength(100);
            entity.Property(e => e.LegalBasis)
                  .HasMaxLength(200);
            entity.Property(e => e.Purpose)
                  .HasMaxLength(500);
            entity.Property(e => e.ConsentVersion)
                  .HasMaxLength(20);
            entity.Property(e => e.SecurityClassification)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.ProcessingLocation)
                  .HasMaxLength(100);
            entity.Property(e => e.TransferMechanism)
                  .HasMaxLength(100);
            entity.Property(e => e.ProcessingMethod)
                  .HasMaxLength(100);
            entity.Property(e => e.EncryptionMethod)
                  .HasMaxLength(100);
            entity.Property(e => e.DataFormat)
                  .HasMaxLength(50);
            entity.Property(e => e.AdditionalMetadata)
                  .HasColumnType("nvarchar(max)");

            // Self-referencing relationship for lineage chain
            entity.HasOne(e => e.ParentLineage)
                  .WithMany(e => e.ChildLineages)
                  .HasForeignKey(e => e.ParentLineageId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.DataType);
            entity.HasIndex(e => e.DataId);
            entity.HasIndex(e => e.Operation);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.IsPersonalData);
            entity.HasIndex(e => e.SecurityClassification);
        });

        // ComplianceAuditEntry configuration
        modelBuilder.Entity<ComplianceAuditEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Framework)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.Requirement)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(e => e.Control)
                  .IsRequired()
                  .HasMaxLength(100);
            entity.Property(e => e.Evidence)
                  .HasMaxLength(2000);
            entity.Property(e => e.Status)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.NonComplianceReason)
                  .HasMaxLength(1000);
            entity.Property(e => e.Severity)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.AssessedBy)
                  .HasMaxLength(100);
            entity.Property(e => e.AssessmentMethod)
                  .HasMaxLength(100);
            entity.Property(e => e.RemediationPlan)
                  .HasMaxLength(2000);
            entity.Property(e => e.RemediationOwner)
                  .HasMaxLength(100);
            entity.Property(e => e.RemediationStatus)
                  .HasConversion<string>();
            entity.Property(e => e.RiskLevel)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.RiskDescription)
                  .HasMaxLength(1000);
            entity.Property(e => e.PotentialFineAmount)
                  .HasPrecision(18, 2);
            entity.Property(e => e.ComplianceData)
                  .HasColumnType("nvarchar(max)");

            // Relationship to AuditLogEntry
            entity.HasOne(e => e.RelatedAuditLog)
                  .WithMany()
                  .HasForeignKey(e => e.RelatedAuditLogId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.Framework);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Severity);
            entity.HasIndex(e => e.RiskLevel);
            entity.HasIndex(e => e.AssessmentDate);
        });

        // AuditAlertRule configuration
        modelBuilder.Entity<AuditAlertRule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(e => e.Description)
                  .HasMaxLength(1000);
            entity.Property(e => e.Conditions)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.Frequency)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.Actions)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.AlertSeverity)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.CreatedBy)
                  .HasMaxLength(100);
            entity.Property(e => e.ModifiedBy)
                  .HasMaxLength(100);

            // Indexes
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.IsEnabled);
            entity.HasIndex(e => e.AlertSeverity);
            entity.HasIndex(e => e.CreatedAt);
        });

        // AuditConfiguration configuration
        modelBuilder.Entity<AuditConfiguration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ConfigurationName)
                  .IsRequired()
                  .HasMaxLength(100);
            entity.Property(e => e.IntegrityAlgorithm)
                  .HasMaxLength(50);
            entity.Property(e => e.CreatedBy)
                  .HasMaxLength(100);
            entity.Property(e => e.ModifiedBy)
                  .HasMaxLength(100);
            entity.Property(e => e.AdditionalSettings)
                  .HasColumnType("nvarchar(max)");

            // Indexes
            entity.HasIndex(e => e.ConfigurationName);
            entity.HasIndex(e => e.IsActive);
        });

        // Configure Security Monitoring models (Phase 5.5)
        ConfigureSecurityMonitoringModels(modelBuilder);
    }

    /// <summary>
    /// Configure security monitoring models (Phase 5.5)
    /// </summary>
    private void ConfigureSecurityMonitoringModels(ModelBuilder modelBuilder)
    {
        // SecurityIncident configuration
        modelBuilder.Entity<SecurityIncident>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.IncidentNumber)
                  .IsRequired()
                  .HasMaxLength(50);
            entity.HasIndex(e => e.IncidentNumber).IsUnique();
            entity.Property(e => e.Title)
                  .IsRequired()
                  .HasMaxLength(500);
            entity.Property(e => e.Description)
                  .HasMaxLength(2000);
            entity.Property(e => e.Severity)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.Type)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.Status)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.Category)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.Source)
                  .HasMaxLength(200);
            entity.Property(e => e.SourceIp)
                  .HasMaxLength(45);
            entity.Property(e => e.TargetResource)
                  .HasMaxLength(500);
            entity.Property(e => e.AffectedUser)
                  .HasMaxLength(100);
            entity.Property(e => e.RiskLevel)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.ImpactAssessment)
                  .HasMaxLength(2000);
            entity.Property(e => e.AssignedTo)
                  .HasMaxLength(100);
            entity.Property(e => e.ResponseTeam)
                  .HasMaxLength(100);
            entity.Property(e => e.ResolutionSummary)
                  .HasMaxLength(2000);
            entity.Property(e => e.IncidentTimeline)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.EvidenceData)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.PublicStatement)
                  .HasMaxLength(2000);

            // Indexes
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.Severity);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.AssignedTo);
            entity.HasIndex(e => e.RiskLevel);
        });

        // ThreatIndicator configuration
        modelBuilder.Entity<ThreatIndicator>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.Value)
                  .IsRequired()
                  .HasMaxLength(1000);
            entity.Property(e => e.Description)
                  .HasMaxLength(2000);
            entity.Property(e => e.Confidence)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.Severity)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.Source)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(e => e.SourceReliability)
                  .HasMaxLength(50);
            entity.Property(e => e.MitreAttackId)
                  .HasMaxLength(20);
            entity.Property(e => e.Campaign)
                  .HasMaxLength(200);
            entity.Property(e => e.Actor)
                  .HasMaxLength(200);
            entity.Property(e => e.Malware)
                  .HasMaxLength(200);
            entity.Property(e => e.AdditionalData)
                  .HasColumnType("nvarchar(max)");

            // Indexes
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.Value);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.Severity);
            entity.HasIndex(e => e.Source);
            entity.HasIndex(e => e.LastSeen);
            entity.HasIndex(e => new { e.Type, e.Value });
        });

        // SecurityAnomaly configuration
        modelBuilder.Entity<SecurityAnomaly>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AnomalyType)
                  .IsRequired()
                  .HasMaxLength(100);
            entity.Property(e => e.Description)
                  .HasMaxLength(1000);
            entity.Property(e => e.Severity)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.Source)
                  .HasMaxLength(200);
            entity.Property(e => e.SourceEntity)
                  .HasMaxLength(200);
            entity.Property(e => e.SourceIp)
                  .HasMaxLength(45);
            entity.Property(e => e.UserName)
                  .HasMaxLength(100);
            entity.Property(e => e.DetectionAlgorithm)
                  .HasMaxLength(100);
            entity.Property(e => e.BaselineProfile)
                  .HasMaxLength(200);
            entity.Property(e => e.ExpectedBehavior)
                  .HasMaxLength(1000);
            entity.Property(e => e.ActualBehavior)
                  .HasMaxLength(1000);
            entity.Property(e => e.AnomalyData)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.StatisticalData)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.Status)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.InvestigatedBy)
                  .HasMaxLength(100);
            entity.Property(e => e.InvestigationNotes)
                  .HasMaxLength(2000);
            entity.Property(e => e.AutomaticResponse)
                  .HasMaxLength(1000);

            // Relationship to SecurityIncident
            entity.HasOne(e => e.RelatedIncident)
                  .WithMany()
                  .HasForeignKey(e => e.RelatedIncidentId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(e => e.DetectedAt);
            entity.HasIndex(e => e.AnomalyType);
            entity.HasIndex(e => e.Severity);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.AnomalyScore);
        });

        // SecurityMonitoringRule configuration
        modelBuilder.Entity<SecurityMonitoringRule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(e => e.Description)
                  .HasMaxLength(1000);
            entity.Property(e => e.Type)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.Severity)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.RuleQuery)
                  .HasMaxLength(2000);
            entity.Property(e => e.RuleConditions)
                  .HasMaxLength(2000);
            entity.Property(e => e.DefaultIncidentSeverity)
                  .HasConversion<string>();
            entity.Property(e => e.CreatedBy)
                  .HasMaxLength(100);
            entity.Property(e => e.ModifiedBy)
                  .HasMaxLength(100);
            entity.Property(e => e.MitreMapping)
                  .HasMaxLength(50);
            entity.Property(e => e.AdditionalConfig)
                  .HasColumnType("nvarchar(max)");

            // Indexes
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.IsEnabled);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.Severity);
            entity.HasIndex(e => e.CreatedAt);
        });

        // SecurityAlert configuration
        modelBuilder.Entity<SecurityAlert>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AlertName)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(e => e.Description)
                  .HasMaxLength(1000);
            entity.Property(e => e.Severity)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.Status)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.RuleName)
                  .HasMaxLength(200);
            entity.Property(e => e.AlertData)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.AssignedTo)
                  .HasMaxLength(100);
            entity.Property(e => e.InvestigationNotes)
                  .HasMaxLength(2000);
            entity.Property(e => e.Disposition)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.AcknowledgedBy)
                  .HasMaxLength(100);
            entity.Property(e => e.ResolvedBy)
                  .HasMaxLength(100);
            entity.Property(e => e.ResolutionReason)
                  .HasMaxLength(1000);
            entity.Property(e => e.EscalatedTo)
                  .HasMaxLength(100);
            entity.Property(e => e.EscalationReason)
                  .HasMaxLength(1000);

            // Relationships
            entity.HasOne(e => e.MonitoringRule)
                  .WithMany()
                  .HasForeignKey(e => e.MonitoringRuleId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.RelatedIncident)
                  .WithMany()
                  .HasForeignKey(e => e.RelatedIncidentId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.Severity);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.AssignedTo);
            entity.HasIndex(e => e.Disposition);
        });

        // SecurityMetric configuration
        modelBuilder.Entity<SecurityMetric>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MetricName)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(e => e.Category)
                  .HasMaxLength(100);
            entity.Property(e => e.Type)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.Unit)
                  .HasMaxLength(50);
            entity.Property(e => e.Description)
                  .HasMaxLength(500);
            entity.Property(e => e.Source)
                  .HasMaxLength(200);
            entity.Property(e => e.AdditionalData)
                  .HasColumnType("nvarchar(max)");

            // Indexes
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.MetricName);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => new { e.MetricName, e.Timestamp });
        });

        // Compliance Framework Configurations (Phase 5.6)
        
        // CompliancePolicy configuration
        modelBuilder.Entity<CompliancePolicy>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(e => e.Description)
                  .HasMaxLength(1000);
            entity.Property(e => e.Framework)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.PolicyType)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.PolicyContent)
                  .IsRequired()
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.RequirementReference)
                  .HasMaxLength(500);
            entity.Property(e => e.CreatedBy)
                  .HasMaxLength(100);
            entity.Property(e => e.ReviewedBy)
                  .HasMaxLength(100);

            // Indexes
            entity.HasIndex(e => e.Framework);
            entity.HasIndex(e => e.PolicyType);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.EffectiveDate);
            entity.HasIndex(e => new { e.Framework, e.IsActive });
        });

        // PolicyControl configuration
        modelBuilder.Entity<PolicyControl>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ControlName)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(e => e.Description)
                  .HasMaxLength(1000);
            entity.Property(e => e.ImplementationGuidance)
                  .IsRequired()
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.ValidationCriteria)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.Status)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.ResponsibleRole)
                  .HasMaxLength(100);
            entity.Property(e => e.Evidence)
                  .HasColumnType("nvarchar(max)");

            // Relationships
            entity.HasOne(e => e.Policy)
                  .WithMany(p => p.Controls)
                  .HasForeignKey(e => e.PolicyId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.PolicyId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.IsAutomated);
            entity.HasIndex(e => e.NextCheckDue);
        });

        // ComplianceViolation configuration
        modelBuilder.Entity<ComplianceViolation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(e => e.Description)
                  .IsRequired()
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.Severity)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.Framework)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.DetectedBy)
                  .HasMaxLength(100);
            entity.Property(e => e.DetectionMethod)
                  .HasMaxLength(200);
            entity.Property(e => e.ResolvedBy)
                  .HasMaxLength(100);
            entity.Property(e => e.ResolutionNotes)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.Status)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.AffectedEntity)
                  .HasMaxLength(500);
            entity.Property(e => e.AffectedData)
                  .HasMaxLength(1000);
            entity.Property(e => e.RiskAssessment)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.RemediationPlan)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.Currency)
                  .HasMaxLength(10);

            // Relationships
            entity.HasOne(e => e.Policy)
                  .WithMany(p => p.Violations)
                  .HasForeignKey(e => e.PolicyId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Control)
                  .WithMany()
                  .HasForeignKey(e => e.ControlId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(e => e.Framework);
            entity.HasIndex(e => e.Severity);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.DetectedAt);
            entity.HasIndex(e => e.RequiresNotification);
            entity.HasIndex(e => e.NotificationDeadline);
            entity.HasIndex(e => new { e.Framework, e.Status });
        });

        // DataSubjectRequest configuration
        modelBuilder.Entity<DataSubjectRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RequestNumber)
                  .IsRequired()
                  .HasMaxLength(100);
            entity.Property(e => e.RequestType)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.SubjectName)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(e => e.SubjectEmail)
                  .IsRequired()
                  .HasMaxLength(255);
            entity.Property(e => e.SubjectPhone)
                  .HasMaxLength(50);
            entity.Property(e => e.IdentityVerification)
                  .HasMaxLength(500);
            entity.Property(e => e.RequestDescription)
                  .IsRequired()
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.Status)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.AssignedTo)
                  .HasMaxLength(100);
            entity.Property(e => e.ResponseMethod)
                  .HasMaxLength(100);
            entity.Property(e => e.ResponseDetails)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.ExtensionReason)
                  .HasMaxLength(500);
            entity.Property(e => e.DataSources)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.ProcessingLegalBasis)
                  .HasMaxLength(500);

            // Indexes
            entity.HasIndex(e => e.RequestNumber).IsUnique();
            entity.HasIndex(e => e.RequestType);
            entity.HasIndex(e => e.SubjectEmail);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.RequestedAt);
            entity.HasIndex(e => e.ResponseDeadline);
        });

        // DataRetentionPolicy configuration
        modelBuilder.Entity<DataRetentionPolicy>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(e => e.Description)
                  .HasMaxLength(1000);
            entity.Property(e => e.DataCategory)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(e => e.ApplicableFramework)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.RetentionCriteria)
                  .HasMaxLength(1000);
            entity.Property(e => e.RetentionAction)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.LegalBasis)
                  .HasMaxLength(500);
            entity.Property(e => e.ResponsibleRole)
                  .HasMaxLength(100);
            entity.Property(e => e.ApprovalWorkflow)
                  .HasMaxLength(500);
            entity.Property(e => e.CreatedBy)
                  .HasMaxLength(100);

            // Indexes
            entity.HasIndex(e => e.DataCategory);
            entity.HasIndex(e => e.ApplicableFramework);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.EffectiveDate);
        });

        // DataRetentionRecord configuration
        modelBuilder.Entity<DataRetentionRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DataIdentifier)
                  .IsRequired()
                  .HasMaxLength(500);
            entity.Property(e => e.DataLocation)
                  .HasMaxLength(500);
            entity.Property(e => e.ScheduledAction)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.Status)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.ActionResult)
                  .HasMaxLength(1000);

            // Relationships
            entity.HasOne(e => e.Policy)
                  .WithMany(p => p.RetentionRecords)
                  .HasForeignKey(e => e.PolicyId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.PolicyId);
            entity.HasIndex(e => e.DataIdentifier);
            entity.HasIndex(e => e.RetentionExpiry);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ScheduledAction);
        });

        // PrivacyImpactAssessment configuration
        modelBuilder.Entity<PrivacyImpactAssessment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(e => e.ProjectName)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(e => e.ProjectDescription)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.DataController)
                  .HasMaxLength(200);
            entity.Property(e => e.DataProcessor)
                  .HasMaxLength(200);
            entity.Property(e => e.ConductedBy)
                  .HasMaxLength(100);
            entity.Property(e => e.Status)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.DataTypes)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.ProcessingPurpose)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.LegalBasis)
                  .HasMaxLength(500);
            entity.Property(e => e.DataSources)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.DataRecipients)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.PrivacyRisks)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.RiskMitigation)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.Recommendations)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.MonitoringPlan)
                  .HasColumnType("nvarchar(max)");

            // Indexes
            entity.HasIndex(e => e.ProjectName);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.AssessmentDate);
            entity.HasIndex(e => e.RiskScore);
            entity.HasIndex(e => e.ReviewDate);
        });

        // ComplianceAssessment configuration
        modelBuilder.Entity<ComplianceAssessment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AssessmentName)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(e => e.Framework)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.AssessorName)
                  .HasMaxLength(100);
            entity.Property(e => e.AssessorOrganization)
                  .HasMaxLength(200);
            entity.Property(e => e.OverallStatus)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.Methodology)
                  .HasMaxLength(200);
            entity.Property(e => e.Scope)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.KeyFindings)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.Recommendations)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.CertificationStatus)
                  .HasMaxLength(100);
            entity.Property(e => e.ExecutiveSummary)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.DetailedReport)
                  .HasColumnType("nvarchar(max)");

            // Relationships
            entity.HasOne(e => e.Policy)
                  .WithMany(p => p.Assessments)
                  .HasForeignKey(e => e.PolicyId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(e => e.Framework);
            entity.HasIndex(e => e.OverallStatus);
            entity.HasIndex(e => e.AssessmentDate);
            entity.HasIndex(e => e.ComplianceScore);
            entity.HasIndex(e => e.NextAssessmentDue);
        });

        // ViolationAction configuration
        modelBuilder.Entity<ViolationAction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ActionType)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(e => e.Description)
                  .IsRequired()
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.AssignedTo)
                  .HasMaxLength(100);
            entity.Property(e => e.Status)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.CompletionNotes)
                  .HasMaxLength(1000);

            // Relationships
            entity.HasOne(e => e.Violation)
                  .WithMany(v => v.Actions)
                  .HasForeignKey(e => e.ViolationId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.ViolationId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.DueDate);
            entity.HasIndex(e => e.AssignedTo);
        });

        // DataSubjectAction configuration
        modelBuilder.Entity<DataSubjectAction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ActionType)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(e => e.Description)
                  .HasMaxLength(1000);
            entity.Property(e => e.PerformedBy)
                  .HasMaxLength(100);
            entity.Property(e => e.Result)
                  .HasMaxLength(1000);

            // Relationships
            entity.HasOne(e => e.Request)
                  .WithMany(r => r.Actions)
                  .HasForeignKey(e => e.RequestId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.RequestId);
            entity.HasIndex(e => e.ActionDate);
            entity.HasIndex(e => e.ActionType);
        });

        // ControlAssessment configuration
        modelBuilder.Entity<ControlAssessment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.Evidence)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.Findings)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.Recommendations)
                  .HasColumnType("nvarchar(max)");

            // Relationships
            entity.HasOne(e => e.Assessment)
                  .WithMany(a => a.ControlAssessments)
                  .HasForeignKey(e => e.AssessmentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Control)
                  .WithMany(c => c.Assessments)
                  .HasForeignKey(e => e.ControlId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.AssessmentId);
            entity.HasIndex(e => e.ControlId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Score);
        });

        // AssessmentFinding configuration
        modelBuilder.Entity<AssessmentFinding>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FindingType)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(e => e.Severity)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.Description)
                  .IsRequired()
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.Recommendation)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.Status)
                  .IsRequired()
                  .HasConversion<string>();

            // Relationships
            entity.HasOne(e => e.Assessment)
                  .WithMany(a => a.Findings)
                  .HasForeignKey(e => e.AssessmentId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.AssessmentId);
            entity.HasIndex(e => e.Severity);
            entity.HasIndex(e => e.Status);
        });

        // PIAStakeholder configuration
        modelBuilder.Entity<PIAStakeholder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(e => e.Role)
                  .HasMaxLength(100);
            entity.Property(e => e.Organization)
                  .HasMaxLength(200);
            entity.Property(e => e.ContactInfo)
                  .HasMaxLength(500);

            // Relationships
            entity.HasOne(e => e.PIA)
                  .WithMany(p => p.Stakeholders)
                  .HasForeignKey(e => e.PIAId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.PIAId);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Role);
        });

        // PIARisk configuration
        modelBuilder.Entity<PIARisk>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RiskDescription)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(e => e.Severity)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.Mitigation)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.ResidualRisk)
                  .HasMaxLength(1000);

            // Relationships
            entity.HasOne(e => e.PIA)
                  .WithMany(p => p.Risks)
                  .HasForeignKey(e => e.PIAId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.PIAId);
            entity.HasIndex(e => e.Severity);
            entity.HasIndex(e => e.RiskScore);
        });
    }

    /// <summary>
    /// Seed initial data for development
    /// </summary>
    private void SeedInitialData(ModelBuilder modelBuilder)
    {
        // Seed admin user
        var adminUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var seedDate = new DateTime(2025, 11, 8, 12, 0, 0, DateTimeKind.Utc);
        
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = adminUserId,
            DisplayName = "Administrator",
            Email = "admin@adpa.local",
            PasswordHash = "admin_hash", // In production, use proper password hashing
            Role = "Admin",
            IsActive = true,
            CreatedAt = seedDate,
            UpdatedAt = seedDate
        });

        // Seed test user
        var testUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = testUserId,
            DisplayName = "Test User",
            Email = "test@adpa.local", 
            PasswordHash = "test_hash", // In production, use proper password hashing
            Role = "User",
            IsActive = true,
            CreatedAt = seedDate,
            UpdatedAt = seedDate
        });

        // API Security & Threat Detection Entity Configurations (Phase 5.7)
        ConfigureAPISecurityEntities(modelBuilder);
    }

    private void ConfigureAPISecurityEntities(ModelBuilder modelBuilder)
    {
        // RateLimitPolicy configuration
        modelBuilder.Entity<RateLimitPolicy>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(e => e.Description)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.Endpoint)
                  .IsRequired()
                  .HasMaxLength(500);
            entity.Property(e => e.Action)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.CreatedBy)
                  .IsRequired()
                  .HasMaxLength(100);
            entity.Property(e => e.ModifiedBy)
                  .HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            entity.HasIndex(e => e.Endpoint);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.Priority);
        });

        // RateLimitViolation configuration
        modelBuilder.Entity<RateLimitViolation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClientIdentifier)
                  .IsRequired()
                  .HasMaxLength(100);
            entity.Property(e => e.IPAddress)
                  .IsRequired()
                  .HasMaxLength(45);
            entity.Property(e => e.Endpoint)
                  .IsRequired()
                  .HasMaxLength(500);
            entity.Property(e => e.ActionTaken)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.ViolationTime)
                  .HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            entity.HasOne(e => e.Policy)
                  .WithMany()
                  .HasForeignKey(e => e.PolicyId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.PolicyId);
            entity.HasIndex(e => e.ClientIdentifier);
            entity.HasIndex(e => e.IPAddress);
            entity.HasIndex(e => e.ViolationTime);
        });

        // InputValidationRule configuration
        modelBuilder.Entity<InputValidationRule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RuleName)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(e => e.Description)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.TargetEndpoint)
                  .IsRequired()
                  .HasMaxLength(500);
            entity.Property(e => e.TargetParameter)
                  .HasMaxLength(100);
            entity.Property(e => e.ValidationType)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.ValidationPattern)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.ValidationMessage)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.Severity)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.CreatedBy)
                  .IsRequired()
                  .HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            entity.HasIndex(e => e.TargetEndpoint);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.Priority);
        });

        // ValidationViolation configuration
        modelBuilder.Entity<ValidationViolation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.IPAddress)
                  .IsRequired()
                  .HasMaxLength(45);
            entity.Property(e => e.Endpoint)
                  .IsRequired()
                  .HasMaxLength(500);
            entity.Property(e => e.Parameter)
                  .IsRequired()
                  .HasMaxLength(100);
            entity.Property(e => e.InputValue)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.Severity)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.ViolationTime)
                  .HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            entity.HasOne(e => e.Rule)
                  .WithMany()
                  .HasForeignKey(e => e.RuleId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.RuleId);
            entity.HasIndex(e => e.IPAddress);
            entity.HasIndex(e => e.ViolationTime);
        });

        // APISecurityEvent configuration
        modelBuilder.Entity<APISecurityEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.IPAddress)
                  .IsRequired()
                  .HasMaxLength(45);
            entity.Property(e => e.UserAgent)
                  .HasMaxLength(1000);
            entity.Property(e => e.Endpoint)
                  .IsRequired()
                  .HasMaxLength(500);
            entity.Property(e => e.Method)
                  .HasMaxLength(10);
            entity.Property(e => e.Severity)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.Description)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.Geolocation)
                  .HasMaxLength(100);
            entity.Property(e => e.EventTime)
                  .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            entity.HasIndex(e => e.EventType);
            entity.HasIndex(e => e.IPAddress);
            entity.HasIndex(e => e.EventTime);
            entity.HasIndex(e => e.Severity);
        });

        // APIVersion configuration
        modelBuilder.Entity<APIVersion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Version)
                  .IsRequired()
                  .HasMaxLength(50);
            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(e => e.Description)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.CreatedBy)
                  .IsRequired()
                  .HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            entity.HasIndex(e => e.Version)
                  .IsUnique();
            entity.HasIndex(e => e.IsActive);
        });

        // APIEndpoint configuration
        modelBuilder.Entity<APIEndpoint>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Path)
                  .IsRequired()
                  .HasMaxLength(500);
            entity.Property(e => e.Method)
                  .IsRequired()
                  .HasMaxLength(10);
            entity.Property(e => e.Description)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.RequiredRoles)
                  .HasMaxLength(500);
            entity.Property(e => e.RequiredPermissions)
                  .HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            entity.HasOne(e => e.Version)
                  .WithMany()
                  .HasForeignKey(e => e.VersionId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.VersionId);
            entity.HasIndex(e => e.Path);
        });

        // SanitizationRule configuration
        modelBuilder.Entity<SanitizationRule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RuleName)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(e => e.Description)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.TargetEndpoint)
                  .IsRequired()
                  .HasMaxLength(500);
            entity.Property(e => e.Level)
                  .IsRequired()
                  .HasConversion<string>();
            entity.Property(e => e.SanitizationPattern)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.ReplacementValue)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.CreatedBy)
                  .IsRequired()
                  .HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            entity.HasIndex(e => e.TargetEndpoint);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.Priority);
        });

        // CORSPolicy configuration
        modelBuilder.Entity<CORSPolicy>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PolicyName)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(e => e.Description)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.AllowedOrigins)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.AllowedMethods)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.AllowedHeaders)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.ExposedHeaders)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.TargetEndpoints)
                  .IsRequired()
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.CreatedBy)
                  .IsRequired()
                  .HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            entity.HasIndex(e => e.PolicyName)
                  .IsUnique();
            entity.HasIndex(e => e.IsActive);
        });

        // SecurityHeader configuration
        modelBuilder.Entity<SecurityHeader>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.HeaderName)
                  .IsRequired()
                  .HasMaxLength(100);
            entity.Property(e => e.HeaderValue)
                  .IsRequired()
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.Description)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.TargetEndpoints)
                  .IsRequired()
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.CreatedBy)
                  .IsRequired()
                  .HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            entity.HasIndex(e => e.HeaderName);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.Priority);
        });

        // APIKey configuration
        modelBuilder.Entity<APIKey>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.KeyName)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(e => e.KeyHash)
                  .IsRequired()
                  .HasMaxLength(100);
            entity.Property(e => e.KeyPrefix)
                  .IsRequired()
                  .HasMaxLength(20);
            entity.Property(e => e.AssignedTo)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(e => e.AllowedEndpoints)
                  .HasColumnType("nvarchar(max)");
            entity.Property(e => e.CreatedBy)
                  .IsRequired()
                  .HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            entity.HasIndex(e => e.KeyHash)
                  .IsUnique();
            entity.HasIndex(e => e.KeyPrefix);
            entity.HasIndex(e => e.IsActive);
        });

        // APIKeyUsage configuration
        modelBuilder.Entity<APIKeyUsage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.IPAddress)
                  .IsRequired()
                  .HasMaxLength(45);
            entity.Property(e => e.Endpoint)
                  .IsRequired()
                  .HasMaxLength(500);
            entity.Property(e => e.UsedAt)
                  .HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            entity.HasOne(e => e.APIKey)
                  .WithMany()
                  .HasForeignKey(e => e.APIKeyId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.APIKeyId);
            entity.HasIndex(e => e.UsedAt);
        });
    }

    /// <summary>
    /// Configure the database connection for development
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Default connection string for development
            // In production, this should come from configuration
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=AdpaDb;Trusted_Connection=true;MultipleActiveResultSets=true;");
        }
    }
}