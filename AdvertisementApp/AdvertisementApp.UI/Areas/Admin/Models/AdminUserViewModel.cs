namespace AdvertisementApp.UI.Areas.Admin.Models
{
    public class AdminUserViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public bool EmailConfirmed { get; set; }
        public List<string> Roles { get; set; } = new();
        public DateTime? CreatedDate { get; set; }

        public string FullName => $"{FirstName} {LastName}";
        public bool IsAdmin => Roles.Contains("Admin");
    }
}
