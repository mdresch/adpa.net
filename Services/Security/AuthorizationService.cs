using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;

namespace ADPA.Services.Security;

/// <summary>
/// Phase 5: Advanced Authorization Service Interface
/// Comprehensive RBAC and ABAC system with dynamic permissions and caching
/// </summary>
public interface IAuthorizationService
{
    // Core Authorization
    Task<AuthorizationResult> AuthorizeAsync(AuthorizationContext context);
    Task<AuthorizationResult> AuthorizeAsync(Guid userId, ResourceType resourceType, string? resourceId, PermissionType permission, Dictionary<ContextAttribute, object>? attributes = null);
    Task<bool> HasPermissionAsync(Guid userId, string permission);
    Task<bool> HasRoleAsync(Guid userId, string roleName);
    Task<bool> IsInGroupAsync(Guid userId, string groupName);
    
    // Role Management
    Task<Role> CreateRoleAsync(CreateRoleRequest request);
    Task<Role?> GetRoleAsync(Guid roleId);
    Task<Role?> GetRoleByNameAsync(string roleName);
    Task<List<Role>> GetRolesAsync();
    Task<Role> UpdateRoleAsync(Guid roleId, UpdateRoleRequest request);
    Task<bool> DeleteRoleAsync(Guid roleId);
    Task<List<Permission>> GetRolePermissionsAsync(Guid roleId, bool includeInherited = true);
    Task<List<Role>> GetUserRolesAsync(Guid userId, bool includeInactive = false);
    
    // Role Assignment
    Task<bool> AssignRoleAsync(AssignRoleRequest request);
    Task<bool> RemoveRoleAsync(Guid userId, Guid roleId);
    Task<List<UserRole>> GetUserRoleAssignmentsAsync(Guid userId);
    Task<bool> IsRoleActiveAsync(Guid userId, Guid roleId, DateTime? atTime = null);
    
    // Permission Management
    Task<Permission> CreatePermissionAsync(CreatePermissionRequest request);
    Task<Permission?> GetPermissionAsync(Guid permissionId);
    Task<Permission?> GetPermissionByNameAsync(string permissionName);
    Task<List<Permission>> GetPermissionsAsync();
    Task<Permission> UpdatePermissionAsync(Guid permissionId, Permission permission);
    Task<bool> DeletePermissionAsync(Guid permissionId);
    Task<List<Permission>> GetUserPermissionsAsync(Guid userId, bool includeInherited = true);
    Task<List<Permission>> GetEffectivePermissionsAsync(Guid userId);
    
    // Policy Management
    Task<AuthorizationPolicy> CreatePolicyAsync(CreatePolicyRequest request);
    Task<AuthorizationPolicy?> GetPolicyAsync(Guid policyId);
    Task<List<AuthorizationPolicy>> GetPoliciesAsync();
    Task<AuthorizationPolicy> UpdatePolicyAsync(Guid policyId, AuthorizationPolicy policy);
    Task<bool> DeletePolicyAsync(Guid policyId);
    Task<List<AuthorizationPolicy>> GetApplicablePoliciesAsync(ResourceType resourceType, string? resourceId = null);
    
    // Group Management
    Task<Group> CreateGroupAsync(string name, string displayName, string description);
    Task<Group?> GetGroupAsync(Guid groupId);
    Task<List<Group>> GetGroupsAsync();
    Task<bool> AddUserToGroupAsync(Guid groupId, Guid userId);
    Task<bool> RemoveUserFromGroupAsync(Guid groupId, Guid userId);
    Task<List<Group>> GetUserGroupsAsync(Guid userId);
    
    // Resource-Level Permissions
    Task<ResourceOwnership> CreateResourceOwnershipAsync(ResourceType resourceType, string resourceId, Guid ownerId);
    Task<bool> GrantResourcePermissionAsync(string resourceType, string resourceId, Guid granteeId, bool isRole, List<PermissionType> permissions);
    Task<bool> RevokeResourcePermissionAsync(string resourceType, string resourceId, Guid granteeId);
    Task<List<ResourcePermissionGrant>> GetResourcePermissionsAsync(string resourceType, string resourceId);
    Task<bool> IsResourceOwnerAsync(Guid userId, ResourceType resourceType, string resourceId);
    
    // Dynamic Permissions
    Task<DynamicPermission> CreateDynamicPermissionAsync(string name, string expression, PermissionType permissionType, ResourceType resourceType);
    Task<bool> EvaluateDynamicPermissionAsync(Guid dynamicPermissionId, AuthorizationContext context);
    Task<List<DynamicPermission>> GetDynamicPermissionsAsync(ResourceType resourceType);
    
    // Cache Management
    Task InvalidateUserCacheAsync(Guid userId);
    Task InvalidateRoleCacheAsync(Guid roleId);
    Task InvalidatePermissionCacheAsync(Guid permissionId);
    Task InvalidateAllCacheAsync();
    Task<bool> IsCachedAsync(string cacheKey);
    
    // Reporting and Analysis
    Task<UserPermissionsResponse> GetUserPermissionMatrixAsync(Guid userId);
    Task<AuthorizationMatrixResponse> GetAuthorizationMatrixAsync(List<Guid>? userIds = null);
    Task<List<string>> GetUnusedPermissionsAsync();
    Task<List<string>> GetOverPrivilegedUsersAsync();
    Task<Dictionary<string, int>> GetPermissionUsageStatsAsync();
    
    // Bulk Operations
    Task<bool> BulkAssignRolesAsync(List<Guid> userIds, List<Guid> roleIds);
    Task<bool> BulkRemoveRolesAsync(List<Guid> userIds, List<Guid> roleIds);
    Task<bool> BulkUpdatePermissionsAsync(Guid roleId, List<Guid> permissionIds);
    
    // Security Analysis
    Task<List<string>> DetectPermissionConflictsAsync();
    Task<List<string>> DetectOrphanedPermissionsAsync();
    Task<bool> ValidateRoleHierarchyAsync();
    Task<Dictionary<string, object>> GetSecurityMetricsAsync();
}

/// <summary>
/// Advanced Authorization Service Implementation
/// </summary>
public class AuthorizationService : IAuthorizationService
{
    private readonly IRoleStore _roleStore;
    private readonly IPermissionStore _permissionStore;
    private readonly IPolicyStore _policyStore;
    private readonly IGroupStore _groupStore;
    private readonly IResourceStore _resourceStore;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AuthorizationService> _logger;
    private readonly SecurityConfiguration _securityConfig;
    private readonly IPolicyEvaluator _policyEvaluator;
    private readonly IExpressionEvaluator _expressionEvaluator;

    public AuthorizationService(
        IRoleStore roleStore,
        IPermissionStore permissionStore,
        IPolicyStore policyStore,
        IGroupStore groupStore,
        IResourceStore resourceStore,
        IMemoryCache cache,
        ILogger<AuthorizationService> logger,
        IOptions<SecurityConfiguration> securityConfig,
        IPolicyEvaluator policyEvaluator,
        IExpressionEvaluator expressionEvaluator)
    {
        _roleStore = roleStore;
        _permissionStore = permissionStore;
        _policyStore = policyStore;
        _groupStore = groupStore;
        _resourceStore = resourceStore;
        _cache = cache;
        _logger = logger;
        _securityConfig = securityConfig.Value;
        _policyEvaluator = policyEvaluator;
        _expressionEvaluator = expressionEvaluator;
    }

    public async Task<AuthorizationResult> AuthorizeAsync(AuthorizationContext context)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            _logger.LogDebug("Authorizing user {UserId} for {ResourceType}:{ResourceId} with permission {Permission}",
                context.UserId, context.ResourceType, context.ResourceId, context.RequestedPermission);

            // Check cache first
            var cacheKey = GenerateCacheKey(context);
            if (_cache.TryGetValue(cacheKey, out AuthorizationResult? cachedResult))
            {
                _logger.LogDebug("Cache hit for authorization request");
                cachedResult!.ProcessingTime = stopwatch.Elapsed;
                return cachedResult;
            }

            var result = new AuthorizationResult
            {
                IsAuthorized = false,
                Decision = AccessDecision.Deny,
                DecisionTime = DateTime.UtcNow
            };

            // Step 1: Check if user exists and is active
            var user = await GetUserAsync(context.UserId);
            if (user == null || user.Status != AccountStatus.Active)
            {
                result.Reason = "User not found or inactive";
                result.ProcessingTime = stopwatch.Elapsed;
                return result;
            }

            // Step 2: Get user's effective permissions
            var userPermissions = await GetEffectivePermissionsAsync(context.UserId);
            var hasDirectPermission = userPermissions.Any(p => 
                p.Type == context.RequestedPermission && 
                p.ResourceType == context.ResourceType &&
                (p.ResourceId == null || p.ResourceId == context.ResourceId || p.Scope == "*"));

            if (hasDirectPermission)
            {
                // Check permission conditions
                var applicablePermission = userPermissions.First(p => 
                    p.Type == context.RequestedPermission && 
                    p.ResourceType == context.ResourceType &&
                    (p.ResourceId == null || p.ResourceId == context.ResourceId || p.Scope == "*"));

                var conditionsValid = await ValidatePermissionConditionsAsync(applicablePermission, context);
                if (conditionsValid)
                {
                    result.IsAuthorized = true;
                    result.Decision = AccessDecision.Allow;
                    result.Reason = "Direct permission granted";
                    result.ProcessingTime = stopwatch.Elapsed;
                    
                    // Cache successful result
                    CacheResult(cacheKey, result);
                    return result;
                }
            }

            // Step 3: Check resource-level permissions
            var resourcePermissions = await GetResourcePermissionsAsync(context.ResourceType.ToString(), context.ResourceId ?? "");
            var hasResourcePermission = resourcePermissions.Any(rp => 
                (rp.GranteeId == context.UserId && !rp.IsRole) ||
                (rp.IsRole && context.UserRoles.Contains(rp.GranteeId.ToString())) &&
                rp.GrantedPermissions.Contains(context.RequestedPermission) &&
                rp.IsActive &&
                (rp.ValidFrom == null || rp.ValidFrom <= DateTime.UtcNow) &&
                (rp.ValidUntil == null || rp.ValidUntil > DateTime.UtcNow));

            if (hasResourcePermission)
            {
                result.IsAuthorized = true;
                result.Decision = AccessDecision.Allow;
                result.Reason = "Resource-level permission granted";
                result.ProcessingTime = stopwatch.Elapsed;
                
                CacheResult(cacheKey, result);
                return result;
            }

            // Step 4: Evaluate authorization policies
            var applicablePolicies = await GetApplicablePoliciesAsync(context.ResourceType, context.ResourceId);
            var policyResults = new List<(AuthorizationPolicy Policy, AccessDecision Decision, string Reason)>();

            foreach (var policy in applicablePolicies.OrderByDescending(p => p.Priority))
            {
                var policyResult = await _policyEvaluator.EvaluatePolicyAsync(policy, context);
                policyResults.Add((policy, policyResult.Decision, policyResult.Reason));
                
                result.MatchedPolicies.Add(policy.Name);
                
                if (policyResult.Decision == AccessDecision.Allow)
                {
                    result.IsAuthorized = true;
                    result.Decision = AccessDecision.Allow;
                    result.Reason = $"Policy '{policy.Name}' granted access: {policyResult.Reason}";
                    break;
                }
                else if (policyResult.Decision == AccessDecision.Deny)
                {
                    result.Decision = AccessDecision.Deny;
                    result.Reason = $"Policy '{policy.Name}' denied access: {policyResult.Reason}";
                    // Continue checking other policies unless this is a blocking deny
                }
            }

            // Step 5: Check dynamic permissions
            var dynamicPermissions = await GetDynamicPermissionsAsync(context.ResourceType);
            foreach (var dynamicPermission in dynamicPermissions.Where(dp => dp.PermissionType == context.RequestedPermission))
            {
                var isGranted = await EvaluateDynamicPermissionAsync(dynamicPermission.Id, context);
                if (isGranted)
                {
                    result.IsAuthorized = true;
                    result.Decision = AccessDecision.Allow;
                    result.Reason = $"Dynamic permission '{dynamicPermission.Name}' granted access";
                    break;
                }
            }

            // Step 6: Default deny
            if (!result.IsAuthorized)
            {
                result.Decision = AccessDecision.Deny;
                if (string.IsNullOrEmpty(result.Reason))
                {
                    result.Reason = "No applicable permissions found";
                }
            }

            result.ProcessingTime = stopwatch.Elapsed;
            
            // Cache result
            CacheResult(cacheKey, result);
            
            _logger.LogDebug("Authorization result for user {UserId}: {Decision} - {Reason} (took {ProcessingTime}ms)",
                context.UserId, result.Decision, result.Reason, result.ProcessingTime.TotalMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authorization for user {UserId}", context.UserId);
            
            return new AuthorizationResult
            {
                IsAuthorized = false,
                Decision = AccessDecision.Deny,
                Reason = "Authorization error occurred",
                ProcessingTime = stopwatch.Elapsed
            };
        }
    }

    public async Task<AuthorizationResult> AuthorizeAsync(Guid userId, ResourceType resourceType, string? resourceId, PermissionType permission, Dictionary<ContextAttribute, object>? attributes = null)
    {
        var context = new AuthorizationContext
        {
            UserId = userId,
            ResourceType = resourceType,
            ResourceId = resourceId,
            RequestedPermission = permission,
            Attributes = attributes ?? new Dictionary<ContextAttribute, object>()
        };

        // Get user roles for context
        var userRoles = await GetUserRolesAsync(userId, false);
        context.UserRoles = userRoles.Select(r => r.Name).ToList();

        return await AuthorizeAsync(context);
    }

    public async Task<bool> HasPermissionAsync(Guid userId, string permission)
    {
        var cacheKey = $"user_permission_{userId}_{permission}";
        if (_cache.TryGetValue(cacheKey, out bool cachedResult))
        {
            return cachedResult;
        }

        var permissions = await GetEffectivePermissionsAsync(userId);
        var hasPermission = permissions.Any(p => p.Name == permission && p.IsActive);

        _cache.Set(cacheKey, hasPermission, TimeSpan.FromMinutes(15));
        return hasPermission;
    }

    public async Task<Role> CreateRoleAsync(CreateRoleRequest request)
    {
        // Validate role name is unique
        var existingRole = await GetRoleByNameAsync(request.Name);
        if (existingRole != null)
        {
            throw new InvalidOperationException($"Role '{request.Name}' already exists");
        }

        // Validate parent role exists if specified
        Role? parentRole = null;
        if (request.ParentRoleId.HasValue)
        {
            parentRole = await GetRoleAsync(request.ParentRoleId.Value);
            if (parentRole == null)
            {
                throw new InvalidOperationException("Parent role not found");
            }
        }

        var role = new Role
        {
            Name = request.Name,
            DisplayName = request.DisplayName,
            Description = request.Description,
            ParentRoleId = request.ParentRoleId,
            Level = parentRole?.Level + 1 ?? 0,
            CreatedAt = DateTime.UtcNow
        };

        // Get permissions
        if (request.PermissionIds.Any())
        {
            var permissions = await _permissionStore.GetPermissionsByIdsAsync(request.PermissionIds);
            role.DirectPermissions = permissions.ToList();
        }

        await _roleStore.CreateRoleAsync(role);
        
        // Invalidate related caches
        await InvalidateRoleCacheAsync(role.Id);
        
        _logger.LogInformation("Created role {RoleName} with ID {RoleId}", role.Name, role.Id);
        
        return role;
    }

    public async Task<List<Permission>> GetEffectivePermissionsAsync(Guid userId)
    {
        var cacheKey = $"effective_permissions_{userId}";
        if (_cache.TryGetValue(cacheKey, out List<Permission>? cachedPermissions))
        {
            return cachedPermissions!;
        }

        var permissions = new List<Permission>();

        // Get direct permissions
        var directPermissions = await _permissionStore.GetUserDirectPermissionsAsync(userId);
        permissions.AddRange(directPermissions);

        // Get role-based permissions
        var userRoles = await GetUserRolesAsync(userId, false);
        foreach (var role in userRoles)
        {
            var rolePermissions = await GetRolePermissionsAsync(role.Id, true);
            permissions.AddRange(rolePermissions);
        }

        // Get group-based permissions
        var userGroups = await GetUserGroupsAsync(userId);
        foreach (var group in userGroups)
        {
            permissions.AddRange(group.Permissions);
            
            foreach (var groupRole in group.Roles)
            {
                var groupRolePermissions = await GetRolePermissionsAsync(groupRole.Id, true);
                permissions.AddRange(groupRolePermissions);
            }
        }

        // Remove duplicates and inactive permissions
        var effectivePermissions = permissions
            .Where(p => p.IsActive)
            .GroupBy(p => new { p.Name, p.ResourceType, p.ResourceId })
            .Select(g => g.First())
            .ToList();

        _cache.Set(cacheKey, effectivePermissions, TimeSpan.FromMinutes(30));
        return effectivePermissions;
    }

    // Helper methods
    private string GenerateCacheKey(AuthorizationContext context)
    {
        return $"auth_{context.UserId}_{context.ResourceType}_{context.ResourceId}_{context.RequestedPermission}";
    }

    private void CacheResult(string cacheKey, AuthorizationResult result)
    {
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(15));
    }

    private async Task<bool> ValidatePermissionConditionsAsync(Permission permission, AuthorizationContext context)
    {
        foreach (var condition in permission.Conditions)
        {
            if (!context.Attributes.TryGetValue(condition.Attribute, out var value))
            {
                if (condition.IsRequired)
                    return false;
                continue;
            }

            var isValid = condition.Operator.ToLower() switch
            {
                "equals" => value?.ToString() == condition.Value,
                "not_equals" => value?.ToString() != condition.Value,
                "contains" => value?.ToString()?.Contains(condition.Value) ?? false,
                "in" => condition.Value.Split(',').Contains(value?.ToString()),
                _ => false
            };

            if (!isValid && condition.IsRequired)
                return false;
        }

        return true;
    }

    private async Task<SecureUser?> GetUserAsync(Guid userId)
    {
        // This would be injected as IUserService or similar
        // Placeholder for actual implementation
        return new SecureUser { Id = userId, Status = AccountStatus.Active };
    }

    // Placeholder implementations for interface methods
    public Task<bool> HasRoleAsync(Guid userId, string roleName) => throw new NotImplementedException();
    public Task<bool> IsInGroupAsync(Guid userId, string groupName) => throw new NotImplementedException();
    public Task<Role?> GetRoleAsync(Guid roleId) => throw new NotImplementedException();
    public Task<Role?> GetRoleByNameAsync(string roleName) => throw new NotImplementedException();
    public Task<List<Role>> GetRolesAsync() => throw new NotImplementedException();
    public Task<Role> UpdateRoleAsync(Guid roleId, UpdateRoleRequest request) => throw new NotImplementedException();
    public Task<bool> DeleteRoleAsync(Guid roleId) => throw new NotImplementedException();
    public Task<List<Permission>> GetRolePermissionsAsync(Guid roleId, bool includeInherited = true) => throw new NotImplementedException();
    public Task<List<Role>> GetUserRolesAsync(Guid userId, bool includeInactive = false) => throw new NotImplementedException();
    public Task<bool> AssignRoleAsync(AssignRoleRequest request) => throw new NotImplementedException();
    public Task<bool> RemoveRoleAsync(Guid userId, Guid roleId) => throw new NotImplementedException();
    public Task<List<UserRole>> GetUserRoleAssignmentsAsync(Guid userId) => throw new NotImplementedException();
    public Task<bool> IsRoleActiveAsync(Guid userId, Guid roleId, DateTime? atTime = null) => throw new NotImplementedException();
    public Task<Permission> CreatePermissionAsync(CreatePermissionRequest request) => throw new NotImplementedException();
    public Task<Permission?> GetPermissionAsync(Guid permissionId) => throw new NotImplementedException();
    public Task<Permission?> GetPermissionByNameAsync(string permissionName) => throw new NotImplementedException();
    public Task<List<Permission>> GetPermissionsAsync() => throw new NotImplementedException();
    public Task<Permission> UpdatePermissionAsync(Guid permissionId, Permission permission) => throw new NotImplementedException();
    public Task<bool> DeletePermissionAsync(Guid permissionId) => throw new NotImplementedException();
    public Task<List<Permission>> GetUserPermissionsAsync(Guid userId, bool includeInherited = true) => throw new NotImplementedException();
    public Task<AuthorizationPolicy> CreatePolicyAsync(CreatePolicyRequest request) => throw new NotImplementedException();
    public Task<AuthorizationPolicy?> GetPolicyAsync(Guid policyId) => throw new NotImplementedException();
    public Task<List<AuthorizationPolicy>> GetPoliciesAsync() => throw new NotImplementedException();
    public Task<AuthorizationPolicy> UpdatePolicyAsync(Guid policyId, AuthorizationPolicy policy) => throw new NotImplementedException();
    public Task<bool> DeletePolicyAsync(Guid policyId) => throw new NotImplementedException();
    public Task<List<AuthorizationPolicy>> GetApplicablePoliciesAsync(ResourceType resourceType, string? resourceId = null) => throw new NotImplementedException();
    public Task<Group> CreateGroupAsync(string name, string displayName, string description) => throw new NotImplementedException();
    public Task<Group?> GetGroupAsync(Guid groupId) => throw new NotImplementedException();
    public Task<List<Group>> GetGroupsAsync() => throw new NotImplementedException();
    public Task<bool> AddUserToGroupAsync(Guid groupId, Guid userId) => throw new NotImplementedException();
    public Task<bool> RemoveUserFromGroupAsync(Guid groupId, Guid userId) => throw new NotImplementedException();
    public Task<List<Group>> GetUserGroupsAsync(Guid userId) => throw new NotImplementedException();
    public Task<ResourceOwnership> CreateResourceOwnershipAsync(ResourceType resourceType, string resourceId, Guid ownerId) => throw new NotImplementedException();
    public Task<bool> GrantResourcePermissionAsync(string resourceType, string resourceId, Guid granteeId, bool isRole, List<PermissionType> permissions) => throw new NotImplementedException();
    public Task<bool> RevokeResourcePermissionAsync(string resourceType, string resourceId, Guid granteeId) => throw new NotImplementedException();
    public Task<List<ResourcePermissionGrant>> GetResourcePermissionsAsync(string resourceType, string resourceId) => throw new NotImplementedException();
    public Task<bool> IsResourceOwnerAsync(Guid userId, ResourceType resourceType, string resourceId) => throw new NotImplementedException();
    public Task<DynamicPermission> CreateDynamicPermissionAsync(string name, string expression, PermissionType permissionType, ResourceType resourceType) => throw new NotImplementedException();
    public Task<bool> EvaluateDynamicPermissionAsync(Guid dynamicPermissionId, AuthorizationContext context) => throw new NotImplementedException();
    public Task<List<DynamicPermission>> GetDynamicPermissionsAsync(ResourceType resourceType) => throw new NotImplementedException();
    public Task InvalidateUserCacheAsync(Guid userId) => throw new NotImplementedException();
    public Task InvalidateRoleCacheAsync(Guid roleId) => throw new NotImplementedException();
    public Task InvalidatePermissionCacheAsync(Guid permissionId) => throw new NotImplementedException();
    public Task InvalidateAllCacheAsync() => throw new NotImplementedException();
    public Task<bool> IsCachedAsync(string cacheKey) => throw new NotImplementedException();
    public Task<UserPermissionsResponse> GetUserPermissionMatrixAsync(Guid userId) => throw new NotImplementedException();
    public Task<AuthorizationMatrixResponse> GetAuthorizationMatrixAsync(List<Guid>? userIds = null) => throw new NotImplementedException();
    public Task<List<string>> GetUnusedPermissionsAsync() => throw new NotImplementedException();
    public Task<List<string>> GetOverPrivilegedUsersAsync() => throw new NotImplementedException();
    public Task<Dictionary<string, int>> GetPermissionUsageStatsAsync() => throw new NotImplementedException();
    public Task<bool> BulkAssignRolesAsync(List<Guid> userIds, List<Guid> roleIds) => throw new NotImplementedException();
    public Task<bool> BulkRemoveRolesAsync(List<Guid> userIds, List<Guid> roleIds) => throw new NotImplementedException();
    public Task<bool> BulkUpdatePermissionsAsync(Guid roleId, List<Guid> permissionIds) => throw new NotImplementedException();
    public Task<List<string>> DetectPermissionConflictsAsync() => throw new NotImplementedException();
    public Task<List<string>> DetectOrphanedPermissionsAsync() => throw new NotImplementedException();
    public Task<bool> ValidateRoleHierarchyAsync() => throw new NotImplementedException();
    public Task<Dictionary<string, object>> GetSecurityMetricsAsync() => throw new NotImplementedException();
}

/// <summary>
/// Supporting service interfaces (to be implemented)
/// </summary>
public interface IRoleStore
{
    Task<Role> CreateRoleAsync(Role role);
    Task<Role?> GetRoleAsync(Guid roleId);
    Task<Role?> GetRoleByNameAsync(string name);
    Task<List<Role>> GetRolesAsync();
    Task<bool> UpdateRoleAsync(Role role);
    Task<bool> DeleteRoleAsync(Guid roleId);
}

public interface IPermissionStore
{
    Task<Permission> CreatePermissionAsync(Permission permission);
    Task<Permission?> GetPermissionAsync(Guid permissionId);
    Task<List<Permission>> GetPermissionsAsync();
    Task<List<Permission>> GetPermissionsByIdsAsync(List<Guid> permissionIds);
    Task<List<Permission>> GetUserDirectPermissionsAsync(Guid userId);
}

public interface IPolicyStore
{
    Task<AuthorizationPolicy> CreatePolicyAsync(AuthorizationPolicy policy);
    Task<List<AuthorizationPolicy>> GetPoliciesAsync();
    Task<List<AuthorizationPolicy>> GetApplicablePoliciesAsync(ResourceType resourceType, string? resourceId);
}

public interface IGroupStore
{
    Task<Group> CreateGroupAsync(Group group);
    Task<List<Group>> GetUserGroupsAsync(Guid userId);
}

public interface IResourceStore
{
    Task<ResourceOwnership> CreateResourceOwnershipAsync(ResourceOwnership ownership);
    Task<List<ResourcePermissionGrant>> GetResourcePermissionsAsync(string resourceType, string resourceId);
}

public interface IPolicyEvaluator
{
    Task<(AccessDecision Decision, string Reason)> EvaluatePolicyAsync(AuthorizationPolicy policy, AuthorizationContext context);
}

public interface IExpressionEvaluator
{
    Task<bool> EvaluateAsync(string expression, AuthorizationContext context);
}