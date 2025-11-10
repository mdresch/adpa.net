using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace ADPA.Services.Security;

/// <summary>
/// Phase 5: Enhanced Authentication Models
/// Comprehensive authentication system with MFA, OAuth, and advanced security features
/// </summary>

/// <summary>
/// Multi-factor authentication types
/// </summary>
public enum MfaType
{
    None,
    SMS,
    Email,
    TOTP, // Time-based One-Time Password (Google Authenticator, etc.)
    Push, // Push notifications
    Biometric,
    Hardware // Hardware tokens
}

/// <summary>
/// Authentication method types
/// </summary>
public enum AuthenticationMethod
{
    Password,
    OAuth,
    OpenID,
    SAML,
    Biometric,
    Certificate,
    ApiKey
}

/// <summary>
/// Account status enumeration
/// </summary>
public enum AccountStatus
{
    Active,
    Inactive,
    Locked,
    Suspended,
    Pending,
    Expired
}

/// <summary>
/// Enhanced user model with security features
/// </summary>
public class SecureUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    
    // Security Properties
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public DateTime? PasswordChangedAt { get; set; }
    public bool RequirePasswordChange { get; set; }
    
    // Account Status
    public AccountStatus Status { get; set; } = AccountStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public DateTime? LockedUntil { get; set; }
    public int FailedLoginAttempts { get; set; }
    
    // MFA Settings
    public bool MfaEnabled { get; set; }
    public MfaType PreferredMfaType { get; set; } = MfaType.None;
    public List<MfaDevice> MfaDevices { get; set; } = new();
    public string? MfaBackupCodes { get; set; } // Encrypted backup codes
    
    // OAuth/External Authentication
    public List<ExternalAuthProvider> ExternalProviders { get; set; } = new();
    
    // Roles and Permissions
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
    
    // Security Settings
    public bool TwoFactorEnabled { get; set; }
    public string? RecoveryEmail { get; set; }
    public string? RecoveryPhone { get; set; }
    public Dictionary<string, string> SecurityQuestions { get; set; } = new();
    
    // Audit Fields
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Multi-factor authentication device
/// </summary>
public class MfaDevice
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public MfaType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty; // Encrypted
    public bool IsActive { get; set; } = true;
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUsedAt { get; set; }
    public Dictionary<string, object> DeviceInfo { get; set; } = new();
}

/// <summary>
/// External authentication provider configuration
/// </summary>
public class ExternalAuthProvider
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string ProviderName { get; set; } = string.Empty; // Google, Microsoft, etc.
    public string ProviderId { get; set; } = string.Empty;
    public string ProviderUserId { get; set; } = string.Empty;
    public string ProviderEmail { get; set; } = string.Empty;
    public DateTime LinkedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> ProviderData { get; set; } = new();
}

/// <summary>
/// Authentication session
/// </summary>
public class AuthenticationSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string SessionToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string DeviceFingerprint { get; set; } = string.Empty;
    public AuthenticationMethod AuthMethod { get; set; }
    public bool MfaCompleted { get; set; }
    public Dictionary<string, object> SessionData { get; set; } = new();
}

/// <summary>
/// Password policy configuration
/// </summary>
public class PasswordPolicy
{
    public int MinLength { get; set; } = 8;
    public int MaxLength { get; set; } = 128;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireDigits { get; set; } = true;
    public bool RequireSpecialChars { get; set; } = true;
    public int MinUniqueChars { get; set; } = 4;
    public int PasswordHistoryCount { get; set; } = 5; // Prevent reuse of last N passwords
    public int MaxAge { get; set; } = 90; // Days before password expires
    public int LockoutThreshold { get; set; } = 5; // Failed attempts before lockout
    public int LockoutDuration { get; set; } = 30; // Lockout duration in minutes
    public List<string> CommonPasswords { get; set; } = new(); // Blacklisted passwords
}

/// <summary>
/// Login attempt record for security monitoring
/// </summary>
public class LoginAttempt
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public string FailureReason { get; set; } = string.Empty;
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty; // GeoIP location
    public AuthenticationMethod AuthMethod { get; set; }
    public bool MfaRequired { get; set; }
    public bool MfaCompleted { get; set; }
    public string DeviceFingerprint { get; set; } = string.Empty;
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}

/// <summary>
/// OAuth configuration for external providers
/// </summary>
public class OAuthConfiguration
{
    public string ProviderId { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string AuthorizeEndpoint { get; set; } = string.Empty;
    public string TokenEndpoint { get; set; } = string.Empty;
    public string UserInfoEndpoint { get; set; } = string.Empty;
    public string[] Scopes { get; set; } = Array.Empty<string>();
    public bool IsEnabled { get; set; } = true;
    public Dictionary<string, object> AdditionalParameters { get; set; } = new();
}

/// <summary>
/// MFA challenge for two-factor authentication
/// </summary>
public class MfaChallenge
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid DeviceId { get; set; }
    public MfaType Type { get; set; }
    public string Challenge { get; set; } = string.Empty;
    public string? ExpectedResponse { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsSuccessful { get; set; }
    public int AttemptCount { get; set; }
    public string? CompletedResponse { get; set; }
}

/// <summary>
/// Biometric authentication data
/// </summary>
public class BiometricData
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string BiometricType { get; set; } = string.Empty; // Fingerprint, Face, Voice, etc.
    public string EncodedData { get; set; } = string.Empty; // Encrypted biometric template
    public string DeviceId { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUsedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Security configuration settings
/// </summary>
public class SecurityConfiguration
{
    public PasswordPolicy PasswordPolicy { get; set; } = new();
    public int SessionTimeoutMinutes { get; set; } = 60;
    public int RefreshTokenLifetimeHours { get; set; } = 24;
    public bool RequireMfaForAdmins { get; set; } = true;
    public bool EnableBruteForceProtection { get; set; } = true;
    public bool EnableGeoLocationCheck { get; set; } = true;
    public bool EnableDeviceFingerprinting { get; set; } = true;
    public bool RequireEmailVerification { get; set; } = true;
    public bool EnableOAuthProviders { get; set; } = true;
    public List<OAuthConfiguration> OAuthProviders { get; set; } = new();
    public bool EnableBiometricAuth { get; set; } = false;
    public Dictionary<string, object> AdvancedSettings { get; set; } = new();
}

/// <summary>
/// API Key for programmatic access
/// </summary>
public class ApiKey
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string KeyHash { get; set; } = string.Empty;
    public string KeyPrefix { get; set; } = string.Empty; // First few chars for identification
    public List<string> Scopes { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string IpWhitelist { get; set; } = string.Empty; // Comma-separated IPs
    public int UsageCount { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Request models for authentication operations
/// </summary>
public class LoginRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
    
    public bool RememberMe { get; set; }
    public string? MfaCode { get; set; }
    public Guid? MfaDeviceId { get; set; }
}

public class MfaSetupRequest
{
    [Required]
    public MfaType Type { get; set; }
    
    public string DeviceName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public Dictionary<string, object> DeviceInfo { get; set; } = new();
}

public class MfaVerifyRequest
{
    [Required]
    public Guid ChallengeId { get; set; }
    
    [Required]
    public string Code { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;
    
    [Required]
    public string NewPassword { get; set; } = string.Empty;
    
    [Required]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    [Required]
    public string Email { get; set; } = string.Empty;
}

public class SetNewPasswordRequest
{
    [Required]
    public string ResetToken { get; set; } = string.Empty;
    
    [Required]
    public string NewPassword { get; set; } = string.Empty;
    
    [Required]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// Response models for authentication operations
/// </summary>
public class LoginResponse
{
    public bool IsSuccessful { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool RequiresMfa { get; set; }
    public Guid? MfaChallengeId { get; set; }
    public List<MfaType> AvailableMfaTypes { get; set; } = new();
    public SecureUser? User { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}

public class MfaSetupResponse
{
    public bool IsSuccessful { get; set; }
    public Guid? DeviceId { get; set; }
    public string? QrCodeData { get; set; } // For TOTP setup
    public string? BackupCodes { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> SetupData { get; set; } = new();
}

public class SecurityStatusResponse
{
    public bool MfaEnabled { get; set; }
    public List<MfaDevice> MfaDevices { get; set; } = new();
    public List<AuthenticationSession> ActiveSessions { get; set; } = new();
    public List<LoginAttempt> RecentLoginAttempts { get; set; } = new();
    public DateTime? LastPasswordChange { get; set; }
    public bool RequirePasswordChange { get; set; }
    public List<ExternalAuthProvider> LinkedProviders { get; set; } = new();
    public Dictionary<string, object> SecurityMetrics { get; set; } = new();
}