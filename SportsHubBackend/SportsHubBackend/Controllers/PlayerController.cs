using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SportsHubBackend.DBContext;
using SportsHubBackend.Model;
using SportsHubBackend.Services;
using System.Data;

namespace SportsHubBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerController : ControllerBase
    {
        private readonly DapperContext _context;
        private readonly IEmailService _emailService;

        public PlayerController(DapperContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var perameter = new DynamicParameters();
                perameter.Add("Flag", 1);
                using (var connetion = _context.CreateConnection())
                {
                    var result = await connetion.QueryAsync<dynamic>("SP_Players", perameter, commandType: CommandType.StoredProcedure);
                    var rdata = new
                    {
                        success = true,
                        Message = "Payers fetched successfully",
                        Data = result
                    };
                    return Ok(rdata);
                }
            }
            catch (Exception ex)
            {
                var rdata = new
                {
                    success = false,
                    Message = "Error - " + ex.Message,
                };
                return BadRequest(rdata);
            }



        }
        [HttpGet("GetPlayerById")]
        public async Task<IActionResult> GetPlayerTypeById(int PlayerID)
        {
            try
            {
                var perameter = new DynamicParameters();
                perameter.Add("Flag", 6);
                perameter.Add("@PlayerID", PlayerID);
                using (var connetion = _context.CreateConnection())
                {
                    var result = await connetion.QueryFirstOrDefaultAsync<dynamic>("SP_Players", perameter, commandType: CommandType.StoredProcedure);
                    var rdata = new
                    {
                        success = true,
                        Message = "Player fetched successfully",
                        Data = result
                    };
                    return Ok(rdata);
                }
            }
            catch (Exception ex)
            {
                var rdata = new
                {
                    success = false,
                    Message = "Error - " + ex.Message,
                };
                return BadRequest(rdata);
            }
        }
        [HttpPost("Player")]
        public async Task<IActionResult> Post([FromForm] Player player)
        {
            try
            {
                var fileName = "";
                //save image to wwwroot/images
                var imageFile = player.PlayerImage;
                if (imageFile != null && imageFile.Length > 0)
                {
                    var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    if (!Directory.Exists(imagesPath))
                    {
                        Directory.CreateDirectory(imagesPath);
                    }
                    fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(imagesPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                }

                using var connection = _context.CreateConnection();
                int? assignedUserId = player.UserId;
                string autoPassword = "";

                // If no UserId is provided, register the user automatically
                if (!assignedUserId.HasValue || assignedUserId.Value == 0)
                {
                    // Check if email already exists
                    var existingUser = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "SELECT UserID FROM UserInfo WHERE Email = @Email", new { Email = player.Email });
                    
                    if (existingUser != null)
                    {
                        assignedUserId = existingUser.UserID;
                    }
                    else if (!string.IsNullOrEmpty(player.Email))
                    {
                        // Register new user for this player
                        autoPassword = "User@" + Guid.NewGuid().ToString().Substring(0, 6);
                        
                        var userParams = new DynamicParameters();
                        userParams.Add("Flag", 1); // Register flag in SP_UserInfo
                        userParams.Add("Name", player.FullName);
                        userParams.Add("Email", player.Email);
                        userParams.Add("Phone", ""); 
                        userParams.Add("UserType", "Player");
                        userParams.Add("Password", autoPassword);

                        var userResult = await connection.QueryFirstOrDefaultAsync<dynamic>(
                            "SP_UserInfo", userParams, commandType: CommandType.StoredProcedure);
                        
                        if (userResult != null)
                        {
                            assignedUserId = userResult.UserID;
                        }
                    }
                }

                // --- Invitation Email Logic ---
                if (assignedUserId.HasValue && !string.IsNullOrEmpty(player.Email) && !string.IsNullOrEmpty(autoPassword))
                {
                    // Get Team Name
                    var teamName = await connection.QueryFirstOrDefaultAsync<string>(
                        "SELECT TeamName FROM Teams WHERE TeamsID = @TeamsID", new { TeamsID = player.TeamsID });
                    
                    // Send Email asynchronously
                    _ = Task.Run(async () => {
                        await _emailService.SendPlayerInvitationEmailAsync(
                            player.Email, 
                            player.FullName ?? "Player", 
                            teamName ?? "Your Team", 
                            player.Email, 
                            autoPassword
                        );
                    });
                }

                var perameter = new DynamicParameters();
                perameter.Add("Flag", 2);
                perameter.Add("TeamsID ", player.TeamsID, DbType.Int32);
                perameter.Add("PlayerRoleID", player.PlayerRoleID, DbType.Int32);
                perameter.Add("PlayerImage", "/images/" + fileName, DbType.String);
                perameter.Add("FullName", player.FullName, DbType.String);
                perameter.Add("Nationality", player.Nationality, DbType.String);
                perameter.Add("DateOfBirth", player.DateOfBirth, DbType.Date);
                perameter.Add("NickName ", player.NickName, DbType.String);
                perameter.Add("BattingStyle", player.BattingStyle, DbType.String);
                perameter.Add("BowlingStyle", player.BowlingStyle, DbType.String);
                perameter.Add("IsActive", player.IsActive, DbType.Boolean);
                
                if (assignedUserId.HasValue)
                {
                    perameter.Add("UserId", assignedUserId.Value, DbType.Int32);
                }

                var result = await connection.QueryAsync<dynamic>("SP_Players", perameter, commandType: CommandType.StoredProcedure);
                
                var rdata = new
                {
                    success = true,
                    Message = "Player added successfully" + (!string.IsNullOrEmpty(autoPassword) ? " and invitation email sent." : "."),
                    Data = result
                };
                return Ok(rdata);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, Message = "Error - " + ex.Message });
            }
        }
        [HttpPut("UpdatePlayer/{id}")]
        public async Task<IActionResult> Put(int id, [FromForm] Player player)
        {
            try
            {
                var fileName = "";
                //save image to wwwroot/images
                var imageFile = player.PlayerImage;
                if (imageFile != null && imageFile.Length > 0)
                {
                    var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    if (!Directory.Exists(imagesPath))
                    {
                        Directory.CreateDirectory(imagesPath);
                    }
                    fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(imagesPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    //teams.TeamLogo = "/images/" + fileName;
                }
                var perameter = new DynamicParameters();
                perameter.Add("Flag", 3);

                perameter.Add("TeamsID ", player.TeamsID, DbType.Int32);
                perameter.Add("PlayerID ", id, DbType.Int32);
                perameter.Add("PlayerRoleID", player.PlayerRoleID, DbType.Int32);
                if(fileName != "")
                {
                    perameter.Add("PlayerImage", "/images/" + fileName, DbType.String);
                }
               
                perameter.Add("FullName", player.FullName, DbType.String);
                perameter.Add("Nationality", player.Nationality, DbType.String);
                perameter.Add("DateOfBirth", player.DateOfBirth, DbType.Date);
                perameter.Add("NickName ", player.NickName, DbType.String);
                perameter.Add("BattingStyle", player.BattingStyle, DbType.String);
                perameter.Add("BowlingStyle", player.BowlingStyle, DbType.String);
                perameter.Add("IsActive", player.IsActive, DbType.Boolean);

                using (var connetion = _context.CreateConnection())
                {
                    var result = await connetion.QueryAsync<dynamic>("SP_Players", perameter, commandType: CommandType.StoredProcedure);
                    var rdata = new
                    {
                        success = true,
                        Message = "Player updated successfully",
                        Data = result
                    };
                    return Ok(rdata);
                }
            }
            catch (Exception ex)
            {
                var rdata = new
                {
                    success = false,
                    Message = "Error - " + ex.Message,
                };
                return BadRequest(rdata);

            }
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var perameter = new DynamicParameters();
                perameter.Add("Flag", 5);
                perameter.Add("PlayerID", id);
                using (var connetion = _context.CreateConnection())
                {
                    var result = await connetion.QueryAsync<dynamic>("SP_Players", perameter, commandType: CommandType.StoredProcedure);
                    var rdata = new
                    {
                        success = true,
                        Message = "Player deleted successfully",
                        Data = result
                    };
                    return Ok(rdata);
                }
            }
            catch (Exception ex)
            {
                var rdata = new
                {
                    success = false,
                    Message = "Error - " + ex.Message,
                };
                return BadRequest(rdata);
            }
        }
        [HttpGet("GetPlayerByTeamOwnerId")]
        public async Task<IActionResult> GetPlayerByTeamOwnerId(int id)
        {
            try
            {
                var perameter = new DynamicParameters();
                perameter.Add("@Flag", 7);
                perameter.Add("@UserId", id);
                using (var connetion = _context.CreateConnection())
                {
                    var result = await connetion.QueryAsync<dynamic>("SP_Players", perameter, commandType: CommandType.StoredProcedure);
                    var rdata = new
                    {
                        success = true,
                        Message = "Payers fetched successfully",
                        Data = result
                    };
                    return Ok(rdata);
                }
            }
            catch (Exception ex)
            {
                var rdata = new
                {
                    success = false,
                    Message = "Error - " + ex.Message,
                };
                return BadRequest(rdata);
            }
        }

        [HttpGet("GetPlayerByUserId")]
        public async Task<IActionResult> GetPlayerByUserId(int userId)
        {
            try
            {
                using (var connection = _context.CreateConnection())
                {
                    // Directly query the Players table since the SP_Players might not have a specific flag for this if not defined
                    // Or we can query directly
                    var query = "SELECT * FROM Players WHERE UserId = @UserId";
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(query, new { UserId = userId });
                    
                    if(result == null)
                    {
                         return Ok(new { success = false, Message = "Player profile not found for this user" });
                    }

                    return Ok(new
                    {
                        success = true,
                        Message = "Player fetched successfully",
                        Data = result
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    Message = "Error - " + ex.Message,
                });
            }
        }

        [HttpGet("GetAllWithTeamName")]
        public async Task<IActionResult> GetAllWithTeamName()
        {
            try
            {
                using (var connection = _context.CreateConnection())
                {
                    var query = @"
                        SELECT 
                            p.PlayerID,
                            p.TeamsID,
                            p.PlayerRoleID,
                            p.PlayerImage,
                            p.FullName,
                            p.Nationality,
                            p.DateOfBirth,
                            p.BirthPlace,
                            p.NickName,
                            p.BattingStyle,
                            p.BowlingStyle,
                            p.IsActive,
                            t.TeamName,
                            pr.RoleName,
                            pr.Description
                        FROM Players p
                        LEFT JOIN Teams t ON p.TeamsID = t.TeamsID
                        LEFT JOIN PlayerRole pr ON p.PlayerRoleID = pr.PlayerRoleID
                        ORDER BY t.TeamName ASC";

                    var result = await connection.QueryAsync<dynamic>(query);
                    var rdata = new
                    {
                        success = true,
                        Message = "Players with team information fetched successfully",
                        Data = result
                    };
                    return Ok(rdata);
                }
            }
            catch (Exception ex)
            {
                var rdata = new
                {
                    success = false,
                    Message = "Error - " + ex.Message,
                };
                return BadRequest(rdata);
            }
        }

        [HttpGet("GetPlayerStats")]
        public async Task<IActionResult> GetPlayerStats(int playerId)
        {
            try
            {
                using (var connection = _context.CreateConnection())
                {
                    var battingQuery = @"
                        SELECT 
                            ISNULL(COUNT(DISTINCT o.CricketMatchID), 0) as MatchesPlayed,
                            ISNULL(SUM(m.Run), 0) as TotalRuns,
                            ISNULL(COUNT(m.BallID), 0) as BallsFaced,
                            (SELECT ISNULL(MAX(MatchRuns), 0) FROM (
                                SELECT SUM(Run) as MatchRuns 
                                FROM MatchBallByBall m2 
                                JOIN Overs o2 ON m2.OverId = o2.Id 
                                WHERE m2.StrikerPlayerID = @PlayerID AND m2.BallType != 'Wide' 
                                GROUP BY o2.CricketMatchID
                            ) as matchScores) as HighestScore
                        FROM MatchBallByBall m
                        JOIN Overs o ON m.OverId = o.Id
                        WHERE m.StrikerPlayerID = @PlayerID AND m.BallType != 'Wide'";

                    var bowlingQuery = @"
                        SELECT
                            ISNULL(SUM(CAST(m.IsWicket AS INT)), 0) as TotalWickets,
                            ISNULL(COUNT(m.BallID), 0) as BallsBowled,
                            ISNULL(SUM(m.Run), 0) as RunsConceded
                        FROM MatchBallByBall m
                        JOIN Overs o ON m.OverId = o.Id
                        WHERE m.BowlerPlayerID = @PlayerID AND m.BallType != 'Wide' AND m.BallType != 'NoBall'";

                    var battingStats = await connection.QueryFirstOrDefaultAsync<dynamic>(battingQuery, new { PlayerID = playerId });
                    var bowlingStats = await connection.QueryFirstOrDefaultAsync<dynamic>(bowlingQuery, new { PlayerID = playerId });

                    int runs = battingStats?.TotalRuns ?? 0;
                    int ballsFaced = battingStats?.BallsFaced ?? 0;
                    double strikeRate = ballsFaced > 0 ? Math.Round(((double)runs / ballsFaced) * 100, 2) : 0;

                    int wickets = bowlingStats?.TotalWickets ?? 0;
                    int ballsBowled = bowlingStats?.BallsBowled ?? 0;
                    int runsConceded = bowlingStats?.RunsConceded ?? 0;
                    double overs = ballsBowled / 6.0;
                    double economy = overs > 0 ? Math.Round(runsConceded / overs, 2) : 0;

                    var stats = new
                    {
                        MatchesPlayed = battingStats?.MatchesPlayed ?? 0,
                        TotalRuns = runs,
                        HighestScore = battingStats?.HighestScore ?? 0,
                        BallsFaced = ballsFaced,
                        StrikeRate = strikeRate,
                        TotalWickets = wickets,
                        BallsBowled = ballsBowled,
                        RunsConceded = runsConceded,
                        Economy = economy,
                        OversBowled = Math.Round(overs, 1)
                    };

                    return Ok(new
                    {
                        success = true,
                        Message = "Player stats fetched successfully",
                        Data = stats
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

        [HttpGet("GetPlayerMatchHistory")]
        public async Task<IActionResult> GetPlayerMatchHistory(int playerId)
        {
            try
            {
                using (var connection = _context.CreateConnection())
                {
                    var query = @"
                        SELECT 
                            cm.CricketMatchID,
                            ts.ScheduledDate,
                            tA.TeamName as TeamA,
                            tB.TeamName as TeamB,
                            (SELECT ISNULL(SUM(Run), 0) FROM MatchBallByBall m2 JOIN Overs o2 ON m2.OverId = o2.Id WHERE m2.StrikerPlayerID = @PlayerID AND o2.CricketMatchID = cm.CricketMatchID AND m2.BallType != 'Wide') as RunsScored,
                            (SELECT ISNULL(COUNT(m2.BallID), 0) FROM MatchBallByBall m2 JOIN Overs o2 ON m2.OverId = o2.Id WHERE m2.StrikerPlayerID = @PlayerID AND o2.CricketMatchID = cm.CricketMatchID AND m2.BallType != 'Wide') as BallsFaced,
                            (SELECT ISNULL(SUM(CAST(m2.IsWicket AS INT)), 0) FROM MatchBallByBall m2 JOIN Overs o2 ON m2.OverId = o2.Id WHERE m2.BowlerPlayerID = @PlayerID AND o2.CricketMatchID = cm.CricketMatchID) as WicketsTaken,
                            (SELECT ISNULL(SUM(m2.Run), 0) FROM MatchBallByBall m2 JOIN Overs o2 ON m2.OverId = o2.Id WHERE m2.BowlerPlayerID = @PlayerID AND o2.CricketMatchID = cm.CricketMatchID AND m2.BallType != 'Wide' AND m2.BallType != 'NoBall') as RunsConceded
                        FROM CricketMatch cm
                        JOIN TeamSchedule ts ON cm.TeamScheduleID = ts.TeamScheduleID
                        JOIN Teams tA ON ts.TeamAID = tA.TeamsID
                        JOIN Teams tB ON ts.TeamBID = tB.TeamsID
                        WHERE EXISTS (
                            SELECT 1 FROM MatchBallByBall m3 JOIN Overs o3 ON m3.OverId = o3.Id 
                            WHERE o3.CricketMatchID = cm.CricketMatchID AND (m3.StrikerPlayerID = @PlayerID OR m3.BowlerPlayerID = @PlayerID)
                        )
                        ORDER BY ts.ScheduledDate DESC";

                    var result = await connection.QueryAsync<dynamic>(query, new { PlayerID = playerId });
                    
                    return Ok(new {
                        success = true,
                        Message = "Player match history fetched successfully",
                        Data = result
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, Message = "Error - " + ex.Message });
            }
        }
        [HttpGet("GetTournamentPerformers")]
        public async Task<IActionResult> GetTournamentPerformers(int tournamentId)
        {
            try
            {
                using (var connection = _context.CreateConnection())
                {
                    var battingQuery = @"
                        SELECT TOP 5
                            p.PlayerID,
                            p.FullName as Name,
                            p.PlayerImage as Image,
                            t.TeamName,
                            ISNULL(SUM(m.Run), 0) as TotalRuns
                        FROM MatchBallByBall m
                        JOIN Players p ON m.StrikerPlayerID = p.PlayerID
                        JOIN Teams t ON p.TeamsID = t.TeamsID
                        JOIN Overs o ON m.OverId = o.Id
                        JOIN CricketMatch cm ON o.CricketMatchID = cm.CricketMatchID
                        JOIN TeamSchedule ts ON cm.TeamScheduleID = ts.TeamScheduleID
                        WHERE ts.TournamentID = @TournamentID AND m.BallType != 'Wide'
                        GROUP BY p.PlayerID, p.FullName, p.PlayerImage, t.TeamName
                        ORDER BY TotalRuns DESC";

                    var bowlingQuery = @"
                        SELECT TOP 5
                            p.PlayerID,
                            p.FullName as Name,
                            p.PlayerImage as Image,
                            t.TeamName,
                            ISNULL(SUM(CAST(m.IsWicket AS INT)), 0) as TotalWickets
                        FROM MatchBallByBall m
                        JOIN Players p ON m.BowlerPlayerID = p.PlayerID
                        JOIN Teams t ON p.TeamsID = t.TeamsID
                        JOIN Overs o ON m.OverId = o.Id
                        JOIN CricketMatch cm ON o.CricketMatchID = cm.CricketMatchID
                        JOIN TeamSchedule ts ON cm.TeamScheduleID = ts.TeamScheduleID
                        WHERE ts.TournamentID = @TournamentID
                        GROUP BY p.PlayerID, p.FullName, p.PlayerImage, t.TeamName
                        ORDER BY TotalWickets DESC";

                    var topBatters = await connection.QueryAsync<dynamic>(battingQuery, new { TournamentID = tournamentId });
                    var topBowlers = await connection.QueryAsync<dynamic>(bowlingQuery, new { TournamentID = tournamentId });

                    return Ok(new
                    {
                        success = true,
                        Data = new
                        {
                            topBatters = topBatters,
                            topBowlers = topBowlers
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
