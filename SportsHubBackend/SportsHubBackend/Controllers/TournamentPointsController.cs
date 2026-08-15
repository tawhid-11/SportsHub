using Dapper;
using Microsoft.AspNetCore.Mvc;
using SportsHubBackend.DBContext;
using SportsHubBackend.Model;
using System.Data;

namespace SportsHubBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TournamentPointsController : ControllerBase
    {
        private readonly DapperContext _context;

        public TournamentPointsController(DapperContext context)
        {
            _context = context;
        }

        [HttpGet("GetPointsTable")]
        public async Task<IActionResult> GetPointsTable(int tournamentId)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("Flag", 1);
                parameters.Add("TournamentID", tournamentId);

                using (var connection = _context.CreateConnection())
                {
                    var result = await connection.QueryAsync<TournamentPointTable>("SP_TournamentPointTable", parameters, commandType: CommandType.StoredProcedure);
                    return Ok(new
                    {
                        success = true,
                        message = "Points table fetched successfully",
                        data = result
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("UpdateStandings")]
        public async Task<IActionResult> UpdateStandings(int matchId)
        {
            try
            {
                using (var connection = _context.CreateConnection())
                {
                    // 1. Get match info (TournamentID, TeamA, TeamB, WinnerTeamID)
                    var matchInfo = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "SELECT ts.TournamentID, ts.TeamAID, ts.TeamBID, cm.WinnerTeamID, cm.MatchStatus " +
                        "FROM CricketMatch cm " +
                        "JOIN TeamSchedule ts ON cm.TeamScheduleID = ts.TeamScheduleID " +
                        "WHERE cm.CricketMatchID = @matchId", new { matchId });

                    if (matchInfo == null || matchInfo.TournamentID == null)
                    {
                        return BadRequest(new { success = false, message = "Tournament match not found" });
                    }

                    if (matchInfo.MatchStatus != "Finished")
                    {
                        return BadRequest(new { success = false, message = "Match is not finished yet" });
                    }

                    int tournamentId = (int)matchInfo.TournamentID;
                    int teamA = (int)matchInfo.TeamAID;
                    int teamB = (int)matchInfo.TeamBID;
                    int? winnerId = (int?)matchInfo.WinnerTeamID;

                    // 1.5 Get Match Stats for NRR
                    var statsQuery = @"
                        SELECT 
                            SUM(CASE WHEN p.TeamsID = @TeamAID THEN m.Run ELSE 0 END) as RunsA,
                            COUNT(CASE WHEN p.TeamsID = @TeamAID AND m.BallType != 'Wide' THEN 1 END) as BallsA,
                            SUM(CASE WHEN p.TeamsID = @TeamBID THEN m.Run ELSE 0 END) as RunsB,
                            COUNT(CASE WHEN p.TeamsID = @TeamBID AND m.BallType != 'Wide' THEN 1 END) as BallsB,
                            -- Opposing stats for conceded calculation
                            SUM(CASE WHEN p_bowl.TeamsID = @TeamAID THEN m.Run ELSE 0 END) as ConcA,
                            COUNT(CASE WHEN p_bowl.TeamsID = @TeamAID AND m.BallType NOT IN ('Wide', 'NoBall') THEN 1 END) as BowledA,
                            SUM(CASE WHEN p_bowl.TeamsID = @TeamBID THEN m.Run ELSE 0 END) as ConcB,
                            COUNT(CASE WHEN p_bowl.TeamsID = @TeamBID AND m.BallType NOT IN ('Wide', 'NoBall') THEN 1 END) as BowledB
                        FROM MatchBallByBall m
                        JOIN Overs o ON m.OverId = o.Id
                        JOIN Players p ON m.StrikerPlayerID = p.PlayerID
                        JOIN Players p_bowl ON m.BowlerPlayerID = p_bowl.PlayerID
                        WHERE o.CricketMatchID = @matchId";

                    var stats = await connection.QueryFirstOrDefaultAsync<dynamic>(statsQuery, new { matchId, TeamAID = teamA, TeamBID = teamB });

                    // 2. Update standings for Team A
                    var pA = new DynamicParameters();
                    pA.Add("Flag", 2);
                    pA.Add("TournamentID", tournamentId);
                    pA.Add("TeamsID", teamA);
                    pA.Add("WinnerTeamID", winnerId);
                    pA.Add("RunsScored", (int)(stats?.RunsA ?? 0));
                    pA.Add("BallsFaced", (int)(stats?.BallsA ?? 0));
                    pA.Add("RunsConceded", (int)(stats?.ConcA ?? 0));
                    pA.Add("BallsBowled", (int)(stats?.BowledA ?? 0));
                    await connection.ExecuteAsync("SP_TournamentPointTable", pA, commandType: CommandType.StoredProcedure);

                    // 3. Update standings for Team B
                    var pB = new DynamicParameters();
                    pB.Add("Flag", 2);
                    pB.Add("TournamentID", tournamentId);
                    pB.Add("TeamsID", teamB);
                    pB.Add("WinnerTeamID", winnerId);
                    pB.Add("RunsScored", (int)(stats?.RunsB ?? 0));
                    pB.Add("BallsFaced", (int)(stats?.BallsB ?? 0));
                    pB.Add("RunsConceded", (int)(stats?.ConcB ?? 0));
                    pB.Add("BallsBowled", (int)(stats?.BowledB ?? 0));
                    await connection.ExecuteAsync("SP_TournamentPointTable", pB, commandType: CommandType.StoredProcedure);

                    return Ok(new { success = true, message = "Standings and NRR updated successfully" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
