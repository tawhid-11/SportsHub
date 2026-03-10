public class Player
{
    public int PlayerID { get; set; }
    public int TeamsID { get; set; }
    public int PlayerRoleID { get; set; }
    public int? UserId { get; set; }
    public string? PlayerImageUrl { get; set; }
    public string ? FullName { get; set; }
    public  string ? Nationality { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? BirthPlace { get; set; }
    public string? NickName { get; set; }
    public string? BattingStyle { get; set; }
    public string? BowlingStyle { get; set; }
    public bool? IsActive { get; set; }
    public IFormFile? PlayerImage { get; set; }
}
