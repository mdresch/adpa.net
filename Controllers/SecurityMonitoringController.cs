using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ADPA.Models.Entities;
using ADPA.Models.DTOs;
using ADPA.Services.Security;

namespace ADPA.Controllers;

/// <summary>
/// Phase 5.5: Security Monitoring & Threat Detection Controller
/// REST API for security incident management, threat intelligence, and anomaly detection
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class SecurityMonitoringController : ControllerBase
{
    private readonly ISecurityMonitoringService _securityService;
    private readonly IAuditService _auditService;
    private readonly ILogger<SecurityMonitoringController> _logger;

    public SecurityMonitoringController(
        ISecurityMonitoringService securityService,
        IAuditService auditService,
        ILogger<SecurityMonitoringController> logger)
    {
        _securityService = securityService;
        _auditService = auditService;
        _logger = logger;
    }

    #region Security Dashboard

    /// <summary>
    /// Get comprehensive security dashboard data
    /// </summary>
    [HttpGet("dashboard")]
    [Authorize(Roles = "Admin,SecurityAnalyst,SecurityManager")]
    public async Task<ActionResult<SecurityDashboardData>> GetSecurityDashboard()
    {
        try
        {
            var dashboardData = await _securityService.GetSecurityDashboardDataAsync();
            
            await _auditService.LogEventAsync("SecurityDashboardAccessed", "View", "SecurityDashboard", true,
                new Dictionary<string, object>
                {
                    ["AccessedBy"] = GetCurrentUserName(),
                    ["Timestamp"] = DateTime.UtcNow
                }, GetCurrentUserId());

            return Ok(dashboardData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get security dashboard data");
            return StatusCode(500, new { message = "Failed to retrieve security dashboard data" });
        }
    }

    /// <summary>
    /// Get real-time security status
    /// </summary>
    [HttpGet("status/realtime")]
    [Authorize(Roles = "Admin,SecurityAnalyst,SecurityManager")]
    public async Task<ActionResult<Dictionary<string, object>>> GetRealTimeSecurityStatus()
    {
        try
        {
            var status = await _securityService.GetRealTimeSecurityStatusAsync();
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get real-time security status");
            return StatusCode(500, new { message = "Failed to retrieve real-time security status" });
        }
    }

    #endregion

    #region Incident Management

    /// <summary>
    /// Create a new security incident
    /// </summary>
    [HttpPost("incidents")]
    [Authorize(Roles = "Admin,SecurityAnalyst,SecurityManager")]
    public async Task<ActionResult<SecurityIncident>> CreateIncident([FromBody] CreateIncidentRequest request)
    {
        try
        {
            var incident = new SecurityIncident
            {
                Title = request.Title,
                Description = request.Description,
                Severity = request.Severity,
                Type = request.Type,
                Category = request.Category,
                Source = request.Source,
                SourceIp = request.SourceIp,
                TargetResource = request.TargetResource,
                AffectedUser = request.AffectedUser,
                AssignedTo = request.AssignedTo
            };

            var createdIncident = await _securityService.CreateIncidentAsync(incident);

            await _auditService.LogEventAsync("SecurityIncidentCreated", "Create", "SecurityIncident", true,
                new Dictionary<string, object>
                {
                    ["IncidentId"] = createdIncident.Id,
                    ["IncidentNumber"] = createdIncident.IncidentNumber,
                    ["Severity"] = createdIncident.Severity.ToString(),
                    ["CreatedBy"] = GetCurrentUserName()
                }, GetCurrentUserId());

            return Ok(createdIncident);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create security incident");
            return StatusCode(500, new { message = "Failed to create security incident" });
        }
    }

    /// <summary>
    /// Get security incidents with filtering
    /// </summary>
    [HttpPost("incidents/search")]
    [Authorize(Roles = "Admin,SecurityAnalyst,SecurityManager")]
    public async Task<ActionResult<List<SecurityIncident>>> SearchIncidents([FromBody] IncidentSearchRequest request)
    {
        try
        {
            var incidents = await _securityService.GetIncidentsAsync(request);
            return Ok(incidents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search security incidents");
            return StatusCode(500, new { message = "Failed to search security incidents" });
        }
    }

    /// <summary>
    /// Get specific security incident
    /// </summary>
    [HttpGet("incidents/{incidentId}")]
    [Authorize(Roles = "Admin,SecurityAnalyst,SecurityManager")]
    public async Task<ActionResult<SecurityIncident>> GetIncident(Guid incidentId)
    {
        try
        {
            var incident = await _securityService.GetIncidentAsync(incidentId);
            if (incident == null)
                return NotFound(new { message = "Security incident not found" });

            return Ok(incident);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get security incident {IncidentId}", incidentId);
            return StatusCode(500, new { message = "Failed to retrieve security incident" });
        }
    }

    /// <summary>
    /// Update security incident
    /// </summary>
    [HttpPut("incidents/{incidentId}")]
    [Authorize(Roles = "Admin,SecurityAnalyst,SecurityManager")]
    public async Task<ActionResult> UpdateIncident(Guid incidentId, [FromBody] UpdateIncidentRequest request)
    {
        try
        {
            var incident = await _securityService.GetIncidentAsync(incidentId);
            if (incident == null)
                return NotFound(new { message = "Security incident not found" });

            // Update incident properties
            incident.Title = request.Title ?? incident.Title;
            incident.Description = request.Description ?? incident.Description;
            incident.Severity = request.Severity ?? incident.Severity;
            incident.Status = request.Status ?? incident.Status;
            incident.AssignedTo = request.AssignedTo ?? incident.AssignedTo;
            incident.ImpactAssessment = request.ImpactAssessment ?? incident.ImpactAssessment;

            var success = await _securityService.UpdateIncidentAsync(incident);
            
            if (success)
            {
                await _auditService.LogEventAsync("SecurityIncidentUpdated", "Update", "SecurityIncident", true,
                    new Dictionary<string, object>
                    {
                        ["IncidentId"] = incidentId,
                        ["UpdatedBy"] = GetCurrentUserName()
                    }, GetCurrentUserId());

                return Ok(new { message = "Security incident updated successfully" });
            }
            else
            {
                return StatusCode(500, new { message = "Failed to update security incident" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update security incident {IncidentId}", incidentId);
            return StatusCode(500, new { message = "Failed to update security incident" });
        }
    }

    /// <summary>
    /// Assign security incident to user
    /// </summary>
    [HttpPost("incidents/{incidentId}/assign")]
    [Authorize(Roles = "Admin,SecurityManager")]
    public async Task<ActionResult> AssignIncident(Guid incidentId, [FromBody] AssignIncidentRequest request)
    {
        try
        {
            var success = await _securityService.AssignIncidentAsync(incidentId, request.AssignedTo);
            
            if (success)
            {
                await _auditService.LogEventAsync("SecurityIncidentAssigned", "Assign", "SecurityIncident", true,
                    new Dictionary<string, object>
                    {
                        ["IncidentId"] = incidentId,
                        ["AssignedTo"] = request.AssignedTo,
                        ["AssignedBy"] = GetCurrentUserName()
                    }, GetCurrentUserId());

                return Ok(new { message = "Security incident assigned successfully" });
            }
            else
            {
                return NotFound(new { message = "Security incident not found" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to assign security incident {IncidentId}", incidentId);
            return StatusCode(500, new { message = "Failed to assign security incident" });
        }
    }

    /// <summary>
    /// Resolve security incident
    /// </summary>
    [HttpPost("incidents/{incidentId}/resolve")]
    [Authorize(Roles = "Admin,SecurityAnalyst,SecurityManager")]
    public async Task<ActionResult> ResolveIncident(Guid incidentId, [FromBody] ResolveIncidentRequest request)
    {
        try
        {
            var success = await _securityService.ResolveIncidentAsync(incidentId, request.Resolution, GetCurrentUserName());
            
            if (success)
            {
                await _auditService.LogEventAsync("SecurityIncidentResolved", "Resolve", "SecurityIncident", true,
                    new Dictionary<string, object>
                    {
                        ["IncidentId"] = incidentId,
                        ["ResolvedBy"] = GetCurrentUserName(),
                        ["Resolution"] = request.Resolution
                    }, GetCurrentUserId());

                return Ok(new { message = "Security incident resolved successfully" });
            }
            else
            {
                return NotFound(new { message = "Security incident not found" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve security incident {IncidentId}", incidentId);
            return StatusCode(500, new { message = "Failed to resolve security incident" });
        }
    }

    /// <summary>
    /// Get incident statistics
    /// </summary>
    [HttpGet("incidents/statistics")]
    [Authorize(Roles = "Admin,SecurityAnalyst,SecurityManager")]
    public async Task<ActionResult<IncidentStatistics>> GetIncidentStatistics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var statistics = await _securityService.GetIncidentStatisticsAsync(startDate, endDate);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get incident statistics");
            return StatusCode(500, new { message = "Failed to retrieve incident statistics" });
        }
    }

    #endregion

    #region Threat Intelligence

    /// <summary>
    /// Add new threat indicator
    /// </summary>
    [HttpPost("threats/indicators")]
    [Authorize(Roles = "Admin,ThreatAnalyst,SecurityManager")]
    public async Task<ActionResult<ThreatIndicator>> AddThreatIndicator([FromBody] CreateThreatIndicatorRequest request)
    {
        try
        {
            var indicator = new ThreatIndicator
            {
                Type = request.Type,
                Value = request.Value,
                Description = request.Description,
                Confidence = request.Confidence,
                Severity = request.Severity,
                Source = request.Source,
                Tags = request.Tags,
                ThreatTypes = request.ThreatTypes,
                ExpiresAt = request.ExpiresAt
            };

            var createdIndicator = await _securityService.AddThreatIndicatorAsync(indicator);

            await _auditService.LogEventAsync("ThreatIndicatorAdded", "Add", "ThreatIndicator", true,
                new Dictionary<string, object>
                {
                    ["IndicatorId"] = createdIndicator.Id,
                    ["Type"] = createdIndicator.Type.ToString(),
                    ["Value"] = createdIndicator.Value,
                    ["AddedBy"] = GetCurrentUserName()
                }, GetCurrentUserId());

            return Ok(createdIndicator);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add threat indicator");
            return StatusCode(500, new { message = "Failed to add threat indicator" });
        }
    }

    /// <summary>
    /// Search threat indicators
    /// </summary>
    [HttpPost("threats/indicators/search")]
    [Authorize(Roles = "Admin,ThreatAnalyst,SecurityAnalyst,SecurityManager")]
    public async Task<ActionResult<List<ThreatIndicator>>> SearchThreatIndicators([FromBody] ThreatIndicatorSearchRequest request)
    {
        try
        {
            var indicators = await _securityService.GetThreatIndicatorsAsync(request);
            return Ok(indicators);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search threat indicators");
            return StatusCode(500, new { message = "Failed to search threat indicators" });
        }
    }

    /// <summary>
    /// Check value against threat indicators
    /// </summary>
    [HttpPost("threats/indicators/check")]
    [Authorize(Roles = "Admin,SecurityAnalyst,ThreatAnalyst,SecurityManager")]
    public async Task<ActionResult<ThreatCheckResult>> CheckThreatIndicators([FromBody] ThreatCheckRequest request)
    {
        try
        {
            var matches = await _securityService.CheckThreatIndicatorsAsync(request.Value, request.Type);

            var result = new ThreatCheckResult
            {
                Value = request.Value,
                IsMatch = matches.Any(),
                MatchCount = matches.Count,
                HighestSeverity = matches.Any() ? matches.Max(m => m.Severity) : ThreatSeverity.Informational,
                Matches = matches.Select(m => new ThreatMatch
                {
                    IndicatorId = m.Id,
                    Type = m.Type,
                    Severity = m.Severity,
                    Confidence = m.Confidence,
                    Source = m.Source,
                    Description = m.Description
                }).ToList()
            };

            if (matches.Any())
            {
                await _auditService.LogEventAsync("ThreatIndicatorMatch", "Check", "ThreatIndicator", true,
                    new Dictionary<string, object>
                    {
                        ["Value"] = request.Value,
                        ["MatchCount"] = matches.Count,
                        ["HighestSeverity"] = result.HighestSeverity.ToString(),
                        ["CheckedBy"] = GetCurrentUserName()
                    }, GetCurrentUserId());
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check threat indicators for value {Value}", request.Value);
            return StatusCode(500, new { message = "Failed to check threat indicators" });
        }
    }

    /// <summary>
    /// Get threat intelligence summary
    /// </summary>
    [HttpGet("threats/intelligence/summary")]
    [Authorize(Roles = "Admin,ThreatAnalyst,SecurityAnalyst,SecurityManager")]
    public async Task<ActionResult<ThreatIntelligenceSummary>> GetThreatIntelligenceSummary()
    {
        try
        {
            var summary = await _securityService.GetThreatIntelligenceSummaryAsync();
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get threat intelligence summary");
            return StatusCode(500, new { message = "Failed to retrieve threat intelligence summary" });
        }
    }

    /// <summary>
    /// Deactivate threat indicator
    /// </summary>
    [HttpPost("threats/indicators/{indicatorId}/deactivate")]
    [Authorize(Roles = "Admin,ThreatAnalyst,SecurityManager")]
    public async Task<ActionResult> DeactivateThreatIndicator(Guid indicatorId)
    {
        try
        {
            var success = await _securityService.DeactivateThreatIndicatorAsync(indicatorId);
            
            if (success)
            {
                await _auditService.LogEventAsync("ThreatIndicatorDeactivated", "Deactivate", "ThreatIndicator", true,
                    new Dictionary<string, object>
                    {
                        ["IndicatorId"] = indicatorId,
                        ["DeactivatedBy"] = GetCurrentUserName()
                    }, GetCurrentUserId());

                return Ok(new { message = "Threat indicator deactivated successfully" });
            }
            else
            {
                return NotFound(new { message = "Threat indicator not found" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deactivate threat indicator {IndicatorId}", indicatorId);
            return StatusCode(500, new { message = "Failed to deactivate threat indicator" });
        }
    }

    #endregion

    #region Anomaly Detection

    /// <summary>
    /// Detect security anomalies
    /// </summary>
    [HttpPost("anomalies/detect")]
    [Authorize(Roles = "Admin,SecurityAnalyst,SecurityManager")]
    public async Task<ActionResult<List<SecurityAnomaly>>> DetectAnomalies([FromBody] AnomalyDetectionRequest request)
    {
        try
        {
            var anomalies = await _securityService.DetectAnomaliesAsync(request);

            await _auditService.LogEventAsync("AnomalyDetectionExecuted", "Detect", "SecurityAnomaly", true,
                new Dictionary<string, object>
                {
                    ["DetectionTypes"] = request.DetectionTypes.Select(t => t.ToString()).ToList(),
                    ["AnomaliesFound"] = anomalies.Count,
                    ["Sensitivity"] = request.Sensitivity,
                    ["ExecutedBy"] = GetCurrentUserName()
                }, GetCurrentUserId());

            return Ok(anomalies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to detect anomalies");
            return StatusCode(500, new { message = "Failed to detect security anomalies" });
        }
    }

    /// <summary>
    /// Get security anomalies with filtering
    /// </summary>
    [HttpPost("anomalies/search")]
    [Authorize(Roles = "Admin,SecurityAnalyst,SecurityManager")]
    public async Task<ActionResult<List<SecurityAnomaly>>> SearchAnomalies([FromBody] AnomalySearchRequest request)
    {
        try
        {
            var anomalies = await _securityService.GetAnomaliesAsync(request);
            return Ok(anomalies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search security anomalies");
            return StatusCode(500, new { message = "Failed to search security anomalies" });
        }
    }

    /// <summary>
    /// Mark anomaly as investigated
    /// </summary>
    [HttpPost("anomalies/{anomalyId}/investigate")]
    [Authorize(Roles = "Admin,SecurityAnalyst,SecurityManager")]
    public async Task<ActionResult> InvestigateAnomaly(Guid anomalyId, [FromBody] InvestigateAnomalyRequest request)
    {
        try
        {
            var success = await _securityService.MarkAnomalyAsInvestigatedAsync(
                anomalyId, 
                GetCurrentUserName(), 
                request.Notes, 
                request.IsFalsePositive);
            
            if (success)
            {
                await _auditService.LogEventAsync("SecurityAnomalyInvestigated", "Investigate", "SecurityAnomaly", true,
                    new Dictionary<string, object>
                    {
                        ["AnomalyId"] = anomalyId,
                        ["IsFalsePositive"] = request.IsFalsePositive,
                        ["InvestigatedBy"] = GetCurrentUserName()
                    }, GetCurrentUserId());

                return Ok(new { message = "Anomaly investigation completed successfully" });
            }
            else
            {
                return NotFound(new { message = "Security anomaly not found" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to investigate anomaly {AnomalyId}", anomalyId);
            return StatusCode(500, new { message = "Failed to investigate security anomaly" });
        }
    }

    /// <summary>
    /// Get anomaly statistics
    /// </summary>
    [HttpGet("anomalies/statistics")]
    [Authorize(Roles = "Admin,SecurityAnalyst,SecurityManager")]
    public async Task<ActionResult<AnomalyStatistics>> GetAnomalyStatistics([FromQuery] int days = 30)
    {
        try
        {
            var period = TimeSpan.FromDays(days);
            var statistics = await _securityService.GetAnomalyStatisticsAsync(period);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get anomaly statistics");
            return StatusCode(500, new { message = "Failed to retrieve anomaly statistics" });
        }
    }

    #endregion

    #region Security Actions

    /// <summary>
    /// Execute security response action
    /// </summary>
    [HttpPost("actions/execute")]
    [Authorize(Roles = "Admin,SecurityManager")]
    public async Task<ActionResult<SecurityActionResult>> ExecuteSecurityAction([FromBody] ExecuteSecurityActionRequest request)
    {
        try
        {
            var action = new SecurityAction
            {
                ActionType = request.ActionType,
                Target = request.Target,
                Parameters = request.Parameters,
                CreatedBy = GetCurrentUserName()
            };

            var success = await _securityService.ExecuteSecurityActionAsync(action);

            await _auditService.LogEventAsync("SecurityActionExecuted", "Execute", "SecurityAction", success,
                new Dictionary<string, object>
                {
                    ["ActionType"] = request.ActionType,
                    ["Target"] = request.Target,
                    ["ExecutedBy"] = GetCurrentUserName(),
                    ["Success"] = success
                }, GetCurrentUserId());

            var result = new SecurityActionResult
            {
                Success = success,
                Message = success ? "Security action executed successfully" : "Security action execution failed",
                ExecutedAt = DateTime.UtcNow
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute security action {ActionType}", request.ActionType);
            return StatusCode(500, new { message = "Failed to execute security action" });
        }
    }

    /// <summary>
    /// Block IP address
    /// </summary>
    [HttpPost("actions/block-ip")]
    [Authorize(Roles = "Admin,SecurityManager")]
    public async Task<ActionResult<SecurityActionResult>> BlockIpAddress([FromBody] BlockIpRequest request)
    {
        try
        {
            var result = await _securityService.BlockIpAddressAsync(request.IpAddress, request.Duration, request.Reason);

            await _auditService.LogEventAsync("IpAddressBlocked", "Block", "IpAddress", result.Success,
                new Dictionary<string, object>
                {
                    ["IpAddress"] = request.IpAddress,
                    ["Duration"] = request.Duration?.ToString() ?? "Permanent",
                    ["Reason"] = request.Reason,
                    ["BlockedBy"] = GetCurrentUserName()
                }, GetCurrentUserId());

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to block IP address {IpAddress}", request.IpAddress);
            return StatusCode(500, new { message = "Failed to block IP address" });
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

#region Request Models

/// <summary>
/// Create incident request
/// </summary>
public class CreateIncidentRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IncidentSeverity Severity { get; set; } = IncidentSeverity.Medium;
    public IncidentType Type { get; set; }
    public IncidentCategory Category { get; set; } = IncidentCategory.Security;
    public string? Source { get; set; }
    public string? SourceIp { get; set; }
    public string? TargetResource { get; set; }
    public string? AffectedUser { get; set; }
    public string? AssignedTo { get; set; }
}

/// <summary>
/// Update incident request
/// </summary>
public class UpdateIncidentRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public IncidentSeverity? Severity { get; set; }
    public IncidentStatus? Status { get; set; }
    public string? AssignedTo { get; set; }
    public string? ImpactAssessment { get; set; }
}

/// <summary>
/// Assign incident request
/// </summary>
public class AssignIncidentRequest
{
    public string AssignedTo { get; set; } = string.Empty;
}

/// <summary>
/// Resolve incident request
/// </summary>
public class ResolveIncidentRequest
{
    public string Resolution { get; set; } = string.Empty;
}

/// <summary>
/// Create threat indicator request
/// </summary>
public class CreateThreatIndicatorRequest
{
    public ThreatIndicatorType Type { get; set; }
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ThreatConfidenceLevel Confidence { get; set; } = ThreatConfidenceLevel.Medium;
    public ThreatSeverity Severity { get; set; } = ThreatSeverity.Medium;
    public string Source { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public List<string> ThreatTypes { get; set; } = new();
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// Threat check request
/// </summary>
public class ThreatCheckRequest
{
    public string Value { get; set; } = string.Empty;
    public ThreatIndicatorType? Type { get; set; }
}

/// <summary>
/// Threat check result
/// </summary>
public class ThreatCheckResult
{
    public string Value { get; set; } = string.Empty;
    public bool IsMatch { get; set; }
    public int MatchCount { get; set; }
    public ThreatSeverity HighestSeverity { get; set; }
    public List<ThreatMatch> Matches { get; set; } = new();
}

/// <summary>
/// Threat match details
/// </summary>
public class ThreatMatch
{
    public Guid IndicatorId { get; set; }
    public ThreatIndicatorType Type { get; set; }
    public ThreatSeverity Severity { get; set; }
    public ThreatConfidenceLevel Confidence { get; set; }
    public string Source { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>
/// Investigate anomaly request
/// </summary>
public class InvestigateAnomalyRequest
{
    public string Notes { get; set; } = string.Empty;
    public bool IsFalsePositive { get; set; } = false;
}

/// <summary>
/// Execute security action request
/// </summary>
public class ExecuteSecurityActionRequest
{
    public string ActionType { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Block IP request
/// </summary>
public class BlockIpRequest
{
    public string IpAddress { get; set; } = string.Empty;
    public TimeSpan? Duration { get; set; }
    public string Reason { get; set; } = string.Empty;
}

#endregion