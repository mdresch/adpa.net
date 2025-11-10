using Microsoft.AspNetCore.Mvc;
using Adpa.Api.Models;
using Adpa.Api.Services;

namespace Adpa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeaturesController : ControllerBase
{
    private readonly IFeatureService _featureService;
    private readonly ILogger<FeaturesController> _logger;

    public FeaturesController(IFeatureService featureService, ILogger<FeaturesController> logger)
    {
        _featureService = featureService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Feature>>> GetFeatures()
    {
        try
        {
            var features = await _featureService.GetAllFeaturesAsync();
            return Ok(features);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving features");
            return StatusCode(500, "An error occurred while retrieving features");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Feature>> GetFeature(int id)
    {
        try
        {
            var feature = await _featureService.GetFeatureByIdAsync(id);
            if (feature == null)
                return NotFound($"Feature with ID {id} not found");

            return Ok(feature);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving feature {FeatureId}", id);
            return StatusCode(500, "An error occurred while retrieving the feature");
        }
    }

    [HttpPost]
    public async Task<ActionResult<Feature>> CreateFeature([FromBody] Feature feature)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(feature.Name))
                return BadRequest("Feature name is required");

            var createdFeature = await _featureService.CreateFeatureAsync(feature);
            return CreatedAtAction(nameof(GetFeature), new { id = createdFeature.Id }, createdFeature);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating feature");
            return StatusCode(500, "An error occurred while creating the feature");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Feature>> UpdateFeature(int id, [FromBody] Feature feature)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(feature.Name))
                return BadRequest("Feature name is required");

            var updatedFeature = await _featureService.UpdateFeatureAsync(id, feature);
            if (updatedFeature == null)
                return NotFound($"Feature with ID {id} not found");

            return Ok(updatedFeature);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating feature {FeatureId}", id);
            return StatusCode(500, "An error occurred while updating the feature");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFeature(int id)
    {
        try
        {
            var result = await _featureService.DeleteFeatureAsync(id);
            if (!result)
                return NotFound($"Feature with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting feature {FeatureId}", id);
            return StatusCode(500, "An error occurred while deleting the feature");
        }
    }
}
