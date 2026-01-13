using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipesApi.Data;
using RecipesApi.Services;

namespace RecipesApi.Endpoints;

/// <summary>
/// Extension class for registering user-related endpoints
/// Handles user profile retrieval and management
/// </summary>
public static class UserEndpoints
{
    /// <summary>
    /// Registers all user endpoints with the application
    /// Called from Program.cs to add user routes to the API
    /// </summary>
    public static void MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users");

        // GET /api/users/me - Get current authenticated user's profile
        // Requires authentication - user must be logged in
        group.MapGet("/me", GetCurrentUserAsync)
            .WithName("GetCurrentUser")
            .WithSummary("Get current user profile")
            .WithDescription("Returns the profile and balance of the currently authenticated user.")
            .RequireAuthorization(); // This endpoint requires a valid JWT token

        // GET /api/users/search?username={username} - Search user by username and get their recipes
        // Requires authentication
        group.MapGet("/search", SearchUserByUsernameAsync)
            .WithName("SearchUserByUsername")
            .WithSummary("Search user by username")
            .WithDescription("Find a user by username and get their owned recipes for trading.")
            .RequireAuthorization();

        // POST /api/users/change-password - Change user's password
        // Requires authentication and current password verification
        group.MapPost("/change-password", ChangePasswordAsync)
            .WithName("ChangePassword")
            .WithSummary("Change user password")
            .WithDescription("Change the current user's password. Requires current password for verification.")
            .RequireAuthorization();
    }

    /// <summary>
    /// Get the current authenticated user's profile and balance
    /// JWT token in cookie is automatically validated by authentication middleware
    /// </summary>
    private static async Task<IResult> GetCurrentUserAsync(
        ClaimsPrincipal user,
        AppDbContext db)
    {
        // Extract user ID from JWT claims
        // The JWT token contains claims set when the token was generated
        // "sub" (subject) claim contains the user's ID
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            // This should never happen if authentication middleware is working correctly
            return Results.Unauthorized();
        }

        // Find user in database by ID
        var currentUser = await db.Users
            .AsNoTracking() // Read-only query for better performance
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (currentUser == null)
        {
            // User had a valid token but doesn't exist in database
            // This could happen if user was deleted after token was issued
            return Results.NotFound(new { error = "User not found" });
        }

        // Return user profile data (excluding sensitive information like password hash)
        return Results.Ok(new
        {
            id = currentUser.Id,
            email = currentUser.Email,
            username = currentUser.Username,
            balance = currentUser.Balance,
            isAdmin = currentUser.IsAdmin,
            createdAt = currentUser.CreatedAt
        });
    }

    /// <summary>
    /// Search for a user by username and return their owned recipes
    /// Used for finding trading partners
    /// </summary>
    private static async Task<IResult> SearchUserByUsernameAsync(
        string username,
        ClaimsPrincipal user,
        AppDbContext db)
    {
        // Get current user ID from JWT claims
        var currentUserIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserIdClaim == null || !Guid.TryParse(currentUserIdClaim, out var currentUserId))
        {
            return Results.Unauthorized();
        }

        // Validate username parameter
        if (string.IsNullOrWhiteSpace(username))
        {
            return Results.BadRequest(new { error = "Username is required" });
        }

        // Find user by username (case-insensitive search)
        var targetUser = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

        if (targetUser == null)
        {
            return Results.NotFound(new { error = "User not found" });
        }

        // Don't allow trading with yourself
        if (targetUser.Id == currentUserId)
        {
            return Results.BadRequest(new { error = "Cannot trade with yourself" });
        }

        // Get all recipes owned by the target user
        var userRecipes = await db.UserRecipes
            .Where(ur => ur.UserId == targetUser.Id)
            .Include(ur => ur.Recipe)
                .ThenInclude(r => r!.Author)
            .Select(ur => new
            {
                id = ur.Recipe!.Id,
                title = ur.Recipe.Title,
                description = ur.Recipe.Description,
                price = ur.Recipe.Price,
                author = new
                {
                    id = ur.Recipe.Author!.Id,
                    username = ur.Recipe.Author.Username
                }
            })
            .ToListAsync();

        return Results.Ok(new
        {
            user = new
            {
                id = targetUser.Id,
                username = targetUser.Username
            },
            recipes = userRecipes
        });
    }

    /// <summary>
    /// Request model for changing password
    /// Contains current password for verification and new password
    /// </summary>
    public record ChangePasswordRequest(
        string CurrentPassword,
        string NewPassword
    );

    /// <summary>
    /// Change the current user's password
    /// Verifies current password before allowing change
    /// This prevents unauthorized password changes if someone gains session access
    /// </summary>
    private static async Task<IResult> ChangePasswordAsync(
        [FromBody] ChangePasswordRequest request,
        ClaimsPrincipal user,
        AppDbContext db,
        PasswordHashService passwordHasher)
    {
        // Get current user ID from JWT claims
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        // Validate input
        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
        {
            return Results.BadRequest(new { error = "Current password is required" });
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return Results.BadRequest(new { error = "New password is required" });
        }

        // Password strength validation
        if (request.NewPassword.Length < 6)
        {
            return Results.BadRequest(new { error = "New password must be at least 6 characters long" });
        }

        // Find user in database
        var currentUser = await db.Users.FindAsync(userId);
        if (currentUser == null)
        {
            return Results.NotFound(new { error = "User not found" });
        }

        // Verify current password
        // This is critical for security - ensures the user knows the current password
        if (!passwordHasher.VerifyPassword(request.CurrentPassword, currentUser.PasswordHash))
        {
            return Results.BadRequest(new { error = "Current password is incorrect" });
        }

        // Don't allow setting the same password
        if (passwordHasher.VerifyPassword(request.NewPassword, currentUser.PasswordHash))
        {
            return Results.BadRequest(new { error = "New password must be different from current password" });
        }

        // Hash and save new password
        currentUser.PasswordHash = passwordHasher.HashPassword(request.NewPassword);
        await db.SaveChangesAsync();

        return Results.Ok(new
        {
            message = "Password changed successfully",
            timestamp = DateTime.UtcNow
        });
    }
}
