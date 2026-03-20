using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using TechEval.Application.Common.Interfaces;

namespace TechEval.Infrastructure.Caching;

// Placeholder para fases futuras (sesiones en vivo, timers)
public sealed class RedisCache(IConnectionMultiplexer redis)
{
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task SetAsync<T>(string clave, T valor, TimeSpan? expiracion = null)
    {
        var json = JsonSerializer.Serialize(valor);
        if (expiracion.HasValue)
            await _db.StringSetAsync(clave, json, expiracion.Value);
        else
            await _db.StringSetAsync(clave, json);
    }

    public async Task<T?> GetAsync<T>(string clave)
    {
        var json = await _db.StringGetAsync(clave);
        if (json.IsNullOrEmpty) return default;
        return JsonSerializer.Deserialize<T>((string?)json!);
    }

    public async Task EliminarAsync(string clave) =>
        await _db.KeyDeleteAsync(clave);
}
