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
                    var result = await connetion.QueryAsync<TournamentType>("SP_TournamentsType", perameter, commandType: CommandType.StoredProcedure);
                    return Ok(ApiResponse<IEnumerable<TournamentType>>.Ok("TournamentType fetched successfully", result));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error("Error - " + ex.Message));
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
                    var result = await connetion.QueryFirstOrDefaultAsync<TournamentType>("SP_TournamentsType", perameter, commandType: CommandType.StoredProcedure);
                    return Ok(ApiResponse<TournamentType>.Ok("TournamentType fetched successfully", result));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error("Error - " + ex.Message));
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
                    await connetion.ExecuteAsync("SP_TournamentsType", perameter, commandType: CommandType.StoredProcedure);
                    return Ok(ApiResponse.Ok("TournamentType added successfully"));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error("Error - " + ex.Message));
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
                    await connetion.ExecuteAsync("SP_TournamentsType", perameter, commandType: CommandType.StoredProcedure);
                    return Ok(ApiResponse.Ok("TournamentType updated successfully"));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error("Error - " + ex.Message));
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
                    await connetion.ExecuteAsync("SP_TournamentsType", perameter, commandType: CommandType.StoredProcedure);
                    return Ok(ApiResponse.Ok("TournamentType deleted successfully"));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error("Error - " + ex.Message));
            }
        }


    }
}


