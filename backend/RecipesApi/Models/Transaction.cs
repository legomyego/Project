namespace RecipesApi.Models;

/// <summary>
/// Represents a financial transaction in the points economy
/// Tracks all point movements: purchases, sales, and top-ups
/// </summary>
public class Transaction
{
    /// <summary>
    /// Unique identifier for this transaction
    /// Using Guid for distributed transaction logging
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the user who performed this transaction
    /// Every transaction is associated with a user
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property: the user who performed this transaction
    /// EF Core automatically loads this when needed
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Amount of points involved in this transaction
    /// Positive for credits (top-ups, sales)
    /// Negative for debits (purchases)
    /// Using decimal for precise financial calculations
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Type of transaction - determines the business logic
    /// Purchase: User buys a recipe (debit)
    /// Sale: User sells/creates a recipe (credit)
    /// TopUp: User adds points to their account (credit)
    /// </summary>
    public TransactionType Type { get; set; }

    /// <summary>
    /// Optional reference to a recipe if this transaction involves one
    /// Used for Purchase and Sale types
    /// Null for TopUp transactions
    /// </summary>
    public Guid? RecipeId { get; set; }

    /// <summary>
    /// Navigation property: the recipe involved in this transaction
    /// Only populated for Purchase and Sale transactions
    /// </summary>
    public Recipe? Recipe { get; set; }

    /// <summary>
    /// When this transaction occurred
    /// Immutable - never updated after creation
    /// Used for transaction history and auditing
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Enum defining the types of transactions in the system
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// User purchased a recipe with points (debit)
    /// </summary>
    Purchase = 0,

    /// <summary>
    /// User received points from selling a recipe (credit)
    /// </summary>
    Sale = 1,

    /// <summary>
    /// User added points to their account (credit)
    /// </summary>
    TopUp = 2
}
