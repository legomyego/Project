namespace RecipesApi.Models;

/// <summary>
/// Represents a user's active or expired subscription
/// Tracks when the subscription was purchased and when it expires
/// Users can have multiple subscriptions (past and present)
/// </summary>
public class UserSubscription
{
    /// <summary>
    /// Unique identifier for this user subscription instance
    /// Using Guid for distributed system compatibility
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the User who owns this subscription
    /// One user can have multiple subscriptions over time
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property: the user who owns this subscription
    /// EF Core automatically loads this when needed
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Foreign key to the Subscription plan that was purchased
    /// References which subscription tier/duration was bought
    /// </summary>
    public Guid SubscriptionId { get; set; }

    /// <summary>
    /// Navigation property: the subscription plan details
    /// Used to get plan name, duration, etc.
    /// </summary>
    public Subscription? Subscription { get; set; }

    /// <summary>
    /// When the subscription became active
    /// Usually the purchase timestamp
    /// </summary>
    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the subscription expires
    /// Calculated as StartDate + Subscription.DurationDays
    /// After this date, IsActive should be false
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Whether this subscription is currently active
    /// True if current time is between StartDate and EndDate
    /// Can be manually disabled by admins if needed
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When this subscription was purchased/created
    /// Used for transaction history and analytics
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
