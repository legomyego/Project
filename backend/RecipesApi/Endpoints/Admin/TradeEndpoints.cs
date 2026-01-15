using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RecipesApi.Data;
using RecipesApi.Models;

namespace RecipesApi.Endpoints.Admin;

/// <summary>
/// Admin endpoints for trade management
/// Handles admin-only trade operations like viewing all trades
/// </summary>
public static class TradeAdminEndpoints
{
    /// <summary>
    /// Registers admin trade endpoints with the application
    /// Called from Program.cs to add admin trade routes to the API
    /// </summary>
    public static void MapAdminTradeEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/admin-api/trades")
            .WithTags("Trades (Admin)")
            .RequireAuthorization(); // All admin endpoints require authentication

        // GET /admin-api/trades - Get all trades (admin only)
        group.MapGet("/", GetAllTradesAsync)
            .WithName("GetAllTrades")
            .WithSummary("Get all trades (Admin)")
            .WithDescription("Returns paginated list of all trades. Admin only.");
    }

    /// <summary>
    /// Get all trades with pagination and filtering
    /// Admin only - requires IsAdmin flag on user account
    /// </summary>
    private static async Task<IResult> GetAllTradesAsync(
        AppDbContext db,
        ClaimsPrincipal user,
        int page = 1,
        int pageSize = 20,
        string? status = null)
    {
        // Get current user ID from JWT claims
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        // Check if user is admin
        var currentUser = await db.Users.FindAsync(userId);
        if (currentUser == null || !currentUser.IsAdmin)
        {
            return Results.Forbid();
        }

        // Validate pagination parameters
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        // Build query with optional status filter
        var query = db.Trades
            .Include(t => t.OfferingUser)
            .Include(t => t.RequestedUser)
            .Include(t => t.OfferedRecipe)
            .Include(t => t.RequestedRecipe)
            .AsQueryable();

        // Apply status filter if provided
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (Enum.TryParse<TradeStatus>(status, true, out var tradeStatus))
            {
                query = query.Where(t => t.Status == tradeStatus);
            }
        }

        // Get total count
        var totalCount = await query.CountAsync();

        // Fetch trades with pagination
        var trades = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                id = t.Id,
                offeringUser = new
                {
                    id = t.OfferingUser!.Id,
                    username = t.OfferingUser.Username,
                    email = t.OfferingUser.Email
                },
                offeredRecipe = new
                {
                    id = t.OfferedRecipe!.Id,
                    title = t.OfferedRecipe.Title
                },
                requestedUser = new
                {
                    id = t.RequestedUser!.Id,
                    username = t.RequestedUser.Username,
                    email = t.RequestedUser.Email
                },
                requestedRecipe = new
                {
                    id = t.RequestedRecipe!.Id,
                    title = t.RequestedRecipe.Title
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
}
