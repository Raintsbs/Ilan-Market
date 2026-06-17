using AdvertisementApp.DataAccess.Entities;

namespace AdvertisementApp.Business.Helpers
{
    public static class AccountStatusHelper
    {
        public static string? GetBlockReason(AppUser user)
        {
            if (user.IsBanned)
            {
                return string.IsNullOrWhiteSpace(user.BanReason)
                    ? "Hesabınız engellenmiş."
                    : $"Hesabınız engellenmiş: {user.BanReason}";
            }

            if (user.IsFrozen && (user.FrozenUntil == null || user.FrozenUntil > DateTime.UtcNow))
                return "Hesabınız dondurulmuş.";

            return null;
        }
    }
}
