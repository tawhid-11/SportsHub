using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SportsHubBackend.DBContext;
using SportsHubBackend.Model;
using System.Data;

namespace SportsHubBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerController : ControllerBase
    {
        private readonly DapperContext _context;

        public PlayerController(DapperContext context)
        {
            _context = context;
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
                    //teams.TeamLogo = "/images/" + fileName;
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
                
                // Add UserId if provided
                if (player.UserId.HasValue && player.UserId.Value > 0)
                {
                    perameter.Add("UserId", player.UserId.Value, DbType.Int32);
                }


                using (var connetion = _context.CreateConnection())
                {
                    var result = await connetion.QueryAsync<dynamic>("SP_Players", perameter, commandType: CommandType.StoredProcedure);
                    var rdata = new
                    {
                        success = true,
                        Message = "Player added successfully",
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

            //var query = @"
            //INSERT INTO Tournaments(TournamentName, Location, StartDate, EndDate, TournamentType, MaxTeams, Status, ContactNumber)
            //    VALUES(@TournamentName, @Location, @StartDate, @EndDate, @TournamentType, @MaxTeams, @Status, @ContactNumber)";
            ////using var connection = _context.CreateConnection();

            //connection.Execute(query, tournament);
            //return Ok("Tournament added successfully");
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

            
        }
}
