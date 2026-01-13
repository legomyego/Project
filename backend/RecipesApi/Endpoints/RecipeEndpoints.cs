using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RecipesApi.Data;
using RecipesApi.Models;

namespace RecipesApi.Endpoints;

/// <summary>
/// Extension class for registering recipe-related endpoints
/// Handles recipe listing, retrieval, and search
/// </summary>
public static class RecipeEndpoints
{
    /// <summary>
    /// Registers all recipe endpoints with the application
    /// Called from Program.cs to add recipe routes to the API
    /// </summary>
    public static void MapRecipeEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/recipes")
            .WithTags("Recipes");

        // GET /api/recipes - List all recipes
        group.MapGet("/", GetRecipesAsync)
            .WithName("GetRecipes")
            .WithSummary("List all recipes")
            .WithDescription("Returns a list of all available recipes with pagination support.")
            .CacheOutput(policy => policy.Expire(TimeSpan.FromMinutes(5))); // Cache for 5 minutes

        // GET /api/recipes/{id} - Get specific recipe by ID
        group.MapGet("/{id:guid}", GetRecipeByIdAsync)
            .WithName("GetRecipeById")
            .WithSummary("Get recipe by ID")
            .WithDescription("Returns detailed information about a specific recipe. Increments view count.");

        // GET /api/recipes/popular - Get most viewed recipes
        group.MapGet("/popular", GetPopularRecipesAsync)
            .WithName("GetPopularRecipes")
            .WithSummary("Get popular recipes")
            .WithDescription("Returns the top 20 most viewed recipes. Uses memory cache for performance.")
            .CacheOutput(policy => policy.Expire(TimeSpan.FromMinutes(10))); // Cache for 10 minutes

        // GET /api/recipes/my - Get user's owned recipes
        group.MapGet("/my", GetMyRecipesAsync)
            .WithName("GetMyRecipes")
            .WithSummary("Get your owned recipes")
            .WithDescription("Returns all recipes owned by the authenticated user (purchased or traded).")
            .RequireAuthorization(); // Requires authentication

        // POST /api/recipes/{id}/buy - Purchase a recipe with points
        group.MapPost("/{id:guid}/buy", BuyRecipeAsync)
            .WithName("BuyRecipe")
            .WithSummary("Purchase a recipe")
            .WithDescription("Purchase a recipe using points. Requires sufficient balance.")
            .RequireAuthorization(); // Requires authentication
    }

    /// <summary>
    /// Get list of all recipes with optional pagination
    /// Default: returns first 50 recipes ordered by creation date (newest first)
    /// </summary>
    private static async Task<IResult> GetRecipesAsync(
        AppDbContext db,
        int page = 1,
        int pageSize = 50)
    {
        // Validate pagination parameters
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 50; // Max 100 items per page

        // Calculate total count for pagination metadata
        var totalCount = await db.Recipes.CountAsync();

        // Fetch recipes with pagination
        // Include author information for each recipe
        var recipes = await db.Recipes
            .Include(r => r.Author) // Load author data
            .OrderByDescending(r => r.CreatedAt) // Newest first
            .Skip((page - 1) * pageSize) // Skip previous pages
            .Take(pageSize) // Take only requested page size
            .Select(r => new
            {
                id = r.Id,
                title = r.Title,
                description = r.Description,
                price = r.Price,
                views = r.Views,
                createdAt = r.CreatedAt,
                author = new
                {
                    id = r.Author!.Id,
                    email = r.Author.Email,
                    username = r.Author.Username
                }
            })
            .ToListAsync();

        // Return recipes with pagination metadata
        return Results.Ok(new
        {
            recipes,
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
    /// Get a specific recipe by its ID
    /// Increments view count each time the recipe is accessed
    /// </summary>
    private static async Task<IResult> GetRecipeByIdAsync(
        Guid id,
        AppDbContext db)
    {
        // Find recipe by ID and include author information
        var recipe = await db.Recipes
            .Include(r => r.Author)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (recipe == null)
        {
            return Results.NotFound(new { error = "Recipe not found" });
        }

        // Increment view count
        // This tracks recipe popularity for trending/popular features
        recipe.Views++;
        await db.SaveChangesAsync();

        // Return recipe details
        return Results.Ok(new
        {
            id = recipe.Id,
            title = recipe.Title,
            description = recipe.Description,
            price = recipe.Price,
            views = recipe.Views,
            createdAt = recipe.CreatedAt,
            author = new
            {
                id = recipe.Author!.Id,
                email = recipe.Author.Email,
                username = recipe.Author.Username
            }
        });
    }

    /// <summary>
    /// Get the most popular recipes based on view count
    /// Uses IMemoryCache for improved performance
    /// Cache is invalidated after 10 minutes
    /// </summary>
    private static async Task<IResult> GetPopularRecipesAsync(
        AppDbContext db,
        IMemoryCache cache)
    {
        // Try to get popular recipes from cache first
        // Cache key is a constant string identifier
        var cacheKey = "popular_recipes";

        // GetOrCreateAsync: returns cached value if exists, otherwise executes the function
        var popularRecipes = await cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            // Set cache expiration to 10 minutes
            // After 10 minutes, cache will be refreshed with latest data
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

            // Fetch top 20 recipes by view count
            return await db.Recipes
                .Include(r => r.Author)
                .OrderByDescending(r => r.Views) // Sort by most viewed
                .Take(20) // Limit to top 20
                .Select(r => new
                {
                    id = r.Id,
                    title = r.Title,
                    description = r.Description,
                    price = r.Price,
                    views = r.Views,
                    createdAt = r.CreatedAt,
                    author = new
                    {
                        id = r.Author!.Id,
                        email = r.Author.Email,
                        username = r.Author.Username
                    }
                })
                .ToListAsync();
        });

        return Results.Ok(new { recipes = popularRecipes });
    }

    /// <summary>
    /// Purchase a recipe with points
    /// Validates balance, creates transaction, and transfers points to recipe author
    /// </summary>
    private static async Task<IResult> BuyRecipeAsync(
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

        // Start a database transaction to ensure data consistency
        // If any step fails, all changes will be rolled back
        using var transaction = await db.Database.BeginTransactionAsync();

        try
        {
            // Find the recipe to purchase
            var recipe = await db.Recipes
                .Include(r => r.Author) // Load author to credit them
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
            {
                return Results.NotFound(new { error = "Recipe not found" });
            }

            // Find the buyer
            var buyer = await db.Users.FindAsync(userId);
            if (buyer == null)
            {
                return Results.NotFound(new { error = "User not found" });
            }

            // Check if user is trying to buy their own recipe
            if (recipe.AuthorId == userId)
            {
                return Results.BadRequest(new { error = "You cannot buy your own recipe" });
            }

            // Check if user already owns this recipe
            var alreadyOwns = await db.UserRecipes
                .AnyAsync(ur => ur.UserId == userId && ur.RecipeId == recipe.Id);

            if (alreadyOwns)
            {
                return Results.BadRequest(new { error = "You already own this recipe" });
            }

            // Check if user has sufficient balance
            if (buyer.Balance < recipe.Price)
            {
                return Results.BadRequest(new
                {
                    error = "Insufficient balance",
                    required = recipe.Price,
                    current = buyer.Balance,
                    needed = recipe.Price - buyer.Balance
                });
            }

            // Deduct points from buyer's balance
            buyer.Balance -= recipe.Price;

            // Create purchase transaction for buyer (negative amount)
            var purchaseTransaction = new Transaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Amount = -recipe.Price, // Negative for debit
                Type = TransactionType.Purchase,
                RecipeId = recipe.Id,
                CreatedAt = DateTime.UtcNow
            };

            db.Transactions.Add(purchaseTransaction);

            // Credit points to recipe author (if author still exists)
            if (recipe.Author != null)
            {
                recipe.Author.Balance += recipe.Price;

                // Create sale transaction for author (positive amount)
                var saleTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    UserId = recipe.AuthorId,
                    Amount = recipe.Price, // Positive for credit
                    Type = TransactionType.Sale,
                    RecipeId = recipe.Id,
                    CreatedAt = DateTime.UtcNow
                };

                db.Transactions.Add(saleTransaction);
            }

            // Add recipe to user's owned recipes
            var userRecipe = new UserRecipe
            {
                UserId = userId,
                RecipeId = recipe.Id,
                AcquisitionType = AcquisitionType.Purchase,
                AcquiredAt = DateTime.UtcNow
            };

            db.UserRecipes.Add(userRecipe);

            // Save all changes
            await db.SaveChangesAsync();

            // Commit the transaction - all changes are now permanent
            await transaction.CommitAsync();

            return Results.Ok(new
            {
                message = "Recipe purchased successfully",
                recipe = new
                {
                    id = recipe.Id,
                    title = recipe.Title,
                    price = recipe.Price
                },
                newBalance = buyer.Balance,
                transactionId = purchaseTransaction.Id
            });
        }
        catch (Exception ex)
        {
            // Rollback transaction on any error
            await transaction.RollbackAsync();
            return Results.Problem(
                title: "Purchase failed",
                detail: ex.Message,
                statusCode: 500
            );
        }
    }

    /// <summary>
    /// Get all recipes owned by the authenticated user
    /// Includes recipes acquired via purchase or trade
    /// Returns with acquisition details (when and how obtained)
    /// </summary>
    private static async Task<IResult> GetMyRecipesAsync(
        ClaimsPrincipal user,
        AppDbContext db,
        int page = 1,
        int pageSize = 50)
    {
        // Get authenticated user ID from JWT claims
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        // Validate pagination parameters
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 50;

        // Get total count for pagination metadata
        var totalCount = await db.UserRecipes
            .Where(ur => ur.UserId == userId)
            .CountAsync();

        // Fetch owned recipes with full details
        var ownedRecipes = await db.UserRecipes
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Recipe)
                .ThenInclude(r => r!.Author) // Include recipe author
            .OrderByDescending(ur => ur.AcquiredAt) // Most recently acquired first
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ur => new
            {
                recipe = new
                {
                    id = ur.Recipe!.Id,
                    title = ur.Recipe.Title,
                    description = ur.Recipe.Description,
                    price = ur.Recipe.Price,
                    views = ur.Recipe.Views,
                    createdAt = ur.Recipe.CreatedAt,
                    author = new
                    {
                        id = ur.Recipe.Author!.Id,
                        email = ur.Recipe.Author.Email,
                        username = ur.Recipe.Author.Username
                    }
                },
                // Acquisition details - when and how user got this recipe
                acquiredAt = ur.AcquiredAt,
                acquisitionType = ur.AcquisitionType.ToString() // "Purchase" or "Trade"
            })
            .ToListAsync();

        return Results.Ok(new
        {
            recipes = ownedRecipes,
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
