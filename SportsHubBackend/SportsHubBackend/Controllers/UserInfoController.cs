using Dapper;
using Microsoft.AspNetCore.Mvc;
using SportsHubBackend.DBContext;
using SportsHubBackend.Model;
using System.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SportsHubBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserInfoController : ControllerBase
    {
        private readonly DapperContext _context;
        private readonly IConfiguration _configuration;

        public UserInfoController(DapperContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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

                if (result != null)
                {
                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
                    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                    
                    var userType = ((IDictionary<string, object>)result).ContainsKey("UserType") 
                                            ? ((IDictionary<string, object>)result)["UserType"]?.ToString() 
                                            : "User";

                    var claims = new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, request.Email ?? ""),
                        new Claim(ClaimTypes.Email, request.Email ?? ""),
                        new Claim(ClaimTypes.Role, userType ?? "User")
                    };

                    var token = new JwtSecurityToken(
                        issuer: _configuration["Jwt:Issuer"],
                        audience: _configuration["Jwt:Audience"],
                        claims: claims,
                        expires: DateTime.Now.AddHours(2),
                        signingCredentials: credentials);

                    var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

                    return Ok(new
                    {
                        success = true,
                        message = "Login successful",
                        token = jwtToken,
                        data = result
                    });
                }
                else
                {
                    throw new Exception("Invalid Username or Password");
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

        // ===================== REGISTER =====================
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] Registration request)
        {
            if (string.IsNullOrEmpty(request.Password) || request.Password.Length < 8)
            {
                return BadRequest(new { success = false, message = "Password must be at least 8 characters long." });
            }

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
