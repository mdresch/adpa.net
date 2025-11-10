#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ADPA.Models.DTOs.Security
{
    // Rate Limiting DTOs
    public class RateLimitPolicyDto
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        [Required]
        public string Endpoint { get; set; }
        
        [Range(1, int.MaxValue)]
        public int RequestLimit { get; set; }
        
        [Range(1, 86400)] // Max 24 hours
        public int TimeWindowSeconds { get; set; }
        
        public string Action { get; set; }
        
        public int Priority { get; set; }
        
        public bool IsActive { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public string CreatedBy { get; set; }
    }

    public class CreateRateLimitPolicyRequest
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        [Required]
        public string Endpoint { get; set; }
        
        [Required]
        [Range(1, int.MaxValue)]
        public int RequestLimit { get; set; }
        
        [Required]
        [Range(1, 86400)]
        public int TimeWindowSeconds { get; set; }
        
        [Required]
        public string Action { get; set; }
        
        public int Priority { get; set; } = 100;
    }

    public class UpdateRateLimitPolicyRequest
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        [Required]
        public string Endpoint { get; set; }
        
        [Required]
        [Range(1, int.MaxValue)]
        public int RequestLimit { get; set; }
        
        [Required]
        [Range(1, 86400)]
        public int TimeWindowSeconds { get; set; }
        
        [Required]
        public string Action { get; set; }
        
        public int Priority { get; set; }
        
        public bool IsActive { get; set; }
    }

    public class RateLimitViolationDto
    {
        public Guid Id { get; set; }
        public Guid PolicyId { get; set; }
        public string PolicyName { get; set; }
        public string ClientIdentifier { get; set; }
        public string IPAddress { get; set; }
        public string Endpoint { get; set; }
        public DateTime ViolationTime { get; set; }
        public string ActionTaken { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime? BlockExpiresAt { get; set; }
    }

    // Input Validation DTOs
    public class InputValidationRuleDto
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string RuleName { get; set; }
        
        public string Description { get; set; }
        
        [Required]
        public string TargetEndpoint { get; set; }
        
        public string TargetParameter { get; set; }
        
        [Required]
        public string ValidationType { get; set; }
        
        public string ValidationPattern { get; set; }
        
        public string ValidationMessage { get; set; }
        
        public string Severity { get; set; }
        
        public bool BlockOnFailure { get; set; }
        
        public bool LogViolations { get; set; }
        
        public int Priority { get; set; }
        
        public bool IsActive { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public string CreatedBy { get; set; }
    }

    public class CreateValidationRuleRequest
    {
        [Required]
        [StringLength(200)]
        public string RuleName { get; set; }
        
        public string Description { get; set; }
        
        [Required]
        public string TargetEndpoint { get; set; }
        
        public string TargetParameter { get; set; }
        
        [Required]
        public string ValidationType { get; set; }
        
        public string ValidationPattern { get; set; }
        
        public string ValidationMessage { get; set; }
        
        public string Severity { get; set; } = "Warning";
        
        public bool BlockOnFailure { get; set; } = true;
        
        public bool LogViolations { get; set; } = true;
        
        public int Priority { get; set; } = 100;
    }

    public class ValidationViolationDto
    {
        public Guid Id { get; set; }
        public Guid RuleId { get; set; }
        public string RuleName { get; set; }
        public string IPAddress { get; set; }
        public string Endpoint { get; set; }
        public string Parameter { get; set; }
        public string InputValue { get; set; }
        public string Severity { get; set; }
        public DateTime ViolationTime { get; set; }
        public bool WasBlocked { get; set; }
    }

    // API Security Event DTOs
    public class APISecurityEventDto
    {
        public Guid Id { get; set; }
        public string EventType { get; set; }
        public string IPAddress { get; set; }
        public string UserAgent { get; set; }
        public string Endpoint { get; set; }
        public string HTTPMethod { get; set; }
        public string Severity { get; set; }
        public string Description { get; set; }
        public DateTime EventTime { get; set; }
        public bool IsBlocked { get; set; }
        public int RiskScore { get; set; }
        public string Geolocation { get; set; }
    }

    // API Version DTOs
    public class APIVersionDto
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Version { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public bool IsActive { get; set; }
        
        public bool IsDeprecated { get; set; }
        
        public DateTime? DeprecationDate { get; set; }
        
        public DateTime? SunsetDate { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public string CreatedBy { get; set; }
    }

    public class CreateAPIVersionRequest
    {
        [Required]
        [StringLength(50)]
        public string Version { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public DateTime? DeprecationDate { get; set; }
        
        public DateTime? SunsetDate { get; set; }
    }

    public class APIEndpointDto
    {
        public Guid Id { get; set; }
        public Guid VersionId { get; set; }
        public string VersionName { get; set; }
        public string Path { get; set; }
        public string HTTPMethod { get; set; }
        public string Description { get; set; }
        public bool RequiresAuthentication { get; set; }
        public string[] RequiredScopes { get; set; }
        public bool IsDeprecated { get; set; }
        public DateTime? DeprecationDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // CORS Policy DTOs
    public class CORSPolicyDto
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string PolicyName { get; set; }
        
        public string Description { get; set; }
        
        public string[] AllowedOrigins { get; set; }
        
        public string[] AllowedMethods { get; set; }
        
        public string[] AllowedHeaders { get; set; }
        
        public string[] ExposedHeaders { get; set; }
        
        public bool AllowCredentials { get; set; }
        
        public int PreflightMaxAge { get; set; }
        
        public string TargetEndpoints { get; set; }
        
        public bool IsActive { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public string CreatedBy { get; set; }
    }

    public class CreateCORSPolicyRequest
    {
        [Required]
        [StringLength(200)]
        public string PolicyName { get; set; }
        
        public string Description { get; set; }
        
        [Required]
        public string[] AllowedOrigins { get; set; }
        
        [Required]
        public string[] AllowedMethods { get; set; }
        
        public string[] AllowedHeaders { get; set; }
        
        public string[] ExposedHeaders { get; set; }
        
        public bool AllowCredentials { get; set; }
        
        public int PreflightMaxAge { get; set; } = 86400;
        
        [Required]
        public string TargetEndpoints { get; set; }
    }

    // Security Header DTOs
    public class SecurityHeaderDto
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string HeaderName { get; set; }
        
        [Required]
        public string HeaderValue { get; set; }
        
        public string Description { get; set; }
        
        public string TargetEndpoints { get; set; }
        
        public int Priority { get; set; }
        
        public bool IsActive { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public string CreatedBy { get; set; }
    }

    public class CreateSecurityHeaderRequest
    {
        [Required]
        [StringLength(100)]
        public string HeaderName { get; set; }
        
        [Required]
        public string HeaderValue { get; set; }
        
        public string Description { get; set; }
        
        [Required]
        public string TargetEndpoints { get; set; }
        
        public int Priority { get; set; } = 100;
    }

    // API Key DTOs
    public class APIKeyDto
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string KeyName { get; set; }
        
        public string KeyPrefix { get; set; }
        
        [Required]
        public string AssignedTo { get; set; }
        
        public string AllowedEndpoints { get; set; }
        
        public DateTime? ExpiresAt { get; set; }
        
        public bool IsActive { get; set; }
        
        public DateTime? LastUsed { get; set; }
        
        public int UsageCount { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public string CreatedBy { get; set; }
    }

    public class CreateAPIKeyRequest
    {
        [Required]
        [StringLength(200)]
        public string KeyName { get; set; }
        
        [Required]
        public string AssignedTo { get; set; }
        
        [Required]
        public string[] AllowedEndpoints { get; set; }
        
        public DateTime? ExpiresAt { get; set; }
    }

    public class CreateAPIKeyResponse
    {
        public Guid Id { get; set; }
        public string KeyName { get; set; }
        public string APIKey { get; set; }
        public string KeyPrefix { get; set; }
        public string AssignedTo { get; set; }
        public string[] AllowedEndpoints { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        
        [System.Text.Json.Serialization.JsonIgnore]
        public string SecurityMessage => "Store this API key securely. It will not be shown again.";
    }

    public class APIKeyUsageDto
    {
        public Guid Id { get; set; }
        public Guid APIKeyId { get; set; }
        public string KeyName { get; set; }
        public string IPAddress { get; set; }
        public string Endpoint { get; set; }
        public int ResponseCode { get; set; }
        public DateTime UsedAt { get; set; }
    }

    // Configuration DTOs
    public class APISecurityConfigurationDto
    {
        public bool EnableRateLimiting { get; set; }
        public bool EnableInputValidation { get; set; }
        public bool EnableOutputSanitization { get; set; }
        public bool EnableSecurityHeaders { get; set; }
        public bool EnableCORSPolicies { get; set; }
        public bool EnableThreatDetection { get; set; }
        public bool EnableAPIKeyValidation { get; set; }
        public bool EnableGeolocationChecking { get; set; }
        public int DefaultRateLimit { get; set; }
        public int DefaultTimeWindow { get; set; }
        public string DefaultSanitizationLevel { get; set; }
        public Dictionary<string, string> DefaultSecurityHeaders { get; set; }
        public Dictionary<string, object> DefaultCORSPolicy { get; set; }
        public int MaxRiskScore { get; set; }
        public string[] BlockedCountries { get; set; }
        public string[] AllowedCountries { get; set; }
    }

    public class UpdateAPISecurityConfigurationRequest
    {
        public bool? EnableRateLimiting { get; set; }
        public bool? EnableInputValidation { get; set; }
        public bool? EnableOutputSanitization { get; set; }
        public bool? EnableSecurityHeaders { get; set; }
        public bool? EnableCORSPolicies { get; set; }
        public bool? EnableThreatDetection { get; set; }
        public bool? EnableAPIKeyValidation { get; set; }
        public bool? EnableGeolocationChecking { get; set; }
        public int? DefaultRateLimit { get; set; }
        public int? DefaultTimeWindow { get; set; }
        public string DefaultSanitizationLevel { get; set; }
        public Dictionary<string, string> DefaultSecurityHeaders { get; set; }
        public Dictionary<string, object> DefaultCORSPolicy { get; set; }
        public int? MaxRiskScore { get; set; }
        public string[] BlockedCountries { get; set; }
        public string[] AllowedCountries { get; set; }
    }

    // Security Metrics DTOs
    public class SecurityMetricsDto
    {
        public int TotalSecurityEvents { get; set; }
        public int RateLimitViolations { get; set; }
        public int ValidationViolations { get; set; }
        public int ActiveAPIKeys { get; set; }
        public int ActiveRateLimitPolicies { get; set; }
        public int ActiveValidationRules { get; set; }
        public int ThreatEvents { get; set; }
        public Dictionary<string, int> EventsByType { get; set; }
        public Dictionary<string, int> ViolationsByEndpoint { get; set; }
        public Dictionary<string, int> TopRiskIPs { get; set; }
        public DateTime ReportGeneratedAt { get; set; }
    }

    // Response DTOs
    public class APISecurityResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class PaginatedAPISecurityResponse<T>
    {
        public bool Success { get; set; }
        public List<T> Data { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}