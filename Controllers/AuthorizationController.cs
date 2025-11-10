using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ADPA.Services.Security;
using AuthorizationResult = ADPA.Services.Security.AuthorizationResult;
using AuthorizationPolicy = ADPA.Services.Security.AuthorizationPolicy;
using IAuthorizationService = ADPA.Services.Security.IAuthorizationService;

namespace ADPA.Controllers;

/// <summary>
/// Phase 5: Advanced Authorization Controller
/// Comprehensive RBAC/ABAC management API with dynamic permissions and security analysis
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class AuthorizationController : ControllerBase
{
    private readonly IAuthorizationService _authService;
    private readonly ILogger<AuthorizationController> _logger;

    public AuthorizationController(
        IAuthorizationService authService,
        ILogger<AuthorizationController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Check if user has specific permission
    /// </summary>
    [HttpPost("check-permission")]
    public async Task<ActionResult<AuthorizationResult>> CheckPermission([FromBody] AuthorizeRequest request)
    {
        try
        {
            var result = await _authService.AuthorizeAsync(
                request.UserId, 
                request.ResourceType, 
                request.ResourceId, 
                request.Permission, 
                request.Context);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission for user {UserId}", request.UserId);
            return StatusCode(500, new { message = "Permission check failed" });
        }
    }

    /// <summary>
    /// Check if current user has specific permission
    /// </summary>
    [HttpGet("can/{permission}")]
    public async Task<ActionResult<bool>> CanPerform(string permission)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user" });
            }

            var hasPermission = await _authService.HasPermissionAsync(userId, permission);
            return Ok(hasPermission);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {Permission} for user {UserId}", 
                permission, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return StatusCode(500, new { message = "Permission check failed" });
        }
    }

    /// <summary>
    /// Get all roles
    /// </summary>
    [HttpGet("roles")]
    [Authorize(Policy = "CanManageRoles")]
    public async Task<ActionResult<List<Role>>> GetRoles()
    {
        try
        {
            var roles = await _authService.GetRolesAsync();
            return Ok(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles");
            return StatusCode(500, new { message = "Failed to get roles" });
        }
    }

    /// <summary>
    /// Get specific role
    /// </summary>
    [HttpGet("roles/{roleId}")]
    [Authorize(Policy = "CanViewRoles")]
    public async Task<ActionResult<RoleResponse>> GetRole(Guid roleId)
    {
        try
        {
            var role = await _authService.GetRoleAsync(roleId);
            if (role == null)
            {
                return NotFound(new { message = "Role not found" });
            }

            var permissions = await _authService.GetRolePermissionsAsync(roleId, true);
            
            return Ok(new RoleResponse
            {
                Role = role,
                EffectivePermissions = permissions
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting role {RoleId}", roleId);
            return StatusCode(500, new { message = "Failed to get role" });
        }
    }

    /// <summary>
    /// Create new role
    /// </summary>
    [HttpPost("roles")]
    [Authorize(Policy = "CanCreateRoles")]
    public async Task<ActionResult<Role>> CreateRole([FromBody] CreateRoleRequest request)
    {
        try
        {
            var role = await _authService.CreateRoleAsync(request);
            return CreatedAtAction(nameof(GetRole), new { roleId = role.Id }, role);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role {RoleName}", request.Name);
            return StatusCode(500, new { message = "Role creation failed" });
        }
    }

    /// <summary>
    /// Update existing role
    /// </summary>
    [HttpPut("roles/{roleId}")]
    [Authorize(Policy = "CanUpdateRoles")]
    public async Task<ActionResult<Role>> UpdateRole(Guid roleId, [FromBody] UpdateRoleRequest request)
    {
        try
        {
            var role = await _authService.UpdateRoleAsync(roleId, request);
            return Ok(role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role {RoleId}", roleId);
            return StatusCode(500, new { message = "Role update failed" });
        }
    }

    /// <summary>
    /// Delete role
    /// </summary>
    [HttpDelete("roles/{roleId}")]
    [Authorize(Policy = "CanDeleteRoles")]
    public async Task<ActionResult> DeleteRole(Guid roleId)
    {
        try
        {
            var success = await _authService.DeleteRoleAsync(roleId);
            if (!success)
            {
                return BadRequest(new { message = "Role deletion failed" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role {RoleId}", roleId);
            return StatusCode(500, new { message = "Role deletion failed" });
        }
    }

    /// <summary>
    /// Assign role to user
    /// </summary>
    [HttpPost("roles/assign")]
    [Authorize(Policy = "CanAssignRoles")]
    public async Task<ActionResult> AssignRole([FromBody] AssignRoleRequest request)
    {
        try
        {
            var success = await _authService.AssignRoleAsync(request);
            if (!success)
            {
                return BadRequest(new { message = "Role assignment failed" });
            }

            return Ok(new { message = "Role assigned successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role {RoleId} to user {UserId}", request.RoleId, request.UserId);
            return StatusCode(500, new { message = "Role assignment failed" });
        }
    }

    /// <summary>
    /// Remove role from user
    /// </summary>
    [HttpDelete("roles/{roleId}/users/{userId}")]
    [Authorize(Policy = "CanAssignRoles")]
    public async Task<ActionResult> RemoveRole(Guid roleId, Guid userId)
    {
        try
        {
            var success = await _authService.RemoveRoleAsync(userId, roleId);
            if (!success)
            {
                return BadRequest(new { message = "Role removal failed" });
            }

            return Ok(new { message = "Role removed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role {RoleId} from user {UserId}", roleId, userId);
            return StatusCode(500, new { message = "Role removal failed" });
        }
    }

    /// <summary>
    /// Get user's roles
    /// </summary>
    [HttpGet("users/{userId}/roles")]
    [Authorize(Policy = "CanViewUserRoles")]
    public async Task<ActionResult<List<Role>>> GetUserRoles(Guid userId)
    {
        try
        {
            var roles = await _authService.GetUserRolesAsync(userId);
            return Ok(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles for user {UserId}", userId);
            return StatusCode(500, new { message = "Failed to get user roles" });
        }
    }

    /// <summary>
    /// Get current user's roles
    /// </summary>
    [HttpGet("my-roles")]
    public async Task<ActionResult<List<Role>>> GetMyRoles()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user" });
            }

            var roles = await _authService.GetUserRolesAsync(userId);
            return Ok(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles for current user");
            return StatusCode(500, new { message = "Failed to get user roles" });
        }
    }

    /// <summary>
    /// Get all permissions
    /// </summary>
    [HttpGet("permissions")]
    [Authorize(Policy = "CanManagePermissions")]
    public async Task<ActionResult<List<Permission>>> GetPermissions()
    {
        try
        {
            var permissions = await _authService.GetPermissionsAsync();
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions");
            return StatusCode(500, new { message = "Failed to get permissions" });
        }
    }

    /// <summary>
    /// Create new permission
    /// </summary>
    [HttpPost("permissions")]
    [Authorize(Policy = "CanCreatePermissions")]
    public async Task<ActionResult<Permission>> CreatePermission([FromBody] CreatePermissionRequest request)
    {
        try
        {
            var permission = await _authService.CreatePermissionAsync(request);
            return CreatedAtAction(nameof(GetPermission), new { permissionId = permission.Id }, permission);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating permission {PermissionName}", request.Name);
            return StatusCode(500, new { message = "Permission creation failed" });
        }
    }

    /// <summary>
    /// Get specific permission
    /// </summary>
    [HttpGet("permissions/{permissionId}")]
    [Authorize(Policy = "CanViewPermissions")]
    public async Task<ActionResult<Permission>> GetPermission(Guid permissionId)
    {
        try
        {
            var permission = await _authService.GetPermissionAsync(permissionId);
            if (permission == null)
            {
                return NotFound(new { message = "Permission not found" });
            }

            return Ok(permission);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permission {PermissionId}", permissionId);
            return StatusCode(500, new { message = "Failed to get permission" });
        }
    }

    /// <summary>
    /// Get user's effective permissions
    /// </summary>
    [HttpGet("users/{userId}/permissions")]
    [Authorize(Policy = "CanViewUserPermissions")]
    public async Task<ActionResult<UserPermissionsResponse>> GetUserPermissions(Guid userId)
    {
        try
        {
            var response = await _authService.GetUserPermissionMatrixAsync(userId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions for user {UserId}", userId);
            return StatusCode(500, new { message = "Failed to get user permissions" });
        }
    }

    /// <summary>
    /// Get current user's effective permissions
    /// </summary>
    [HttpGet("my-permissions")]
    public async Task<ActionResult<UserPermissionsResponse>> GetMyPermissions()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user" });
            }

            var response = await _authService.GetUserPermissionMatrixAsync(userId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions for current user");
            return StatusCode(500, new { message = "Failed to get user permissions" });
        }
    }

    /// <summary>
    /// Get authorization policies
    /// </summary>
    [HttpGet("policies")]
    [Authorize(Policy = "CanManagePolicies")]
    public async Task<ActionResult<List<AuthorizationPolicy>>> GetPolicies()
    {
        try
        {
            var policies = await _authService.GetPoliciesAsync();
            return Ok(policies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting authorization policies");
            return StatusCode(500, new { message = "Failed to get policies" });
        }
    }

    /// <summary>
    /// Create authorization policy
    /// </summary>
    [HttpPost("policies")]
    [Authorize(Policy = "CanCreatePolicies")]
    public async Task<ActionResult<AuthorizationPolicy>> CreatePolicy([FromBody] CreatePolicyRequest request)
    {
        try
        {
            var policy = await _authService.CreatePolicyAsync(request);
            return CreatedAtAction(nameof(GetPolicy), new { policyId = policy.Id }, policy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating authorization policy {PolicyName}", request.Name);
            return StatusCode(500, new { message = "Policy creation failed" });
        }
    }

    /// <summary>
    /// Get specific authorization policy
    /// </summary>
    [HttpGet("policies/{policyId}")]
    [Authorize(Policy = "CanViewPolicies")]
    public async Task<ActionResult<AuthorizationPolicy>> GetPolicy(Guid policyId)
    {
        try
        {
            var policy = await _authService.GetPolicyAsync(policyId);
            if (policy == null)
            {
                return NotFound(new { message = "Policy not found" });
            }

            return Ok(policy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting authorization policy {PolicyId}", policyId);
            return StatusCode(500, new { message = "Failed to get policy" });
        }
    }

    /// <summary>
    /// Get groups
    /// </summary>
    [HttpGet("groups")]
    [Authorize(Policy = "CanManageGroups")]
    public async Task<ActionResult<List<Group>>> GetGroups()
    {
        try
        {
            var groups = await _authService.GetGroupsAsync();
            return Ok(groups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting groups");
            return StatusCode(500, new { message = "Failed to get groups" });
        }
    }

    /// <summary>
    /// Create group
    /// </summary>
    [HttpPost("groups")]
    [Authorize(Policy = "CanCreateGroups")]
    public async Task<ActionResult<Group>> CreateGroup([FromBody] CreateGroupRequest request)
    {
        try
        {
            var group = await _authService.CreateGroupAsync(request.Name, request.DisplayName, request.Description);
            return CreatedAtAction(nameof(GetGroup), new { groupId = group.Id }, group);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating group {GroupName}", request.Name);
            return StatusCode(500, new { message = "Group creation failed" });
        }
    }

    /// <summary>
    /// Get specific group
    /// </summary>
    [HttpGet("groups/{groupId}")]
    [Authorize(Policy = "CanViewGroups")]
    public async Task<ActionResult<Group>> GetGroup(Guid groupId)
    {
        try
        {
            var group = await _authService.GetGroupAsync(groupId);
            if (group == null)
            {
                return NotFound(new { message = "Group not found" });
            }

            return Ok(group);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group {GroupId}", groupId);
            return StatusCode(500, new { message = "Failed to get group" });
        }
    }

    /// <summary>
    /// Add user to group
    /// </summary>
    [HttpPost("groups/{groupId}/members/{userId}")]
    [Authorize(Policy = "CanManageGroupMembers")]
    public async Task<ActionResult> AddUserToGroup(Guid groupId, Guid userId)
    {
        try
        {
            var success = await _authService.AddUserToGroupAsync(groupId, userId);
            if (!success)
            {
                return BadRequest(new { message = "Failed to add user to group" });
            }

            return Ok(new { message = "User added to group successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user {UserId} to group {GroupId}", userId, groupId);
            return StatusCode(500, new { message = "Failed to add user to group" });
        }
    }

    /// <summary>
    /// Remove user from group
    /// </summary>
    [HttpDelete("groups/{groupId}/members/{userId}")]
    [Authorize(Policy = "CanManageGroupMembers")]
    public async Task<ActionResult> RemoveUserFromGroup(Guid groupId, Guid userId)
    {
        try
        {
            var success = await _authService.RemoveUserFromGroupAsync(groupId, userId);
            if (!success)
            {
                return BadRequest(new { message = "Failed to remove user from group" });
            }

            return Ok(new { message = "User removed from group successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user {UserId} from group {GroupId}", userId, groupId);
            return StatusCode(500, new { message = "Failed to remove user from group" });
        }
    }

    /// <summary>
    /// Get authorization matrix for users
    /// </summary>
    [HttpGet("matrix")]
    [Authorize(Policy = "CanViewAuthorizationMatrix")]
    public async Task<ActionResult<AuthorizationMatrixResponse>> GetAuthorizationMatrix([FromQuery] List<Guid>? userIds = null)
    {
        try
        {
            var matrix = await _authService.GetAuthorizationMatrixAsync(userIds);
            return Ok(matrix);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting authorization matrix");
            return StatusCode(500, new { message = "Failed to get authorization matrix" });
        }
    }

    /// <summary>
    /// Get security metrics and analytics
    /// </summary>
    [HttpGet("metrics")]
    [Authorize(Policy = "CanViewSecurityMetrics")]
    public async Task<ActionResult<Dictionary<string, object>>> GetSecurityMetrics()
    {
        try
        {
            var metrics = await _authService.GetSecurityMetricsAsync();
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting security metrics");
            return StatusCode(500, new { message = "Failed to get security metrics" });
        }
    }

    /// <summary>
    /// Detect permission conflicts
    /// </summary>
    [HttpGet("analysis/conflicts")]
    [Authorize(Policy = "CanAnalyzeSecurity")]
    public async Task<ActionResult<List<string>>> DetectPermissionConflicts()
    {
        try
        {
            var conflicts = await _authService.DetectPermissionConflictsAsync();
            return Ok(conflicts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting permission conflicts");
            return StatusCode(500, new { message = "Failed to detect conflicts" });
        }
    }

    /// <summary>
    /// Get unused permissions
    /// </summary>
    [HttpGet("analysis/unused-permissions")]
    [Authorize(Policy = "CanAnalyzeSecurity")]
    public async Task<ActionResult<List<string>>> GetUnusedPermissions()
    {
        try
        {
            var unused = await _authService.GetUnusedPermissionsAsync();
            return Ok(unused);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unused permissions");
            return StatusCode(500, new { message = "Failed to get unused permissions" });
        }
    }

    /// <summary>
    /// Get over-privileged users
    /// </summary>
    [HttpGet("analysis/over-privileged")]
    [Authorize(Policy = "CanAnalyzeSecurity")]
    public async Task<ActionResult<List<string>>> GetOverPrivilegedUsers()
    {
        try
        {
            var users = await _authService.GetOverPrivilegedUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting over-privileged users");
            return StatusCode(500, new { message = "Failed to get over-privileged users" });
        }
    }

    /// <summary>
    /// Bulk assign roles to users
    /// </summary>
    [HttpPost("bulk/assign-roles")]
    [Authorize(Policy = "CanBulkAssignRoles")]
    public async Task<ActionResult> BulkAssignRoles([FromBody] BulkRoleAssignmentRequest request)
    {
        try
        {
            var success = await _authService.BulkAssignRolesAsync(request.UserIds, request.RoleIds);
            if (!success)
            {
                return BadRequest(new { message = "Bulk role assignment failed" });
            }

            return Ok(new { message = "Roles assigned successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk role assignment");
            return StatusCode(500, new { message = "Bulk role assignment failed" });
        }
    }

    /// <summary>
    /// Invalidate user authorization cache
    /// </summary>
    [HttpPost("cache/invalidate/user/{userId}")]
    [Authorize(Policy = "CanManageCache")]
    public async Task<ActionResult> InvalidateUserCache(Guid userId)
    {
        try
        {
            await _authService.InvalidateUserCacheAsync(userId);
            return Ok(new { message = "User cache invalidated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for user {UserId}", userId);
            return StatusCode(500, new { message = "Cache invalidation failed" });
        }
    }

    /// <summary>
    /// Invalidate all authorization cache
    /// </summary>
    [HttpPost("cache/invalidate/all")]
    [Authorize(Policy = "CanManageCache")]
    public async Task<ActionResult> InvalidateAllCache()
    {
        try
        {
            await _authService.InvalidateAllCacheAsync();
            return Ok(new { message = "All cache invalidated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating all cache");
            return StatusCode(500, new { message = "Cache invalidation failed" });
        }
    }
}

/// <summary>
/// Supporting request models
/// </summary>
public class CreateGroupRequest
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class AuthorizeRequest
{
    public Guid UserId { get; set; }
    public ResourceType ResourceType { get; set; }
    public string? ResourceId { get; set; }
    public PermissionType Permission { get; set; }
    public Dictionary<ContextAttribute, object>? Context { get; set; }
}

public class BulkRoleAssignmentRequest
{
    public List<Guid> UserIds { get; set; } = new();
    public List<Guid> RoleIds { get; set; } = new();
}