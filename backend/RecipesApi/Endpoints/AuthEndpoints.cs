using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipesApi.Data;
using RecipesApi.Models;
using RecipesApi.Services;

namespace RecipesApi.Endpoints;

/// <summary>
/// Extension class for registering authentication-related endpoints
/// Handles user registration, login, and logout
/// </summary>
public static class AuthEndpoints
{
    /// <summary>
    /// Registers all authentication endpoints with the application
    /// Called from Program.cs to add auth routes to the API
    /// </summary>
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        // POST /api/auth/register - Register a new user
        group.MapPost("/register", RegisterAsync)
            .WithName("Register")
            .WithSummary("Register a new user account")
            .WithDescription("Creates a new user account with email and password. Returns a JWT token in an httpOnly cookie.");

        // POST /api/auth/login - Authenticate existing user
        group.MapPost("/login", LoginAsync)
            .WithName("Login")
            .WithSummary("Login with email and password")
            .WithDescription("Authenticates a user and returns a JWT token in an httpOnly cookie.");

        // POST /api/auth/logout - Clear authentication cookie
        group.MapPost("/logout", LogoutAsync)
            .WithName("Logout")
            .WithSummary("Logout current user")
            .WithDescription("Clears the authentication cookie, effectively logging out the user.");
    }

    /// <summary>
    /// Register a new user account
    /// Creates user record and returns JWT token
    /// </summary>
    private static async Task<IResult> RegisterAsync(
        [FromBody] RegisterRequest request,
        AppDbContext db,
        PasswordHashService passwordHasher,
        JwtTokenService jwtService,
        HttpContext httpContext)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.BadRequest(new { error = "Email and password are required" });
        }

        // Check if email is already registered
        var existingUser = await db.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (existingUser != null)
        {
            return Results.Conflict(new { error = "Email already registered" });
        }

        // Validate password strength (minimum 6 characters)
        if (request.Password.Length < 6)
        {
            return Results.BadRequest(new { error = "Password must be at least 6 characters" });
        }

        // Create new user with hashed password
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLowerInvariant().Trim(),
            PasswordHash = passwordHasher.HashPassword(request.Password),
            Balance = 0, // New users start with 0 balance
            CreatedAt = DateTime.UtcNow
        };

        // Save user to database
        db.Users.Add(user);
        await db.SaveChangesAsync();

        // Generate JWT token for immediate login
        var token = jwtService.GenerateToken(user.Id, user.Email);

        // Set JWT token as httpOnly cookie
        // httpOnly prevents JavaScript access (XSS protection)
        // Secure flag will be enabled in production (HTTPS only)
        httpContext.Response.Cookies.Append("auth_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // Set to true in production with HTTPS
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(7) // Cookie expires in 7 days
        });

        // Return user data (without sensitive information)
        return Results.Ok(new
        {
            id = user.Id,
            email = user.Email,
            balance = user.Balance,
            createdAt = user.CreatedAt,
            message = "Registration successful"
        });
    }

    /// <summary>
    /// Login with existing user credentials
    /// Validates credentials and returns JWT token
    /// </summary>
    private static async Task<IResult> LoginAsync(
        [FromBody] LoginRequest request,
        AppDbContext db,
        PasswordHashService passwordHasher,
        JwtTokenService jwtService,
        HttpContext httpContext)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.BadRequest(new { error = "Email and password are required" });
        }

        // Find user by email (case-insensitive)
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant().Trim());

        // Check if user exists
        if (user == null)
        {
            // Use generic message to prevent email enumeration
            return Results.Unauthorized();
        }

        // Verify password
        if (!passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            // Use same generic message for security
            return Results.Unauthorized();
        }

        // Generate JWT token
        var token = jwtService.GenerateToken(user.Id, user.Email);

        // Set JWT token as httpOnly cookie
        httpContext.Response.Cookies.Append("auth_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // Set to true in production with HTTPS
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });

        // Return user data
        return Results.Ok(new
        {
            id = user.Id,
            email = user.Email,
            balance = user.Balance,
            createdAt = user.CreatedAt,
            message = "Login successful"
        });
    }

    /// <summary>
    /// Logout current user by clearing the authentication cookie
    /// Client-side should also clear any cached user data
    /// </summary>
    private static IResult LogoutAsync(HttpContext httpContext)
    {
        // Delete the auth_token cookie
        // This immediately invalidates the user's session
        httpContext.Response.Cookies.Delete("auth_token");

        return Results.Ok(new { message = "Logout successful" });
    }
}

/// <summary>
/// Request model for user registration
/// </summary>
public record RegisterRequest(string Email, string Password);

/// <summary>
/// Request model for user login
/// </summary>
public record LoginRequest(string Email, string Password);
