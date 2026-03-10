namespace SportsHubBackend.Model
{
    public class PlayerRole
    {
        public int PlayerRoleID { get; set; }
        public required string RoleName { get; set; }
        public string ? Description { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
