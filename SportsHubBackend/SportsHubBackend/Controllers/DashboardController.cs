using Dapper;
using Microsoft.AspNetCore.Mvc;
using SportsHubBackend.DBContext;
using System.Data;
using SportsHubBackend.Model;

namespace SportsHubBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly DapperContext _context;

        public DashboardController(DapperContext context)
        {
            _context = context;
        }

        [HttpGet("GetStatistics")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                using (var connection = _context.CreateConnection())
                {
                    var parameter = new DynamicParameters();
                    parameter.Add("@Flag", 1);
                    var stats = await connection.QueryFirstOrDefaultAsync<Model.DashboardStats>(
                        "SP_Dashboard", 
                        parameter, 
                        commandType: CommandType.StoredProcedure
                    );

                    return Ok(ApiResponse<dynamic>.Ok("Statistics fetched successfully", new {
                            totalTeams = stats?.TotalTeams ?? 0,
                            totalTournaments = stats?.TotalTournaments ?? 0,
                            totalPlayers = stats?.TotalPlayers ?? 0,
                            totalMatches = stats?.TotalMatches ?? 0
                        }));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error("Error - " + ex.Message));
            }
        }
    }
}
