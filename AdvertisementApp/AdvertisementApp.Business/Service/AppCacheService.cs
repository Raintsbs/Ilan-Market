using System.Text.Json;
using AdvertisementApp.Business.Interface;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace AdvertisementApp.Business.Service
{
    public class AppCacheService : IAppCacheService
    {
        private readonly IDistributedCache _distributed;
        private readonly IMemoryCache _memory;

        public AppCacheService(IDistributedCache distributed, IMemoryCache memory)
        {
            _distributed = distributed;
            _memory = memory;
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
        {
            if (_memory.TryGetValue(key, out T? cached)) return cached;

            var bytes = await _distributed.GetAsync(key, ct);
            if (bytes == null || bytes.Length == 0) return default;

            var value = JsonSerializer.Deserialize<T>(bytes);
            if (value != null)
                _memory.Set(key, value, TimeSpan.FromMinutes(2));
            return value;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
            await _distributed.SetAsync(key, bytes, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl,
            }, ct);
            _memory.Set(key, value, ttl);
        }

        public async Task RemoveAsync(string key, CancellationToken ct = default)
        {
            _memory.Remove(key);
            await _distributed.RemoveAsync(key, ct);
        }

        public async Task<T> GetOrCreateAsync<T>(string key, TimeSpan ttl, Func<Task<T>> factory, CancellationToken ct = default)
        {
            var existing = await GetAsync<T>(key, ct);
            if (existing != null) return existing;

            var value = await factory();
            await SetAsync(key, value, ttl, ct);
            return value;
        }
    }
}
