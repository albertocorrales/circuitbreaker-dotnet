using CircuitBreaker.Abstractions;
using StackExchange.Redis;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CircuitBreaker.ElastiCache.Repository
{
    public class CircuitBreakerRepositoryElastiCache : ICircuitBreakerRepository
    {
        private readonly ConnectionMultiplexer _connection;
        private readonly IDatabase _database;

        public CircuitBreakerRepositoryElastiCache(EndPointCollection endPoints, string connectionString) 
        {
            var elastiCacheConfig = new ConfigurationOptions()
            {
                Ssl = true,
                EndPoints = { }
            };

            AddEndpointsFromConnectionString(endPoints, connectionString);

            _connection = ConnectionMultiplexer.Connect(elastiCacheConfig);
            _database = _connection.GetDatabase();
        }

        private static void AddEndpointsFromConnectionString(EndPointCollection endPoints, string connectionString)
        {
            var endpoints = connectionString.Split(",");
            endpoints.ToList().ForEach(endpoint => { endPoints.Add(endpoint); });
        }

        public async Task Upsert(CircuitBreakerModel obj)
        {
            var objString = JsonSerializer.Serialize(obj);
            await _database.StringSetAsync(new RedisKey(obj.Id.ToLower()), new RedisValue(objString));
        }

        public async Task<CircuitBreakerModel> GetById(string id)
        {
            var value = await _database.StringGetAsync(new RedisKey(id.ToLower()));
            return value.IsNullOrEmpty ? null : JsonSerializer.Deserialize<CircuitBreakerModel>(value.ToString());
        }
    }
}
