using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipesApi.Data;
using RecipesApi.Models;

namespace RecipesApi.Endpoints;

/// <summary>
/// Extension class for registering points-related endpoints
/// Handles points top-up and transaction history
/// </summary>
public static class PointsEndpoints
{
    /// <summary>
    /// Registers all points endpoints with the application
    /// Called from Program.cs to add points routes to the API
    /// </summary>
    public static void MapPointsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/points")
            .WithTags("Points")
            .RequireAuthorization(); // All points endpoints require authentication

        // POST /api/points/topup - Add points to user's balance
        group.MapPost("/topup", TopUpPointsAsync)
            .WithName("TopUpPoints")
            .WithSummary("Add points to user balance")
            .WithDescription("Adds the specified amount of points to the authenticated user's balance.");

        // GET /api/transactions - Get user's transaction history
        group.MapGet("/transactions", GetTransactionsAsync)
            .WithName("GetTransactions")
            .WithSummary("Get transaction history")
            .WithDescription("Returns the authenticated user's transaction history with pagination.");
    }

    /// <summary>
    /// Top up user's points balance
    /// In a real app, this would integrate with a payment provider
    /// For now, it's a simple stub that adds points directly
    /// </summary>
    private static async Task<IResult> TopUpPointsAsync(
        [FromBody] TopUpRequest request,
        ClaimsPrincipal user,
        AppDbContext db)
    {
        // Get authenticated user ID from JWT claims
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        // Validate amount
        if (request.Amount <= 0)
        {
            return Results.BadRequest(new { error = "Amount must be greater than 0" });
        }

        // Find user in database
        var currentUser = await db.Users.FindAsync(userId);
        if (currentUser == null)
        {
            return Results.NotFound(new { error = "User not found" });
        }

        // Create transaction record
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Amount = request.Amount,
            Type = TransactionType.TopUp,
            RecipeId = null, // No recipe for top-up transactions
            CreatedAt = DateTime.UtcNow
        };

        // Add points to user balance
        currentUser.Balance += request.Amount;

        // Save transaction and updated balance
        db.Transactions.Add(transaction);
        await db.SaveChangesAsync();

        return Results.Ok(new
        {
            message = "Points added successfully",
            amount = request.Amount,
            newBalance = currentUser.Balance,
            transactionId = transaction.Id
        });
    }

    /// <summary>
    /// Get user's transaction history with pagination
    /// Returns all transactions (top-ups, purchases, sales)
    /// </summary>
    private static async Task<IResult> GetTransactionsAsync(
        ClaimsPrincipal user,
        AppDbContext db,
        int page = 1,
        int pageSize = 20)
    {
        // Get authenticated user ID from JWT claims
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        // Validate pagination parameters
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        // Get total count for pagination metadata
        var totalCount = await db.Transactions
            .Where(t => t.UserId == userId)
            .CountAsync();

        // Fetch transactions with related recipe data
        var transactions = await db.Transactions
            .Where(t => t.UserId == userId)
            .Include(t => t.Recipe) // Load related recipe if exists
            .OrderByDescending(t => t.CreatedAt) // Newest first
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                id = t.Id,
                amount = t.Amount,
                type = t.Type.ToString(),
                createdAt = t.CreatedAt,
                recipe = t.Recipe != null ? new
                {
                    id = t.Recipe.Id,
                    title = t.Recipe.Title,
                    price = t.Recipe.Price
                } : null
            })
            .ToListAsync();

        return Results.Ok(new
        {
            transactions,
            pagination = new
            {
                currentPage = page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            }
        });
    }
}

/// <summary>
/// Request model for points top-up
/// </summary>
public record TopUpRequest(decimal Amount);
