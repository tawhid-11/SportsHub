using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SportsHubBackend.DBContext;
using System.Data;

namespace SportsHubBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamScheduleController : ControllerBase
    {
        private readonly DapperContext _context;

        public TeamScheduleController(DapperContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int tournamentId)
        {
            try
            {
                var perameter = new DynamicParameters();
                perameter.Add("@Flag", 1);
                perameter.Add("@TournamentID", tournamentId);
                using (var connetion = _context.CreateConnection())
                {
                    var result = await connetion.QueryAsync<dynamic>("SP_TeamSchedule", perameter, commandType: CommandType.StoredProcedure);
                    var rdata = new
                    {
                        success = true,
                        Message = "Teams Schedule getting successfully",
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
        [HttpGet("GetTeamScheduleById")]
        public async Task<IActionResult> GetTeamScheduleById(int TeamScheduleID)
        {
            try
            {
                var perameter = new DynamicParameters();
                perameter.Add("Flag", 5);
                perameter.Add("@TeamScheduleID", TeamScheduleID);
                using (var connetion = _context.CreateConnection())
                {
                    var result = await connetion.QueryFirstOrDefaultAsync<dynamic>("SP_TeamSchedule", perameter, commandType: CommandType.StoredProcedure);
                    var rdata = new
                    {
                        success = true,
                        Message = "Teams get  successfully using id",
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


        [HttpGet("GetPlayerListByTeamScheduleId")]
        public async Task<IActionResult> GetPlayerListByTeamScheduleId(int teamScheduleId)
        {
            try
            {
                var perameter = new DynamicParameters();
                perameter.Add("@Flag", 6);
                perameter.Add("@TeamScheduleID", teamScheduleId);
                using (var connetion = _context.CreateConnection())
                {
                    var result = await connetion.QueryAsync<dynamic>("SP_TeamSchedule", perameter, commandType: CommandType.StoredProcedure);
                    var rdata = new
                    {
                        success = true,
                        Message = "Player getting successfully",
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



        [HttpGet("GetTodayMatches")]
        public async Task<IActionResult> GetTodayMatches()
        {
            try
            {
                var parameter = new DynamicParameters();
                parameter.Add("@Flag", 7); // You can define a new flag in your SP for "today's matches"

                using (var connection = _context.CreateConnection())
                {
                    var result = await connection.QueryAsync<dynamic>(
                        "SP_TeamSchedule",
                        parameter,
                        commandType: CommandType.StoredProcedure
                    );

                    var rdata = new
                    {
                        success = true,
                        Message = "Today's matches fetched successfully",
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
                    Message = "Error - " + ex.Message
                };
                return BadRequest(rdata);
            }
        }

    }
}
