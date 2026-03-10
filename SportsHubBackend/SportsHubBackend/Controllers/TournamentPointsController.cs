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

                    // 2. Update standings for Team A
                    var pA = new DynamicParameters();
                    pA.Add("Flag", 2);
                    pA.Add("TournamentID", tournamentId);
                    pA.Add("TeamsID", teamA);
                    pA.Add("WinnerTeamID", winnerId);
                    await connection.ExecuteAsync("SP_TournamentPointTable", pA, commandType: CommandType.StoredProcedure);

                    // 3. Update standings for Team B
                    var pB = new DynamicParameters();
                    pB.Add("Flag", 2);
                    pB.Add("TournamentID", tournamentId);
                    pB.Add("TeamsID", teamB);
                    pB.Add("WinnerTeamID", winnerId);
                    await connection.ExecuteAsync("SP_TournamentPointTable", pB, commandType: CommandType.StoredProcedure);

                    return Ok(new { success = true, message = "Standings updated successfully" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
