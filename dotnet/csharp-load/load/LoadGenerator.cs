
using System.Diagnostics;
using System.Threading.Channels;

class LoadGenerator {
    private readonly CancellationTokenSource cancellationTokenSource;
    private readonly Grpc.Core.Metadata headers;
    private readonly uint keyspace = 10000;
    private readonly string endpoint;

    public LoadGenerator(CancellationTokenSource cancellationTokenSource, string endpoint, string authToken, string cache)
    {
        this.cancellationTokenSource = cancellationTokenSource;
        this.endpoint = endpoint;
        headers = new Grpc.Core.Metadata();
        headers.Add(new Grpc.Core.Metadata.Entry("authorization", authToken));
        headers.Add(new Grpc.Core.Metadata.Entry("cache", cache));
    }

    public async Task Run(uint hz, uint reportInterval, uint clients, uint concurrency) {
        var cancellationToken = cancellationTokenSource.Token;

        var metricsChannel = Channel.CreateUnbounded<Snapshot>();

        var jobs = new List<Task>();
        for (int i = 0; i < clients; ++i) {
            jobs.Add(RunLoadAsync(i, hz / clients, concurrency, metricsChannel.Writer, cancellationToken));
        }
        jobs.Add(PrintPeriodicMetrics(cancellationToken, metricsChannel.Reader, TimeSpan.FromSeconds(reportInterval)));

        await Task.WhenAny(jobs);
        if (!cancellationToken.IsCancellationRequested) {
            Console.WriteLine("Aborting load generation");
            cancellationTokenSource.Cancel();
        }

        await Task.WhenAll(jobs);
        Console.WriteLine("Load is all done");
    }

    private static async Task PrintPeriodicMetrics(CancellationToken cancellationToken, ChannelReader<Snapshot> reader, TimeSpan interval)
    {
        cancellationToken.Register(() => {
            Console.WriteLine("Task metrics emitter: Cancelled");
        });
        Console.WriteLine("Starting metrics emitter");

        var accumulation = Snapshot.Default();
        
        var lastReport = DateTime.Now;
        while (!cancellationToken.IsCancellationRequested) {
            var snapshot = await reader.ReadAsync(cancellationToken);
            accumulation.Add(snapshot);

            var now = DateTime.Now;
            if (lastReport + interval < now) {
                accumulation.Print(now - lastReport);
                accumulation.Reset();

                lastReport = now;
            }
        }
    }

    private async Task RunLoadAsync(int number, uint hz, uint maxLocalConcurrency, ChannelWriter<Snapshot> snapshotChannel, CancellationToken cancellationToken) {
        cancellationToken.Register(() => {
            Console.WriteLine("Task {0:D}: Cancelled", number);
        });
        Console.WriteLine("Starting {0:D} at {1:D}hz", number, hz);

        var statSnapshot = Snapshot.Default();
        const int operationCount = 2;

        var lastReport = DateTime.Now;
        var interval = new TimeSpan(TimeSpan.TicksPerSecond / hz) * operationCount;
        var schedule = DateTime.Now;
        var tasks = new HashSet<Task<Stats>>((int)maxLocalConcurrency);
        ulong i = 0;

        var channel = new Grpc.Core.Channel(endpoint, Grpc.Core.ChannelCredentials.SecureSsl);
        var client = new CacheClient.Scs.ScsClient(channel);

        while (!cancellationToken.IsCancellationRequested) {
            i += 1;
            if (maxLocalConcurrency <= tasks.Count) {
                await Task.WhenAny(tasks);
            }

            var completeTasks = tasks.Where(task => task.IsCompleted).ToHashSet();
            foreach (var complete in completeTasks) {
                var stats = complete.Result;
                statSnapshot.Add(stats);
            }
            tasks = tasks.Except(completeTasks).ToHashSet();

            tasks.Add(RunOne(client, new Stats { concurrency = (uint)tasks.Count + 1 }, (uint)(i % keyspace), cancellationToken));

            if (DateTime.Now < lastReport + TimeSpan.FromSeconds(1)) {
                snapshotChannel.TryWrite(statSnapshot);
                statSnapshot = Snapshot.Default();
                lastReport += TimeSpan.FromSeconds(1);
            }

            var now = DateTime.Now;
            var previous = schedule;
            var next = schedule + interval;
            schedule = next < now ? now : next;
            await Task.Delay(schedule - now, cancellationToken);
        }
    }

    private async Task<Stats> RunOne(CacheClient.Scs.ScsClient client, Stats stats, uint i, CancellationToken cancellationToken)
    {
        await Set(i, client, stats, cancellationToken);

        await Get(i, client, stats, cancellationToken);

        return stats;
    }

    private async Task Set(uint i, CacheClient.Scs.ScsClient client, Stats stats, CancellationToken cancellationToken)
    {
        try
        {
            var request = new CacheClient._SetRequest
            {
                TtlMilliseconds = 60000,
                CacheKey = Google.Protobuf.ByteString.CopyFromUtf8($"A key {i}"),
                CacheBody = Google.Protobuf.ByteString.CopyFromUtf8($"A value {i}"),
            };
            var callOptions = GetCallOptions(cancellationToken);

            var startTimestamp = Stopwatch.GetTimestamp();
            await client.SetAsync(request, callOptions);
            stats.setLatencyTicks = Stopwatch.GetTimestamp() - startTimestamp;
        }
        catch (Grpc.Core.RpcException e)
        {
            stats.setError = e;
        }
    }

    private async Task Get(uint i, CacheClient.Scs.ScsClient client, Stats stats, CancellationToken cancellationToken)
    {
        try
        {
            var request = new CacheClient._GetRequest
            {
                CacheKey = Google.Protobuf.ByteString.CopyFromUtf8($"A key {i}"),
            };
            var callOptions = GetCallOptions(cancellationToken);

            var startTimestamp = Stopwatch.GetTimestamp();
            await client.GetAsync(request, callOptions);
            stats.getLatencyTicks = Stopwatch.GetTimestamp() - startTimestamp;
        }
        catch (Grpc.Core.RpcException e)
        {
            stats.getError = e;
        }
    }

    private Grpc.Core.CallOptions GetCallOptions(CancellationToken cancellationToken)
    {
        return new Grpc.Core.CallOptions(
            cancellationToken: cancellationToken,
            headers: headers,
            deadline: DateTime.UtcNow + TimeSpan.FromSeconds(1)
        );
    }
}
