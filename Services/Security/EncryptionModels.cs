using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace ADPA.Services.Security;

/// <summary>
/// Phase 5: Data Encryption & Protection Models
/// Comprehensive encryption system with KMS, field-level encryption, data masking, and secure storage
/// </summary>

/// <summary>
/// Encryption algorithm types
/// </summary>
public enum EncryptionAlgorithm
{
    AES256,
    AES192,
    AES128,
    ChaCha20,
    RSA2048,
    RSA4096,
    ECC256,
    ECC384
}

/// <summary>
/// Key types for different encryption purposes
/// </summary>
public enum KeyType
{
    DataEncryption, // DEK - Data Encryption Key
    KeyEncryption,  // KEK - Key Encryption Key
    MasterKey,      // MEK - Master Encryption Key
    Signing,        // Digital signatures
    Certificate,    // SSL/TLS certificates
    ApiKey,         // API access keys
    Session         // Session encryption keys
}

/// <summary>
/// Key status for lifecycle management
/// </summary>
public enum KeyStatus
{
    Active,
    Inactive,
    Rotated,
    Revoked,
    Expired,
    Compromised
}

/// <summary>
/// Data classification levels
/// </summary>
public enum DataClassification
{
    Public,         // No encryption required
    Internal,       // Basic encryption
    Confidential,   // Strong encryption
    Restricted,     // Maximum security encryption
    TopSecret       // Military-grade encryption
}

/// <summary>
/// Encryption key information
/// </summary>
public class EncryptionKey
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public KeyType Type { get; set; }
    public EncryptionAlgorithm Algorithm { get; set; }
    public int KeySize { get; set; }
    public KeyStatus Status { get; set; } = KeyStatus.Active;
    
    // Key data (encrypted with KEK)
    public string EncryptedKeyData { get; set; } = string.Empty;
    public string KeyHash { get; set; } = string.Empty; // For integrity verification
    public string Salt { get; set; } = string.Empty;
    public string InitializationVector { get; set; } = string.Empty;
    
    // Key hierarchy
    public Guid? ParentKeyId { get; set; } // KEK that encrypts this key
    public List<Guid> ChildKeyIds { get; set; } = new();
    
    // Lifecycle
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public DateTime? LastRotatedAt { get; set; }
    public int RotationCount { get; set; }
    
    // Usage tracking
    public int UsageCount { get; set; }
    public int MaxUsageCount { get; set; } = -1; // -1 = unlimited
    
    // Access control
    public List<string> AuthorizedRoles { get; set; } = new();
    public List<Guid> AuthorizedUsers { get; set; } = new();
    public List<string> AuthorizedApplications { get; set; } = new();
    
    // Metadata
    public string CreatedBy { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Field-level encryption configuration
/// </summary>
public class FieldEncryptionConfig
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TableName { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string FullFieldPath { get; set; } = string.Empty; // For nested JSON fields
    public DataClassification Classification { get; set; }
    public EncryptionAlgorithm Algorithm { get; set; }
    public Guid EncryptionKeyId { get; set; }
    public bool IsEnabled { get; set; } = true;
    
    // Encryption options
    public bool UseCompression { get; set; } = false;
    public bool UseSalting { get; set; } = true;
    public string? CustomSalt { get; set; }
    
    // Performance options
    public bool EnableCaching { get; set; } = true;
    public int CacheExpirationMinutes { get; set; } = 30;
    
    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Encrypted data storage
/// </summary>
public class EncryptedData
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string EncryptedValue { get; set; } = string.Empty;
    public string EncryptionMetadata { get; set; } = string.Empty; // JSON metadata
    public Guid EncryptionKeyId { get; set; }
    public EncryptionAlgorithm Algorithm { get; set; }
    public string InitializationVector { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public DateTime EncryptedAt { get; set; } = DateTime.UtcNow;
    public string EncryptedBy { get; set; } = string.Empty;
    public int Version { get; set; } = 1; // For key rotation
}

/// <summary>
/// Data masking configuration
/// </summary>
public class DataMaskingConfig
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string FieldPath { get; set; } = string.Empty;
    public MaskingType MaskingType { get; set; }
    public string MaskingPattern { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    
    // Conditional masking
    public List<MaskingCondition> Conditions { get; set; } = new();
    
    // Role-based masking
    public Dictionary<string, MaskingLevel> RoleMaskingLevels { get; set; } = new();
    
    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Types of data masking
/// </summary>
public enum MaskingType
{
    Full,           // Complete replacement: ********
    Partial,        // Show first/last chars: J***n
    Email,          // Email masking: j***@***.com
    Phone,          // Phone masking: (***) ***-1234
    CreditCard,     // CC masking: ****-****-****-1234
    SSN,            // SSN masking: ***-**-1234
    Custom,         // Custom pattern
    Redaction,      // Complete removal: [REDACTED]
    Hashing,        // One-way hash
    Tokenization,   // Replace with token
    Shuffle         // Randomize characters
}

/// <summary>
/// Masking levels based on user permissions
/// </summary>
public enum MaskingLevel
{
    None,           // No masking
    Light,          // Minimal masking
    Medium,         // Moderate masking
    Heavy,          // Significant masking
    Complete        // Full masking/redaction
}

/// <summary>
/// Conditional masking rule
/// </summary>
public class MaskingCondition
{
    public string PropertyName { get; set; } = string.Empty;
    public string Operator { get; set; } = "equals"; // equals, contains, matches, etc.
    public string Value { get; set; } = string.Empty;
    public MaskingType MaskingTypeOverride { get; set; }
}

/// <summary>
/// Secure document storage
/// </summary>
public class SecureDocument
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Hash { get; set; } = string.Empty; // SHA-256 hash
    public DataClassification Classification { get; set; }
    
    // Encryption
    public Guid EncryptionKeyId { get; set; }
    public EncryptionAlgorithm Algorithm { get; set; }
    public string EncryptedStoragePath { get; set; } = string.Empty;
    public string InitializationVector { get; set; } = string.Empty;
    
    // Versioning
    public int Version { get; set; } = 1;
    public Guid? PreviousVersionId { get; set; }
    public List<Guid> VersionHistory { get; set; } = new();
    
    // Access control
    public Guid OwnerId { get; set; }
    public List<DocumentPermission> Permissions { get; set; } = new();
    public List<DocumentAccessLog> AccessLogs { get; set; } = new();
    
    // Lifecycle
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    
    // Metadata
    public Dictionary<string, object> Metadata { get; set; } = new();
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// Document permission for access control
/// </summary>
public class DocumentPermission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DocumentId { get; set; }
    public Guid? UserId { get; set; }
    public string? RoleName { get; set; }
    public DocumentAccessLevel AccessLevel { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string GrantedBy { get; set; } = string.Empty;
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Document access levels
/// </summary>
public enum DocumentAccessLevel
{
    None,
    Read,
    Write,
    Delete,
    Admin
}

/// <summary>
/// Document access audit log
/// </summary>
public class DocumentAccessLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DocumentId { get; set; }
    public Guid UserId { get; set; }
    public string Action { get; set; } = string.Empty; // Read, Write, Delete, Download, etc.
    public DateTime AccessedAt { get; set; } = DateTime.UtcNow;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public bool WasSuccessful { get; set; } = true;
    public string? FailureReason { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}

/// <summary>
/// Encryption configuration settings
/// </summary>
public class EncryptionConfiguration
{
    public EncryptionAlgorithm DefaultAlgorithm { get; set; } = EncryptionAlgorithm.AES256;
    public int DefaultKeySize { get; set; } = 256;
    public TimeSpan DefaultKeyLifetime { get; set; } = TimeSpan.FromDays(365);
    public TimeSpan KeyRotationInterval { get; set; } = TimeSpan.FromDays(90);
    public bool EnableAutoRotation { get; set; } = true;
    public bool EnableKeyEscrow { get; set; } = false;
    public string KeyEscrowPath { get; set; } = string.Empty;
    
    // Performance settings
    public bool EnableEncryptionCaching { get; set; } = true;
    public int EncryptionCacheExpirationMinutes { get; set; } = 30;
    public int MaxConcurrentEncryptionOperations { get; set; } = 100;
    
    // Security settings
    public bool RequireKeyApproval { get; set; } = true;
    public bool EnableKeyAuditing { get; set; } = true;
    public bool EnableTamperDetection { get; set; } = true;
    public bool EnableHSMIntegration { get; set; } = false;
    public string? HSMProvider { get; set; }
    
    // Compliance settings
    public bool EnableFIPS140Compliance { get; set; } = false;
    public bool EnableCommonCriteria { get; set; } = false;
    public List<string> RequiredCompliance { get; set; } = new();
    
    // Algorithm preferences by classification
    public Dictionary<DataClassification, EncryptionAlgorithm> ClassificationAlgorithms { get; set; } = new()
    {
        [DataClassification.Public] = EncryptionAlgorithm.AES128,
        [DataClassification.Internal] = EncryptionAlgorithm.AES192,
        [DataClassification.Confidential] = EncryptionAlgorithm.AES256,
        [DataClassification.Restricted] = EncryptionAlgorithm.AES256,
        [DataClassification.TopSecret] = EncryptionAlgorithm.AES256
    };
}

/// <summary>
/// Key derivation parameters
/// </summary>
public class KeyDerivationParameters
{
    public string Algorithm { get; set; } = "PBKDF2"; // PBKDF2, Scrypt, Argon2
    public int Iterations { get; set; } = 100000;
    public int SaltSize { get; set; } = 32;
    public int KeySize { get; set; } = 32;
    public HashAlgorithmName HashAlgorithm { get; set; } = HashAlgorithmName.SHA256;
}

/// <summary>
/// Encryption operation result
/// </summary>
public class EncryptionResult
{
    public bool IsSuccessful { get; set; }
    public string? EncryptedData { get; set; }
    public string? InitializationVector { get; set; }
    public string? Salt { get; set; }
    public Guid KeyId { get; set; }
    public EncryptionAlgorithm Algorithm { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Decryption operation result
/// </summary>
public class DecryptionResult
{
    public bool IsSuccessful { get; set; }
    public string? DecryptedData { get; set; }
    public string? ErrorMessage { get; set; }
    public bool WasKeyRotated { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Key management audit log
/// </summary>
public class KeyManagementAuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid KeyId { get; set; }
    public string Action { get; set; } = string.Empty; // Create, Rotate, Revoke, Access, etc.
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string IpAddress { get; set; } = string.Empty;
    public string Application { get; set; } = string.Empty;
    public bool WasSuccessful { get; set; } = true;
    public string? FailureReason { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}

/// <summary>
/// Request models for encryption operations
/// </summary>
public class EncryptDataRequest
{
    [Required]
    public string Data { get; set; } = string.Empty;
    
    public DataClassification? Classification { get; set; }
    public Guid? KeyId { get; set; }
    public EncryptionAlgorithm? Algorithm { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class DecryptDataRequest
{
    [Required]
    public string EncryptedData { get; set; } = string.Empty;
    
    [Required]
    public Guid KeyId { get; set; }
    
    public string? InitializationVector { get; set; }
    public string? Salt { get; set; }
    public EncryptionAlgorithm? Algorithm { get; set; }
}

public class CreateKeyRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    public KeyType Type { get; set; } = KeyType.DataEncryption;
    public EncryptionAlgorithm Algorithm { get; set; } = EncryptionAlgorithm.AES256;
    public int? KeySize { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public List<string> AuthorizedRoles { get; set; } = new();
    public List<Guid> AuthorizedUsers { get; set; } = new();
}

public class UploadSecureDocumentRequest
{
    [Required]
    public IFormFile File { get; set; } = null!;
    
    public DataClassification Classification { get; set; } = DataClassification.Internal;
    public List<string> Tags { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime? ExpiresAt { get; set; }
}

public class MaskDataRequest
{
    [Required]
    public object Data { get; set; } = new();
    
    public string? UserRole { get; set; }
    public List<string> FieldsToMask { get; set; } = new();
    public MaskingLevel? MaskingLevel { get; set; }
}

/// <summary>
/// Response models for encryption operations
/// </summary>
public class KeyInfoResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public KeyType Type { get; set; }
    public EncryptionAlgorithm Algorithm { get; set; }
    public int KeySize { get; set; }
    public KeyStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public int UsageCount { get; set; }
    public List<string> AuthorizedRoles { get; set; } = new();
}

public class SecureDocumentResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public DataClassification Classification { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public List<string> Tags { get; set; } = new();
    public DocumentAccessLevel UserAccessLevel { get; set; }
}

public class EncryptionStatusResponse
{
    public int TotalKeys { get; set; }
    public int ActiveKeys { get; set; }
    public int ExpiredKeys { get; set; }
    public int KeysNearingExpiry { get; set; }
    public int EncryptedFields { get; set; }
    public int SecureDocuments { get; set; }
    public Dictionary<DataClassification, int> ClassificationCounts { get; set; } = new();
    public Dictionary<EncryptionAlgorithm, int> AlgorithmUsage { get; set; } = new();
    public List<KeyInfoResponse> RecentKeyActivity { get; set; } = new();
}