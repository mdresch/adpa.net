namespace ADPA.Services.Workflow;

/// <summary>
/// Workflow instance representing an active or completed workflow
/// </summary>
public class WorkflowInstance
{
    public Guid WorkflowId { get; set; }
    public Guid DocumentId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public WorkflowStatus Status { get; set; }
    public int CurrentStep { get; set; }
    public List<WorkflowStepInstance> Steps { get; set; } = new();
    public Dictionary<string, object> Variables { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? CompletedBy { get; set; }
}

/// <summary>
/// Workflow template defining the structure and steps
/// </summary>
public class WorkflowTemplate
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<WorkflowStep> Steps { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Template step definition
/// </summary>
public class WorkflowStep
{
    public string Name { get; set; } = string.Empty;
    public WorkflowStepType Type { get; set; }
    public string? AssignedTo { get; set; }
    public IEnumerable<string>? RequiredRoles { get; set; }
    public IEnumerable<WorkflowAction>? Actions { get; set; }
    public IEnumerable<string>? AutoTriggerConditions { get; set; }
    public int? TimeoutMinutes { get; set; }
}

/// <summary>
/// Runtime workflow step instance
/// </summary>
public class WorkflowStepInstance
{
    public int StepId { get; set; }
    public string Name { get; set; } = string.Empty;
    public WorkflowStepType Type { get; set; }
    public WorkflowStepStatus Status { get; set; }
    public string? AssignedTo { get; set; }
    public List<string> RequiredRoles { get; set; } = new();
    public List<WorkflowAction> Actions { get; set; } = new();
    public List<string> AutoTriggerConditions { get; set; } = new();
    public int? TimeoutMinutes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletedBy { get; set; }
    public List<WorkflowActionRecord> ActionHistory { get; set; } = new();
}

/// <summary>
/// Workflow action definition
/// </summary>
public class WorkflowAction
{
    public WorkflowActionType Type { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? AssignTo { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
}

/// <summary>
/// Record of an action taken in the workflow
/// </summary>
public class WorkflowActionRecord
{
    public WorkflowAction Action { get; set; } = new();
    public string UserId { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Workflow notification for real-time updates
/// </summary>
public class WorkflowNotification
{
    public Guid WorkflowId { get; set; }
    public Guid DocumentId { get; set; }
    public WorkflowNotificationType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Workflow status enumeration
/// </summary>
public enum WorkflowStatus
{
    Created,
    Active,
    Completed,
    Cancelled,
    Rejected,
    OnHold
}

/// <summary>
/// Workflow step types
/// </summary>
public enum WorkflowStepType
{
    Review,
    Approval,
    Processing,
    Notification,
    Integration,
    Decision
}

/// <summary>
/// Workflow step status
/// </summary>
public enum WorkflowStepStatus
{
    NotStarted,
    Pending,
    InProgress,
    Completed,
    Rejected,
    ChangesRequested,
    Timeout,
    Skipped
}

/// <summary>
/// Types of workflow actions
/// </summary>
public enum WorkflowActionType
{
    Approve,
    Reject,
    RequestChanges,
    Delegate,
    Complete,
    Skip,
    Escalate,
    Hold,
    Resume
}

/// <summary>
/// Types of workflow notifications
/// </summary>
public enum WorkflowNotificationType
{
    WorkflowStarted,
    WorkflowCompleted,
    WorkflowCancelled,
    StepStarted,
    StepCompleted,
    StepApproved,
    StepRejected,
    StepTimeout,
    StepDelegated,
    ChangesRequested,
    AssignmentChanged
}