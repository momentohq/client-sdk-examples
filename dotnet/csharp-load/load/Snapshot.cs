using HdrHistogram;
using static Printing.Printing;

record class Stats
{
    public long getLatencyTicks;
    public long setLatencyTicks;
    public uint concurrency;
    public Grpc.Core.RpcException? getError;
    public Grpc.Core.RpcException? setError;
}


record class Snapshot
{
    public LongHistogram getLatency;
    public LongHistogram setLatency;
    public LongHistogram concurrency;
    public Dictionary<string, uint> getErrors;
    public Dictionary<string, uint> setErrors;

    public Snapshot(LongHistogram getLatency, LongHistogram setLatency, LongHistogram concurrency, Dictionary<string, uint> getErrors, Dictionary<string, uint> setErrors)
    {
        this.getLatency = getLatency;
        this.setLatency = setLatency;
        this.concurrency = concurrency;
        this.getErrors = getErrors;
        this.setErrors = setErrors;
    }

    public static Snapshot Default()
    {
        return new Snapshot (
            getLatency: new LongHistogram(TimeStamp.Seconds(10), 3),
            setLatency: new LongHistogram(TimeStamp.Seconds(10), 3),
            concurrency: new LongHistogram(1000, 3),
            getErrors: new(),
            setErrors: new()
        );
    }

    public void Add(Snapshot other)
    {
        getLatency.Add(other.getLatency);
        setLatency.Add(other.setLatency);
        concurrency.Add(other.concurrency);
        foreach (var (key, value) in other.getErrors)
        {
            getErrors[key] = getErrors.GetValueOrDefault(key) + value;
        }
        foreach (var (key, value) in other.setErrors)
        {
            setErrors[key] = setErrors.GetValueOrDefault(key) + value;
        }
    }

    public void Add(Stats other)
    {
        getLatency.RecordValue(other.getLatencyTicks);
        setLatency.RecordValue(other.setLatencyTicks);
        concurrency.RecordValue(other.concurrency);
        if (other.getError != null)
        {
            getErrors[other.getError.Status.Detail] = getErrors.GetValueOrDefault(other.getError.Status.Detail) + 1;
        }
        if (other.setError != null)
        {
            setErrors[other.setError.Status.Detail] = setErrors.GetValueOrDefault(other.setError.Status.Detail) + 1;
        }
    }

    public void Reset()
    {
        getLatency.Reset();
        setLatency.Reset();
        concurrency.Reset();
        getErrors.Clear();
        setErrors.Clear();
    }

    public void Print(TimeSpan interval)
    {
        Console.WriteLine("\n");
        var latency = new LongHistogram(TimeStamp.Seconds(10), 3);
        latency.Add(getLatency);
        latency.Add(setLatency);

        PrintColumns(
            new Column("Latency", latency.PercentileDistributionToString(OutputScalingFactor.TimeStampToMicroseconds).Split('\n')),
            new Column("Concurrency", concurrency.PercentileDistributionToString(1).Split('\n'))
        );

        var tps = latency.TotalCount / interval.TotalSeconds;
        var errorRate = (getErrors.Sum(p => p.Value) + setErrors.Sum(p => p.Value)) / (double)latency.TotalCount;
        var p999 = latency.GetValueAtPercentile(99.9) / OutputScalingFactor.TimeStampToMilliseconds;
        var p90 = latency.GetValueAtPercentile(90) / OutputScalingFactor.TimeStampToMilliseconds;
        var p50 = latency.GetValueAtPercentile(50) / OutputScalingFactor.TimeStampToMilliseconds;

        getErrors.PrintDictionaryIfNotEmpty("Get errors");
        setErrors.PrintDictionaryIfNotEmpty("Set errors");
        Console.WriteLine($"tps: {tps,7:N2}  |  p999: {p999,5:N2}ms  |  p90: {p90,5:N2}ms  |  p50: {p50,5:N2}ms  |  error: {errorRate * 100,4:N2}%");
    }
}
