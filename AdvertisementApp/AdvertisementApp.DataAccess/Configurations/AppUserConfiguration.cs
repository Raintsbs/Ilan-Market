using AdvertisementApp.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdvertisementApp.DataAccess.Configurations
{
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.Property(u => u.IsBanned).HasDefaultValue(false);
            builder.Property(u => u.IsFrozen).HasDefaultValue(false);
            builder.Property(u => u.IsVerified).HasDefaultValue(false);
            builder.Property(u => u.PhoneVerified).HasDefaultValue(false);
            builder.Property(u => u.WarningCount).HasDefaultValue(0);
            builder.Property(u => u.BanReason).HasMaxLength(500);
            builder.Property(u => u.FirstName).HasMaxLength(100);
            builder.Property(u => u.LastName).HasMaxLength(100);
            builder.Property(u => u.ProfileImagePath).HasMaxLength(500);
        }
    }
}
