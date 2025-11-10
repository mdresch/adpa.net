using ADPA.Models.Entities;
using ADPA.Data;
using Microsoft.EntityFrameworkCore;

namespace ADPA.Data.Repositories;

/// <summary>
/// EF Core User Repository implementation
/// </summary>
public class EfUserRepository : IUserRepository
{
    private readonly AdpaEfDbContext _context;
    private readonly ILogger<EfUserRepository> _logger;

    public EfUserRepository(AdpaEfDbContext context, ILogger<EfUserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    public async Task<User?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _context.Users.FindAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID: {UserId}", id);
            throw;
        }
    }

    /// <summary>
    /// Get user by email
    /// </summary>
    public async Task<User?> GetByEmailAsync(string email)
    {
        try
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLowerInvariant() == email.ToLowerInvariant());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email: {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Get all users
    /// </summary>
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        try
        {
            return await _context.Users.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            throw;
        }
    }

    /// <summary>
    /// Create new user
    /// </summary>
    public async Task<User> CreateAsync(User user)
    {
        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("User created: {UserId} - {Email}", user.Id, user.Email);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {Email}", user.Email);
            throw;
        }
    }

    /// <summary>
    /// Update existing user
    /// </summary>
    public async Task<User> UpdateAsync(User user)
    {
        try
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("User updated: {UserId} - {Email}", user.Id, user.Email);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", user.Id);
            throw;
        }
    }

    /// <summary>
    /// Delete user
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var user = await GetByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("User deleted: {UserId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", id);
            throw;
        }
    }

    /// <summary>
    /// Get users by role
    /// </summary>
    public async Task<IEnumerable<User>> GetByRoleAsync(string role)
    {
        try
        {
            return await _context.Users
                .Where(u => u.Role == role)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users by role: {Role}", role);
            throw;
        }
    }

    /// <summary>
    /// Get active users
    /// </summary>
    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        try
        {
            return await _context.Users
                .Where(u => u.IsActive)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active users");
            throw;
        }
    }

    /// <summary>
    /// Check if user exists
    /// </summary>
    public async Task<bool> ExistsAsync(Guid id)
    {
        try
        {
            return await _context.Users.AnyAsync(u => u.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user exists: {UserId}", id);
            throw;
        }
    }
}

/// <summary>
/// EF Core Document Repository implementation
/// </summary>
public class EfDocumentRepository : IDocumentRepository
{
    private readonly AdpaEfDbContext _context;
    private readonly ILogger<EfDocumentRepository> _logger;

    public EfDocumentRepository(AdpaEfDbContext context, ILogger<EfDocumentRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get document by ID
    /// </summary>
    public async Task<Document?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _context.Documents.FindAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting document by ID: {DocumentId}", id);
            throw;
        }
    }

    /// <summary>
    /// Get all documents
    /// </summary>
    public async Task<IEnumerable<Document>> GetAllAsync()
    {
        try
        {
            return await _context.Documents
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all documents");
            throw;
        }
    }

    /// <summary>
    /// Get documents by user ID
    /// </summary>
    public async Task<IEnumerable<Document>> GetByUserIdAsync(Guid userId)
    {
        try
        {
            return await _context.Documents
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting documents by user ID: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get document by file hash
    /// </summary>
    public async Task<Document?> GetByHashAsync(string fileHash)
    {
        try
        {
            return await _context.Documents
                .FirstOrDefaultAsync(d => d.FileHash == fileHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting document by hash: {FileHash}", fileHash);
            throw;
        }
    }

    /// <summary>
    /// Create new document
    /// </summary>
    public async Task<Document> CreateAsync(Document document)
    {
        try
        {
            _context.Documents.Add(document);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Document created: {DocumentId} - {FileName}", document.Id, document.FileName);
            return document;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating document: {FileName}", document.FileName);
            throw;
        }
    }

    /// <summary>
    /// Update existing document
    /// </summary>
    public async Task<Document> UpdateAsync(Document document)
    {
        try
        {
            _context.Documents.Update(document);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Document updated: {DocumentId}", document.Id);
            return document;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document: {DocumentId}", document.Id);
            throw;
        }
    }

    /// <summary>
    /// Delete document
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var document = await GetByIdAsync(id);
            if (document == null)
            {
                return false;
            }

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Document deleted: {DocumentId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document: {DocumentId}", id);
            throw;
        }
    }

    /// <summary>
    /// Get documents by status
    /// </summary>
    public async Task<IEnumerable<Document>> GetByStatusAsync(ProcessingStatus status)
    {
        try
        {
            return await _context.Documents
                .Where(d => d.Status == status)
                .OrderBy(d => d.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting documents by status: {Status}", status);
            throw;
        }
    }

    /// <summary>
    /// Get pending documents for processing
    /// </summary>
    public async Task<IEnumerable<Document>> GetPendingDocumentsAsync()
    {
        return await GetByStatusAsync(ProcessingStatus.Pending);
    }

    /// <summary>
    /// Check if document exists
    /// </summary>
    public async Task<bool> ExistsAsync(Guid id)
    {
        try
        {
            return await _context.Documents.AnyAsync(d => d.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if document exists: {DocumentId}", id);
            throw;
        }
    }
}

/// <summary>
/// EF Core Processing Result Repository implementation  
/// </summary>
public class EfProcessingResultRepository
{
    private readonly AdpaEfDbContext _context;
    private readonly ILogger<EfProcessingResultRepository> _logger;

    public EfProcessingResultRepository(AdpaEfDbContext context, ILogger<EfProcessingResultRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get processing result by ID
    /// </summary>
    public async Task<ProcessingResult?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _context.ProcessingResults.FindAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting processing result by ID: {ResultId}", id);
            throw;
        }
    }

    /// <summary>
    /// Get processing results by document ID
    /// </summary>
    public async Task<IEnumerable<ProcessingResult>> GetByDocumentIdAsync(Guid documentId)
    {
        try
        {
            return await _context.ProcessingResults
                .Where(pr => pr.DocumentId == documentId)
                .OrderByDescending(pr => pr.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting processing results by document ID: {DocumentId}", documentId);
            throw;
        }
    }

    /// <summary>
    /// Create new processing result
    /// </summary>
    public async Task<ProcessingResult> CreateAsync(ProcessingResult result)
    {
        try
        {
            _context.ProcessingResults.Add(result);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Processing result created: {ResultId} for document {DocumentId}", result.Id, result.DocumentId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating processing result for document: {DocumentId}", result.DocumentId);
            throw;
        }
    }

    /// <summary>
    /// Get all processing results
    /// </summary>
    public async Task<IEnumerable<ProcessingResult>> GetAllAsync()
    {
        try
        {
            return await _context.ProcessingResults
                .OrderByDescending(pr => pr.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all processing results");
            throw;
        }
    }
}