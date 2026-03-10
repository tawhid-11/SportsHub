namespace SportsHubBackend.Model
{
    public class TeamPayment
    {
        public int ? ID { get; set; }
        public string ? Phone { get; set; }
        public string ? OTP { get; set; }
        public decimal ? Amount { get; set; }
        public int userId { get; set; }
    }
}
