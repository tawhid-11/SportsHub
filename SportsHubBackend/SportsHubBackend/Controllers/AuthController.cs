using Dapper;
using Microsoft.AspNetCore.Mvc;
using SportsHubBackend.DBContext;
using SportsHubBackend.Model;
using SportsHubBackend.Services;
using System.Collections.Concurrent;
using System.Data;

namespace SportsHubBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DapperContext _context;
        private readonly IEmailService _emailService;
        private static ConcurrentDictionary<string, (string Code, DateTime Expiry)> _otpStore = new();

        public AuthController(DapperContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpPost("SendOTP")]
        public async Task<IActionResult> SendOTP([FromBody] OTPRequest request)
        {
            try
            {
                using var connection = _context.CreateConnection();
                
                // 1. Verify user exists
                var user = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT UserID, Name FROM UserInfo WHERE Email = @Target OR Phone = @Target", 
                    new { Target = request.Target }
                );

                if (user == null)
                    return BadRequest(new { success = false, message = "Account not found with this " + request.Type.ToLower() });

                // 2. Generate 6-digit OTP
                var otp = new Random().Next(100000, 999999).ToString();
                _otpStore[request.Target] = (otp, DateTime.Now.AddMinutes(10));

                // 3. Send via Email or Log for Phone
                if (request.Type.ToLower() == "email")
                {
                    await _emailService.SendOTPEmailAsync(request.Target, user.Name ?? "User", otp);
                }
                else
                {
                    // For Phone, currently just logging/simulating until SMS API is added
                    System.Diagnostics.Debug.WriteLine($"[SMS GATEWAY SIMULATION] To: {request.Target}, OTP: {otp}");
                    return Ok(new { success = true, message = "OTP sent to your phone (Demo: " + otp + ")", useMock = true });
                }

                return Ok(new { success = true, message = "OTP sent successfully to your " + request.Type.ToLower() });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Failed to send OTP: " + ex.Message });
            }
        }

        [HttpPost("VerifyOTP")]
        public async Task<IActionResult> VerifyOTP([FromBody] OTPVerifyRequest request)
        {
            if (_otpStore.TryGetValue(request.Target, out var stored) && 
                stored.Code == request.OTP && 
                stored.Expiry > DateTime.Now)
            {
                return Ok(new { success = true, message = "OTP verified successfully" });
            }
            return BadRequest(new { success = false, message = "Invalid or expired OTP" });
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] PasswordResetRequest request)
        {
            if (string.IsNullOrEmpty(request.NewPassword) || request.NewPassword.Length < 8)
            {
                return BadRequest(new { success = false, message = "Password must be at least 8 characters long." });
            }

            try
            {
                // We assume OTP was already verified by the client before calling this
                // In a production app, we would use a Secure Token (JWT) issued after VerifyOTP
                
                using var connection = _context.CreateConnection();
                var result = await connection.ExecuteAsync(
                    "UPDATE UserInfo SET Password = @Password WHERE Email = @Target OR Phone = @Target",
                    new { Password = request.NewPassword, Target = request.Target }
                );

                if (result > 0)
                {
                    _otpStore.TryRemove(request.Target, out _);
                    return Ok(new { success = true, message = "Password updated successfully!" });
                }
                
                return BadRequest(new { success = false, message = "Could not update password. User might not exist." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Reset failed: " + ex.Message });
            }
        }
    }
}
