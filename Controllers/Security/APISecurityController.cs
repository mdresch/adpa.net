using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ADPA.Services.Security;
using ADPA.Models.Entities;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace ADPA.Controllers.Security
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class APISecurityController : ControllerBase
    {
        private readonly IAPISecurityService _apiSecurityService;

        public APISecurityController(IAPISecurityService apiSecurityService)
        {
            _apiSecurityService = apiSecurityService;
        }

        // Rate Limiting Endpoints
        [HttpGet("rate-limits")]
        public async Task<ActionResult<IEnumerable<RateLimitPolicy>>> GetRateLimitPolicies([FromQuery] bool activeOnly = false)
        {
            try
            {
                var policies = await _apiSecurityService.GetRateLimitPoliciesAsync(activeOnly);
                return Ok(policies);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPost("rate-limits")]
        public async Task<ActionResult<RateLimitPolicy>> CreateRateLimitPolicy([FromBody] RateLimitPolicy policy)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.Identity?.Name ?? "system";
                var createdPolicy = await _apiSecurityService.CreateRateLimitPolicyAsync(policy, userId);
                
                return CreatedAtAction(nameof(GetRateLimitPolicies), new { id = createdPolicy.Id }, createdPolicy);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPut("rate-limits/{id}")]
        public async Task<ActionResult<RateLimitPolicy>> UpdateRateLimitPolicy(Guid id, [FromBody] RateLimitPolicy policy)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.Identity?.Name ?? "system";
                var updatedPolicy = await _apiSecurityService.UpdateRateLimitPolicyAsync(id, policy, userId);
                
                return Ok(updatedPolicy);
            }
            catch (ArgumentException)
            {
                return NotFound(new { error = "Rate limit policy not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpDelete("rate-limits/{id}")]
        public async Task<ActionResult> DeleteRateLimitPolicy(Guid id)
        {
            try
            {
                var userId = User.Identity?.Name ?? "system";
                var success = await _apiSecurityService.DeleteRateLimitPolicyAsync(id, userId);
                
                if (!success)
                    return NotFound(new { error = "Rate limit policy not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpGet("rate-limits/violations")]
        public async Task<ActionResult<IEnumerable<RateLimitViolation>>> GetRateLimitViolations(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var violations = await _apiSecurityService.GetRateLimitViolationsAsync(fromDate, toDate);
                return Ok(violations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        // Input Validation Endpoints
        [HttpGet("validation-rules")]
        public async Task<ActionResult<IEnumerable<InputValidationRule>>> GetValidationRules([FromQuery] string? endpoint = null)
        {
            try
            {
                var rules = await _apiSecurityService.GetValidationRulesAsync(endpoint);
                return Ok(rules);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPost("validation-rules")]
        public async Task<ActionResult<InputValidationRule>> CreateValidationRule([FromBody] InputValidationRule rule)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.Identity?.Name ?? "system";
                var createdRule = await _apiSecurityService.CreateValidationRuleAsync(rule, userId);
                
                return CreatedAtAction(nameof(GetValidationRules), new { id = createdRule.Id }, createdRule);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPut("validation-rules/{id}")]
        public async Task<ActionResult<InputValidationRule>> UpdateValidationRule(Guid id, [FromBody] InputValidationRule rule)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.Identity?.Name ?? "system";
                var updatedRule = await _apiSecurityService.UpdateValidationRuleAsync(id, rule, userId);
                
                return Ok(updatedRule);
            }
            catch (ArgumentException)
            {
                return NotFound(new { error = "Validation rule not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpDelete("validation-rules/{id}")]
        public async Task<ActionResult> DeleteValidationRule(Guid id)
        {
            try
            {
                var userId = User.Identity?.Name ?? "system";
                var success = await _apiSecurityService.DeleteValidationRuleAsync(id, userId);
                
                if (!success)
                    return NotFound(new { error = "Validation rule not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPost("validate-input")]
        public async Task<ActionResult> ValidateInput([FromBody] ValidationRequest request)
        {
            try
            {
                var result = await _apiSecurityService.ValidateInputAsync(request.Endpoint, request.Parameters);
                
                if (!result.IsValid)
                    return BadRequest(new { errors = result.Errors });

                return Ok(new { message = "Input validation passed" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        // Security Events Endpoints
        [HttpGet("security-events")]
        public async Task<ActionResult<IEnumerable<APISecurityEvent>>> GetSecurityEvents(
            [FromQuery] string? eventType = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                APISecurityEventType? eventTypeEnum = null;
                if (!string.IsNullOrEmpty(eventType) && Enum.TryParse<APISecurityEventType>(eventType, out var parsedType))
                    eventTypeEnum = parsedType;

                var events = await _apiSecurityService.GetSecurityEventsAsync(eventTypeEnum, fromDate, toDate);
                return Ok(events);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPost("security-events")]
        public async Task<ActionResult<APISecurityEvent>> LogSecurityEvent([FromBody] APISecurityEvent securityEvent)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var loggedEvent = await _apiSecurityService.LogSecurityEventAsync(securityEvent);
                
                return CreatedAtAction(nameof(GetSecurityEvents), new { id = loggedEvent.Id }, loggedEvent);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        // API Version Management
        [HttpGet("api-versions")]
        public async Task<ActionResult<IEnumerable<APIVersion>>> GetAPIVersions([FromQuery] bool activeOnly = false)
        {
            try
            {
                var versions = await _apiSecurityService.GetAPIVersionsAsync(activeOnly);
                return Ok(versions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPost("api-versions")]
        public async Task<ActionResult<APIVersion>> CreateAPIVersion([FromBody] APIVersion version)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.Identity?.Name ?? "system";
                var createdVersion = await _apiSecurityService.CreateAPIVersionAsync(version, userId);
                
                return CreatedAtAction(nameof(GetAPIVersions), new { id = createdVersion.Id }, createdVersion);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPut("api-versions/{id}")]
        public async Task<ActionResult<APIVersion>> UpdateAPIVersion(Guid id, [FromBody] APIVersion version)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.Identity?.Name ?? "system";
                var updatedVersion = await _apiSecurityService.UpdateAPIVersionAsync(id, version, userId);
                
                return Ok(updatedVersion);
            }
            catch (ArgumentException)
            {
                return NotFound(new { error = "API version not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpGet("api-endpoints")]
        public async Task<ActionResult<IEnumerable<APIEndpoint>>> GetAPIEndpoints([FromQuery] Guid? versionId = null)
        {
            try
            {
                var endpoints = await _apiSecurityService.GetAPIEndpointsAsync(versionId);
                return Ok(endpoints);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        // CORS Policy Management
        [HttpGet("cors-policies")]
        public async Task<ActionResult<IEnumerable<CORSPolicy>>> GetCORSPolicies([FromQuery] bool activeOnly = false)
        {
            try
            {
                var policies = await _apiSecurityService.GetCORSPoliciesAsync(activeOnly);
                return Ok(policies);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPost("cors-policies")]
        public async Task<ActionResult<CORSPolicy>> CreateCORSPolicy([FromBody] CORSPolicy policy)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.Identity?.Name ?? "system";
                var createdPolicy = await _apiSecurityService.CreateCORSPolicyAsync(policy, userId);
                
                return CreatedAtAction(nameof(GetCORSPolicies), new { id = createdPolicy.Id }, createdPolicy);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        // Security Headers Management
        [HttpGet("security-headers")]
        public async Task<ActionResult<IEnumerable<SecurityHeader>>> GetSecurityHeaders([FromQuery] string? endpoint = null)
        {
            try
            {
                var headers = await _apiSecurityService.GetSecurityHeadersAsync(endpoint);
                return Ok(headers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPost("security-headers")]
        public async Task<ActionResult<SecurityHeader>> CreateSecurityHeader([FromBody] SecurityHeader header)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.Identity?.Name ?? "system";
                var createdHeader = await _apiSecurityService.CreateSecurityHeaderAsync(header, userId);
                
                return CreatedAtAction(nameof(GetSecurityHeaders), new { id = createdHeader.Id }, createdHeader);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        // API Key Management
        [HttpGet("api-keys")]
        public async Task<ActionResult<IEnumerable<APIKey>>> GetAPIKeys([FromQuery] bool activeOnly = false)
        {
            try
            {
                var apiKeys = await _apiSecurityService.GetAPIKeysAsync(activeOnly);
                
                // Remove sensitive data from response
                var sanitizedKeys = apiKeys.Select(k => new
                {
                    k.Id,
                    k.KeyName,
                    k.KeyPrefix,
                    k.AssignedTo,
                    k.AllowedEndpoints,
                    k.ExpiresAt,
                    k.IsActive,
                    k.LastUsed,
                    k.UsageCount,
                    k.CreatedAt,
                    k.CreatedBy
                });
                
                return Ok(sanitizedKeys);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPost("api-keys")]
        public async Task<ActionResult> CreateAPIKey([FromBody] CreateAPIKeyRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.Identity?.Name ?? "system";
                var createdKey = await _apiSecurityService.CreateAPIKeyAsync(
                    request.KeyName,
                    request.AssignedTo,
                    request.AllowedEndpoints,
                    userId);
                
                // Return the full key only once
                return CreatedAtAction(nameof(GetAPIKeys), new { id = createdKey.Id }, new
                {
                    createdKey.Id,
                    createdKey.KeyName,
                    createdKey.KeyPrefix,
                    createdKey.AssignedTo,
                    createdKey.AllowedEndpoints,
                    createdKey.CreatedAt,
                    createdKey.CreatedBy,
                    Message = "Store this API key securely. It will not be shown again."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpDelete("api-keys/{id}")]
        public async Task<ActionResult> RevokeAPIKey(Guid id)
        {
            try
            {
                var userId = User.Identity?.Name ?? "system";
                var success = await _apiSecurityService.RevokeAPIKeyAsync(id, userId);
                
                if (!success)
                    return NotFound(new { error = "API key not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpGet("api-keys/{id}/usage")]
        public async Task<ActionResult<IEnumerable<APIKeyUsage>>> GetAPIKeyUsage(Guid id)
        {
            try
            {
                // This would need to be implemented in the service
                return Ok(new List<APIKeyUsage>());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        // Configuration Management
        [HttpGet("configuration")]
        public async Task<ActionResult<APISecurityConfiguration>> GetConfiguration()
        {
            try
            {
                var configuration = await _apiSecurityService.GetConfigurationAsync();
                return Ok(configuration);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPut("configuration")]
        public async Task<ActionResult> UpdateConfiguration([FromBody] APISecurityConfiguration configuration)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.Identity?.Name ?? "system";
                var success = await _apiSecurityService.UpdateConfigurationAsync(configuration, userId);
                
                if (!success)
                    return BadRequest(new { error = "Failed to update configuration" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        // Security Metrics
        [HttpGet("metrics")]
        public async Task<ActionResult<Dictionary<string, object>>> GetSecurityMetrics()
        {
            try
            {
                var metrics = await _apiSecurityService.GetSecurityMetricsAsync();
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        // Threat Detection
        [HttpPost("detect-sql-injection")]
        public async Task<ActionResult> DetectSQLInjection([FromBody] ThreatDetectionRequest request)
        {
            try
            {
                var isThreat = await _apiSecurityService.DetectSQLInjectionAsync(request.Input);
                return Ok(new { isSQLInjection = isThreat, input = request.Input });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPost("detect-xss")]
        public async Task<ActionResult> DetectXSS([FromBody] ThreatDetectionRequest request)
        {
            try
            {
                var isThreat = await _apiSecurityService.DetectXSSAsync(request.Input);
                return Ok(new { isXSS = isThreat, input = request.Input });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPost("calculate-risk-score")]
        public async Task<ActionResult> CalculateRiskScore([FromBody] RiskCalculationRequest request)
        {
            try
            {
                var riskScore = await _apiSecurityService.CalculateRiskScoreAsync(
                    request.IPAddress,
                    request.UserAgent,
                    request.Endpoint);
                
                return Ok(new { riskScore, threshold = 70 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }
    }

    // Request models for the controller
    public class ValidationRequest
    {
        public string Endpoint { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
    }

    public class CreateAPIKeyRequest
    {
        public string KeyName { get; set; } = string.Empty;
        public string AssignedTo { get; set; } = string.Empty;
        public string[] AllowedEndpoints { get; set; } = Array.Empty<string>();
    }

    public class ThreatDetectionRequest
    {
        public string Input { get; set; } = string.Empty;
    }

    public class RiskCalculationRequest
    {
        public string IPAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
    }
}