using RideSharing.Helpers;
using RideSharing.Models;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using StackExchange.Redis.MultiplexerPool;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using System.Threading.Tasks;

namespace RideSharing.Repositories
{
    public interface IRedisStore
    {
        Task<IConnectionMultiplexer> GetRedisConnection();
        Task<IServer> GetServer();
        Task<IDatabase> GetDatabase();
    }
    public class RedisStore : IRedisStore
    {

        private IConnectionMultiplexerPool _connectionPool;
        private string Hostname;
        private int Port;

        public RedisStore(IConfiguration Configuration)
        {
            // Connection setup
            Hostname = Configuration["Redis:Hostname"];
            Port = int.Parse(Configuration["Redis:Port"]);
            var config = new ConfigurationOptions()
            {
                KeepAlive = 0,
                AllowAdmin = true,
                EndPoints = { { Hostname, Port } },
                ConnectTimeout = 1000,
                ConnectRetry = 5,
                SyncTimeout = 1000,
                AbortOnConnectFail = false,
            };
            int poolSize = 200;
            _connectionPool = ConnectionMultiplexerPoolFactory.Create(
               poolSize: poolSize,
               configurationOptions: config,
               connectionSelectionStrategy: ConnectionSelectionStrategy.LeastLoaded);
        }

        public async Task<IConnectionMultiplexer> GetRedisConnection()
        {
            var connection = await _connectionPool.GetAsync();
            return connection.Connection;
        }

        public async Task<IServer> GetServer()
        {
            var connection = await _connectionPool.GetAsync();
            return connection.Connection.GetServer(Hostname, Port);
        }
        public async Task<IDatabase> GetDatabase()
        {
            var connection = await _connectionPool.GetAsync();
            return connection.Connection.GetDatabase(0);
        }

    }
    public class CacheRepository
    {
        private DataContext DataContext;
        private IConfiguration Configuration;
        private IRedisStore RedisStore;
        public CacheRepository(DataContext DataContext, IRedisStore RedisStore, IConfiguration Configuration)
        {
            this.DataContext = DataContext;
            this.Configuration = Configuration;
            this.RedisStore = RedisStore;
        }
        protected async Task<T> GetFromCache<T>(string key)
        {
            try
            {
                key = $"{StaticParams.ModuleName}.{key}";
                IDatabase Database = await RedisStore.GetDatabase();
                string value = await Database.StringGetAsync(key);
                if (string.IsNullOrWhiteSpace(value))
                    return default(T);
                T cachedResult = JsonSerializer.Deserialize<T>(value);
                return cachedResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetFromCache:" + key + ex.Message);
                return default(T);
            }
        }

        protected async Task SetToCache<T>(string key, T value)
        {
            key = $"{StaticParams.ModuleName}.{key}";
            try
            {
                var options = new JsonSerializerOptions
                {
                    IgnoreNullValues = true,
                    WriteIndented = false
                };
                string json = JsonSerializer.Serialize(value, options);
                IDatabase Database = await RedisStore.GetDatabase();
                await Database.StringSetAsync(key, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("SetToCache:" + key + ex.Message);
            }
        }

        protected async Task RemoveFromCache(string key)
        {
            try
            {
                key = $"{StaticParams.ModuleName}.{key}";
                IServer Server = await RedisStore.GetServer();
                var RedisKeys = Server.Keys(0, $"{key}*");
                foreach (var k in RedisKeys)
                {
                    try
                    {
                        IDatabase Database = await RedisStore.GetDatabase();
                        await Database.KeyDeleteAsync(k, CommandFlags.FireAndForget);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("RemoveFromCache:" + key + ex.Message);
                    }
                }
            }
            catch (Exception ex2)
            {
                Console.WriteLine("RemoveFromCache:" + ex2.Message);
            }
        }

        private byte[] Serialize<T>(T value)
        {
            byte[] array;
            try
            {
                IFormatter formatter = new BinaryFormatter();
                using MemoryStream MemoryStream = new MemoryStream();
                formatter.Serialize(MemoryStream, value);
                array = MemoryStream.ToArray();
                MemoryStream.Close();
            }
            catch
            {
                var options = new JsonSerializerOptions
                {
                    IgnoreNullValues = true,
                    WriteIndented = true
                };
                array = JsonSerializer.SerializeToUtf8Bytes(value, options);
            }
            return array;
        }
        private T Deserialize<T>(byte[] array)
        {
            T cachedResult;
            try
            {
                IFormatter formatter = new BinaryFormatter();
                using MemoryStream MemoryStream = new MemoryStream(array);
                cachedResult = (T)formatter.Deserialize(MemoryStream);
            }
            catch
            {
                cachedResult = JsonSerializer.Deserialize<T>(array);

            }
            return cachedResult;
        }
    }
}