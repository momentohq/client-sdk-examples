using System;
using MomentoSdk;
using MomentoSdk.Responses;

namespace MomentoApplication
{
    class Program
    {
        static readonly String MOMENTO_AUTH_TOKEN = Environment.GetEnvironmentVariable("MOMENTO_AUTH_TOKEN");
        static readonly String CACHE_NAME = "cache";
        static readonly String KEY = "MyKey";
        static readonly String VALUE = "MyData";
        
        static void Main(string[] args)
        {
            using Momento momento = new Momento(MOMENTO_AUTH_TOKEN);
            using MomentoCache cache = momento.GetOrCreateCache(CACHE_NAME, 60);
            Console.WriteLine($"Setting Key: {KEY} with Value: {VALUE}");
            cache.Set(KEY, VALUE);
            Console.WriteLine($"Get Value for  Key: {KEY}");
            CacheGetResponse getResponse = cache.Get(KEY);
            Console.WriteLine($"Lookup Result: {getResponse.Result}");
            Console.WriteLine($"Lookedup Value: {getResponse.String()}, Stored Value: {VALUE}");
        }
    }
}
