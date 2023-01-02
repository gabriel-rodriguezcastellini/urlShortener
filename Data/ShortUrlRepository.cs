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
        if (await ExistsAsync(shortUrl.Path) || await ExistsDestinationAsync(shortUrl.Destination))
        {
            throw new ShortUrlExistsException($"Shortened URL with path '{shortUrl.Path}' already exists.");
        }
        
        await database.StringSetAsync(shortUrl.Path, shortUrl.Destination);
    }

    public async Task DeleteAsync(string path)
    {
        if (await ExistsAsync(path) == false)
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

            foreach (var item in database.Multiplexer.GetServer(database.Multiplexer.GetEndPoints()[0]).Keys())
            {                
                redisValue = await database.StringGetAsync(item.ToString());

                if (redisValue.ToString() == path)
                {
                    valueFound = true;
                    redisValue = item.ToString();
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

    public async Task<bool> ExistsAsync(string path) => await database.KeyExistsAsync(path);

    private async Task<bool> ExistsDestinationAsync(string destination)
    {
        foreach (var item in database.Multiplexer.GetServer(database.Multiplexer.GetEndPoints()[0]).Keys())
        {
            var redisValue = await database.StringGetAsync(item.ToString());

            if (redisValue.ToString() == destination)
            {
                return true;
            }
        }

        return false;
    }

    private bool ShouldSearchByDestination(string path) => path.Contains(':');    
}
