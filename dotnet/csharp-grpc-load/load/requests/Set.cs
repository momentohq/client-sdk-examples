using System.Collections.Concurrent;
using System.Diagnostics;
using Momento.Protos.CacheClient;

namespace momento_csharp_load_generator.load.requests
{
    public static class Set
    {
        private static string filler = "fillerfillerfillerfillerfillerfillerfillerfillerfillerfillerfillerfillerfillerfillerf"; // 85
        private static ConcurrentDictionary<uint, Google.Protobuf.ByteString> elements = new();
        private static ConcurrentDictionary<uint, Google.Protobuf.ByteString> setNames = new();

        public static Google.Protobuf.ByteString Element(uint i)
        {
            return elements.GetOrAdd(i, j => Google.Protobuf.ByteString.CopyFromUtf8($"value {j,8} {filler}")); // + 15 = 100 bytes
        }

        public static Google.Protobuf.ByteString SetName(uint i)
        {
            return setNames.GetOrAdd(i, j => Google.Protobuf.ByteString.CopyFromUtf8($"A dictionary {j}"));
        }

        public static async Task Add(uint setNumber, uint elementStart, uint count, Scs.ScsClient client, RequestUtil util, Stats stats, CancellationToken cancellationToken)
        {
            try
            {
                var request = new _SetUnionRequest
                {
                    TtlMilliseconds = 60000,
                    RefreshTtl = true,
                    SetName = SetName(setNumber),
                };
                for (uint i = 0; i < count; ++i)
                {
                    request.Elements.Add(Element(elementStart + i));
                }

                var callOptions = util.GetCallOptions(cancellationToken);

                var startTimestamp = Stopwatch.GetTimestamp();
                await client.SetUnionAsync(request, callOptions);
                stats.setLatencyTicks = Stopwatch.GetTimestamp() - startTimestamp;
            }
            catch (Grpc.Core.RpcException e)
            {
                stats.setError = e;
            }
        }

        public static async Task Get(uint dictionaryNumber, uint fieldStart, uint count, Scs.ScsClient client, RequestUtil util, Stats stats, CancellationToken cancellationToken)
        {
            try
            {
                var request = new _SetFetchRequest
                {
                    SetName = SetName(dictionaryNumber),
                };

                var callOptions = util.GetCallOptions(cancellationToken);

                var startTimestamp = Stopwatch.GetTimestamp();
                await client.SetFetchAsync(request, callOptions);
                stats.getLatencyTicks = Stopwatch.GetTimestamp() - startTimestamp;
            }
            catch (Grpc.Core.RpcException e)
            {
                stats.getError = e;
            }
        }
    }
}
