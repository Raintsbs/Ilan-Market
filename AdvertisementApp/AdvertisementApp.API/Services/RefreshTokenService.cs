using System.Security.Cryptography;
using System.Text;
using AdvertisementApp.DataAccess.Context;
using AdvertisementApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdvertisementApp.API.Services
{
    public interface IRefreshTokenService
    {
        Task<string> IssueAsync(int userId);
        Task<(int UserId, string NewRefreshToken)?> RotateAsync(string refreshToken);
        Task RevokeAsync(string refreshToken);
    }

    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly AdvertisementAppDbContext _db;
        private readonly IConfiguration _config;

        public RefreshTokenService(AdvertisementAppDbContext db, IConfiguration configuration)
        {
            _db = db;
            _config = configuration;
        }

        public async Task<string> IssueAsync(int userId)
        {
            var raw = GenerateToken();
            var hash = Hash(raw);
            var days = _config.GetValue("Jwt:RefreshTokenDays", 30);

            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = userId,
                TokenHash = hash,
                ExpiresAt = DateTime.UtcNow.AddDays(days),
                CreatedAt = DateTime.UtcNow,
            });
            await _db.SaveChangesAsync();
            return raw;
        }

        public async Task<(int UserId, string NewRefreshToken)?> RotateAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken)) return null;
            var hash = Hash(refreshToken.Trim());
            var row = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash);
            if (row == null || row.RevokedAt != null || row.ExpiresAt <= DateTime.UtcNow)
                return null;

            row.RevokedAt = DateTime.UtcNow;
            var newRaw = GenerateToken();
            row.ReplacedByTokenHash = Hash(newRaw);

            var days = _config.GetValue("Jwt:RefreshTokenDays", 30);
            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = row.UserId,
                TokenHash = row.ReplacedByTokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(days),
                CreatedAt = DateTime.UtcNow,
            });
            await _db.SaveChangesAsync();
            return (row.UserId, newRaw);
        }

        public async Task RevokeAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken)) return;
            var hash = Hash(refreshToken.Trim());
            var row = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash);
            if (row == null || row.RevokedAt != null) return;
            row.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        private static string GenerateToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }

        private static string Hash(string raw)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
            return Convert.ToHexString(bytes);
        }
    }
}
