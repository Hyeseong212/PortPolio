using StackExchange.Redis;

public class RedisContext
{
    private readonly ConnectionMultiplexer _connectionMultiplexer;
    public IDatabase RedisDB { get; }

    public RedisContext(string connStr)
    {
         _connectionMultiplexer = ConnectionMultiplexer.Connect(connStr);
        RedisDB = _connectionMultiplexer.GetDatabase();
    }
}