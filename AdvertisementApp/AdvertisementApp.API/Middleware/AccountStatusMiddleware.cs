using System.Security.Claims;
using System.Text.Json;
using AdvertisementApp.Business.Helpers;
using AdvertisementApp.DataAccess.Entities;
using Microsoft.AspNetCore.Identity;

namespace AdvertisementApp.API.Middleware
{
    public class AccountStatusMiddleware
    {
        private readonly RequestDelegate _next;

        public AccountStatusMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context, UserManager<AppUser> userManager)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdClaim, out var userId))
                {
                    var user = await userManager.FindByIdAsync(userId.ToString());
                    var blockReason = user == null ? "Oturum geçersiz." : AccountStatusHelper.GetBlockReason(user);
                    if (blockReason != null)
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";
                        var json = JsonSerializer.Serialize(new
                        {
                            success = false,
                            message = blockReason,
                        });
                        await context.Response.WriteAsync(json);
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}
