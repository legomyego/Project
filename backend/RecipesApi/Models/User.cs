namespace RecipesApi.Models;

/// <summary>
/// Represents a user in the Recipe PWA system
/// Users can purchase points, buy recipes, and trade recipes with other users
/// </summary>
public class User
{
    /// <summary>
    /// Unique identifier for the user
    /// Using Guid instead of int for better security and distribution
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User's email address - used for login
    /// Must be unique across all users
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// User's display name (nickname/username)
    /// Shows in UI instead of email for better privacy
    /// Must be unique across all users
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// Hashed password using BCrypt or similar
    /// Never store plain text passwords - this contains the hash
    /// </summary>
    public required string PasswordHash { get; set; }

    /// <summary>
    /// Current points balance
    /// Users spend points to purchase recipes
    /// Decimal type ensures precise financial calculations
    /// </summary>
    public decimal Balance { get; set; } = 0;

    /// <summary>
    /// Whether this user has admin privileges
    /// Admins can access the admin panel and manage subscriptions, recipes, users
    /// Regular users cannot access admin features
    /// </summary>
    public bool IsAdmin { get; set; } = false;

    /// <summary>
    /// When this user account was created
    /// Useful for analytics and account age verification
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property: recipes created by this user
    /// EF Core will populate this automatically when needed
    /// </summary>
    public ICollection<Recipe> CreatedRecipes { get; set; } = new List<Recipe>();

    /// <summary>
    /// Navigation property: all transactions for this user
    /// Includes purchases, sales, and point top-ups
    /// </summary>
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    /// <summary>
    /// Navigation property: recipes owned by this user
    /// Represents the many-to-many relationship through UserRecipe junction table
    /// Includes recipes acquired via purchase or trade
    /// </summary>
    public ICollection<UserRecipe> OwnedRecipes { get; set; } = new List<UserRecipe>();
}
