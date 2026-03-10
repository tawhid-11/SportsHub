namespace SportsHubBackend.Model
{
    public class TournamentPointTable
    {
        public int PointTableID { get; set; }
        public int TournamentID { get; set; }
        public int TeamsID { get; set; }
        public int Played { get; set; }
        public int Won { get; set; }
        public int Lost { get; set; }
        public int Draw { get; set; }
        public int NR { get; set; }
        public int Points { get; set; }
        public decimal NRR { get; set; }
        public int TotalRunsScored { get; set; }
        public int TotalBallsFaced { get; set; }
        public int TotalRunsConceded { get; set; }
        public int TotalBallsBowled { get; set; }
        
        // Navigation / Extension properties
        public string? TeamName { get; set; }
        public string? TeamLogo { get; set; }
    }
}
