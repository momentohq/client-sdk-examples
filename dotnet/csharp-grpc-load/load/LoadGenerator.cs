
using System.Diagnostics;
using System.Threading.Channels;
using Bert.RateLimiters;
using Momento.Grpc;
using momento_csharp_load_generator.load.requests;
using MomentoSdk;

class LoadGenerator {
    private readonly CancellationTokenSource cancellationTokenSource;
    private readonly uint keyspace;
    private readonly uint structuresize;
    private readonly string endpoint;
    private readonly RequestUtil util;
    private readonly TestMode testMode;

    private readonly SimpleCacheClient momentoClient;
    private readonly string cacheName;

    public LoadGenerator(TestMode testMode, string endpoint, string authToken, string cache, uint keyspace, uint structuresize, CancellationTokenSource cancellationTokenSource)
    {
        this.testMode = testMode;
        this.cancellationTokenSource = cancellationTokenSource;
        this.endpoint = endpoint;
        var headers = new Grpc.Core.Metadata();
        headers.Add(new Grpc.Core.Metadata.Entry("authorization", authToken));
        headers.Add(new Grpc.Core.Metadata.Entry("cache", cache));
        util = new RequestUtil(headers);
        momentoClient = new SimpleCacheClient(authToken, 60);
        cacheName = cache;

        Console.WriteLine($"Test mode: {testMode}, endpoint: {endpoint}, cache: {cache}, keyspace: {keyspace}, structuresize: {structuresize}");

        this.keyspace = keyspace;
        this.structuresize = structuresize;
    }

    public async Task Run(uint hz, uint reportInterval, uint clients, uint concurrency) {
        var cancellationToken = cancellationTokenSource.Token;

        var metricsChannel = Channel.CreateUnbounded<Snapshot>();

        var jobs = new List<Task>();
        for (int i = 0; i < clients; ++i) {
            jobs.Add(RunLoadAsync(i, hz / clients, concurrency, metricsChannel.Writer, cancellationToken));
        }
        jobs.Add(PrintPeriodicMetrics(testMode, metricsChannel.Reader, TimeSpan.FromSeconds(reportInterval), cancellationToken));

        await Task.WhenAny(jobs);
        if (!cancellationToken.IsCancellationRequested) {
            Console.WriteLine("Aborting load generation");
            cancellationTokenSource.Cancel();
        }

        await Task.WhenAll(jobs);
        Console.WriteLine("Load is all done");
    }

    private static async Task PrintPeriodicMetrics(TestMode testMode, ChannelReader<Snapshot> reader, TimeSpan interval, CancellationToken cancellationToken)
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
                accumulation.Print(testMode, now - lastReport);
                accumulation.Reset();

                lastReport = now;
            }
        }
    }

    private async Task RunLoadAsync(int number, uint hz, uint maxLocalConcurrency, ChannelWriter<Snapshot> snapshotChannel, CancellationToken cancellationToken) {
        cancellationToken.Register(() => {
            Console.WriteLine("Task {0:D}: Cancelled", number);
        });

        TokenBucket tb = new FixedTokenBucket(hz / 10, 1, 100);

        const int operationCount = 2;
        var interval = new TimeSpan(TimeSpan.TicksPerSecond / (hz / operationCount));
        Console.WriteLine("Starting {0:D} at {1:D}hz ({2}) | ticks_per_second: {3} high resolution: {4}", number, hz, interval, Stopwatch.Frequency, Stopwatch.IsHighResolution);

        var statSnapshot = Snapshot.Default();

        var lastReport = DateTime.Now;
        var tasks = new HashSet<Task<Stats>>((int)maxLocalConcurrency);
        ulong i = 0;

        var channel = new Grpc.Core.Channel(endpoint, Grpc.Core.ChannelCredentials.SecureSsl, new Grpc.Core.ChannelOption[] { new Grpc.Core.ChannelOption(Grpc.Core.ChannelOptions.Http2InitialSequenceNumber, number) });
        var client = new CacheClient.Scs.ScsClient(channel);
        var random = new Random();

        while (!cancellationToken.IsCancellationRequested)
        {
            i += 1;
            tasks = RecordComplete(statSnapshot, tasks);
            if (maxLocalConcurrency <= tasks.Count)
            {
                await Task.WhenAny(tasks);
                tasks = RecordComplete(statSnapshot, tasks);
            }

            var stats = new Stats { concurrency = (uint)tasks.Count + 1 };
            var oneJob = testMode switch
            {
                TestMode.Unary => RunOneUnary(client, stats, i, cancellationToken),
                TestMode.Dictionary => RunOneDictionary(client, stats, i, random, cancellationToken),
                TestMode.ClientUnary => RunOneClientUnary(stats, i),
                // This is a bad thing in c#: Enums are not closed discrete data types.
                _ => throw new NotImplementedException(testMode.ToString()),
            };

            tasks.Add(oneJob);

            var now = DateTime.Now;
            if (now < lastReport + TimeSpan.FromSeconds(1))
            {
                snapshotChannel.TryWrite(statSnapshot);
                statSnapshot = Snapshot.Default();
                lastReport = now;
            }

            TimeSpan waitTime = new();
            if (tb.ShouldThrottle(operationCount, out waitTime))
            {
                await Task.Delay(waitTime, cancellationToken);
            }
        }
    }

    private static HashSet<Task<Stats>> RecordComplete(Snapshot statSnapshot, HashSet<Task<Stats>> tasks)
    {
        var completeTasks = tasks.Where(task => task.IsCompleted).ToHashSet();
        foreach (var complete in completeTasks)
        {
            var stats = complete.Result;
            statSnapshot.Add(stats);
        }
        return tasks.Except(completeTasks).ToHashSet();
    }

    private async Task<Stats> RunOneUnary(CacheClient.Scs.ScsClient client, Stats stats, ulong i, CancellationToken cancellationToken)
    {
        uint item = (uint)(i % keyspace);
        await Unary.Set(item, client, util, stats, cancellationToken);

        await Unary.Get(item, client, util, stats, cancellationToken);
        return stats;
    }

    private async Task<Stats> RunOneDictionary(CacheClient.Scs.ScsClient client, Stats stats, ulong i, Random random, CancellationToken cancellationToken)
    {
        uint dictionary = (uint)random.NextInt64(keyspace);
        uint count = 3;
        uint fieldStart = (uint)(i % (structuresize - count));
        await Dictionary.Set(dictionary, fieldStart, count, client, util, stats, cancellationToken);

        await Dictionary.Get(dictionary, fieldStart, count, client, util, stats, cancellationToken);
        return stats;
    }

    private async Task<Stats> RunOneClientUnary(Stats stats, ulong i)
    {
        uint item = (uint)(i % keyspace);
        await Unary.ClientSet(item, momentoClient, cacheName, stats);

        await Unary.ClientGet(item, momentoClient, cacheName, stats);
        return stats;
    }
}
