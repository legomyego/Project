using System.Net;
using System.Net.Mail;

namespace RecipesApi.Services;

/// <summary>
/// Email service for sending transactional emails
/// Supports password reset emails, verification emails, etc.
/// Uses SMTP configuration from appsettings.json
/// </summary>
public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Send a password reset email with token link
    /// Token should be URL-safe and have expiration
    /// </summary>
    public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
    {
        var resetLink = $"{_configuration["AppSettings:FrontendUrl"]}/reset-password?token={resetToken}";

        var subject = "Reset Your Password - Recipes App";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>Password Reset Request</h2>
                <p>You requested to reset your password for Recipes App.</p>
                <p>Click the link below to reset your password:</p>
                <p><a href='{resetLink}' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Reset Password</a></p>
                <p>Or copy and paste this URL into your browser:</p>
                <p>{resetLink}</p>
                <p><strong>This link will expire in 1 hour.</strong></p>
                <p>If you didn't request this, you can safely ignore this email.</p>
                <hr>
                <p style='color: #666; font-size: 12px;'>Recipes App - Recipe Trading Platform</p>
            </body>
            </html>
        ";

        await SendEmailAsync(toEmail, subject, body);
    }

    /// <summary>
    /// Core email sending method using SMTP
    /// Configured via appsettings.json EmailSettings section
    /// </summary>
    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        // Check if email is enabled (can be disabled in development)
        var emailEnabled = _configuration.GetValue<bool>("EmailSettings:Enabled", true);
        if (!emailEnabled)
        {
            _logger.LogWarning("Email sending is disabled. Would have sent to {Email}: {Subject}", toEmail, subject);
            return;
        }

        // Get SMTP configuration from appsettings.json
        var smtpHost = _configuration["EmailSettings:SmtpHost"];
        var smtpPort = _configuration.GetValue<int>("EmailSettings:SmtpPort", 587);
        var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
        var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
        var fromEmail = _configuration["EmailSettings:FromEmail"];
        var fromName = _configuration["EmailSettings:FromName"] ?? "Recipes App";

        // Validate configuration
        if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUsername) ||
            string.IsNullOrEmpty(smtpPassword) || string.IsNullOrEmpty(fromEmail))
        {
            _logger.LogError("Email settings are not configured properly. Check appsettings.json");
            throw new InvalidOperationException("Email service is not configured");
        }

        try
        {
            // Create email message
            using var message = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            // Configure SMTP client
            using var smtpClient = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword)
            };

            // Send email
            await smtpClient.SendMailAsync(message);
            _logger.LogInformation("Email sent successfully to {Email}: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}: {Subject}", toEmail, subject);
            throw new InvalidOperationException("Failed to send email", ex);
        }
    }
}
