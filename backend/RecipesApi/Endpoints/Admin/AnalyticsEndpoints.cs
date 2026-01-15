using Microsoft.EntityFrameworkCore;
using RecipesApi.Data;

namespace RecipesApi.Endpoints.Admin;

/// <summary>
/// Extension class for analytics and statistics endpoints
/// Provides data for admin dashboard and reports
/// </summary>
public static class AnalyticsAdminEndpoints
{
    // Helper classes for typed grouped data
    // These prevent type inference issues in switch expressions

    /// <summary>
    /// Represents user growth data for a specific time period
    /// </summary>
    private class UserGrowthData
    {
        public string Date { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    /// <summary>
    /// Represents revenue data for a specific time period
    /// </summary>
    private class RevenueData
    {
        public string Date { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int Transactions { get; set; }
    }

    /// <summary>
    /// Represents transaction trend data for a specific time period
    /// </summary>
    private class TransactionTrendData
    {
        public string Date { get; set; } = string.Empty;
        public int Total { get; set; }
        public int Purchases { get; set; }
        public int Sales { get; set; }
        public int TopUps { get; set; }
        public decimal Volume { get; set; }
    }
    /// <summary>
    /// Registers all analytics endpoints with the application
    /// </summary>
    public static void MapAdminAnalyticsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/admin-api/analytics")
            .WithTags("Analytics");

        // GET /api/analytics/dashboard - Get dashboard statistics
        group.MapGet("/dashboard", GetDashboardStatsAsync)
            .WithName("GetDashboardStats")
            .WithSummary("Get dashboard statistics")
            .WithDescription("Returns key metrics for admin dashboard: users, recipes, transactions, revenue.")
            .RequireAuthorization(); // Admin only in production

        // GET /api/analytics/user-growth?period=daily|weekly|monthly - Get user growth over time
        group.MapGet("/user-growth", GetUserGrowthAsync)
            .WithName("GetUserGrowth")
            .WithSummary("Get user growth analytics")
            .WithDescription("Returns user registration trends grouped by day, week, or month.")
            .RequireAuthorization();

        // GET /api/analytics/revenue?period=daily|weekly|monthly - Get revenue over time
        group.MapGet("/revenue", GetRevenueAsync)
            .WithName("GetRevenue")
            .WithSummary("Get revenue analytics")
            .WithDescription("Returns revenue trends from purchases and subscriptions grouped by day, week, or month.")
            .RequireAuthorization();

        // GET /api/analytics/transactions?period=daily|weekly|monthly - Get transaction trends
        group.MapGet("/transactions", GetTransactionTrendsAsync)
            .WithName("GetTransactionTrends")
            .WithSummary("Get transaction trends")
            .WithDescription("Returns transaction volume and types over time grouped by day, week, or month.")
            .RequireAuthorization();
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

    /// <summary>
    /// Get user growth analytics over time
    /// Groups user registrations by day, week, or month
    /// </summary>
    /// <param name="period">Grouping period: daily, weekly, or monthly (default: daily)</param>
    /// <param name="days">Number of days to look back (default: 30)</param>
    private static async Task<IResult> GetUserGrowthAsync(AppDbContext db, string period = "daily", int days = 30)
    {
        // Validate period parameter
        if (period != "daily" && period != "weekly" && period != "monthly")
        {
            return Results.BadRequest(new { error = "Period must be 'daily', 'weekly', or 'monthly'" });
        }

        // Calculate the start date based on days parameter
        var startDate = DateTime.UtcNow.AddDays(-days).Date;

        // Fetch all users created after start date
        var users = await db.Users
            .Where(u => u.CreatedAt >= startDate)
            .Select(u => new { u.CreatedAt })
            .ToListAsync();

        // Group users by the specified period
        // Using typed helper class to avoid type inference issues in switch expression
        var groupedData = period switch
        {
            "daily" => users
                .GroupBy(u => u.CreatedAt.Date)
                .Select(g => new UserGrowthData
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList(),

            "weekly" => users
                .GroupBy(u => GetWeekStart(u.CreatedAt))
                .Select(g => new UserGrowthData
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList(),

            "monthly" => users
                .GroupBy(u => new DateTime(u.CreatedAt.Year, u.CreatedAt.Month, 1))
                .Select(g => new UserGrowthData
                {
                    Date = g.Key.ToString("yyyy-MM"),
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList(),

            _ => new List<UserGrowthData>()
        };

        // Calculate cumulative growth
        var cumulative = 0;
        var dataWithCumulative = groupedData.Select(item =>
        {
            cumulative += item.Count;
            return new
            {
                date = item.Date,
                count = item.Count,
                cumulative
            };
        }).ToList();

        return Results.Ok(new
        {
            period,
            days,
            data = dataWithCumulative,
            totalNewUsers = groupedData.Sum(x => x.Count)
        });
    }

    /// <summary>
    /// Get revenue analytics over time
    /// Groups revenue from purchases and subscriptions by day, week, or month
    /// </summary>
    /// <param name="period">Grouping period: daily, weekly, or monthly (default: daily)</param>
    /// <param name="days">Number of days to look back (default: 30)</param>
    private static async Task<IResult> GetRevenueAsync(AppDbContext db, string period = "daily", int days = 30)
    {
        // Validate period parameter
        if (period != "daily" && period != "weekly" && period != "monthly")
        {
            return Results.BadRequest(new { error = "Period must be 'daily', 'weekly', or 'monthly'" });
        }

        // Calculate the start date
        var startDate = DateTime.UtcNow.AddDays(-days).Date;

        // Fetch all purchase transactions (revenue generating)
        // Note: Purchase transactions have negative amounts, so we take absolute value
        var transactions = await db.Transactions
            .Where(t => t.CreatedAt >= startDate && t.Type == Models.TransactionType.Purchase)
            .Select(t => new
            {
                t.CreatedAt,
                Amount = Math.Abs(t.Amount) // Convert negative purchases to positive revenue
            })
            .ToListAsync();

        // Group revenue by the specified period
        // Using typed helper class to avoid type inference issues in switch expression
        var groupedData = period switch
        {
            "daily" => transactions
                .GroupBy(t => t.CreatedAt.Date)
                .Select(g => new RevenueData
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Revenue = g.Sum(t => t.Amount),
                    Transactions = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList(),

            "weekly" => transactions
                .GroupBy(t => GetWeekStart(t.CreatedAt))
                .Select(g => new RevenueData
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Revenue = g.Sum(t => t.Amount),
                    Transactions = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList(),

            "monthly" => transactions
                .GroupBy(t => new DateTime(t.CreatedAt.Year, t.CreatedAt.Month, 1))
                .Select(g => new RevenueData
                {
                    Date = g.Key.ToString("yyyy-MM"),
                    Revenue = g.Sum(t => t.Amount),
                    Transactions = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList(),

            _ => new List<RevenueData>()
        };

        // Calculate cumulative revenue
        var cumulativeRevenue = 0m;
        var dataWithCumulative = groupedData.Select(item =>
        {
            cumulativeRevenue += item.Revenue;
            return new
            {
                date = item.Date,
                revenue = item.Revenue,
                transactions = item.Transactions,
                cumulativeRevenue
            };
        }).ToList();

        return Results.Ok(new
        {
            period,
            days,
            data = dataWithCumulative,
            totalRevenue = groupedData.Sum(x => x.Revenue),
            totalTransactions = groupedData.Sum(x => x.Transactions)
        });
    }

    /// <summary>
    /// Get transaction trends over time
    /// Groups all transactions by type and period
    /// </summary>
    /// <param name="period">Grouping period: daily, weekly, or monthly (default: daily)</param>
    /// <param name="days">Number of days to look back (default: 30)</param>
    private static async Task<IResult> GetTransactionTrendsAsync(AppDbContext db, string period = "daily", int days = 30)
    {
        // Validate period parameter
        if (period != "daily" && period != "weekly" && period != "monthly")
        {
            return Results.BadRequest(new { error = "Period must be 'daily', 'weekly', or 'monthly'" });
        }

        // Calculate the start date
        var startDate = DateTime.UtcNow.AddDays(-days).Date;

        // Fetch all transactions
        var transactions = await db.Transactions
            .Where(t => t.CreatedAt >= startDate)
            .Select(t => new
            {
                t.CreatedAt,
                t.Type,
                t.Amount
            })
            .ToListAsync();

        // Group transactions by period
        // Using typed helper class to avoid type inference issues in switch expression
        var groupedData = period switch
        {
            "daily" => transactions
                .GroupBy(t => t.CreatedAt.Date)
                .Select(g => new TransactionTrendData
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Total = g.Count(),
                    Purchases = g.Count(t => t.Type == Models.TransactionType.Purchase),
                    Sales = g.Count(t => t.Type == Models.TransactionType.Sale),
                    TopUps = g.Count(t => t.Type == Models.TransactionType.TopUp),
                    Volume = g.Sum(t => Math.Abs(t.Amount))
                })
                .OrderBy(x => x.Date)
                .ToList(),

            "weekly" => transactions
                .GroupBy(t => GetWeekStart(t.CreatedAt))
                .Select(g => new TransactionTrendData
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Total = g.Count(),
                    Purchases = g.Count(t => t.Type == Models.TransactionType.Purchase),
                    Sales = g.Count(t => t.Type == Models.TransactionType.Sale),
                    TopUps = g.Count(t => t.Type == Models.TransactionType.TopUp),
                    Volume = g.Sum(t => Math.Abs(t.Amount))
                })
                .OrderBy(x => x.Date)
                .ToList(),

            "monthly" => transactions
                .GroupBy(t => new DateTime(t.CreatedAt.Year, t.CreatedAt.Month, 1))
                .Select(g => new TransactionTrendData
                {
                    Date = g.Key.ToString("yyyy-MM"),
                    Total = g.Count(),
                    Purchases = g.Count(t => t.Type == Models.TransactionType.Purchase),
                    Sales = g.Count(t => t.Type == Models.TransactionType.Sale),
                    TopUps = g.Count(t => t.Type == Models.TransactionType.TopUp),
                    Volume = g.Sum(t => Math.Abs(t.Amount))
                })
                .OrderBy(x => x.Date)
                .ToList(),

            _ => new List<TransactionTrendData>()
        };

        return Results.Ok(new
        {
            period,
            days,
            data = groupedData,
            summary = new
            {
                totalTransactions = groupedData.Sum(x => x.Total),
                totalPurchases = groupedData.Sum(x => x.Purchases),
                totalSales = groupedData.Sum(x => x.Sales),
                totalTopUps = groupedData.Sum(x => x.TopUps),
                totalVolume = groupedData.Sum(x => x.Volume)
            }
        });
    }

    /// <summary>
    /// Helper method to get the start of the week (Monday) for a given date
    /// </summary>
    private static DateTime GetWeekStart(DateTime date)
    {
        // Calculate days to subtract to get to Monday
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff).Date;
    }
}
