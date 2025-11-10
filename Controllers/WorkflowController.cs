using ADPA.Services.Workflow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ADPA.Controllers;

/// <summary>
/// Phase 4: Workflow Automation API Controller
/// Provides workflow management and automation capabilities
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkflowController : ControllerBase
{
    private readonly IWorkflowService _workflowService;
    private readonly ILogger<WorkflowController> _logger;

    public WorkflowController(IWorkflowService workflowService, ILogger<WorkflowController> logger)
    {
        _workflowService = workflowService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new workflow for a document
    /// </summary>
    /// <param name="request">Workflow creation request</param>
    /// <returns>Created workflow instance</returns>
    [HttpPost("create")]
    public async Task<ActionResult<WorkflowInstance>> CreateWorkflowAsync([FromBody] CreateWorkflowRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("🔄 Creating workflow for document {DocumentId} using template {Template} by user {UserId}", 
                request.DocumentId, request.TemplateName, userId);

            if (request.DocumentId == Guid.Empty)
            {
                return BadRequest(new { error = "DocumentId is required" });
            }

            if (string.IsNullOrEmpty(request.TemplateName))
            {
                return BadRequest(new { error = "TemplateName is required" });
            }

            var workflow = await _workflowService.CreateWorkflowAsync(request.DocumentId, request.TemplateName);

            _logger.LogInformation("✅ Workflow {WorkflowId} created successfully", workflow.WorkflowId);
            return Ok(workflow);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "❌ Invalid workflow creation request");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Workflow creation failed for document {DocumentId}", request.DocumentId);
            return StatusCode(500, new { error = "Workflow creation failed", details = ex.Message });
        }
    }

    /// <summary>
    /// Get workflow by ID
    /// </summary>
    /// <param name="workflowId">Workflow ID</param>
    /// <returns>Workflow instance</returns>
    [HttpGet("{workflowId}")]
    public async Task<ActionResult<WorkflowInstance>> GetWorkflowAsync(Guid workflowId)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("📋 Retrieving workflow {WorkflowId} for user {UserId}", workflowId, userId);

            var workflow = await _workflowService.GetWorkflowAsync(workflowId);

            return Ok(workflow);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "❌ Workflow {WorkflowId} not found", workflowId);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to retrieve workflow {WorkflowId}", workflowId);
            return StatusCode(500, new { error = "Failed to retrieve workflow", details = ex.Message });
        }
    }

    /// <summary>
    /// Get workflows for a document
    /// </summary>
    /// <param name="documentId">Document ID</param>
    /// <returns>List of workflows for the document</returns>
    [HttpGet("document/{documentId}")]
    public async Task<ActionResult<IEnumerable<WorkflowInstance>>> GetDocumentWorkflowsAsync(Guid documentId)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("📋 Retrieving workflows for document {DocumentId} by user {UserId}", documentId, userId);

            var workflows = await _workflowService.GetDocumentWorkflowsAsync(documentId);

            return Ok(workflows);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to retrieve workflows for document {DocumentId}", documentId);
            return StatusCode(500, new { error = "Failed to retrieve document workflows", details = ex.Message });
        }
    }

    /// <summary>
    /// Advance workflow with an action
    /// </summary>
    /// <param name="workflowId">Workflow ID</param>
    /// <param name="request">Workflow action request</param>
    /// <returns>Updated workflow instance</returns>
    [HttpPost("{workflowId}/action")]
    public async Task<ActionResult<WorkflowInstance>> AdvanceWorkflowAsync(Guid workflowId, [FromBody] WorkflowActionRequest request)
    {
        try
        {
            var userId = GetCurrentUserId().ToString();
            
            _logger.LogInformation("⚡ Advancing workflow {WorkflowId} with action {ActionType} by user {UserId}", 
                workflowId, request.ActionType, userId);

            var action = new WorkflowAction
            {
                Type = request.ActionType,
                Label = request.ActionType.ToString(),
                AssignTo = request.AssignTo,
                Parameters = request.Parameters
            };

            var workflow = await _workflowService.AdvanceWorkflowAsync(workflowId, action, userId, request.Comment);

            _logger.LogInformation("✅ Workflow {WorkflowId} advanced successfully", workflowId);
            return Ok(workflow);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "❌ Invalid workflow action request for workflow {WorkflowId}", workflowId);
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "❌ Invalid workflow operation for workflow {WorkflowId}", workflowId);
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to advance workflow {WorkflowId}", workflowId);
            return StatusCode(500, new { error = "Failed to advance workflow", details = ex.Message });
        }
    }

    /// <summary>
    /// Cancel a workflow
    /// </summary>
    /// <param name="workflowId">Workflow ID</param>
    /// <param name="request">Cancellation request</param>
    /// <returns>Success status</returns>
    [HttpPost("{workflowId}/cancel")]
    public async Task<ActionResult> CancelWorkflowAsync(Guid workflowId, [FromBody] CancelWorkflowRequest request)
    {
        try
        {
            var userId = GetCurrentUserId().ToString();
            
            _logger.LogInformation("🚫 Cancelling workflow {WorkflowId} by user {UserId}: {Reason}", 
                workflowId, userId, request.Reason);

            var success = await _workflowService.CancelWorkflowAsync(workflowId, userId, request.Reason);

            if (success)
            {
                _logger.LogInformation("✅ Workflow {WorkflowId} cancelled successfully", workflowId);
                return Ok(new { message = "Workflow cancelled successfully" });
            }
            else
            {
                return NotFound(new { error = "Workflow not found" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to cancel workflow {WorkflowId}", workflowId);
            return StatusCode(500, new { error = "Failed to cancel workflow", details = ex.Message });
        }
    }

    /// <summary>
    /// Get available workflow templates
    /// </summary>
    /// <returns>List of available templates</returns>
    [HttpGet("templates")]
    public async Task<ActionResult<IEnumerable<WorkflowTemplate>>> GetTemplatesAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("📋 Retrieving workflow templates for user {UserId}", userId);

            var templates = await _workflowService.GetAvailableTemplatesAsync();

            return Ok(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to retrieve workflow templates");
            return StatusCode(500, new { error = "Failed to retrieve workflow templates", details = ex.Message });
        }
    }

    /// <summary>
    /// Create a new workflow template
    /// </summary>
    /// <param name="template">Template definition</param>
    /// <returns>Created template</returns>
    [HttpPost("templates")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<WorkflowTemplate>> CreateTemplateAsync([FromBody] WorkflowTemplate template)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("📝 Creating workflow template '{Name}' by user {UserId}", template.Name, userId);

            template.CreatedBy = userId.ToString();
            var createdTemplate = await _workflowService.CreateTemplateAsync(template);

            _logger.LogInformation("✅ Workflow template '{Name}' created successfully", template.Name);
            return Ok(createdTemplate);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "❌ Invalid workflow template creation request");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to create workflow template");
            return StatusCode(500, new { error = "Failed to create workflow template", details = ex.Message });
        }
    }

    /// <summary>
    /// Get workflow automation capabilities and supported features
    /// </summary>
    /// <returns>Workflow service capabilities</returns>
    [HttpGet("capabilities")]
    public ActionResult<object> GetWorkflowCapabilities()
    {
        try
        {
            var capabilities = new
            {
                Version = "4.0",
                SupportedWorkflowTypes = new[]
                {
                    "DocumentApproval",
                    "QuickProcessing",
                    "ComplianceReview",
                    "QualityAssurance"
                },
                SupportedStepTypes = new[]
                {
                    "Review",
                    "Approval", 
                    "Processing",
                    "Notification",
                    "Integration",
                    "Decision"
                },
                SupportedActions = new[]
                {
                    "Approve",
                    "Reject",
                    "RequestChanges",
                    "Delegate",
                    "Complete",
                    "Skip",
                    "Escalate",
                    "Hold",
                    "Resume"
                },
                Features = new[]
                {
                    "Configurable Workflow Templates",
                    "Automatic Step Triggers",
                    "Role-based Assignment",
                    "Timeout Handling",
                    "Real-time Notifications",
                    "Action History Tracking",
                    "Variable Context Support",
                    "Conditional Logic"
                },
                AutomationFeatures = new[]
                {
                    "Document Classification Triggers",
                    "Confidence-based Routing",
                    "Risk-level Assessment",
                    "Timeout Escalation",
                    "Automatic Approvals"
                },
                Limits = new
                {
                    MaxStepsPerWorkflow = 50,
                    MaxActiveWorkflows = 10000,
                    MaxTemplateVariables = 100,
                    ProcessingTimeout = "24 hours"
                }
            };

            return Ok(capabilities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to get workflow capabilities");
            return StatusCode(500, new { error = "Failed to get workflow capabilities", details = ex.Message });
        }
    }

    /// <summary>
    /// Get current user ID from claims
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }
        return userId;
    }
}

/// <summary>
/// Request model for creating a workflow
/// </summary>
public class CreateWorkflowRequest
{
    public Guid DocumentId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
}

/// <summary>
/// Request model for workflow actions
/// </summary>
public class WorkflowActionRequest
{
    public WorkflowActionType ActionType { get; set; }
    public string? Comment { get; set; }
    public string? AssignTo { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
}

/// <summary>
/// Request model for workflow cancellation
/// </summary>
public class CancelWorkflowRequest
{
    public string Reason { get; set; } = string.Empty;
}