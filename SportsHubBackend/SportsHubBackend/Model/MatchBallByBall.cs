namespace SportsHubBackend.Model
{
    public class MatchBallByBall
    {
        public int BallID { get; set; }
        public int OverId { get; set; }
        public int StrikerPlayerID { get; set; }
        public int NonStrikerPlayerID { get; set; }
        public int BowlerPlayerID { get; set; }
        public int Run { get; set; }
        public bool IsWicket { get; set; }
        public string? BallType { get; set; } // "Normal", "Wide", "NoBall"
        public string? WicketType { get; set; }
        public int? PlayerOutID { get; set; }
        public bool IsBoundary { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
