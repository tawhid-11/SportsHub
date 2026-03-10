using Dapper;
using Microsoft.AspNetCore.Mvc;
using SportsHubBackend.DBContext;
using System.Data;

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
                    // Get total teams count
                    var totalTeams = await connection.QueryFirstOrDefaultAsync<int>(
                        "SELECT COUNT(*) FROM Teams WHERE IsActive = 1"
                    );

                    // Get total tournaments count
                    var totalTournaments = await connection.QueryFirstOrDefaultAsync<int>(
                        "SELECT COUNT(*) FROM Tournaments"
                    );

                    // Get total players count
                    var totalPlayers = await connection.QueryFirstOrDefaultAsync<int>(
                        "SELECT COUNT(*) FROM Players WHERE IsActive = 1"
                    );

                    // Get total matches played (finished matches)
                    var totalMatches = await connection.QueryFirstOrDefaultAsync<int>(
                        "SELECT COUNT(*) FROM CricketMatch WHERE MatchStatus = 'Finished'"
                    );

                    var statistics = new
                    {
                        totalTeams = totalTeams,
                        totalTournaments = totalTournaments,
                        totalPlayers = totalPlayers,
                        totalMatches = totalMatches
                    };

                    return Ok(new
                    {
                        success = true,
                        message = "Statistics fetched successfully",
                        data = statistics
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Error - " + ex.Message
                });
            }
        }
    }
}
