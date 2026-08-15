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

        public async Task<bool> SendPlayerInvitationEmailAsync(string toEmail, string playerName, string teamName, string email, string password)
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
                        Subject = "Welcome to SportsHub - You've been added to a team!",
                        Body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #2196F3; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 30px; border-radius: 0 0 5px 5px; }}
        .credentials {{ background-color: #fff; padding: 20px; margin: 20px 0; border-left: 4px solid #2196F3; }}
        .credentials h3 {{ margin-top: 0; color: #2196F3; }}
        .credentials p {{ margin: 10px 0; }}
        .credentials strong {{ color: #333; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🏏 Welcome to SportsHub!</h1>
        </div>
        <div class=""content"">
            <p>Dear <strong>{playerName}</strong>,</p>
            
            <p>You have been added as a player to the team <strong>""{teamName}""</strong> in SportsHub.</p>
            
            <p>You can now log in to your dashboard to view your match statistics, team details, and more.</p>
            
            <div class=""credentials"">
                <h3>🔐 Your Login Credentials</h3>
                <p><strong>Email:</strong> {email}</p>
                <p><strong>Password:</strong> {password}</p>
            </div>
            
            <p>Please log in and complete your profile. Best of luck for your upcoming matches!</p>
            
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
                    _logger.LogInformation($"Invitation email sent successfully to {toEmail}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send invitation email to {toEmail}: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> SendOTPEmailAsync(string toEmail, string userName, string otp)
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
                        Subject = "Your Password Reset OTP - SportsHub",
                        Body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; line-height: 1.6; color: #1e293b; background-color: #f8fafc; }}
        .container {{ max-width: 500px; margin: 40px auto; padding: 0; background: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 25px rgba(0,0,0,0.05); }}
        .header {{ background: linear-gradient(135deg, #0ea5e9 0%, #2563eb 100%); color: white; padding: 40px 20px; text-align: center; }}
        .content {{ padding: 40px; text-align: center; }}
        .otp-box {{ background: #f1f5f9; padding: 20px; font-size: 32px; font-weight: 800; letter-spacing: 12px; color: #0f172a; border-radius: 12px; margin: 30px 0; border: 1px solid #e2e8f0; }}
        .info {{ color: #64748b; font-size: 14px; margin-top: 20px; }}
        .footer {{ text-align: center; padding: 20px; color: #94a3b8; font-size: 12px; border-top: 1px solid #f1f5f9; }}
        h2 {{ margin: 0; font-size: 24px; font-weight: 700; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h2>Verify Your Identity</h2>
        </div>
        <div class=""content"">
            <p>Hi <strong>{userName}</strong>,</p>
            <p>You recently requested to reset your SportsHub account password. Use the following OTP to proceed:</p>
            
            <div class=""otp-box"">{otp}</div>
            
            <p>This code is valid for <strong>10 minutes</strong>. If you didn't request this, you can safely ignore this email.</p>
            
            <div class=""info"">
                For security, never share this OTP with anyone, including SportsHub staff.
            </div>
        </div>
        <div class=""footer"">
            <p>&copy; {DateTime.Now.Year} SportsHub Foundation. All rights reserved.</p>
        </div>
    </div>
</body>
</html>",
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);
                    _logger.LogInformation($"OTP Email sent successfully to {toEmail}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send OTP email to {toEmail}: {ex.Message}");
                return false;
            }
        }
    }
}
