using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipesApi.Data;
using RecipesApi.Models;

namespace RecipesApi.Endpoints.Portal;

/// <summary>
/// Extension class for registering trade-related endpoints
/// Handles recipe trading between users - offers, accepts, declines, cancels
/// </summary>
public static class TradeEndpoints
{
    /// <summary>
    /// Registers all trade endpoints with the application
    /// Called from Program.cs to add trade routes to the API
    /// </summary>
    public static void MapTradeEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/trades")
            .WithTags("Trades")
            .RequireAuthorization(); // All trade endpoints require authentication

        // POST /api/trades/offer - Create a new trade offer
        group.MapPost("/offer", CreateTradeOfferAsync)
            .WithName("CreateTradeOffer")
            .WithSummary("Create a trade offer")
            .WithDescription("Offers to trade one of your recipes for another user's recipe.");

        // GET /api/trades/incoming - Get trade offers received by current user
        group.MapGet("/incoming", GetIncomingTradesAsync)
            .WithName("GetIncomingTrades")
            .WithSummary("Get incoming trade offers")
            .WithDescription("Returns trade offers where you are the requested user (someone wants your recipe).");

        // GET /api/trades/outgoing - Get trade offers made by current user
        group.MapGet("/outgoing", GetOutgoingTradesAsync)
            .WithName("GetOutgoingTrades")
            .WithSummary("Get outgoing trade offers")
            .WithDescription("Returns trade offers you made to other users.");

        // POST /api/trades/{id}/accept - Accept a trade offer
        group.MapPost("/{id}/accept", AcceptTradeAsync)
            .WithName("AcceptTrade")
            .WithSummary("Accept a trade offer")
            .WithDescription("Accepts a trade offer, exchanging recipes between users.");

        // POST /api/trades/{id}/decline - Decline a trade offer
        group.MapPost("/{id}/decline", DeclineTradeAsync)
            .WithName("DeclineTrade")
            .WithSummary("Decline a trade offer")
            .WithDescription("Declines a trade offer.");

        // POST /api/trades/{id}/cancel - Cancel your own trade offer
        group.MapPost("/{id}/cancel", CancelTradeAsync)
            .WithName("CancelTrade")
            .WithSummary("Cancel your trade offer")
            .WithDescription("Cancels a trade offer you made.");
    }

    /// <summary>
    /// Create a new trade offer
    /// User offers one of their recipes in exchange for another user's recipe
    /// </summary>
    private static async Task<IResult> CreateTradeOfferAsync(
        [FromBody] CreateTradeRequest request,
        ClaimsPrincipal user,
        AppDbContext db)
    {
        // Get authenticated user ID from JWT claims
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        // Validate that offered and requested recipes are different
        if (request.OfferedRecipeId == request.RequestedRecipeId)
        {
            return Results.BadRequest(new { error = "Cannot trade a recipe for itself" });
        }

        // Validate that offering user is not the same as requested user
        if (userId == request.RequestedUserId)
        {
            return Results.BadRequest(new { error = "Cannot trade with yourself" });
        }

        // Check if offering user owns the offered recipe
        var ownsOfferedRecipe = await db.UserRecipes
            .AnyAsync(ur => ur.UserId == userId && ur.RecipeId == request.OfferedRecipeId);

        if (!ownsOfferedRecipe)
        {
            return Results.BadRequest(new { error = "You don't own the offered recipe" });
        }

        // Check if requested user owns the requested recipe
        var ownsRequestedRecipe = await db.UserRecipes
            .AnyAsync(ur => ur.UserId == request.RequestedUserId && ur.RecipeId == request.RequestedRecipeId);

        if (!ownsRequestedRecipe)
        {
            return Results.BadRequest(new { error = "The requested user doesn't own the requested recipe" });
        }

        // Check if offering user already owns the requested recipe
        var alreadyOwnsRequested = await db.UserRecipes
            .AnyAsync(ur => ur.UserId == userId && ur.RecipeId == request.RequestedRecipeId);

        if (alreadyOwnsRequested)
        {
            return Results.BadRequest(new { error = "You already own the requested recipe" });
        }

        // Check if there's already a pending trade between these recipes
        var existingTrade = await db.Trades
            .AnyAsync(t =>
                t.Status == TradeStatus.Pending &&
                t.OfferingUserId == userId &&
                t.RequestedUserId == request.RequestedUserId &&
                t.OfferedRecipeId == request.OfferedRecipeId &&
                t.RequestedRecipeId == request.RequestedRecipeId);

        if (existingTrade)
        {
            return Results.BadRequest(new { error = "You already have a pending trade offer for these recipes" });
        }

        // Create the trade offer
        var trade = new Trade
        {
            Id = Guid.NewGuid(),
            OfferingUserId = userId,
            OfferedRecipeId = request.OfferedRecipeId,
            RequestedUserId = request.RequestedUserId,
            RequestedRecipeId = request.RequestedRecipeId,
            Status = TradeStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        db.Trades.Add(trade);
        await db.SaveChangesAsync();

        // Load related data for response
        await db.Entry(trade).Reference(t => t.OfferedRecipe).LoadAsync();
        await db.Entry(trade).Reference(t => t.RequestedRecipe).LoadAsync();
        await db.Entry(trade).Reference(t => t.RequestedUser).LoadAsync();

        return Results.Ok(new
        {
            message = "Trade offer created successfully",
            trade = new
            {
                id = trade.Id,
                offeredRecipe = new
                {
                    id = trade.OfferedRecipe!.Id,
                    title = trade.OfferedRecipe.Title
                },
                requestedRecipe = new
                {
                    id = trade.RequestedRecipe!.Id,
                    title = trade.RequestedRecipe.Title
                },
                requestedUser = new
                {
                    id = trade.RequestedUser!.Id,
                    email = trade.RequestedUser.Email
                },
                status = trade.Status.ToString(),
                createdAt = trade.CreatedAt
            }
        });
    }

    /// <summary>
    /// Get incoming trade offers for the current user
    /// These are offers where someone wants to trade for your recipe
    /// </summary>
    private static async Task<IResult> GetIncomingTradesAsync(
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
        var totalCount = await db.Trades
            .Where(t => t.RequestedUserId == userId && t.Status == TradeStatus.Pending)
            .CountAsync();

        // Fetch trades with related data
        var trades = await db.Trades
            .Where(t => t.RequestedUserId == userId && t.Status == TradeStatus.Pending)
            .Include(t => t.OfferingUser)
            .Include(t => t.OfferedRecipe)
            .Include(t => t.RequestedRecipe)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                id = t.Id,
                offeringUser = new
                {
                    id = t.OfferingUser.Id,
                    email = t.OfferingUser.Email,
                    username = t.OfferingUser.Username
                },
                offeredRecipe = new
                {
                    id = t.OfferedRecipe.Id,
                    title = t.OfferedRecipe.Title,
                    price = t.OfferedRecipe.Price
                },
                requestedRecipe = new
                {
                    id = t.RequestedRecipe.Id,
                    title = t.RequestedRecipe.Title,
                    price = t.RequestedRecipe.Price
                },
                status = t.Status.ToString(),
                createdAt = t.CreatedAt
            })
            .ToListAsync();

        return Results.Ok(new
        {
            trades,
            pagination = new
            {
                currentPage = page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            }
        });
    }

    /// <summary>
    /// Get outgoing trade offers made by the current user
    /// These are offers where you want to trade for someone else's recipe
    /// </summary>
    private static async Task<IResult> GetOutgoingTradesAsync(
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

        // Get total count for pagination metadata (including all statuses for outgoing)
        var totalCount = await db.Trades
            .Where(t => t.OfferingUserId == userId)
            .CountAsync();

        // Fetch trades with related data
        var trades = await db.Trades
            .Where(t => t.OfferingUserId == userId)
            .Include(t => t.RequestedUser)
            .Include(t => t.OfferedRecipe)
            .Include(t => t.RequestedRecipe)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                id = t.Id,
                requestedUser = new
                {
                    id = t.RequestedUser.Id,
                    email = t.RequestedUser.Email,
                    username = t.RequestedUser.Username
                },
                offeredRecipe = new
                {
                    id = t.OfferedRecipe.Id,
                    title = t.OfferedRecipe.Title,
                    price = t.OfferedRecipe.Price
                },
                requestedRecipe = new
                {
                    id = t.RequestedRecipe.Id,
                    title = t.RequestedRecipe.Title,
                    price = t.RequestedRecipe.Price
                },
                status = t.Status.ToString(),
                createdAt = t.CreatedAt,
                updatedAt = t.UpdatedAt
            })
            .ToListAsync();

        return Results.Ok(new
        {
            trades,
            pagination = new
            {
                currentPage = page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            }
        });
    }

    /// <summary>
    /// Accept a trade offer
    /// Exchanges recipes between the two users atomically using a transaction
    /// </summary>
    private static async Task<IResult> AcceptTradeAsync(
        Guid id,
        ClaimsPrincipal user,
        AppDbContext db)
    {
        // Get authenticated user ID from JWT claims
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        // Use database transaction to ensure atomicity
        // Either both recipe transfers happen, or neither does
        await using var transaction = await db.Database.BeginTransactionAsync();

        try
        {
            // Find the trade offer
            var trade = await db.Trades
                .Include(t => t.OfferedRecipe)
                .Include(t => t.RequestedRecipe)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trade == null)
            {
                return Results.NotFound(new { error = "Trade offer not found" });
            }

            // Only the requested user can accept the trade
            if (trade.RequestedUserId != userId)
            {
                return Results.BadRequest(new { error = "You can only accept trades offered to you" });
            }

            // Check if trade is still pending
            if (trade.Status != TradeStatus.Pending)
            {
                return Results.BadRequest(new { error = $"Trade is already {trade.Status.ToString().ToLower()}" });
            }

            // Verify both users still own their respective recipes
            var offeringUserOwnsRecipe = await db.UserRecipes
                .AnyAsync(ur => ur.UserId == trade.OfferingUserId && ur.RecipeId == trade.OfferedRecipeId);

            var requestedUserOwnsRecipe = await db.UserRecipes
                .AnyAsync(ur => ur.UserId == trade.RequestedUserId && ur.RecipeId == trade.RequestedRecipeId);

            if (!offeringUserOwnsRecipe)
            {
                await transaction.RollbackAsync();
                return Results.BadRequest(new { error = "The offering user no longer owns their recipe" });
            }

            if (!requestedUserOwnsRecipe)
            {
                await transaction.RollbackAsync();
                return Results.BadRequest(new { error = "You no longer own the requested recipe" });
            }

            // Check if offering user already owns the requested recipe
            var offeringUserAlreadyOwnsRequested = await db.UserRecipes
                .AnyAsync(ur => ur.UserId == trade.OfferingUserId && ur.RecipeId == trade.RequestedRecipeId);

            if (offeringUserAlreadyOwnsRequested)
            {
                await transaction.RollbackAsync();
                return Results.BadRequest(new { error = "The offering user already owns the requested recipe" });
            }

            // Exchange recipes:
            // 1. Give offered recipe to requested user
            var offeredRecipeToRequestedUser = new UserRecipe
            {
                UserId = trade.RequestedUserId,
                RecipeId = trade.OfferedRecipeId,
                AcquisitionType = AcquisitionType.Trade,
                AcquiredAt = DateTime.UtcNow
            };

            // 2. Give requested recipe to offering user
            var requestedRecipeToOfferingUser = new UserRecipe
            {
                UserId = trade.OfferingUserId,
                RecipeId = trade.RequestedRecipeId,
                AcquisitionType = AcquisitionType.Trade,
                AcquiredAt = DateTime.UtcNow
            };

            // Add new ownership records
            db.UserRecipes.Add(offeredRecipeToRequestedUser);
            db.UserRecipes.Add(requestedRecipeToOfferingUser);

            // Update trade status
            trade.Status = TradeStatus.Accepted;
            trade.UpdatedAt = DateTime.UtcNow;

            // Save all changes
            await db.SaveChangesAsync();

            // Commit the transaction
            await transaction.CommitAsync();

            return Results.Ok(new
            {
                message = "Trade accepted successfully",
                trade = new
                {
                    id = trade.Id,
                    offeredRecipe = new
                    {
                        id = trade.OfferedRecipe.Id,
                        title = trade.OfferedRecipe.Title
                    },
                    requestedRecipe = new
                    {
                        id = trade.RequestedRecipe.Id,
                        title = trade.RequestedRecipe.Title
                    },
                    status = trade.Status.ToString(),
                    updatedAt = trade.UpdatedAt
                }
            });
        }
        catch (Exception ex)
        {
            // Rollback on any error
            await transaction.RollbackAsync();
            return Results.Problem(
                detail: ex.Message,
                statusCode: 500,
                title: "Trade acceptance failed"
            );
        }
    }

    /// <summary>
    /// Decline a trade offer
    /// Can only be done by the requested user
    /// </summary>
    private static async Task<IResult> DeclineTradeAsync(
        Guid id,
        ClaimsPrincipal user,
        AppDbContext db)
    {
        // Get authenticated user ID from JWT claims
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        // Find the trade offer
        var trade = await db.Trades.FindAsync(id);

        if (trade == null)
        {
            return Results.NotFound(new { error = "Trade offer not found" });
        }

        // Only the requested user can decline the trade
        if (trade.RequestedUserId != userId)
        {
            return Results.BadRequest(new { error = "You can only decline trades offered to you" });
        }

        // Check if trade is still pending
        if (trade.Status != TradeStatus.Pending)
        {
            return Results.BadRequest(new { error = $"Trade is already {trade.Status.ToString().ToLower()}" });
        }

        // Update trade status
        trade.Status = TradeStatus.Declined;
        trade.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return Results.Ok(new
        {
            message = "Trade declined",
            tradeId = trade.Id,
            status = trade.Status.ToString()
        });
    }

    /// <summary>
    /// Cancel a trade offer
    /// Can only be done by the offering user
    /// </summary>
    private static async Task<IResult> CancelTradeAsync(
        Guid id,
        ClaimsPrincipal user,
        AppDbContext db)
    {
        // Get authenticated user ID from JWT claims
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        // Find the trade offer
        var trade = await db.Trades.FindAsync(id);

        if (trade == null)
        {
            return Results.NotFound(new { error = "Trade offer not found" });
        }

        // Only the offering user can cancel the trade
        if (trade.OfferingUserId != userId)
        {
            return Results.BadRequest(new { error = "You can only cancel your own trade offers" });
        }

        // Check if trade is still pending
        if (trade.Status != TradeStatus.Pending)
        {
            return Results.BadRequest(new { error = $"Trade is already {trade.Status.ToString().ToLower()}" });
        }

        // Update trade status
        trade.Status = TradeStatus.Cancelled;
        trade.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return Results.Ok(new
        {
            message = "Trade cancelled",
            tradeId = trade.Id,
            status = trade.Status.ToString()
        });
    }
}

/// <summary>
/// Request model for creating a trade offer
/// </summary>
public record CreateTradeRequest(
    Guid OfferedRecipeId,   // Recipe you're offering (must own)
    Guid RequestedUserId,    // User you're offering to
    Guid RequestedRecipeId   // Recipe you want (they must own)
);
