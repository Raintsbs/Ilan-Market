using AdvertisementApp.DataAccess.Entities;
using AdvertisementApp.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdvertisementApp.DataAccess.Configurations
{
    public class AdvertisementConfiguration : IEntityTypeConfiguration<Advertisement>
    {
        public void Configure(EntityTypeBuilder<Advertisement> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.Content)
                .IsRequired();

            builder.Property(x => x.ImagePath)
                .HasMaxLength(300);

            builder.Property(x => x.ImagePathsJson)
                .HasMaxLength(4000);

            builder.Property(x => x.ListingDetailsJson)
                .HasMaxLength(8000);

            builder.Property(x => x.VideoPath).HasMaxLength(300);
            builder.Property(x => x.PanoramaPath).HasMaxLength(300);
            builder.Property(x => x.TramerResultJson).HasMaxLength(4000);
            builder.Property(x => x.ListingType).HasDefaultValue(ListingType.Standard);

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.CreatedTime)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            builder.Property(x => x.UpdatedTime)
                .IsRequired(false);

            builder.HasOne(x => x.Category)
                .WithMany(c => c.Advertisements)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("Advertisements");
        }
    }
}
