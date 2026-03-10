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
    public class PlayerRoleController : ControllerBase
    {

        private readonly DapperContext _context;

        public PlayerRoleController(DapperContext context)
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
                    var result = await connetion.QueryAsync<dynamic>("SP_PlayerRole", perameter, commandType: CommandType.StoredProcedure);
                    var rdata = new
                    {
                        success = true,
                        Message = "Player Role fetched successfully",
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
        [HttpGet("GetPlayerRoleById")]
        public async Task<IActionResult> GetPlayerRoleById(int PLayerRoleID)
        {
            try
            {
                var perameter = new DynamicParameters();
                perameter.Add("Flag", 5);
                perameter.Add("PLayerRoleID", PLayerRoleID);
                using (var connetion = _context.CreateConnection())
                {
                    var result = await connetion.QueryFirstOrDefaultAsync<dynamic>("SP_PlayerRole", perameter, commandType: CommandType.StoredProcedure);
                    var rdata = new
                    {
                        success = true,
                        Message = "Player Role fetched successfully",
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

        [HttpPost("PlayerRole")]
        public async Task<IActionResult> Post([FromBody] PlayerRole player)
        {
            try
            {
                var perameter = new DynamicParameters();
                perameter.Add("Flag", 2);
                perameter.Add("RoleName", player.RoleName, DbType.String);
                perameter.Add("Description", player.Description, DbType.String);
                perameter.Add("IsActive", player.IsActive, DbType.Boolean);
                perameter.Add("CreatedAt", player.CreatedAt, DbType.DateTime);


                using (var connetion = _context.CreateConnection())
                {
                    var result = await connetion.QueryAsync<dynamic>("SP_PlayerRole", perameter, commandType: CommandType.StoredProcedure);
                    var rdata = new
                    {
                        success = true,
                        Message = "PlayerRole added successfully",
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
        [HttpPut("UpdatePlayerRole/{PlayerRoleID}")]
        public async Task<IActionResult> Put(int PlayerRoleID, [FromBody] PlayerRole player)
        {
            try
            {
                var perameter = new DynamicParameters();
                perameter.Add("Flag", 3);
                perameter.Add("PlayerRoleID", PlayerRoleID); 
                perameter.Add("RoleName", player.RoleName);
                perameter.Add("Description", player.Description);
                perameter.Add("IsActive", player.IsActive);
                perameter.Add("CreatedAt", player.CreatedAt);

                using (var connetion = _context.CreateConnection())
                {
                    var result = await connetion.QueryAsync<dynamic>(
                        "SP_PlayerRole",
                        perameter,
                        commandType: CommandType.StoredProcedure
                    );

                    return Ok(new
                    {
                        success = true,
                        Message = "Player Role updated successfully",
                        Data = result
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




        [HttpDelete("{PLayerRoleID}")]
        public async Task<IActionResult> Delete(int PLayerRoleID)
        {
            try
            {
                var perameter = new DynamicParameters();
                perameter.Add("Flag", 4);
                perameter.Add("PLayerRoleID", PLayerRoleID);
                using (var connetion = _context.CreateConnection())
                {
                    var result = await connetion.QueryAsync<dynamic>("SP_PlayerRole", perameter, commandType: CommandType.StoredProcedure);
                    var rdata = new
                    {
                        success = true,
                        Message = "Player Role deleted successfully",
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
