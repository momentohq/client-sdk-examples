using System.Collections.Concurrent;
using System.Diagnostics;

namespace momento_csharp_load_generator.load.requests
{
    public static class Dictionary
    {
        private static ConcurrentDictionary<uint, ConcurrentDictionary<string, Google.Protobuf.ByteString>> fields = new();
        private static ConcurrentDictionary<uint, ConcurrentDictionary<string, Google.Protobuf.ByteString>> values = new();
        private static ConcurrentDictionary<uint, Google.Protobuf.ByteString> dictionaryNames = new();

        public static Google.Protobuf.ByteString Field(uint i, string suffix)
        {
            return fields.GetOrAdd(i, j => new()).GetOrAdd(suffix, s => Google.Protobuf.ByteString.CopyFromUtf8($"field {i} {s}"));
        }

        public static Google.Protobuf.ByteString Value(uint i, string suffix)
        {
            return values.GetOrAdd(i, j => new()).GetOrAdd(suffix, s => Google.Protobuf.ByteString.CopyFromUtf8($"value {i} {s}"));
        }

        public static Google.Protobuf.ByteString DictionaryName(uint i)
        {
            return dictionaryNames.GetOrAdd(i, j => Google.Protobuf.ByteString.CopyFromUtf8($"A dictionary {j}"));
        }

        private static CacheClient._DictionaryFieldValuePair FieldValuePair(uint i, string suffix)
        {
            var v = new CacheClient._DictionaryFieldValuePair();
            v.Field = Field(i, suffix);
            v.Value = Value(i, suffix);
            return v;
        }

        private static IEnumerable<CacheClient._DictionaryFieldValuePair> GetValues(uint i)
        {
            return new CacheClient._DictionaryFieldValuePair[] {
                FieldValuePair(i, "a"),
                FieldValuePair(i, "b"),
                FieldValuePair(i, "c"),
            };
        }

        private static IEnumerable<Google.Protobuf.ByteString> GetKeys(uint i)
        {
            return new Google.Protobuf.ByteString[] {
                Field(i, "a"),
                Field(i, "b"),
                Field(i, "c"),
            };
        }

        public static async Task Set(uint i, CacheClient.Scs.ScsClient client, RequestUtil util, Stats stats, CancellationToken cancellationToken)
        {
            try
            {
                var fieldNumber = i % 10;
                var dictionaryNumber = i - fieldNumber;
                var request = new CacheClient._DictionarySetRequest
                {
                    TtlMilliseconds = 60000,
                    RefreshTtl = true,
                    DictionaryName = DictionaryName(dictionaryNumber),
                };
                request.Items.AddRange(GetValues(fieldNumber));

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

        public static async Task Get(uint i, CacheClient.Scs.ScsClient client, RequestUtil util, Stats stats, CancellationToken cancellationToken)
        {
            try
            {
                var fieldNumber = i % 10;
                var dictionaryNumber = i - fieldNumber;
                var request = new CacheClient._DictionaryGetRequest
                {
                    DictionaryName = DictionaryName(dictionaryNumber),
                };
                request.Fields.AddRange(GetKeys(fieldNumber));

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

