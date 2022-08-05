using System;
using MomentoSdk;
using MomentoSdk.Exceptions;
using MomentoSdk.Responses;

namespace MomentoApplication
{
    class Program
    {
        static readonly String MOMENTO_AUTH_TOKEN = Environment.GetEnvironmentVariable("MOMENTO_AUTH_TOKEN");
        static readonly String CACHE_NAME = "cache";
        static readonly String KEY = "MyKey";
        static readonly String VALUE = "MyData";
        static readonly uint DEFAULT_TTL_SECONDS = 60;

        static void Main(string[] args)
        {
            using SimpleCacheClient client = new SimpleCacheClient(MOMENTO_AUTH_TOKEN, DEFAULT_TTL_SECONDS);
            try
            {
                client.CreateCache(CACHE_NAME);
            }
            catch (AlreadyExistsException)
            {
                Console.WriteLine($"Cache with name {CACHE_NAME} already exists.\n");
            }
            Console.WriteLine("Listing caches:");
            String token = null;
            do
            {
                ListCachesResponse resp = client.ListCaches(token);
                foreach (CacheInfo cacheInfo in resp.Caches)
                {
                    Console.WriteLine(cacheInfo.Name);
                }
                token = resp.NextPageToken;
            } while (!String.IsNullOrEmpty(token));
            Console.WriteLine($"\nSetting key: {KEY} with value: {VALUE}");
            client.Set(CACHE_NAME, KEY, VALUE);
            Console.WriteLine($"\nGet value for  key: {KEY}");
            CacheGetResponse getResponse = client.Get(CACHE_NAME, KEY);
            Console.WriteLine($"\nLookedup value: {getResponse.String()}, Stored value: {VALUE}");
        }
    }
}
