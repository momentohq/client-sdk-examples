using System;
using MomentoSdk;
using MomentoSdk.Responses;

namespace MomentoApplication
{
    class Program
    {
        static readonly String MOMENTO_AUTH_TOKEN = Environment.GetEnvironmentVariable("MOMENTO_AUTH_TOKEN");
        static readonly String CACHE_NAME = "cache";
        static readonly String KEY = "key";
        static readonly String VALUE = "value";
        
        static void Main(string[] args)

        {
            Momento momento = new Momento(MOMENTO_AUTH_TOKEN);

            MomentoCache cache = momento.CreateOrGetCache(CACHE_NAME, 60);

            CacheSetResponse response = cache.Set(KEY, VALUE, 60);
            CacheGetResponse getResponse = cache.Get(KEY);

            Console.WriteLine(getResponse.result);
            Console.WriteLine(getResponse.String());

            getResponse = cache.Get("World");
            Console.WriteLine(getResponse.result);
            Console.WriteLine(getResponse.String());
    
        }
    }
}
