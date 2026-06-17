using AdvertisementApp.Business.DependencyResolvers;
using AdvertisementApp.Business.Interface;
using AdvertisementApp.Business.Service;
using AdvertisementApp.DataAccess.Extension;
using AdvertisementApp.DataAccess.Seed;
using AdvertisementApp.UI.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDataAccessDependencies(connectionString);
builder.Services.AddDependencies();

builder.Services.AddSingleton<IImageStorageService>(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    var webRootPath = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    return new ImageStorageService(webRootPath);
});

var app = builder.Build();

try
{
    await DatabaseSeeder.SeedAsync(app.Services);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    logger.LogError(ex, "Seed sırasında hata oluştu: {Message}", ex.Message);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<NotificationHub>("/hubs/notification");

// Geçici seed endpoint - çalıştıktan sonra kaldır
app.MapGet("/run-seed", async (IServiceProvider sp) =>
{
    try
    {
        await AdvertisementApp.DataAccess.Seed.DatabaseSeeder.SeedAsync(sp);
        return Results.Ok("Seed tamamlandı.");
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.ToString());
    }
});

app.Run();
