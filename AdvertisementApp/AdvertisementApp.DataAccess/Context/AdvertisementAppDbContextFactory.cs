using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AdvertisementApp.DataAccess.Context
{
    public class AdvertisementAppDbContextFactory : IDesignTimeDbContextFactory<AdvertisementAppDbContext>
    {
        public AdvertisementAppDbContext CreateDbContext(string[] args)
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "AdvertisementApp.UI");
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            var optionsBuilder = new DbContextOptionsBuilder<AdvertisementAppDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new AdvertisementAppDbContext(optionsBuilder.Options);
        }
    }
}
