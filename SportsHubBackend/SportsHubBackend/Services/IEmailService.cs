namespace SportsHubBackend.Services
{
    public interface IEmailService
    {
        Task<bool> SendTeamCreationEmailAsync(string toEmail, string teamOwnerName, string teamName, string email, string password);
        Task<bool> SendPlayerInvitationEmailAsync(string toEmail, string playerName, string teamName, string email, string password);
        Task<bool> SendOTPEmailAsync(string toEmail, string userName, string otp);
    }
}
