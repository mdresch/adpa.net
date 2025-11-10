using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

namespace ADPA.Services.Security;

/// <summary>
/// Phase 5: Data Encryption & Protection Service Interface
/// Comprehensive encryption system with KMS, field-level encryption, and data masking
/// </summary>
public interface IEncryptionService
{
    // Core Encryption/Decryption
    Task<EncryptionResult> EncryptAsync(string data, DataClassification classification, Guid? keyId = null);
    Task<DecryptionResult> DecryptAsync(string encryptedData, Guid keyId, string? iv = null, string? salt = null);
    Task<EncryptionResult> EncryptFieldAsync(string tableName, string fieldName, string data);
    Task<DecryptionResult> DecryptFieldAsync(string tableName, string fieldName, string encryptedData);
    
    // Key Management
    Task<EncryptionKey> CreateKeyAsync(CreateKeyRequest request);
    Task<EncryptionKey?> GetKeyAsync(Guid keyId);
    Task<List<KeyInfoResponse>> GetKeysAsync(KeyType? type = null, KeyStatus? status = null);
    Task<bool> RotateKeyAsync(Guid keyId);
    Task<bool> RevokeKeyAsync(Guid keyId, string reason);
    Task<EncryptionKey> GenerateKeyAsync(KeyType type, EncryptionAlgorithm algorithm, int keySize);
    
    // Field-Level Encryption
    Task<FieldEncryptionConfig> ConfigureFieldEncryptionAsync(string tableName, string fieldName, DataClassification classification, Guid keyId);
    Task<List<FieldEncryptionConfig>> GetFieldEncryptionConfigsAsync(string? tableName = null);
    Task<bool> EnableFieldEncryptionAsync(Guid configId);
    Task<bool> DisableFieldEncryptionAsync(Guid configId);
    
    // Data Masking
    Task<object> MaskDataAsync(object data, string? userRole = null, MaskingLevel? level = null);
    Task<string> MaskStringAsync(string data, MaskingType type, string? pattern = null);
    Task<DataMaskingConfig> CreateMaskingConfigAsync(string entityType, string fieldPath, MaskingType maskingType);
    Task<List<DataMaskingConfig>> GetMaskingConfigsAsync(string? entityType = null);
    
    // Secure Document Storage
    Task<SecureDocument> UploadSecureDocumentAsync(Stream documentStream, string fileName, string contentType, DataClassification classification);
    Task<Stream?> DownloadSecureDocumentAsync(Guid documentId, Guid userId);
    Task<SecureDocument?> GetSecureDocumentAsync(Guid documentId);
    Task<List<SecureDocumentResponse>> GetUserSecureDocumentsAsync(Guid userId);
    Task<bool> DeleteSecureDocumentAsync(Guid documentId, Guid userId);
    
    // Key Derivation
    Task<byte[]> DeriveKeyAsync(string password, byte[] salt, KeyDerivationParameters parameters);
    Task<byte[]> GenerateSaltAsync(int size = 32);
    Task<string> GenerateSecureRandomStringAsync(int length = 32);
    
    // Hash Functions
    Task<string> HashDataAsync(string data, HashAlgorithmName? algorithm = null);
    Task<bool> VerifyHashAsync(string data, string hash, HashAlgorithmName? algorithm = null);
    
    // Certificate Management
    Task<X509Certificate2> GenerateSelfSignedCertificateAsync(string subjectName, TimeSpan validity);
    Task<bool> ValidateCertificateAsync(X509Certificate2 certificate);
    
    // Compliance & Audit
    Task<EncryptionStatusResponse> GetEncryptionStatusAsync();
    Task<List<KeyManagementAuditLog>> GetKeyAuditLogsAsync(Guid? keyId = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task LogKeyUsageAsync(Guid keyId, string action, Guid userId, bool wasSuccessful = true, string? failureReason = null);
    
    // Configuration
    Task<EncryptionConfiguration> GetConfigurationAsync();
    Task<bool> UpdateConfigurationAsync(EncryptionConfiguration config);
    Task<bool> ValidateConfigurationAsync(EncryptionConfiguration config);
    
    // Backup & Recovery
    Task<byte[]> ExportKeyAsync(Guid keyId, string password);
    Task<EncryptionKey> ImportKeyAsync(byte[] keyData, string password, CreateKeyRequest metadata);
    Task<bool> BackupKeysAsync(string backupPath, string password);
    Task<bool> RestoreKeysAsync(string backupPath, string password);
}

/// <summary>
/// Data Encryption & Protection Service Implementation
/// </summary>
public class EncryptionService : IEncryptionService
{
    private readonly IEncryptionKeyStore _keyStore;
    private readonly IFieldEncryptionStore _fieldConfigStore;
    private readonly IDataMaskingStore _maskingStore;
    private readonly ISecureDocumentStore _documentStore;
    private readonly IEncryptionAuditLogger _auditLogger;
    private readonly ILogger<EncryptionService> _logger;
    private readonly EncryptionConfiguration _config;

    public EncryptionService(
        IEncryptionKeyStore keyStore,
        IFieldEncryptionStore fieldConfigStore,
        IDataMaskingStore maskingStore,
        ISecureDocumentStore documentStore,
        IEncryptionAuditLogger auditLogger,
        ILogger<EncryptionService> logger,
        IOptions<EncryptionConfiguration> config)
    {
        _keyStore = keyStore;
        _fieldConfigStore = fieldConfigStore;
        _maskingStore = maskingStore;
        _documentStore = documentStore;
        _auditLogger = auditLogger;
        _logger = logger;
        _config = config.Value;
    }

    public async Task<EncryptionResult> EncryptAsync(string data, DataClassification classification, Guid? keyId = null)
    {
        try
        {
            _logger.LogDebug("Encrypting data with classification {Classification}", classification);

            // Get or create appropriate key
            EncryptionKey key;
            if (keyId.HasValue)
            {
                var keyResult = await _keyStore.GetKeyAsync(keyId.Value);
                if (keyResult == null || keyResult.Status != KeyStatus.Active)
                {
                    return new EncryptionResult 
                    { 
                        IsSuccessful = false, 
                        ErrorMessage = "Invalid or inactive encryption key" 
                    };
                }
                key = keyResult;
            }
            else
            {
                // Get default key for classification
                key = await GetDefaultKeyForClassificationAsync(classification);
            }

            // Generate IV and salt
            var iv = GenerateIV(key.Algorithm);
            var salt = await GenerateSaltAsync();

            // Encrypt data
            var encryptedBytes = await EncryptBytesAsync(Encoding.UTF8.GetBytes(data), key, iv, salt);
            var encryptedBase64 = Convert.ToBase64String(encryptedBytes);

            // Update key usage
            await UpdateKeyUsageAsync(key.Id);

            return new EncryptionResult
            {
                IsSuccessful = true,
                EncryptedData = encryptedBase64,
                InitializationVector = Convert.ToBase64String(iv),
                Salt = Convert.ToBase64String(salt),
                KeyId = key.Id,
                Algorithm = key.Algorithm
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to encrypt data with classification {Classification}", classification);
            return new EncryptionResult 
            { 
                IsSuccessful = false, 
                ErrorMessage = "Encryption failed" 
            };
        }
    }

    public async Task<DecryptionResult> DecryptAsync(string encryptedData, Guid keyId, string? iv = null, string? salt = null)
    {
        try
        {
            _logger.LogDebug("Decrypting data with key {KeyId}", keyId);

            var key = await _keyStore.GetKeyAsync(keyId);
            if (key == null)
            {
                return new DecryptionResult 
                { 
                    IsSuccessful = false, 
                    ErrorMessage = "Encryption key not found" 
                };
            }

            // Handle key rotation
            if (key.Status == KeyStatus.Rotated)
            {
                // Try to find current active key
                var activeKey = await _keyStore.GetActiveKeyForRotatedKeyAsync(keyId);
                if (activeKey != null)
                {
                    key = activeKey;
                }
            }

            var encryptedBytes = Convert.FromBase64String(encryptedData);
            var ivBytes = !string.IsNullOrEmpty(iv) ? Convert.FromBase64String(iv) : new byte[16];
            var saltBytes = !string.IsNullOrEmpty(salt) ? Convert.FromBase64String(salt) : new byte[32];

            // Decrypt data
            var decryptedBytes = await DecryptBytesAsync(encryptedBytes, key, ivBytes, saltBytes);
            var decryptedData = Encoding.UTF8.GetString(decryptedBytes);

            // Update key usage
            await UpdateKeyUsageAsync(key.Id);

            return new DecryptionResult
            {
                IsSuccessful = true,
                DecryptedData = decryptedData,
                WasKeyRotated = key.Status == KeyStatus.Rotated
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt data with key {KeyId}", keyId);
            return new DecryptionResult 
            { 
                IsSuccessful = false, 
                ErrorMessage = "Decryption failed" 
            };
        }
    }

    public async Task<EncryptionKey> CreateKeyAsync(CreateKeyRequest request)
    {
        _logger.LogInformation("Creating new encryption key {KeyName}", request.Name);

        var key = new EncryptionKey
        {
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            Algorithm = request.Algorithm,
            KeySize = request.KeySize ?? GetDefaultKeySize(request.Algorithm),
            Status = KeyStatus.Active,
            ExpiresAt = request.ExpiresAt ?? DateTime.UtcNow.Add(_config.DefaultKeyLifetime),
            AuthorizedRoles = request.AuthorizedRoles,
            AuthorizedUsers = request.AuthorizedUsers,
            CreatedAt = DateTime.UtcNow
        };

        // Generate the actual key
        var keyBytes = await GenerateKeyBytesAsync(key.Algorithm, key.KeySize);
        
        // Encrypt key with KEK (Key Encryption Key)
        var kek = await GetOrCreateKEKAsync();
        key.EncryptedKeyData = await EncryptKeyDataAsync(keyBytes, kek);
        key.KeyHash = ComputeKeyHash(keyBytes);
        key.Salt = Convert.ToBase64String(await GenerateSaltAsync());

        // Store key
        await _keyStore.CreateKeyAsync(key);

        // Audit log
        await _auditLogger.LogKeyActionAsync(key.Id, "CREATE", Guid.Empty, true);

        _logger.LogInformation("Created encryption key {KeyId} with name {KeyName}", key.Id, key.Name);
        return key;
    }

    public async Task<object> MaskDataAsync(object data, string? userRole = null, MaskingLevel? level = null)
    {
        try
        {
            if (data == null) return null;

            var jsonString = JsonSerializer.Serialize(data);
            var jsonDocument = JsonDocument.Parse(jsonString);
            var maskedJson = await MaskJsonElementAsync(jsonDocument.RootElement, userRole, level);
            
            return JsonSerializer.Deserialize<object>(maskedJson) ?? new object();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mask data for role {UserRole}", userRole);
            return data; // Return original data if masking fails
        }
    }

    public async Task<string> MaskStringAsync(string data, MaskingType type, string? pattern = null)
    {
        if (string.IsNullOrEmpty(data)) return data;

        return type switch
        {
            MaskingType.Full => new string('*', data.Length),
            MaskingType.Partial => MaskPartial(data),
            MaskingType.Email => MaskEmail(data),
            MaskingType.Phone => MaskPhone(data),
            MaskingType.CreditCard => MaskCreditCard(data),
            MaskingType.SSN => MaskSSN(data),
            MaskingType.Custom => MaskCustom(data, pattern ?? "*"),
            MaskingType.Redaction => "[REDACTED]",
            MaskingType.Hashing => await HashDataAsync(data),
            MaskingType.Tokenization => await TokenizeAsync(data),
            MaskingType.Shuffle => ShuffleString(data),
            _ => data
        };
    }

    public async Task<SecureDocument> UploadSecureDocumentAsync(Stream documentStream, string fileName, string contentType, DataClassification classification)
    {
        _logger.LogInformation("Uploading secure document {FileName} with classification {Classification}", fileName, classification);

        // Create document record
        var document = new SecureDocument
        {
            Name = Path.GetFileNameWithoutExtension(fileName),
            OriginalFileName = fileName,
            ContentType = contentType,
            Size = documentStream.Length,
            Classification = classification,
            CreatedAt = DateTime.UtcNow
        };

        // Generate encryption key for document
        var key = await GenerateKeyAsync(KeyType.DataEncryption, _config.ClassificationAlgorithms[classification], GetDefaultKeySize(_config.ClassificationAlgorithms[classification]));
        document.EncryptionKeyId = key.Id;
        document.Algorithm = key.Algorithm;

        // Generate IV
        var iv = GenerateIV(key.Algorithm);
        document.InitializationVector = Convert.ToBase64String(iv);

        // Encrypt and store document
        var encryptedStream = await EncryptStreamAsync(documentStream, key, iv);
        document.Hash = await ComputeStreamHashAsync(documentStream);
        
        // Store encrypted document
        var storagePath = await StoreEncryptedDocumentAsync(document.Id, encryptedStream);
        document.EncryptedStoragePath = storagePath;

        // Save document record
        await _documentStore.CreateDocumentAsync(document);

        _logger.LogInformation("Uploaded secure document {DocumentId} with name {FileName}", document.Id, fileName);
        return document;
    }

    // Helper methods
    private async Task<EncryptionKey> GetDefaultKeyForClassificationAsync(DataClassification classification)
    {
        var existingKey = await _keyStore.GetDefaultKeyForClassificationAsync(classification);
        if (existingKey != null && existingKey.Status == KeyStatus.Active)
        {
            return existingKey;
        }

        // Create new default key
        var algorithm = _config.ClassificationAlgorithms.GetValueOrDefault(classification, _config.DefaultAlgorithm);
        var keySize = GetDefaultKeySize(algorithm);
        
        return await GenerateKeyAsync(KeyType.DataEncryption, algorithm, keySize);
    }

    private async Task<byte[]> EncryptBytesAsync(byte[] data, EncryptionKey key, byte[] iv, byte[] salt)
    {
        var keyBytes = await GetKeyBytesAsync(key);
        
        return key.Algorithm switch
        {
            EncryptionAlgorithm.AES256 or EncryptionAlgorithm.AES192 or EncryptionAlgorithm.AES128 => 
                await EncryptAESAsync(data, keyBytes, iv),
            EncryptionAlgorithm.ChaCha20 => 
                await EncryptChaCha20Async(data, keyBytes, iv),
            _ => throw new NotSupportedException($"Algorithm {key.Algorithm} not supported")
        };
    }

    private async Task<byte[]> DecryptBytesAsync(byte[] encryptedData, EncryptionKey key, byte[] iv, byte[] salt)
    {
        var keyBytes = await GetKeyBytesAsync(key);
        
        return key.Algorithm switch
        {
            EncryptionAlgorithm.AES256 or EncryptionAlgorithm.AES192 or EncryptionAlgorithm.AES128 => 
                await DecryptAESAsync(encryptedData, keyBytes, iv),
            EncryptionAlgorithm.ChaCha20 => 
                await DecryptChaCha20Async(encryptedData, keyBytes, iv),
            _ => throw new NotSupportedException($"Algorithm {key.Algorithm} not supported")
        };
    }

    private async Task<byte[]> EncryptAESAsync(byte[] data, byte[] key, byte[] iv)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        return encryptor.TransformFinalBlock(data, 0, data.Length);
    }

    private async Task<byte[]> DecryptAESAsync(byte[] encryptedData, byte[] key, byte[] iv)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
    }

    private byte[] GenerateIV(EncryptionAlgorithm algorithm)
    {
        var size = algorithm switch
        {
            EncryptionAlgorithm.AES256 or EncryptionAlgorithm.AES192 or EncryptionAlgorithm.AES128 => 16,
            EncryptionAlgorithm.ChaCha20 => 12,
            _ => 16
        };
        
        return RandomNumberGenerator.GetBytes(size);
    }

    private int GetDefaultKeySize(EncryptionAlgorithm algorithm) => algorithm switch
    {
        EncryptionAlgorithm.AES256 => 32,
        EncryptionAlgorithm.AES192 => 24,
        EncryptionAlgorithm.AES128 => 16,
        EncryptionAlgorithm.ChaCha20 => 32,
        EncryptionAlgorithm.RSA2048 => 256,
        EncryptionAlgorithm.RSA4096 => 512,
        _ => 32
    };

    private string MaskPartial(string data)
    {
        if (data.Length <= 2) return new string('*', data.Length);
        if (data.Length <= 4) return data[0] + new string('*', data.Length - 2) + data[^1];
        
        var visibleChars = Math.Max(1, data.Length / 4);
        var prefix = data[..visibleChars];
        var suffix = data[^visibleChars..];
        var masked = new string('*', data.Length - (visibleChars * 2));
        
        return prefix + masked + suffix;
    }

    private string MaskEmail(string email)
    {
        if (!email.Contains('@')) return MaskPartial(email);
        
        var parts = email.Split('@');
        var username = MaskPartial(parts[0]);
        var domain = parts[1].Contains('.') ? 
            MaskPartial(parts[1].Split('.')[0]) + "." + parts[1].Split('.')[^1] : 
            MaskPartial(parts[1]);
        
        return username + "@" + domain;
    }

    private string MaskPhone(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.Length >= 10)
        {
            return phone.Replace(digits[3..^4], new string('*', digits.Length - 7));
        }
        return MaskPartial(phone);
    }

    private string MaskCreditCard(string cc)
    {
        var digits = new string(cc.Where(char.IsDigit).ToArray());
        if (digits.Length >= 13)
        {
            return cc.Replace(digits[4..^4], new string('*', digits.Length - 8));
        }
        return MaskPartial(cc);
    }

    private string MaskSSN(string ssn)
    {
        var digits = new string(ssn.Where(char.IsDigit).ToArray());
        if (digits.Length == 9)
        {
            return ssn.Replace(digits[..5], "*****");
        }
        return MaskPartial(ssn);
    }

    private string MaskCustom(string data, string pattern) => 
        pattern.Replace("*", new string('*', data.Length));

    private string ShuffleString(string data)
    {
        var array = data.ToCharArray();
        var random = new Random();
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
        return new string(array);
    }

    // Placeholder implementations for interface methods
    public Task<EncryptionResult> EncryptFieldAsync(string tableName, string fieldName, string data) => throw new NotImplementedException();
    public Task<DecryptionResult> DecryptFieldAsync(string tableName, string fieldName, string encryptedData) => throw new NotImplementedException();
    public Task<EncryptionKey?> GetKeyAsync(Guid keyId) => throw new NotImplementedException();
    public Task<List<KeyInfoResponse>> GetKeysAsync(KeyType? type = null, KeyStatus? status = null) => throw new NotImplementedException();
    public Task<bool> RotateKeyAsync(Guid keyId) => throw new NotImplementedException();
    public Task<bool> RevokeKeyAsync(Guid keyId, string reason) => throw new NotImplementedException();
    public Task<EncryptionKey> GenerateKeyAsync(KeyType type, EncryptionAlgorithm algorithm, int keySize) => throw new NotImplementedException();
    public Task<FieldEncryptionConfig> ConfigureFieldEncryptionAsync(string tableName, string fieldName, DataClassification classification, Guid keyId) => throw new NotImplementedException();
    public Task<List<FieldEncryptionConfig>> GetFieldEncryptionConfigsAsync(string? tableName = null) => throw new NotImplementedException();
    public Task<bool> EnableFieldEncryptionAsync(Guid configId) => throw new NotImplementedException();
    public Task<bool> DisableFieldEncryptionAsync(Guid configId) => throw new NotImplementedException();
    public Task<DataMaskingConfig> CreateMaskingConfigAsync(string entityType, string fieldPath, MaskingType maskingType) => throw new NotImplementedException();
    public Task<List<DataMaskingConfig>> GetMaskingConfigsAsync(string? entityType = null) => throw new NotImplementedException();
    public Task<Stream?> DownloadSecureDocumentAsync(Guid documentId, Guid userId) => throw new NotImplementedException();
    public Task<SecureDocument?> GetSecureDocumentAsync(Guid documentId) => throw new NotImplementedException();
    public Task<List<SecureDocumentResponse>> GetUserSecureDocumentsAsync(Guid userId) => throw new NotImplementedException();
    public Task<bool> DeleteSecureDocumentAsync(Guid documentId, Guid userId) => throw new NotImplementedException();
    public Task<byte[]> DeriveKeyAsync(string password, byte[] salt, KeyDerivationParameters parameters) => throw new NotImplementedException();
    public Task<byte[]> GenerateSaltAsync(int size = 32) => throw new NotImplementedException();
    public Task<string> GenerateSecureRandomStringAsync(int length = 32) => throw new NotImplementedException();
    public Task<string> HashDataAsync(string data, HashAlgorithmName? algorithm = null) => throw new NotImplementedException();
    public Task<bool> VerifyHashAsync(string data, string hash, HashAlgorithmName? algorithm = null) => throw new NotImplementedException();
    public Task<X509Certificate2> GenerateSelfSignedCertificateAsync(string subjectName, TimeSpan validity) => throw new NotImplementedException();
    public Task<bool> ValidateCertificateAsync(X509Certificate2 certificate) => throw new NotImplementedException();
    public Task<EncryptionStatusResponse> GetEncryptionStatusAsync() => throw new NotImplementedException();
    public Task<List<KeyManagementAuditLog>> GetKeyAuditLogsAsync(Guid? keyId = null, DateTime? fromDate = null, DateTime? toDate = null) => throw new NotImplementedException();
    public Task LogKeyUsageAsync(Guid keyId, string action, Guid userId, bool wasSuccessful = true, string? failureReason = null) => throw new NotImplementedException();
    public Task<EncryptionConfiguration> GetConfigurationAsync() => throw new NotImplementedException();
    public Task<bool> UpdateConfigurationAsync(EncryptionConfiguration config) => throw new NotImplementedException();
    public Task<bool> ValidateConfigurationAsync(EncryptionConfiguration config) => throw new NotImplementedException();
    public Task<byte[]> ExportKeyAsync(Guid keyId, string password) => throw new NotImplementedException();
    public Task<EncryptionKey> ImportKeyAsync(byte[] keyData, string password, CreateKeyRequest metadata) => throw new NotImplementedException();
    public Task<bool> BackupKeysAsync(string backupPath, string password) => throw new NotImplementedException();
    public Task<bool> RestoreKeysAsync(string backupPath, string password) => throw new NotImplementedException();

    // Private helper methods (placeholders)
    private Task<byte[]> GenerateKeyBytesAsync(EncryptionAlgorithm algorithm, int keySize) => throw new NotImplementedException();
    private Task<EncryptionKey> GetOrCreateKEKAsync() => throw new NotImplementedException();
    private Task<string> EncryptKeyDataAsync(byte[] keyBytes, EncryptionKey kek) => throw new NotImplementedException();
    private string ComputeKeyHash(byte[] keyBytes) => throw new NotImplementedException();
    private Task UpdateKeyUsageAsync(Guid keyId) => throw new NotImplementedException();
    private Task<byte[]> GetKeyBytesAsync(EncryptionKey key) => throw new NotImplementedException();
    private Task<byte[]> EncryptChaCha20Async(byte[] data, byte[] key, byte[] iv) => throw new NotImplementedException();
    private Task<byte[]> DecryptChaCha20Async(byte[] data, byte[] key, byte[] iv) => throw new NotImplementedException();
    private Task<string> MaskJsonElementAsync(JsonElement element, string? userRole, MaskingLevel? level) => throw new NotImplementedException();
    private Task<string> TokenizeAsync(string data) => throw new NotImplementedException();
    private Task<Stream> EncryptStreamAsync(Stream stream, EncryptionKey key, byte[] iv) => throw new NotImplementedException();
    private Task<string> ComputeStreamHashAsync(Stream stream) => throw new NotImplementedException();
    private Task<string> StoreEncryptedDocumentAsync(Guid documentId, Stream encryptedStream) => throw new NotImplementedException();
}

/// <summary>
/// Supporting service interfaces (to be implemented)
/// </summary>
public interface IEncryptionKeyStore
{
    Task<EncryptionKey> CreateKeyAsync(EncryptionKey key);
    Task<EncryptionKey?> GetKeyAsync(Guid keyId);
    Task<List<EncryptionKey>> GetKeysAsync(KeyType? type, KeyStatus? status);
    Task<EncryptionKey?> GetDefaultKeyForClassificationAsync(DataClassification classification);
    Task<EncryptionKey?> GetActiveKeyForRotatedKeyAsync(Guid rotatedKeyId);
    Task<bool> UpdateKeyAsync(EncryptionKey key);
}

public interface IFieldEncryptionStore
{
    Task<FieldEncryptionConfig> CreateConfigAsync(FieldEncryptionConfig config);
    Task<List<FieldEncryptionConfig>> GetConfigsAsync(string? tableName);
    Task<FieldEncryptionConfig?> GetConfigAsync(string tableName, string fieldName);
}

public interface IDataMaskingStore
{
    Task<DataMaskingConfig> CreateConfigAsync(DataMaskingConfig config);
    Task<List<DataMaskingConfig>> GetConfigsAsync(string? entityType);
    Task<DataMaskingConfig?> GetConfigAsync(string entityType, string fieldPath);
}

public interface ISecureDocumentStore
{
    Task<SecureDocument> CreateDocumentAsync(SecureDocument document);
    Task<SecureDocument?> GetDocumentAsync(Guid documentId);
    Task<List<SecureDocument>> GetUserDocumentsAsync(Guid userId);
    Task<bool> UpdateDocumentAsync(SecureDocument document);
    Task<bool> DeleteDocumentAsync(Guid documentId);
}

public interface IEncryptionAuditLogger
{
    Task LogKeyActionAsync(Guid keyId, string action, Guid userId, bool wasSuccessful, string? failureReason = null);
    Task<List<KeyManagementAuditLog>> GetAuditLogsAsync(Guid? keyId, DateTime? fromDate, DateTime? toDate);
}