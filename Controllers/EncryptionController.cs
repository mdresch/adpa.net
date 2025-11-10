using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ADPA.Services.Security;
using System.Security.Cryptography;

namespace ADPA.Controllers;

/// <summary>
/// Phase 5: Data Encryption & Protection Controller
/// Comprehensive encryption API with KMS, field-level encryption, data masking, and secure document storage
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class EncryptionController : ControllerBase
{
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<EncryptionController> _logger;

    public EncryptionController(
        IEncryptionService encryptionService,
        ILogger<EncryptionController> logger)
    {
        _encryptionService = encryptionService;
        _logger = logger;
    }

    /// <summary>
    /// Encrypt data with specified classification level
    /// </summary>
    [HttpPost("encrypt")]
    [Authorize(Policy = "CanEncryptData")]
    public async Task<ActionResult<EncryptionResult>> EncryptData([FromBody] EncryptDataRequest request)
    {
        try
        {
            var result = await _encryptionService.EncryptAsync(
                request.Data, 
                request.Classification ?? DataClassification.Internal, 
                request.KeyId);

            if (!result.IsSuccessful)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to encrypt data");
            return StatusCode(500, new { message = "Encryption failed" });
        }
    }

    /// <summary>
    /// Decrypt data using specified key
    /// </summary>
    [HttpPost("decrypt")]
    [Authorize(Policy = "CanDecryptData")]
    public async Task<ActionResult<DecryptionResult>> DecryptData([FromBody] DecryptDataRequest request)
    {
        try
        {
            var result = await _encryptionService.DecryptAsync(
                request.EncryptedData,
                request.KeyId,
                request.InitializationVector,
                request.Salt);

            if (!result.IsSuccessful)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt data");
            return StatusCode(500, new { message = "Decryption failed" });
        }
    }

    /// <summary>
    /// Create new encryption key
    /// </summary>
    [HttpPost("keys")]
    [Authorize(Policy = "CanManageEncryptionKeys")]
    public async Task<ActionResult<KeyInfoResponse>> CreateKey([FromBody] CreateKeyRequest request)
    {
        try
        {
            var key = await _encryptionService.CreateKeyAsync(request);
            
            var response = new KeyInfoResponse
            {
                Id = key.Id,
                Name = key.Name,
                Description = key.Description,
                Type = key.Type,
                Algorithm = key.Algorithm,
                KeySize = key.KeySize,
                Status = key.Status,
                CreatedAt = key.CreatedAt,
                ExpiresAt = key.ExpiresAt,
                LastUsedAt = key.LastUsedAt,
                UsageCount = key.UsageCount,
                AuthorizedRoles = key.AuthorizedRoles
            };

            return CreatedAtAction(nameof(GetKey), new { keyId = key.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create encryption key {KeyName}", request.Name);
            return StatusCode(500, new { message = "Key creation failed" });
        }
    }

    /// <summary>
    /// Get encryption key information
    /// </summary>
    [HttpGet("keys/{keyId}")]
    [Authorize(Policy = "CanViewEncryptionKeys")]
    public async Task<ActionResult<KeyInfoResponse>> GetKey(Guid keyId)
    {
        try
        {
            var key = await _encryptionService.GetKeyAsync(keyId);
            if (key == null)
            {
                return NotFound(new { message = "Encryption key not found" });
            }

            var response = new KeyInfoResponse
            {
                Id = key.Id,
                Name = key.Name,
                Description = key.Description,
                Type = key.Type,
                Algorithm = key.Algorithm,
                KeySize = key.KeySize,
                Status = key.Status,
                CreatedAt = key.CreatedAt,
                ExpiresAt = key.ExpiresAt,
                LastUsedAt = key.LastUsedAt,
                UsageCount = key.UsageCount,
                AuthorizedRoles = key.AuthorizedRoles
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get encryption key {KeyId}", keyId);
            return StatusCode(500, new { message = "Failed to get encryption key" });
        }
    }

    /// <summary>
    /// Get all encryption keys
    /// </summary>
    [HttpGet("keys")]
    [Authorize(Policy = "CanViewEncryptionKeys")]
    public async Task<ActionResult<List<KeyInfoResponse>>> GetKeys(
        [FromQuery] KeyType? type = null,
        [FromQuery] KeyStatus? status = null)
    {
        try
        {
            var keys = await _encryptionService.GetKeysAsync(type, status);
            return Ok(keys);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get encryption keys");
            return StatusCode(500, new { message = "Failed to get encryption keys" });
        }
    }

    /// <summary>
    /// Rotate encryption key
    /// </summary>
    [HttpPost("keys/{keyId}/rotate")]
    [Authorize(Policy = "CanRotateEncryptionKeys")]
    public async Task<ActionResult> RotateKey(Guid keyId)
    {
        try
        {
            var success = await _encryptionService.RotateKeyAsync(keyId);
            if (!success)
            {
                return BadRequest(new { message = "Key rotation failed" });
            }

            return Ok(new { message = "Key rotated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rotate encryption key {KeyId}", keyId);
            return StatusCode(500, new { message = "Key rotation failed" });
        }
    }

    /// <summary>
    /// Revoke encryption key
    /// </summary>
    [HttpPost("keys/{keyId}/revoke")]
    [Authorize(Policy = "CanRevokeEncryptionKeys")]
    public async Task<ActionResult> RevokeKey(Guid keyId, [FromBody] RevokeKeyRequest request)
    {
        try
        {
            var success = await _encryptionService.RevokeKeyAsync(keyId, request.Reason);
            if (!success)
            {
                return BadRequest(new { message = "Key revocation failed" });
            }

            return Ok(new { message = "Key revoked successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke encryption key {KeyId}", keyId);
            return StatusCode(500, new { message = "Key revocation failed" });
        }
    }

    /// <summary>
    /// Configure field-level encryption
    /// </summary>
    [HttpPost("field-encryption")]
    [Authorize(Policy = "CanConfigureFieldEncryption")]
    public async Task<ActionResult<FieldEncryptionConfig>> ConfigureFieldEncryption([FromBody] ConfigureFieldEncryptionRequest request)
    {
        try
        {
            var config = await _encryptionService.ConfigureFieldEncryptionAsync(
                request.TableName,
                request.FieldName,
                request.Classification,
                request.KeyId);

            return CreatedAtAction(nameof(GetFieldEncryptionConfigs), new { tableName = request.TableName }, config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to configure field encryption for {TableName}.{FieldName}", 
                request.TableName, request.FieldName);
            return StatusCode(500, new { message = "Field encryption configuration failed" });
        }
    }

    /// <summary>
    /// Get field encryption configurations
    /// </summary>
    [HttpGet("field-encryption")]
    [Authorize(Policy = "CanViewFieldEncryption")]
    public async Task<ActionResult<List<FieldEncryptionConfig>>> GetFieldEncryptionConfigs([FromQuery] string? tableName = null)
    {
        try
        {
            var configs = await _encryptionService.GetFieldEncryptionConfigsAsync(tableName);
            return Ok(configs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get field encryption configurations");
            return StatusCode(500, new { message = "Failed to get field encryption configurations" });
        }
    }

    /// <summary>
    /// Mask sensitive data
    /// </summary>
    [HttpPost("mask")]
    [Authorize(Policy = "CanMaskData")]
    public async Task<ActionResult<object>> MaskData([FromBody] MaskDataRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = request.UserRole ?? User.FindFirst(ClaimTypes.Role)?.Value;

            var maskedData = await _encryptionService.MaskDataAsync(
                request.Data,
                userRole,
                request.MaskingLevel);

            return Ok(maskedData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mask data");
            return StatusCode(500, new { message = "Data masking failed" });
        }
    }

    /// <summary>
    /// Create data masking configuration
    /// </summary>
    [HttpPost("masking-config")]
    [Authorize(Policy = "CanConfigureDataMasking")]
    public async Task<ActionResult<DataMaskingConfig>> CreateMaskingConfig([FromBody] CreateMaskingConfigRequest request)
    {
        try
        {
            var config = await _encryptionService.CreateMaskingConfigAsync(
                request.EntityType,
                request.FieldPath,
                request.MaskingType);

            return CreatedAtAction(nameof(GetMaskingConfigs), new { entityType = request.EntityType }, config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create masking configuration for {EntityType}.{FieldPath}",
                request.EntityType, request.FieldPath);
            return StatusCode(500, new { message = "Masking configuration creation failed" });
        }
    }

    /// <summary>
    /// Get data masking configurations
    /// </summary>
    [HttpGet("masking-config")]
    [Authorize(Policy = "CanViewDataMasking")]
    public async Task<ActionResult<List<DataMaskingConfig>>> GetMaskingConfigs([FromQuery] string? entityType = null)
    {
        try
        {
            var configs = await _encryptionService.GetMaskingConfigsAsync(entityType);
            return Ok(configs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get masking configurations");
            return StatusCode(500, new { message = "Failed to get masking configurations" });
        }
    }

    /// <summary>
    /// Upload secure document
    /// </summary>
    [HttpPost("documents")]
    [Authorize(Policy = "CanUploadSecureDocuments")]
    public async Task<ActionResult<SecureDocumentResponse>> UploadDocument([FromForm] UploadSecureDocumentRequest request)
    {
        try
        {
            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest(new { message = "No file provided" });
            }

            using var stream = request.File.OpenReadStream();
            var document = await _encryptionService.UploadSecureDocumentAsync(
                stream,
                request.File.FileName,
                request.File.ContentType,
                request.Classification);

            var response = new SecureDocumentResponse
            {
                Id = document.Id,
                Name = document.Name,
                OriginalFileName = document.OriginalFileName,
                ContentType = document.ContentType,
                Size = document.Size,
                Classification = document.Classification,
                Version = document.Version,
                CreatedAt = document.CreatedAt,
                UpdatedAt = document.UpdatedAt,
                ExpiresAt = document.ExpiresAt,
                Tags = document.Tags,
                UserAccessLevel = DocumentAccessLevel.Admin // Set based on user permissions
            };

            return CreatedAtAction(nameof(GetDocument), new { documentId = document.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload secure document {FileName}", request.File?.FileName);
            return StatusCode(500, new { message = "Document upload failed" });
        }
    }

    /// <summary>
    /// Download secure document
    /// </summary>
    [HttpGet("documents/{documentId}/download")]
    [Authorize(Policy = "CanDownloadSecureDocuments")]
    public async Task<ActionResult> DownloadDocument(Guid documentId)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user" });
            }

            var document = await _encryptionService.GetSecureDocumentAsync(documentId);
            if (document == null)
            {
                return NotFound(new { message = "Document not found" });
            }

            var stream = await _encryptionService.DownloadSecureDocumentAsync(documentId, userId);
            if (stream == null)
            {
                return Forbid();
            }

            return File(stream, document.ContentType, document.OriginalFileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download secure document {DocumentId}", documentId);
            return StatusCode(500, new { message = "Document download failed" });
        }
    }

    /// <summary>
    /// Get secure document information
    /// </summary>
    [HttpGet("documents/{documentId}")]
    [Authorize(Policy = "CanViewSecureDocuments")]
    public async Task<ActionResult<SecureDocumentResponse>> GetDocument(Guid documentId)
    {
        try
        {
            var document = await _encryptionService.GetSecureDocumentAsync(documentId);
            if (document == null)
            {
                return NotFound(new { message = "Document not found" });
            }

            var response = new SecureDocumentResponse
            {
                Id = document.Id,
                Name = document.Name,
                OriginalFileName = document.OriginalFileName,
                ContentType = document.ContentType,
                Size = document.Size,
                Classification = document.Classification,
                Version = document.Version,
                CreatedAt = document.CreatedAt,
                UpdatedAt = document.UpdatedAt,
                ExpiresAt = document.ExpiresAt,
                Tags = document.Tags,
                UserAccessLevel = DocumentAccessLevel.Read // Set based on user permissions
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get secure document {DocumentId}", documentId);
            return StatusCode(500, new { message = "Failed to get document" });
        }
    }

    /// <summary>
    /// Get user's secure documents
    /// </summary>
    [HttpGet("documents")]
    [Authorize]
    public async Task<ActionResult<List<SecureDocumentResponse>>> GetUserDocuments()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user" });
            }

            var documents = await _encryptionService.GetUserSecureDocumentsAsync(userId);
            return Ok(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user documents");
            return StatusCode(500, new { message = "Failed to get documents" });
        }
    }

    /// <summary>
    /// Delete secure document
    /// </summary>
    [HttpDelete("documents/{documentId}")]
    [Authorize(Policy = "CanDeleteSecureDocuments")]
    public async Task<ActionResult> DeleteDocument(Guid documentId)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user" });
            }

            var success = await _encryptionService.DeleteSecureDocumentAsync(documentId, userId);
            if (!success)
            {
                return BadRequest(new { message = "Document deletion failed" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete secure document {DocumentId}", documentId);
            return StatusCode(500, new { message = "Document deletion failed" });
        }
    }

    /// <summary>
    /// Generate secure random string
    /// </summary>
    [HttpPost("random-string")]
    [Authorize(Policy = "CanGenerateRandomData")]
    public async Task<ActionResult<string>> GenerateRandomString([FromBody] GenerateRandomStringRequest request)
    {
        try
        {
            var randomString = await _encryptionService.GenerateSecureRandomStringAsync(request.Length);
            return Ok(new { randomString });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate random string");
            return StatusCode(500, new { message = "Random string generation failed" });
        }
    }

    /// <summary>
    /// Hash data
    /// </summary>
    [HttpPost("hash")]
    [Authorize(Policy = "CanHashData")]
    public async Task<ActionResult<string>> HashData([FromBody] HashDataRequest request)
    {
        try
        {
            var hash = await _encryptionService.HashDataAsync(request.Data, request.Algorithm);
            return Ok(new { hash });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to hash data");
            return StatusCode(500, new { message = "Data hashing failed" });
        }
    }

    /// <summary>
    /// Get encryption status and metrics
    /// </summary>
    [HttpGet("status")]
    [Authorize(Policy = "CanViewEncryptionStatus")]
    public async Task<ActionResult<EncryptionStatusResponse>> GetEncryptionStatus()
    {
        try
        {
            var status = await _encryptionService.GetEncryptionStatusAsync();
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get encryption status");
            return StatusCode(500, new { message = "Failed to get encryption status" });
        }
    }

    /// <summary>
    /// Get key management audit logs
    /// </summary>
    [HttpGet("audit-logs")]
    [Authorize(Policy = "CanViewEncryptionAuditLogs")]
    public async Task<ActionResult<List<KeyManagementAuditLog>>> GetAuditLogs(
        [FromQuery] Guid? keyId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var logs = await _encryptionService.GetKeyAuditLogsAsync(keyId, fromDate, toDate);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get encryption audit logs");
            return StatusCode(500, new { message = "Failed to get audit logs" });
        }
    }

    /// <summary>
    /// Get encryption configuration
    /// </summary>
    [HttpGet("configuration")]
    [Authorize(Policy = "CanViewEncryptionConfiguration")]
    public async Task<ActionResult<EncryptionConfiguration>> GetConfiguration()
    {
        try
        {
            var config = await _encryptionService.GetConfigurationAsync();
            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get encryption configuration");
            return StatusCode(500, new { message = "Failed to get configuration" });
        }
    }

    /// <summary>
    /// Update encryption configuration
    /// </summary>
    [HttpPut("configuration")]
    [Authorize(Policy = "CanUpdateEncryptionConfiguration")]
    public async Task<ActionResult> UpdateConfiguration([FromBody] EncryptionConfiguration config)
    {
        try
        {
            var success = await _encryptionService.UpdateConfigurationAsync(config);
            if (!success)
            {
                return BadRequest(new { message = "Configuration update failed" });
            }

            return Ok(new { message = "Configuration updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update encryption configuration");
            return StatusCode(500, new { message = "Configuration update failed" });
        }
    }
}

/// <summary>
/// Supporting request models
/// </summary>
public class RevokeKeyRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class ConfigureFieldEncryptionRequest
{
    public string TableName { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public DataClassification Classification { get; set; }
    public Guid KeyId { get; set; }
}

public class CreateMaskingConfigRequest
{
    public string EntityType { get; set; } = string.Empty;
    public string FieldPath { get; set; } = string.Empty;
    public MaskingType MaskingType { get; set; }
}

public class GenerateRandomStringRequest
{
    public int Length { get; set; } = 32;
}

public class HashDataRequest
{
    public string Data { get; set; } = string.Empty;
    public HashAlgorithmName? Algorithm { get; set; }
}