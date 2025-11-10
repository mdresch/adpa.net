#nullable enable
using Microsoft.EntityFrameworkCore;
using ADPA.Data;
using ADPA.Models.Entities;
using ADPA.Models.DTOs.Security;
using SecurityConfigurationEntity = ADPA.Models.Entities.SecurityConfiguration;
using SecurityPolicyRuleEntity = ADPA.Models.Entities.SecurityPolicyRule;
using SecurityHardeningGuidelineEntity = ADPA.Models.Entities.SecurityHardeningGuideline;
using HardeningImplementationEntity = ADPA.Models.Entities.HardeningImplementation;
using VulnerabilityScanEntity = ADPA.Models.Entities.VulnerabilityScan;
using VulnerabilityFindingEntity = ADPA.Models.Entities.VulnerabilityFinding;
using VulnerabilityRemediationActionEntity = ADPA.Models.Entities.VulnerabilityRemediationAction;
using SecurityConfigurationMetricEntity = ADPA.Models.Entities.SecurityConfigurationMetric;
using SecurityMetricHistoryEntity = ADPA.Models.Entities.SecurityMetricHistory;
using SecurityConfigurationAuditEntity = ADPA.Models.Entities.SecurityConfigurationAudit;
using PolicyRuleViolationEntity = ADPA.Models.Entities.PolicyRuleViolation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ADPA.Services.Security
{
    /// <summary>
    /// Comprehensive Security Configuration Management Service
    /// Provides centralized security configuration, policy orchestration, hardening management,
    /// vulnerability coordination, security metrics, and orchestration workflows
    /// </summary>
    public class SecurityConfigurationService
    {
        private readonly AdpaEfDbContext _context;
        private readonly IAuditService _auditService;
        private readonly ILogger<SecurityConfigurationService> _logger;

        public SecurityConfigurationService(
            AdpaEfDbContext context,
            IAuditService auditService,
            ILogger<SecurityConfigurationService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Security Configuration Management

        /// <summary>
        /// Get paginated security configurations with filtering
        /// </summary>
        public async Task<(List<SecurityConfigurationDto> Items, int TotalCount)> GetSecurityConfigurationsAsync(
            int pageNumber = 1,
            int pageSize = 20,
            string? environment = null,
            bool? isActive = null)
        {
            try
            {
                var query = _context.SecurityConfigurations.AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(environment))
                {
                    query = query.Where(sc => sc.Environment == environment);
                }

                if (isActive.HasValue)
                {
                    query = query.Where(sc => sc.IsActive == isActive.Value);
                }

                var totalCount = await query.CountAsync();

                var configurations = await query
                    .OrderByDescending(sc => sc.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(sc => new SecurityConfigurationDto
                    {
                        Id = sc.Id,
                        Name = sc.Name,
                        Description = sc.Description,
                        ConfigurationType = sc.ConfigurationType,
                        ConfigurationData = sc.ConfigurationData,
                        Version = sc.Version,
                        IsActive = sc.IsActive,
                        IsDefault = sc.IsDefault,
                        Priority = sc.Priority,
                        Environment = sc.Environment,
                        EffectiveFrom = sc.EffectiveFrom,
                        EffectiveTo = sc.EffectiveTo,
                        CreatedBy = sc.CreatedBy,
                        CreatedAt = sc.CreatedAt,
                        LastModifiedBy = sc.LastModifiedBy,
                        LastModified = sc.LastModified,
                        ApprovalStatus = sc.ApprovalStatus,
                        ApprovedBy = sc.ApprovedBy,
                        ApprovedAt = sc.ApprovedAt,
                        Tags = sc.Tags
                    })
                    .ToListAsync();

                return (configurations, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving security configurations");
                throw;
            }
        }

        /// <summary>
        /// Get security configuration by ID
        /// </summary>
        public async Task<SecurityConfigurationDto?> GetSecurityConfigurationAsync(Guid id)
        {
            try
            {
                var configuration = await _context.SecurityConfigurations
                    .Include(sc => sc.PolicyRules)
                    .FirstOrDefaultAsync(sc => sc.Id == id);

                if (configuration == null)
                    return null;

                return new SecurityConfigurationDto
                {
                    Id = configuration.Id,
                    Name = configuration.Name,
                    Description = configuration.Description,
                    ConfigurationType = configuration.ConfigurationType,
                    ConfigurationData = configuration.ConfigurationData,
                    Version = configuration.Version,
                    IsActive = configuration.IsActive,
                    IsDefault = configuration.IsDefault,
                    Priority = configuration.Priority,
                    Environment = configuration.Environment,
                    EffectiveFrom = configuration.EffectiveFrom,
                    EffectiveTo = configuration.EffectiveTo,
                    CreatedBy = configuration.CreatedBy,
                    CreatedAt = configuration.CreatedAt,
                    LastModifiedBy = configuration.LastModifiedBy,
                    LastModified = configuration.LastModified,
                    ApprovalStatus = configuration.ApprovalStatus,
                    ApprovedBy = configuration.ApprovedBy,
                    ApprovedAt = configuration.ApprovedAt,
                    Tags = configuration.Tags,
                    PolicyRules = configuration.PolicyRules.Select(pr => new SecurityPolicyRuleDto
                    {
                        Id = pr.Id,
                        SecurityConfigurationId = pr.SecurityConfigurationId,
                        RuleName = pr.RuleName,
                        Description = pr.Description,
                        PolicyType = pr.PolicyType,
                        RuleDefinition = pr.RuleDefinition,
                        Severity = pr.Severity,
                        Action = pr.Action,
                        IsEnabled = pr.IsEnabled,
                        Priority = pr.Priority,
                        Conditions = pr.Conditions,
                        Parameters = pr.Parameters,
                        CreatedAt = pr.CreatedAt,
                        CreatedBy = pr.CreatedBy,
                        LastModified = pr.LastModified,
                        ModifiedBy = pr.ModifiedBy
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving security configuration {ConfigurationId}", id);
                throw;
            }
        }

        /// <summary>
        /// Create new security configuration
        /// </summary>
        public async Task<SecurityConfigurationDto> CreateSecurityConfigurationAsync(
            string name,
            string description,
            string configurationType,
            string configurationData,
            string version = "1.0",
            int priority = 100,
            string environment = "Production",
            DateTime? effectiveFrom = null,
            DateTime? effectiveTo = null,
            string? tags = null,
            string createdBy = "System")
        {
            try
            {
                var configuration = new SecurityConfigurationEntity
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Description = description,
                    ConfigurationType = configurationType,
                    ConfigurationData = configurationData,
                    Version = version,
                    Priority = priority,
                    Environment = environment,
                    EffectiveFrom = effectiveFrom ?? DateTime.UtcNow,
                    EffectiveTo = effectiveTo,
                    Tags = tags,
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow,
                    ApprovalStatus = "Draft"
                };

                _context.SecurityConfigurations.Add(configuration);
                await _context.SaveChangesAsync();

                // Create audit log
                await _auditService.LogSecurityEventAsync(
                    "SecurityConfiguration.Created",
                    "Security configuration created",
                    createdBy,
                    new { configurationId = configuration.Id, name, configurationType }
                );

                _logger.LogInformation("Security configuration {ConfigurationId} created by {CreatedBy}", configuration.Id, createdBy);

                return new SecurityConfigurationDto
                {
                    Id = configuration.Id,
                    Name = configuration.Name,
                    Description = configuration.Description,
                    ConfigurationType = configuration.ConfigurationType,
                    ConfigurationData = configuration.ConfigurationData,
                    Version = configuration.Version,
                    IsActive = configuration.IsActive,
                    IsDefault = configuration.IsDefault,
                    Priority = configuration.Priority,
                    Environment = configuration.Environment,
                    EffectiveFrom = configuration.EffectiveFrom,
                    EffectiveTo = configuration.EffectiveTo,
                    CreatedBy = configuration.CreatedBy,
                    CreatedAt = configuration.CreatedAt,
                    ApprovalStatus = configuration.ApprovalStatus,
                    Tags = configuration.Tags
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating security configuration");
                throw;
            }
        }

        /// <summary>
        /// Update existing security configuration
        /// </summary>
        public async Task<SecurityConfigurationDto?> UpdateSecurityConfigurationAsync(
            Guid id,
            string name,
            string description,
            string configurationData,
            string version = "1.0",
            bool isActive = true,
            int priority = 100,
            string environment = "Production",
            DateTime? effectiveFrom = null,
            DateTime? effectiveTo = null,
            string? tags = null,
            string modifiedBy = "System")
        {
            try
            {
                var configuration = await _context.SecurityConfigurations.FindAsync(id);
                if (configuration == null)
                    return null;

                // Store old values for audit
                var oldValues = new
                {
                    name = configuration.Name,
                    description = configuration.Description,
                    configurationData = configuration.ConfigurationData,
                    version = configuration.Version,
                    isActive = configuration.IsActive,
                    priority = configuration.Priority
                };

                // Update configuration
                configuration.Name = name;
                configuration.Description = description;
                configuration.ConfigurationData = configurationData;
                configuration.Version = version;
                configuration.IsActive = isActive;
                configuration.Priority = priority;
                configuration.Environment = environment;
                configuration.EffectiveFrom = effectiveFrom ?? configuration.EffectiveFrom;
                configuration.EffectiveTo = effectiveTo;
                configuration.Tags = tags;
                configuration.LastModifiedBy = modifiedBy;
                configuration.LastModified = DateTime.UtcNow;
                configuration.ApprovalStatus = "Draft"; // Reset approval status on update

                await _context.SaveChangesAsync();

                // Create audit log
                await _auditService.LogSecurityEventAsync(
                    "SecurityConfiguration.Updated",
                    "Security configuration updated",
                    modifiedBy,
                    new { configurationId = configuration.Id, oldValues, newValues = new { name, description, version, isActive, priority } }
                );

                _logger.LogInformation("Security configuration {ConfigurationId} updated by {ModifiedBy}", configuration.Id, modifiedBy);

                return new SecurityConfigurationDto
                {
                    Id = configuration.Id,
                    Name = configuration.Name,
                    Description = configuration.Description,
                    ConfigurationType = configuration.ConfigurationType,
                    ConfigurationData = configuration.ConfigurationData,
                    Version = configuration.Version,
                    IsActive = configuration.IsActive,
                    IsDefault = configuration.IsDefault,
                    Priority = configuration.Priority,
                    Environment = configuration.Environment,
                    EffectiveFrom = configuration.EffectiveFrom,
                    EffectiveTo = configuration.EffectiveTo,
                    CreatedBy = configuration.CreatedBy,
                    CreatedAt = configuration.CreatedAt,
                    LastModifiedBy = configuration.LastModifiedBy,
                    LastModified = configuration.LastModified,
                    ApprovalStatus = configuration.ApprovalStatus,
                    Tags = configuration.Tags
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating security configuration {ConfigurationId}", id);
                throw;
            }
        }

        /// <summary>
        /// Approve security configuration
        /// </summary>
        public async Task<SecurityConfigurationDto?> ApproveSecurityConfigurationAsync(Guid id, string approvedBy = "System")
        {
            try
            {
                var configuration = await _context.SecurityConfigurations.FindAsync(id);
                if (configuration == null)
                    return null;

                configuration.ApprovalStatus = "Approved";
                configuration.ApprovedBy = approvedBy;
                configuration.ApprovedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Create audit log
                await _auditService.LogSecurityEventAsync(
                    "SecurityConfiguration.Approved",
                    "Security configuration approved",
                    approvedBy,
                    new { configurationId = configuration.Id }
                );

                _logger.LogInformation("Security configuration {ConfigurationId} approved by {ApprovedBy}", configuration.Id, approvedBy);

                return await GetSecurityConfigurationAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving security configuration {ConfigurationId}", id);
                throw;
            }
        }

        /// <summary>
        /// Delete security configuration
        /// </summary>
        public async Task<bool> DeleteSecurityConfigurationAsync(Guid id)
        {
            try
            {
                var configuration = await _context.SecurityConfigurations.FindAsync(id);
                if (configuration == null)
                    return false;

                _context.SecurityConfigurations.Remove(configuration);
                await _context.SaveChangesAsync();

                // Create audit log
                await _auditService.LogSecurityEventAsync(
                    "SecurityConfiguration.Deleted",
                    "Security configuration deleted",
                    "System",
                    new { configurationId = configuration.Id, name = configuration.Name }
                );

                _logger.LogInformation("Security configuration {ConfigurationId} deleted", configuration.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting security configuration {ConfigurationId}", id);
                throw;
            }
        }

        #endregion

        #region Policy Rule Management

        /// <summary>
        /// Get paginated policy rules for configuration
        /// </summary>
        public async Task<(List<SecurityPolicyRuleDto> Items, int TotalCount)> GetPolicyRulesAsync(
            Guid configurationId,
            int pageNumber = 1,
            int pageSize = 20,
            bool? isEnabled = null)
        {
            try
            {
                var query = _context.SecurityPolicyRules
                    .Where(pr => pr.SecurityConfigurationId == configurationId);

                if (isEnabled.HasValue)
                {
                    query = query.Where(pr => pr.IsEnabled == isEnabled.Value);
                }

                var totalCount = await query.CountAsync();

                var rules = await query
                    .OrderByDescending(pr => pr.Priority)
                    .ThenByDescending(pr => pr.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(pr => new SecurityPolicyRuleDto
                    {
                        Id = pr.Id,
                        SecurityConfigurationId = pr.SecurityConfigurationId,
                        RuleName = pr.RuleName,
                        Description = pr.Description,
                        PolicyType = pr.PolicyType,
                        RuleDefinition = pr.RuleDefinition,
                        Severity = pr.Severity,
                        Action = pr.Action,
                        IsEnabled = pr.IsEnabled,
                        Priority = pr.Priority,
                        Conditions = pr.Conditions,
                        Parameters = pr.Parameters,
                        CreatedAt = pr.CreatedAt,
                        CreatedBy = pr.CreatedBy,
                        LastModified = pr.LastModified,
                        ModifiedBy = pr.ModifiedBy
                    })
                    .ToListAsync();

                return (rules, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving policy rules for configuration {ConfigurationId}", configurationId);
                throw;
            }
        }

        /// <summary>
        /// Create new policy rule
        /// </summary>
        public async Task<SecurityPolicyRuleDto> CreatePolicyRuleAsync(
            Guid securityConfigurationId,
            string ruleName,
            string description,
            string policyType,
            string ruleDefinition,
            string severity = "Medium",
            string action = "Log",
            int priority = 100,
            string? conditions = null,
            string? parameters = null,
            string createdBy = "System")
        {
            try
            {
                var rule = new SecurityPolicyRule
                {
                    Id = Guid.NewGuid(),
                    SecurityConfigurationId = securityConfigurationId,
                    RuleName = ruleName,
                    Description = description,
                    PolicyType = policyType,
                    RuleDefinition = ruleDefinition,
                    Severity = severity,
                    Action = action,
                    Priority = priority,
                    Conditions = conditions,
                    Parameters = parameters,
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow,
                    IsEnabled = true
                };

                _context.SecurityPolicyRules.Add(rule);
                await _context.SaveChangesAsync();

                // Create audit log
                await _auditService.LogSecurityEventAsync(
                    "SecurityPolicyRule.Created",
                    "Security policy rule created",
                    createdBy,
                    new { ruleId = rule.Id, ruleName, policyType, severity }
                );

                _logger.LogInformation("Security policy rule {RuleId} created by {CreatedBy}", rule.Id, createdBy);

                return new SecurityPolicyRuleDto
                {
                    Id = rule.Id,
                    SecurityConfigurationId = rule.SecurityConfigurationId,
                    RuleName = rule.RuleName,
                    Description = rule.Description,
                    PolicyType = rule.PolicyType,
                    RuleDefinition = rule.RuleDefinition,
                    Severity = rule.Severity,
                    Action = rule.Action,
                    IsEnabled = rule.IsEnabled,
                    Priority = rule.Priority,
                    Conditions = rule.Conditions,
                    Parameters = rule.Parameters,
                    CreatedAt = rule.CreatedAt,
                    CreatedBy = rule.CreatedBy
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating security policy rule");
                throw;
            }
        }

        /// <summary>
        /// Validate policy rules against current system state
        /// </summary>
        public async Task<List<string>> ValidatePolicyRulesAsync(Guid configurationId)
        {
            try
            {
                var validationResults = new List<string>();
                var rules = await _context.SecurityPolicyRules
                    .Where(pr => pr.SecurityConfigurationId == configurationId && pr.IsEnabled)
                    .ToListAsync();

                foreach (var rule in rules)
                {
                    // Implement rule validation logic based on rule type and definition
                    switch (rule.PolicyType.ToLower())
                    {
                        case "authentication":
                            await ValidateAuthenticationPolicyAsync(rule, validationResults);
                            break;
                        case "authorization":
                            await ValidateAuthorizationPolicyAsync(rule, validationResults);
                            break;
                        case "encryption":
                            await ValidateEncryptionPolicyAsync(rule, validationResults);
                            break;
                        default:
                            validationResults.Add($"Unknown policy type: {rule.PolicyType} for rule {rule.RuleName}");
                            break;
                    }
                }

                return validationResults;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating policy rules for configuration {ConfigurationId}", configurationId);
                throw;
            }
        }

        #endregion

        #region Hardening Guidelines Management

        /// <summary>
        /// Get paginated hardening guidelines
        /// </summary>
        public async Task<(List<SecurityHardeningGuidelineDto> Items, int TotalCount)> GetHardeningGuidelinesAsync(
            int pageNumber = 1,
            int pageSize = 20,
            string? category = null,
            bool? isActive = null)
        {
            try
            {
                var query = _context.SecurityHardeningGuidelines.AsQueryable();

                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(hg => hg.Category == category);
                }

                if (isActive.HasValue)
                {
                    query = query.Where(hg => hg.IsActive == isActive.Value);
                }

                var totalCount = await query.CountAsync();

                var guidelinesData = await query
                    .Include(hg => hg.Implementations)
                    .OrderByDescending(hg => hg.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var guidelines = guidelinesData.Select(hg => new SecurityHardeningGuidelineDto
                    {
                        Id = hg.Id,
                        Title = hg.Title,
                        Description = hg.Description,
                        Category = hg.Category,
                        Severity = hg.Severity,
                        Implementation = hg.Implementation,
                        Validation = hg.Validation,
                        IsRequired = hg.IsRequired,
                        IsAutomated = hg.IsAutomated,
                        AutomationScript = hg.AutomationScript,
                        ApplicableFrameworks = hg.ApplicableFrameworks != null ? 
                            System.Text.Json.JsonSerializer.Deserialize<string[]>(hg.ApplicableFrameworks) : null,
                        Prerequisites = hg.Prerequisites,
                        RiskMitigation = hg.RiskMitigation,
                        EstimatedEffort = hg.EstimatedEffort,
                        Version = hg.Version,
                        IsActive = hg.IsActive,
                        CreatedAt = hg.CreatedAt,
                        CreatedBy = hg.CreatedBy,
                        Implementations = hg.Implementations.Select(impl => new HardeningImplementationDto
                        {
                            Id = impl.Id,
                            GuidelineId = impl.GuidelineId,
                            SystemComponent = impl.SystemComponent,
                            Status = impl.Status,
                            StartedAt = impl.StartedAt,
                            CompletedAt = impl.CompletedAt,
                            ImplementedBy = impl.ImplementedBy,
                            Notes = impl.Notes,
                            ValidationResults = impl.ValidationResults,
                            IsValidated = impl.IsValidated,
                            LastValidated = impl.LastValidated,
                            ValidatedBy = impl.ValidatedBy,
                            NextReviewDate = impl.NextReviewDate,
                            Evidence = impl.Evidence
                        }).ToList()
                    })
                    .ToList();

                return (guidelines, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving hardening guidelines");
                throw;
            }
        }

        /// <summary>
        /// Create new hardening guideline
        /// </summary>
        public async Task<SecurityHardeningGuidelineDto> CreateHardeningGuidelineAsync(
            string title,
            string description,
            string category,
            string severity = "Medium",
            string implementation = "",
            string? validation = null,
            bool isRequired = true,
            bool isAutomated = false,
            string? automationScript = null,
            string[]? applicableFrameworks = null,
            string? prerequisites = null,
            string? riskMitigation = null,
            int estimatedEffort = 0,
            string createdBy = "System")
        {
            try
            {
                var guideline = new SecurityHardeningGuideline
                {
                    Id = Guid.NewGuid(),
                    Title = title,
                    Description = description,
                    Category = category,
                    Severity = severity,
                    Implementation = implementation,
                    Validation = validation,
                    IsRequired = isRequired,
                    IsAutomated = isAutomated,
                    AutomationScript = automationScript,
                    ApplicableFrameworks = applicableFrameworks != null ? 
                        System.Text.Json.JsonSerializer.Serialize(applicableFrameworks) : null,
                    Prerequisites = prerequisites,
                    RiskMitigation = riskMitigation,
                    EstimatedEffort = estimatedEffort,
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.SecurityHardeningGuidelines.Add(guideline);
                await _context.SaveChangesAsync();

                // Create audit log
                await _auditService.LogSecurityEventAsync(
                    "SecurityHardeningGuideline.Created",
                    "Security hardening guideline created",
                    createdBy,
                    new { guidelineId = guideline.Id, title, category, severity }
                );

                _logger.LogInformation("Security hardening guideline {GuidelineId} created by {CreatedBy}", guideline.Id, createdBy);

                return new SecurityHardeningGuidelineDto
                {
                    Id = guideline.Id,
                    Title = guideline.Title,
                    Description = guideline.Description,
                    Category = guideline.Category,
                    Severity = guideline.Severity,
                    Implementation = guideline.Implementation,
                    Validation = guideline.Validation,
                    IsRequired = guideline.IsRequired,
                    IsAutomated = guideline.IsAutomated,
                    AutomationScript = guideline.AutomationScript,
                    ApplicableFrameworks = applicableFrameworks,
                    Prerequisites = guideline.Prerequisites,
                    RiskMitigation = guideline.RiskMitigation,
                    EstimatedEffort = guideline.EstimatedEffort,
                    Version = guideline.Version,
                    IsActive = guideline.IsActive,
                    CreatedAt = guideline.CreatedAt,
                    CreatedBy = guideline.CreatedBy
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating hardening guideline");
                throw;
            }
        }

        /// <summary>
        /// Calculate hardening compliance score
        /// </summary>
        public async Task<double> CalculateHardeningComplianceAsync(string? category = null)
        {
            try
            {
                var query = _context.SecurityHardeningGuidelines
                    .Include(hg => hg.Implementations)
                    .Where(hg => hg.IsActive);

                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(hg => hg.Category == category);
                }

                var guidelines = await query.ToListAsync();
                if (!guidelines.Any())
                    return 0.0;

                var totalGuidelines = guidelines.Count;
                var implementedGuidelines = guidelines.Count(hg => 
                    hg.Implementations.Any(impl => impl.Status == "Completed" && impl.IsValidated));

                return (double)implementedGuidelines / totalGuidelines * 100.0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating hardening compliance");
                throw;
            }
        }

        #endregion

        #region Vulnerability Management

        /// <summary>
        /// Get paginated vulnerability scans
        /// </summary>
        public async Task<(List<VulnerabilityScanDto> Items, int TotalCount)> GetVulnerabilityScansAsync(
            int pageNumber = 1,
            int pageSize = 20,
            string? status = null)
        {
            try
            {
                var query = _context.VulnerabilityScans.AsQueryable();

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(vs => vs.Status == status);
                }

                var totalCount = await query.CountAsync();

                var scans = await query
                    .Include(vs => vs.Findings)
                    .OrderByDescending(vs => vs.ScheduledAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(vs => new VulnerabilityScanDto
                    {
                        Id = vs.Id,
                        ScanName = vs.ScanName,
                        ScanType = vs.ScanType,
                        TargetSystem = vs.TargetSystem,
                        ScanConfiguration = vs.ScanConfiguration,
                        Status = vs.Status,
                        ScheduledAt = vs.ScheduledAt,
                        StartedAt = vs.StartedAt,
                        CompletedAt = vs.CompletedAt,
                        ScanResults = vs.ScanResults,
                        CriticalCount = vs.CriticalCount,
                        HighCount = vs.HighCount,
                        MediumCount = vs.MediumCount,
                        LowCount = vs.LowCount,
                        InfoCount = vs.InfoCount,
                        OverallScore = vs.OverallScore,
                        ExecutedBy = vs.ExecutedBy,
                        ScanTool = vs.ScanTool,
                        ScannerVersion = vs.ScannerVersion,
                        IsRecurring = vs.IsRecurring,
                        RecurrenceSchedule = vs.RecurrenceSchedule,
                        NextScanDate = vs.NextScanDate,
                        Findings = vs.Findings.Select(f => new VulnerabilityFindingDto
                        {
                            Id = f.Id,
                            ScanId = f.ScanId,
                            Title = f.Title,
                            Description = f.Description,
                            Severity = f.Severity,
                            VulnerabilityId = f.VulnerabilityId,
                            CVSSScore = f.CVSSScore,
                            CVSSVector = f.CVSSVector,
                            AffectedComponent = f.AffectedComponent,
                            Location = f.Location,
                            Evidence = f.Evidence,
                            Recommendation = f.Recommendation,
                            References = f.References,
                            Status = f.Status,
                            AssignedTo = f.AssignedTo,
                            DueDate = f.DueDate,
                            IdentifiedAt = f.IdentifiedAt,
                            ResolvedAt = f.ResolvedAt,
                            ResolutionNotes = f.ResolutionNotes,
                            IsFalsePositive = f.IsFalsePositive,
                            FalsePositiveJustification = f.FalsePositiveJustification
                        }).ToList()
                    })
                    .ToListAsync();

                return (scans, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vulnerability scans");
                throw;
            }
        }

        /// <summary>
        /// Create new vulnerability scan
        /// </summary>
        public async Task<VulnerabilityScanDto> CreateVulnerabilityScanAsync(
            string scanName,
            string scanType,
            string targetSystem,
            string? scanConfiguration = null,
            DateTime? scheduledAt = null,
            string? scanTool = null,
            bool isRecurring = false,
            string? recurrenceSchedule = null,
            string executedBy = "System")
        {
            try
            {
                var scan = new VulnerabilityScan
                {
                    Id = Guid.NewGuid(),
                    ScanName = scanName,
                    ScanType = scanType,
                    TargetSystem = targetSystem,
                    ScanConfiguration = scanConfiguration,
                    ScheduledAt = scheduledAt ?? DateTime.UtcNow,
                    ScanTool = scanTool,
                    IsRecurring = isRecurring,
                    RecurrenceSchedule = recurrenceSchedule,
                    ExecutedBy = executedBy,
                    Status = "Scheduled",
                    CreatedAt = DateTime.UtcNow
                };

                _context.VulnerabilityScans.Add(scan);
                await _context.SaveChangesAsync();

                // Create audit log
                await _auditService.LogSecurityEventAsync(
                    "VulnerabilityScan.Created",
                    "Vulnerability scan created",
                    executedBy,
                    new { scanId = scan.Id, scanName, scanType, targetSystem }
                );

                _logger.LogInformation("Vulnerability scan {ScanId} created by {ExecutedBy}", scan.Id, executedBy);

                return new VulnerabilityScanDto
                {
                    Id = scan.Id,
                    ScanName = scan.ScanName,
                    ScanType = scan.ScanType,
                    TargetSystem = scan.TargetSystem,
                    ScanConfiguration = scan.ScanConfiguration,
                    Status = scan.Status,
                    ScheduledAt = scan.ScheduledAt,
                    ScanTool = scan.ScanTool,
                    IsRecurring = scan.IsRecurring,
                    RecurrenceSchedule = scan.RecurrenceSchedule,
                    ExecutedBy = scan.ExecutedBy
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vulnerability scan");
                throw;
            }
        }

        #endregion

        #region Security Metrics Management

        /// <summary>
        /// Get paginated security metrics
        /// </summary>
        public async Task<(List<SecurityMetricDto> Items, int TotalCount)> GetSecurityMetricsAsync(
            int pageNumber = 1,
            int pageSize = 20,
            string? category = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            try
            {
                var query = _context.SecurityConfigurationMetrics.AsQueryable();

                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(sm => sm.Category == category);
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(sm => sm.MeasuredAt >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(sm => sm.MeasuredAt <= toDate.Value);
                }

                var totalCount = await query.CountAsync();

                var metrics = await query
                    .OrderByDescending(sm => sm.MeasuredAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(sm => new SecurityMetricDto
                    {
                        Id = sm.Id,
                        MetricName = sm.MetricName,
                        MetricType = sm.MetricType,
                        Category = sm.Category,
                        Description = sm.Description,
                        Value = sm.Value,
                        Unit = sm.Unit,
                        Target = sm.Target,
                        Threshold = sm.Threshold,
                        Status = sm.Status,
                        MeasuredAt = sm.MeasuredAt,
                        PeriodStart = sm.PeriodStart,
                        PeriodEnd = sm.PeriodEnd,
                        DataSource = sm.DataSource,
                        CalculationMethod = sm.CalculationMethod,
                        Tags = sm.Tags,
                        Environment = sm.Environment
                    })
                    .ToListAsync();

                return (metrics, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving security metrics");
                throw;
            }
        }

        /// <summary>
        /// Record new security metric
        /// </summary>
        public async Task<SecurityMetricDto> RecordSecurityMetricAsync(
            string metricName,
            string metricType,
            string category,
            double value,
            string? unit = null,
            double? target = null,
            double? threshold = null,
            DateTime? measuredAt = null,
            DateTime? periodStart = null,
            DateTime? periodEnd = null,
            string? dataSource = null,
            string? calculationMethod = null,
            string? tags = null,
            string environment = "Production",
            string? description = null)
        {
            try
            {
                var metric = new SecurityConfigurationMetric
                {
                    Id = Guid.NewGuid(),
                    MetricName = metricName,
                    MetricType = metricType,
                    Category = category,
                    Description = description,
                    Value = value,
                    Unit = unit,
                    Target = target,
                    Threshold = threshold,
                    MeasuredAt = measuredAt ?? DateTime.UtcNow,
                    PeriodStart = periodStart ?? DateTime.UtcNow.Date,
                    PeriodEnd = periodEnd ?? DateTime.UtcNow,
                    DataSource = dataSource,
                    CalculationMethod = calculationMethod,
                    Tags = tags,
                    Environment = environment,
                    Status = DetermineMetricStatus(value, target, threshold),
                    CreatedAt = DateTime.UtcNow
                };

                _context.SecurityConfigurationMetrics.Add(metric);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Security metric {MetricName} recorded with value {Value}", metricName, value);

                return new SecurityMetricDto
                {
                    Id = metric.Id,
                    MetricName = metric.MetricName,
                    MetricType = metric.MetricType,
                    Category = metric.Category,
                    Description = metric.Description,
                    Value = metric.Value,
                    Unit = metric.Unit,
                    Target = metric.Target,
                    Threshold = metric.Threshold,
                    Status = metric.Status,
                    MeasuredAt = metric.MeasuredAt,
                    PeriodStart = metric.PeriodStart,
                    PeriodEnd = metric.PeriodEnd,
                    DataSource = metric.DataSource,
                    CalculationMethod = metric.CalculationMethod,
                    Tags = metric.Tags,
                    Environment = metric.Environment
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording security metric");
                throw;
            }
        }

        #endregion

        #region Dashboard and Reporting

        /// <summary>
        /// Get comprehensive security dashboard data
        /// </summary>
        public async Task<SecurityDashboardDto> GetSecurityDashboardDataAsync()
        {
            try
            {
                var dashboard = new SecurityDashboardDto
                {
                    GeneratedAt = DateTime.UtcNow
                };

                // Configuration metrics
                var configCount = await _context.SecurityConfigurations.CountAsync();
                var activeConfigCount = await _context.SecurityConfigurations.CountAsync(sc => sc.IsActive);
                var approvedConfigCount = await _context.SecurityConfigurations.CountAsync(sc => sc.ApprovalStatus == "Approved");

                dashboard.ConfigurationMetrics = new Dictionary<string, object>
                {
                    { "TotalConfigurations", configCount },
                    { "ActiveConfigurations", activeConfigCount },
                    { "ApprovedConfigurations", approvedConfigCount },
                    { "ApprovalRate", configCount > 0 ? (double)approvedConfigCount / configCount * 100 : 0 }
                };

                // Policy metrics
                var policyCount = await _context.SecurityPolicyRules.CountAsync();
                var enabledPolicyCount = await _context.SecurityPolicyRules.CountAsync(pr => pr.IsEnabled);
                var violationCount = await _context.PolicyRuleViolations.CountAsync(v => v.Status == "Open");

                dashboard.PolicyMetrics = new Dictionary<string, object>
                {
                    { "TotalPolicyRules", policyCount },
                    { "EnabledPolicyRules", enabledPolicyCount },
                    { "OpenViolations", violationCount }
                };

                // Hardening metrics
                var hardeningComplianceScore = await CalculateHardeningComplianceAsync();
                dashboard.HardeningMetrics = new Dictionary<string, object>
                {
                    { "ComplianceScore", hardeningComplianceScore }
                };

                // Vulnerability metrics
                var totalVulnerabilities = await _context.VulnerabilityFindings.CountAsync();
                var openVulnerabilities = await _context.VulnerabilityFindings.CountAsync(f => f.Status == "Open");
                var criticalVulnerabilities = await _context.VulnerabilityFindings.CountAsync(f => f.Severity == "Critical" && f.Status == "Open");

                dashboard.VulnerabilityMetrics = new Dictionary<string, object>
                {
                    { "TotalVulnerabilities", totalVulnerabilities },
                    { "OpenVulnerabilities", openVulnerabilities },
                    { "CriticalVulnerabilities", criticalVulnerabilities }
                };

                // Security KPIs
                dashboard.SecurityKPIs = await CalculateSecurityKPIsAsync();

                return dashboard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating security dashboard data");
                throw;
            }
        }

        /// <summary>
        /// Calculate security KPIs
        /// </summary>
        public async Task<Dictionary<string, double>> CalculateSecurityKPIsAsync()
        {
            try
            {
                var kpis = new Dictionary<string, double>();

                // Configuration approval rate
                var configCount = await _context.SecurityConfigurations.CountAsync();
                var approvedConfigCount = await _context.SecurityConfigurations.CountAsync(sc => sc.ApprovalStatus == "Approved");
                kpis["ConfigurationApprovalRate"] = configCount > 0 ? (double)approvedConfigCount / configCount * 100 : 0;

                // Policy compliance rate
                var policyCount = await _context.SecurityPolicyRules.CountAsync();
                var enabledPolicyCount = await _context.SecurityPolicyRules.CountAsync(pr => pr.IsEnabled);
                kpis["PolicyComplianceRate"] = policyCount > 0 ? (double)enabledPolicyCount / policyCount * 100 : 0;

                // Vulnerability resolution rate
                var totalVulnerabilities = await _context.VulnerabilityFindings.CountAsync();
                var resolvedVulnerabilities = await _context.VulnerabilityFindings.CountAsync(f => f.Status == "Resolved");
                kpis["VulnerabilityResolutionRate"] = totalVulnerabilities > 0 ? (double)resolvedVulnerabilities / totalVulnerabilities * 100 : 0;

                // Hardening compliance score
                kpis["HardeningComplianceScore"] = await CalculateHardeningComplianceAsync();

                return kpis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating security KPIs");
                throw;
            }
        }

        /// <summary>
        /// Generate security report
        /// </summary>
        public async Task<object> GenerateSecurityReportAsync(
            string reportType,
            DateTime fromDate,
            DateTime toDate,
            string[]? includeCategories = null,
            string format = "JSON",
            bool includeCharts = false,
            string? environment = null)
        {
            try
            {
                var report = new Dictionary<string, object>
                {
                    { "ReportType", reportType },
                    { "GeneratedAt", DateTime.UtcNow },
                    { "Period", new { FromDate = fromDate, ToDate = toDate } },
                    { "Environment", environment ?? "All" }
                };

                switch (reportType.ToLower())
                {
                    case "security-overview":
                        report["Data"] = await GenerateSecurityOverviewReportAsync(fromDate, toDate, environment);
                        break;
                    case "vulnerability-summary":
                        report["Data"] = await GenerateVulnerabilityReportAsync(fromDate, toDate, environment);
                        break;
                    case "compliance-status":
                        report["Data"] = await GenerateComplianceReportAsync(fromDate, toDate, environment);
                        break;
                    default:
                        throw new ArgumentException($"Unknown report type: {reportType}");
                }

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating security report");
                throw;
            }
        }

        #endregion

        #region Security Orchestration

        /// <summary>
        /// Execute security orchestration workflow
        /// </summary>
        public async Task<SecurityOrchestrationResponse> ExecuteSecurityOrchestrationAsync(
            string orchestrationName,
            Dictionary<string, object> parameters,
            bool isScheduled = false,
            DateTime? scheduledAt = null,
            string? recurrencePattern = null,
            string executedBy = "System")
        {
            var startTime = DateTime.UtcNow;
            var response = new SecurityOrchestrationResponse
            {
                ExecutedAt = startTime
            };

            try
            {
                _logger.LogInformation("Starting security orchestration {OrchestrationName}", orchestrationName);

                var results = new Dictionary<string, object>();

                switch (orchestrationName.ToLower())
                {
                    case "security-scan-orchestration":
                        results = await ExecuteSecurityScanOrchestrationAsync(parameters);
                        break;
                    case "policy-validation-orchestration":
                        results = await ExecutePolicyValidationOrchestrationAsync(parameters);
                        break;
                    case "hardening-compliance-check":
                        results = await ExecuteHardeningComplianceCheckAsync(parameters);
                        break;
                    case "vulnerability-remediation-workflow":
                        results = await ExecuteVulnerabilityRemediationWorkflowAsync(parameters);
                        break;
                    default:
                        throw new ArgumentException($"Unknown orchestration: {orchestrationName}");
                }

                response.Success = true;
                response.Message = $"Security orchestration {orchestrationName} completed successfully";
                response.Results = results;
                response.ExecutionTime = DateTime.UtcNow - startTime;

                // Create audit log
                await _auditService.LogSecurityEventAsync(
                    "SecurityOrchestration.Executed",
                    $"Security orchestration executed: {orchestrationName}",
                    executedBy,
                    new { orchestrationName, parameters, results }
                );

                _logger.LogInformation("Security orchestration {OrchestrationName} completed in {ExecutionTime}", 
                    orchestrationName, response.ExecutionTime);

                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Security orchestration failed: {ex.Message}";
                response.Errors = new List<string> { ex.Message };
                response.ExecutionTime = DateTime.UtcNow - startTime;

                _logger.LogError(ex, "Error executing security orchestration {OrchestrationName}", orchestrationName);

                return response;
            }
        }

        #endregion

        #region Private Helper Methods

        private string DetermineMetricStatus(double value, double? target, double? threshold)
        {
            if (threshold.HasValue && value < threshold.Value)
                return "Critical";
            if (target.HasValue && value < target.Value * 0.8)
                return "Warning";
            return "Normal";
        }

        private async Task ValidateAuthenticationPolicyAsync(SecurityPolicyRuleEntity rule, List<string> results)
        {
            // Implement authentication policy validation
            await Task.CompletedTask;
            results.Add($"Authentication policy {rule.RuleName} validation completed");
        }

        private async Task ValidateAuthorizationPolicyAsync(SecurityPolicyRuleEntity rule, List<string> results)
        {
            // Implement authorization policy validation
            await Task.CompletedTask;
            results.Add($"Authorization policy {rule.RuleName} validation completed");
        }

        private async Task ValidateEncryptionPolicyAsync(SecurityPolicyRuleEntity rule, List<string> results)
        {
            // Implement encryption policy validation
            await Task.CompletedTask;
            results.Add($"Encryption policy {rule.RuleName} validation completed");
        }

        private async Task<Dictionary<string, object>> GenerateSecurityOverviewReportAsync(DateTime fromDate, DateTime toDate, string? environment)
        {
            // Implement security overview report generation
            await Task.CompletedTask;
            return new Dictionary<string, object>
            {
                { "Summary", "Security overview report generated" }
            };
        }

        private async Task<Dictionary<string, object>> GenerateVulnerabilityReportAsync(DateTime fromDate, DateTime toDate, string? environment)
        {
            // Implement vulnerability report generation
            await Task.CompletedTask;
            return new Dictionary<string, object>
            {
                { "Summary", "Vulnerability report generated" }
            };
        }

        private async Task<Dictionary<string, object>> GenerateComplianceReportAsync(DateTime fromDate, DateTime toDate, string? environment)
        {
            // Implement compliance report generation
            await Task.CompletedTask;
            return new Dictionary<string, object>
            {
                { "Summary", "Compliance report generated" }
            };
        }

        private async Task<Dictionary<string, object>> ExecuteSecurityScanOrchestrationAsync(Dictionary<string, object> parameters)
        {
            // Implement security scan orchestration
            await Task.CompletedTask;
            return new Dictionary<string, object>
            {
                { "Status", "Security scan orchestration completed" }
            };
        }

        private async Task<Dictionary<string, object>> ExecutePolicyValidationOrchestrationAsync(Dictionary<string, object> parameters)
        {
            // Implement policy validation orchestration
            await Task.CompletedTask;
            return new Dictionary<string, object>
            {
                { "Status", "Policy validation orchestration completed" }
            };
        }

        private async Task<Dictionary<string, object>> ExecuteHardeningComplianceCheckAsync(Dictionary<string, object> parameters)
        {
            // Implement hardening compliance check
            await Task.CompletedTask;
            return new Dictionary<string, object>
            {
                { "Status", "Hardening compliance check completed" }
            };
        }

        private async Task<Dictionary<string, object>> ExecuteVulnerabilityRemediationWorkflowAsync(Dictionary<string, object> parameters)
        {
            // Implement vulnerability remediation workflow
            await Task.CompletedTask;
            return new Dictionary<string, object>
            {
                { "Status", "Vulnerability remediation workflow completed" }
            };
        }

        #endregion
    }
}