using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipesApi.Data;
using RecipesApi.Models;

namespace RecipesApi.Endpoints;

/// <summary>
/// Subscription-related API endpoints
/// Handles CRUD operations for subscription plans (admin)
/// and subscription purchasing for users
/// </summary>
public static class SubscriptionEndpoints
{
    /// <summary>
    /// Register all subscription-related endpoints
    /// Groups endpoints under /api/subscriptions
    /// </summary>
    public static void MapSubscriptionEndpoints(this IEndpointRouteBuilder app)
    {
        // Create a route group for all subscription endpoints
        // This prefixes all routes with /api/subscriptions
        var group = app.MapGroup("/api/subscriptions")
            .WithTags("Subscriptions");

        // GET /api/subscriptions - Get all active subscription plans (public)
        group.MapGet("/", GetAllSubscriptionsAsync)
            .WithName("GetAllSubscriptions")
            .WithSummary("Get all active subscriptions")
            .WithDescription("Returns all active subscription plans available for purchase");

        // GET /api/subscriptions/{id} - Get subscription by ID with recipes (public)
        group.MapGet("/{id:guid}", GetSubscriptionByIdAsync)
            .WithName("GetSubscriptionById")
            .WithSummary("Get subscription by ID")
            .WithDescription("Returns detailed subscription information including included recipes");

        // POST /api/subscriptions - Create new subscription (admin only)
        group.MapPost("/", CreateSubscriptionAsync)
            .WithName("CreateSubscription")
            .WithSummary("Create a new subscription plan")
            .WithDescription("Admin endpoint to create a new subscription plan")
            .RequireAuthorization();

        // PUT /api/subscriptions/{id} - Update subscription (admin only)
        group.MapPut("/{id:guid}", UpdateSubscriptionAsync)
            .WithName("UpdateSubscription")
            .WithSummary("Update subscription plan")
            .WithDescription("Admin endpoint to update subscription details")
            .RequireAuthorization();

        // DELETE /api/subscriptions/{id} - Soft delete subscription (admin only)
        group.MapDelete("/{id:guid}", DeleteSubscriptionAsync)
            .WithName("DeleteSubscription")
            .WithSummary("Delete subscription plan")
            .WithDescription("Admin endpoint to deactivate a subscription (soft delete)")
            .RequireAuthorization();

        // POST /api/subscriptions/{id}/recipes - Add recipes to subscription (admin only)
        group.MapPost("/{id:guid}/recipes", AddRecipesToSubscriptionAsync)
            .WithName("AddRecipesToSubscription")
            .WithSummary("Add recipes to subscription")
            .WithDescription("Admin endpoint to add recipes to a subscription plan")
            .RequireAuthorization();

        // DELETE /api/subscriptions/{id}/recipes/{recipeId} - Remove recipe from subscription (admin only)
        group.MapDelete("/{id:guid}/recipes/{recipeId:guid}", RemoveRecipeFromSubscriptionAsync)
            .WithName("RemoveRecipeFromSubscription")
            .WithSummary("Remove recipe from subscription")
            .WithDescription("Admin endpoint to remove a recipe from a subscription plan")
            .RequireAuthorization();

        // POST /api/subscriptions/{id}/buy - Purchase subscription (authenticated users)
        group.MapPost("/{id:guid}/buy", BuySubscriptionAsync)
            .WithName("BuySubscription")
            .WithSummary("Purchase a subscription")
            .WithDescription("User endpoint to purchase a subscription with points")
            .RequireAuthorization();

        // GET /api/subscriptions/my - Get user's active subscriptions (authenticated users)
        group.MapGet("/my", GetMySubscriptionsAsync)
            .WithName("GetMySubscriptions")
            .WithSummary("Get my active subscriptions")
            .WithDescription("Returns the current user's active subscriptions")
            .RequireAuthorization();
    }

    /// <summary>
    /// GET /api/subscriptions
    /// Get all active subscription plans
    /// Public endpoint - no authentication required
    /// </summary>
    private static async Task<IResult> GetAllSubscriptionsAsync(AppDbContext db)
    {
        // Fetch all active subscriptions
        // Include the count of recipes in each subscription
        var subscriptions = await db.Subscriptions
            .Where(s => s.IsActive)
            .Select(s => new
            {
                id = s.Id,
                name = s.Name,
                description = s.Description,
                durationDays = s.DurationDays,
                price = s.Price,
                isActive = s.IsActive,
                recipeCount = s.SubscriptionRecipes.Count,
                createdAt = s.CreatedAt
            })
            .OrderBy(s => s.durationDays)
            .ToListAsync();

        return Results.Ok(subscriptions);
    }

    /// <summary>
    /// GET /api/subscriptions/{id}
    /// Get subscription by ID with included recipes
    /// Public endpoint - no authentication required
    /// </summary>
    private static async Task<IResult> GetSubscriptionByIdAsync(Guid id, AppDbContext db)
    {
        // Fetch subscription with all included recipes
        var subscription = await db.Subscriptions
            .Where(s => s.Id == id)
            .Select(s => new
            {
                id = s.Id,
                name = s.Name,
                description = s.Description,
                durationDays = s.DurationDays,
                price = s.Price,
                isActive = s.IsActive,
                createdAt = s.CreatedAt,
                recipes = s.SubscriptionRecipes.Select(sr => new
                {
                    id = sr.Recipe!.Id,
                    title = sr.Recipe.Title,
                    description = sr.Recipe.Description,
                    views = sr.Recipe.Views,
                    addedAt = sr.AddedAt
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (subscription == null)
        {
            return Results.NotFound(new { error = "Subscription not found" });
        }

        return Results.Ok(subscription);
    }

    /// <summary>
    /// POST /api/subscriptions
    /// Create a new subscription plan
    /// Admin only - requires authentication
    /// </summary>
    private static async Task<IResult> CreateSubscriptionAsync(
        CreateSubscriptionRequest request,
        ClaimsPrincipal user,
        AppDbContext db)
    {
        // Validate authentication
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
        {
            return Results.Unauthorized();
        }

        // Validate input
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.BadRequest(new { error = "Name is required" });
        }

        if (request.DurationDays <= 0)
        {
            return Results.BadRequest(new { error = "Duration must be positive" });
        }

        if (request.Price < 0)
        {
            return Results.BadRequest(new { error = "Price cannot be negative" });
        }

        // Create new subscription
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            DurationDays = request.DurationDays,
            Price = request.Price,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Add to database
        db.Subscriptions.Add(subscription);
        await db.SaveChangesAsync();

        // Return created subscription
        return Results.Created($"/api/subscriptions/{subscription.Id}", new
        {
            id = subscription.Id,
            name = subscription.Name,
            description = subscription.Description,
            durationDays = subscription.DurationDays,
            price = subscription.Price,
            isActive = subscription.IsActive,
            createdAt = subscription.CreatedAt
        });
    }

    /// <summary>
    /// PUT /api/subscriptions/{id}
    /// Update subscription plan
    /// Admin only - requires authentication
    /// </summary>
    private static async Task<IResult> UpdateSubscriptionAsync(
        Guid id,
        UpdateSubscriptionRequest request,
        ClaimsPrincipal user,
        AppDbContext db)
    {
        // Validate authentication
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
        {
            return Results.Unauthorized();
        }

        // Find subscription
        var subscription = await db.Subscriptions.FindAsync(id);
        if (subscription == null)
        {
            return Results.NotFound(new { error = "Subscription not found" });
        }

        // Validate input
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.BadRequest(new { error = "Name is required" });
        }

        if (request.DurationDays <= 0)
        {
            return Results.BadRequest(new { error = "Duration must be positive" });
        }

        if (request.Price < 0)
        {
            return Results.BadRequest(new { error = "Price cannot be negative" });
        }

        // Update subscription
        subscription.Name = request.Name.Trim();
        subscription.Description = request.Description?.Trim();
        subscription.DurationDays = request.DurationDays;
        subscription.Price = request.Price;
        subscription.IsActive = request.IsActive;

        await db.SaveChangesAsync();

        return Results.Ok(new
        {
            id = subscription.Id,
            name = subscription.Name,
            description = subscription.Description,
            durationDays = subscription.DurationDays,
            price = subscription.Price,
            isActive = subscription.IsActive,
            createdAt = subscription.CreatedAt
        });
    }

    /// <summary>
    /// DELETE /api/subscriptions/{id}
    /// Soft delete subscription (set IsActive = false)
    /// Admin only - requires authentication
    /// </summary>
    private static async Task<IResult> DeleteSubscriptionAsync(
        Guid id,
        ClaimsPrincipal user,
        AppDbContext db)
    {
        // Validate authentication
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
        {
            return Results.Unauthorized();
        }

        // Find subscription
        var subscription = await db.Subscriptions.FindAsync(id);
        if (subscription == null)
        {
            return Results.NotFound(new { error = "Subscription not found" });
        }

        // Soft delete - just deactivate
        subscription.IsActive = false;
        await db.SaveChangesAsync();

        return Results.Ok(new { message = "Subscription deactivated successfully" });
    }

    /// <summary>
    /// POST /api/subscriptions/{id}/recipes
    /// Add recipes to a subscription plan
    /// Admin only - requires authentication
    /// </summary>
    private static async Task<IResult> AddRecipesToSubscriptionAsync(
        Guid id,
        AddRecipesRequest request,
        ClaimsPrincipal user,
        AppDbContext db)
    {
        // Validate authentication
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
        {
            return Results.Unauthorized();
        }

        // Find subscription
        var subscription = await db.Subscriptions.FindAsync(id);
        if (subscription == null)
        {
            return Results.NotFound(new { error = "Subscription not found" });
        }

        // Validate recipe IDs
        if (request.RecipeIds == null || request.RecipeIds.Count == 0)
        {
            return Results.BadRequest(new { error = "At least one recipe ID is required" });
        }

        // Get existing recipe links
        var existingLinks = await db.SubscriptionRecipes
            .Where(sr => sr.SubscriptionId == id)
            .Select(sr => sr.RecipeId)
            .ToListAsync();

        // Filter out recipes that are already linked
        var newRecipeIds = request.RecipeIds.Except(existingLinks).ToList();

        if (newRecipeIds.Count == 0)
        {
            return Results.BadRequest(new { error = "All recipes are already in this subscription" });
        }

        // Verify all recipes exist
        var existingRecipes = await db.Recipes
            .Where(r => newRecipeIds.Contains(r.Id))
            .Select(r => r.Id)
            .ToListAsync();

        if (existingRecipes.Count != newRecipeIds.Count)
        {
            return Results.BadRequest(new { error = "One or more recipes not found" });
        }

        // Create subscription-recipe links
        var subscriptionRecipes = newRecipeIds.Select(recipeId => new SubscriptionRecipe
        {
            SubscriptionId = id,
            RecipeId = recipeId,
            AddedAt = DateTime.UtcNow
        }).ToList();

        db.SubscriptionRecipes.AddRange(subscriptionRecipes);
        await db.SaveChangesAsync();

        return Results.Ok(new
        {
            message = $"Added {subscriptionRecipes.Count} recipes to subscription",
            addedRecipeIds = newRecipeIds
        });
    }

    /// <summary>
    /// DELETE /api/subscriptions/{id}/recipes/{recipeId}
    /// Remove a recipe from a subscription plan
    /// Admin only - requires authentication
    /// </summary>
    private static async Task<IResult> RemoveRecipeFromSubscriptionAsync(
        Guid id,
        Guid recipeId,
        ClaimsPrincipal user,
        AppDbContext db)
    {
        // Validate authentication
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
        {
            return Results.Unauthorized();
        }

        // Find subscription-recipe link
        var link = await db.SubscriptionRecipes
            .FirstOrDefaultAsync(sr => sr.SubscriptionId == id && sr.RecipeId == recipeId);

        if (link == null)
        {
            return Results.NotFound(new { error = "Recipe not found in this subscription" });
        }

        // Remove link
        db.SubscriptionRecipes.Remove(link);
        await db.SaveChangesAsync();

        return Results.Ok(new { message = "Recipe removed from subscription" });
    }

    /// <summary>
    /// POST /api/subscriptions/{id}/buy
    /// Purchase a subscription with points
    /// Requires authentication
    /// </summary>
    private static async Task<IResult> BuySubscriptionAsync(
        Guid id,
        ClaimsPrincipal user,
        AppDbContext db)
    {
        // Get current user ID from JWT claims
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        // Find subscription
        var subscription = await db.Subscriptions.FindAsync(id);
        if (subscription == null)
        {
            return Results.NotFound(new { error = "Subscription not found" });
        }

        if (!subscription.IsActive)
        {
            return Results.BadRequest(new { error = "Subscription is not available" });
        }

        // Find user
        var currentUser = await db.Users.FindAsync(userId);
        if (currentUser == null)
        {
            return Results.NotFound(new { error = "User not found" });
        }

        // Check balance
        if (currentUser.Balance < subscription.Price)
        {
            return Results.BadRequest(new
            {
                error = "Insufficient balance",
                required = subscription.Price,
                current = currentUser.Balance
            });
        }

        // Start database transaction for consistency
        using var transaction = await db.Database.BeginTransactionAsync();

        try
        {
            // Deduct points from user
            currentUser.Balance -= subscription.Price;

            // Create user subscription
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(subscription.DurationDays);

            var userSubscription = new UserSubscription
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SubscriptionId = id,
                StartDate = startDate,
                EndDate = endDate,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            db.UserSubscriptions.Add(userSubscription);

            // Create transaction record
            var transactionRecord = new Transaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Amount = -subscription.Price,
                Type = TransactionType.Purchase, // Reusing Purchase type
                RecipeId = null, // No specific recipe for subscription purchase
                CreatedAt = DateTime.UtcNow
            };

            db.Transactions.Add(transactionRecord);

            // Commit all changes
            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Results.Ok(new
            {
                message = "Subscription purchased successfully",
                subscription = new
                {
                    id = userSubscription.Id,
                    name = subscription.Name,
                    durationDays = subscription.DurationDays,
                    startDate = userSubscription.StartDate,
                    endDate = userSubscription.EndDate
                },
                newBalance = currentUser.Balance
            });
        }
        catch
        {
            // Rollback on error
            await transaction.RollbackAsync();
            return Results.Problem("Failed to process subscription purchase");
        }
    }

    /// <summary>
    /// GET /api/subscriptions/my
    /// Get current user's active subscriptions
    /// Requires authentication
    /// </summary>
    private static async Task<IResult> GetMySubscriptionsAsync(
        ClaimsPrincipal user,
        AppDbContext db)
    {
        // Get current user ID from JWT claims
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        // Fetch user's active subscriptions
        var subscriptions = await db.UserSubscriptions
            .Where(us => us.UserId == userId && us.IsActive && us.EndDate > DateTime.UtcNow)
            .Include(us => us.Subscription)
            .Select(us => new
            {
                id = us.Id,
                subscription = new
                {
                    id = us.Subscription!.Id,
                    name = us.Subscription.Name,
                    description = us.Subscription.Description,
                    durationDays = us.Subscription.DurationDays
                },
                startDate = us.StartDate,
                endDate = us.EndDate,
                isActive = us.IsActive,
                daysRemaining = (us.EndDate - DateTime.UtcNow).Days
            })
            .OrderByDescending(us => us.endDate)
            .ToListAsync();

        return Results.Ok(subscriptions);
    }

    // Request DTOs

    /// <summary>
    /// Request model for creating a subscription
    /// </summary>
    public record CreateSubscriptionRequest(
        string Name,
        string? Description,
        int DurationDays,
        decimal Price
    );

    /// <summary>
    /// Request model for updating a subscription
    /// </summary>
    public record UpdateSubscriptionRequest(
        string Name,
        string? Description,
        int DurationDays,
        decimal Price,
        bool IsActive
    );

    /// <summary>
    /// Request model for adding recipes to a subscription
    /// </summary>
    public record AddRecipesRequest(
        List<Guid> RecipeIds
    );
}
