using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SportsHubBackend.DBContext;
using SportsHubBackend.Hubs;
using SportsHubBackend.Model;
using System.Data;

namespace SportsHubBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LiveMatchController : ControllerBase
    {
        private readonly DapperContext _context;
        private readonly IHubContext<SignalRHub> _hubContext;

        public LiveMatchController(DapperContext context, IHubContext<SignalRHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPost("AddBall")]
        public async Task<IActionResult> AddBall([FromBody] BallInputDto input)
        {
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Get or Create Current Over
                        var currentOver = await GetOrCreateOver(connection, transaction, input.CricketMatchID, input.BowlerPlayerID);

                        // 2. Insert Ball (Flag 29)
                        var p29 = new DynamicParameters();
                        p29.Add("Flag", 29);
                        p29.Add("OverId", currentOver.Id);
                        p29.Add("StrikerId", input.StrikerPlayerID);
                        p29.Add("NonStrikerId", input.NonStrikerPlayerID);
                        p29.Add("BowlerId", input.BowlerPlayerID);
                        p29.Add("Run", input.Run);
                        p29.Add("IsWicket", input.IsWicket);
                        p29.Add("BallType", input.BallType);
                        p29.Add("WicketType", input.WicketType);
                        p29.Add("PlayerOutId", input.PlayerOutID);
                        p29.Add("IsBye", input.IsBye);
                        await connection.ExecuteAsync("SP_LiveMatch", p29, transaction, commandType: CommandType.StoredProcedure);

                        // 2b. Sync Current Players to CricketMatch Table
                        var syncParams = new DynamicParameters();
                        syncParams.Add("Flag", 7);
                        syncParams.Add("MatchId", input.CricketMatchID);
                        syncParams.Add("StrikerId", input.StrikerPlayerID);
                        syncParams.Add("NonStrikerId", input.NonStrikerPlayerID);
                        syncParams.Add("BowlerId", input.BowlerPlayerID);
                        await connection.ExecuteAsync("SP_LiveMatch", syncParams, transaction, commandType: CommandType.StoredProcedure);

                        // 2c. Reset status from 'Innings Break' to 'Live' if in 2nd innings
                        var statusParams = new DynamicParameters();
                        statusParams.Add("Flag", 1);
                        statusParams.Add("MatchId", input.CricketMatchID);
                        var matchStatusCheck = await connection.QueryFirstOrDefaultAsync<dynamic>("SP_LiveMatch", statusParams, transaction, commandType: CommandType.StoredProcedure);
                        
                        int checkInnings = matchStatusCheck?.CurrentInnings ?? 1;
                        string checkStatus = matchStatusCheck?.MatchStatus ?? "Live";
                        
                        if (checkInnings == 2 && checkStatus == "Innings Break")
                        {
                            var updateStatusParams = new DynamicParameters();
                            updateStatusParams.Add("Flag", 2);
                            updateStatusParams.Add("MatchId", input.CricketMatchID);
                            updateStatusParams.Add("MatchStatus", "Live");
                            await connection.ExecuteAsync("SP_LiveMatch", updateStatusParams, transaction, commandType: CommandType.StoredProcedure);
                        }


                        // 3. Update Match Stats (Implementation of Score Calculation)
                        var matchStats = await GetMatchStats(connection, transaction, input.CricketMatchID);

                        // 4. Check for End of Innings
                        var matchInfoParams = new DynamicParameters();
                        matchInfoParams.Add("Flag", 3);
                        matchInfoParams.Add("MatchId", input.CricketMatchID);
                        var matchInfo = await connection.QueryFirstOrDefaultAsync<dynamic>("SP_LiveMatch", matchInfoParams, transaction, commandType: CommandType.StoredProcedure);

                        int currentInnings = matchInfo?.CurrentInnings ?? 1;
                        int matchOvers = (int)(matchInfo?.MatchOvers ?? 0);
                        int totalWickets = matchStats.Wickets;
                        
                        // Detect end of innings (all out or overs done)
                        bool isAllOut = totalWickets >= 10; // Assuming 11 players
                        
                        // Parse TotalBalls from matchStats.Overs (e.g., "1.2" -> 8)
                        string[] overParts = matchStats.Overs.Split('.');
                        int completedOvers = int.Parse(overParts[0]);
                        int ballsInOver = overParts.Length > 1 ? int.Parse(overParts[1]) : 0;
                        int totalBalls = (completedOvers * 6) + ballsInOver;

                        bool isOversComplete = totalBalls >= (matchOvers * 6);

                        bool isTargetReached = currentInnings == 2 && matchStats.Target.HasValue && matchStats.TotalRuns >= matchStats.Target.Value;

                        if ((isAllOut || isOversComplete) && currentInnings == 1)
                        {
                            // Transition to 2nd Innings
                            var transitionParams = new DynamicParameters();
                            transitionParams.Add("Flag", 4);
                            transitionParams.Add("MatchId", input.CricketMatchID);
                            transitionParams.Add("Innings", 2);
                            transitionParams.Add("MatchStatus", "Innings Break");
                            await connection.ExecuteAsync("SP_LiveMatch", transitionParams, transaction, commandType: CommandType.StoredProcedure);
                        }
                        else if ((isAllOut || isOversComplete || isTargetReached) && currentInnings == 2)
                        {
                            // Determine Winner
                            int? winnerId = await GetWinnerTeamIdInternal(connection, transaction, input.CricketMatchID);
                            
                            // End of Match
                            var finishParams = new DynamicParameters();
                            finishParams.Add("Flag", 5);
                            finishParams.Add("MatchId", input.CricketMatchID);
                            finishParams.Add("WinnerId", winnerId);
                            await connection.ExecuteAsync("SP_LiveMatch", finishParams, transaction, commandType: CommandType.StoredProcedure);
                            
                            // Update Tournament Standings if it's a tournament match
                            var tourMatchParams = new DynamicParameters();
                            tourMatchParams.Add("Flag", 6);
                            tourMatchParams.Add("MatchId", input.CricketMatchID);
                            var tournamentMatch = await connection.QueryFirstOrDefaultAsync<dynamic>("SP_LiveMatch", tourMatchParams, transaction, commandType: CommandType.StoredProcedure);

                            if (tournamentMatch != null && tournamentMatch.TournamentID != null)
                            {
                                int tournamentId = (int)tournamentMatch.TournamentID;
                                int teamA = (int)tournamentMatch.TeamAID;
                                int teamB = (int)tournamentMatch.TeamBID;
                                int tossWinnerId = tournamentMatch.TossWinnerTeamID != null ? (int)tournamentMatch.TossWinnerTeamID : 0;
                                string tossChoice = tournamentMatch.TossChoice ?? "Bat";
                                int tournamentOvers = tournamentMatch.Overs != null ? (int)tournamentMatch.Overs : 20;
                                bool isDraw = (winnerId == null);

                                // Determine which team batted first
                                int innings1TeamId, innings2TeamId;
                                if (tossWinnerId == 0) {
                                    innings1TeamId = teamA;
                                    innings2TeamId = teamB;
                                } else if (tossChoice == "Bat") {
                                    innings1TeamId = (tossWinnerId == teamA) ? teamA : teamB;
                                    innings2TeamId = (tossWinnerId == teamA) ? teamB : teamA;
                                } else {
                                    innings1TeamId = (tossWinnerId == teamA) ? teamB : teamA;
                                    innings2TeamId = (tossWinnerId == teamA) ? teamA : teamB;
                                }

                                // Get innings 1 stats
                                var innings1Stats = await GetMatchStatsInnings(connection, transaction, input.CricketMatchID, 1);
                                int innings1Runs = innings1Stats?.TotalRuns ?? 0;
                                int innings1Wickets = innings1Stats?.Wickets ?? 0;
                                int innings1Balls = 0;
                                
                                // Parse overs to get balls
                                if (!string.IsNullOrEmpty(innings1Stats?.Overs))
                                {
                                    var oversParts = innings1Stats.Overs.Split('.');
                                    if (oversParts.Length == 2)
                                    {
                                        int oversCount = int.Parse(oversParts[0]);
                                        int balls = int.Parse(oversParts[1]);
                                        innings1Balls = (oversCount * 6) + balls;
                                    }
                                }
                                
                                // If all out, use full quota
                                if (innings1Wickets >= 10)
                                {
                                    innings1Balls = tournamentOvers * 6;
                                }

                                // Get innings 2 stats
                                var innings2Stats = await GetMatchStatsInnings(connection, transaction, input.CricketMatchID, 2);
                                int innings2Runs = innings2Stats?.TotalRuns ?? 0;
                                int innings2Wickets = innings2Stats?.Wickets ?? 0;
                                int innings2Balls = 0;
                                
                                if (!string.IsNullOrEmpty(innings2Stats?.Overs))
                                {
                                    var oversParts = innings2Stats.Overs.Split('.');
                                    if (oversParts.Length == 2)
                                    {
                                        int oversCount = int.Parse(oversParts[0]);
                                        int balls = int.Parse(oversParts[1]);
                                        innings2Balls = (oversCount * 6) + balls;
                                    }
                                }
                                
                                // If all out, use full quota
                                if (innings2Wickets >= 10)
                                {
                                    innings2Balls = tournamentOvers * 6;
                                }


                                // Update for innings 1 team
                                var pInnings1 = new DynamicParameters();
                                pInnings1.Add("Flag", 2);
                                pInnings1.Add("TournamentID", tournamentId);
                                pInnings1.Add("TeamsID", innings1TeamId);
                                pInnings1.Add("WinnerTeamID", winnerId);
                                pInnings1.Add("IsDraw", isDraw);
                                pInnings1.Add("RunsScored", innings1Runs);
                                pInnings1.Add("BallsFaced", innings1Balls);
                                pInnings1.Add("RunsConceded", innings2Runs);
                                pInnings1.Add("BallsBowled", innings2Balls);
                                await connection.ExecuteAsync("SP_TournamentPointTable", pInnings1, transaction, commandType: CommandType.StoredProcedure);

                                // Update for innings 2 team
                                var pInnings2 = new DynamicParameters();
                                pInnings2.Add("Flag", 2);
                                pInnings2.Add("TournamentID", tournamentId);
                                pInnings2.Add("TeamsID", innings2TeamId);
                                pInnings2.Add("WinnerTeamID", winnerId);
                                pInnings2.Add("IsDraw", isDraw);
                                pInnings2.Add("RunsScored", innings2Runs);
                                pInnings2.Add("BallsFaced", innings2Balls);
                                pInnings2.Add("RunsConceded", innings1Runs);
                                pInnings2.Add("BallsBowled", innings1Balls);
                                await connection.ExecuteAsync("SP_TournamentPointTable", pInnings2, transaction, commandType: CommandType.StoredProcedure);
                            }

                            // Re-fetch stats to include WinnerMessage
                            matchStats = await GetMatchStats(connection, transaction, input.CricketMatchID);
                        }

                        transaction.Commit();

                        // 5. Broadcast via SignalR
                        await _hubContext.Clients.All.SendAsync("UpdateLiveScore", matchStats);

                        return Ok(ApiResponse<dynamic>.Ok("Ball Added", new { Stats = matchStats, IsInningsOver = (isAllOut || isOversComplete || isTargetReached) }));
                    }
                    catch (Exception ex)
                    {
                        if (transaction.Connection != null) transaction.Rollback();
                        return StatusCode(500, ex.Message);
                    }
                }
            }
        }

        [HttpPost("UndoLastBall")]
        public async Task<IActionResult> UndoLastBall([FromBody] int matchId)
        {
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Find the last ball
                        var lastBallParams = new DynamicParameters();
                        lastBallParams.Add("Flag", 9);
                        lastBallParams.Add("MatchId", matchId);
                        var lastBall = await connection.QueryFirstOrDefaultAsync<dynamic>("SP_LiveMatch", lastBallParams, transaction, commandType: CommandType.StoredProcedure);

                        if (lastBall == null)
                        {
                            return BadRequest(ApiResponse.Error("No balls to undo."));
                        }

                        // 2. Delete the ball and clean up empty over in SP
                        var undoParams = new DynamicParameters();
                        undoParams.Add("Flag", 10);
                        undoParams.Add("MatchId", matchId);
                        await connection.ExecuteAsync("SP_LiveMatch", undoParams, transaction, commandType: CommandType.StoredProcedure);

                        // 4. Revert state logic
                        // Re-fetch match info to check current state (Flag 1)
                        var statusParams = new DynamicParameters();
                        statusParams.Add("Flag", 1);
                        statusParams.Add("MatchId", matchId);
                        var matchInfo = await connection.QueryFirstOrDefaultAsync<dynamic>("SP_LiveMatch", statusParams, transaction, commandType: CommandType.StoredProcedure);

                        // If match was finished, revert it (Flag 2)
                        if (matchInfo.MatchStatus == "Finished")
                        {
                            var updateStatusParams = new DynamicParameters();
                            updateStatusParams.Add("Flag", 2);
                            updateStatusParams.Add("MatchId", matchId);
                            updateStatusParams.Add("MatchStatus", "Live");
                            await connection.ExecuteAsync("SP_LiveMatch", updateStatusParams, transaction, commandType: CommandType.StoredProcedure);
                        }
                        
                        // If we are undoing a ball from a previous innings (e.g. undoing last ball of Innings 1 while currently in Break/Innings 2)
                        int ballInnings = (int)lastBall.Innings;
                        int currentMatchInnings = (int)matchInfo.CurrentInnings;

                        if (currentMatchInnings > ballInnings || matchInfo.MatchStatus == "Innings Break") {
                             var revertParams = new DynamicParameters();
                             revertParams.Add("Flag", 4);
                             revertParams.Add("MatchId", matchId);
                             revertParams.Add("Innings", ballInnings);
                             revertParams.Add("MatchStatus", "Live");
                             await connection.ExecuteAsync("SP_LiveMatch", revertParams, transaction, commandType: CommandType.StoredProcedure);
                        }

                        // Re-fetch stats to broadcast
                        var matchStats = await GetMatchStats(connection, transaction, matchId);
                        
                        // Sync current players to CricketMatch from the NEW last ball if possible (Flag 9)
                        var nextBallParams = new DynamicParameters();
                        nextBallParams.Add("Flag", 9);
                        nextBallParams.Add("MatchId", matchId);
                        var newLastBall = await connection.QueryFirstOrDefaultAsync<dynamic>("SP_LiveMatch", nextBallParams, transaction, commandType: CommandType.StoredProcedure);

                        if (newLastBall != null) {
                            var syncUndoParams = new DynamicParameters();
                            syncUndoParams.Add("Flag", 7);
                            syncUndoParams.Add("MatchId", matchId);
                            syncUndoParams.Add("StrikerId", (int)newLastBall.StrikerPlayerID);
                            syncUndoParams.Add("NonStrikerId", (int)newLastBall.NonStrikerPlayerID);
                            syncUndoParams.Add("BowlerId", (int)newLastBall.BowlerPlayerID);
                            await connection.ExecuteAsync("SP_LiveMatch", syncUndoParams, transaction, commandType: CommandType.StoredProcedure);
                        }

                        transaction.Commit();
                        
                        // Broadcast updated stats
                        await _hubContext.Clients.All.SendAsync("UpdateLiveScore", matchStats);
                        
                        return Ok(ApiResponse<dynamic>.Ok("Ball Undone", new { Stats = matchStats }));
                    }
                    catch (Exception ex)
                    {
                        if (transaction.Connection != null) transaction.Rollback();
                        return StatusCode(500, ex.Message);
                    }
                }
            }
        }



        [HttpPost("ChangeBowler")]
        public async Task<IActionResult> ChangeBowler([FromBody] ChangeBowlerDto input)
        {
            using (var connection = _context.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("Flag", 13);
                p.Add("MatchId", input.MatchId);
                p.Add("BowlerId", input.BowlerId);
                await connection.ExecuteAsync("SP_LiveMatch", p, commandType: CommandType.StoredProcedure);
                
                // Broadcast updated stats
                var stats = await GetMatchStats(connection, null, input.MatchId);
                await _hubContext.Clients.All.SendAsync("ReceiveLiveMatchUpdate", stats);

                return Ok(ApiResponse.Ok("Bowler Updated"));
            }
        }
        
        [HttpPost("UpdateMatchPlayers")]
        public async Task<IActionResult> UpdateMatchPlayers([FromBody] UpdateMatchPlayersDto input)
        {
            using (var connection = _context.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("Flag", 14);
                p.Add("MatchId", input.MatchId);
                p.Add("StrikerId", input.StrikerId);
                p.Add("NonStrikerId", input.NonStrikerId);
                p.Add("BowlerId", input.BowlerId);
                await connection.ExecuteAsync("SP_LiveMatch", p, commandType: CommandType.StoredProcedure);

                // Broadcast updated stats
                var stats = await GetMatchStats(connection, null, input.MatchId);
                await _hubContext.Clients.All.SendAsync("ReceiveLiveMatchUpdate", stats);

                return Ok(ApiResponse.Ok("Players Updated"));
            }
        }

        [HttpGet("GetFullScorecard")]
        public async Task<IActionResult> GetFullScorecard(int matchId)
        {
            using (var connection = _context.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("Flag", 15);
                p.Add("MatchId", matchId);
                var matchInfo = await connection.QueryFirstOrDefaultAsync<dynamic>("SP_LiveMatch", p, commandType: CommandType.StoredProcedure);

                if (matchInfo == null) return NotFound(ApiResponse.Error("Match not found"));

                int teamAId = (matchInfo.TeamAID != null) ? (int)matchInfo.TeamAID : 0;
                string teamAName = matchInfo.TeamAName ?? "Team A";
                string teamBName = matchInfo.TeamBName ?? "Team B";
                int tossWinnerId = (matchInfo.TossWinnerTeamID != null) ? (int)matchInfo.TossWinnerTeamID : 0;
                string tossChoice = matchInfo.TossChoice ?? "Bat";

                string innings1Team, innings2Team;
                if (tossChoice == "Bat") {
                    innings1Team = (tossWinnerId == teamAId) ? teamAName : teamBName;
                    innings2Team = (tossWinnerId == teamAId) ? teamBName : teamAName;
                } else {
                    innings1Team = (tossWinnerId == teamAId) ? teamBName : teamAName;
                    innings2Team = (tossWinnerId == teamAId) ? teamAName : teamBName;
                }

                return Ok(ApiResponse<FullScorecardDto>.Ok("Scorecard fetched successfully", new FullScorecardDto
                {
                    MatchId = matchId,
                    Innings1 = await GetInningsScorecard(connection, matchId, 1, innings1Team),
                    Innings2 = await GetInningsScorecard(connection, matchId, 2, innings2Team)
                }));
            }
        }

        private async Task<InningsScorecardDto> GetInningsScorecard(IDbConnection connection, int matchId, int innings, string teamName)
        {
            var stats = await GetMatchStatsInnings(connection, null, matchId, innings);
            
            // Get Batting Card (Flag 16)
            var batP = new DynamicParameters();
            batP.Add("Flag", 16);
            batP.Add("MatchId", matchId);
            batP.Add("Innings", innings);
            var batting = await connection.QueryAsync<BatsmanScorecardDto>("SP_LiveMatch", batP, commandType: CommandType.StoredProcedure);

            // Get Bowling Card (Flag 17)
            var bowlP = new DynamicParameters();
            bowlP.Add("Flag", 17);
            bowlP.Add("MatchId", matchId);
            bowlP.Add("Innings", innings);
            var bowling = await connection.QueryAsync<BowlerScorecardDto>("SP_LiveMatch", bowlP, commandType: CommandType.StoredProcedure);

            // Fetch Fall of Wickets (Flag 18)
            var fowP = new DynamicParameters();
            fowP.Add("Flag", 18);
            fowP.Add("MatchId", matchId);
            fowP.Add("Innings", innings);
            var fallOfWickets = await connection.QueryAsync<FallOfWicketDto>("SP_LiveMatch", fowP, commandType: CommandType.StoredProcedure);

            return new InningsScorecardDto
            {
                TeamName = teamName,
                TotalRuns = stats.TotalRuns,
                Wickets = stats.Wickets,
                Overs = stats.Overs,
                Batting = batting.ToList(),
                Bowling = bowling.ToList(),
                FallOfWickets = fallOfWickets.ToList()
            };
        }

        [HttpPost("SaveSquad")]
        public async Task<IActionResult> SaveSquad([FromBody] SaveSquadRequestDto request)
        {
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Delete existing squad (Flag 30)
                        var p30 = new DynamicParameters();
                        p30.Add("Flag", 30);
                        p30.Add("MatchId", request.CricketMatchID);
                        p30.Add("TeamId", request.TeamID);
                        await connection.ExecuteAsync("SP_LiveMatch", p30, transaction, commandType: CommandType.StoredProcedure);

                        // 2. Insert new squad (Flag 31)
                        foreach (var player in request.Players)
                        {
                            var p31 = new DynamicParameters();
                            p31.Add("Flag", 31);
                            p31.Add("MatchId", request.CricketMatchID);
                            p31.Add("TeamId", request.TeamID);
                            p31.Add("PlayerId", player.PlayerId);
                            p31.Add("IsPlaying", player.IsPlaying);
                            p31.Add("IsCaptain", player.IsCaptain);
                            p31.Add("IsWicketKeeper", player.IsWicketKeeper);
                            await connection.ExecuteAsync("SP_LiveMatch", p31, transaction, commandType: CommandType.StoredProcedure);
                        }

                        transaction.Commit();
                        return Ok(ApiResponse.Ok("Squad saved successfully"));
                    }
                    catch (Exception ex)
                    {
                        if (transaction.Connection != null) transaction.Rollback();
                        return BadRequest(ApiResponse.Error("Error - " + ex.Message));
                    }
                }
            }
        }

        [HttpGet("GetSquads")]
        public async Task<IActionResult> GetSquads(int matchId)
        {
            using (var connection = _context.CreateConnection())
            {
                var matchParams = new DynamicParameters();
                matchParams.Add("Flag", 11);
                matchParams.Add("MatchId", matchId);
                var matchInfo = await connection.QueryFirstOrDefaultAsync<dynamic>("SP_LiveMatch", matchParams, commandType: CommandType.StoredProcedure);

                if (matchInfo == null) return NotFound(ApiResponse.Error("Match not found"));

                int teamAId = (int)matchInfo.TeamAID;
                int teamBId = (int)matchInfo.TeamBID;

                var teamAParams = new DynamicParameters();
                teamAParams.Add("Flag", 12);
                teamAParams.Add("MatchId", matchId);
                teamAParams.Add("TeamId", teamAId);
                var teamAPlayers = await connection.QueryAsync<MatchSquadDto>("SP_LiveMatch", teamAParams, commandType: CommandType.StoredProcedure);

                var teamBParams = new DynamicParameters();
                teamBParams.Add("Flag", 12);
                teamBParams.Add("MatchId", matchId);
                teamBParams.Add("TeamId", teamBId);
                var teamBPlayers = await connection.QueryAsync<MatchSquadDto>("SP_LiveMatch", teamBParams, commandType: CommandType.StoredProcedure);

                return Ok(ApiResponse<SquadDto>.Ok("Squad fetched successfully", new SquadDto
                {
                    TeamAName = matchInfo.TeamAName,
                    TeamBName = matchInfo.TeamBName,
                    MatchPlayer = matchInfo.MatchPlayer ?? 11,
                    ExtraPlayer = matchInfo.ExtraPlayer ?? 0,
                    TeamAPlayers = teamAPlayers.ToList(),
                    TeamBPlayers = teamBPlayers.ToList()
                }));
            }
        }

        [HttpGet("GetLiveScore")]
        public async Task<IActionResult> GetLiveScore(int matchId)
        {
            using (var connection = _context.CreateConnection())
            {
               var stats = await GetMatchStats(connection, null, matchId);
                return Ok(ApiResponse<MatchStatsDto>.Ok("Live score fetched successfully", stats));
            }
        }

        private async Task<Overs> GetOrCreateOver(IDbConnection connection, IDbTransaction transaction, int matchId, int bowlerId)
        {
            // Fetch match info for current innings (Flag 1)
            var p1 = new DynamicParameters();
            p1.Add("Flag", 1);
            p1.Add("MatchId", matchId);
            var matchInfo = await connection.QueryFirstOrDefaultAsync<dynamic>("SP_LiveMatch", p1, transaction, commandType: CommandType.StoredProcedure);
            int currentInnings = matchInfo?.CurrentInnings ?? 1;

            // Find the latest over for THIS innings (Flag 21)
            var p21 = new DynamicParameters();
            p21.Add("Flag", 21);
            p21.Add("MatchId", matchId);
            p21.Add("Innings", currentInnings);
            var lastOver = await connection.QueryFirstOrDefaultAsync<Overs>("SP_LiveMatch", p21, transaction, commandType: CommandType.StoredProcedure);

            if (lastOver != null)
            {
                // Count valid balls (Flag 22)
                var p22 = new DynamicParameters();
                p22.Add("Flag", 22);
                p22.Add("OverId", lastOver.Id);
                var ballCount = await connection.ExecuteScalarAsync<int>("SP_LiveMatch", p22, transaction, commandType: CommandType.StoredProcedure);

                if (ballCount < 6) return lastOver;
            }

            // Validation: Same bowler cannot bowl consecutive overs
            if (lastOver != null && lastOver.BowlerId == bowlerId)
            {
                throw new InvalidOperationException("Same bowler cannot bowl consecutive overs.");
            }

            // Create New Over (Flag 23)
            var p23 = new DynamicParameters();
            p23.Add("Flag", 23);
            p23.Add("MatchId", matchId);
            p23.Add("BowlerId", bowlerId);
            p23.Add("Innings", currentInnings);
            var newOverId = await connection.ExecuteScalarAsync<int>("SP_LiveMatch", p23, transaction, commandType: CommandType.StoredProcedure);

            return new Overs { Id = newOverId, CricketMatchID = matchId, BowlerId = bowlerId, Innings = currentInnings };
        }

        private async Task<MatchStatsDto> GetMatchStats(IDbConnection connection, IDbTransaction transaction, int matchId)
        {
            // 1. Get Match Info (Flag 8)
            var p8 = new DynamicParameters();
            p8.Add("Flag", 8);
            p8.Add("MatchId", matchId);
            var matchInfo = await connection.QueryFirstOrDefaultAsync<dynamic>("SP_LiveMatch", p8, transaction, commandType: CommandType.StoredProcedure);
 
            int strikerId = (matchInfo?.StrikerPlayerID != null) ? (int)matchInfo.StrikerPlayerID : 0;
            int nonStrikerId = (matchInfo?.NonStrikerPlayerID != null) ? (int)matchInfo.NonStrikerPlayerID : 0;
            int bowlerId = (matchInfo?.BowlerPlayerID != null) ? (int)matchInfo.BowlerPlayerID : 0;
            int currentInnings = (matchInfo?.CurrentInnings != null) ? (int)matchInfo.CurrentInnings : 1;
            string matchStatus = matchInfo?.MatchStatus ?? "Live";

            // Get Recent Balls (Flag 25)
            var p25 = new DynamicParameters();
            p25.Add("Flag", 25);
            p25.Add("MatchId", matchId);
            p25.Add("Innings", currentInnings);
            var recentBalls = await connection.QueryAsync<string>("SP_LiveMatch", p25, transaction, commandType: CommandType.StoredProcedure);
            var recentBallsList = recentBalls.Reverse().ToList(); // Show oldest to newest

            string teamAName = matchInfo?.TeamAName ?? "Team A";
            string teamBName = matchInfo?.TeamBName ?? "Team B";
            int teamAId = (matchInfo?.TeamAID != null) ? (int)matchInfo.TeamAID : 0;
            int tossWinnerId = (matchInfo?.TossWinnerTeamID != null) ? (int)matchInfo.TossWinnerTeamID : 0;
            string tossChoice = matchInfo?.TossChoice ?? "Bat";

            string innings1Team = "";
            string innings2Team = "";
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
 
            // 2. Aggregate Match Totals for current innings (Flag 24)
            var p24 = new DynamicParameters();
            p24.Add("Flag", 24);
            p24.Add("MatchId", matchId);
            p24.Add("Innings", currentInnings);
            var totalStats = await connection.QueryFirstOrDefaultAsync<dynamic>("SP_LiveMatch", p24, transaction, commandType: CommandType.StoredProcedure);
            
            int totalBalls = (totalStats != null && totalStats.TotalBalls != null) ? Convert.ToInt32(totalStats.TotalBalls) : 0;
            int? innings1TotalRuns = (totalStats != null && totalStats.Innings1Runs != null) ? Convert.ToInt32(totalStats.Innings1Runs) : (int?)null;
            int? innings1TotalWickets = (totalStats != null && totalStats.Innings1Wickets != null) ? Convert.ToInt32(totalStats.Innings1Wickets) : (int?)null;

            int overs = totalBalls / 6;
            int balls = totalBalls % 6;
            string overStr = $"{overs}.{balls}";

            // Calculate CRR
            double currentRuns = (totalStats != null && totalStats.TotalRuns != null) ? Convert.ToDouble(totalStats.TotalRuns) : 0;
            double crr = totalBalls > 0 ? (currentRuns / totalBalls) * 6 : 0;

            int? target = null;
            double rrr = 0;
            if (currentInnings == 2)
            {
                var p3 = new DynamicParameters();
                p3.Add("Flag", 3);
                p3.Add("MatchId", matchId);
                var matchInfo2 = await connection.QueryFirstOrDefaultAsync<dynamic>("SP_LiveMatch", p3, transaction, commandType: CommandType.StoredProcedure);
                int matchOvers = (matchInfo2?.MatchOvers != null) ? (int)matchInfo2.MatchOvers : 20;
                int totalMatchBalls = matchOvers * 6;

                // Calculate target using Flag 24 for Innings 1
                var p24_1 = new DynamicParameters();
                p24_1.Add("Flag", 24);
                p24_1.Add("MatchId", matchId);
                p24_1.Add("Innings", 1);
                var innings1Stats = await connection.QueryFirstOrDefaultAsync<dynamic>("SP_LiveMatch", p24_1, transaction, commandType: CommandType.StoredProcedure);
                
                if (innings1Stats != null && innings1Stats.TotalRuns != null)
                {
                    int innings1Runs = Convert.ToInt32(innings1Stats.TotalRuns);
                    target = innings1Runs + 1;
                    int runsNeeded = target.Value - (int)currentRuns;
                    int remainingBalls = totalMatchBalls - totalBalls;
                    if (remainingBalls > 0)
                    {
                        rrr = (double)runsNeeded / remainingBalls * 6;
                    }
                    else if (runsNeeded > 0)
                    {
                        rrr = 99.99;
                    }
                }
            }

            // 3. Helper to Calculate Batting Stats
            async Task<BatsmanStatsDto> GetBatsmanStats(int playerId) {
                 if(playerId == 0) return new BatsmanStatsDto { PlayerName = "Unknown", PlayerImage = "" };
                 
                 var p26 = new DynamicParameters();
                 p26.Add("Flag", 26);
                 p26.Add("MatchId", matchId);
                 p26.Add("Innings", currentInnings);
                 p26.Add("PlayerId", playerId);
                 var res = await connection.QueryFirstOrDefaultAsync<dynamic>("SP_LiveMatch", p26, transaction, commandType: CommandType.StoredProcedure);
                 
                 int runs = (res != null && res.Runs != null) ? Convert.ToInt32(res.Runs) : 0;
                 int balls = (res != null && res.Balls != null) ? Convert.ToInt32(res.Balls) : 0;
                 double sr = balls > 0 ? (double)runs / balls * 100 : 0;
                 
                 return new BatsmanStatsDto {
                     PlayerId = playerId,
                     PlayerName = res?.PlayerName ?? "Unknown",
                     PlayerImage = res?.PlayerImage ?? "",
                     Runs = runs,
                     Balls = balls,
                     Fours = (res != null && res.Fours != null) ? Convert.ToInt32(res.Fours) : 0,
                     Sixes = (res != null && res.Sixes != null) ? Convert.ToInt32(res.Sixes) : 0,
                     StrikeRate = Math.Round(sr, 2)
                 };
            }

            // 4. Helper to Calculate Bowling Stats
            async Task<BowlerStatsDto> GetBowlerStats(int playerId) {
                  if(playerId == 0) return new BowlerStatsDto { PlayerName = "Unknown", PlayerImage = "", Overs = "0.0" };
                  
                  var p27 = new DynamicParameters();
                  p27.Add("Flag", 27);
                  p27.Add("MatchId", matchId);
                  p27.Add("Innings", currentInnings);
                  p27.Add("PlayerId", playerId);
                  var res = await connection.QueryFirstOrDefaultAsync<dynamic>("SP_LiveMatch", p27, transaction, commandType: CommandType.StoredProcedure);

                 int runsCount = (res != null && res.RunsConceded != null) ? Convert.ToInt32(res.RunsConceded) : 0;
                 int validBallsCount = (res != null && res.ValidBalls != null) ? Convert.ToInt32(res.ValidBalls) : 0;
                 
                 double fieldOvers = validBallsCount / 6 + (validBallsCount % 6) * 0.1; // Display format NOT mathematical
                 double totalOversMath = validBallsCount / 6.0;
                 double er = totalOversMath > 0 ? runsCount / totalOversMath : 0;

                 return new BowlerStatsDto {
                     PlayerId = playerId,
                     PlayerName = res?.PlayerName ?? "Unknown",
                     PlayerImage = res?.PlayerImage ?? "",
                     Runs = runsCount,
                     Wickets = (res != null && res.Wickets != null) ? Convert.ToInt32(res.Wickets) : 0,
                     Overs = fieldOvers.ToString("0.0"),
                     Maidens = 0, // Placeholder
                     Economy = Math.Round(er, 2)
                 };
            }

            // 5. Get List of Dismissed Players for current innings (Flag 28)
            var p28 = new DynamicParameters();
            p28.Add("Flag", 28);
            p28.Add("MatchId", matchId);
            p28.Add("Innings", currentInnings);
            var outPlayerIds = await connection.QueryAsync<int>("SP_LiveMatch", p28, transaction, commandType: CommandType.StoredProcedure);

            string? winnerMessage = null;
            if (matchStatus == "Finished")
            {
                winnerMessage = await GetWinnerMessageInternal(connection, transaction, matchId);
            }

            return new MatchStatsDto
            {
                CricketMatchID = matchId,
                TotalRuns = (int)currentRuns,
                Wickets = (totalStats != null && totalStats.TotalWickets != null) ? Convert.ToInt32(totalStats.TotalWickets) : 0,
                Overs = overStr,
                Target = target,
                CurrentInnings = currentInnings,
                CRR = Math.Round(crr, 2),
                RRR = Math.Round(rrr, 2),
                TeamAName = teamAName,
                TeamBName = teamBName,
                BattingTeamName = battingTeamName,
                MatchStatus = matchStatus,
                RecentBalls = recentBallsList,
                StrikerStats = await GetBatsmanStats(strikerId),
                NonStrikerStats = await GetBatsmanStats(nonStrikerId),
                BowlerStats = await GetBowlerStats(bowlerId),
                OutPlayerIds = outPlayerIds.ToList(),
                WinnerMessage = winnerMessage,
                Innings1TotalRuns = innings1TotalRuns,
                Innings1TotalWickets = innings1TotalWickets,
                Innings1TeamName = innings1Team,
                Innings2TeamName = innings2Team
            };
        }

        [HttpGet("GetMatchSummary")]
        public async Task<IActionResult> GetMatchSummary(int matchId)
        {
            using (var connection = _context.CreateConnection())
            {
                var summary = new MatchSummaryDto
                {
                    MatchId = matchId,
                    Innings1 = await GetMatchStatsInnings(connection, null, matchId, 1),
                    Innings2 = await GetMatchStatsInnings(connection, null, matchId, 2)
                };

                var p = new DynamicParameters();
                p.Add("Flag", 15);
                p.Add("MatchId", matchId);
                var matchInfo = await connection.QueryFirstOrDefaultAsync<dynamic>("SP_LiveMatch", p, commandType: CommandType.StoredProcedure);

                int teamAId = (matchInfo?.TeamAID != null) ? (int)matchInfo.TeamAID : 0;
                string teamAName = matchInfo?.TeamAName ?? "Team A";
                string teamBName = matchInfo?.TeamBName ?? "Team B";
                int tossWinnerId = (matchInfo?.TossWinnerTeamID != null) ? (int)matchInfo.TossWinnerTeamID : 0;
                string tossChoice = matchInfo?.TossChoice ?? "Bat";

                if (tossChoice == "Bat") {
                    summary.Innings1TeamName = (tossWinnerId == teamAId) ? teamAName : teamBName;
                    summary.Innings2TeamName = (tossWinnerId == teamAId) ? teamBName : teamAName;
                } else {
                    summary.Innings1TeamName = (tossWinnerId == teamAId) ? teamBName : teamAName;
                    summary.Innings2TeamName = (tossWinnerId == teamAId) ? teamAName : teamBName;
                }

                summary.WinnerMessage = await GetWinnerMessageInternal(connection, null, matchId);

                return Ok(ApiResponse<dynamic>.Ok("Match Summary Retrieved", summary));
            }
        }

        private async Task<string?> GetWinnerMessageInternal(IDbConnection connection, IDbTransaction? transaction, int matchId)
        {
            var innings1 = await GetMatchStatsInnings(connection, transaction, matchId, 1);
            var innings2 = await GetMatchStatsInnings(connection, transaction, matchId, 2);

            var p = new DynamicParameters();
            p.Add("Flag", 15);
            p.Add("MatchId", matchId);
            var matchInfo = await connection.QueryFirstOrDefaultAsync<dynamic>("SP_LiveMatch", p, transaction, commandType: CommandType.StoredProcedure);

            if (matchInfo == null) return null;

            int teamAId = (matchInfo.TeamAID != null) ? (int)matchInfo.TeamAID : 0;
            string teamAName = matchInfo.TeamAName ?? "Team A";
            string teamBName = matchInfo.TeamBName ?? "Team B";
            int tossWinnerId = (matchInfo.TossWinnerTeamID != null) ? (int)matchInfo.TossWinnerTeamID : 0;
            string tossChoice = matchInfo.TossChoice ?? "Bat";

            string innings1TeamName, innings2TeamName;
    if (tossWinnerId == 0) {
        // Default to Team A batting first if no toss info
        innings1TeamName = teamAName;
        innings2TeamName = teamBName;
    } else if (tossChoice == "Bat") {
        innings1TeamName = (tossWinnerId == teamAId) ? teamAName : teamBName;
        innings2TeamName = (tossWinnerId == teamAId) ? teamBName : teamAName;
    } else {
        innings1TeamName = (tossWinnerId == teamAId) ? teamBName : teamAName;
        innings2TeamName = (tossWinnerId == teamAId) ? teamAName : teamBName;
    }

            if (innings1 != null && innings2 != null && innings1.TotalRuns > 0)
            {
                if (innings2.TotalRuns > innings1.TotalRuns)
                {
                    int wicketsLeft = 10 - innings2.Wickets;
                    return $"{innings2TeamName} Won by {wicketsLeft} Wickets";
                }
                else if (innings1.TotalRuns > innings2.TotalRuns)
                {
                    int runsDiff = innings1.TotalRuns - innings2.TotalRuns;
                    return $"{innings1TeamName} Won by {runsDiff} Runs";
                }
                else if (innings1.TotalRuns == innings2.TotalRuns && innings2.Overs != "0.0")
                {
                    return "Match Tied";
                }
            }
            return null;
        }

        private async Task<int?> GetWinnerTeamIdInternal(IDbConnection connection, IDbTransaction? transaction, int matchId)
        {
            var innings1 = await GetMatchStatsInnings(connection, transaction, matchId, 1);
            var innings2 = await GetMatchStatsInnings(connection, transaction, matchId, 2);

            var p = new DynamicParameters();
            p.Add("Flag", 15);
            p.Add("MatchId", matchId);
            var matchInfo = await connection.QueryFirstOrDefaultAsync<dynamic>("SP_LiveMatch", p, transaction, commandType: CommandType.StoredProcedure);

            if (matchInfo == null) return null;

            int teamAId = (matchInfo.TeamAID != null) ? (int)matchInfo.TeamAID : 0;
            int teamBId = (matchInfo.TeamBID != null) ? (int)matchInfo.TeamBID : 0;
            int tossWinnerId = (matchInfo.TossWinnerTeamID != null) ? (int)matchInfo.TossWinnerTeamID : 0;
            string tossChoice = matchInfo.TossChoice ?? "Bat";

            int innings1TeamId, innings2TeamId;
            if (tossChoice == "Bat") {
                innings1TeamId = (tossWinnerId == teamAId) ? teamAId : teamBId;
                innings2TeamId = (tossWinnerId == teamAId) ? teamBId : teamAId;
            } else {
                innings1TeamId = (tossWinnerId == teamAId) ? teamBId : teamAId;
                innings2TeamId = (tossWinnerId == teamAId) ? teamAId : teamBId;
            }

            if (innings1 != null && innings2 != null && innings1.TotalRuns > 0)
            {
                if (innings2.TotalRuns > innings1.TotalRuns) return innings2TeamId;
                else if (innings1.TotalRuns > innings2.TotalRuns) return innings1TeamId;
            }
            return null;
        }

        private async Task<MatchStatsDto> GetMatchStatsInnings(IDbConnection connection, IDbTransaction? transaction, int matchId, int innings)
        {
             // Aggregate Total Stats for Innings (Flag 24)
            var p = new DynamicParameters();
            p.Add("Flag", 24);
            p.Add("MatchId", matchId);
            p.Add("Innings", innings);
            var totalStats = await connection.QueryFirstOrDefaultAsync<dynamic>("SP_LiveMatch", p, transaction, commandType: CommandType.StoredProcedure);
            
            if (totalStats == null || totalStats.TotalRuns == null) return new MatchStatsDto { Overs = "0.0" };

            int totalBalls = Convert.ToInt32(totalStats.TotalBalls);
            int overs = totalBalls / 6;
            int balls = totalBalls % 6;

            return new MatchStatsDto
            {
                CricketMatchID = matchId,
                TotalRuns = Convert.ToInt32(totalStats.TotalRuns),
                Wickets = Convert.ToInt32(totalStats.TotalWickets),
                Overs = $"{overs}.{balls}"
            };
        }

        [HttpGet("GetMatchInfo")]
        public async Task<IActionResult> GetMatchInfo(int matchId)
        {
            using (var connection = _context.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("Flag", 19);
                p.Add("MatchId", matchId);
                var result = await connection.QueryFirstOrDefaultAsync<dynamic>("SP_LiveMatch", p, commandType: CommandType.StoredProcedure);
                return Ok(ApiResponse<dynamic>.Ok("Match Info Retrieved", result));
            }
        }

        [HttpGet("GetOversDetails")]
        public async Task<IActionResult> GetOversDetails(int matchId)
        {
            using (var connection = _context.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("Flag", 20);
                p.Add("MatchId", matchId);
                var data = await connection.QueryAsync<dynamic>("SP_LiveMatch", p, commandType: CommandType.StoredProcedure);
                
                var grouped = data.GroupBy(x => new { x.Innings, x.OverNumber, x.BowlerName })
                    .Select(g => new {
                        g.Key.Innings,
                        g.Key.OverNumber,
                        g.Key.BowlerName,
                        Balls = g.Select(b => new {
                            b.Run,
                            b.IsWicket,
                            b.BallType,
                            Display = (b.IsWicket == true || b.IsWicket == 1) ? "W" : 
                                     (b.BallType == "Wide" ? b.Run + "wd" : 
                                      (b.BallType == "NoBall" ? b.Run + "nb" : b.Run.ToString()))
                        })
                    });

                return Ok(ApiResponse<dynamic>.Ok("Overs Details Retrieved", grouped));
            }
        }
    }

    
}