using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace API.Extensions;

public static class DistributedCacheExtensions
{
    
    public static Task SetAsJsonAsync<T>(
        this IDistributedCache cache,
        string key,
        T value,
        DistributedCacheEntryOptions options,
        CancellationToken token = default (CancellationToken)
        )
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
        return cache.SetAsync(key, bytes, options, token);
    }

    public static async Task<T?> GetAsJsonAsync<T>(
        this IDistributedCache cache,
        string key,
        CancellationToken token = default(CancellationToken))
    {
        var bytes = await cache.GetAsync(key, token);
        if (bytes == null) return default;
        
        return JsonSerializer.Deserialize<T>(bytes);
    }
    
}