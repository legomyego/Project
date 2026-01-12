namespace RecipesApi.Models;

/// <summary>
/// Represents a trade offer between two users
/// User A offers their recipe in exchange for User B's recipe
/// </summary>
public class Trade
{
    /// <summary>
    /// Unique identifier for this trade
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User who initiated the trade (offering their recipe)
    /// </summary>
    public Guid OfferingUserId { get; set; }

    /// <summary>
    /// Navigation property to the user offering the trade
    /// </summary>
    public User? OfferingUser { get; set; }

    /// <summary>
    /// Recipe that the offering user wants to trade away
    /// </summary>
    public Guid OfferedRecipeId { get; set; }

    /// <summary>
    /// Navigation property to the offered recipe
    /// </summary>
    public Recipe? OfferedRecipe { get; set; }

    /// <summary>
    /// User who receives the trade offer (owns the requested recipe)
    /// </summary>
    public Guid RequestedUserId { get; set; }

    /// <summary>
    /// Navigation property to the user receiving the offer
    /// </summary>
    public User? RequestedUser { get; set; }

    /// <summary>
    /// Recipe that the offering user wants to receive
    /// </summary>
    public Guid RequestedRecipeId { get; set; }

    /// <summary>
    /// Navigation property to the requested recipe
    /// </summary>
    public Recipe? RequestedRecipe { get; set; }

    /// <summary>
    /// Current status of the trade
    /// Pending: waiting for response
    /// Accepted: both recipes exchanged
    /// Declined: offer was rejected
    /// Cancelled: offering user cancelled before acceptance
    /// </summary>
    public TradeStatus Status { get; set; } = TradeStatus.Pending;

    /// <summary>
    /// When this trade offer was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this trade was last updated (accepted/declined/cancelled)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Enum defining the status of a trade
/// </summary>
public enum TradeStatus
{
    /// <summary>
    /// Trade is pending, waiting for response
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Trade was accepted, recipes exchanged
    /// </summary>
    Accepted = 1,

    /// <summary>
    /// Trade was declined by the requested user
    /// </summary>
    Declined = 2,

    /// <summary>
    /// Trade was cancelled by the offering user
    /// </summary>
    Cancelled = 3
}
