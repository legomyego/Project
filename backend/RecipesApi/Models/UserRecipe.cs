namespace RecipesApi.Models;

/// <summary>
/// Junction table for many-to-many relationship between Users and Recipes
/// Tracks which recipes a user owns (has purchased or received through trade)
/// </summary>
public class UserRecipe
{
    /// <summary>
    /// Foreign key to the user who owns this recipe
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property to the user
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Foreign key to the recipe that is owned
    /// </summary>
    public Guid RecipeId { get; set; }

    /// <summary>
    /// Navigation property to the recipe
    /// </summary>
    public Recipe? Recipe { get; set; }

    /// <summary>
    /// When this recipe was acquired by the user
    /// Either through purchase or trade
    /// </summary>
    public DateTime AcquiredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// How this recipe was acquired
    /// Purchase: bought with points
    /// Trade: received through recipe exchange
    /// </summary>
    public AcquisitionType AcquisitionType { get; set; }
}

/// <summary>
/// Enum defining how a recipe was acquired
/// </summary>
public enum AcquisitionType
{
    /// <summary>
    /// Recipe was purchased with points
    /// </summary>
    Purchase = 0,

    /// <summary>
    /// Recipe was received through a trade
    /// </summary>
    Trade = 1
}
