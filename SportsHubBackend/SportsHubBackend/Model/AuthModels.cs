namespace SportsHubBackend.Model
{
    public class OTPRequest
    {
        public string Target { get; set; } = string.Empty; // Email or Phone
        public string Type { get; set; } = "Email";       // "Email" or "Phone"
    }

    public class OTPVerifyRequest
    {
        public string Target { get; set; } = string.Empty;
        public string OTP { get; set; } = string.Empty;
    }

    public class PasswordResetRequest
    {
        public string Target { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
