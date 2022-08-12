# Momento C# Direct Integration

A lower-level direct grpc integration that gives an example of how to bypass the Momento Client.

## How to run
You need `dotnet` to build and run. Grab [the current LTS](https://dotnet.microsoft.com/en-us/download) and continue.

An example invocation would look like:
```
# Make sure you have a cache!
➜  momento cache create --name your_cache

➜  dotnet run -- --endpoint=cache.cell-us-east-1-1.prod.a.momentohq.com --hz=1000 --clients=4 --concurrency=8 --interval=5 --cache=your_cache --token=$YOUR_MOMENTO_TOKEN
```

```
➜  dotnet run
Momento C# Load Generator

Usage:
  --endpoint=cache.cell-us-east-1-1.prod.a.momentohq.com  [required]
  --token=your_momento_token                              [required]
  --cache=cache_to_test                                   [required]
  --hz=2000                                               [100]
    Set the request rate to attempt
  --clients=5                                             [processorcount - 1]
    Set the number of concurrent clients to use to attempt the configured hz
  --concurrency=5                                         [100]
    Set the maximum number of concurrent h2 streams per client
  --interval=10                                           [10]
    Set the console report interval (in seconds)
```
