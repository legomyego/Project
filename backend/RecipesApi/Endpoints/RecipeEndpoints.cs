using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RecipesApi.Data;

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
                    email = r.Author.Email
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
                email = recipe.Author.Email
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
                        email = r.Author.Email
                    }
                })
                .ToListAsync();
        });

        return Results.Ok(new { recipes = popularRecipes });
    }
}
