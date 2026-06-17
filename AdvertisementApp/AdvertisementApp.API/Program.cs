using System.Threading.RateLimiting;
using System.Text;
using AdvertisementApp.API.Configuration;
using AdvertisementApp.API.Filters;
using AdvertisementApp.API.Middleware;
using AdvertisementApp.API.Hubs;
using AdvertisementApp.API.Services;
using AdvertisementApp.Business.DependencyResolvers;
using AdvertisementApp.Business.Interface;
using AdvertisementApp.Business.Configuration;
using AdvertisementApp.Business.Service;
using AdvertisementApp.DataAccess.Extension;
using AdvertisementApp.DataAccess.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var railwayPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(railwayPort))
    builder.WebHost.UseUrls($"http://0.0.0.0:{railwayPort}");

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiExceptionFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AdvertisementApp API",
        Version = "v1",
        Description = "Next.js frontend için JWT korumalı REST API"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
    c.MapType<IFormFile>(() => new OpenApiSchema { Type = "string", Format = "binary" });
    c.OperationFilter<FormFileOperationFilter>();
});

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:3000" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("NextJs", policy =>
    {
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddMemoryCache();
var redisConnection = builder.Configuration["Redis:ConnectionString"];
if (!string.IsNullOrWhiteSpace(redisConnection))
{
    builder.Services.AddStackExchangeRedisCache(options => options.Configuration = redisConnection);
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

builder.Services.AddDataAccessDependencies(connectionString);
builder.Services.AddDependencies();
builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection(StorageOptions.SectionName));
builder.Services.AddHostedService<SavedSearchNotificationBackgroundService>();
builder.Services.AddHostedService<PriceAlertNotificationBackgroundService>();
builder.Services.AddSignalR();
builder.Services.AddScoped<IRealtimeNotifier, SignalRRealtimeNotifier>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IExternalAuthService, ExternalAuthService>();

var storageProvider = builder.Configuration["Storage:Provider"] ?? "Local";
if (storageProvider.Equals("AzureBlob", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<IMediaStorageService, AzureBlobMediaStorageService>();
}
else
{
    builder.Services.AddScoped<IMediaStorageService>(sp =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        var webRoot = sp.GetRequiredService<IWebHostEnvironment>().WebRootPath;
        var uploadPath = config["Storage:LocalUploadPath"];
        var root = string.IsNullOrWhiteSpace(uploadPath) ? webRoot : uploadPath;
        return new MediaStorageService(root);
    });
}
builder.Services.AddScoped<IImageStorageService>(sp => sp.GetRequiredService<IMediaStorageService>());
builder.Services.AddHttpClient<ITramerService, TramerService>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
            }));
    options.AddPolicy("write", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
            }));
});

builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString, name: "database");

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key missing. Set Jwt__Key or ILANMARKET_Jwt__Key environment variable.");

// Identity cookie varsayılanını ez: API yalnızca JWT Bearer kullanır
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (!context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                return Task.CompletedTask;

            var accessToken = context.Request.Query["access_token"].ToString();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                var authHeader = context.Request.Headers.Authorization.ToString();
                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    accessToken = authHeader["Bearer ".Length..].Trim();
            }

            if (!string.IsNullOrWhiteSpace(accessToken))
                context.Token = accessToken;

            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            // SignalR negotiate özel JSON yanıtı beklemiyor
            if (context.Request.Path.StartsWithSegments("/hubs"))
                return Task.CompletedTask;

            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            var json = System.Text.Json.JsonSerializer.Serialize(new
            {
                success = false,
                message = "Giriş gerekli veya oturum süresi dolmuş."
            });
            return context.Response.WriteAsync(json);
        }
    };
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(
                "{\"success\":false,\"message\":\"Giriş gerekli.\"}");
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

ProductionConfigurationValidator.Validate(app.Configuration, app.Environment, app.Logger);

if (app.Environment.IsProduction())
{
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    });
    app.UseHsts();
    app.UseHttpsRedirection();
}

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseCors("NextJs");
app.UseAuthentication();
app.UseMiddleware<AccountStatusMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.MapHub<AppHub>("/hubs/app").RequireCors("NextJs");
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = check => check.Name != "database",
});

_ = Task.Run(async () =>
{
    try
    {
        await DatabaseSeeder.SeedAsync(app.Services);
        using var scope = app.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ILocationService>().EnsureSeededAsync();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Arka plan seed/migration basarisiz — API yine de calisir; DB baglantisini kontrol edin.");
    }
});

app.Run();
