using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using ADPA.Models.Entities;
using ADPA.Models.DTOs;
using ADPA.Services.Security;
// Use alias to disambiguate ComplianceViolation between DTOs and Entities
using ComplianceViolationDto = ADPA.Models.DTOs.ComplianceViolation;

namespace ADPA.Controllers;

/// <summary>
/// Phase 5.4: Comprehensive Audit Controller
/// REST API for audit logging, compliance reporting, and data lineage tracking
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;
    private readonly ILogger<AuditController> _logger;

    public AuditController(
        IAuditService auditService,
        ILogger<AuditController> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Search audit logs with advanced filtering
    /// </summary>
    [HttpPost("search")]
    [Authorize(Roles = "Admin,Auditor")]
    public async Task<ActionResult<AuditSearchResponse>> SearchAuditLogs([FromBody] AuditSearchRequest request)
    {
        try
        {
            var result = await _auditService.SearchAuditLogsAsync(request);
            
            // Log the search operation
            await _auditService.LogEventAsync("AuditLogSearch", "Search", "AuditLogs", true,
                new Dictionary<string, object>
                {
                    ["SearchCriteria"] = new
                    {
                        request.StartDate,
                        request.EndDate,
                        request.EventTypes,
                        request.Severities,
                        request.SearchTerm
                    },
                    ["ResultCount"] = result.TotalCount
                }, GetCurrentUserId());

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search audit logs");
            
            await _auditService.LogEventAsync("AuditLogSearch", "Search", "AuditLogs", false,
                new Dictionary<string, object> { ["ErrorMessage"] = ex.Message }, GetCurrentUserId());

            return StatusCode(500, new { message = "Audit log search failed" });
        }
    }

    /// <summary>
    /// Get recent audit logs
    /// </summary>
    [HttpGet("recent")]
    [Authorize(Roles = "Admin,Auditor")]
    public async Task<ActionResult<List<AuditLogEntry>>> GetRecentAuditLogs(
        [FromQuery] int count = 50,
        [FromQuery] string? eventType = null,
        [FromQuery] AuditSeverity? severity = null)
    {
        try
        {
            var request = new AuditSearchRequest
            {
                StartDate = DateTime.UtcNow.AddDays(-7),
                PageSize = count,
                SortBy = "Timestamp",
                Descending = true
            };

            if (!string.IsNullOrEmpty(eventType))
                request.EventTypes.Add(eventType);

            if (severity.HasValue)
                request.Severities.Add(severity.Value);

            var result = await _auditService.GetAuditLogsAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get recent audit logs");
            return StatusCode(500, new { message = "Failed to retrieve recent audit logs" });
        }
    }

    /// <summary>
    /// Get audit statistics for a time period
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,Auditor")]
    public async Task<ActionResult<Dictionary<string, object>>> GetAuditStatistics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var statistics = await _auditService.GetAuditStatisticsAsync(start, end);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get audit statistics");
            return StatusCode(500, new { message = "Failed to retrieve audit statistics" });
        }
    }

    /// <summary>
    /// Get high-risk events
    /// </summary>
    [HttpGet("high-risk-events")]
    [Authorize(Roles = "Admin,Auditor")]
    public async Task<ActionResult<List<AuditLogEntry>>> GetHighRiskEvents([FromQuery] int count = 10)
    {
        try
        {
            var events = await _auditService.GetRecentHighRiskEventsAsync(count);
            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get high-risk events");
            return StatusCode(500, new { message = "Failed to retrieve high-risk events" });
        }
    }

    /// <summary>
    /// Export audit logs in various formats
    /// </summary>
    [HttpPost("export")]
    [Authorize(Roles = "Admin,Auditor")]
    public async Task<ActionResult> ExportAuditLogs(
        [FromBody] AuditSearchRequest request,
        [FromQuery] ReportFormat format = ReportFormat.CSV)
    {
        try
        {
            var data = await _auditService.ExportAuditLogsAsync(request, format);
            
            var contentType = format switch
            {
                ReportFormat.PDF => "application/pdf",
                ReportFormat.Excel => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ReportFormat.CSV => "text/csv",
                ReportFormat.JSON => "application/json",
                _ => "application/octet-stream"
            };

            var fileName = $"audit_logs_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{format.ToString().ToLower()}";
            
            await _auditService.LogEventAsync("AuditLogsExported", "Export", "AuditLogs", true,
                new Dictionary<string, object>
                {
                    ["Format"] = format.ToString(),
                    ["FileName"] = fileName,
                    ["ExportSize"] = data.Length
                }, GetCurrentUserId());

            return File(data, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export audit logs");
            
            await _auditService.LogEventAsync("AuditLogsExported", "Export", "AuditLogs", false,
                new Dictionary<string, object> { ["ErrorMessage"] = ex.Message }, GetCurrentUserId());

            return StatusCode(500, new { message = "Audit logs export failed" });
        }
    }

    /// <summary>
    /// Analyze audit trail for patterns and anomalies
    /// </summary>
    [HttpPost("analyze")]
    [Authorize(Roles = "Admin,Auditor")]
    public async Task<ActionResult<AuditAnalysisResult>> AnalyzeAuditTrail([FromBody] AuditAnalysisRequest request)
    {
        try
        {
            var result = await _auditService.AnalyzeAuditTrailAsync(request.StartDate, request.EndDate);
            
            await _auditService.LogEventAsync("AuditTrailAnalyzed", "Analyze", "AuditTrail", true,
                new Dictionary<string, object>
                {
                    ["AnalysisPeriod"] = result.AnalysisPeriod,
                    ["TotalEvents"] = result.TotalEvents,
                    ["PatternsDetected"] = result.DetectedPatterns.Count,
                    ["AnomaliesDetected"] = result.DetectedAnomalies.Count
                }, GetCurrentUserId());

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze audit trail");
            return StatusCode(500, new { message = "Audit trail analysis failed" });
        }
    }

    /// <summary>
    /// Verify audit trail integrity
    /// </summary>
    [HttpPost("verify-integrity")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AuditIntegrityResult>> VerifyAuditIntegrity([FromBody] AuditIntegrityRequest request)
    {
        try
        {
            var isIntegrityValid = await _auditService.VerifyAuditTrailIntegrityAsync(request.StartDate, request.EndDate);
            
            var result = new AuditIntegrityResult
            {
                IsValid = isIntegrityValid,
                VerificationDate = DateTime.UtcNow,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                VerifiedBy = GetCurrentUserName()
            };

            await _auditService.LogEventAsync("AuditIntegrityVerified", "Verify", "AuditIntegrity", isIntegrityValid,
                new Dictionary<string, object>
                {
                    ["StartDate"] = request.StartDate,
                    ["EndDate"] = request.EndDate,
                    ["IntegrityValid"] = isIntegrityValid
                }, GetCurrentUserId());

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify audit integrity");
            return StatusCode(500, new { message = "Audit integrity verification failed" });
        }
    }

    #region Data Lineage

    /// <summary>
    /// Get data lineage for a specific data type
    /// </summary>
    [HttpGet("data-lineage/{dataType}")]
    [Authorize(Roles = "Admin,Auditor,DataProtectionOfficer")]
    public async Task<ActionResult<List<DataLineageEntry>>> GetDataLineage(
        string dataType,
        [FromQuery] string? dataId = null)
    {
        try
        {
            var lineage = await _auditService.GetDataLineageAsync(dataType, dataId);
            return Ok(lineage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get data lineage for {DataType}", dataType);
            return StatusCode(500, new { message = "Failed to retrieve data lineage" });
        }
    }

    /// <summary>
    /// Trace data flow for a specific data item
    /// </summary>
    [HttpGet("data-lineage/trace/{dataId}")]
    [Authorize(Roles = "Admin,Auditor,DataProtectionOfficer")]
    public async Task<ActionResult<List<DataLineageEntry>>> TraceDataFlow(
        string dataId,
        [FromQuery] DateTime? fromDate = null)
    {
        try
        {
            var trace = await _auditService.TraceDataFlowAsync(dataId, fromDate);
            return Ok(trace);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to trace data flow for {DataId}", dataId);
            return StatusCode(500, new { message = "Failed to trace data flow" });
        }
    }

    /// <summary>
    /// Log a data operation for lineage tracking
    /// </summary>
    [HttpPost("data-lineage")]
    [Authorize]
    public async Task<ActionResult> LogDataOperation([FromBody] LogDataOperationRequest request)
    {
        try
        {
            await _auditService.LogDataOperationAsync(
                request.DataType,
                request.DataId,
                request.Operation,
                GetCurrentUserId(),
                request.Purpose,
                request.LegalBasis);

            return Ok(new { message = "Data operation logged successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log data operation");
            return StatusCode(500, new { message = "Failed to log data operation" });
        }
    }

    #endregion

    #region Compliance

    /// <summary>
    /// Generate compliance report
    /// </summary>
    [HttpPost("compliance/report")]
    [Authorize(Roles = "Admin,Auditor,ComplianceOfficer")]
    public async Task<ActionResult<ComplianceReportResponse>> GenerateComplianceReport([FromBody] ComplianceReportRequest request)
    {
        try
        {
            var report = await _auditService.GenerateComplianceReportAsync(request);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate compliance report for {Framework}", request.Framework);
            return StatusCode(500, new { message = "Compliance report generation failed" });
        }
    }

    /// <summary>
    /// Get compliance audit trail
    /// </summary>
    [HttpGet("compliance/{framework}")]
    [Authorize(Roles = "Admin,Auditor,ComplianceOfficer")]
    public async Task<ActionResult<List<ComplianceAuditEntry>>> GetComplianceAuditTrail(
        ComplianceFramework framework,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var auditTrail = await _auditService.GetComplianceAuditTrailAsync(framework, startDate, endDate);
            return Ok(auditTrail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get compliance audit trail for {Framework}", framework);
            return StatusCode(500, new { message = "Failed to retrieve compliance audit trail" });
        }
    }

    /// <summary>
    /// Get compliance violations
    /// </summary>
    [HttpGet("compliance/{framework}/violations")]
    [Authorize(Roles = "Admin,Auditor,ComplianceOfficer")]
    public async Task<ActionResult<List<ComplianceViolationDto>>> GetComplianceViolations(
        ComplianceFramework framework,
        [FromQuery] ComplianceStatus? status = null)
    {
        try
        {
            var violations = await _auditService.GetComplianceViolationsAsync(framework, status);
            return Ok(violations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get compliance violations for {Framework}", framework);
            return StatusCode(500, new { message = "Failed to retrieve compliance violations" });
        }
    }

    #endregion

    #region Alert Management

    /// <summary>
    /// Get audit alert rules
    /// </summary>
    [HttpGet("alert-rules")]
    [Authorize(Roles = "Admin,Auditor")]
    public async Task<ActionResult<List<AuditAlertRule>>> GetAlertRules()
    {
        try
        {
            var rules = await _auditService.GetAlertRulesAsync();
            return Ok(rules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get alert rules");
            return StatusCode(500, new { message = "Failed to retrieve alert rules" });
        }
    }

    /// <summary>
    /// Create new audit alert rule
    /// </summary>
    [HttpPost("alert-rules")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AuditAlertRule>> CreateAlertRule([FromBody] CreateAlertRuleRequest request)
    {
        try
        {
            var rule = new AuditAlertRule
            {
                Name = request.Name,
                IsEnabled = request.IsEnabled,
                Conditions = JsonSerializer.Serialize(request.Conditions),
                Frequency = request.Frequency,
                Threshold = request.Threshold,
                TimeWindow = request.TimeWindow,
                Actions = JsonSerializer.Serialize(request.Actions),
                AlertSeverity = request.AlertSeverity,
                CreatedBy = GetCurrentUserName()
            };

            var createdRule = await _auditService.CreateAlertRuleAsync(rule);
            return Ok(createdRule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create alert rule");
            return StatusCode(500, new { message = "Failed to create alert rule" });
        }
    }

    /// <summary>
    /// Update audit alert rule
    /// </summary>
    [HttpPut("alert-rules/{ruleId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UpdateAlertRule(Guid ruleId, [FromBody] UpdateAlertRuleRequest request)
    {
        try
        {
            var rule = new AuditAlertRule
            {
                Id = ruleId,
                Name = request.Name,
                IsEnabled = request.IsEnabled,
                Conditions = JsonSerializer.Serialize(request.Conditions),
                Frequency = request.Frequency,
                Threshold = request.Threshold,
                TimeWindow = request.TimeWindow,
                Actions = JsonSerializer.Serialize(request.Actions),
                AlertSeverity = request.AlertSeverity
            };

            var success = await _auditService.UpdateAlertRuleAsync(rule);
            
            if (success)
                return Ok(new { message = "Alert rule updated successfully" });
            else
                return StatusCode(500, new { message = "Failed to update alert rule" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update alert rule {RuleId}", ruleId);
            return StatusCode(500, new { message = "Failed to update alert rule" });
        }
    }

    /// <summary>
    /// Delete audit alert rule
    /// </summary>
    [HttpDelete("alert-rules/{ruleId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteAlertRule(Guid ruleId)
    {
        try
        {
            var success = await _auditService.DeleteAlertRuleAsync(ruleId);
            
            if (success)
                return Ok(new { message = "Alert rule deleted successfully" });
            else
                return NotFound(new { message = "Alert rule not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete alert rule {RuleId}", ruleId);
            return StatusCode(500, new { message = "Failed to delete alert rule" });
        }
    }

    #endregion

    #region Configuration

    /// <summary>
    /// Get audit configuration
    /// </summary>
    [HttpGet("configuration")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AuditConfiguration>> GetAuditConfiguration()
    {
        try
        {
            var config = await _auditService.GetAuditConfigurationAsync();
            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get audit configuration");
            return StatusCode(500, new { message = "Failed to retrieve audit configuration" });
        }
    }

    /// <summary>
    /// Update audit configuration
    /// </summary>
    [HttpPut("configuration")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UpdateAuditConfiguration([FromBody] AuditConfiguration config)
    {
        try
        {
            var success = await _auditService.UpdateAuditConfigurationAsync(config);
            
            if (success)
                return Ok(new { message = "Audit configuration updated successfully" });
            else
                return BadRequest(new { message = "Invalid audit configuration" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update audit configuration");
            return StatusCode(500, new { message = "Failed to update audit configuration" });
        }
    }

    #endregion

    #region Archive & Maintenance

    /// <summary>
    /// Archive old audit logs
    /// </summary>
    [HttpPost("archive")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> ArchiveAuditLogs([FromBody] ArchiveRequest request)
    {
        try
        {
            var success = await _auditService.ArchiveAuditLogsAsync(request.BeforeDate);
            
            if (success)
                return Ok(new { message = "Audit logs archived successfully" });
            else
                return StatusCode(500, new { message = "Failed to archive audit logs" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to archive audit logs");
            return StatusCode(500, new { message = "Archive operation failed" });
        }
    }

    /// <summary>
    /// Purge old audit logs (WARNING: Permanent deletion)
    /// </summary>
    [HttpPost("purge")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> PurgeAuditLogs([FromBody] PurgeRequest request)
    {
        try
        {
            var success = await _auditService.PurgeAuditLogsAsync(request.BeforeDate, request.Force);
            
            if (success)
                return Ok(new { message = "Audit logs purged successfully" });
            else
                return BadRequest(new { message = "Purge operation rejected - use force flag for recent data" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to purge audit logs");
            return StatusCode(500, new { message = "Purge operation failed" });
        }
    }

    #endregion

    #region Helper Methods

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    private string GetCurrentUserName()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
    }

    #endregion
}

/// <summary>
/// Supporting request models
/// </summary>
public class AuditAnalysisRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class AuditIntegrityRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class AuditIntegrityResult
{
    public bool IsValid { get; set; }
    public DateTime VerificationDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string VerifiedBy { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

public class LogDataOperationRequest
{
    public string DataType { get; set; } = string.Empty;
    public string DataId { get; set; } = string.Empty;
    public DataOperation Operation { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string? LegalBasis { get; set; }
}

public class CreateAlertRuleRequest
{
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public List<AuditAlertCondition> Conditions { get; set; } = new();
    public AlertFrequency Frequency { get; set; } = AlertFrequency.Immediate;
    public int Threshold { get; set; } = 1;
    public TimeSpan TimeWindow { get; set; } = TimeSpan.FromMinutes(5);
    public List<AlertAction> Actions { get; set; } = new();
    public AuditSeverity AlertSeverity { get; set; } = AuditSeverity.Warning;
}

public class UpdateAlertRuleRequest
{
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public List<AuditAlertCondition> Conditions { get; set; } = new();
    public AlertFrequency Frequency { get; set; } = AlertFrequency.Immediate;
    public int Threshold { get; set; } = 1;
    public TimeSpan TimeWindow { get; set; } = TimeSpan.FromMinutes(5);
    public List<AlertAction> Actions { get; set; } = new();
    public AuditSeverity AlertSeverity { get; set; } = AuditSeverity.Warning;
}

public class ArchiveRequest
{
    public DateTime BeforeDate { get; set; }
}

public class PurgeRequest
{
    public DateTime BeforeDate { get; set; }
    public bool Force { get; set; } = false;
}