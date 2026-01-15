using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RecipesApi.Data;

namespace RecipesApi.Endpoints.Admin;

/// <summary>
/// Admin endpoints for user management
/// Handles admin-only user operations like listing all users
/// </summary>
public static class UserAdminEndpoints
{
    /// <summary>
    /// Registers admin user endpoints with the application
    /// Called from Program.cs to add admin user routes to the API
    /// </summary>
    public static void MapAdminUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/admin-api/users")
            .WithTags("Users (Admin)")
            .RequireAuthorization(); // All admin endpoints require authentication

        // GET /admin-api/users - Get all users (admin only)
        group.MapGet("/", GetAllUsersAsync)
            .WithName("GetAllUsers")
            .WithSummary("Get all users (Admin)")
            .WithDescription("Returns paginated list of all users with basic info. Admin only.");
    }

    /// <summary>
    /// Get all users with pagination and search
    /// Admin only - requires IsAdmin flag on user account
    /// </summary>
    private static async Task<IResult> GetAllUsersAsync(
        AppDbContext db,
        ClaimsPrincipal user,
        int page = 1,
        int pageSize = 20,
        string? search = null)
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

        // Build query with optional search
        var query = db.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(u =>
                (u.Username != null && u.Username.ToLower().Contains(searchLower)) ||
                u.Email.ToLower().Contains(searchLower)
            );
        }

        // Get total count
        var totalCount = await query.CountAsync();

        // Fetch users with pagination
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new
            {
                id = u.Id,
                email = u.Email,
                username = u.Username,
                balance = u.Balance,
                isAdmin = u.IsAdmin,
                createdAt = u.CreatedAt
            })
            .ToListAsync();

        return Results.Ok(new
        {
            users,
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
