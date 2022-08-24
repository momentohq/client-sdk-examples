using System.Collections.Concurrent;
using System.Diagnostics;
using Momento.Protos.CacheClient;

namespace momento_csharp_load_generator.load.requests
{
    public static class Dictionary
    {
        private static string filler = "fillerfillerfillerfillerfillerfillerfillerfillerfillerfillerfillerfillerfil"; // 75
        private static ConcurrentDictionary<uint, Google.Protobuf.ByteString> fields = new();
        private static ConcurrentDictionary<uint, Google.Protobuf.ByteString> values = new();
        private static ConcurrentDictionary<uint, Google.Protobuf.ByteString> dictionaryNames = new();

        public static Google.Protobuf.ByteString Field(uint i)
        {
            return fields.GetOrAdd(i, j => Google.Protobuf.ByteString.CopyFromUtf8($"field {j,4}")); // 10 bytes
        }

        public static Google.Protobuf.ByteString Value(uint i)
        {
            return values.GetOrAdd(i, j => Google.Protobuf.ByteString.CopyFromUtf8($"value {j,8} {filler}")); // + 15 bytes = 25 + 75 bytes = 100 bytes
        }

        public static Google.Protobuf.ByteString DictionaryName(uint i)
        {
            return dictionaryNames.GetOrAdd(i, j => Google.Protobuf.ByteString.CopyFromUtf8($"A dictionary {j}"));
        }

        private static _DictionaryFieldValuePair FieldValuePair(uint i)
        {
            var v = new _DictionaryFieldValuePair();
            v.Field = Field(i);
            v.Value = Value(i);
            return v;
        }

        public static async Task Set(uint dictionaryNumber, uint fieldStart, uint count, Scs.ScsClient client, RequestUtil util, Stats stats, CancellationToken cancellationToken)
        {
            try
            {
                var request = new _DictionarySetRequest
                {
                    TtlMilliseconds = 60000,
                    RefreshTtl = true,
                    DictionaryName = DictionaryName(dictionaryNumber),
                };
                for (uint i = 0; i < count; ++i)
                {
                    request.Items.Add(FieldValuePair(fieldStart + i));
                }

                var callOptions = util.GetCallOptions(cancellationToken);

                var startTimestamp = Stopwatch.GetTimestamp();
                await client.DictionarySetAsync(request, callOptions);
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
                var request = new _DictionaryGetRequest
                {
                    DictionaryName = DictionaryName(dictionaryNumber),
                };
                for (uint i = 0; i < count; ++i)
                {
                    request.Fields.Add(Field(fieldStart + i));
                }

                var callOptions = util.GetCallOptions(cancellationToken);

                var startTimestamp = Stopwatch.GetTimestamp();
                await client.DictionaryGetAsync(request, callOptions);
                stats.getLatencyTicks = Stopwatch.GetTimestamp() - startTimestamp;
            }
            catch (Grpc.Core.RpcException e)
            {
                stats.getError = e;
            }
        }
    }
}
