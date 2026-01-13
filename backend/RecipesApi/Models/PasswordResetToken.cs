namespace RecipesApi.Models;

/// <summary>
/// Password reset token for forgot password flow
/// Stores token, expiration, and whether it has been used
/// One-time use tokens with 1 hour expiration
/// </summary>
public class PasswordResetToken
{
    /// <summary>
    /// Unique identifier for the token record
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User ID this token belongs to
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The actual reset token (hashed for security)
    /// This is what gets sent in the email link
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// When this token expires
    /// Typically 1 hour from creation
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Whether this token has been used
    /// Prevents token reuse attacks
    /// </summary>
    public bool IsUsed { get; set; } = false;

    /// <summary>
    /// When this token was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Navigation property to User
    /// </summary>
    public User? User { get; set; }
}
