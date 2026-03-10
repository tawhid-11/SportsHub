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

                        // 2. Insert Ball
                        var sqlBall = @"
                            INSERT INTO MatchBallByBall (OverId, StrikerPlayerID, NonStrikerPlayerID, BowlerPlayerID, Run, IsWicket, BallType, WicketType, PlayerOutID, IsBye, CreatedAt)
                            VALUES (@OverId, @StrikerPlayerID, @NonStrikerPlayerID, @BowlerPlayerID, @Run, @IsWicket, @BallType, @WicketType, @PlayerOutID, @IsBye, GETDATE());
                        ";
                        
                        await connection.ExecuteAsync(sqlBall, new
                        {
                            OverId = currentOver.Id,
                            input.StrikerPlayerID,
                            input.NonStrikerPlayerID,
                            input.BowlerPlayerID,
                            input.Run,
                            input.IsWicket,
                            BallType = input.BallType, // 'Normal', 'Wide', 'NoBall'
                            input.WicketType,
                            input.PlayerOutID,
                            input.IsBye
                        }, transaction);

                        // 2b. Sync Current Players to CricketMatch Table
                        var syncPlayersSql = @"
                            UPDATE CricketMatch SET 
                                StrikerPlayerID = @StrikerId,
                                NonStrikerPlayerID = @NonStrikerId,
                                BowlerPlayerID = @BowlerId
                            WHERE CricketMatchID = @MatchId";
                        await connection.ExecuteAsync(syncPlayersSql, new 
                        { 
                            StrikerId = input.StrikerPlayerID, 
                            NonStrikerId = input.NonStrikerPlayerID, 
                            BowlerId = input.BowlerPlayerID,
                            MatchId = input.CricketMatchID 
                        }, transaction);

                        // 2c. Reset status from 'Innings Break' to 'Live' if in 2nd innings
                        var matchStatusCheck = await connection.QueryFirstOrDefaultAsync<dynamic>(
                            "SELECT CurrentInnings, MatchStatus FROM CricketMatch WHERE CricketMatchID = @MatchId", 
                            new { MatchId = input.CricketMatchID }, 
                            transaction);
                        
                        int checkInnings = matchStatusCheck?.CurrentInnings ?? 1;
                        string checkStatus = matchStatusCheck?.MatchStatus ?? "Live";
                        
                        if (checkInnings == 2 && checkStatus == "Innings Break")
                        {
                            await connection.ExecuteAsync(
                                "UPDATE CricketMatch SET MatchStatus = 'Live' WHERE CricketMatchID = @MatchId", 
                                new { MatchId = input.CricketMatchID }, 
                                transaction);
                        }


                        // 3. Update Match Stats (Implementation of Score Calculation)
                        var matchStats = await GetMatchStats(connection, transaction, input.CricketMatchID);

                        // 4. Check for End of Innings
                        var matchInfo = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
                            SELECT cm.*, cm.Overs as MatchOvers 
                            FROM CricketMatch cm 
                            JOIN TeamSchedule ts ON cm.TeamScheduleID = ts.TeamScheduleID 
                            WHERE cm.CricketMatchID = @MatchId", new { MatchId = input.CricketMatchID }, transaction);

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
                            var updateSql = @"
                                UPDATE CricketMatch 
                                SET CurrentInnings = 2, 
                                    MatchStatus = 'Innings Break',
                                    StrikerPlayerID = NULL,
                                    NonStrikerPlayerID = NULL,
                                    BowlerPlayerID = NULL
                                WHERE CricketMatchID = @MatchId";
                            await connection.ExecuteAsync(updateSql, new { MatchId = input.CricketMatchID }, transaction);
                        }
                        else if ((isAllOut || isOversComplete || isTargetReached) && currentInnings == 2)
                        {
                            // Determine Winner
                            int? winnerId = await GetWinnerTeamIdInternal(connection, transaction, input.CricketMatchID);
                            
                            // End of Match
                            await connection.ExecuteAsync("UPDATE CricketMatch SET MatchStatus = 'Finished', WinnerTeamID = @WinnerId WHERE CricketMatchID = @MatchId", 
                                new { WinnerId = winnerId, MatchId = input.CricketMatchID }, transaction);
                            
                            // Update Tournament Standings if it's a tournament match
                            var tournamentMatch = await connection.QueryFirstOrDefaultAsync<dynamic>(
                                "SELECT ts.TournamentID, ts.TeamAID, ts.TeamBID, cm.TossWinnerTeamID, cm.TossChoice, cm.Overs " +
                                "FROM CricketMatch cm JOIN TeamSchedule ts ON cm.TeamScheduleID = ts.TeamScheduleID " +
                                "WHERE cm.CricketMatchID = @MatchId", new { MatchId = input.CricketMatchID }, transaction);

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
                                        int overs = int.Parse(oversParts[0]);
                                        int balls = int.Parse(oversParts[1]);
                                        innings1Balls = (overs * 6) + balls;
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
                                        int overs = int.Parse(oversParts[0]);
                                        int balls = int.Parse(oversParts[1]);
                                        innings2Balls = (overs * 6) + balls;
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

                        return Ok(new { Message = "Ball Added", Stats = matchStats, IsInningsOver = (isAllOut || isOversComplete || isTargetReached) });
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
                var sql = "UPDATE CricketMatch SET BowlerPlayerID = @BowlerId WHERE CricketMatchID = @MatchId";
                await connection.ExecuteAsync(sql, new { BowlerId = input.BowlerId, MatchId = input.MatchId });
                
                // Broadcast updated stats
                var stats = await GetMatchStats(connection, null, input.MatchId);
                await _hubContext.Clients.All.SendAsync("ReceiveLiveMatchUpdate", stats);

                return Ok(new { Message = "Bowler Updated" });
            }
        }
        
        [HttpPost("UpdateMatchPlayers")]
        public async Task<IActionResult> UpdateMatchPlayers([FromBody] UpdateMatchPlayersDto input)
        {
            using (var connection = _context.CreateConnection())
            {
                // Also reset MatchStatus to 'Live' if it was 'Innings Break'
                var sql = @"UPDATE CricketMatch SET 
                            StrikerPlayerID = @StrikerId, 
                            NonStrikerPlayerID = @NonStrikerId, 
                            BowlerPlayerID = @BowlerId,
                            MatchStatus = CASE WHEN MatchStatus = 'Innings Break' THEN 'Live' ELSE MatchStatus END
                            WHERE CricketMatchID = @MatchId";
                            
                await connection.ExecuteAsync(sql, new 
                { 
                    StrikerId = input.StrikerId, 
                    NonStrikerId = input.NonStrikerId, 
                    BowlerId = input.BowlerId,
                    MatchId = input.MatchId 
                });

                // Broadcast updated stats
                var stats = await GetMatchStats(connection, null, input.MatchId);
                await _hubContext.Clients.All.SendAsync("ReceiveLiveMatchUpdate", stats);

                return Ok(new { Message = "Players Updated" });
            }
        }

        [HttpGet("GetFullScorecard")]
        public async Task<IActionResult> GetFullScorecard(int matchId)
        {
            using (var connection = _context.CreateConnection())
            {
                var matchInfo = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
                    SELECT 
                        ts.TeamAID, ts.TeamBID, ta.TeamName as TeamAName, tb.TeamName as TeamBName,
                        cm.TossWinnerTeamID, cm.TossChoice
                    FROM CricketMatch cm
                    INNER JOIN TeamSchedule ts ON cm.TeamScheduleID = ts.TeamScheduleID
                    LEFT JOIN Teams ta ON ta.TeamsID = ts.TeamAID
                    LEFT JOIN Teams tb ON tb.TeamsID = ts.TeamBID
                    WHERE cm.CricketMatchID = @MatchId;
                ", new { MatchId = matchId });

                if (matchInfo == null) return NotFound();

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

                return Ok(new FullScorecardDto
                {
                    MatchId = matchId,
                    Innings1 = await GetInningsScorecard(connection, matchId, 1, innings1Team),
                    Innings2 = await GetInningsScorecard(connection, matchId, 2, innings2Team)
                });
            }
        }

        private async Task<InningsScorecardDto> GetInningsScorecard(IDbConnection connection, int matchId, int innings, string teamName)
        {
            var stats = await GetMatchStatsInnings(connection, null, matchId, innings);
            
            // Get Batting Card
            var battingSql = @"
                SELECT 
                    p.PlayerID as PlayerId,
                    p.FullName as PlayerName,
                    p.PlayerImage,
                    SUM(CASE WHEN b.BallType = 'Normal' AND (b.IsBye = 0 OR b.IsBye IS NULL) THEN b.Run WHEN b.BallType = 'NoBall' AND (b.IsBye = 0 OR b.IsBye IS NULL) THEN (b.Run - 1) ELSE 0 END) as Runs,
                    COUNT(CASE WHEN b.BallType != 'Wide' THEN 1 END) as Balls,
                    COUNT(CASE WHEN (b.BallType = 'Normal' AND b.Run = 4 AND (b.IsBye = 0 OR b.IsBye IS NULL)) OR (b.BallType = 'NoBall' AND b.Run = 5 AND (b.IsBye = 0 OR b.IsBye IS NULL)) THEN 1 END) as Fours,
                    COUNT(CASE WHEN (b.BallType = 'Normal' AND b.Run = 6 AND (b.IsBye = 0 OR b.IsBye IS NULL)) OR (b.BallType = 'NoBall' AND b.Run = 7 AND (b.IsBye = 0 OR b.IsBye IS NULL)) THEN 1 END) as Sixes,
                    MAX(b.WicketType) as WicketType,
                    MAX(bw.FullName) as BowlerName,
                    MAX(CASE WHEN b.IsWicket = 1 THEN 'out' ELSE 'not out' END) as OutStatusFlag
                FROM Players p
                INNER JOIN (
                    SELECT bb.* FROM MatchBallByBall bb
                    JOIN Overs oo ON bb.OverId = oo.Id
                    WHERE oo.CricketMatchID = @MatchId AND oo.Innings = @Innings
                ) b ON p.PlayerID = b.StrikerPlayerID
                LEFT JOIN Players bw ON b.BowlerPlayerID = bw.PlayerID
                GROUP BY p.PlayerID, p.FullName, p.PlayerImage";
            
            var battingData = await connection.QueryAsync<dynamic>(battingSql, new { MatchId = matchId, Innings = innings });
            var battingList = battingData.Select(d => {
                int runs = Convert.ToInt32(d.Runs);
                int balls = Convert.ToInt32(d.Balls);
                return new BatsmanScorecardDto {
                    PlayerId = d.PlayerId,
                    PlayerName = d.PlayerName,
                    PlayerImage = d.PlayerImage,
                    Runs = runs,
                    Balls = balls,
                    Fours = Convert.ToInt32(d.Fours),
                    Sixes = Convert.ToInt32(d.Sixes),
                    StrikeRate = balls > 0 ? Math.Round((double)runs / balls * 100, 2) : 0,
                    Dismissal = d.OutStatusFlag == "out" ? $"{d.WicketType} b {d.BowlerName}" : "not out",
                    OutStatus = d.OutStatusFlag
                };
            }).ToList();

            // Get Bowling Card
            var bowlingSql = @"
                SELECT 
                    p.PlayerID as PlayerId,
                    p.FullName as PlayerName,
                    p.PlayerImage,
                    SUM(CASE WHEN (b.IsBye = 1 OR b.IsBye IS NULL) AND b.BallType = 'Normal' THEN 0 WHEN (b.IsBye = 1 OR b.IsBye IS NULL) AND b.BallType = 'NoBall' THEN 1 ELSE b.Run END) as RunsConceded,
                    COUNT(CASE WHEN IsWicket = 1 AND WicketType != 'Run Out' THEN 1 END) as Wickets,
                    COUNT(CASE WHEN BallType NOT IN ('Wide', 'NoBall') THEN 1 END) as ValidBalls
                FROM Players p
                INNER JOIN (
                    SELECT bb.* FROM MatchBallByBall bb
                    JOIN Overs oo ON bb.OverId = oo.Id
                    WHERE oo.CricketMatchID = @MatchId AND oo.Innings = @Innings
                ) b ON p.PlayerID = b.BowlerPlayerID
                GROUP BY p.PlayerID, p.FullName, p.PlayerImage";
            
            var bowlingData = await connection.QueryAsync<dynamic>(bowlingSql, new { MatchId = matchId, Innings = innings });
            var bowlingList = bowlingData.Select(d => {
                int runs = Convert.ToInt32(d.RunsConceded);
                int validBalls = Convert.ToInt32(d.ValidBalls);
                double overs = validBalls / 6 + (validBalls % 6) * 0.1;
                double totalOversMath = validBalls / 6.0;
                return new BowlerScorecardDto {
                    PlayerId = d.PlayerId,
                    PlayerName = d.PlayerName,
                    PlayerImage = d.PlayerImage,
                    Runs = runs,
                    Wickets = Convert.ToInt32(d.Wickets),
                    Overs = overs.ToString("0.0"),
                    Economy = totalOversMath > 0 ? Math.Round(runs / totalOversMath, 2) : 0
                };
            }).ToList();

            // Fall of Wickets
            var fowSql = @"
                SELECT 
                    p.FullName as PlayerName,
                    SUM(b.Run) OVER (ORDER BY b.BallID) as CumulativeRuns,
                    COUNT(CASE WHEN b.IsWicket = 1 THEN 1 END) OVER (ORDER BY b.BallID) as WicketNumber,
                    (SELECT COUNT(*) FROM MatchBallByBall b2 JOIN Overs o2 ON b2.OverId = o2.Id WHERE o2.CricketMatchID = @MatchId AND o2.Innings = @Innings AND b2.BallID <= b.BallID AND b2.BallType NOT IN ('Wide', 'NoBall')) as TotalBalls
                FROM MatchBallByBall b
                JOIN Overs o ON b.OverId = o.Id
                JOIN Players p ON b.PlayerOutID = p.PlayerID
                WHERE o.CricketMatchID = @MatchId AND o.Innings = @Innings AND b.IsWicket = 1";
            
            var fowData = await connection.QueryAsync<dynamic>(fowSql, new { MatchId = matchId, Innings = innings });
            var fowList = fowData.Select(d => {
                int totalBalls = Convert.ToInt32(d.TotalBalls);
                return new FallOfWicketDto {
                    PlayerName = d.PlayerName,
                    Runs = Convert.ToInt32(d.CumulativeRuns),
                    WicketNumber = Convert.ToInt32(d.WicketNumber),
                    Over = $"{totalBalls / 6}.{totalBalls % 6}"
                };
            }).ToList();

            return new InningsScorecardDto
            {
                TeamName = teamName,
                TotalRuns = stats.TotalRuns,
                Wickets = stats.Wickets,
                Overs = stats.Overs,
                Batting = battingList,
                Bowling = bowlingList,
                FallOfWickets = fowList
            };
        }

        [HttpGet("GetSquads")]
        public async Task<IActionResult> GetSquads(int matchId)
        {
            using (var connection = _context.CreateConnection())
            {
                var matchInfo = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
                    SELECT ts.TeamAID, ts.TeamBID, ta.TeamName as TeamAName, tb.TeamName as TeamBName
                    FROM CricketMatch cm
                    JOIN TeamSchedule ts ON cm.TeamScheduleID = ts.TeamScheduleID
                    LEFT JOIN Teams ta ON ts.TeamAID = ta.TeamsID
                    LEFT JOIN Teams tb ON ts.TeamBID = tb.TeamsID
                    WHERE cm.CricketMatchID = @MatchId", new { MatchId = matchId });

                if (matchInfo == null) return NotFound();

                int teamAId = (int)matchInfo.TeamAID;
                int teamBId = (int)matchInfo.TeamBID;

                var playerSql = @"
                    SELECT p.PlayerID as PlayerId, p.FullName, p.PlayerImage, pr.RoleName
                    FROM Players p
                    JOIN PlayerRole pr ON p.PlayerRoleID = pr.PlayerRoleID
                    WHERE p.TeamsID = @TeamId";

                var teamAPlayers = await connection.QueryAsync<PlayerDto>(playerSql, new { TeamId = teamAId });
                var teamBPlayers = await connection.QueryAsync<PlayerDto>(playerSql, new { TeamId = teamBId });

                return Ok(new SquadDto
                {
                    TeamAName = matchInfo.TeamAName,
                    TeamBName = matchInfo.TeamBName,
                    TeamAPlayers = teamAPlayers.ToList(),
                    TeamBPlayers = teamBPlayers.ToList()
                });
            }
        }

        [HttpGet("GetLiveScore")]
        public async Task<IActionResult> GetLiveScore(int matchId)
        {
            using (var connection = _context.CreateConnection())
            {
               var stats = await GetMatchStats(connection, null, matchId);
               return Ok(stats);
            }
        }

        private async Task<Overs> GetOrCreateOver(IDbConnection connection, IDbTransaction transaction, int matchId, int bowlerId)
        {
            // Fetch match info for current innings
            var matchInfo = await connection.QueryFirstOrDefaultAsync<CricketMatch>("SELECT * FROM CricketMatch WHERE CricketMatchID = @MatchId", new { MatchId = matchId }, transaction);
            int currentInnings = matchInfo?.CurrentInnings ?? 1;

            // Logic: Find the latest over for THIS innings. Check if it is complete (6 valid balls).
            var lastOverSql = "SELECT TOP 1 * FROM Overs WHERE CricketMatchID = @MatchId AND Innings = @Innings ORDER BY Id DESC";
            var lastOver = await connection.QueryFirstOrDefaultAsync<Overs>(lastOverSql, new { MatchId = matchId, Innings = currentInnings }, transaction);

            if (lastOver != null)
            {
                var ballCountSql = "SELECT COUNT(*) FROM MatchBallByBall WHERE OverId = @OverId AND BallType NOT IN ('Wide', 'NoBall')";
                var ballCount = await connection.ExecuteScalarAsync<int>(ballCountSql, new { OverId = lastOver.Id }, transaction);

                if (ballCount < 6)
                {
                    // Current over continues
                    return lastOver;
                }
            }

            // Validation: Cannot bowl consecutive overs
            if (lastOver != null && lastOver.BowlerId == bowlerId)
            {
                throw new InvalidOperationException("Same bowler cannot bowl consecutive overs.");
            }

            // Create New Over
            var newOverNumber = (lastOver?.OverNumber ?? 0) + 1;
            var insertOverSql = @"
                INSERT INTO Overs (CricketMatchID, BowlerId, Innings, OverNumber)
                OUTPUT INSERTED.Id
                VALUES (@MatchId, @BowlerId, @Innings, @OverNumber)";

            var newOverId = await connection.ExecuteScalarAsync<int>(insertOverSql, new 
            { 
                MatchId = matchId, 
                BowlerId = bowlerId,
                Innings = currentInnings,
                OverNumber = newOverNumber
            }, transaction);

            return new Overs { Id = newOverId, CricketMatchID = matchId, BowlerId = bowlerId, OverNumber = newOverNumber, Innings = currentInnings };
        }

        private async Task<MatchStatsDto> GetMatchStats(IDbConnection connection, IDbTransaction transaction, int matchId)
        {
            // 1. Get Match Info to identify current players and innings
             var matchSql = @"
                SELECT 
                    cm.StrikerPlayerID, cm.NonStrikerPlayerID, cm.BowlerPlayerID, cm.CurrentInnings,
                    ts.TeamAID, ts.TeamBID, ta.TeamName as TeamAName, tb.TeamName as TeamBName,
                    cm.TossWinnerTeamID, cm.TossChoice, cm.MatchStatus
                FROM CricketMatch cm
                JOIN TeamSchedule ts ON cm.TeamScheduleID = ts.TeamScheduleID
                LEFT JOIN Teams ta ON ts.TeamAID = ta.TeamsID
                LEFT JOIN Teams tb ON ts.TeamBID = tb.TeamsID
                WHERE cm.CricketMatchID = @MatchId";
            var matchInfo = await connection.QueryFirstOrDefaultAsync<dynamic>(matchSql, new { MatchId = matchId }, transaction);
 
            int strikerId = (matchInfo?.StrikerPlayerID != null) ? (int)matchInfo.StrikerPlayerID : 0;
            int nonStrikerId = (matchInfo?.NonStrikerPlayerID != null) ? (int)matchInfo.NonStrikerPlayerID : 0;
            int bowlerId = (matchInfo?.BowlerPlayerID != null) ? (int)matchInfo.BowlerPlayerID : 0;
            int currentInnings = (matchInfo?.CurrentInnings != null) ? (int)matchInfo.CurrentInnings : 1;
            string matchStatus = matchInfo?.MatchStatus ?? "Live";

            // Get Recent Balls
            var recentBallsSql = @"
                SELECT TOP 8 
                    CASE 
                        WHEN IsWicket = 1 THEN 'W'
                        WHEN BallType = 'Wide' THEN CAST(Run AS VARCHAR) + 'wd'
                        WHEN BallType = 'NoBall' THEN CAST(Run AS VARCHAR) + 'nb'
                        WHEN IsBye = 1 THEN CAST(Run AS VARCHAR) + 'lb'
                        ELSE CAST(Run AS VARCHAR)
                    END as BallDetail
                FROM MatchBallByBall b
                JOIN Overs o ON b.OverId = o.Id
                WHERE o.CricketMatchID = @MatchId AND o.Innings = @Innings
                ORDER BY b.BallID DESC";
            var recentBalls = await connection.QueryAsync<string>(recentBallsSql, new { MatchId = matchId, Innings = currentInnings }, transaction);
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
 
            // 2. Aggregate Match Totals for current innings
             var totalSql = @"
                SELECT 
                    SUM(Run) as TotalRuns,
                    COUNT(CASE WHEN IsWicket = 1 THEN 1 END) as TotalWickets,
                    (SELECT COUNT(*) FROM MatchBallByBall b JOIN Overs o ON b.OverId = o.Id WHERE o.CricketMatchID = @MatchId AND o.Innings = @Innings AND b.BallType NOT IN ('Wide', 'NoBall')) as TotalBalls,
                    (SELECT SUM(Run) FROM MatchBallByBall b JOIN Overs o ON b.OverId = o.Id WHERE o.CricketMatchID = @MatchId AND o.Innings = 1) as Innings1Runs,
                    (SELECT COUNT(*) FROM MatchBallByBall b JOIN Overs o ON b.OverId = o.Id WHERE o.CricketMatchID = @MatchId AND o.Innings = 1 AND b.IsWicket = 1) as Innings1Wickets
                FROM MatchBallByBall b
                JOIN Overs o ON b.OverId = o.Id
                WHERE o.CricketMatchID = @MatchId AND o.Innings = @Innings
            ";
            var totalStats = await connection.QueryFirstOrDefaultAsync(totalSql, new { MatchId = matchId, Innings = currentInnings }, transaction);
            
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
                var matchInfo2 = await connection.QueryFirstOrDefaultAsync<dynamic>("SELECT Overs FROM CricketMatch WHERE CricketMatchID = @MatchId", new { MatchId = matchId }, transaction);
                int matchOvers = (matchInfo2?.Overs != null) ? (int)matchInfo2.Overs : 20;
                int totalMatchBalls = matchOvers * 6;

                var targetSql = @"
                    SELECT SUM(Run) 
                    FROM MatchBallByBall b 
                    JOIN Overs o ON b.OverId = o.Id 
                    WHERE o.CricketMatchID = @MatchId AND o.Innings = 1";
                var innings1Runs = await connection.ExecuteScalarAsync<int?>(targetSql, new { MatchId = matchId }, transaction);
                if (innings1Runs.HasValue)
                {
                    target = innings1Runs.Value + 1;
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
                 var sql = @"
                    SELECT 
                        p.FullName as PlayerName,
                        p.PlayerImage,
                        SUM(CASE 
                            WHEN b.BallType = 'Normal' AND (b.IsBye = 0 OR b.IsBye IS NULL) THEN b.Run 
                            WHEN b.BallType = 'NoBall' AND (b.IsBye = 0 OR b.IsBye IS NULL) THEN (b.Run - 1) 
                            ELSE 0 END) as Runs,
                        COUNT(CASE WHEN b.BallType != 'Wide' THEN 1 END) as Balls,
                        COUNT(CASE WHEN (b.BallType = 'Normal' AND b.Run = 4 AND (b.IsBye = 0 OR b.IsBye IS NULL)) OR (b.BallType = 'NoBall' AND b.Run = 5 AND (b.IsBye = 0 OR b.IsBye IS NULL)) THEN 1 END) as Fours,
                        COUNT(CASE WHEN (b.BallType = 'Normal' AND b.Run = 6 AND (b.IsBye = 0 OR b.IsBye IS NULL)) OR (b.BallType = 'NoBall' AND b.Run = 7 AND (b.IsBye = 0 OR b.IsBye IS NULL)) THEN 1 END) as Sixes
                    FROM Players p
                    LEFT JOIN (
                        SELECT bb.* FROM MatchBallByBall bb
                        JOIN Overs oo ON bb.OverId = oo.Id
                        WHERE oo.CricketMatchID = @MatchId AND oo.Innings = @Innings
                    ) b ON p.PlayerID = b.StrikerPlayerID
                    WHERE p.PlayerID = @PlayerId
                    GROUP BY p.FullName, p.PlayerImage"; 
                 
                 var res = await connection.QueryFirstOrDefaultAsync(sql, new { MatchId = matchId, PlayerId = playerId, Innings = currentInnings }, transaction);
                 
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
                  var sql = @"
                    SELECT 
                        p.FullName as PlayerName,
                        p.PlayerImage,
                        SUM(CASE 
                            WHEN (b.IsBye = 1 OR b.IsBye IS NULL) AND b.BallType = 'Normal' THEN 0 
                            WHEN (b.IsBye = 1 OR b.IsBye IS NULL) AND b.BallType = 'NoBall' THEN 1 -- Bowler still gets the penalty 
                            ELSE b.Run END) as RunsConceded,
                        COUNT(CASE WHEN IsWicket = 1 AND WicketType != 'Run Out' THEN 1 END) as Wickets,
                        COUNT(CASE WHEN BallType NOT IN ('Wide', 'NoBall') THEN 1 END) as ValidBalls
                    FROM Players p
                    LEFT JOIN (
                        SELECT bb.* FROM MatchBallByBall bb
                        JOIN Overs oo ON bb.OverId = oo.Id
                        WHERE oo.CricketMatchID = @MatchId AND oo.Innings = @Innings
                    ) b ON p.PlayerID = b.BowlerPlayerID
                    WHERE p.PlayerID = @PlayerId
                    GROUP BY p.FullName, p.PlayerImage";
                 
                 var res = await connection.QueryFirstOrDefaultAsync(sql, new { MatchId = matchId, PlayerId = playerId, Innings = currentInnings }, transaction);

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

            // 5. Get List of Dismissed Players for current innings
            var outPlayersSql = "SELECT DISTINCT PlayerOutID FROM MatchBallByBall b JOIN Overs o ON b.OverId = o.Id WHERE o.CricketMatchID = @MatchId AND o.Innings = @Innings AND PlayerOutID IS NOT NULL";
            var outPlayerIds = await connection.QueryAsync<int>(outPlayersSql, new { MatchId = matchId, Innings = currentInnings }, transaction);

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

                var matchInfo = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
                    SELECT 
                        ts.TeamAID, ts.TeamBID, ta.TeamName as TeamAName, tb.TeamName as TeamBName,
                        cm.TossWinnerTeamID, cm.TossChoice
                    FROM CricketMatch cm
                    INNER JOIN TeamSchedule ts ON cm.TeamScheduleID = ts.TeamScheduleID
                    LEFT JOIN Teams ta ON ta.TeamsID = ts.TeamAID
                    LEFT JOIN Teams tb ON tb.TeamsID = ts.TeamBID
                    WHERE cm.CricketMatchID = @MatchId;
                ", new { MatchId = matchId });

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

                return Ok(summary);
            }
        }

        private async Task<string?> GetWinnerMessageInternal(IDbConnection connection, IDbTransaction? transaction, int matchId)
        {
            var innings1 = await GetMatchStatsInnings(connection, transaction, matchId, 1);
            var innings2 = await GetMatchStatsInnings(connection, transaction, matchId, 2);

            var matchInfo = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
                SELECT 
                    ts.TeamAID, ts.TeamBID, ta.TeamName as TeamAName, tb.TeamName as TeamBName,
                    cm.TossWinnerTeamID, cm.TossChoice
                FROM CricketMatch cm
                INNER JOIN TeamSchedule ts ON cm.TeamScheduleID = ts.TeamScheduleID
                LEFT JOIN Teams ta ON ta.TeamsID = ts.TeamAID
                LEFT JOIN Teams tb ON tb.TeamsID = ts.TeamBID
                WHERE cm.CricketMatchID = @MatchId", new { MatchId = matchId }, transaction);

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

            var matchInfo = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
                SELECT 
                    ts.TeamAID, ts.TeamBID,
                    cm.TossWinnerTeamID, cm.TossChoice
                FROM CricketMatch cm
                INNER JOIN TeamSchedule ts ON cm.TeamScheduleID = ts.TeamScheduleID
                WHERE cm.CricketMatchID = @MatchId", new { MatchId = matchId }, transaction);

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
             // Detailed summary for a specific innings
             var totalSql = @"
                SELECT 
                    SUM(Run) as TotalRuns,
                    COUNT(CASE WHEN IsWicket = 1 THEN 1 END) as TotalWickets,
                    (SELECT COUNT(*) FROM MatchBallByBall b JOIN Overs o ON b.OverId = o.Id WHERE o.CricketMatchID = @MatchId AND o.Innings = @Innings AND b.BallType NOT IN ('Wide', 'NoBall')) as TotalBalls
                FROM MatchBallByBall b
                JOIN Overs o ON b.OverId = o.Id
                WHERE o.CricketMatchID = @MatchId AND o.Innings = @Innings
            ";
            var totalStats = await connection.QueryFirstOrDefaultAsync(totalSql, new { MatchId = matchId, Innings = innings }, transaction);
            
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
    }

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

    public class BatsmanStatsDto {
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

    public class BowlerStatsDto {
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
        public List<PlayerDto> TeamAPlayers { get; set; } = new List<PlayerDto>();
        public List<PlayerDto> TeamBPlayers { get; set; } = new List<PlayerDto>();
    }

    public class PlayerDto
    {
        public int PlayerId { get; set; }
        public string FullName { get; set; }
        public string PlayerImage { get; set; }
        public string RoleName { get; set; }
    }
}
