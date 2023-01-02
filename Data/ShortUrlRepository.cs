using Common.Exceptions;
using StackExchange.Redis;

namespace Data;

public sealed class ShortUrlRepository
{   
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly IDatabase database;

    public ShortUrlRepository(IConnectionMultiplexer redis)
    {
        _connectionMultiplexer = redis;
        database = redis.GetDatabase();
    }

    public async Task CreateAsync(ShortUrl shortUrl)
    {
        if (await Exists(shortUrl.Path))
        {
            throw new ShortUrlExistsException($"Shortened URL with path '{shortUrl.Path}' already exists.");
        }
        
        await database.StringSetAsync(shortUrl.Path, shortUrl.Destination);
    }

    public async Task DeleteAsync(string path)
    {
        if (await Exists(path) == false)
        {
            throw new UnableToDeleteUrlException($"Shortened URL with path '{path}' does not exist.");            
        }

        await database.KeyDeleteAsync(path);

        if (await GetAsync(path) != null)
        {
            throw new UnableToDeleteUrlException("Failed to delete shortened URL.");            
        }
    }

    public async Task<ShortUrl?> GetAsync(string path)
    {
        RedisValue redisValue = default;
        bool valueFound = false;

        if (ShouldSearchByDestination(path))
        {
            var endpoints = database.Multiplexer.GetEndPoints();
            var server = database.Multiplexer.GetServer(endpoints[0]);
            var keys = server.Keys();

            foreach (var item in keys)
            {
                redisValue = (await database.HashGetAllAsync(item.ToString()))?.LastOrDefault().Value.ToString();

                if (redisValue.ToString() == path)
                {
                    valueFound = true;
                    break;
                }
            }
        }
        else
        {
            redisValue = await database.StringGetAsync(path);
            valueFound = redisValue.HasValue;
        }        

        return valueFound ? new ShortUrl(redisValue.ToString(), path) : null;
    }    

    public async Task<bool> Exists(string path) => await database.KeyExistsAsync(path);

    private bool ShouldSearchByDestination(string path) => path.Contains(':');
}
