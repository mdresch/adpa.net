namespace ADPA.Services.Security;

/// <summary>
/// Phase 5: Advanced Authorization Models
/// Comprehensive RBAC and ABAC system with dynamic permissions and resource-level access control
/// </summary>

/// <summary>
/// Permission types for granular access control
/// </summary>
public enum PermissionType
{
    Read,
    Write, 
    Delete,
    Execute,
    Admin,
    Create,
    Update,
    Approve,
    Publish,
    Export,
    Import,
    Audit,
    Configure,
    Manage
}

/// <summary>
/// Resource types that can be secured
/// </summary>
public enum ResourceType
{
    User,
    Role,
    Document,
    Report,
    Analytics,
    Configuration,
    AuditLog,
    ApiKey,
    Session,
    Notification,
    Workflow,
    Dashboard,
    Integration,
    Backup,
    System
}

/// <summary>
/// Access decision enumeration
/// </summary>
public enum AccessDecision
{
    Allow,
    Deny,
    NotApplicable
}

/// <summary>
/// Context for attribute-based access control
/// </summary>
public enum ContextAttribute
{
    TimeOfDay,
    DayOfWeek,
    Location,
    IpAddress,
    DeviceType,
    UserAgent,
    Department,
    Project,
    DataClassification,
    RequestMethod,
    ApiVersion,
    ClientType
}

/// <summary>
/// Role definition with hierarchical support
/// </summary>
public class Role
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsSystemRole { get; set; } // Cannot be deleted
    public int Level { get; set; } // Hierarchy level (0 = top level)
    
    // Hierarchical relationships
    public Guid? ParentRoleId { get; set; }
    public Role? ParentRole { get; set; }
    public List<Role> ChildRoles { get; set; } = new();
    
    // Permissions
    public List<Permission> DirectPermissions { get; set; } = new();
    public List<Permission> InheritedPermissions { get; set; } = new();
    
    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Permission definition with resource-level granularity
/// </summary>
public class Permission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PermissionType Type { get; set; }
    public ResourceType ResourceType { get; set; }
    public string? ResourceId { get; set; } // Specific resource instance
    public bool IsActive { get; set; } = true;
    
    // Scope and conditions
    public string Scope { get; set; } = "*"; // *, specific ID, pattern
    public List<PermissionCondition> Conditions { get; set; } = new();
    
    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Permission condition for ABAC (Attribute-Based Access Control)
/// </summary>
public class PermissionCondition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PermissionId { get; set; }
    public ContextAttribute Attribute { get; set; }
    public string Operator { get; set; } = "equals"; // equals, not_equals, contains, in, between, etc.
    public string Value { get; set; } = string.Empty;
    public string? SecondaryValue { get; set; } // For range conditions
    public bool IsRequired { get; set; } = true;
}

/// <summary>
/// User role assignment with temporal and conditional support
/// </summary>
public class UserRole
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = new();
    
    // Temporal constraints
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Assignment context
    public string AssignedBy { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public string? AssignmentReason { get; set; }
    public List<RoleCondition> Conditions { get; set; } = new();
    
    // Metadata
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Role condition for conditional role activation
/// </summary>
public class RoleCondition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserRoleId { get; set; }
    public ContextAttribute Attribute { get; set; }
    public string Operator { get; set; } = "equals";
    public string Value { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = true;
}

/// <summary>
/// Authorization policy for complex access rules
/// </summary>
public class AuthorizationPolicy
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 0; // Higher number = higher priority
    
    // Policy rules
    public List<PolicyRule> Rules { get; set; } = new();
    public string CombinationLogic { get; set; } = "AND"; // AND, OR
    
    // Applicability
    public List<ResourceType> ApplicableResourceTypes { get; set; } = new();
    public List<string> ApplicableResourceIds { get; set; } = new();
    
    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Individual rule within an authorization policy
/// </summary>
public class PolicyRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PolicyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public AccessDecision Decision { get; set; } = AccessDecision.Deny;
    
    // Rule conditions
    public List<RuleCondition> Conditions { get; set; } = new();
    public string ConditionLogic { get; set; } = "AND"; // AND, OR
    
    // Rule targets
    public List<string> RequiredRoles { get; set; } = new();
    public List<string> RequiredPermissions { get; set; } = new();
    public List<string> ExcludedRoles { get; set; } = new();
    
    public int Order { get; set; } = 0;
}

/// <summary>
/// Condition within a policy rule
/// </summary>
public class RuleCondition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RuleId { get; set; }
    public ContextAttribute Attribute { get; set; }
    public string Operator { get; set; } = "equals";
    public string Value { get; set; } = string.Empty;
    public bool IsNegated { get; set; } = false;
}

/// <summary>
/// Authorization context for access decisions
/// </summary>
public class AuthorizationContext
{
    public Guid UserId { get; set; }
    public List<string> UserRoles { get; set; } = new();
    public List<string> UserPermissions { get; set; } = new();
    public ResourceType ResourceType { get; set; }
    public string? ResourceId { get; set; }
    public PermissionType RequestedPermission { get; set; }
    
    // Context attributes
    public DateTime RequestTime { get; set; } = DateTime.UtcNow;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public Dictionary<ContextAttribute, object> Attributes { get; set; } = new();
    
    // Additional context
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}

/// <summary>
/// Authorization decision result
/// </summary>
public class AuthorizationResult
{
    public bool IsAuthorized { get; set; }
    public AccessDecision Decision { get; set; } = AccessDecision.Deny;
    public string Reason { get; set; } = string.Empty;
    public List<string> MatchedPolicies { get; set; } = new();
    public List<string> MatchedRules { get; set; } = new();
    public List<string> FailedConditions { get; set; } = new();
    public DateTime DecisionTime { get; set; } = DateTime.UtcNow;
    public TimeSpan ProcessingTime { get; set; }
    public Dictionary<string, object> AdditionalInfo { get; set; } = new();
}

/// <summary>
/// Cached permission for performance optimization
/// </summary>
public class PermissionCache
{
    public string Key { get; set; } = string.Empty; // Composite key
    public Guid UserId { get; set; }
    public string ResourceType { get; set; } = string.Empty;
    public string? ResourceId { get; set; }
    public string Permission { get; set; } = string.Empty;
    public bool IsAuthorized { get; set; }
    public DateTime CachedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public string CacheVersion { get; set; } = string.Empty; // For cache invalidation
}

/// <summary>
/// Resource ownership for resource-level access control
/// </summary>
public class ResourceOwnership
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public ResourceType ResourceType { get; set; }
    public string ResourceId { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public List<Guid> CoOwners { get; set; } = new();
    public List<ResourcePermissionGrant> PermissionGrants { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Permission grant for specific resources
/// </summary>
public class ResourcePermissionGrant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ResourceOwnershipId { get; set; }
    public Guid GranteeId { get; set; } // User or Role ID
    public bool IsRole { get; set; } // True if GranteeId is a role
    public List<PermissionType> GrantedPermissions { get; set; } = new();
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string GrantedBy { get; set; } = string.Empty;
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Dynamic permission based on runtime conditions
/// </summary>
public class DynamicPermission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Expression { get; set; } = string.Empty; // C# expression or rule
    public PermissionType PermissionType { get; set; }
    public ResourceType ResourceType { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Group-based access control
/// </summary>
public class Group
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    
    // Group membership
    public List<Guid> MemberIds { get; set; } = new();
    public List<Guid> AdminIds { get; set; } = new();
    
    // Group permissions
    public List<Permission> Permissions { get; set; } = new();
    public List<Role> Roles { get; set; } = new();
    
    // Hierarchy
    public Guid? ParentGroupId { get; set; }
    public Group? ParentGroup { get; set; }
    public List<Group> SubGroups { get; set; } = new();
    
    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Request models for authorization operations
/// </summary>
public class CreateRoleRequest
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? ParentRoleId { get; set; }
    public List<Guid> PermissionIds { get; set; } = new();
}

public class UpdateRoleRequest
{
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public List<Guid>? PermissionIds { get; set; }
    public bool? IsActive { get; set; }
}

public class AssignRoleRequest
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string? AssignmentReason { get; set; }
    public List<RoleCondition> Conditions { get; set; } = new();
}

public class CreatePermissionRequest
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PermissionType Type { get; set; }
    public ResourceType ResourceType { get; set; }
    public string? ResourceId { get; set; }
    public string Scope { get; set; } = "*";
    public List<PermissionCondition> Conditions { get; set; } = new();
}

public class AuthorizeRequest
{
    public Guid UserId { get; set; }
    public ResourceType ResourceType { get; set; }
    public string? ResourceId { get; set; }
    public PermissionType Permission { get; set; }
    public Dictionary<ContextAttribute, object> Context { get; set; } = new();
}

public class CreatePolicyRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<PolicyRule> Rules { get; set; } = new();
    public List<ResourceType> ApplicableResourceTypes { get; set; } = new();
    public int Priority { get; set; } = 0;
}

/// <summary>
/// Response models for authorization operations
/// </summary>
public class RoleResponse
{
    public Role Role { get; set; } = new();
    public List<Permission> EffectivePermissions { get; set; } = new();
    public List<SecureUser> Members { get; set; } = new();
}

public class UserPermissionsResponse
{
    public Guid UserId { get; set; }
    public List<Role> Roles { get; set; } = new();
    public List<Permission> DirectPermissions { get; set; } = new();
    public List<Permission> InheritedPermissions { get; set; } = new();
    public List<Permission> EffectivePermissions { get; set; } = new();
    public Dictionary<ResourceType, List<string>> ResourceAccess { get; set; } = new();
}

public class AuthorizationMatrixResponse
{
    public List<SecurityMatrixRow> Matrix { get; set; } = new();
    public Dictionary<string, int> Summary { get; set; } = new();
}

public class SecurityMatrixRow
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public Dictionary<string, bool> Permissions { get; set; } = new();
}