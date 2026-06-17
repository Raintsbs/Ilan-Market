namespace AdvertisementApp.Dtos.AuthDtos
{
    public class UserProfileDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? ProfileImagePath { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
        public bool IsVerified { get; set; }
        public string? PhoneNumber { get; set; }
        public bool PhoneVerified { get; set; }
    }
}
