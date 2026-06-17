using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AdvertisementApp.API.Hubs
{
    [Authorize]
    public class AppHub : Hub
    {
        public static string UserGroup(int userId) => $"user_{userId}";

        public override async Task OnConnectedAsync()
        {
            var idClaim = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(idClaim, out var userId))
                await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));

            await base.OnConnectedAsync();
        }
    }
}
