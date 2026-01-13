namespace RecipesApi.Models;

/// <summary>
/// Represents a recipe that can be purchased with points
/// Recipes are created by users and can be traded between users
/// </summary>
public class Recipe
{
    /// <summary>
    /// Unique identifier for the recipe
    /// Using Guid for distributed system compatibility
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Recipe title/name
    /// Examples: "Chocolate Chip Cookies", "Spaghetti Carbonara"
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Full recipe description including ingredients and instructions
    /// Can be markdown formatted for rich text display
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Price in points to purchase this recipe
    /// Set by the recipe author when creating the recipe
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Foreign key to the User who created this recipe
    /// Used to attribute authorship and potentially share revenue
    /// </summary>
    public Guid AuthorId { get; set; }

    /// <summary>
    /// Navigation property: the user who created this recipe
    /// EF Core automatically loads this when needed
    /// </summary>
    public User? Author { get; set; }

    /// <summary>
    /// Number of times this recipe has been viewed
    /// Used for popularity ranking and trending recipes
    /// </summary>
    public int Views { get; set; } = 0;

    /// <summary>
    /// When this recipe was created
    /// Used for sorting by "newest" and analytics
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether this recipe requires an active subscription to view
    /// If true, users must have an active subscription that includes this recipe
    /// If false, recipe can be purchased individually with points
    /// </summary>
    public bool RequiresSubscription { get; set; } = false;

    /// <summary>
    /// Navigation property: subscriptions that include this recipe
    /// Many-to-many relationship via SubscriptionRecipe join table
    /// </summary>
    public ICollection<SubscriptionRecipe> SubscriptionRecipes { get; set; } = new List<SubscriptionRecipe>();
}
