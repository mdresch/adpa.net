using Microsoft.AspNetCore.Mvc;
using ADPA.Services;
using ADPA.Models.DTOs;

namespace ADPA.Controllers;

/// <summary>
/// Controller for health check operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IHealthCheckService _healthCheckService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(IHealthCheckService healthCheckService, ILogger<HealthController> logger)
    {
        _healthCheckService = healthCheckService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the health status of the application
    /// </summary>
    /// <returns>Health status information</returns>
    [HttpGet]
    [ProducesResponseType(typeof(HealthCheckResponseDto), 200)]
    public async Task<ActionResult<HealthCheckResponseDto>> GetHealth()
    {
        try
        {
            var healthStatus = await _healthCheckService.GetHealthStatusAsync();
            return Ok(healthStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting health status");
            return StatusCode(500, "Internal server error");
        }
    }
}