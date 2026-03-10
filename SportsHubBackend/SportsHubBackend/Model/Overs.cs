namespace SportsHubBackend.Model
{
    public class Overs
    {
        public int Id { get; set; }
        public int CricketMatchID { get; set; }
        public int BowlerId { get; set; }
        public int Innings { get; set; } // 1 or 2
        public int OverNumber { get; set; }
    }
}
