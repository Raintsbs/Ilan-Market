namespace AdvertisementApp.Dtos.AuthDtos
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = null!;
        public string? RefreshToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; } = null!;
        public IList<string> Roles { get; set; } = new List<string>();
    }
}
