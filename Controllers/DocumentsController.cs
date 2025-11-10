using Microsoft.AspNetCore.Mvc;
using ADPA.Services;
using ADPA.Models.DTOs;

namespace ADPA.Controllers;

/// <summary>
/// Controller for document operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(IDocumentService documentService, ILogger<DocumentsController> logger)
    {
        _documentService = documentService;
        _logger = logger;
    }

    /// <summary>
    /// Upload a document for processing
    /// </summary>
    /// <param name="uploadDto">Document upload details</param>
    /// <returns>Created document information</returns>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(DocumentDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<DocumentDto>> UploadDocument([FromForm] DocumentUploadDto uploadDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // For demo purposes, use a default user ID
            // In production, get this from authentication context
            var userId = Guid.Parse("22222222-2222-2222-2222-222222222222"); // Demo user

            var document = await _documentService.UploadDocumentAsync(userId, uploadDto);
            return CreatedAtAction(nameof(GetDocument), new { id = document.Id }, document);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get document by ID
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <returns>Document information</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DocumentDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<DocumentDto>> GetDocument(Guid id)
    {
        try
        {
            var document = await _documentService.GetDocumentAsync(id);
            if (document == null)
            {
                return NotFound($"Document with ID {id} not found");
            }

            return Ok(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving document: {DocumentId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get all documents
    /// </summary>
    /// <returns>List of all documents</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DocumentDto>), 200)]
    public async Task<ActionResult<IEnumerable<DocumentDto>>> GetAllDocuments()
    {
        try
        {
            var documents = await _documentService.GetAllDocumentsAsync();
            return Ok(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving documents");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get documents for current user
    /// </summary>
    /// <returns>List of user's documents</returns>
    [HttpGet("my-documents")]
    [ProducesResponseType(typeof(IEnumerable<DocumentDto>), 200)]
    public async Task<ActionResult<IEnumerable<DocumentDto>>> GetMyDocuments()
    {
        try
        {
            // For demo purposes, use a default user ID
            // In production, get this from authentication context
            var userId = Guid.Parse("22222222-2222-2222-2222-222222222222"); // Demo user

            var documents = await _documentService.GetUserDocumentsAsync(userId);
            return Ok(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user documents");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete document
    /// </summary>
    /// <param name="id">Document ID to delete</param>
    /// <returns>Deletion result</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> DeleteDocument(Guid id)
    {
        try
        {
            var deleted = await _documentService.DeleteDocumentAsync(id);
            if (!deleted)
            {
                return NotFound($"Document with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document: {DocumentId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Process all pending documents
    /// </summary>
    /// <returns>Processing initiation result</returns>
    [HttpPost("process-pending")]
    [ProducesResponseType(200)]
    public async Task<ActionResult> ProcessPendingDocuments()
    {
        try
        {
            await _documentService.ProcessPendingDocumentsAsync();
            return Ok("Processing of pending documents initiated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing pending documents");
            return StatusCode(500, "Internal server error");
        }
    }
}