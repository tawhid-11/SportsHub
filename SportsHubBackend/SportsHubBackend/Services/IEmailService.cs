namespace SportsHubBackend.Services
{
    public interface IEmailService
    {
        Task<bool> SendTeamCreationEmailAsync(string toEmail, string teamOwnerName, string teamName, string email, string password);
    }
}
