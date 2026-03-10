namespace SportsHubBackend.Model
{
    public class CricketMatch
    {
        public int? CricketMatchID { get; set; }
        public int TeamScheduleID { get; set; }
        public int? TossWinnerTeamID { get; set; }
        public string? TossChoice { get; set; } // Bat or Ball
        public int? Overs { get; set; }
        public string? Umpire { get; set; }
        public string? Venue { get; set; }
        public int? StrikerPlayerID { get; set; }
        public int? NonStrikerPlayerID { get; set; }
        public int? BowlerPlayerID { get; set; }
        public int CurrentInnings { get; set; } = 1;
        public string? MatchStatus { get; set; }
    }
}
