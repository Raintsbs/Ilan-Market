using AdvertisementApp.DataAccess.Context;
using AdvertisementApp.DataAccess.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AdvertisementApp.DataAccess.Extension
{
    public static class DependencyExtension
    {
        public const string AdminScheme = "AdminCookie";

        public static void AddDataAccessDependencies(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<AdvertisementAppDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            services.AddIdentity<AppUser, IdentityRole<int>>(opt =>
            {
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequiredLength = 4;
                opt.Password.RequireLowercase = false;
                opt.Password.RequireUppercase = false;
            })
            .AddEntityFrameworkStores<AdvertisementAppDbContext>()
            .AddDefaultTokenProviders();

            // Normal kullanıcı cookie
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.Cookie.Name = "UserSession";
            });

            // Admin için ayrı cookie scheme
            services.AddAuthentication()
                .AddCookie(AdminScheme, options =>
                {
                    options.LoginPath = "/Admin/Account/Login";
                    options.AccessDeniedPath = "/Admin/Account/Login";
                    options.Cookie.Name = "AdminSession";
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                });
        }
    }
}
