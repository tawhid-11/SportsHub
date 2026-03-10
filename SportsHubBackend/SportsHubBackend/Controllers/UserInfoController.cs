using Dapper;
using Microsoft.AspNetCore.Mvc;
using SportsHubBackend.DBContext;
using SportsHubBackend.Model;
using System.Data;

namespace SportsHubBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserInfoController : ControllerBase
    {
        private readonly DapperContext _context;

        public UserInfoController(DapperContext context)
        {
            _context = context;
        }

        // ===================== LOGIN =====================
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] Login request)
        {
            try
            {
                var parameter = new DynamicParameters();
                parameter.Add("Flag", 2);
                parameter.Add("Email", request.Email, DbType.String);
                parameter.Add("Password", request.Password, DbType.String);

                using var connection = _context.CreateConnection();
                var result = await connection.QueryFirstAsync<dynamic>(
                    "SP_UserInfo",
                    parameter,
                    commandType: CommandType.StoredProcedure
                );

                return Ok(new
                {
                    success = true,
                    message = "Login successful",
                    data = result
                });
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

        // ===================== REGISTER =====================
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] Registration request)
        {
            try
            {
                var parameter = new DynamicParameters();
                parameter.Add("Flag", 1); 
                parameter.Add("Name", request.Name, DbType.String);
                parameter.Add("Email", request.Email, DbType.String);
                parameter.Add("Phone", request.Phone, DbType.String);
                parameter.Add("UserType", request.UserType, DbType.String);
                parameter.Add("Password", request.Password, DbType.String);

                using var connection = _context.CreateConnection();
                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "SP_UserInfo",
                    parameter,
                    commandType: CommandType.StoredProcedure
                );

                return Ok(new
                {
                    success = true,
                    message = "Registration successful",
                    data = result
                });
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

        // ===================== GET ALL USERS =====================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var parameter = new DynamicParameters();
                parameter.Add("Flag", 3);

                using var connection = _context.CreateConnection();
                var result = await connection.QueryAsync<dynamic>(
                    "SP_UserInfo",
                    parameter,
                    commandType: CommandType.StoredProcedure
                );

                return Ok(new
                {
                    success = true,
                    message = "Users fetched successfully",
                    data = result
                });
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
