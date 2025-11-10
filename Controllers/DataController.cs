using Microsoft.AspNetCore.Mvc;
using ADPA.Services;
using ADPA.Models.DTOs;

namespace ADPA.Controllers;

/// <summary>
/// Controller for data processing operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DataController : ControllerBase
{
    private readonly IDataProcessingService _dataProcessingService;
    private readonly ILogger<DataController> _logger;

    public DataController(IDataProcessingService dataProcessingService, ILogger<DataController> logger)
    {
        _dataProcessingService = dataProcessingService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all processed data
    /// </summary>
    /// <returns>List of processed data</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DataResponseDto>), 200)]
    public async Task<ActionResult<IEnumerable<DataResponseDto>>> GetAllData()
    {
        try
        {
            var data = await _dataProcessingService.GetAllDataAsync();
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving data");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Gets processed data by ID
    /// </summary>
    /// <param name="id">The data ID</param>
    /// <returns>Processed data information</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DataResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<DataResponseDto>> GetDataById(int id)
    {
        try
        {
            var data = await _dataProcessingService.GetDataByIdAsync(id);
            if (data == null)
            {
                return NotFound($"Data with ID {id} not found");
            }

            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving data by ID: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Creates a new data processing request
    /// </summary>
    /// <param name="request">The data processing request</param>
    /// <returns>Created data processing information</returns>
    [HttpPost]
    [ProducesResponseType(typeof(DataResponseDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<DataResponseDto>> CreateDataProcessingRequest([FromBody] CreateDataRequestDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.RawData))
            {
                return BadRequest("RawData cannot be null or empty");
            }

            var result = await _dataProcessingService.CreateDataProcessingRequestAsync(request);
            return CreatedAtAction(
                nameof(GetDataById),
                new { id = result.Id },
                result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating data processing request");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Triggers processing of pending data items
    /// </summary>
    /// <returns>Success message</returns>
    [HttpPost("process-pending")]
    [ProducesResponseType(200)]
    public async Task<ActionResult> ProcessPendingData()
    {
        try
        {
            await _dataProcessingService.ProcessPendingDataAsync();
            return Ok("Processing of pending data initiated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing pending data");
            return StatusCode(500, "Internal server error");
        }
    }
}