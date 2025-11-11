using Microsoft.AspNetCore.Components.Forms;
using ADPA.Shared.DTOs;

namespace ADPA.Web.Services;

public interface IDocumentService
{
    Task<DocumentDto> UploadDocumentAsync(IBrowserFile file, Action<int>? progressCallback = null);
    Task<List<DocumentDto>> GetDocumentsAsync();
    Task<DocumentDto> GetDocumentAsync(Guid id);
    Task DeleteDocumentAsync(Guid id);
}

public class DocumentService : IDocumentService
{
    private readonly ApiService _apiService;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(ApiService apiService, ILogger<DocumentService> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<DocumentDto> UploadDocumentAsync(IBrowserFile file, Action<int>? progressCallback = null)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            
            var fileContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 100_000_000));
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            
            content.Add(fileContent, "file", file.Name);

            var response = await _apiService.PostMultipartAsync<DocumentDto>("/documents/upload", content, progressCallback);
            
            return response ?? throw new Exception("Upload failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload document: {FileName}", file.Name);
            throw;
        }
    }

    public async Task<List<DocumentDto>> GetDocumentsAsync()
    {
        try
        {
            return await _apiService.GetAsync<List<DocumentDto>>("/documents") ?? new List<DocumentDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get documents");
            return new List<DocumentDto>();
        }
    }

    public async Task<DocumentDto> GetDocumentAsync(Guid id)
    {
        try
        {
            return await _apiService.GetAsync<DocumentDto>($"/documents/{id}") 
                ?? throw new Exception("Document not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get document: {Id}", id);
            throw;
        }
    }

    public async Task DeleteDocumentAsync(Guid id)
    {
        try
        {
            await _apiService.DeleteAsync($"/documents/{id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete document: {Id}", id);
            throw;
        }
    }
}