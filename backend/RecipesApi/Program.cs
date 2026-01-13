using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RecipesApi.Data;
using RecipesApi.Endpoints;
using RecipesApi.Services;

// Create the web application builder
// This is the entry point for configuring services and middleware
var builder = WebApplication.CreateBuilder(args);

// ==================== CONFIGURATION ====================

// Read JWT settings from appsettings.json
var jwtSecretKey = builder.Configuration["JwtSettings:SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey is not configured");
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"]
    ?? throw new InvalidOperationException("JWT Issuer is not configured");
var jwtExpirationMinutes = int.Parse(
    builder.Configuration["JwtSettings:ExpirationMinutes"] ?? "60"
);

// Read database connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Database connection string is not configured");

// ==================== SERVICES CONFIGURATION ====================

// Configure PostgreSQL database with Entity Framework Core
// UseLazyLoadingProxies enables automatic loading of related entities
// UseSnakeCaseNamingConvention converts C# PascalCase to PostgreSQL snake_case
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    // Enable detailed error messages in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Register JWT token service as singleton (one instance for the entire application)
// Singleton is appropriate because this service is stateless and thread-safe
builder.Services.AddSingleton(new JwtTokenService(
    jwtSecretKey,
    jwtIssuer,
    jwtExpirationMinutes
));

// Register password hashing service as singleton
// BCrypt is thread-safe and stateless, so singleton is appropriate
builder.Services.AddSingleton<PasswordHashService>();

// Configure JWT authentication
// This middleware will validate JWT tokens on protected endpoints
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Configure how to validate incoming JWT tokens
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Verify that the token was signed with our secret key
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecretKey)
            ),

            // Verify the issuer (who created the token)
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            // Verify the audience (who the token is intended for)
            ValidateAudience = true,
            ValidAudience = jwtIssuer,

            // Verify the token hasn't expired
            ValidateLifetime = true,

            // No grace period for token expiration
            ClockSkew = TimeSpan.Zero
        };

        // Configure to read JWT from cookies instead of Authorization header
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Try to read token from "auth_token" cookie
                // If cookie exists, set it as the token for validation
                context.Token = context.Request.Cookies["auth_token"];
                return Task.CompletedTask;
            }
        };
    });

// Add authorization services
// This enables the [Authorize] attribute on endpoints
builder.Services.AddAuthorization();

// Configure CORS (Cross-Origin Resource Sharing)
// This allows frontend applications to make requests to the API
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        // Allow requests from Nuxt app (port 3000) and React admin (port 5173)
        policy.WithOrigins(
                "http://localhost:3000",  // Nuxt main app
                "http://localhost:5173"   // React admin panel
            )
            // Allow credentials (cookies) to be sent with requests
            // Required for httpOnly cookie authentication
            .AllowCredentials()
            // Allow any HTTP method (GET, POST, PUT, DELETE, etc.)
            .AllowAnyMethod()
            // Allow any headers in requests
            .AllowAnyHeader();
    });
});

// Add memory caching for frequently accessed data
// IMemoryCache provides in-process caching (lost on app restart)
builder.Services.AddMemoryCache();

// Add output caching for HTTP responses
// Caches entire HTTP responses for specified durations
builder.Services.AddOutputCache();

// Add rate limiting to protect against DDoS and brute force attacks
// Uses built-in ASP.NET Core 9 rate limiting middleware
builder.Services.AddRateLimiter(options =>
{
    // Fixed window limiter for general API requests
    // Allows 100 requests per minute per IP address
    options.AddFixedWindowLimiter("api", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 10;
    });

    // Strict limiter for authentication endpoints (login, register)
    // Prevents brute force attacks: 5 requests per minute per IP
    options.AddFixedWindowLimiter("auth", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 2;
    });

    // Response when rate limit is exceeded
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429; // Too Many Requests
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Too many requests. Please try again later.",
            retryAfter = context.Lease.TryGetMetadata(System.Threading.RateLimiting.MetadataName.RetryAfter, out var retryAfter)
                ? retryAfter.TotalSeconds
                : 60
        }, cancellationToken: token);
    };
});

// Add Swagger/OpenAPI for API documentation and testing
// Only available in development mode
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Recipe PWA API",
        Version = "v1",
        Description = "API for Recipe PWA - Points economy and recipe trading system"
    });
});

// ==================== BUILD APPLICATION ====================

var app = builder.Build();

// ==================== DATABASE MIGRATION ====================

// Automatically apply pending EF Core migrations on startup
// This ensures the database schema is always up to date
// WARNING: In production, use a proper migration strategy
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        // Apply all pending migrations
        db.Database.Migrate();
        Console.WriteLine("✓ Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Database migration failed: {ex.Message}");
        // Log but don't crash - database might not be ready yet
    }
}

// ==================== MIDDLEWARE PIPELINE ====================

// Configure the HTTP request pipeline
// Order matters! Middleware runs in the order it's added

// Enable CORS (must be before auth middleware)
app.UseCors();

// Enable Swagger UI in development for API testing
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Recipe PWA API v1");
        options.RoutePrefix = "swagger"; // Access at /swagger
    });
}

// Redirect HTTP to HTTPS (disabled in development for simplicity)
// app.UseHttpsRedirection();

// Enable authentication middleware
// This validates JWT tokens on protected endpoints
app.UseAuthentication();

// Enable authorization middleware
// This enforces [Authorize] attributes
app.UseAuthorization();

// Enable output caching
app.UseOutputCache();

// Enable rate limiting middleware
// This must be placed before endpoint mapping to intercept requests
app.UseRateLimiter();

// ==================== API ENDPOINTS ====================

// Health check endpoint - verify API is running
app.MapGet("/api/health", () => new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
})
.WithName("HealthCheck")
.WithTags("System");

// TEMPORARY: Admin setup endpoint - remove after setting up admin users
app.MapPost("/api/admin/make-admin/{userId:guid}", async (Guid userId, AppDbContext db) =>
{
    var user = await db.Users.FindAsync(userId);
    if (user == null) return Results.NotFound(new { error = "User not found" });

    user.IsAdmin = true;
    await db.SaveChangesAsync();

    return Results.Ok(new { message = $"User {user.Email} is now an admin", isAdmin = user.IsAdmin });
})
.WithTags("System");

// Register API endpoint groups
// Each endpoint group is defined in a separate file in the Endpoints folder
app.MapAuthEndpoints();          // Authentication: /api/auth/*
app.MapUserEndpoints();          // Users: /api/users/*
app.MapRecipeEndpoints();        // Recipes: /api/recipes/*
app.MapPointsEndpoints();        // Points: /api/points/*
app.MapTradeEndpoints();         // Trades: /api/trades/*
app.MapSubscriptionEndpoints();  // Subscriptions: /api/subscriptions/*

// ==================== START APPLICATION ====================

Console.WriteLine("Recipe PWA API starting...");
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"Database: {connectionString.Split(';')[0]}"); // Just show host

app.Run();
