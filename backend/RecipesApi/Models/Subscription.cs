namespace RecipesApi.Models;

/// <summary>
/// Represents a subscription plan that users can purchase
/// Examples: "1 Day Pass", "3 Day Pass", "Weekly Access"
/// Subscriptions grant access to specific recipes
/// </summary>
public class Subscription
{
    /// <summary>
    /// Unique identifier for the subscription plan
    /// Using Guid for distributed system compatibility
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Display name of the subscription
    /// Examples: "Daily Pass", "3-Day Access", "Weekly Premium"
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Detailed description of what's included in this subscription
    /// Examples: "Access to all premium recipes for 7 days"
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Duration of the subscription in days
    /// Examples: 1 (day), 3 (3 days), 7 (week), 30 (month)
    /// </summary>
    public int DurationDays { get; set; }

    /// <summary>
    /// Price in points to purchase this subscription
    /// Users pay with their point balance
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Whether this subscription plan is currently available for purchase
    /// Admins can disable subscriptions without deleting them
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When this subscription plan was created
    /// Used for tracking and analytics
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property: recipes included in this subscription
    /// Many-to-many relationship via SubscriptionRecipe join table
    /// </summary>
    public ICollection<SubscriptionRecipe> SubscriptionRecipes { get; set; } = new List<SubscriptionRecipe>();

    /// <summary>
    /// Navigation property: users who have purchased this subscription
    /// One subscription plan can have many user subscriptions
    /// </summary>
    public ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
}
