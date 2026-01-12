using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RecipesApi.Data;

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
            balance = currentUser.Balance,
            createdAt = currentUser.CreatedAt
        });
    }
}
