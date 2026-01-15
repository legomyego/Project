using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RecipesApi.Data;
using RecipesApi.Models;

namespace RecipesApi.Endpoints.Admin;

/// <summary>
/// Admin endpoints for recipe management
/// Handles recipe updates and deletion - admin-only operations
/// </summary>
public static class RecipeAdminEndpoints
{
    /// <summary>
    /// Registers admin recipe endpoints with the application
    /// Called from Program.cs to add admin recipe routes to the API
    /// </summary>
    public static void MapAdminRecipeEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/admin-api/recipes")
            .WithTags("Recipes (Admin)")
            .RequireAuthorization(); // All admin endpoints require authentication

        // PUT /admin-api/recipes/{id} - Update an existing recipe
        group.MapPut("/{id:guid}", UpdateRecipeAsync)
            .WithName("UpdateRecipe")
            .WithSummary("Update a recipe (Admin)")
            .WithDescription("Update an existing recipe. Only admins or recipe authors can update recipes.");

        // DELETE /admin-api/recipes/{id} - Delete a recipe
        group.MapDelete("/{id:guid}", DeleteRecipeAsync)
            .WithName("DeleteRecipe")
            .WithSummary("Delete a recipe (Admin)")
            .WithDescription("Delete a recipe. Only admins or recipe authors can delete recipes.");
    }

    /// <summary>
    /// Request model for updating an existing recipe
    /// Contains fields that can be updated
    /// </summary>
    public record UpdateRecipeRequest(
        string? Title,
        string? Description,
        decimal? Price,
        bool? RequiresSubscription
    );

    /// <summary>
    /// Update an existing recipe
    /// Only admins or the recipe author can update
    /// </summary>
    private static async Task<IResult> UpdateRecipeAsync(
        Guid id,
        UpdateRecipeRequest request,
        ClaimsPrincipal user,
        AppDbContext db)
    {
        // Get current user ID from JWT claims
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        // Find the recipe
        var recipe = await db.Recipes
            .Include(r => r.Author)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (recipe == null)
        {
            return Results.NotFound(new { error = "Recipe not found" });
        }

        // Check if user is admin
        var currentUser = await db.Users.FindAsync(userId);
        var isAdmin = currentUser?.IsAdmin ?? false;

        // Check authorization: must be admin or recipe author
        if (!isAdmin && recipe.AuthorId != userId)
        {
            return Results.Forbid();
        }

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            recipe.Title = request.Title.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            recipe.Description = request.Description.Trim();
        }

        if (request.Price.HasValue && request.Price.Value >= 0)
        {
            recipe.Price = request.Price.Value;
        }

        if (request.RequiresSubscription.HasValue)
        {
            recipe.RequiresSubscription = request.RequiresSubscription.Value;
        }

        // Save changes
        await db.SaveChangesAsync();

        // Return updated recipe
        return Results.Ok(new
        {
            id = recipe.Id,
            title = recipe.Title,
            description = recipe.Description,
            price = recipe.Price,
            requiresSubscription = recipe.RequiresSubscription,
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
    /// Delete a recipe
    /// Only admins or the recipe author can delete
    /// Also removes all associated user recipe ownerships and trade offers
    /// </summary>
    private static async Task<IResult> DeleteRecipeAsync(
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

        // Find the recipe
        var recipe = await db.Recipes.FindAsync(id);

        if (recipe == null)
        {
            return Results.NotFound(new { error = "Recipe not found" });
        }

        // Check if user is admin
        var currentUser = await db.Users.FindAsync(userId);
        var isAdmin = currentUser?.IsAdmin ?? false;

        // Check authorization: must be admin or recipe author
        if (!isAdmin && recipe.AuthorId != userId)
        {
            return Results.Forbid();
        }

        // Delete the recipe (cascade will handle related records)
        db.Recipes.Remove(recipe);
        await db.SaveChangesAsync();

        return Results.Ok(new
        {
            message = "Recipe deleted successfully",
            deletedRecipeId = id
        });
    }
}
