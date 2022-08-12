using HdrHistogram;
using Momento.Grpc;
using static Printing.Printing;

public record class Stats
{
    public long getLatencyTicks;
    public long setLatencyTicks;
    public uint concurrency;
    public Exception? getError;
    public Exception? setError;
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
            getLatency: new LongHistogram(TimeStamp.Seconds(2), 2),
            setLatency: new LongHistogram(TimeStamp.Seconds(2), 2),
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
        try
        {
            getLatency.RecordValue(other.getLatencyTicks);
        }
        catch (IndexOutOfRangeException)
        {
            getLatency.RecordValue(getLatency.HighestTrackableValue);
        }
        try
        {
            setLatency.RecordValue(other.setLatencyTicks);
        }
        catch (IndexOutOfRangeException)
        {
            setLatency.RecordValue(setLatency.HighestTrackableValue);
        }
        try
        {
            concurrency.RecordValue(other.concurrency);
        }
        catch (IndexOutOfRangeException)
        {
            concurrency.RecordValue(concurrency.HighestTrackableValue);
        }
        if (other.getError != null)
        {
            if (other.getError is Grpc.Core.RpcException)
            {
                var e = other.getError as Grpc.Core.RpcException;
                getErrors[e!.Status.Detail] = getErrors.GetValueOrDefault(e!.Status.Detail) + 1;
            } else
            {
                getErrors[other.getError.Message] = getErrors.GetValueOrDefault(other.getError.Message) + 1;
            }
        }
        if (other.setError != null)
        {
            if (other.setError is Grpc.Core.RpcException)
            {
                var e = other.setError as Grpc.Core.RpcException;
                setErrors[e!.Status.Detail] = setErrors.GetValueOrDefault(e!.Status.Detail) + 1;
            }
            else
            {
                setErrors[other.setError.Message] = setErrors.GetValueOrDefault(other.setError.Message) + 1;
            }
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

    public void Print(TestMode testMode, TimeSpan interval)
    {
        Console.WriteLine("\n");
        var latency = new LongHistogram(TimeStamp.Seconds(2), 2);
        latency.Add(getLatency);
        latency.Add(setLatency);

        PrintColumns(
            new Column($"{testMode} Set Latency", setLatency.PercentileDistributionToString(OutputScalingFactor.TimeStampToMicroseconds).Split('\n')),
            new Column($"{testMode} Get Latency", getLatency.PercentileDistributionToString(OutputScalingFactor.TimeStampToMicroseconds).Split('\n')),
            new Column($"{testMode} Concurrency", concurrency.PercentileDistributionToString(1).Split('\n'))
        );

        var tps = (getLatency.TotalCount + setLatency.TotalCount) / interval.TotalSeconds;
        var errorRate = (getErrors.Sum(p => p.Value) + setErrors.Sum(p => p.Value)) / (double)(getLatency.TotalCount + setLatency.TotalCount);
        var p999 = latency.GetValueAtPercentile(99.9) / OutputScalingFactor.TimeStampToMilliseconds;
        var p90 = latency.GetValueAtPercentile(90) / OutputScalingFactor.TimeStampToMilliseconds;
        var p50 = latency.GetValueAtPercentile(50) / OutputScalingFactor.TimeStampToMilliseconds;

        getErrors.PrintDictionaryIfNotEmpty($"{testMode} Get errors");
        setErrors.PrintDictionaryIfNotEmpty($"{testMode} Set errors");
        Console.WriteLine($"tps: {tps,7:N2}  |  p999: {p999,5:N2}ms  |  p90: {p90,5:N2}ms  |  p50: {p50,5:N2}ms  |  error: {errorRate * 100,4:N2}%  |  requests: {latency.TotalCount,-8}");
    }
}
