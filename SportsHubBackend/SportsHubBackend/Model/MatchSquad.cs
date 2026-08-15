namespace SportsHubBackend.Model
{
    public class MatchSquad
    {
        public int MatchSquadID { get; set; }
        public int CricketMatchID { get; set; }
        public int TeamID { get; set; }
        public int PlayerID { get; set; }
        public bool IsPlaying { get; set; } = true;
        public bool IsCaptain { get; set; } = false;
        public bool IsWicketKeeper { get; set; } = false;
    }
}
