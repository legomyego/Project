namespace RecipesApi.Services;

/// <summary>
/// Service for securely hashing and verifying passwords
/// Uses BCrypt algorithm which is designed for password hashing
/// BCrypt is slow by design to prevent brute-force attacks
/// </summary>
public class PasswordHashService
{
    /// <summary>
    /// Hash a plain text password using BCrypt
    /// The resulting hash includes a salt, making each hash unique
    /// even for the same password
    /// </summary>
    /// <param name="password">Plain text password to hash</param>
    /// <returns>BCrypt hash string (60 characters)</returns>
    public string HashPassword(string password)
    {
        // BCrypt.Net.HashPassword automatically generates a salt
        // and combines it with the hash
        // The result format is: $2a$[cost]$[22-char salt][31-char hash]
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    /// <summary>
    /// Verify that a plain text password matches a BCrypt hash
    /// Timing-safe comparison to prevent timing attacks
    /// </summary>
    /// <param name="password">Plain text password to verify</param>
    /// <param name="hash">BCrypt hash from database</param>
    /// <returns>True if password matches, false otherwise</returns>
    public bool VerifyPassword(string password, string hash)
    {
        // BCrypt.Net.Verify extracts the salt from the hash
        // and compares the hashed password
        // This is timing-safe to prevent timing attack vulnerabilities
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
