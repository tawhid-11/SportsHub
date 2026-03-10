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
    public class TournamentTypeController : ControllerBase
    {
        private readonly DapperContext _context;

        public TournamentTypeController(DapperContext context)
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
                    var result = await connetion.QueryAsync<dynamic>("SP_TournamentsType", perameter, commandType: CommandType.StoredProcedure);
                    var rdata = new
                    {
                        success = true,
                        Message = "TournamentType fetched successfully",
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
        [HttpGet("GetTournamentTypeById")]
        public async Task<IActionResult> GetTournamentTypeById(int id)
        {
            try
            {
                var perameter = new DynamicParameters();
                perameter.Add("Flag", 5);
                perameter.Add("Id", id);
                using (var connetion = _context.CreateConnection())
                {
                    var result = await connetion.QueryFirstOrDefaultAsync<dynamic>("SP_TournamentsType", perameter, commandType: CommandType.StoredProcedure);
                    var rdata = new
                    {
                        success = true,
                        Message = "TournamentType fetched successfully",
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

            [HttpPost("TournamentType")]
        public async Task<IActionResult> Post([FromBody] TournamentType tournament)
        {
            try
            {
                var perameter = new DynamicParameters();
                perameter.Add("Flag", 2);
                perameter.Add("Name", tournament.Name, DbType.String);


                using (var connetion = _context.CreateConnection())
                {
                    var result = await connetion.QueryAsync<dynamic>("SP_TournamentsType", perameter, commandType: CommandType.StoredProcedure);
                    var rdata = new
                    {
                        success = true,
                        Message = "TournamentType added successfully",
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
        [HttpPut("UpdateTournamentType/{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] TournamentType tournament)
        {
            try
            {
                var perameter = new DynamicParameters();
                perameter.Add("Flag", 3);
                perameter.Add("@Id", tournament.Id);
                perameter.Add("@Name", tournament.Name, DbType.String);

                using (var connetion = _context.CreateConnection())
                {
                    var result = await connetion.QueryAsync<dynamic>("SP_TournamentsType", perameter, commandType: CommandType.StoredProcedure);
                    var rdata = new
                    {
                        success = true,
                        Message = "TournamentType updated successfully",
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
                perameter.Add("Id", id);
                using (var connetion = _context.CreateConnection())
                {
                    var result = await connetion.QueryAsync<dynamic>("SP_TournamentsType", perameter, commandType: CommandType.StoredProcedure);
                    var rdata = new
                    {
                        success = true,
                        Message = "TournamentType deleted successfully",
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


