using Microsoft.EntityFrameworkCore;
using ADPA.Data;

namespace ADPA.Services.Security;

/// <summary>
/// Phase 5.3: Data Encryption & Protection - Store Implementations
/// Provides data persistence for encryption system components
/// </summary>

/// <summary>
/// Audit logger implementation for encryption operations
/// </summary>
public class EncryptionAuditLogger : IEncryptionAuditLogger
{
    private readonly ILogger<EncryptionAuditLogger> _logger;
    private readonly AdpaEfDbContext _context;

    public EncryptionAuditLogger(ILogger<EncryptionAuditLogger> logger, AdpaEfDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public Task LogKeyActionAsync(Guid keyId, string action, Guid userId, bool wasSuccessful, string? failureReason = null)
    {
        _logger.LogInformation("Key action: {Action} on key {KeyId} by user {UserId}. Success: {Success}. Reason: {Reason}",
            action, keyId, userId, wasSuccessful, failureReason);
        
        // In a full implementation, this would write to audit tables
        return Task.CompletedTask;
    }

    public Task<List<KeyManagementAuditLog>> GetAuditLogsAsync(Guid? keyId, DateTime? fromDate, DateTime? toDate)
    {
        // In a full implementation, this would query audit tables
        return Task.FromResult(new List<KeyManagementAuditLog>());
    }
}

/// <summary>
/// Store implementation for encryption keys with EF Core
/// </summary>
public class EncryptionKeyStore : IEncryptionKeyStore
{
    private readonly AdpaEfDbContext _context;
    private readonly ILogger<EncryptionKeyStore> _logger;

    public EncryptionKeyStore(AdpaEfDbContext context, ILogger<EncryptionKeyStore> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<EncryptionKey> CreateKeyAsync(EncryptionKey key)
    {
        _context.Set<EncryptionKey>().Add(key);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Created encryption key {KeyId} with name {KeyName}", key.Id, key.Name);
        return key;
    }

    public async Task<EncryptionKey?> GetKeyAsync(Guid keyId)
    {
        return await _context.Set<EncryptionKey>()
            .FirstOrDefaultAsync(k => k.Id == keyId);
    }

    public async Task<List<EncryptionKey>> GetKeysAsync(KeyType? type, KeyStatus? status)
    {
        var query = _context.Set<EncryptionKey>().AsQueryable();
        
        if (type.HasValue)
        {
            query = query.Where(k => k.Type == type.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(k => k.Status == status.Value);
        }

        return await query.OrderBy(k => k.Name).ToListAsync();
    }

    public async Task<EncryptionKey?> GetDefaultKeyForClassificationAsync(DataClassification classification)
    {
        // Return the most recently created active key for this classification
        return await _context.Set<EncryptionKey>()
            .Where(k => k.Status == KeyStatus.Active && k.Type == KeyType.DataEncryption)
            .OrderByDescending(k => k.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<EncryptionKey?> GetActiveKeyForRotatedKeyAsync(Guid rotatedKeyId)
    {
        var rotatedKey = await GetKeyAsync(rotatedKeyId);
        if (rotatedKey == null) return null;

        // Find the key that replaced this one
        return await _context.Set<EncryptionKey>()
            .Where(k => k.ParentKeyId == rotatedKeyId && k.Status == KeyStatus.Active)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateKeyAsync(EncryptionKey key)
    {
        try
        {
            _context.Set<EncryptionKey>().Update(key);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated encryption key {KeyId}", key.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update encryption key {KeyId}", key.Id);
            return false;
        }
    }
}

/// <summary>
/// Store implementation for field encryption configurations
/// </summary>
public class FieldEncryptionStore : IFieldEncryptionStore
{
    private readonly AdpaEfDbContext _context;
    private readonly ILogger<FieldEncryptionStore> _logger;

    public FieldEncryptionStore(AdpaEfDbContext context, ILogger<FieldEncryptionStore> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<FieldEncryptionConfig> CreateConfigAsync(FieldEncryptionConfig config)
    {
        _context.Set<FieldEncryptionConfig>().Add(config);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Created field encryption config for {Table}.{Field}", config.TableName, config.FieldName);
        return config;
    }

    public async Task<List<FieldEncryptionConfig>> GetConfigsAsync(string? tableName)
    {
        var query = _context.Set<FieldEncryptionConfig>().AsQueryable();
        
        if (!string.IsNullOrEmpty(tableName))
        {
            query = query.Where(c => c.TableName == tableName);
        }

        return await query.OrderBy(c => c.TableName).ThenBy(c => c.FieldName).ToListAsync();
    }

    public async Task<FieldEncryptionConfig?> GetConfigAsync(string tableName, string fieldName)
    {
        return await _context.Set<FieldEncryptionConfig>()
            .FirstOrDefaultAsync(c => c.TableName == tableName && c.FieldName == fieldName);
    }
}

/// <summary>
/// Store implementation for data masking configurations
/// </summary>
public class DataMaskingStore : IDataMaskingStore
{
    private readonly AdpaEfDbContext _context;
    private readonly ILogger<DataMaskingStore> _logger;

    public DataMaskingStore(AdpaEfDbContext context, ILogger<DataMaskingStore> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DataMaskingConfig> CreateConfigAsync(DataMaskingConfig config)
    {
        _context.Set<DataMaskingConfig>().Add(config);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Created data masking config for {EntityType}.{FieldPath}", config.EntityType, config.FieldPath);
        return config;
    }

    public async Task<List<DataMaskingConfig>> GetConfigsAsync(string? entityType)
    {
        var query = _context.Set<DataMaskingConfig>().AsQueryable();
        
        if (!string.IsNullOrEmpty(entityType))
        {
            query = query.Where(c => c.EntityType == entityType);
        }

        return await query.OrderBy(c => c.EntityType).ThenBy(c => c.FieldPath).ToListAsync();
    }

    public async Task<DataMaskingConfig?> GetConfigAsync(string entityType, string fieldPath)
    {
        return await _context.Set<DataMaskingConfig>()
            .FirstOrDefaultAsync(c => c.EntityType == entityType && c.FieldPath == fieldPath);
    }
}

/// <summary>
/// Store implementation for secure documents
/// </summary>
public class SecureDocumentStore : ISecureDocumentStore
{
    private readonly AdpaEfDbContext _context;
    private readonly ILogger<SecureDocumentStore> _logger;

    public SecureDocumentStore(AdpaEfDbContext context, ILogger<SecureDocumentStore> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SecureDocument> CreateDocumentAsync(SecureDocument document)
    {
        _context.Set<SecureDocument>().Add(document);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Created secure document {DocumentId} with name {FileName}", document.Id, document.OriginalFileName);
        return document;
    }

    public async Task<SecureDocument?> GetDocumentAsync(Guid documentId)
    {
        return await _context.Set<SecureDocument>()
            .FirstOrDefaultAsync(d => d.Id == documentId);
    }

    public async Task<List<SecureDocument>> GetUserDocumentsAsync(Guid userId)
    {
        return await _context.Set<SecureDocument>()
            .Where(d => d.OwnerId == userId || d.Permissions.Any(p => p.UserId == userId))
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> UpdateDocumentAsync(SecureDocument document)
    {
        try
        {
            _context.Set<SecureDocument>().Update(document);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated secure document {DocumentId}", document.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update secure document {DocumentId}", document.Id);
            return false;
        }
    }

    public async Task<bool> DeleteDocumentAsync(Guid documentId)
    {
        try
        {
            var document = await GetDocumentAsync(documentId);
            if (document != null)
            {
                _context.Set<SecureDocument>().Remove(document);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted secure document {DocumentId}", documentId);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete secure document {DocumentId}", documentId);
            return false;
        }
    }
}