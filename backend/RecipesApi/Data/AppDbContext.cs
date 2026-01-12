using Microsoft.EntityFrameworkCore;
using RecipesApi.Models;

namespace RecipesApi.Data;

/// <summary>
/// Entity Framework Core database context for the Recipe PWA application
/// This class defines the database structure and configures entity relationships
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Constructor accepts DbContextOptions to configure the database connection
    /// Options are typically configured in Program.cs with connection string
    /// </summary>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// DbSet for Users table
    /// Represents the collection of all users in the database
    /// EF Core maps this to a "Users" table
    /// </summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// DbSet for Recipes table
    /// Contains all recipes created by users
    /// EF Core maps this to a "Recipes" table
    /// </summary>
    public DbSet<Recipe> Recipes => Set<Recipe>();

    /// <summary>
    /// DbSet for Transactions table
    /// Logs all financial transactions (purchases, sales, top-ups)
    /// EF Core maps this to a "Transactions" table
    /// </summary>
    public DbSet<Transaction> Transactions => Set<Transaction>();

    /// <summary>
    /// Configure entity relationships and constraints using Fluent API
    /// This method is called when EF Core creates the model
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            // Email must be unique across all users
            entity.HasIndex(u => u.Email).IsUnique();

            // Email is required and has max length
            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);

            // PasswordHash is required
            entity.Property(u => u.PasswordHash)
                .IsRequired();

            // Balance defaults to 0 and uses decimal(18,2) precision
            // This allows values up to 999,999,999,999,999.99
            entity.Property(u => u.Balance)
                .HasPrecision(18, 2)
                .HasDefaultValue(0);

            // CreatedAt defaults to current UTC time
            entity.Property(u => u.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Configure Recipe entity
        modelBuilder.Entity<Recipe>(entity =>
        {
            // Title is required
            entity.Property(r => r.Title)
                .IsRequired()
                .HasMaxLength(500);

            // Description is optional but has max length
            entity.Property(r => r.Description)
                .HasMaxLength(5000);

            // Price uses decimal(18,2) precision
            entity.Property(r => r.Price)
                .HasPrecision(18, 2);

            // Views defaults to 0
            entity.Property(r => r.Views)
                .HasDefaultValue(0);

            // CreatedAt defaults to current UTC time
            entity.Property(r => r.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Configure relationship: Recipe belongs to User (Author)
            // When a User is deleted, their recipes are also deleted (cascade)
            entity.HasOne(r => r.Author)
                .WithMany(u => u.CreatedRecipes)
                .HasForeignKey(r => r.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Transaction entity
        modelBuilder.Entity<Transaction>(entity =>
        {
            // Amount uses decimal(18,2) precision
            entity.Property(t => t.Amount)
                .HasPrecision(18, 2);

            // Type is stored as integer enum
            entity.Property(t => t.Type)
                .HasConversion<int>();

            // CreatedAt defaults to current UTC time
            entity.Property(t => t.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Configure relationship: Transaction belongs to User
            // When a User is deleted, their transactions are also deleted (cascade)
            entity.HasOne(t => t.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure optional relationship: Transaction may reference a Recipe
            // When a Recipe is deleted, set RecipeId to null (don't delete transactions)
            entity.HasOne(t => t.Recipe)
                .WithMany()
                .HasForeignKey(t => t.RecipeId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
