using ADPA.Data;
using ADPA.Models.Entities;
using ADPA.Services.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;

namespace ADPA.Services.Security
{
    public interface IAPISecurityService
    {
        // Rate Limiting
        Task<RateLimitPolicy> CreateRateLimitPolicyAsync(RateLimitPolicy policy, string userId);
        Task<RateLimitPolicy> UpdateRateLimitPolicyAsync(Guid policyId, RateLimitPolicy policy, string userId);
        Task<bool> DeleteRateLimitPolicyAsync(Guid policyId, string userId);
        Task<IEnumerable<RateLimitPolicy>> GetRateLimitPoliciesAsync(bool activeOnly = false);
        Task<bool> CheckRateLimitAsync(string clientId, string endpoint, string method = "GET");
        Task<RateLimitViolation> RecordRateLimitViolationAsync(string clientId, string endpoint, string ipAddress);
        Task<IEnumerable<RateLimitViolation>> GetRateLimitViolationsAsync(DateTime? fromDate = null, DateTime? toDate = null);

        // Input Validation
        Task<InputValidationRule> CreateValidationRuleAsync(InputValidationRule rule, string userId);
        Task<InputValidationRule> UpdateValidationRuleAsync(Guid ruleId, InputValidationRule rule, string userId);
        Task<bool> DeleteValidationRuleAsync(Guid ruleId, string userId);
        Task<IEnumerable<InputValidationRule>> GetValidationRulesAsync(string endpoint = null);
        Task<ValidationResult> ValidateInputAsync(string endpoint, Dictionary<string, object> parameters);
        Task<ValidationViolation> RecordValidationViolationAsync(ValidationViolation violation);

        // Output Sanitization
        Task<SanitizationRule> CreateSanitizationRuleAsync(SanitizationRule rule, string userId);
        Task<IEnumerable<SanitizationRule>> GetSanitizationRulesAsync(string endpoint = null);
        Task<string> SanitizeOutputAsync(string content, string endpoint, SanitizationLevel level = SanitizationLevel.Standard);
        Task<object> SanitizeObjectAsync(object data, string endpoint, SanitizationLevel level = SanitizationLevel.Standard);

        // API Versioning
        Task<APIVersion> CreateAPIVersionAsync(APIVersion version, string userId);
        Task<APIVersion> UpdateAPIVersionAsync(Guid versionId, APIVersion version, string userId);
        Task<IEnumerable<APIVersion>> GetAPIVersionsAsync(bool activeOnly = false);
        Task<APIEndpoint> CreateAPIEndpointAsync(APIEndpoint endpoint, string userId);
        Task<IEnumerable<APIEndpoint>> GetAPIEndpointsAsync(Guid? versionId = null);
        Task<bool> IsValidAPIVersionAsync(string version);
        Task<APIVersion> GetDefaultAPIVersionAsync();

        // Security Events
        Task<APISecurityEvent> LogSecurityEventAsync(APISecurityEvent securityEvent);
        Task<IEnumerable<APISecurityEvent>> GetSecurityEventsAsync(APISecurityEventType? eventType = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<bool> IsIPBlacklistedAsync(string ipAddress);
        Task<bool> IsSuspiciousActivityAsync(string ipAddress, string endpoint);
        Task<int> CalculateRiskScoreAsync(string ipAddress, string userAgent, string endpoint);

        // CORS Management
        Task<CORSPolicy> CreateCORSPolicyAsync(CORSPolicy policy, string userId);
        Task<IEnumerable<CORSPolicy>> GetCORSPoliciesAsync(bool activeOnly = false);
        Task<CORSPolicy> GetCORSPolicyForEndpointAsync(string endpoint);
        Task<bool> IsOriginAllowedAsync(string origin, string endpoint);

        // Security Headers
        Task<SecurityHeader> CreateSecurityHeaderAsync(SecurityHeader header, string userId);
        Task<IEnumerable<SecurityHeader>> GetSecurityHeadersAsync(string endpoint = null);
        Task<Dictionary<string, string>> GetSecurityHeadersForEndpointAsync(string endpoint);

        // API Key Management
        Task<APIKey> CreateAPIKeyAsync(string keyName, string assignedTo, string[] allowedEndpoints, string userId);
        Task<APIKey> UpdateAPIKeyAsync(Guid keyId, APIKey apiKey, string userId);
        Task<bool> ValidateAPIKeyAsync(string keyValue, string endpoint = null, string ipAddress = null);
        Task<bool> RevokeAPIKeyAsync(Guid keyId, string userId);
        Task<IEnumerable<APIKey>> GetAPIKeysAsync(bool activeOnly = false);
        Task<APIKeyUsage> RecordAPIKeyUsageAsync(Guid keyId, string endpoint, string ipAddress, int responseCode);

        // Threat Detection
        Task<bool> DetectSQLInjectionAsync(string input);
        Task<bool> DetectXSSAsync(string input);
        Task<bool> DetectCSRFAsync(Dictionary<string, string> headers, string referrer);
        Task<bool> IsGeolocationAllowedAsync(string ipAddress);
        Task<string> GetGeolocationAsync(string ipAddress);

        // Configuration
        Task<APISecurityConfiguration> GetConfigurationAsync();
        Task<bool> UpdateConfigurationAsync(APISecurityConfiguration configuration, string userId);
        Task<Dictionary<string, object>> GetSecurityMetricsAsync();
    }

    public class APISecurityService : IAPISecurityService
    {
        private readonly AdpaEfDbContext _context;
        private readonly IAuditService _auditService;
        private readonly ILogger<APISecurityService> _logger;
        private readonly APISecurityConfiguration _config;

        // Common attack patterns for detection
        private readonly string[] _sqlInjectionPatterns = {
            @"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|CREATE|ALTER|EXEC|UNION)\b)",
            @"(\b(OR|AND)\b\s+\d+\s*=\s*\d+)",
            @"('|\""|;|--|\*|\|)",
            @"(\bSCRIPT\b|\bALERT\b|\bDOCUMENT\b|\bWINDOW\b)"
        };

        private readonly string[] _xssPatterns = {
            @"<script[^>]*>.*?</script>",
            @"javascript:",
            @"vbscript:",
            @"on\w+\s*=",
            @"<iframe[^>]*>",
            @"<object[^>]*>",
            @"<embed[^>]*>",
            @"<link[^>]*>"
        };

        public APISecurityService(
            AdpaEfDbContext context,
            IAuditService auditService,
            ILogger<APISecurityService> logger,
            IOptions<APISecurityConfiguration> config)
        {
            _context = context;
            _auditService = auditService;
            _logger = logger;
            _config = config.Value;
        }

        // Rate Limiting Implementation
        public async Task<RateLimitPolicy> CreateRateLimitPolicyAsync(RateLimitPolicy policy, string userId)
        {
            policy.Id = Guid.NewGuid();
            policy.CreatedAt = DateTime.UtcNow;
            policy.CreatedBy = userId;
            policy.IsActive = true;

            _context.RateLimitPolicies.Add(policy);
            await _context.SaveChangesAsync();

            await _auditService.LogEventAsync("RateLimitPolicyCreated", $"Rate limit policy '{policy.Name}' created", userId);

            _logger.LogInformation($"Rate limit policy created: {policy.Name} by {userId}");
            return policy;
        }

        public async Task<RateLimitPolicy> UpdateRateLimitPolicyAsync(Guid policyId, RateLimitPolicy policy, string userId)
        {
            var existingPolicy = await _context.RateLimitPolicies.FindAsync(policyId);
            if (existingPolicy == null)
                throw new ArgumentException("Rate limit policy not found");

            existingPolicy.Name = policy.Name;
            existingPolicy.Description = policy.Description;
            existingPolicy.RequestLimit = policy.RequestLimit;
            existingPolicy.TimeWindowSeconds = policy.TimeWindowSeconds;
            existingPolicy.Action = policy.Action;
            existingPolicy.IsActive = policy.IsActive;
            existingPolicy.LastModified = DateTime.UtcNow;
            existingPolicy.ModifiedBy = userId;

            await _context.SaveChangesAsync();

            await _auditService.LogEventAsync("RateLimitPolicyUpdated", $"Rate limit policy '{existingPolicy.Name}' updated", userId);

            return existingPolicy;
        }

        public async Task<bool> DeleteRateLimitPolicyAsync(Guid policyId, string userId)
        {
            var policy = await _context.RateLimitPolicies.FindAsync(policyId);
            if (policy == null) return false;

            policy.IsActive = false;
            policy.LastModified = DateTime.UtcNow;
            policy.ModifiedBy = userId;

            await _context.SaveChangesAsync();

            await _auditService.LogEventAsync("RateLimitPolicyDeleted", $"Rate limit policy '{policy.Name}' deactivated", userId);

            return true;
        }

        public async Task<IEnumerable<RateLimitPolicy>> GetRateLimitPoliciesAsync(bool activeOnly = false)
        {
            var query = _context.RateLimitPolicies.AsQueryable();

            if (activeOnly)
                query = query.Where(p => p.IsActive);

            return await query.OrderBy(p => p.Priority).ThenBy(p => p.Name).ToListAsync();
        }

        public async Task<bool> CheckRateLimitAsync(string clientId, string endpoint, string method = "GET")
        {
            if (!_config.EnableRateLimiting)
                return true;

            var policies = await _context.RateLimitPolicies
                .Where(p => p.IsActive && (p.Endpoint == endpoint || p.Endpoint == "*"))
                .OrderBy(p => p.Priority)
                .ToListAsync();

            var cutoffTime = DateTime.UtcNow.AddSeconds(-_config.DefaultTimeWindow);

            foreach (var policy in policies)
            {
                var windowStart = DateTime.UtcNow.AddSeconds(-policy.TimeWindowSeconds);
                
                var violationCount = await _context.RateLimitViolations
                    .CountAsync(v => v.PolicyId == policy.Id && 
                               v.ClientIdentifier == clientId &&
                               v.ViolationTime >= windowStart);

                if (violationCount >= policy.RequestLimit)
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<RateLimitViolation> RecordRateLimitViolationAsync(string clientId, string endpoint, string ipAddress)
        {
            var policy = await _context.RateLimitPolicies
                .Where(p => p.IsActive && (p.Endpoint == endpoint || p.Endpoint == "*"))
                .OrderBy(p => p.Priority)
                .FirstOrDefaultAsync();

            if (policy == null)
                return null;

            var violation = new RateLimitViolation
            {
                Id = Guid.NewGuid(),
                PolicyId = policy.Id,
                ClientIdentifier = clientId,
                IPAddress = ipAddress,
                Endpoint = endpoint,
                ViolationTime = DateTime.UtcNow,
                ActionTaken = policy.Action,
                IsBlocked = policy.Action == ThrottleAction.Block,
                BlockExpiresAt = policy.Action == ThrottleAction.Block ? DateTime.UtcNow.AddMinutes(15) : null
            };

            _context.RateLimitViolations.Add(violation);
            await _context.SaveChangesAsync();

            // Log security event
            await LogSecurityEventAsync(new APISecurityEvent
            {
                EventType = APISecurityEventType.RateLimitExceeded,
                IPAddress = ipAddress,
                Endpoint = endpoint,
                Severity = ValidationSeverity.Warning,
                Description = $"Rate limit exceeded for client {clientId}",
                EventTime = DateTime.UtcNow,
                IsBlocked = violation.IsBlocked
            });

            return violation;
        }

        public async Task<IEnumerable<RateLimitViolation>> GetRateLimitViolationsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.RateLimitViolations
                .Include(v => v.Policy)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(v => v.ViolationTime >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(v => v.ViolationTime <= toDate.Value);

            return await query.OrderByDescending(v => v.ViolationTime).ToListAsync();
        }

        // Input Validation Implementation
        public async Task<InputValidationRule> CreateValidationRuleAsync(InputValidationRule rule, string userId)
        {
            rule.Id = Guid.NewGuid();
            rule.CreatedAt = DateTime.UtcNow;
            rule.CreatedBy = userId;
            rule.IsActive = true;

            _context.InputValidationRules.Add(rule);
            await _context.SaveChangesAsync();

            await _auditService.LogEventAsync("ValidationRuleCreated", $"Validation rule '{rule.RuleName}' created", userId);

            return rule;
        }

        public async Task<InputValidationRule> UpdateValidationRuleAsync(Guid ruleId, InputValidationRule rule, string userId)
        {
            var existingRule = await _context.InputValidationRules.FindAsync(ruleId);
            if (existingRule == null)
                throw new ArgumentException("Validation rule not found");

            existingRule.RuleName = rule.RuleName;
            existingRule.Description = rule.Description;
            existingRule.ValidationPattern = rule.ValidationPattern;
            existingRule.ValidationMessage = rule.ValidationMessage;
            existingRule.Severity = rule.Severity;
            existingRule.BlockOnFailure = rule.BlockOnFailure;
            existingRule.IsActive = rule.IsActive;

            await _context.SaveChangesAsync();

            await _auditService.LogEventAsync("ValidationRuleUpdated", $"Validation rule '{existingRule.RuleName}' updated", userId);

            return existingRule;
        }

        public async Task<bool> DeleteValidationRuleAsync(Guid ruleId, string userId)
        {
            var rule = await _context.InputValidationRules.FindAsync(ruleId);
            if (rule == null) return false;

            rule.IsActive = false;
            await _context.SaveChangesAsync();

            await _auditService.LogEventAsync("ValidationRuleDeleted", $"Validation rule '{rule.RuleName}' deactivated", userId);

            return true;
        }

        public async Task<IEnumerable<InputValidationRule>> GetValidationRulesAsync(string endpoint = null)
        {
            var query = _context.InputValidationRules.Where(r => r.IsActive);

            if (!string.IsNullOrEmpty(endpoint))
                query = query.Where(r => r.TargetEndpoint == endpoint || r.TargetEndpoint == "*");

            return await query.OrderBy(r => r.Priority).ToListAsync();
        }

        public async Task<ValidationResult> ValidateInputAsync(string endpoint, Dictionary<string, object> parameters)
        {
            var result = new ValidationResult { IsValid = true, Errors = new List<string>() };

            if (!_config.EnableInputValidation)
                return result;

            var rules = await GetValidationRulesAsync(endpoint);

            foreach (var rule in rules)
            {
                if (string.IsNullOrEmpty(rule.TargetParameter) || 
                    !parameters.ContainsKey(rule.TargetParameter))
                    continue;

                var value = parameters[rule.TargetParameter]?.ToString() ?? "";
                var isValid = await ValidateValue(value, rule);

                if (!isValid)
                {
                    result.IsValid = false;
                    result.Errors.Add(rule.ValidationMessage ?? $"Validation failed for {rule.TargetParameter}");

                    if (rule.LogViolations)
                    {
                        await RecordValidationViolationAsync(new ValidationViolation
                        {
                            Id = Guid.NewGuid(),
                            RuleId = rule.Id,
                            IPAddress = "Unknown", // This would be provided by middleware
                            Endpoint = endpoint,
                            Parameter = rule.TargetParameter,
                            InputValue = value,
                            Severity = rule.Severity,
                            ViolationTime = DateTime.UtcNow,
                            WasBlocked = rule.BlockOnFailure
                        });
                    }

                    if (rule.BlockOnFailure)
                        break;
                }
            }

            return result;
        }

        private async Task<bool> ValidateValue(string value, InputValidationRule rule)
        {
            switch (rule.ValidationType)
            {
                case InputValidationType.Required:
                    return !string.IsNullOrWhiteSpace(value);

                case InputValidationType.RegularExpression:
                    return Regex.IsMatch(value, rule.ValidationPattern);

                case InputValidationType.SQLInjection:
                    return !await DetectSQLInjectionAsync(value);

                case InputValidationType.XSS:
                    return !await DetectXSSAsync(value);

                case InputValidationType.StringLength:
                    var parts = rule.ValidationPattern.Split(',');
                    if (parts.Length == 2 && 
                        int.TryParse(parts[0], out int min) && 
                        int.TryParse(parts[1], out int max))
                    {
                        return value.Length >= min && value.Length <= max;
                    }
                    return true;

                case InputValidationType.Custom:
                    // Custom validation logic would be implemented here
                    return true;

                default:
                    return true;
            }
        }

        public async Task<ValidationViolation> RecordValidationViolationAsync(ValidationViolation violation)
        {
            _context.ValidationViolations.Add(violation);
            await _context.SaveChangesAsync();

            // Log security event
            await LogSecurityEventAsync(new APISecurityEvent
            {
                EventType = APISecurityEventType.InvalidInput,
                IPAddress = violation.IPAddress,
                Endpoint = violation.Endpoint,
                Severity = violation.Severity,
                Description = $"Validation violation: {violation.Parameter}",
                EventTime = DateTime.UtcNow,
                IsBlocked = violation.WasBlocked
            });

            return violation;
        }

        // Output Sanitization Implementation
        public async Task<SanitizationRule> CreateSanitizationRuleAsync(SanitizationRule rule, string userId)
        {
            rule.Id = Guid.NewGuid();
            rule.CreatedAt = DateTime.UtcNow;
            rule.CreatedBy = userId;
            rule.IsActive = true;

            _context.SanitizationRules.Add(rule);
            await _context.SaveChangesAsync();

            await _auditService.LogEventAsync("SanitizationRuleCreated", $"Sanitization rule '{rule.RuleName}' created", userId);

            return rule;
        }

        public async Task<IEnumerable<SanitizationRule>> GetSanitizationRulesAsync(string endpoint = null)
        {
            var query = _context.SanitizationRules.Where(r => r.IsActive && r.ApplyToOutput);

            if (!string.IsNullOrEmpty(endpoint))
                query = query.Where(r => r.TargetEndpoint == endpoint || r.TargetEndpoint == "*");

            return await query.OrderBy(r => r.Priority).ToListAsync();
        }

        public async Task<string> SanitizeOutputAsync(string content, string endpoint, SanitizationLevel level = SanitizationLevel.Standard)
        {
            if (!_config.EnableOutputSanitization || string.IsNullOrEmpty(content))
                return content;

            var sanitizedContent = content;

            switch (level)
            {
                case SanitizationLevel.Basic:
                    sanitizedContent = SanitizeBasic(sanitizedContent);
                    break;

                case SanitizationLevel.Standard:
                    sanitizedContent = SanitizeStandard(sanitizedContent);
                    break;

                case SanitizationLevel.Strict:
                    sanitizedContent = SanitizeStrict(sanitizedContent);
                    break;

                case SanitizationLevel.Maximum:
                    sanitizedContent = SanitizeMaximum(sanitizedContent);
                    break;
            }

            return sanitizedContent;
        }

        public async Task<object> SanitizeObjectAsync(object data, string endpoint, SanitizationLevel level = SanitizationLevel.Standard)
        {
            if (data == null) return null;

            if (data is string stringData)
            {
                return await SanitizeOutputAsync(stringData, endpoint, level);
            }

            // For complex objects, serialize and sanitize the JSON
            var jsonString = JsonSerializer.Serialize(data);
            var sanitizedJson = await SanitizeOutputAsync(jsonString, endpoint, level);
            
            return JsonSerializer.Deserialize<object>(sanitizedJson);
        }

        private string SanitizeBasic(string content)
        {
            return content
                .Replace("<script", "&lt;script")
                .Replace("</script>", "&lt;/script&gt;")
                .Replace("javascript:", "blocked:");
        }

        private string SanitizeStandard(string content)
        {
            var sanitized = SanitizeBasic(content);
            
            // Remove common XSS patterns
            foreach (var pattern in _xssPatterns)
            {
                sanitized = Regex.Replace(sanitized, pattern, "", RegexOptions.IgnoreCase);
            }

            return sanitized;
        }

        private string SanitizeStrict(string content)
        {
            var sanitized = SanitizeStandard(content);
            
            // Additional strict sanitization
            sanitized = Regex.Replace(sanitized, @"<[^>]*>", ""); // Remove all HTML tags
            sanitized = Regex.Replace(sanitized, @"[^\w\s\-\.,!?]", ""); // Keep only safe characters
            
            return sanitized;
        }

        private string SanitizeMaximum(string content)
        {
            var sanitized = SanitizeStrict(content);
            
            // Maximum sanitization - only alphanumeric and basic punctuation
            sanitized = Regex.Replace(sanitized, @"[^\w\s\.]", "");
            
            return sanitized;
        }

        // API Versioning Implementation
        public async Task<APIVersion> CreateAPIVersionAsync(APIVersion version, string userId)
        {
            version.Id = Guid.NewGuid();
            version.CreatedAt = DateTime.UtcNow;
            version.CreatedBy = userId;
            version.IsActive = true;

            _context.APIVersions.Add(version);
            await _context.SaveChangesAsync();

            await _auditService.LogEventAsync("APIVersionCreated", $"API version '{version.Version}' created", userId);

            return version;
        }

        public async Task<APIVersion> UpdateAPIVersionAsync(Guid versionId, APIVersion version, string userId)
        {
            var existingVersion = await _context.APIVersions.FindAsync(versionId);
            if (existingVersion == null)
                throw new ArgumentException("API version not found");

            existingVersion.Name = version.Name;
            existingVersion.Description = version.Description;
            existingVersion.IsActive = version.IsActive;
            existingVersion.IsDeprecated = version.IsDeprecated;
            existingVersion.DeprecationDate = version.DeprecationDate;
            existingVersion.SunsetDate = version.SunsetDate;

            await _context.SaveChangesAsync();

            await _auditService.LogEventAsync("APIVersionUpdated", $"API version '{existingVersion.Version}' updated", userId);

            return existingVersion;
        }

        public async Task<IEnumerable<APIVersion>> GetAPIVersionsAsync(bool activeOnly = false)
        {
            var query = _context.APIVersions.AsQueryable();

            if (activeOnly)
                query = query.Where(v => v.IsActive);

            return await query.OrderByDescending(v => v.CreatedAt).ToListAsync();
        }

        public async Task<APIEndpoint> CreateAPIEndpointAsync(APIEndpoint endpoint, string userId)
        {
            endpoint.Id = Guid.NewGuid();
            endpoint.CreatedAt = DateTime.UtcNow;

            _context.APIEndpoints.Add(endpoint);
            await _context.SaveChangesAsync();

            await _auditService.LogEventAsync("APIEndpointCreated", $"API endpoint '{endpoint.Path}' created", userId);

            return endpoint;
        }

        public async Task<IEnumerable<APIEndpoint>> GetAPIEndpointsAsync(Guid? versionId = null)
        {
            var query = _context.APIEndpoints.Include(e => e.Version).AsQueryable();

            if (versionId.HasValue)
                query = query.Where(e => e.VersionId == versionId);

            return await query.OrderBy(e => e.Path).ToListAsync();
        }

        public async Task<bool> IsValidAPIVersionAsync(string version)
        {
            return await _context.APIVersions
                .AnyAsync(v => v.Version == version && v.IsActive);
        }

        public async Task<APIVersion> GetDefaultAPIVersionAsync()
        {
            return await _context.APIVersions
                .Where(v => v.IsActive)
                .OrderByDescending(v => v.CreatedAt)
                .FirstOrDefaultAsync();
        }

        // Security Events Implementation
        public async Task<APISecurityEvent> LogSecurityEventAsync(APISecurityEvent securityEvent)
        {
            securityEvent.Id = Guid.NewGuid();
            
            if (securityEvent.EventTime == default)
                securityEvent.EventTime = DateTime.UtcNow;

            _context.APISecurityEvents.Add(securityEvent);
            await _context.SaveChangesAsync();

            _logger.LogWarning($"API Security Event: {securityEvent.EventType} - {securityEvent.Description} from {securityEvent.IPAddress}");

            return securityEvent;
        }

        public async Task<IEnumerable<APISecurityEvent>> GetSecurityEventsAsync(APISecurityEventType? eventType = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.APISecurityEvents.AsQueryable();

            if (eventType.HasValue)
                query = query.Where(e => e.EventType == eventType);

            if (fromDate.HasValue)
                query = query.Where(e => e.EventTime >= fromDate);

            if (toDate.HasValue)
                query = query.Where(e => e.EventTime <= toDate);

            return await query.OrderByDescending(e => e.EventTime).ToListAsync();
        }

        // Threat Detection Implementation
        public async Task<bool> DetectSQLInjectionAsync(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            foreach (var pattern in _sqlInjectionPatterns)
            {
                if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<bool> DetectXSSAsync(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            foreach (var pattern in _xssPatterns)
            {
                if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<bool> DetectCSRFAsync(Dictionary<string, string> headers, string referrer)
        {
            // Basic CSRF detection logic
            if (!headers.ContainsKey("X-Requested-With") && 
                !headers.ContainsKey("X-CSRF-Token"))
            {
                return true;
            }

            return false;
        }

        // Additional helper methods and remaining interface implementations would continue here...
        
        public async Task<bool> IsIPBlacklistedAsync(string ipAddress)
        {
            // Implementation for IP blacklist checking
            return false;
        }

        public async Task<bool> IsSuspiciousActivityAsync(string ipAddress, string endpoint)
        {
            // Implementation for suspicious activity detection
            var recentEvents = await _context.APISecurityEvents
                .Where(e => e.IPAddress == ipAddress && e.EventTime >= DateTime.UtcNow.AddHours(-1))
                .CountAsync();

            return recentEvents > 100; // Threshold for suspicious activity
        }

        public async Task<int> CalculateRiskScoreAsync(string ipAddress, string userAgent, string endpoint)
        {
            int riskScore = 0;

            // Check for suspicious patterns in user agent
            if (string.IsNullOrEmpty(userAgent) || userAgent.Length < 10)
                riskScore += 20;

            // Check recent security events
            var recentEvents = await _context.APISecurityEvents
                .Where(e => e.IPAddress == ipAddress && e.EventTime >= DateTime.UtcNow.AddHours(-24))
                .CountAsync();

            riskScore += Math.Min(recentEvents * 5, 50);

            return Math.Min(riskScore, 100);
        }

        // Placeholder implementations for remaining methods
        public async Task<CORSPolicy> CreateCORSPolicyAsync(CORSPolicy policy, string userId)
        {
            policy.Id = Guid.NewGuid();
            policy.CreatedAt = DateTime.UtcNow;
            policy.CreatedBy = userId;

            _context.CORSPolicies.Add(policy);
            await _context.SaveChangesAsync();

            return policy;
        }

        public async Task<IEnumerable<CORSPolicy>> GetCORSPoliciesAsync(bool activeOnly = false)
        {
            var query = _context.CORSPolicies.AsQueryable();
            if (activeOnly) query = query.Where(p => p.IsActive);
            return await query.ToListAsync();
        }

        public async Task<CORSPolicy> GetCORSPolicyForEndpointAsync(string endpoint)
        {
            return await _context.CORSPolicies
                .Where(p => p.IsActive && (p.TargetEndpoints.Contains(endpoint) || p.TargetEndpoints == "*"))
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsOriginAllowedAsync(string origin, string endpoint)
        {
            var policy = await GetCORSPolicyForEndpointAsync(endpoint);
            return policy?.AllowedOrigins?.Contains(origin) == true || policy?.AllowedOrigins?.Contains("*") == true;
        }

        public async Task<SecurityHeader> CreateSecurityHeaderAsync(SecurityHeader header, string userId)
        {
            header.Id = Guid.NewGuid();
            header.CreatedAt = DateTime.UtcNow;
            header.CreatedBy = userId;

            _context.SecurityHeaders.Add(header);
            await _context.SaveChangesAsync();

            return header;
        }

        public async Task<IEnumerable<SecurityHeader>> GetSecurityHeadersAsync(string endpoint = null)
        {
            var query = _context.SecurityHeaders.Where(h => h.IsActive);
            if (!string.IsNullOrEmpty(endpoint))
                query = query.Where(h => h.TargetEndpoints.Contains(endpoint) || h.TargetEndpoints == "*");
            
            return await query.OrderBy(h => h.Priority).ToListAsync();
        }

        public async Task<Dictionary<string, string>> GetSecurityHeadersForEndpointAsync(string endpoint)
        {
            var headers = await GetSecurityHeadersAsync(endpoint);
            return headers.ToDictionary(h => h.HeaderName, h => h.HeaderValue);
        }

        public async Task<APIKey> CreateAPIKeyAsync(string keyName, string assignedTo, string[] allowedEndpoints, string userId)
        {
            var keyValue = GenerateAPIKey();
            var keyHash = HashAPIKey(keyValue);

            var apiKey = new APIKey
            {
                Id = Guid.NewGuid(),
                KeyName = keyName,
                KeyHash = keyHash,
                KeyPrefix = keyValue.Substring(0, 8),
                AssignedTo = assignedTo,
                AllowedEndpoints = string.Join(",", allowedEndpoints),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.APIKeys.Add(apiKey);
            await _context.SaveChangesAsync();

            return apiKey;
        }

        public async Task<APIKey> UpdateAPIKeyAsync(Guid keyId, APIKey apiKey, string userId)
        {
            var existing = await _context.APIKeys.FindAsync(keyId);
            if (existing == null) throw new ArgumentException("API Key not found");

            existing.KeyName = apiKey.KeyName;
            existing.AssignedTo = apiKey.AssignedTo;
            existing.AllowedEndpoints = apiKey.AllowedEndpoints;
            existing.IsActive = apiKey.IsActive;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> ValidateAPIKeyAsync(string keyValue, string endpoint = null, string ipAddress = null)
        {
            var keyHash = HashAPIKey(keyValue);
            var apiKey = await _context.APIKeys.FirstOrDefaultAsync(k => k.KeyHash == keyHash && k.IsActive);

            if (apiKey == null) return false;

            // Update usage
            apiKey.LastUsed = DateTime.UtcNow;
            apiKey.UsageCount++;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RevokeAPIKeyAsync(Guid keyId, string userId)
        {
            var apiKey = await _context.APIKeys.FindAsync(keyId);
            if (apiKey == null) return false;

            apiKey.IsActive = false;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<APIKey>> GetAPIKeysAsync(bool activeOnly = false)
        {
            var query = _context.APIKeys.AsQueryable();
            if (activeOnly) query = query.Where(k => k.IsActive);
            return await query.OrderByDescending(k => k.CreatedAt).ToListAsync();
        }

        public async Task<APIKeyUsage> RecordAPIKeyUsageAsync(Guid keyId, string endpoint, string ipAddress, int responseCode)
        {
            var usage = new APIKeyUsage
            {
                Id = Guid.NewGuid(),
                APIKeyId = keyId,
                IPAddress = ipAddress,
                Endpoint = endpoint,
                ResponseCode = responseCode,
                UsedAt = DateTime.UtcNow
            };

            _context.APIKeyUsage.Add(usage);
            await _context.SaveChangesAsync();

            return usage;
        }

        public async Task<bool> IsGeolocationAllowedAsync(string ipAddress)
        {
            // Placeholder for geolocation checking
            return true;
        }

        public async Task<string> GetGeolocationAsync(string ipAddress)
        {
            // Placeholder for geolocation lookup
            return "Unknown";
        }

        public async Task<APISecurityConfiguration> GetConfigurationAsync()
        {
            return _config;
        }

        public async Task<bool> UpdateConfigurationAsync(APISecurityConfiguration configuration, string userId)
        {
            // Configuration would typically be stored in database or config file
            await _auditService.LogEventAsync("APISecurityConfigurationUpdated", "API security configuration updated", userId);
            return true;
        }

        public async Task<Dictionary<string, object>> GetSecurityMetricsAsync()
        {
            return new Dictionary<string, object>
            {
                ["TotalSecurityEvents"] = await _context.APISecurityEvents.CountAsync(),
                ["RateLimitViolations"] = await _context.RateLimitViolations.CountAsync(),
                ["ValidationViolations"] = await _context.ValidationViolations.CountAsync(),
                ["ActiveAPIKeys"] = await _context.APIKeys.CountAsync(k => k.IsActive),
                ["ActiveRateLimitPolicies"] = await _context.RateLimitPolicies.CountAsync(p => p.IsActive)
            };
        }

        private string GenerateAPIKey()
        {
            const int keyLength = 32;
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, keyLength)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string HashAPIKey(string apiKey)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(apiKey));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}