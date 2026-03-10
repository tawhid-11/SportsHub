namespace SportsHubBackend.Model
{
    public class Teams
    {
        public int TeamsID { get; set; }
        public required string TeamName { get; set; }
        public int UserId { get; set; }
        public required string ShortName { get; set; }
        public  IFormFile? TeamLogo { get; set; }
        public required string TeamOwnerName { get; set; }
        public required string TeamOwnerEmail { get; set; }
        public required string TeamOwnerPhoneNumber { get; set; }
        public required string CoachName { get; set; }
        public int FoundedYear { get; set; }
        public int TotalPlayers { get; set; }
        public bool IsActive { get; set; }




    }
}
