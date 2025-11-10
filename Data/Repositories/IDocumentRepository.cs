using ADPA.Models.Entities;
using System.Collections.Concurrent;

namespace ADPA.Data.Repositories;

/// <summary>
/// Interface for Document repository
/// </summary>
public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(Guid id);
    Task<IEnumerable<Document>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Document>> GetAllAsync();
    Task<IEnumerable<Document>> GetByStatusAsync(ProcessingStatus status);
    Task<Document> CreateAsync(Document document);
    Task<Document> UpdateAsync(Document document);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<Document?> GetByHashAsync(string fileHash);
}

/// <summary>
/// In-memory implementation of Document repository
/// </summary>
public class InMemoryDocumentRepository : IDocumentRepository
{
    private readonly ConcurrentDictionary<Guid, Document> _documents;

    public InMemoryDocumentRepository()
    {
        _documents = new ConcurrentDictionary<Guid, Document>();
    }

    public async Task<Document?> GetByIdAsync(Guid id)
    {
        await Task.CompletedTask;
        return _documents.TryGetValue(id, out var document) ? document : null;
    }

    public async Task<IEnumerable<Document>> GetByUserIdAsync(Guid userId)
    {
        await Task.CompletedTask;
        return _documents.Values.Where(d => d.UserId == userId).ToList();
    }

    public async Task<IEnumerable<Document>> GetAllAsync()
    {
        await Task.CompletedTask;
        return _documents.Values.OrderByDescending(d => d.CreatedAt).ToList();
    }

    public async Task<IEnumerable<Document>> GetByStatusAsync(ProcessingStatus status)
    {
        await Task.CompletedTask;
        return _documents.Values.Where(d => d.Status == status).ToList();
    }

    public async Task<Document> CreateAsync(Document document)
    {
        await Task.CompletedTask;
        document.Id = Guid.NewGuid();
        document.CreatedAt = DateTime.UtcNow;
        _documents.TryAdd(document.Id, document);
        return document;
    }

    public async Task<Document> UpdateAsync(Document document)
    {
        await Task.CompletedTask;
        _documents.TryUpdate(document.Id, document, _documents[document.Id]);
        return document;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await Task.CompletedTask;
        return _documents.TryRemove(id, out _);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        await Task.CompletedTask;
        return _documents.ContainsKey(id);
    }

    public async Task<Document?> GetByHashAsync(string fileHash)
    {
        await Task.CompletedTask;
        return _documents.Values.FirstOrDefault(d => d.FileHash == fileHash);
    }
}