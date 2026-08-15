using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SportsHubBackend.DBContext;
using SportsHubBackend.Model;
using System.Data;
using System.Reflection.Metadata;

namespace SportsHubBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TournamentsController : ControllerBase
    {
        private readonly DapperContext _context;

        public TournamentsController(DapperContext context)
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
                    var result = await connetion.QueryAsync<dynamic>("SP_Tournaments", perameter, commandType: CommandType.StoredProcedure);
                    var rdata = new
                    {
                        success = true,
                        Message = "Tournaments fetched successfully",
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


        [HttpGet("GetAllUpComing")]
        public async Task<IActionResult> GetAllUpComing()
        {
            try
            {
                var perameter = new DynamicParameters();
                perameter.Add("Flag", 9);
                using (var connetion = _context.CreateConnection())
                {
                    var result = await connetion.QueryAsync<dynamic>("SP_Tournaments", perameter, commandType: CommandType.StoredProcedure);
                    var rdata = new
                    {
                        success = true,
                        Message = "Tournaments fetched successfully",
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
        [HttpGet("GetTournamentsById")]
        public async Task<IActionResult> GetTournamentTypeById(int TournamentId)
        {
            try
            {
                var perameter = new DynamicParameters();
                perameter.Add("Flag", 6);
                perameter.Add("@TournamentId", TournamentId);
                using (var connetion = _context.CreateConnection())
                {
                    var result = await connetion.QueryFirstOrDefaultAsync<dynamic>("SP_Tournaments", perameter, commandType: CommandType.StoredProcedure);
                    var rdata = new
                    {
                        success = true,
                        Message = "Tournament fetched successfully",
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
        [HttpPost("tournaments")]

        public async Task<IActionResult> Post([FromBody] Tournaments tournament)
        {
            try
            {
                if (tournament.MatchPlayer > tournament.TotalPlayer)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Number of match players cannot be greater than total players."
                    });
                }
                if(tournament.RegistrationDeadline > tournament.StartDate)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Registration deadline cannot be after the tournament start date."
                    });
                }

                if(tournament.EndDate < tournament.StartDate)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "End date cannot be before the start date."
                    });
                }
                var perameter = new DynamicParameters();
                perameter.Add("Flag", 2);
                perameter.Add("TournamentName", tournament.TournamentName, DbType.String);
                perameter.Add("Prize", tournament.Prize, DbType.String);
                perameter.Add("Location", tournament.Location, DbType.String);
                perameter.Add("StartDate", tournament.StartDate, DbType.DateTime);
                perameter.Add("EndDate", tournament.EndDate, DbType.DateTime);
                perameter.Add("TournamentTypeID", tournament.TournamentTypeID, DbType.Int32);
                perameter.Add("RegistrationDeadline", tournament.RegistrationDeadline, DbType.DateTime);
                perameter.Add("RegistrationFee", tournament.RegistrationFee, DbType.Int32);
                perameter.Add("FieldFee", tournament.FieldFee, DbType.Int32);
                perameter.Add("MaxTeams", tournament.MaxTeams, DbType.Int32);
                perameter.Add("TotalPlayer", tournament.TotalPlayer, DbType.Int32);
                perameter.Add("MatchPlayer", tournament.MatchPlayer, DbType.Int32);
                perameter.Add("ExtraPlayer", tournament.ExtraPlayer, DbType.Int32);
                perameter.Add("Status", tournament.Status, DbType.String);
                perameter.Add("ContactNumber", tournament.ContactNumber, DbType.Int32);
                perameter.Add("CreatedBy", tournament.CreatedBy, DbType.Int32);
                perameter.Add("CreatedAt", tournament.CreatedAt, DbType.DateTime);
                perameter.Add("UpdatedBy", tournament.UpdatedBy, DbType.Int32);
                perameter.Add("UpdatedAt", tournament.UpdatedAt, DbType.DateTime);
                perameter.Add("IsActive", tournament.IsActive, DbType.Boolean);
                perameter.Add("NumberOfGroups", tournament.NumberOfGroups, DbType.Int32);
                perameter.Add("TeamsPerGroup", tournament.TeamsPerGroup, DbType.Int32);
                perameter.Add("StartTimeMorning", tournament.StartTimeMorning, DbType.String);
                perameter.Add("StartTimeAfternoon", tournament.StartTimeAfternoon, DbType.String);


                using (var connetion = _context.CreateConnection())
                {
                    var result = await connetion.QueryAsync<dynamic>("SP_Tournaments", perameter, commandType: CommandType.StoredProcedure);
                    var rdata = new
                    {
                        success = true,
                        Message = "Tournament added successfully",
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
        [HttpPut("UpdateTournaments/{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Tournaments tournament)
        {
            try
            {
                var perameter = new DynamicParameters();
                perameter.Add("Flag", 3);
                perameter.Add("TournamentID", id);
                perameter.Add("Prize", tournament.Prize, DbType.String);
                perameter.Add("TournamentName", tournament.TournamentName, DbType.String);
                perameter.Add("Location", tournament.Location, DbType.String);
                perameter.Add("StartDate", tournament.StartDate, DbType.DateTime);
                perameter.Add("EndDate", tournament.EndDate, DbType.DateTime);
                perameter.Add("TournamentTypeID", tournament.TournamentTypeID, DbType.Int32);
                perameter.Add("RegistrationDeadline", tournament.RegistrationDeadline, DbType.DateTime);
                perameter.Add("RegistrationFee", tournament.RegistrationFee, DbType.Int32);
                perameter.Add("FieldFee", tournament.FieldFee, DbType.Int32);
                perameter.Add("MaxTeams", tournament.MaxTeams, DbType.Int32);
                perameter.Add("TotalPlayer", tournament.TotalPlayer, DbType.Int32);
                perameter.Add("MatchPlayer", tournament.MatchPlayer, DbType.Int32);
                perameter.Add("ExtraPlayer", tournament.ExtraPlayer, DbType.Int32);
                perameter.Add("Status", tournament.Status, DbType.String);
                perameter.Add("ContactNumber", tournament.ContactNumber, DbType.Int32);
                perameter.Add("CreatedBy", tournament.CreatedBy, DbType.Int32);
                perameter.Add("CreatedAt", tournament.CreatedAt, DbType.DateTime);
                perameter.Add("UpdatedBy", tournament.UpdatedBy, DbType.Int32);
                perameter.Add("UpdatedAt", tournament.UpdatedAt, DbType.DateTime);
                perameter.Add("IsActive", tournament.IsActive, DbType.Boolean);
                perameter.Add("NumberOfGroups", tournament.NumberOfGroups, DbType.Int32);
                perameter.Add("TeamsPerGroup", tournament.TeamsPerGroup, DbType.Int32);
                perameter.Add("StartTimeMorning", tournament.StartTimeMorning, DbType.String);
                perameter.Add("StartTimeAfternoon", tournament.StartTimeAfternoon, DbType.String);

                using (var connetion = _context.CreateConnection())
                {
                    var result = await connetion.QueryAsync<dynamic>("SP_Tournaments", perameter, commandType: CommandType.StoredProcedure);
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



        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var perameter = new DynamicParameters();
                perameter.Add("Flag", 4);
                perameter.Add("TournamentID", id);
                using (var connetion = _context.CreateConnection())
                {
                    var result = await connetion.QueryAsync<dynamic>("SP_Tournaments", perameter, commandType: CommandType.StoredProcedure);
                    var rdata = new
                    {
                        success = true,
                        Message = "Tournament deleted successfully",
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
        [HttpGet("GetTournamentsByuserId")]
        public async Task<IActionResult> GetTournamentsByuserId(int userId)
        {
            try
            {
                var perameter = new DynamicParameters();
                perameter.Add("Flag", 7);
                perameter.Add("@userId", userId);
                using (var connetion = _context.CreateConnection())
                {
                    var result = await connetion.QueryAsync<dynamic>("SP_Tournaments", perameter, commandType: CommandType.StoredProcedure);
                    var rdata = new
                    {
                        success = true,
                        Message = "Tournament fetched successfully",
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
        [HttpGet("GetUnregisterTournamentByuserId")]
        public async Task<IActionResult> GetUnregisterTournamentByuserId(int userId)
        {
            try
            {
                var perameter = new DynamicParameters();
                perameter.Add("Flag", 8);
                perameter.Add("@userId", userId);
                using (var connetion = _context.CreateConnection())
                {
                    var result = await connetion.QueryAsync<dynamic>("SP_Tournaments", perameter, commandType: CommandType.StoredProcedure);
                    var rdata = new
                    {
                        success = true,
                        Message = "Tournament registration successfully",
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


        [HttpPost("UpdateGroupAssignment")]
        public async Task<IActionResult> UpdateGroupAssignment([FromBody] List<TeamGroupAssignment> assignments)
        {
            try
            {
                using (var connection = _context.CreateConnection())
                {
                    foreach (var item in assignments)
                    {
                        var parameter = new DynamicParameters();
                        parameter.Add("TournamentId", item.TournamentId);
                        parameter.Add("TeamId", item.TeamId);
                        parameter.Add("GroupId", item.GroupId);

                        await connection.ExecuteAsync(
                            "UPDATE TournamentTeamMapping SET GroupId = @GroupId WHERE TournamentId = @TournamentId AND TeamId = @TeamId",
                            parameter
                        );
                    }
                    return Ok(new { success = true, message = "Groups assigned successfully" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Error - " + ex.Message });
            }
        }
    }

    public class TeamGroupAssignment
    {
        public int TournamentId { get; set; }
        public int TeamId { get; set; }
        public int GroupId { get; set; }
    }
}

