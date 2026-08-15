using System.Collections.Generic;

namespace SportsHubBackend.Model
{
    public class BallInputDto
    {
        public int CricketMatchID { get; set; }
        public int StrikerPlayerID { get; set; }
        public int NonStrikerPlayerID { get; set; }
        public int BowlerPlayerID { get; set; }
        public int Run { get; set; }
        public bool IsWicket { get; set; }
        public bool IsBye { get; set; }
        public string? BallType { get; set; } = "Normal";
        public string? WicketType { get; set; }
        public int? PlayerOutID { get; set; }
    }

    public class MatchStatsDto
    {
        public int CricketMatchID { get; set; }
        public int TotalRuns { get; set; }
        public int Wickets { get; set; }
        public string Overs { get; set; }
        public int? Target { get; set; }
        public int CurrentInnings { get; set; }
        public double CRR { get; set; }
        public double RRR { get; set; }
        public string TeamAName { get; set; }
        public string TeamBName { get; set; }
        public string BattingTeamName { get; set; }
        public string MatchStatus { get; set; }
        public List<string> RecentBalls { get; set; } = new List<string>();
        public BatsmanStatsDto StrikerStats { get; set; }
        public BatsmanStatsDto NonStrikerStats { get; set; }
        public BowlerStatsDto BowlerStats { get; set; }
        public List<int>? OutPlayerIds { get; set; }
        public string? WinnerMessage { get; set; }
        public int? Innings1TotalRuns { get; set; }
        public int? Innings1TotalWickets { get; set; }
        public string? Innings1TeamName { get; set; }
        public string? Innings2TeamName { get; set; }
    }

    public class BatsmanStatsDto
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public string PlayerImage { get; set; }
        public int Runs { get; set; }
        public int Balls { get; set; }
        public int Fours { get; set; }
        public int Sixes { get; set; }
        public double StrikeRate { get; set; }
    }

    public class UpdateMatchPlayersDto
    {
        public int MatchId { get; set; }
        public int StrikerId { get; set; }
        public int NonStrikerId { get; set; }
        public int BowlerId { get; set; }
    }

    public class ChangeBowlerDto
    {
        public int MatchId { get; set; }
        public int BowlerId { get; set; }
    }

    public class BowlerStatsDto
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public string PlayerImage { get; set; }
        public string Overs { get; set; }
        public int Maidens { get; set; }
        public int Runs { get; set; }
        public int Wickets { get; set; }
        public double Economy { get; set; }
    }

    public class MatchSummaryDto
    {
        public int MatchId { get; set; }
        public MatchStatsDto Innings1 { get; set; }
        public MatchStatsDto Innings2 { get; set; }
        public string Innings1TeamName { get; set; }
        public string Innings2TeamName { get; set; }
        public string? WinnerMessage { get; set; }
    }

    public class FullScorecardDto
    {
        public int MatchId { get; set; }
        public InningsScorecardDto Innings1 { get; set; }
        public InningsScorecardDto Innings2 { get; set; }
    }

    public class InningsScorecardDto
    {
        public string TeamName { get; set; }
        public int TotalRuns { get; set; }
        public int Wickets { get; set; }
        public string Overs { get; set; }
        public List<BatsmanScorecardDto> Batting { get; set; } = new List<BatsmanScorecardDto>();
        public List<BowlerScorecardDto> Bowling { get; set; } = new List<BowlerScorecardDto>();
        public List<FallOfWicketDto> FallOfWickets { get; set; } = new List<FallOfWicketDto>();
    }

    public class BatsmanScorecardDto : BatsmanStatsDto
    {
        public string Dismissal { get; set; }
        public string OutStatus { get; set; }
    }

    public class BowlerScorecardDto : BowlerStatsDto { }

    public class FallOfWicketDto
    {
        public string PlayerName { get; set; }
        public int Runs { get; set; }
        public int WicketNumber { get; set; }
        public string Over { get; set; }
    }

    public class SquadDto
    {
        public string TeamAName { get; set; }
        public string TeamBName { get; set; }
        public int MatchPlayer { get; set; }
        public int ExtraPlayer { get; set; }
        public List<MatchSquadDto> TeamAPlayers { get; set; } = new List<MatchSquadDto>();
        public List<MatchSquadDto> TeamBPlayers { get; set; } = new List<MatchSquadDto>();
    }

    public class MatchSquadDto
    {
        public int PlayerId { get; set; }
        public string FullName { get; set; }
        public string PlayerImage { get; set; }
        public string RoleName { get; set; }
        public bool IsPlaying { get; set; }
        public bool IsCaptain { get; set; }
        public bool IsWicketKeeper { get; set; }
    }

    public class SaveSquadRequestDto
    {
        public int CricketMatchID { get; set; }
        public int TeamID { get; set; }
        public List<MatchSquadDto> Players { get; set; }
    }
}
