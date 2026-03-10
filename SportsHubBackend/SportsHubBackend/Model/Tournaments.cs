namespace SportsHubBackend.Model
{
    public class Tournaments
    {
  
        public required string TournamentName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public required string Location { get; set; }   
        public required int TournamentTypeID { get; set; }
        public DateTime RegistrationDeadline { get; set; }
        public int TotalPlayer { get; set; }
        public int MatchPlayer { get; set; }
        public int ExtraPlayer { get; set; }
        public string? Status { get; set; }
        public int RegistrationFee { get; set; }
        public int FieldFee { get; set; }
        public int MaxTeams { get; set; }
        public string? ContactNumber { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public  bool? IsActive { get; set; }
        public string? Prize { get; set; }
        
    }
}
