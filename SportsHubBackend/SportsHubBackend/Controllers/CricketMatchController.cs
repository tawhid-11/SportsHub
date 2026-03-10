using Dapper;
using Microsoft.AspNetCore.Mvc;
using SportsHubBackend.DBContext;
using SportsHubBackend.Model;
using System.Data;

namespace SportsHubBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CricketMatchController : ControllerBase
    {
        private readonly DapperContext _context;

        public CricketMatchController(DapperContext context)
        {
            _context = context;
        }

        // GET: api/CricketMatch/GetAll
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var parameter = new DynamicParameters();
                parameter.Add("@Flag", 4); // Flag 4 = select all in SP

                using (var connection = _context.CreateConnection())
                {
                    var result = await connection.QueryAsync<dynamic>(
                        "SP_CricketMatch",
                        parameter,
                        commandType: CommandType.StoredProcedure
                    );

                    return Ok(new
                    {
                        success = true,
                        Message = "All cricket matches fetched successfully",
                        Data = result
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, Message = "Error - " + ex.Message });
            }
        }

        // GET: api/CricketMatch/GetById?CricketMatchID=1
        [HttpGet("GetById")]
        public async Task<IActionResult> GetById(int CricketMatchID)
        {
            try
            {
                var parameter = new DynamicParameters();
                parameter.Add("@Flag", 5); // Flag 5 = select by id
                parameter.Add("@CricketMatchID", CricketMatchID);

                using (var connection = _context.CreateConnection())
                {
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "SP_CricketMatch",
                        parameter,
                        commandType: CommandType.StoredProcedure
                    );

                    return Ok(new
                    {
                        success = true,
                        Message = "Cricket match fetched successfully",
                        Data = result
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, Message = "Error - " + ex.Message });
            }
        }

        // POST: api/CricketMatch/Insert
        [HttpPost("Insert")]
        public async Task<IActionResult> Insert([FromBody] CricketMatch match)
        {
            try
            {
                var parameter = new DynamicParameters();
                parameter.Add("@Flag", 1); // Flag 1 = insert
                parameter.Add("@TeamScheduleID", match.TeamScheduleID);
                parameter.Add("@TossWinnerTeamID", match.TossWinnerTeamID);
                parameter.Add("@TossChoice", match.TossChoice);
                parameter.Add("@Overs", match.Overs);
                parameter.Add("@Umpire", match.Umpire);
                parameter.Add("@Venue", match.Venue);

                using (var connection = _context.CreateConnection())
                {
                    var result = await connection.QueryAsync<dynamic>(
                        "SP_CricketMatch",
                        parameter,
                        commandType: CommandType.StoredProcedure
                    );

                    return Ok(new
                    {
                        success = true,
                        Message = "Cricket match inserted successfully",
                        Data = result
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, Message = "Error - " + ex.Message });
            }
        }

        // PUT: api/CricketMatch/Update
        [HttpPut("Update")]
        public async Task<IActionResult> Update([FromBody] CricketMatch match)
        {
            try
            {
                var parameter = new DynamicParameters();
                parameter.Add("@Flag", 2); // Flag 2 = update
                parameter.Add("@CricketMatchID", match.CricketMatchID);
                parameter.Add("@TeamScheduleID", match.TeamScheduleID);
                parameter.Add("@TossWinnerTeamID", match.TossWinnerTeamID);
                parameter.Add("@TossChoice", match.TossChoice);
                parameter.Add("@Overs", match.Overs);
                parameter.Add("@Umpire", match.Umpire);
                parameter.Add("@Venue", match.Venue);

                using (var connection = _context.CreateConnection())
                {
                    var result = await connection.QueryAsync<dynamic>(
                        "SP_CricketMatch",
                        parameter,
                        commandType: CommandType.StoredProcedure
                    );

                    return Ok(new
                    {
                        success = true,
                        Message = "Cricket match updated successfully",
                        Data = result
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, Message = "Error - " + ex.Message });
            }
        }

        // DELETE: api/CricketMatch/Delete?CricketMatchID=1
        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete(int CricketMatchID)
        {
            try
            {
                var parameter = new DynamicParameters();
                parameter.Add("@Flag", 3); // Flag 3 = delete
                parameter.Add("@CricketMatchID", CricketMatchID);

                using (var connection = _context.CreateConnection())
                {
                    var result = await connection.QueryAsync<dynamic>(
                        "SP_CricketMatch",
                        parameter,
                        commandType: CommandType.StoredProcedure
                    );

                    return Ok(new
                    {
                        success = true,
                        Message = "Cricket match deleted successfully",
                        Data = result
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, Message = "Error - " + ex.Message });
            }
        }
        [HttpGet("GetByTeamScheduleId")]
        public async Task<IActionResult> GetByTeamScheduleId(int teamScheduleId)
        {
            try
            {
                var parameter = new DynamicParameters();
                parameter.Add("@Flag", 6); // Flag 5 = select by id
                parameter.Add("@TeamScheduleID", teamScheduleId);

                using (var connection = _context.CreateConnection())
                {
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "SP_CricketMatch",
                        parameter,
                        commandType: CommandType.StoredProcedure
                    );

                    return Ok(new
                    {
                        success = true,
                        Message = "Cricket match fetched successfully",
                        Data = result
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, Message = "Error - " + ex.Message });
            }
        }
        [HttpPut("UpdatePlayersByCricketMatchID")]
        public async Task<IActionResult> UpdateLivePlayers(CricketMatch model)
        {
            try
            {
                var parameter = new DynamicParameters();
                parameter.Add("@Flag", 6);
                parameter.Add("@CricketMatchID", model.CricketMatchID);
                parameter.Add("@StrikerPlayerID", model.StrikerPlayerID);
                parameter.Add("@NonStrikerPlayerID", model.NonStrikerPlayerID);
                parameter.Add("@BowlerPlayerID", model.BowlerPlayerID);


                using (var connection = _context.CreateConnection())
                {
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "SP_CricketMatch",
                        parameter,
                        commandType: CommandType.StoredProcedure
                    );

                    return Ok(new
                    {
                        success = true,
                        Message = "Players updated successfully",
                        Data = result
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    Message = "Error - " + ex.Message
                });
            }
        }
        [HttpGet("GetAllLiveMatch")]
        public async Task<IActionResult> GetAllLiveMatch()
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Flag", 8);

                using (var connection = _context.CreateConnection())
                {
                    var matches = await connection.QueryAsync<dynamic>(
                        "SP_CricketMatch",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    var liveMatches = matches.Where(m => m.MatchStatus != "Finished").ToList();
                    var results = new List<object>();

                    foreach (var match in liveMatches)
                    {
                        int matchId = (int)match.CricketMatchID;
                        
                        // Get complete match stats from LiveMatchController logic
                        var matchStatsSql = @"
                            SELECT 
                                cm.StrikerPlayerID, cm.NonStrikerPlayerID, cm.BowlerPlayerID, 
                                cm.CurrentInnings, cm.MatchStatus,
                                ts.TeamAID, ts.TeamBID, ta.TeamName as TeamAName, tb.TeamName as TeamBName,
                                ta.TeamLogo as TeamALogo, tb.TeamLogo as TeamBLogo,
                                cm.TossWinnerTeamID, cm.TossChoice, cm.Overs as MatchOvers
                            FROM CricketMatch cm
                            JOIN TeamSchedule ts ON cm.TeamScheduleID = ts.TeamScheduleID
                            LEFT JOIN Teams ta ON ts.TeamAID = ta.TeamsID
                            LEFT JOIN Teams tb ON ts.TeamBID = tb.TeamsID
                            WHERE cm.CricketMatchID = @MatchId";
                        
                        var matchInfo = await connection.QueryFirstOrDefaultAsync<dynamic>(matchStatsSql, new { MatchId = matchId });
                        
                        if (matchInfo == null) continue;

                        int currentInnings = matchInfo.CurrentInnings ?? 1;
                        string matchStatus = matchInfo.MatchStatus ?? "Live";
                        
                        // Get current innings stats
                        var totalSql = @"
                            SELECT 
                                SUM(Run) as TotalRuns,
                                COUNT(CASE WHEN IsWicket = 1 THEN 1 END) as TotalWickets,
                                (SELECT COUNT(*) FROM MatchBallByBall b JOIN Overs o ON b.OverId = o.Id 
                                 WHERE o.CricketMatchID = @MatchId AND o.Innings = @Innings AND b.BallType NOT IN ('Wide', 'NoBall')) as TotalBalls
                            FROM MatchBallByBall b
                            JOIN Overs o ON b.OverId = o.Id
                            WHERE o.CricketMatchID = @MatchId AND o.Innings = @Innings";
                        
                        var totalStats = await connection.QueryFirstOrDefaultAsync<dynamic>(totalSql, new { MatchId = matchId, Innings = currentInnings });
                        
                        int totalBalls = totalStats?.TotalBalls ?? 0;
                        int overs = totalBalls / 6;
                        int balls = totalBalls % 6;
                        string overStr = $"{overs}.{balls}";
                        int totalRuns = totalStats?.TotalRuns ?? 0;
                        int totalWickets = totalStats?.TotalWickets ?? 0;

                        // Determine innings team names
                        int teamAId = matchInfo.TeamAID ?? 0;
                        int tossWinnerId = matchInfo.TossWinnerTeamID ?? 0;
                        string tossChoice = matchInfo.TossChoice ?? "Bat";
                        string teamAName = matchInfo.TeamAName ?? "Team A";
                        string teamBName = matchInfo.TeamBName ?? "Team B";
                        
                        string innings1Team, innings2Team;
                        if (tossWinnerId == 0) {
                            innings1Team = teamAName;
                            innings2Team = teamBName;
                        } else if (tossChoice == "Bat") {
                            innings1Team = (tossWinnerId == teamAId) ? teamAName : teamBName;
                            innings2Team = (tossWinnerId == teamAId) ? teamBName : teamAName;
                        } else {
                            innings1Team = (tossWinnerId == teamAId) ? teamBName : teamAName;
                            innings2Team = (tossWinnerId == teamAId) ? teamAName : teamBName;
                        }
                        
                        string battingTeamName = (currentInnings == 1) ? innings1Team : innings2Team;

                        // Get innings 1 stats if in 2nd innings
                        int? innings1Runs = null;
                        int? innings1Wickets = null;
                        int? target = null;
                        
                        if (currentInnings == 2)
                        {
                            var innings1Stats = await connection.QueryFirstOrDefaultAsync<dynamic>(totalSql, new { MatchId = matchId, Innings = 1 });
                            innings1Runs = innings1Stats?.TotalRuns ?? 0;
                            innings1Wickets = innings1Stats?.TotalWickets ?? 0;
                            target = innings1Runs + 1;
                        }

                        results.Add(new
                        {
                            CricketMatchID = matchId,
                            TeamAName = teamAName,
                            TeamALogo = matchInfo.TeamALogo,
                            TeamBName = teamBName,
                            TeamBLogo = matchInfo.TeamBLogo,
                            TotalRun = totalRuns,
                            Wicket = totalWickets,
                            Overs = overStr,
                            MatchStatus = matchStatus,
                            CurrentInnings = currentInnings,
                            BattingTeamName = battingTeamName,
                            Innings1TeamName = innings1Team,
                            Innings2TeamName = innings2Team,
                            Innings1TotalRuns = innings1Runs,
                            Innings1TotalWickets = innings1Wickets,
                            Target = target,
                            MatchOvers = matchInfo.MatchOvers ?? 20
                        });
                    }

                    return Ok(new
                    {
                        success = true,
                        Message = "Live matches retrieved successfully",
                        Data = results
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Error - " + ex.Message
                });
            }
        }


    }
}
