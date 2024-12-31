using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Infra.Interfaces;

public class RedisKVS : IKeyValueStore
{
	private readonly Dictionary<string, IConnectionMultiplexer> _connections = new();

	// Use logger if necessary
	private readonly ILogger<RedisKVS> logger;

	public RedisKVS(ILogger<RedisKVS> logger)
	{
		this._connections["Primary"] = ConnectionMultiplexer.Connect(Constants.RedisPrimary);
		// this._connections["Secondary"] = ConnectionMultiplexer.Connect(Constants.RedisSecondary);
		this.logger = logger;
	}

	public string GetString(string key)
	{
		var db = this._connections["Primary"].GetDatabase();
		RedisValue value = db.StringGet(key);
        return value.HasValue ? value : default;
	}

    public T Get<T>(string key)
	{
		var db = this._connections["Primary"].GetDatabase();
		RedisValue value = db.StringGet(key);
        return value.HasValue ? JsonConvert.DeserializeObject<T>(value) : default;
	}

    public void Put<T>(string key, T value)
	{
        var db = this._connections["Primary"].GetDatabase();
		if(value is string valStr)
		{
			db.StringSet(key, valStr);
		} else {
			var serialized = JsonConvert.SerializeObject(value);
			db.StringSet(key, serialized);
		}
	}

}

