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
    /// DbSet for UserRecipes junction table
    /// Represents the many-to-many relationship between Users and Recipes
    /// Tracks which recipes each user owns (via purchase or trade)
    /// </summary>
    public DbSet<UserRecipe> UserRecipes => Set<UserRecipe>();

    /// <summary>
    /// DbSet for Trades table
    /// Contains all trade offers between users for recipe exchanges
    /// EF Core maps this to a "Trades" table
    /// </summary>
    public DbSet<Trade> Trades => Set<Trade>();

    /// <summary>
    /// DbSet for Subscriptions table
    /// Contains all subscription plans that users can purchase
    /// EF Core maps this to a "Subscriptions" table
    /// </summary>
    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    /// <summary>
    /// DbSet for UserSubscriptions table
    /// Tracks which users have which active or expired subscriptions
    /// EF Core maps this to a "UserSubscriptions" table
    /// </summary>
    public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();

    /// <summary>
    /// DbSet for SubscriptionRecipes junction table
    /// Represents the many-to-many relationship between Subscriptions and Recipes
    /// Tracks which recipes are included in which subscription plans
    /// </summary>
    public DbSet<SubscriptionRecipe> SubscriptionRecipes => Set<SubscriptionRecipe>();

    /// <summary>
    /// DbSet for PasswordResetTokens table
    /// Stores password reset tokens for forgot password flow
    /// Tokens are one-time use with expiration
    /// </summary>
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

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

            // IsAdmin defaults to false - regular user by default
            entity.Property(u => u.IsAdmin)
                .HasDefaultValue(false);

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

        // Configure UserRecipe junction table (many-to-many between User and Recipe)
        modelBuilder.Entity<UserRecipe>(entity =>
        {
            // Composite primary key: combination of UserId and RecipeId must be unique
            // A user can only own a specific recipe once
            entity.HasKey(ur => new { ur.UserId, ur.RecipeId });

            // AcquisitionType is stored as integer enum (0=Purchase, 1=Trade)
            entity.Property(ur => ur.AcquisitionType)
                .HasConversion<int>();

            // AcquiredAt defaults to current UTC time
            entity.Property(ur => ur.AcquiredAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Configure relationship: UserRecipe belongs to User
            // When a User is deleted, their owned recipes are also deleted (cascade)
            entity.HasOne(ur => ur.User)
                .WithMany(u => u.OwnedRecipes)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure relationship: UserRecipe belongs to Recipe
            // When a Recipe is deleted, ownership records are also deleted (cascade)
            entity.HasOne(ur => ur.Recipe)
                .WithMany()
                .HasForeignKey(ur => ur.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Trade entity
        modelBuilder.Entity<Trade>(entity =>
        {
            // Status is stored as integer enum (0=Pending, 1=Accepted, 2=Declined, 3=Cancelled)
            entity.Property(t => t.Status)
                .HasConversion<int>();

            // CreatedAt defaults to current UTC time
            entity.Property(t => t.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Configure relationship: Trade has an offering user (who initiated the trade)
            // When a User is deleted, their offered trades are deleted (cascade)
            entity.HasOne(t => t.OfferingUser)
                .WithMany()
                .HasForeignKey(t => t.OfferingUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure relationship: Trade has an offered recipe (what the offering user gives)
            // When a Recipe is deleted, trades referencing it are deleted (cascade)
            entity.HasOne(t => t.OfferedRecipe)
                .WithMany()
                .HasForeignKey(t => t.OfferedRecipeId)
                .OnDelete(DeleteBehavior.Restrict); // Restrict to prevent accidental deletion

            // Configure relationship: Trade has a requested user (who receives the offer)
            // When a User is deleted, trades targeting them are deleted (cascade)
            entity.HasOne(t => t.RequestedUser)
                .WithMany()
                .HasForeignKey(t => t.RequestedUserId)
                .OnDelete(DeleteBehavior.Restrict); // Restrict to prevent cascade conflict with OfferingUser

            // Configure relationship: Trade has a requested recipe (what the offering user wants)
            // When a Recipe is deleted, trades requesting it are deleted (cascade)
            entity.HasOne(t => t.RequestedRecipe)
                .WithMany()
                .HasForeignKey(t => t.RequestedRecipeId)
                .OnDelete(DeleteBehavior.Restrict); // Restrict to prevent accidental deletion
        });

        // Configure Subscription entity
        modelBuilder.Entity<Subscription>(entity =>
        {
            // Name is required
            entity.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(200);

            // Description is optional but has max length
            entity.Property(s => s.Description)
                .HasMaxLength(1000);

            // Price uses decimal(18,2) precision
            entity.Property(s => s.Price)
                .HasPrecision(18, 2);

            // DurationDays is required (must be positive)
            entity.Property(s => s.DurationDays)
                .IsRequired();

            // IsActive defaults to true
            entity.Property(s => s.IsActive)
                .HasDefaultValue(true);

            // CreatedAt defaults to current UTC time
            entity.Property(s => s.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Configure UserSubscription entity
        modelBuilder.Entity<UserSubscription>(entity =>
        {
            // IsActive defaults to true
            entity.Property(us => us.IsActive)
                .HasDefaultValue(true);

            // CreatedAt defaults to current UTC time
            entity.Property(us => us.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Configure relationship: UserSubscription belongs to User
            // When a User is deleted, their subscriptions are also deleted (cascade)
            entity.HasOne(us => us.User)
                .WithMany()
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure relationship: UserSubscription belongs to Subscription
            // When a Subscription is deleted, user subscriptions are restricted (don't allow)
            entity.HasOne(us => us.Subscription)
                .WithMany(s => s.UserSubscriptions)
                .HasForeignKey(us => us.SubscriptionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure SubscriptionRecipe junction table (many-to-many between Subscription and Recipe)
        modelBuilder.Entity<SubscriptionRecipe>(entity =>
        {
            // Composite primary key: combination of SubscriptionId and RecipeId must be unique
            // A recipe can only be in a subscription once
            entity.HasKey(sr => new { sr.SubscriptionId, sr.RecipeId });

            // AddedAt defaults to current UTC time
            entity.Property(sr => sr.AddedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Configure relationship: SubscriptionRecipe belongs to Subscription
            // When a Subscription is deleted, its recipe links are also deleted (cascade)
            entity.HasOne(sr => sr.Subscription)
                .WithMany(s => s.SubscriptionRecipes)
                .HasForeignKey(sr => sr.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure relationship: SubscriptionRecipe belongs to Recipe
            // When a Recipe is deleted, subscription links are also deleted (cascade)
            entity.HasOne(sr => sr.Recipe)
                .WithMany(r => r.SubscriptionRecipes)
                .HasForeignKey(sr => sr.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure PasswordResetToken entity
        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            // Token must be unique
            entity.HasIndex(t => t.Token).IsUnique();

            // Token is required
            entity.Property(t => t.Token)
                .IsRequired()
                .HasMaxLength(500);

            // Configure relationship: PasswordResetToken belongs to User
            // When User is deleted, their reset tokens are also deleted (cascade)
            entity.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Add index on ExpiresAt for efficient cleanup of expired tokens
            entity.HasIndex(t => t.ExpiresAt);

            // Add index on UserId for efficient lookup
            entity.HasIndex(t => t.UserId);
        });
    }
}
