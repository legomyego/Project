namespace RecipesApi.Models;

/// <summary>
/// Join table for many-to-many relationship between Subscriptions and Recipes
/// Links which recipes are included in which subscription plans
/// Example: "Weekly Premium" subscription includes Recipe A, B, and C
/// </summary>
public class SubscriptionRecipe
{
    /// <summary>
    /// Foreign key to the Subscription plan
    /// One subscription can include many recipes
    /// </summary>
    public Guid SubscriptionId { get; set; }

    /// <summary>
    /// Navigation property: the subscription plan
    /// Used to get subscription details (name, duration, price)
    /// </summary>
    public Subscription? Subscription { get; set; }

    /// <summary>
    /// Foreign key to the Recipe
    /// One recipe can be included in multiple subscription plans
    /// </summary>
    public Guid RecipeId { get; set; }

    /// <summary>
    /// Navigation property: the recipe details
    /// Used to get recipe title, description, author, etc.
    /// </summary>
    public Recipe? Recipe { get; set; }

    /// <summary>
    /// When this recipe was added to the subscription
    /// Used for tracking changes to subscription content
    /// </summary>
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
