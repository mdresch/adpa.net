using ADPA.Models;
using ADPA.Services;
using ADPA.Services.Notifications;
using System.Collections.Concurrent;
using System.Text.Json;

namespace ADPA.Services.Workflow;

/// <summary>
/// Phase 4: Workflow Automation Service
/// Provides configurable workflows for document routing, approval, and automated actions
/// </summary>
public interface IWorkflowService
{
    Task<WorkflowInstance> CreateWorkflowAsync(Guid documentId, string workflowTemplate);
    Task<WorkflowInstance> GetWorkflowAsync(Guid workflowId);
    Task<IEnumerable<WorkflowInstance>> GetDocumentWorkflowsAsync(Guid documentId);
    Task<WorkflowInstance> AdvanceWorkflowAsync(Guid workflowId, WorkflowAction action, string userId, string? comment = null);
    Task<bool> CancelWorkflowAsync(Guid workflowId, string userId, string reason);
    Task<IEnumerable<WorkflowTemplate>> GetAvailableTemplatesAsync();
    Task<WorkflowTemplate> CreateTemplateAsync(WorkflowTemplate template);
    Task ProcessAutomaticActionsAsync();
}

public class WorkflowService : IWorkflowService
{
    private readonly IDocumentService _documentService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<WorkflowService> _logger;
    
    // In-memory storage for this implementation (would be database in production)
    private readonly ConcurrentDictionary<Guid, WorkflowInstance> _workflows = new();
    private readonly ConcurrentDictionary<string, WorkflowTemplate> _templates = new();

    public WorkflowService(
        IDocumentService documentService,
        INotificationService notificationService,
        ILogger<WorkflowService> logger)
    {
        _documentService = documentService;
        _notificationService = notificationService;
        _logger = logger;
        
        InitializeDefaultTemplates();
    }

    public async Task<WorkflowInstance> CreateWorkflowAsync(Guid documentId, string workflowTemplate)
    {
        try
        {
            _logger.LogInformation("🔄 Creating workflow for document {DocumentId} using template {Template}", documentId, workflowTemplate);

            var template = _templates.GetValueOrDefault(workflowTemplate);
            if (template == null)
            {
                throw new ArgumentException($"Workflow template '{workflowTemplate}' not found");
            }

            var document = await _documentService.GetDocumentAsync(documentId);
            if (document == null)
            {
                throw new ArgumentException($"Document {documentId} not found");
            }

            var workflow = new WorkflowInstance
            {
                WorkflowId = Guid.NewGuid(),
                DocumentId = documentId,
                TemplateName = workflowTemplate,
                Status = WorkflowStatus.Active,
                CurrentStep = 0,
                Steps = template.Steps.Select((s, i) => new WorkflowStepInstance
                {
                    StepId = i,
                    Name = s.Name,
                    Type = s.Type,
                    Status = i == 0 ? WorkflowStepStatus.Pending : WorkflowStepStatus.NotStarted,
                    AssignedTo = s.AssignedTo,
                    RequiredRoles = s.RequiredRoles?.ToList() ?? new List<string>(),
                    Actions = s.Actions?.ToList() ?? new List<WorkflowAction>(),
                    AutoTriggerConditions = s.AutoTriggerConditions?.ToList() ?? new List<string>(),
                    TimeoutMinutes = s.TimeoutMinutes,
                    CreatedAt = DateTime.UtcNow
                }).ToList(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system", // Default to system since DocumentDto doesn't have UploadedBy
                Variables = new Dictionary<string, object>
                {
                    ["DocumentId"] = documentId,
                    ["DocumentName"] = document.FileName,
                    ["DocumentStatus"] = document.Status,
                    ["ContentType"] = document.ContentType,
                    ["FileSize"] = document.FileSize,
                    ["DetectedLanguage"] = document.DetectedLanguage ?? "unknown"
                }
            };

            _workflows.TryAdd(workflow.WorkflowId, workflow);

            // Check for automatic triggers
            await CheckAutomaticTriggersAsync(workflow);

            // Send notification
            await _notificationService.SendWorkflowNotificationAsync(new WorkflowNotification
            {
                WorkflowId = workflow.WorkflowId,
                DocumentId = documentId,
                Type = WorkflowNotificationType.WorkflowStarted,
                Message = $"Workflow '{workflowTemplate}' started for document '{document.FileName}'",
                UserId = workflow.CreatedBy
            });

            _logger.LogInformation("✅ Workflow {WorkflowId} created successfully", workflow.WorkflowId);
            return workflow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to create workflow for document {DocumentId}", documentId);
            throw;
        }
    }

    public async Task<WorkflowInstance> GetWorkflowAsync(Guid workflowId)
    {
        var workflow = _workflows.GetValueOrDefault(workflowId);
        if (workflow == null)
        {
            throw new ArgumentException($"Workflow {workflowId} not found");
        }
        
        return await Task.FromResult(workflow);
    }

    public async Task<IEnumerable<WorkflowInstance>> GetDocumentWorkflowsAsync(Guid documentId)
    {
        var workflows = _workflows.Values.Where(w => w.DocumentId == documentId).ToList();
        return await Task.FromResult(workflows);
    }

    public async Task<WorkflowInstance> AdvanceWorkflowAsync(Guid workflowId, WorkflowAction action, string userId, string? comment = null)
    {
        try
        {
            _logger.LogInformation("⚡ Advancing workflow {WorkflowId} with action {Action} by user {UserId}", 
                workflowId, action.Type, userId);

            var workflow = _workflows.GetValueOrDefault(workflowId);
            if (workflow == null)
            {
                throw new ArgumentException($"Workflow {workflowId} not found");
            }

            if (workflow.Status != WorkflowStatus.Active)
            {
                throw new InvalidOperationException($"Workflow is not active (status: {workflow.Status})");
            }

            var currentStep = workflow.Steps[workflow.CurrentStep];
            if (currentStep.Status == WorkflowStepStatus.Completed)
            {
                throw new InvalidOperationException("Current step is already completed");
            }

            // Validate action is allowed for current step
            if (!currentStep.Actions.Any(a => a.Type == action.Type))
            {
                throw new InvalidOperationException($"Action '{action.Type}' is not allowed for current step");
            }

            // Record the action
            var actionRecord = new WorkflowActionRecord
            {
                Action = action,
                UserId = userId,
                Comment = comment,
                Timestamp = DateTime.UtcNow
            };

            currentStep.ActionHistory.Add(actionRecord);
            currentStep.CompletedAt = DateTime.UtcNow;
            currentStep.CompletedBy = userId;

            // Process the action
            switch (action.Type)
            {
                case WorkflowActionType.Approve:
                    currentStep.Status = WorkflowStepStatus.Completed;
                    await MoveToNextStepAsync(workflow);
                    break;

                case WorkflowActionType.Reject:
                    currentStep.Status = WorkflowStepStatus.Rejected;
                    workflow.Status = WorkflowStatus.Rejected;
                    workflow.CompletedAt = DateTime.UtcNow;
                    break;

                case WorkflowActionType.RequestChanges:
                    currentStep.Status = WorkflowStepStatus.ChangesRequested;
                    // Workflow stays active, waiting for resubmission
                    break;

                case WorkflowActionType.Delegate:
                    if (!string.IsNullOrEmpty(action.AssignTo))
                    {
                        currentStep.AssignedTo = action.AssignTo;
                        currentStep.Status = WorkflowStepStatus.Pending;
                    }
                    break;

                case WorkflowActionType.Complete:
                    currentStep.Status = WorkflowStepStatus.Completed;
                    await MoveToNextStepAsync(workflow);
                    break;
            }

            workflow.UpdatedAt = DateTime.UtcNow;

            // Send notifications
            await SendWorkflowActionNotificationAsync(workflow, actionRecord);

            _logger.LogInformation("✅ Workflow {WorkflowId} advanced successfully", workflowId);
            return workflow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to advance workflow {WorkflowId}", workflowId);
            throw;
        }
    }

    public async Task<bool> CancelWorkflowAsync(Guid workflowId, string userId, string reason)
    {
        try
        {
            _logger.LogInformation("🚫 Cancelling workflow {WorkflowId} by user {UserId}: {Reason}", 
                workflowId, userId, reason);

            var workflow = _workflows.GetValueOrDefault(workflowId);
            if (workflow == null)
            {
                return false;
            }

            workflow.Status = WorkflowStatus.Cancelled;
            workflow.CompletedAt = DateTime.UtcNow;
            workflow.Variables["CancelledBy"] = userId;
            workflow.Variables["CancelReason"] = reason;

            // Send notification
            await _notificationService.SendWorkflowNotificationAsync(new WorkflowNotification
            {
                WorkflowId = workflowId,
                DocumentId = workflow.DocumentId,
                Type = WorkflowNotificationType.WorkflowCancelled,
                Message = $"Workflow cancelled by {userId}: {reason}",
                UserId = userId
            });

            _logger.LogInformation("✅ Workflow {WorkflowId} cancelled successfully", workflowId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to cancel workflow {WorkflowId}", workflowId);
            return false;
        }
    }

    public async Task<IEnumerable<WorkflowTemplate>> GetAvailableTemplatesAsync()
    {
        return await Task.FromResult(_templates.Values);
    }

    public async Task<WorkflowTemplate> CreateTemplateAsync(WorkflowTemplate template)
    {
        try
        {
            _logger.LogInformation("📝 Creating workflow template: {Name}", template.Name);

            if (string.IsNullOrEmpty(template.Name))
            {
                throw new ArgumentException("Template name is required");
            }

            if (_templates.ContainsKey(template.Name))
            {
                throw new ArgumentException($"Template '{template.Name}' already exists");
            }

            template.CreatedAt = DateTime.UtcNow;
            _templates.TryAdd(template.Name, template);

            _logger.LogInformation("✅ Workflow template '{Name}' created successfully", template.Name);
            return await Task.FromResult(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to create workflow template");
            throw;
        }
    }

    public async Task ProcessAutomaticActionsAsync()
    {
        try
        {
            var activeWorkflows = _workflows.Values.Where(w => w.Status == WorkflowStatus.Active).ToList();
            
            foreach (var workflow in activeWorkflows)
            {
                await ProcessWorkflowAutomationAsync(workflow);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to process automatic actions");
        }
    }

    private async Task MoveToNextStepAsync(WorkflowInstance workflow)
    {
        workflow.CurrentStep++;
        
        if (workflow.CurrentStep >= workflow.Steps.Count)
        {
            // Workflow completed
            workflow.Status = WorkflowStatus.Completed;
            workflow.CompletedAt = DateTime.UtcNow;
            
            await _notificationService.SendWorkflowNotificationAsync(new WorkflowNotification
            {
                WorkflowId = workflow.WorkflowId,
                DocumentId = workflow.DocumentId,
                Type = WorkflowNotificationType.WorkflowCompleted,
                Message = "Workflow completed successfully",
                UserId = workflow.CreatedBy
            });
        }
        else
        {
            // Activate next step
            var nextStep = workflow.Steps[workflow.CurrentStep];
            nextStep.Status = WorkflowStepStatus.Pending;
            nextStep.StartedAt = DateTime.UtcNow;
            
            await CheckAutomaticTriggersAsync(workflow);
        }
    }

    private async Task CheckAutomaticTriggersAsync(WorkflowInstance workflow)
    {
        if (workflow.Status != WorkflowStatus.Active || workflow.CurrentStep >= workflow.Steps.Count)
            return;

        var currentStep = workflow.Steps[workflow.CurrentStep];
        
        // Check for automatic triggers
        if (currentStep.AutoTriggerConditions?.Any() == true)
        {
            foreach (var condition in currentStep.AutoTriggerConditions)
            {
                if (await EvaluateConditionAsync(workflow, condition))
                {
                    _logger.LogInformation("🤖 Auto-triggering step {StepName} for workflow {WorkflowId}", 
                        currentStep.Name, workflow.WorkflowId);
                    
                    // Execute automatic action
                    var autoAction = currentStep.Actions.FirstOrDefault(a => a.Type == WorkflowActionType.Complete);
                    if (autoAction != null)
                    {
                        await AdvanceWorkflowAsync(workflow.WorkflowId, autoAction, "system", "Automatic action");
                    }
                    break;
                }
            }
        }
    }

    private async Task<bool> EvaluateConditionAsync(WorkflowInstance workflow, string condition)
    {
        // Simple condition evaluation (would be more sophisticated in production)
        try
        {
            switch (condition.ToLowerInvariant())
            {
                case "document_classified":
                    var document = await _documentService.GetDocumentAsync(workflow.DocumentId);
                    return document?.Status == "Completed";
                
                case "high_confidence":
                    var doc = await _documentService.GetDocumentAsync(workflow.DocumentId);
                    // Check if document has processing results with high confidence
                    return doc?.ProcessingResults?.Any(pr => pr.ConfidenceScore > 0.9) == true;
                
                case "low_risk_document":
                    return workflow.Variables.ContainsKey("RiskLevel") && 
                           workflow.Variables["RiskLevel"]?.ToString() == "Low";
                
                default:
                    return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to evaluate condition: {Condition}", condition);
            return false;
        }
    }

    private async Task ProcessWorkflowAutomationAsync(WorkflowInstance workflow)
    {
        var currentStep = workflow.Steps[workflow.CurrentStep];
        
        // Check for timeouts
        if (currentStep.TimeoutMinutes.HasValue && 
            currentStep.StartedAt.HasValue && 
            DateTime.UtcNow > currentStep.StartedAt.Value.AddMinutes(currentStep.TimeoutMinutes.Value))
        {
            _logger.LogWarning("⏰ Workflow step timeout: {WorkflowId} - {StepName}", 
                workflow.WorkflowId, currentStep.Name);
            
            // Handle timeout (escalate, notify, etc.)
            await HandleStepTimeoutAsync(workflow, currentStep);
        }
    }

    private async Task HandleStepTimeoutAsync(WorkflowInstance workflow, WorkflowStepInstance step)
    {
        await _notificationService.SendWorkflowNotificationAsync(new WorkflowNotification
        {
            WorkflowId = workflow.WorkflowId,
            DocumentId = workflow.DocumentId,
            Type = WorkflowNotificationType.StepTimeout,
            Message = $"Workflow step '{step.Name}' has timed out",
            UserId = step.AssignedTo ?? "system"
        });
    }

    private async Task SendWorkflowActionNotificationAsync(WorkflowInstance workflow, WorkflowActionRecord actionRecord)
    {
        var notificationType = actionRecord.Action.Type switch
        {
            WorkflowActionType.Approve => WorkflowNotificationType.StepApproved,
            WorkflowActionType.Reject => WorkflowNotificationType.StepRejected,
            WorkflowActionType.RequestChanges => WorkflowNotificationType.ChangesRequested,
            WorkflowActionType.Delegate => WorkflowNotificationType.StepDelegated,
            _ => WorkflowNotificationType.StepCompleted
        };

        await _notificationService.SendWorkflowNotificationAsync(new WorkflowNotification
        {
            WorkflowId = workflow.WorkflowId,
            DocumentId = workflow.DocumentId,
            Type = notificationType,
            Message = $"Workflow action: {actionRecord.Action.Type}",
            UserId = actionRecord.UserId,
            Comment = actionRecord.Comment
        });
    }

    private void InitializeDefaultTemplates()
    {
        // Document Approval Workflow
        var approvalTemplate = new WorkflowTemplate
        {
            Name = "DocumentApproval",
            Description = "Standard document approval workflow",
            Steps = new List<WorkflowStep>
            {
                new WorkflowStep
                {
                    Name = "Initial Review",
                    Type = WorkflowStepType.Review,
                    RequiredRoles = new[] { "Reviewer" },
                    Actions = new[]
                    {
                        new WorkflowAction { Type = WorkflowActionType.Approve, Label = "Approve" },
                        new WorkflowAction { Type = WorkflowActionType.RequestChanges, Label = "Request Changes" },
                        new WorkflowAction { Type = WorkflowActionType.Reject, Label = "Reject" }
                    },
                    TimeoutMinutes = 1440 // 24 hours
                },
                new WorkflowStep
                {
                    Name = "Manager Approval",
                    Type = WorkflowStepType.Approval,
                    RequiredRoles = new[] { "Manager" },
                    Actions = new[]
                    {
                        new WorkflowAction { Type = WorkflowActionType.Approve, Label = "Approve" },
                        new WorkflowAction { Type = WorkflowActionType.Reject, Label = "Reject" },
                        new WorkflowAction { Type = WorkflowActionType.Delegate, Label = "Delegate" }
                    },
                    TimeoutMinutes = 2880 // 48 hours
                },
                new WorkflowStep
                {
                    Name = "Final Processing",
                    Type = WorkflowStepType.Processing,
                    AutoTriggerConditions = new[] { "document_classified", "high_confidence" },
                    Actions = new[]
                    {
                        new WorkflowAction { Type = WorkflowActionType.Complete, Label = "Complete" }
                    }
                }
            }
        };

        // Quick Processing Workflow
        var quickTemplate = new WorkflowTemplate
        {
            Name = "QuickProcessing",
            Description = "Automated processing for low-risk documents",
            Steps = new List<WorkflowStep>
            {
                new WorkflowStep
                {
                    Name = "Automatic Classification",
                    Type = WorkflowStepType.Processing,
                    AutoTriggerConditions = new[] { "document_classified", "low_risk_document" },
                    Actions = new[]
                    {
                        new WorkflowAction { Type = WorkflowActionType.Complete, Label = "Auto-Complete" }
                    }
                }
            }
        };

        _templates.TryAdd(approvalTemplate.Name, approvalTemplate);
        _templates.TryAdd(quickTemplate.Name, quickTemplate);
        
        _logger.LogInformation("📋 Initialized {Count} default workflow templates", _templates.Count);
    }
}