using AdvertisementApp.Business.Interface;
using AdvertisementApp.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AdvertisementApp.Business.Service
{
    public class SqlFullTextSearchService : IFullTextSearchService
    {
        private readonly AdvertisementAppDbContext _db;
        private readonly IConfiguration _config;
        private readonly ILogger<SqlFullTextSearchService> _logger;
        private bool? _available;

        public SqlFullTextSearchService(
            AdvertisementAppDbContext db,
            IConfiguration config,
            ILogger<SqlFullTextSearchService> logger)
        {
            _db = db;
            _config = config;
            _logger = logger;
        }

        public bool IsAvailable => _available ??= CheckAvailable();

        public async Task<List<int>> SearchAdvertisementIdsAsync(string term, int maxResults = 500)
        {
            if (!IsAvailable || string.IsNullOrWhiteSpace(term))
                return new List<int>();

            var words = term.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (words.Length == 0) return new List<int>();

            var formatted = string.Join(" AND ", words.Select(w => $"\"{w.Replace("\"", "")}*\""));

            try
            {
                var sql = """
                    SELECT TOP (@max) [KEY] AS [Value]
                    FROM CONTAINSTABLE(Advertisements, (Title, Description, Content), @term)
                    ORDER BY [RANK] DESC
                    """;

                var ids = await _db.Database
                    .SqlQueryRaw<int>(sql,
                        new Microsoft.Data.SqlClient.SqlParameter("@max", maxResults),
                        new Microsoft.Data.SqlClient.SqlParameter("@term", formatted))
                    .ToListAsync();
                return ids;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Full-text search failed; falling back to Contains");
                _available = false;
                return new List<int>();
            }
        }

        private bool CheckAvailable()
        {
            if (_config["Database:Provider"]?.Equals("Sqlite", StringComparison.OrdinalIgnoreCase) == true)
                return false;
            if (!_config.GetValue("Search:FullTextEnabled", true)) return false;
            try
            {
                var count = _db.Database.SqlQueryRaw<int>(
                    "SELECT COUNT(1) AS [Value] FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('Advertisements')")
                    .AsEnumerable()
                    .FirstOrDefault();
                return count > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
