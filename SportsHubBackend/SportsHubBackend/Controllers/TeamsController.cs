using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SportsHubBackend.DBContext;
using SportsHubBackend.Model;
using SportsHubBackend.Services;
using System.Data;
using System.Reflection;

namespace SportsHubBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamsController : ControllerBase
    {
        private readonly DapperContext _context;
        private readonly IBKashService _bkash;
        private readonly IEmailService _emailService;

        public TeamsController(DapperContext context, IBKashService bkash, IEmailService emailService)
        {
            _context = context;
            _bkash = bkash;
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
                    var result = await connetion.QueryAsync<dynamic>("SP_Teams", perameter, commandType: CommandType.StoredProcedure);
                    var rdata = new
                    {
                        success = true,
                        Message = "Teams fetched successfully",
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

        [HttpGet("GetTeamsById")]
        public async Task<IActionResult> GetTeamsById(int TeamsID)
        {
            try
            {
                var perameter = new DynamicParameters();
                perameter.Add("Flag", 6);
                perameter.Add("@TeamsID", TeamsID);
                using (var connetion = _context.CreateConnection())
                {
                    var result = await connetion.QueryFirstOrDefaultAsync<dynamic>("SP_Teams", perameter, commandType: CommandType.StoredProcedure);
                    var rdata = new
                    {
                        success = true,
                        Message = "Teams fetched successfully",
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
        [HttpPost("teams")]

        public async Task<IActionResult> Post([FromForm] Teams teams)
        {
            try
            {
                var fileName = "";
                //save image to wwwroot/images
                var imageFile = teams.TeamLogo;
                if (imageFile != null && imageFile.Length > 0)
                {
                    var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    if (!Directory.Exists(imagesPath))
                    {
                        Directory.CreateDirectory(imagesPath);
                    }
                     fileName= Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(imagesPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    //teams.TeamLogo = "/images/" + fileName;
                }

                var perameter = new DynamicParameters();
                perameter.Add("Flag", 2);
                perameter.Add("TeamName", teams.TeamName, DbType.String);
                perameter.Add("UserId", teams.UserId, DbType.Int32);
                perameter.Add("ShortName", teams.ShortName, DbType.String);
                perameter.Add("TeamLogo", "/images/"+fileName, DbType.String);
                perameter.Add("TeamOwnerName", teams.TeamOwnerName, DbType.String);
                perameter.Add("TeamOwnerEmail", teams.TeamOwnerEmail, DbType.String);
                perameter.Add("TeamOwnerPhoneNumber", teams.TeamOwnerPhoneNumber, DbType.String);
                perameter.Add("CoachName", teams.CoachName, DbType.String);
                perameter.Add("FoundedYear", teams.FoundedYear, DbType.Int32);
                perameter.Add("TotalPlayers", teams.TotalPlayers, DbType.Int32);             
                perameter.Add("IsActive", teams.IsActive, DbType.Boolean);


                using (var connetion = _context.CreateConnection())
                {
                    var result = await connetion.QueryFirstOrDefaultAsync<dynamic>("SP_Teams", perameter, commandType: CommandType.StoredProcedure);
                    
                    // Get user credentials from database
                    string userEmail = teams.TeamOwnerEmail;
                    string userPassword = teams.TeamOwnerEmail;
                    
                    if (teams.UserId > 0)
                    {
                        try
                        {
                            // Try to get user info using stored procedure
                            var userParam = new DynamicParameters();
                            userParam.Add("Flag", 4); // Assuming Flag 4 gets user by ID
                            userParam.Add("UserID", teams.UserId, DbType.Int32);
                            
                            var userInfo = await connetion.QueryFirstOrDefaultAsync<dynamic>(
                                "SP_UserInfo",
                                userParam,
                                commandType: CommandType.StoredProcedure
                            );
                            
                            if (userInfo != null)
                            {
                                userEmail = userInfo.Email?.ToString() ?? teams.TeamOwnerEmail;
                                userPassword = userInfo.Password?.ToString() ?? "";
                            }
                            
                            // If stored procedure doesn't work, try direct query
                            if (string.IsNullOrEmpty(userPassword))
                            {
                                var directUserInfo = await connetion.QueryFirstOrDefaultAsync<dynamic>(
                                    "SELECT Email, Password FROM UserInfo WHERE UserID = @UserId OR ID = @UserId",
                                    new { UserId = teams.UserId }
                                );
                                if (directUserInfo != null)
                                {
                                    userEmail = directUserInfo.Email?.ToString() ?? teams.TeamOwnerEmail;
                                    userPassword = directUserInfo.Password?.ToString() ?? "";
                                }
                            }
                        }
                        catch (Exception userEx)
                        {
                            // If user lookup fails, use team owner email
                            Console.WriteLine($"Failed to get user credentials: {userEx.Message}");
                            userEmail = teams.TeamOwnerEmail;
                        }
                    }

                    // Send email to team owner
                    try
                    {
                        await _emailService.SendTeamCreationEmailAsync(
                            teams.TeamOwnerEmail,
                            teams.TeamOwnerName,
                            teams.TeamName,
                            userEmail,
                            userPassword
                        );
                    }
                    catch (Exception emailEx)
                    {
                        // Log email error but don't fail the team creation
                        Console.WriteLine($"Failed to send email: {emailEx.Message}");
                    }

                    var rdata = new
                    {
                        success = true,
                        Message = "Teams added successfully",
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
        [HttpPost("TournamentTeamMapping")]

        public async Task<IActionResult> TournamentTeamMapping([FromBody] TournamentTeamMapping teamstournament)
        {
            try
            {

                var perameter = new DynamicParameters();
                perameter.Add("Flag", 2);
                perameter.Add("TournamentId", teamstournament.TournamentId, DbType.Int32);
                perameter.Add("TeamId", teamstournament.TeamId, DbType.Int32);
                perameter.Add("UserId", teamstournament.userId, DbType.Int32);




                using (var connetion = _context.CreateConnection())
                {
                    var result = await connetion.QueryFirstOrDefaultAsync<dynamic>("SP_TournamentTeamMapping", perameter, commandType: CommandType.StoredProcedure);
                    
                    if (result == null)
                    {
                        return BadRequest(new { success = false, Message = "Failed to create tournament team mapping in database." });
                    }

                    var tName = (string)result.TournamentName;
                    var sanitizedTName = tName.Replace(" ", "-");

                    var bkash = await _bkash.InitiatePaymentAsync(new PaymentRequest
                    {
                        Amount = result.RegistrationFee,
                        Currency = "BDT",
                        MerchantInvoiceNumber = $"INV-{sanitizedTName}-{DateTime.UtcNow.Ticks}",
                        SuccessUrl = "http://localhost:4200/payment-confirmation"
                    });

                    if (bkash.Success)
                    {
                        perameter.Add("bkashPaymentId", bkash.PaymentId);
                        perameter.Add("Flag", 1);
                        await connetion.QueryFirstOrDefaultAsync<dynamic>("SP_TournamentTeamMapping", perameter, commandType: CommandType.StoredProcedure);
                        return Ok(bkash);
                    }
                    else
                    {
                        return BadRequest(new { success = false, Message = "bKash payment initiation failed: " + bkash.Message });
                    }
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
        [HttpGet("Success_URL")]
        public async Task<IActionResult> SuccessUrl(string paymemtId)
        {
            var bkash = await _bkash.ConfirmPaymentAsync(paymemtId);
            using var con = _context.CreateConnection();
            await con.ExecuteAsync(
                      "SP_TournamentTeamMapping",
                      new
                      {
                          flag = 3,
                          bkashPaymentId = paymemtId
                      },
                      commandType: CommandType.StoredProcedure
                  );


            return Ok(bkash);
        }
        [HttpPut("UpdateTeams/{TeamsID}")]
        public async Task<IActionResult> Put(int TeamsID, [FromBody] Teams teams)
        {
            try
            {
                var perameter = new DynamicParameters();
                perameter.Add("Flag", 3);
                perameter.Add("TeamName", teams.TeamName, DbType.String);
                perameter.Add("UserId", teams.UserId, DbType.Int32);
                perameter.Add("ShortName", teams.ShortName, DbType.String);
                perameter.Add("TeamLogo", teams.TeamLogo, DbType.String);
                perameter.Add("TeamOwnerName", teams.TeamOwnerName, DbType.String);
                perameter.Add("TeamOwnerEmail", teams.TeamOwnerEmail, DbType.String);
                perameter.Add("TeamOwnerPhoneNumber", teams.TeamOwnerPhoneNumber, DbType.String);
                perameter.Add("CoachName", teams.CoachName, DbType.String);
                perameter.Add("FoundedYear", teams.FoundedYear, DbType.DateTime);
                perameter.Add("TotalPlayers", teams.TotalPlayers, DbType.Int32);
                perameter.Add("IsActive", teams.IsActive, DbType.Boolean);

                using (var connetion = _context.CreateConnection())
                {
                    var result = await connetion.QueryAsync<dynamic>("SP_Teams", perameter, commandType: CommandType.StoredProcedure);
                    var rdata = new
                    {
                        success = true,
                        Message = "Tournament updated successfully",
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


        [HttpDelete("{TeamsID}")]
        public async Task<IActionResult> Delete(int TeamsID, [FromBody] Teams teams)
        {
            try
            {
                var perameter = new DynamicParameters();
                perameter.Add("Flag", 4);
                perameter.Add("TeamsID", TeamsID);
                using (var connetion = _context.CreateConnection())
                {
                    var result = await connetion.QueryAsync<dynamic>("SP_Teams", perameter, commandType: CommandType.StoredProcedure);
                    var rdata = new
                    {
                        success = true,
                        Message = "Teams deleted successfully",
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
        [HttpGet("GetTeamIdbyUserId")]
        public async Task<IActionResult> GetTeamIdbyUserId(int id)
        {
            try
            {
                var perameter = new DynamicParameters();
                perameter.Add("@Flag", 7);
                perameter.Add("@UserId", id);
                using (var connetion = _context.CreateConnection())
                {
                    var result = await connetion.QueryFirstOrDefaultAsync<dynamic>("SP_Teams", perameter, commandType: CommandType.StoredProcedure);
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
        [HttpGet("GetTeamIdbyTournamentId")]
        public async Task<IActionResult> GetTeamIdbyTournamentId(int id)
        {
            try
            {
                var perameter = new DynamicParameters();
                perameter.Add("@Flag", 8);
                perameter.Add("@TournamentId", id);
                using (var connetion = _context.CreateConnection())
                {
                    var result = await connetion.QueryAsync<dynamic>("SP_Teams", perameter, commandType: CommandType.StoredProcedure);
                    var rdata = new
                    {
                        success = true,
                        Message = "Team Showing successfully",
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
        [HttpGet("GetPlayerbyTeamId")]
        public async Task<IActionResult> GetPlayerIdbyTeamId(int id)
        {
            try
            {
                var perameter = new DynamicParameters();
                perameter.Add("@Flag", 9);
                perameter.Add("@TeamsID", id);
                using (var connetion = _context.CreateConnection())
                {
                    var result = await connetion.QueryAsync<dynamic>("SP_Teams", perameter, commandType: CommandType.StoredProcedure);
                    var rdata = new
                    {
                        success = true,
                        Message = "Player Showing successfully",
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

        [HttpGet("GetAllWithTournament")]
        public async Task<IActionResult> GetAllWithTournament()
        {
            try
            {
                using (var connection = _context.CreateConnection())
                {
                    var query = @"
                        SELECT 
                            t.TeamsID,
                            t.TeamName,
                            t.ShortName,
                            t.TeamLogo,
                            t.TeamOwnerName,
                            t.TeamOwnerEmail,
                            t.TeamOwnerPhoneNumber,
                            t.CoachName,
                            t.FoundedYear,
                            t.TotalPlayers,
                            t.IsActive,
                            t.UserId,
                            ttm.TournamentId,
                            tr.TournamentName,
                            tr.TournamentID as TournamentID
                        FROM Teams t
                        LEFT JOIN TournamentTeamMapping ttm ON t.TeamsID = ttm.TeamId
                        LEFT JOIN Tournaments tr ON ttm.TournamentId = tr.TournamentID
                        ORDER BY t.TeamsID DESC";

                    var result = await connection.QueryAsync<dynamic>(query);
                    var rdata = new
                    {
                        success = true,
                        Message = "Teams with tournament information fetched successfully",
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
