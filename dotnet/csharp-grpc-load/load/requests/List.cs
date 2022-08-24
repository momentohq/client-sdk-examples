using System.Collections.Concurrent;
using System.Diagnostics;
using Momento.Protos.CacheClient;

namespace momento_csharp_load_generator.load.requests
{
    public static class List
    {
        private static string filler = "fillerfillerfillerfillerfillerfillerfillerfillerfillerfillerfillerfillerfil"; // 75
        private static ConcurrentDictionary<uint, Google.Protobuf.ByteString> elements = new();
        private static ConcurrentDictionary<uint, Google.Protobuf.ByteString> listNames = new();

        public static Google.Protobuf.ByteString Element(uint i)
        {
            return elements.GetOrAdd(i, j => Google.Protobuf.ByteString.CopyFromUtf8($"value {j,8} {filler}")); // + 15 bytes = 25 + 75 bytes = 100 bytes
        }

        public static Google.Protobuf.ByteString ListName(uint i)
        {
            return listNames.GetOrAdd(i, j => Google.Protobuf.ByteString.CopyFromUtf8($"A list {j}"));
        }
        public static async Task Add(uint listNumber, uint elementStart, uint count, Scs.ScsClient client, RequestUtil util, Stats stats, CancellationToken cancellationToken)
        {
            try
            {
                for (uint i = 0; i < count; ++i)
                {
                    var request = new _ListPushBackRequest()
                    {
                        TtlMilliseconds = 60000,
                        RefreshTtl = true,
                        ListName = ListName(listNumber),
                    };
                    request.Value = Element(elementStart + 1);
                    var callOptions = util.GetCallOptions(cancellationToken);

                    var startTimestamp = Stopwatch.GetTimestamp();
                    await client.ListPushBackAsync(request, callOptions);
                    stats.setLatencyTicks = Stopwatch.GetTimestamp() - startTimestamp;
                }
            }
            catch (Grpc.Core.RpcException e)
            {
                stats.setError = e;
            }
        }

        public static async Task Get(uint listNumber, uint fieldStart, uint count, Scs.ScsClient client, RequestUtil util, Stats stats, CancellationToken cancellationToken)
        {
            try
            {
                var request = new _ListFetchRequest()
                {
                    ListName = ListName(listNumber),
                };

                var callOptions = util.GetCallOptions(cancellationToken);

                var startTimestamp = Stopwatch.GetTimestamp();
                await client.ListFetchAsync(request, callOptions);
                stats.getLatencyTicks = Stopwatch.GetTimestamp() - startTimestamp;
            }
            catch (Grpc.Core.RpcException e)
            {
                stats.getError = e;
            }
        }
    }
}
