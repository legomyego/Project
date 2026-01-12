using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace RecipesApi.Services;

/// <summary>
/// Service for generating and validating JWT (JSON Web Tokens)
/// JWTs are used for stateless authentication - the token contains
/// all the information needed to verify the user's identity
/// </summary>
public class JwtTokenService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly int _expirationMinutes;

    /// <summary>
    /// Constructor accepts JWT configuration from appsettings.json
    /// </summary>
    /// <param name="secretKey">Secret key for signing tokens (min 32 characters)</param>
    /// <param name="issuer">Issuer identifier (e.g., "RecipesAPI")</param>
    /// <param name="expirationMinutes">How long until token expires</param>
    public JwtTokenService(string secretKey, string issuer, int expirationMinutes)
    {
        _secretKey = secretKey;
        _issuer = issuer;
        _expirationMinutes = expirationMinutes;
    }

    /// <summary>
    /// Generate a new JWT token for a user
    /// The token contains the user's ID and email as claims
    /// </summary>
    /// <param name="userId">User's unique identifier</param>
    /// <param name="email">User's email address</param>
    /// <returns>JWT token string</returns>
    public string GenerateToken(Guid userId, string email)
    {
        // Create claims - pieces of information stored in the token
        // Claims are readable by the client but cannot be modified without invalidating the token
        var claims = new[]
        {
            // "sub" (subject) claim - standard JWT claim for user identifier
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),

            // "email" claim - user's email address
            new Claim(JwtRegisteredClaimNames.Email, email),

            // "jti" (JWT ID) claim - unique identifier for this specific token
            // Useful for token revocation in more advanced scenarios
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

            // "iat" (issued at) claim - when the token was created
            // Using Unix timestamp
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        // Convert secret key string to bytes for signing
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));

        // Create signing credentials using HMAC-SHA256 algorithm
        // This ensures the token cannot be tampered with
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Create the actual token
        var token = new JwtSecurityToken(
            issuer: _issuer,                                       // Who created the token
            audience: _issuer,                                     // Who the token is intended for
            claims: claims,                                        // User information
            expires: DateTime.UtcNow.AddMinutes(_expirationMinutes), // When token expires
            signingCredentials: credentials                        // How to verify the token
        );

        // Serialize the token to a string that can be sent to the client
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Extract user ID from a JWT token
    /// Useful for getting the current user from a validated token
    /// </summary>
    /// <param name="token">JWT token string</param>
    /// <returns>User ID if token is valid, null otherwise</returns>
    public Guid? GetUserIdFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            // Validate the token and extract claims
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _issuer,
                ValidateLifetime = true,  // Check if token is expired
                ClockSkew = TimeSpan.Zero // No grace period for expiration
            }, out var validatedToken);

            // Extract the "sub" (subject) claim which contains the user ID
            var jwtToken = (JwtSecurityToken)validatedToken;
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);

            // Parse and return the user ID
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return null;
        }
        catch
        {
            // Token is invalid, expired, or malformed
            return null;
        }
    }
}
