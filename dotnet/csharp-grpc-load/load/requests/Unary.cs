using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using Momento.Sdk;
using Momento.Protos.CacheClient;

namespace momento_csharp_load_generator.load.requests
{

    public static class Unary
    {
        private static ConcurrentDictionary<uint, Google.Protobuf.ByteString> keys = new();
        private static ConcurrentDictionary<uint, Google.Protobuf.ByteString> values = new();

        public static Google.Protobuf.ByteString Key(uint i)
        {
            return keys.GetOrAdd(i, j => Google.Protobuf.ByteString.CopyFromUtf8($"A key {j}"));
        }

        public static Google.Protobuf.ByteString Value(uint i)
        {
            return values.GetOrAdd(i, j => Google.Protobuf.ByteString.CopyFromUtf8($"A value {j}"));
        }

        private static ConcurrentDictionary<uint, byte[]> byteKeys = new();
        private static ConcurrentDictionary<uint, byte[]> byteValues = new();

        public static byte[] ByteKey(uint i)
        {
            return byteKeys.GetOrAdd(i, j => Encoding.UTF8.GetBytes($"A key {j}") );
        }

        public static byte[] ByteValue(uint i)
        {
            return byteValues.GetOrAdd(i, j => Encoding.UTF8.GetBytes($"A value {j}"));
        }

        public static async Task Set(uint i, Scs.ScsClient client, RequestUtil util, Stats stats, CancellationToken cancellationToken)
        {
            try
            {
                var request = new _SetRequest
                {
                    TtlMilliseconds = 60000,
                    CacheKey = Key(i),
                    CacheBody = Value(i),
                };
                var callOptions = util.GetCallOptions(cancellationToken);

                var startTimestamp = Stopwatch.GetTimestamp();
                await client.SetAsync(request, callOptions);
                stats.setLatencyTicks = Stopwatch.GetTimestamp() - startTimestamp;
            }
            catch (Grpc.Core.RpcException e)
            {
                stats.setError = e;
            }
        }

        public static async Task Get(uint i, Scs.ScsClient client, RequestUtil util, Stats stats, CancellationToken cancellationToken)
        {
            try
            {
                var request = new _GetRequest
                {
                    CacheKey = Key(i),
                };
                var callOptions = util.GetCallOptions(cancellationToken);

                var startTimestamp = Stopwatch.GetTimestamp();
                await client.GetAsync(request, callOptions);
                stats.getLatencyTicks = Stopwatch.GetTimestamp() - startTimestamp;
            }
            catch (Grpc.Core.RpcException e)
            {
                stats.getError = e;
            }
        }

        /// Have to include cache name with the simple cache client
        public static async Task ClientSet(uint i, SimpleCacheClient momentoClient, string cacheName, Stats stats)
        {
            try
            {
                var startTimestamp = Stopwatch.GetTimestamp();
                await momentoClient.SetAsync(cacheName, ByteKey(i), ByteValue(i));
                stats.setLatencyTicks = Stopwatch.GetTimestamp() - startTimestamp;
            }
            catch (Momento.Sdk.Exceptions.SdkException e)
            {
                stats.setError = e;
            }
        }

        /// Have to include cache name with the simple cache client
        public static async Task ClientGet(uint i, SimpleCacheClient momentoClient, string cacheName, Stats stats)
        {
            try
            {
                var startTimestamp = Stopwatch.GetTimestamp();
                await momentoClient.GetAsync(cacheName, ByteKey(i));
                stats.getLatencyTicks = Stopwatch.GetTimestamp() - startTimestamp;
            }
            catch (Momento.Sdk.Exceptions.SdkException e)
            {
                stats.getError = e;
            }
        }

        public static async Task ClientListAdd(uint i, Momento.Sdk.Incubating.SimpleCacheClient momentoClient, string cacheName, Stats stats)
        {
            var listName = i.ToString();
            var value = i.ToString();
            var startPushFrontTimestamp = Stopwatch.GetTimestamp();
            await momentoClient.ListPushFrontAsync(cacheName, listName, value, true);
            stats.setLatencyTicks = Stopwatch.GetTimestamp() - startPushFrontTimestamp;

            var startPopBackTimestamp = Stopwatch.GetTimestamp();
            await momentoClient.ListPopBackAsync(cacheName, listName);
            stats.setLatencyTicks = Stopwatch.GetTimestamp() - startPopBackTimestamp;
        }
        
        public static async Task ClientListFetch(uint i, Momento.Sdk.Incubating.SimpleCacheClient momentoClient, string cacheName, Stats stats)
        {
            var listName = i.ToString();
            var startTimestamp = Stopwatch.GetTimestamp();
            await momentoClient.ListFetchAsync(cacheName, listName);
            stats.getLatencyTicks = Stopwatch.GetTimestamp() - startTimestamp;
        }
    }
}
