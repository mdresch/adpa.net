using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ADPA.Services.Security;
using ADPA.Models.DTOs;
using ADPA.Models.DTOs.Security;
using ADPA.Models.Entities;

namespace ADPA.Controllers;

/// <summary>
/// Phase 5.8: Security Configuration Management Controller
/// Comprehensive REST API for managing all 11 SecurityConfiguration entities:
/// SecurityConfiguration, SecurityPolicyRule, SecurityHardeningGuideline, HardeningImplementation,
/// VulnerabilityScan, VulnerabilityFinding, VulnerabilityRemediationAction, 
/// SecurityConfigurationMetric, SecurityMetricHistory, SecurityConfigurationAudit, PolicyRuleViolation
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class SecurityConfigurationController : ControllerBase
{
    private readonly SecurityConfigurationService _securityConfigService;
    private readonly ILogger<SecurityConfigurationController> _logger;

    public SecurityConfigurationController(
        SecurityConfigurationService securityConfigService,
        ILogger<SecurityConfigurationController> logger)
    {
        _securityConfigService = securityConfigService ?? throw new ArgumentNullException(nameof(securityConfigService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Security Configuration Management

    /// <summary>
    /// Get paginated security configurations with filtering
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,SecurityManager,SecurityAnalyst")]
    public async Task<ActionResult<SecurityConfigurationPagedResponse>> GetSecurityConfigurations(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? environment = null,
        [FromQuery] bool? isActive = null)
    {
        try
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var (items, totalCount) = await _securityConfigService.GetSecurityConfigurationsAsync(
                pageNumber, pageSize, environment, isActive);

            var response = new SecurityConfigurationPagedResponse
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get security configurations");
            return StatusCode(500, new { message = "Failed to retrieve security configurations" });
        }
    }

    /// <summary>
    /// Get specific security configuration by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,SecurityManager,SecurityAnalyst")]
    public async Task<ActionResult<SecurityConfigurationDto>> GetSecurityConfiguration(Guid id)
    {
        try
        {
            var configuration = await _securityConfigService.GetSecurityConfigurationAsync(id);
            
            if (configuration == null)
                return NotFound(new { message = "Security configuration not found" });

            return Ok(configuration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get security configuration {ConfigurationId}", id);
            return StatusCode(500, new { message = "Failed to retrieve security configuration" });
        }
    }

    /// <summary>
    /// Create new security configuration
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,SecurityManager")]
    public async Task<ActionResult<SecurityConfigurationDto>> CreateSecurityConfiguration(
        [FromBody] CreateSecurityConfigurationRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User?.Identity?.Name ?? "Anonymous";
            
            var configuration = await _securityConfigService.CreateSecurityConfigurationAsync(
                request.Name,
                request.Description,
                request.ConfigurationType,
                request.ConfigurationData,
                request.Version ?? "1.0",
                request.Priority ?? 100,
                request.Environment ?? "Production",
                request.EffectiveFrom,
                request.EffectiveTo,
                request.Tags,
                userId);

            return CreatedAtAction(
                nameof(GetSecurityConfiguration),
                new { id = configuration.Id },
                configuration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create security configuration");
            return StatusCode(500, new { message = "Failed to create security configuration" });
        }
    }

    /// <summary>
    /// Update existing security configuration
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,SecurityManager")]
    public async Task<ActionResult<SecurityConfigurationDto>> UpdateSecurityConfiguration(
        Guid id, [FromBody] UpdateSecurityConfigurationRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User?.Identity?.Name ?? "Anonymous";

            var configuration = await _securityConfigService.UpdateSecurityConfigurationAsync(
                id,
                request.Name,
                request.Description,
                request.ConfigurationData,
                request.Version ?? "1.0",
                request.IsActive ?? true,
                request.Priority ?? 100,
                request.Environment ?? "Production",
                request.EffectiveFrom,
                request.EffectiveTo,
                request.Tags,
                userId);

            if (configuration == null)
                return NotFound(new { message = "Security configuration not found" });

            return Ok(configuration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update security configuration {ConfigurationId}", id);
            return StatusCode(500, new { message = "Failed to update security configuration" });
        }
    }

    /// <summary>
    /// Approve security configuration
    /// </summary>
    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin,SecurityManager")]
    public async Task<ActionResult<SecurityConfigurationDto>> ApproveSecurityConfiguration(Guid id)
    {
        try
        {
            var userId = User?.Identity?.Name ?? "Anonymous";
            
            var configuration = await _securityConfigService.ApproveSecurityConfigurationAsync(id, userId);
            
            if (configuration == null)
                return NotFound(new { message = "Security configuration not found" });

            return Ok(configuration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to approve security configuration {ConfigurationId}", id);
            return StatusCode(500, new { message = "Failed to approve security configuration" });
        }
    }

    /// <summary>
    /// Delete security configuration
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteSecurityConfiguration(Guid id)
    {
        try
        {
            var success = await _securityConfigService.DeleteSecurityConfigurationAsync(id);
            
            if (!success)
                return NotFound(new { message = "Security configuration not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete security configuration {ConfigurationId}", id);
            return StatusCode(500, new { message = "Failed to delete security configuration" });
        }
    }

    #endregion

    #region Security Policy Rules

    /// <summary>
    /// Get paginated policy rules for a security configuration
    /// </summary>
    [HttpGet("{configurationId}/policy-rules")]
    [Authorize(Roles = "Admin,SecurityManager,SecurityAnalyst")]
    public async Task<ActionResult<SecurityPolicyRulePagedResponse>> GetPolicyRules(
        Guid configurationId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? isEnabled = null)
    {
        try
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var (items, totalCount) = await _securityConfigService.GetPolicyRulesAsync(
                configurationId, pageNumber, pageSize, isEnabled);

            var response = new SecurityPolicyRulePagedResponse
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get policy rules for configuration {ConfigurationId}", configurationId);
            return StatusCode(500, new { message = "Failed to retrieve policy rules" });
        }
    }

    /// <summary>
    /// Create new policy rule for a security configuration
    /// </summary>
    [HttpPost("{configurationId}/policy-rules")]
    [Authorize(Roles = "Admin,SecurityManager")]
    public async Task<ActionResult<SecurityPolicyRuleDto>> CreatePolicyRule(
        Guid configurationId, [FromBody] CreatePolicyRuleRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User?.Identity?.Name ?? "Anonymous";

            var policyRule = await _securityConfigService.CreatePolicyRuleAsync(
                configurationId,
                request.RuleName,
                request.Description,
                request.PolicyType,
                request.RuleDefinition,
                request.Severity ?? "Medium",
                request.Action ?? "Log",
                request.Priority ?? 100,
                request.Conditions,
                request.Parameters,
                userId);

            return CreatedAtAction(
                nameof(GetPolicyRules),
                new { configurationId },
                policyRule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create policy rule for configuration {ConfigurationId}", configurationId);
            return StatusCode(500, new { message = "Failed to create policy rule" });
        }
    }

    #endregion

    #region Security Hardening Guidelines

    /// <summary>
    /// Get hardening guidelines - placeholder for future implementation
    /// </summary>
    [HttpGet("{configurationId}/hardening-guidelines")]
    [Authorize(Roles = "Admin,SecurityManager,SecurityAnalyst")]
    public async Task<ActionResult<List<SecurityHardeningGuidelineDto>>> GetHardeningGuidelines(Guid configurationId)
    {
        try
        {
            // TODO: Implement when service methods are available
            await Task.CompletedTask;
            return Ok(new List<SecurityHardeningGuidelineDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get hardening guidelines for configuration {ConfigurationId}", configurationId);
            return StatusCode(500, new { message = "Failed to retrieve hardening guidelines" });
        }
    }

    #endregion

    #region Vulnerability Management

    /// <summary>
    /// Get vulnerability scans - placeholder for future implementation
    /// </summary>
    [HttpGet("{configurationId}/vulnerability-scans")]
    [Authorize(Roles = "Admin,SecurityManager,SecurityAnalyst")]
    public async Task<ActionResult<List<VulnerabilityScanDto>>> GetVulnerabilityScans(Guid configurationId)
    {
        try
        {
            // TODO: Implement when service methods are available
            await Task.CompletedTask;
            return Ok(new List<VulnerabilityScanDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get vulnerability scans for configuration {ConfigurationId}", configurationId);
            return StatusCode(500, new { message = "Failed to retrieve vulnerability scans" });
        }
    }

    #endregion

    #region Security Metrics

    /// <summary>
    /// Get security metrics - placeholder for future implementation
    /// </summary>
    [HttpGet("{configurationId}/metrics")]
    [Authorize(Roles = "Admin,SecurityManager,SecurityAnalyst")]
    public async Task<ActionResult<SecurityMetricsDashboard>> GetSecurityMetrics(Guid configurationId)
    {
        try
        {
            // TODO: Implement when service methods are available
            await Task.CompletedTask;
            return Ok(new SecurityMetricsDashboard { ConfigurationId = configurationId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get security metrics for configuration {ConfigurationId}", configurationId);
            return StatusCode(500, new { message = "Failed to retrieve security metrics" });
        }
    }

    #endregion

    #region Security Orchestration

    /// <summary>
    /// Execute security orchestration workflow
    /// </summary>
    [HttpPost("orchestration/execute")]
    [Authorize(Roles = "Admin,SecurityManager")]
    public async Task<ActionResult<SecurityOrchestrationResponse>> ExecuteSecurityOrchestration(
        [FromBody] ExecuteOrchestrationRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User?.Identity?.Name ?? "Anonymous";

            var response = await _securityConfigService.ExecuteSecurityOrchestrationAsync(
                request.OrchestrationName,
                request.Parameters ?? new Dictionary<string, object>(),
                request.IsScheduled ?? false,
                request.ScheduledAt,
                request.RecurrencePattern,
                userId);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute security orchestration {OrchestrationName}", request.OrchestrationName);
            return StatusCode(500, new { message = "Failed to execute security orchestration" });
        }
    }

    #endregion

    #region Reports

    /// <summary>
    /// Generate security configuration report - placeholder for future implementation
    /// </summary>
    [HttpPost("reports/generate")]
    [Authorize(Roles = "Admin,SecurityManager,SecurityAnalyst")]
    public async Task<ActionResult<SecurityReportResponse>> GenerateSecurityReport(
        [FromBody] GenerateReportRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // TODO: Implement when service methods are available
            await Task.CompletedTask;
            
            var report = new SecurityReportResponse
            {
                ReportId = Guid.NewGuid().ToString(),
                ReportType = request.ReportType,
                GeneratedBy = User?.Identity?.Name ?? "Anonymous"
            };

            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate security report {ReportType}", request.ReportType);
            return StatusCode(500, new { message = "Failed to generate security report" });
        }
    }

    #endregion
}

#region Request/Response Models

/// <summary>
/// Request model for creating security configuration
/// </summary>
public class CreateSecurityConfigurationRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ConfigurationType { get; set; } = string.Empty;
    public string ConfigurationData { get; set; } = string.Empty;
    public string? Version { get; set; }
    public int? Priority { get; set; }
    public string? Environment { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? Tags { get; set; }
}

/// <summary>
/// Request model for updating security configuration
/// </summary>
public class UpdateSecurityConfigurationRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ConfigurationData { get; set; } = string.Empty;
    public string? Version { get; set; }
    public bool? IsActive { get; set; }
    public int? Priority { get; set; }
    public string? Environment { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? Tags { get; set; }
}

/// <summary>
/// Request model for creating policy rule
/// </summary>
public class CreatePolicyRuleRequest
{
    public string RuleName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PolicyType { get; set; } = string.Empty;
    public string RuleDefinition { get; set; } = string.Empty;
    public string? Severity { get; set; }
    public string? Action { get; set; }
    public int? Priority { get; set; }
    public string? Conditions { get; set; }
    public string? Parameters { get; set; }
}

/// <summary>
/// Request model for creating hardening guideline
/// </summary>
public class CreateHardeningGuidelineRequest
{
    public string GuidelineName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Priority { get; set; }
    public string Implementation { get; set; } = string.Empty;
    public string Verification { get; set; } = string.Empty;
    public string? References { get; set; }
    public string? Tags { get; set; }
}

/// <summary>
/// Request model for creating vulnerability scan
/// </summary>
public class CreateVulnerabilityScanRequest
{
    public string ScanName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ScanType { get; set; } = string.Empty;
    public string TargetSystems { get; set; } = string.Empty;
    public string? ScanParameters { get; set; }
    public DateTime? ScheduledAt { get; set; }
}

/// <summary>
/// Request model for executing security orchestration
/// </summary>
public class ExecuteOrchestrationRequest
{
    public string OrchestrationName { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
    public bool? IsScheduled { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public string? RecurrencePattern { get; set; }
}

/// <summary>
/// Request model for generating security reports
/// </summary>
public class GenerateReportRequest
{
    public string ReportType { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public Guid? ConfigurationId { get; set; }
    public string? Environment { get; set; }
    public bool? IncludeMetrics { get; set; }
    public bool? IncludeViolations { get; set; }
}

/// <summary>
/// Paged response model for security configurations
/// </summary>
public class SecurityConfigurationPagedResponse
{
    public List<SecurityConfigurationDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

/// <summary>
/// Paged response model for security policy rules
/// </summary>
public class SecurityPolicyRulePagedResponse
{
    public List<SecurityPolicyRuleDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

/// <summary>
/// Paged response model for security hardening guidelines
/// </summary>
public class SecurityHardeningGuidelinePagedResponse
{
    public List<SecurityHardeningGuidelineDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

/// <summary>
/// Paged response model for vulnerability scans
/// </summary>
public class VulnerabilityScanPagedResponse
{
    public List<VulnerabilityScanDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

/// <summary>
/// Paged response model for vulnerability findings
/// </summary>
public class VulnerabilityFindingPagedResponse
{
    public List<VulnerabilityFindingDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

/// <summary>
/// Paged response model for security metric history
/// </summary>
public class SecurityMetricHistoryPagedResponse
{
    public List<SecurityMetricHistoryDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

/// <summary>
/// Paged response model for security configuration audit logs
/// </summary>
public class SecurityConfigurationAuditPagedResponse
{
    public List<SecurityConfigurationAuditDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

/// <summary>
/// Paged response model for policy rule violations
/// </summary>
public class PolicyRuleViolationPagedResponse
{
    public List<PolicyRuleViolationDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

#endregion