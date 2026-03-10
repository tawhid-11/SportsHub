using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace SportsHubBackend.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendTeamCreationEmailAsync(string toEmail, string teamOwnerName, string teamName, string email, string password)
        {
            try
            {
                var smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"] ?? "";
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"] ?? "";
                var fromEmail = _configuration["EmailSettings:FromEmail"] ?? smtpUsername;
                var fromName = _configuration["EmailSettings:FromName"] ?? "SportsHub";

                using (var client = new SmtpClient(smtpHost, smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(fromEmail, fromName),
                        Subject = "Team Created Successfully - SportsHub",
                        Body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 30px; border-radius: 0 0 5px 5px; }}
        .credentials {{ background-color: #fff; padding: 20px; margin: 20px 0; border-left: 4px solid #4CAF50; }}
        .credentials h3 {{ margin-top: 0; color: #4CAF50; }}
        .credentials p {{ margin: 10px 0; }}
        .credentials strong {{ color: #333; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🎉 Team Created Successfully!</h1>
        </div>
        <div class=""content"">
            <p>Dear <strong>{teamOwnerName}</strong>,</p>
            
            <p>Congratulations! Your team <strong>""{teamName}""</strong> has been successfully created in SportsHub.</p>
            
            <p>You can now manage your team, register for tournaments, and participate in various cricket events.</p>
            
            <div class=""credentials"">
                <h3>🔐 Your Login Credentials</h3>
                <p><strong>Email:</strong> {email}</p>
                <p><strong>Password:</strong> {password}</p>
            </div>
            
            <p>Please keep these credentials safe and secure. You can use them to log in to your SportsHub account.</p>
            
            <p>If you have any questions or need assistance, please don't hesitate to contact our support team.</p>
            
            <p>Best regards,<br>
            <strong>SportsHub Team</strong></p>
        </div>
        <div class=""footer"">
            <p>This is an automated email. Please do not reply to this message.</p>
        </div>
    </div>
</body>
</html>",
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);
                    _logger.LogInformation($"Email sent successfully to {toEmail}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send email to {toEmail}: {ex.Message}");
                return false;
            }
        }
    }
}
