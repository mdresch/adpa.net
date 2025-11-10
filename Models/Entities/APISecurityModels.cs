using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADPA.Models.Entities
{
    // API Security Enums
    public enum RateLimitType
    {
        PerUser,
        PerIP,
        PerEndpoint,
        PerAPIKey,
        Global
    }

    public enum ThrottleAction
    {
        Block,
        Delay,
        Log,
        Redirect
    }

    public enum ValidationSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }

    public enum APISecurityEventType
    {
        RateLimitExceeded,
        InvalidInput,
        SuspiciousActivity,
        UnauthorizedAccess,
        MaliciousPayload,
        SQLInjectionAttempt,
        XSSAttempt,
        CSRFAttempt,
        InvalidAPIKey,
        ExcessiveRequests
    }

    public enum InputValidationType
    {
        Required,
        DataType,
        Range,
        StringLength,
        RegularExpression,
        Custom,
        SQLInjection,
        XSS,
        CSRF
    }

    public enum SanitizationLevel
    {
        None,
        Basic,
        Standard,
        Strict,
        Maximum
    }

    // Rate Limiting Models
    [Table("RateLimitPolicies")]
    public class RateLimitPolicy
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public RateLimitType LimitType { get; set; }

        [Required]
        public string Endpoint { get; set; }

        public string Method { get; set; }

        public int RequestLimit { get; set; }

        public int TimeWindowSeconds { get; set; }

        public bool IsActive { get; set; }

        public int Priority { get; set; }

        public ThrottleAction Action { get; set; }

        public string ActionParameters { get; set; }

        public bool EnableWhitelist { get; set; }

        public string WhitelistIPs { get; set; }

        public bool EnableBlacklist { get; set; }

        public string BlacklistIPs { get; set; }

        public DateTime CreatedAt { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? LastModified { get; set; }

        public string ModifiedBy { get; set; }

        // Navigation Properties
        public virtual ICollection<RateLimitViolation> Violations { get; set; } = new List<RateLimitViolation>();
    }

    [Table("RateLimitViolations")]
    public class RateLimitViolation
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid PolicyId { get; set; }

        [Required]
        public string ClientIdentifier { get; set; }

        [Required]
        public string IPAddress { get; set; }

        public string UserAgent { get; set; }

        [Required]
        public string Endpoint { get; set; }

        public string Method { get; set; }

        public int RequestCount { get; set; }

        public int TimeWindowSeconds { get; set; }

        public DateTime ViolationTime { get; set; }

        public ThrottleAction ActionTaken { get; set; }

        public string ActionResult { get; set; }

        public bool IsBlocked { get; set; }

        public DateTime? BlockExpiresAt { get; set; }

        public string AdditionalData { get; set; }

        // Navigation Properties
        [ForeignKey("PolicyId")]
        public virtual RateLimitPolicy Policy { get; set; }
    }

    // Input Validation Models
    [Table("InputValidationRules")]
    public class InputValidationRule
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string RuleName { get; set; }

        public string Description { get; set; }

        [Required]
        public string TargetEndpoint { get; set; }

        public string TargetParameter { get; set; }

        [Required]
        public InputValidationType ValidationType { get; set; }

        [Required]
        public string ValidationPattern { get; set; }

        public string ValidationMessage { get; set; }

        public ValidationSeverity Severity { get; set; }

        public bool IsActive { get; set; }

        public int Priority { get; set; }

        public bool BlockOnFailure { get; set; }

        public bool LogViolations { get; set; }

        public string CustomValidationCode { get; set; }

        public DateTime CreatedAt { get; set; }

        public string CreatedBy { get; set; }

        // Navigation Properties
        public virtual ICollection<ValidationViolation> Violations { get; set; } = new List<ValidationViolation>();
    }

    [Table("ValidationViolations")]
    public class ValidationViolation
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid RuleId { get; set; }

        [Required]
        public string IPAddress { get; set; }

        public string UserAgent { get; set; }

        [Required]
        public string Endpoint { get; set; }

        public string Parameter { get; set; }

        public string InputValue { get; set; }

        public string ExpectedFormat { get; set; }

        public ValidationSeverity Severity { get; set; }

        public DateTime ViolationTime { get; set; }

        public bool WasBlocked { get; set; }

        public string ActionTaken { get; set; }

        public string AdditionalData { get; set; }

        // Navigation Properties
        [ForeignKey("RuleId")]
        public virtual InputValidationRule Rule { get; set; }
    }

    // API Security Events
    [Table("APISecurityEvents")]
    public class APISecurityEvent
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public APISecurityEventType EventType { get; set; }

        [Required]
        public string IPAddress { get; set; }

        public string UserAgent { get; set; }

        [Required]
        public string Endpoint { get; set; }

        public string Method { get; set; }

        public string UserId { get; set; }

        public string SessionId { get; set; }

        [Required]
        public ValidationSeverity Severity { get; set; }

        [Required]
        public string Description { get; set; }

        public string RequestHeaders { get; set; }

        public string RequestBody { get; set; }

        public string ResponseCode { get; set; }

        public string ActionTaken { get; set; }

        public DateTime EventTime { get; set; }

        public bool IsBlocked { get; set; }

        public int RiskScore { get; set; }

        public string ThreatIntelligence { get; set; }

        public string Geolocation { get; set; }

        public string AdditionalMetadata { get; set; }
    }

    // API Versioning Models
    [Table("APIVersions")]
    public class APIVersion
    {
        [Key]
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

        public string CompatibilityNotes { get; set; }

        public string ChangeLog { get; set; }

        public DateTime CreatedAt { get; set; }

        public string CreatedBy { get; set; }

        // Navigation Properties
        public virtual ICollection<APIEndpoint> Endpoints { get; set; } = new List<APIEndpoint>();
    }

    [Table("APIEndpoints")]
    public class APIEndpoint
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid VersionId { get; set; }

        [Required]
        [StringLength(500)]
        public string Path { get; set; }

        [Required]
        [StringLength(10)]
        public string Method { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        public string Description { get; set; }

        public bool RequiresAuthentication { get; set; }

        public string RequiredRoles { get; set; }

        public string RequiredPermissions { get; set; }

        public bool IsDeprecated { get; set; }

        public DateTime? DeprecationDate { get; set; }

        public string AlternativeEndpoint { get; set; }

        public int MaxRequestSize { get; set; }

        public int TimeoutSeconds { get; set; }

        public bool EnableCaching { get; set; }

        public int CacheDurationSeconds { get; set; }

        public SanitizationLevel InputSanitizationLevel { get; set; }

        public SanitizationLevel OutputSanitizationLevel { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation Properties
        [ForeignKey("VersionId")]
        public virtual APIVersion Version { get; set; }
    }

    // Output Sanitization Models
    [Table("SanitizationRules")]
    public class SanitizationRule
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string RuleName { get; set; }

        public string Description { get; set; }

        [Required]
        public string TargetEndpoint { get; set; }

        public string TargetField { get; set; }

        [Required]
        public SanitizationLevel Level { get; set; }

        [Required]
        public string SanitizationPattern { get; set; }

        public string ReplacementValue { get; set; }

        public bool IsActive { get; set; }

        public int Priority { get; set; }

        public bool ApplyToInput { get; set; }

        public bool ApplyToOutput { get; set; }

        public string CustomSanitizationCode { get; set; }

        public DateTime CreatedAt { get; set; }

        public string CreatedBy { get; set; }
    }

    // CORS Configuration
    [Table("CORSPolicies")]
    public class CORSPolicy
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string PolicyName { get; set; }

        public string Description { get; set; }

        public string AllowedOrigins { get; set; }

        public string AllowedMethods { get; set; }

        public string AllowedHeaders { get; set; }

        public string ExposedHeaders { get; set; }

        public bool AllowCredentials { get; set; }

        public int MaxAge { get; set; }

        public bool IsActive { get; set; }

        public int Priority { get; set; }

        public string TargetEndpoints { get; set; }

        public DateTime CreatedAt { get; set; }

        public string CreatedBy { get; set; }
    }

    // Security Headers
    [Table("SecurityHeaders")]
    public class SecurityHeader
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string HeaderName { get; set; }

        [Required]
        public string HeaderValue { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }

        public int Priority { get; set; }

        public string TargetEndpoints { get; set; }

        public bool OverrideExisting { get; set; }

        public DateTime CreatedAt { get; set; }

        public string CreatedBy { get; set; }
    }

    // API Key Management
    [Table("APIKeys")]
    public class APIKey
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string KeyName { get; set; }

        [Required]
        [StringLength(500)]
        public string KeyHash { get; set; }

        public string KeyPrefix { get; set; }

        public string Description { get; set; }

        public string AssignedTo { get; set; }

        public string AllowedEndpoints { get; set; }

        public string AllowedMethods { get; set; }

        public string AllowedIPs { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public DateTime? LastUsed { get; set; }

        public int UsageCount { get; set; }

        public int RateLimit { get; set; }

        public string Permissions { get; set; }

        public string CreatedBy { get; set; }

        // Navigation Properties
        public virtual ICollection<APIKeyUsage> UsageHistory { get; set; } = new List<APIKeyUsage>();
    }

    [Table("APIKeyUsage")]
    public class APIKeyUsage
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid APIKeyId { get; set; }

        [Required]
        public string IPAddress { get; set; }

        public string UserAgent { get; set; }

        [Required]
        public string Endpoint { get; set; }

        public string Method { get; set; }

        public int ResponseCode { get; set; }

        public DateTime UsedAt { get; set; }

        public int ResponseTime { get; set; }

        public string RequestSize { get; set; }

        public string ResponseSize { get; set; }

        // Navigation Properties
        [ForeignKey("APIKeyId")]
        public virtual APIKey APIKey { get; set; }
    }

    // API Security Configuration
    public class APISecurityConfiguration
    {
        public bool EnableRateLimiting { get; set; } = true;
        public bool EnableInputValidation { get; set; } = true;
        public bool EnableOutputSanitization { get; set; } = true;
        public bool EnableSecurityHeaders { get; set; } = true;
        public bool EnableAPIVersioning { get; set; } = true;
        public bool EnableAPIKeyAuthentication { get; set; } = false;
        public bool EnableCORSProtection { get; set; } = true;
        public bool EnableRequestLogging { get; set; } = true;

        public int DefaultRateLimit { get; set; } = 1000;
        public int DefaultTimeWindow { get; set; } = 3600; // 1 hour
        public int MaxRequestSize { get; set; } = 10485760; // 10MB
        public int DefaultTimeout { get; set; } = 30; // 30 seconds
        
        public string DefaultAPIVersion { get; set; } = "v1";
        public bool RequireAPIVersionHeader { get; set; } = false;
        public string APIVersionHeader { get; set; } = "X-API-Version";
        
        public SanitizationLevel DefaultInputSanitization { get; set; } = SanitizationLevel.Standard;
        public SanitizationLevel DefaultOutputSanitization { get; set; } = SanitizationLevel.Basic;
        
        public bool BlockSuspiciousRequests { get; set; } = true;
        public int ThreatDetectionThreshold { get; set; } = 75;
        public bool EnableGeolocationBlocking { get; set; } = false;
        public string BlockedCountries { get; set; } = string.Empty;
        
        public Dictionary<string, string> CustomSecurityHeaders { get; set; } = new Dictionary<string, string>
        {
            ["X-Content-Type-Options"] = "nosniff",
            ["X-Frame-Options"] = "DENY",
            ["X-XSS-Protection"] = "1; mode=block",
            ["Referrer-Policy"] = "strict-origin-when-cross-origin",
            ["Content-Security-Policy"] = "default-src 'self'",
            ["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains"
        };
        
        public string[] DefaultAllowedOrigins { get; set; } = { "https://localhost:7050" };
        public string[] DefaultAllowedMethods { get; set; } = { "GET", "POST", "PUT", "DELETE", "OPTIONS" };
        public string[] DefaultAllowedHeaders { get; set; } = { "Content-Type", "Authorization", "X-API-Version" };
        
        // Additional properties expected by Program.cs
        public int MaxRiskScore { get; set; } = 100;
        public string DefaultSanitizationLevel { get; set; } = "Standard";
        public Dictionary<string, string> DefaultSecurityHeaders { get; set; } = new Dictionary<string, string>
        {
            ["X-Content-Type-Options"] = "nosniff",
            ["X-Frame-Options"] = "DENY",
            ["X-XSS-Protection"] = "1; mode=block"
        };
        public string DefaultCORSPolicy { get; set; } = "DefaultPolicy";
        public string[] AllowedCountries { get; set; } = Array.Empty<string>();
    }
}