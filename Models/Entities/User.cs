using System.ComponentModel.DataAnnotations;

namespace ADPA.Models.Entities;

/// <summary>
/// User entity for authentication and authorization
/// </summary>
public class User
{
    /// <summary>
    /// Unique identifier for the user
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// User's email address (used for login)
    /// </summary>
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hashed password for authentication
    /// </summary>
    [Required]
    [StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// User's role for authorization (Admin, User, Viewer)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Role { get; set; } = "User";

    /// <summary>
    /// User's display name
    /// </summary>
    [StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Whether the user account is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When the user account was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the user account was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property for documents uploaded by this user
    /// </summary>
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}