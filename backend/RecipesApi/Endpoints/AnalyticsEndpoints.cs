using Microsoft.EntityFrameworkCore;
using RecipesApi.Data;

namespace RecipesApi.Endpoints;

/// <summary>
/// Extension class for analytics and statistics endpoints
/// Provides data for admin dashboard and reports
/// </summary>
public static class AnalyticsEndpoints
{
    /// <summary>
    /// Registers all analytics endpoints with the application
    /// </summary>
    public static void MapAnalyticsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/analytics")
            .WithTags("Analytics");

        // GET /api/analytics/dashboard - Get dashboard statistics
        group.MapGet("/dashboard", GetDashboardStatsAsync)
            .WithName("GetDashboardStats")
            .WithSummary("Get dashboard statistics")
            .WithDescription("Returns key metrics for admin dashboard: users, recipes, transactions, revenue.")
            .RequireAuthorization(); // Admin only in production
    }

    /// <summary>
    /// Get comprehensive dashboard statistics
    /// Includes user counts, recipe counts, transaction data, and revenue
    /// </summary>
    private static async Task<IResult> GetDashboardStatsAsync(AppDbContext db)
    {
        // Run queries in parallel for better performance
        var totalUsersTask = db.Users.CountAsync();
        var totalRecipesTask = db.Recipes.CountAsync();
        var totalTransactionsTask = db.Transactions.CountAsync();
        var activeSubscriptionsTask = db.UserSubscriptions
            .Where(us => us.IsActive && us.EndDate > DateTime.UtcNow)
            .CountAsync();

        // Calculate total revenue (sum of all Purchase transactions)
        var totalRevenueTask = db.Transactions
            .Where(t => t.Type == Models.TransactionType.Purchase)
            .SumAsync(t => (decimal?)t.Amount);

        // Get recent transactions (last 10)
        var recentTransactionsTask = db.Transactions
            .Include(t => t.User)
            .OrderByDescending(t => t.CreatedAt)
            .Take(10)
            .Select(t => new
            {
                id = t.Id,
                type = t.Type.ToString(),
                amount = t.Amount,
                createdAt = t.CreatedAt,
                user = new
                {
                    id = t.User!.Id,
                    username = t.User.Username
                }
            })
            .ToListAsync();

        // Get most popular recipes
        var topRecipesTask = db.Recipes
            .Include(r => r.Author)
            .OrderByDescending(r => r.Views)
            .Take(5)
            .Select(r => new
            {
                id = r.Id,
                title = r.Title,
                views = r.Views,
                price = r.Price,
                author = new
                {
                    id = r.Author!.Id,
                    username = r.Author.Username
                }
            })
            .ToListAsync();

        // Wait for all queries to complete
        await Task.WhenAll(
            totalUsersTask,
            totalRecipesTask,
            totalTransactionsTask,
            activeSubscriptionsTask,
            totalRevenueTask,
            recentTransactionsTask,
            topRecipesTask
        );

        // Return all statistics
        return Results.Ok(new
        {
            overview = new
            {
                totalUsers = await totalUsersTask,
                totalRecipes = await totalRecipesTask,
                totalTransactions = await totalTransactionsTask,
                activeSubscriptions = await activeSubscriptionsTask,
                totalRevenue = Math.Abs(await totalRevenueTask ?? 0m) // Absolute value since purchases are negative
            },
            recentTransactions = await recentTransactionsTask,
            topRecipes = await topRecipesTask,
            timestamp = DateTime.UtcNow
        });
    }
}
