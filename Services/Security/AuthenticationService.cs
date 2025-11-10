using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ADPA.Services.Security;

/// <summary>
/// Phase 5: Enhanced Authentication Service
/// Comprehensive authentication with MFA, OAuth, biometric support, and advanced security features
/// </summary>
public interface IAuthenticationService
{
    // Core Authentication
    Task<LoginResponse> AuthenticateAsync(LoginRequest request);
    Task<LoginResponse> RefreshTokenAsync(string refreshToken);
    Task<bool> LogoutAsync(Guid sessionId);
    Task<bool> LogoutAllSessionsAsync(Guid userId);
    
    // User Management
    Task<SecureUser?> GetUserAsync(Guid userId);
    Task<SecureUser?> GetUserByUsernameAsync(string username);
    Task<SecureUser?> GetUserByEmailAsync(string email);
    Task<SecureUser> CreateUserAsync(SecureUser user, string password);
    Task<bool> UpdateUserAsync(SecureUser user);
    Task<bool> DeactivateUserAsync(Guid userId, string reason);
    
    // Password Management
    Task<bool> ValidatePasswordAsync(string password, PasswordPolicy? policy = null);
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    Task<string> GeneratePasswordResetTokenAsync(Guid userId);
    Task<bool> ResetPasswordAsync(SetNewPasswordRequest request);
    Task<bool> ValidatePasswordHistoryAsync(Guid userId, string newPassword);
    
    // Multi-Factor Authentication
    Task<MfaSetupResponse> SetupMfaDeviceAsync(Guid userId, MfaSetupRequest request);
    Task<MfaChallenge> CreateMfaChallengeAsync(Guid userId, MfaType type);
    Task<bool> VerifyMfaAsync(MfaVerifyRequest request);
    Task<bool> DisableMfaDeviceAsync(Guid userId, Guid deviceId);
    Task<List<string>> GenerateBackupCodesAsync(Guid userId);
    Task<bool> VerifyBackupCodeAsync(Guid userId, string code);
    
    // Session Management
    Task<AuthenticationSession> CreateSessionAsync(Guid userId, AuthenticationMethod method, string ipAddress, string userAgent);
    Task<AuthenticationSession?> GetSessionAsync(Guid sessionId);
    Task<List<AuthenticationSession>> GetUserSessionsAsync(Guid userId);
    Task<bool> ValidateSessionAsync(Guid sessionId);
    Task<bool> ExtendSessionAsync(Guid sessionId);
    
    // OAuth & External Authentication
    Task<string> GenerateOAuthStateAsync(string providerId);
    Task<LoginResponse> AuthenticateWithOAuthAsync(string providerId, string code, string state);
    Task<bool> LinkOAuthProviderAsync(Guid userId, string providerId, string providerUserId, Dictionary<string, object> providerData);
    Task<bool> UnlinkOAuthProviderAsync(Guid userId, Guid providerId);
    
    // Biometric Authentication
    Task<bool> EnrollBiometricAsync(Guid userId, string biometricType, string encodedData, string deviceId);
    Task<bool> AuthenticateWithBiometricAsync(Guid userId, string biometricType, string encodedData, string deviceId);
    Task<bool> RemoveBiometricAsync(Guid userId, Guid biometricId);
    
    // API Key Management
    Task<(string apiKey, ApiKey keyInfo)> CreateApiKeyAsync(Guid userId, string name, List<string> scopes, DateTime? expiresAt = null);
    Task<ApiKey?> ValidateApiKeyAsync(string apiKey);
    Task<bool> RevokeApiKeyAsync(Guid keyId);
    Task<List<ApiKey>> GetUserApiKeysAsync(Guid userId);
    
    // Security Monitoring
    Task<SecurityStatusResponse> GetSecurityStatusAsync(Guid userId);
    Task<List<LoginAttempt>> GetLoginHistoryAsync(Guid userId, int pageSize = 50, int pageNumber = 1);
    Task<bool> CheckForSuspiciousActivityAsync(Guid userId, string ipAddress, string userAgent);
    Task<bool> ReportSecurityEventAsync(string eventType, Guid? userId, Dictionary<string, object> eventData);
    
    // Account Security
    Task<bool> LockAccountAsync(Guid userId, TimeSpan? lockDuration = null, string reason = "");
    Task<bool> UnlockAccountAsync(Guid userId);
    Task<bool> RequirePasswordChangeAsync(Guid userId);
    Task<bool> SendSecurityAlertAsync(Guid userId, string alertType, Dictionary<string, object> alertData);
}

/// <summary>
/// Enhanced Authentication Service Implementation
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IUserStore _userStore;
    private readonly ISessionStore _sessionStore;
    private readonly IPasswordHasher<SecureUser> _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IMfaService _mfaService;
    private readonly IOAuthService _oauthService;
    private readonly IBiometricService _biometricService;
    private readonly ISecurityLogger _securityLogger;
    private readonly IConfiguration _configuration;
    private readonly SecurityConfiguration _securityConfig;
    private readonly PasswordPolicy _passwordPolicy;

    public AuthenticationService(
        IUserStore userStore,
        ISessionStore sessionStore,
        IPasswordHasher<SecureUser> passwordHasher,
        ITokenService tokenService,
        IMfaService mfaService,
        IOAuthService oauthService,
        IBiometricService biometricService,
        ISecurityLogger securityLogger,
        IConfiguration configuration,
        IOptions<SecurityConfiguration> securityConfig)
    {
        _userStore = userStore;
        _sessionStore = sessionStore;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _mfaService = mfaService;
        _oauthService = oauthService;
        _biometricService = biometricService;
        _securityLogger = securityLogger;
        _configuration = configuration;
        _securityConfig = securityConfig.Value;
        _passwordPolicy = _securityConfig.PasswordPolicy;
    }

    public async Task<LoginResponse> AuthenticateAsync(LoginRequest request)
    {
        var loginAttempt = new LoginAttempt
        {
            Username = request.Username,
            AttemptedAt = DateTime.UtcNow,
            AuthMethod = AuthenticationMethod.Password
        };

        try
        {
            // Get user by username or email
            var user = await GetUserByUsernameAsync(request.Username) 
                      ?? await GetUserByEmailAsync(request.Username);

            if (user == null)
            {
                loginAttempt.IsSuccessful = false;
                loginAttempt.FailureReason = "User not found";
                await _securityLogger.LogLoginAttemptAsync(loginAttempt);
                
                return new LoginResponse 
                { 
                    IsSuccessful = false, 
                    ErrorMessage = "Invalid credentials" 
                };
            }

            loginAttempt.Email = user.Email;

            // Check account status
            if (user.Status != AccountStatus.Active)
            {
                loginAttempt.IsSuccessful = false;
                loginAttempt.FailureReason = $"Account status: {user.Status}";
                await _securityLogger.LogLoginAttemptAsync(loginAttempt);
                
                return new LoginResponse 
                { 
                    IsSuccessful = false, 
                    ErrorMessage = "Account is not active" 
                };
            }

            // Check account lockout
            if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
            {
                loginAttempt.IsSuccessful = false;
                loginAttempt.FailureReason = "Account locked";
                await _securityLogger.LogLoginAttemptAsync(loginAttempt);
                
                return new LoginResponse 
                { 
                    IsSuccessful = false, 
                    ErrorMessage = "Account is temporarily locked" 
                };
            }

            // Verify password
            var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (passwordResult == PasswordVerificationResult.Failed)
            {
                await HandleFailedLoginAsync(user);
                
                loginAttempt.IsSuccessful = false;
                loginAttempt.FailureReason = "Invalid password";
                await _securityLogger.LogLoginAttemptAsync(loginAttempt);
                
                return new LoginResponse 
                { 
                    IsSuccessful = false, 
                    ErrorMessage = "Invalid credentials" 
                };
            }

            // Check if MFA is required
            if (user.MfaEnabled)
            {
                // If MFA code provided, verify it
                if (!string.IsNullOrEmpty(request.MfaCode) && request.MfaDeviceId.HasValue)
                {
                    var mfaValid = await _mfaService.VerifyCodeAsync(user.Id, request.MfaDeviceId.Value, request.MfaCode);
                    if (!mfaValid)
                    {
                        loginAttempt.IsSuccessful = false;
                        loginAttempt.FailureReason = "Invalid MFA code";
                        loginAttempt.MfaRequired = true;
                        await _securityLogger.LogLoginAttemptAsync(loginAttempt);
                        
                        return new LoginResponse 
                        { 
                            IsSuccessful = false, 
                            ErrorMessage = "Invalid MFA code" 
                        };
                    }
                    loginAttempt.MfaCompleted = true;
                }
                else
                {
                    // Return challenge for MFA
                    var availableMfaTypes = user.MfaDevices
                        .Where(d => d.IsActive && d.IsVerified)
                        .Select(d => d.Type)
                        .Distinct()
                        .ToList();

                    loginAttempt.IsSuccessful = false;
                    loginAttempt.FailureReason = "MFA required";
                    loginAttempt.MfaRequired = true;
                    await _securityLogger.LogLoginAttemptAsync(loginAttempt);

                    return new LoginResponse
                    {
                        IsSuccessful = false,
                        RequiresMfa = true,
                        AvailableMfaTypes = availableMfaTypes,
                        ErrorMessage = "Multi-factor authentication required"
                    };
                }
            }

            // Successful authentication
            await HandleSuccessfulLoginAsync(user);
            
            // Create session
            var session = await CreateSessionAsync(user.Id, AuthenticationMethod.Password, "", "");
            
            // Generate tokens
            var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id, session.Id);

            loginAttempt.IsSuccessful = true;
            await _securityLogger.LogLoginAttemptAsync(loginAttempt);

            return new LoginResponse
            {
                IsSuccessful = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = session.ExpiresAt,
                User = user
            };
        }
        catch (Exception ex)
        {
            loginAttempt.IsSuccessful = false;
            loginAttempt.FailureReason = "System error";
            await _securityLogger.LogLoginAttemptAsync(loginAttempt);
            await _securityLogger.LogSecurityEventAsync("LoginError", null, new Dictionary<string, object>
            {
                ["Error"] = ex.Message,
                ["Username"] = request.Username
            });

            return new LoginResponse 
            { 
                IsSuccessful = false, 
                ErrorMessage = "Authentication failed" 
            };
        }
    }

    public async Task<bool> ValidatePasswordAsync(string password, PasswordPolicy? policy = null)
    {
        policy ??= _passwordPolicy;

        // Length check
        if (password.Length < policy.MinLength || password.Length > policy.MaxLength)
            return false;

        // Character requirements
        if (policy.RequireUppercase && !password.Any(char.IsUpper))
            return false;

        if (policy.RequireLowercase && !password.Any(char.IsLower))
            return false;

        if (policy.RequireDigits && !password.Any(char.IsDigit))
            return false;

        if (policy.RequireSpecialChars && !password.Any(c => !char.IsLetterOrDigit(c)))
            return false;

        // Unique character count
        if (password.Distinct().Count() < policy.MinUniqueChars)
            return false;

        // Common password check
        if (policy.CommonPasswords.Contains(password.ToLower()))
            return false;

        return true;
    }

    public async Task<SecureUser> CreateUserAsync(SecureUser user, string password)
    {
        if (!await ValidatePasswordAsync(password))
            throw new ArgumentException("Password does not meet policy requirements");

        user.PasswordHash = _passwordHasher.HashPassword(user, password);
        user.PasswordSalt = GenerateSalt();
        user.PasswordChangedAt = DateTime.UtcNow;
        user.CreatedAt = DateTime.UtcNow;
        user.Status = AccountStatus.Active;

        await _userStore.CreateUserAsync(user);
        
        await _securityLogger.LogSecurityEventAsync("UserCreated", user.Id, new Dictionary<string, object>
        {
            ["Username"] = user.Username,
            ["Email"] = user.Email
        });

        return user;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        if (request.NewPassword != request.ConfirmPassword)
            return false;

        var user = await GetUserAsync(userId);
        if (user == null)
            return false;

        // Verify current password
        var currentPasswordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
        if (currentPasswordResult == PasswordVerificationResult.Failed)
            return false;

        // Validate new password
        if (!await ValidatePasswordAsync(request.NewPassword))
            return false;

        // Check password history
        if (!await ValidatePasswordHistoryAsync(userId, request.NewPassword))
            return false;

        // Update password
        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        user.PasswordChangedAt = DateTime.UtcNow;
        user.RequirePasswordChange = false;

        await _userStore.UpdateUserAsync(user);
        
        // Store password history
        await _userStore.StorePasswordHistoryAsync(userId, user.PasswordHash);

        await _securityLogger.LogSecurityEventAsync("PasswordChanged", userId, new Dictionary<string, object>
        {
            ["Username"] = user.Username
        });

        return true;
    }

    public async Task<AuthenticationSession> CreateSessionAsync(Guid userId, AuthenticationMethod method, string ipAddress, string userAgent)
    {
        var session = new AuthenticationSession
        {
            UserId = userId,
            SessionToken = GenerateSessionToken(),
            RefreshToken = GenerateRefreshToken(),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_securityConfig.SessionTimeoutMinutes),
            IpAddress = ipAddress,
            UserAgent = userAgent,
            AuthMethod = method,
            DeviceFingerprint = GenerateDeviceFingerprint(userAgent, ipAddress)
        };

        await _sessionStore.CreateSessionAsync(session);
        return session;
    }

    // Helper methods
    private async Task HandleFailedLoginAsync(SecureUser user)
    {
        user.FailedLoginAttempts++;
        
        if (user.FailedLoginAttempts >= _passwordPolicy.LockoutThreshold)
        {
            user.LockedUntil = DateTime.UtcNow.AddMinutes(_passwordPolicy.LockoutDuration);
            user.Status = AccountStatus.Locked;
            
            await _securityLogger.LogSecurityEventAsync("AccountLocked", user.Id, new Dictionary<string, object>
            {
                ["Username"] = user.Username,
                ["FailedAttempts"] = user.FailedLoginAttempts
            });
        }

        await _userStore.UpdateUserAsync(user);
    }

    private async Task HandleSuccessfulLoginAsync(SecureUser user)
    {
        user.LastLoginAt = DateTime.UtcNow;
        user.LastActivityAt = DateTime.UtcNow;
        user.FailedLoginAttempts = 0;
        
        if (user.Status == AccountStatus.Locked)
        {
            user.Status = AccountStatus.Active;
            user.LockedUntil = null;
        }

        await _userStore.UpdateUserAsync(user);
    }

    private string GenerateSalt() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    private string GenerateSessionToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    private string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    private string GenerateDeviceFingerprint(string userAgent, string ipAddress) => 
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes($"{userAgent}:{ipAddress}")));

    // Placeholder implementations for interface methods
    public Task<LoginResponse> RefreshTokenAsync(string refreshToken) => throw new NotImplementedException();
    public Task<bool> LogoutAsync(Guid sessionId) => throw new NotImplementedException();
    public Task<bool> LogoutAllSessionsAsync(Guid userId) => throw new NotImplementedException();
    public Task<SecureUser?> GetUserAsync(Guid userId) => throw new NotImplementedException();
    public Task<SecureUser?> GetUserByUsernameAsync(string username) => throw new NotImplementedException();
    public Task<SecureUser?> GetUserByEmailAsync(string email) => throw new NotImplementedException();
    public Task<bool> UpdateUserAsync(SecureUser user) => throw new NotImplementedException();
    public Task<bool> DeactivateUserAsync(Guid userId, string reason) => throw new NotImplementedException();
    public Task<string> GeneratePasswordResetTokenAsync(Guid userId) => throw new NotImplementedException();
    public Task<bool> ResetPasswordAsync(SetNewPasswordRequest request) => throw new NotImplementedException();
    public Task<bool> ValidatePasswordHistoryAsync(Guid userId, string newPassword) => throw new NotImplementedException();
    public Task<MfaSetupResponse> SetupMfaDeviceAsync(Guid userId, MfaSetupRequest request) => throw new NotImplementedException();
    public Task<MfaChallenge> CreateMfaChallengeAsync(Guid userId, MfaType type) => throw new NotImplementedException();
    public Task<bool> VerifyMfaAsync(MfaVerifyRequest request) => throw new NotImplementedException();
    public Task<bool> DisableMfaDeviceAsync(Guid userId, Guid deviceId) => throw new NotImplementedException();
    public Task<List<string>> GenerateBackupCodesAsync(Guid userId) => throw new NotImplementedException();
    public Task<bool> VerifyBackupCodeAsync(Guid userId, string code) => throw new NotImplementedException();
    public Task<AuthenticationSession?> GetSessionAsync(Guid sessionId) => throw new NotImplementedException();
    public Task<List<AuthenticationSession>> GetUserSessionsAsync(Guid userId) => throw new NotImplementedException();
    public Task<bool> ValidateSessionAsync(Guid sessionId) => throw new NotImplementedException();
    public Task<bool> ExtendSessionAsync(Guid sessionId) => throw new NotImplementedException();
    public Task<string> GenerateOAuthStateAsync(string providerId) => throw new NotImplementedException();
    public Task<LoginResponse> AuthenticateWithOAuthAsync(string providerId, string code, string state) => throw new NotImplementedException();
    public Task<bool> LinkOAuthProviderAsync(Guid userId, string providerId, string providerUserId, Dictionary<string, object> providerData) => throw new NotImplementedException();
    public Task<bool> UnlinkOAuthProviderAsync(Guid userId, Guid providerId) => throw new NotImplementedException();
    public Task<bool> EnrollBiometricAsync(Guid userId, string biometricType, string encodedData, string deviceId) => throw new NotImplementedException();
    public Task<bool> AuthenticateWithBiometricAsync(Guid userId, string biometricType, string encodedData, string deviceId) => throw new NotImplementedException();
    public Task<bool> RemoveBiometricAsync(Guid userId, Guid biometricId) => throw new NotImplementedException();
    public Task<(string apiKey, ApiKey keyInfo)> CreateApiKeyAsync(Guid userId, string name, List<string> scopes, DateTime? expiresAt = null) => throw new NotImplementedException();
    public Task<ApiKey?> ValidateApiKeyAsync(string apiKey) => throw new NotImplementedException();
    public Task<bool> RevokeApiKeyAsync(Guid keyId) => throw new NotImplementedException();
    public Task<List<ApiKey>> GetUserApiKeysAsync(Guid userId) => throw new NotImplementedException();
    public Task<SecurityStatusResponse> GetSecurityStatusAsync(Guid userId) => throw new NotImplementedException();
    public Task<List<LoginAttempt>> GetLoginHistoryAsync(Guid userId, int pageSize = 50, int pageNumber = 1) => throw new NotImplementedException();
    public Task<bool> CheckForSuspiciousActivityAsync(Guid userId, string ipAddress, string userAgent) => throw new NotImplementedException();
    public Task<bool> ReportSecurityEventAsync(string eventType, Guid? userId, Dictionary<string, object> eventData) => throw new NotImplementedException();
    public Task<bool> LockAccountAsync(Guid userId, TimeSpan? lockDuration = null, string reason = "") => throw new NotImplementedException();
    public Task<bool> UnlockAccountAsync(Guid userId) => throw new NotImplementedException();
    public Task<bool> RequirePasswordChangeAsync(Guid userId) => throw new NotImplementedException();
    public Task<bool> SendSecurityAlertAsync(Guid userId, string alertType, Dictionary<string, object> alertData) => throw new NotImplementedException();
}

/// <summary>
/// Supporting service interfaces (to be implemented)
/// </summary>
public interface IUserStore
{
    Task<SecureUser?> GetByIdAsync(Guid id);
    Task<SecureUser?> GetByUsernameAsync(string username);
    Task<SecureUser?> GetByEmailAsync(string email);
    Task<SecureUser> CreateUserAsync(SecureUser user);
    Task<bool> UpdateUserAsync(SecureUser user);
    Task<bool> DeleteUserAsync(Guid id);
    Task StorePasswordHistoryAsync(Guid userId, string passwordHash);
    Task<List<string>> GetPasswordHistoryAsync(Guid userId, int count);
}

public interface ISessionStore
{
    Task<AuthenticationSession> CreateSessionAsync(AuthenticationSession session);
    Task<AuthenticationSession?> GetSessionAsync(Guid sessionId);
    Task<List<AuthenticationSession>> GetUserSessionsAsync(Guid userId);
    Task<bool> UpdateSessionAsync(AuthenticationSession session);
    Task<bool> DeleteSessionAsync(Guid sessionId);
    Task<bool> DeleteUserSessionsAsync(Guid userId);
}

public interface ITokenService
{
    Task<string> GenerateAccessTokenAsync(SecureUser user);
    Task<string> GenerateRefreshTokenAsync(Guid userId, Guid sessionId);
    Task<bool> ValidateTokenAsync(string token);
    Task<ClaimsPrincipal?> GetPrincipalFromTokenAsync(string token);
}

public interface IMfaService
{
    Task<MfaDevice> SetupTotpAsync(Guid userId, string deviceName);
    Task<bool> VerifyCodeAsync(Guid userId, Guid deviceId, string code);
    Task<MfaChallenge> SendSmsCodeAsync(Guid userId, string phoneNumber);
    Task<MfaChallenge> SendEmailCodeAsync(Guid userId, string email);
}

public interface IOAuthService
{
    Task<string> GetAuthorizationUrlAsync(string providerId, string state, List<string> scopes);
    Task<Dictionary<string, object>> ExchangeCodeForTokenAsync(string providerId, string code);
    Task<Dictionary<string, object>> GetUserInfoAsync(string providerId, string accessToken);
}

public interface IBiometricService
{
    Task<bool> EnrollBiometricAsync(Guid userId, string biometricType, string template, string deviceId);
    Task<bool> VerifyBiometricAsync(Guid userId, string biometricType, string template, string deviceId);
    Task<List<BiometricData>> GetUserBiometricsAsync(Guid userId);
}

public interface ISecurityLogger
{
    Task LogLoginAttemptAsync(LoginAttempt attempt);
    Task LogSecurityEventAsync(string eventType, Guid? userId, Dictionary<string, object> eventData);
    Task<List<LoginAttempt>> GetLoginHistoryAsync(Guid userId, int pageSize, int pageNumber);
}