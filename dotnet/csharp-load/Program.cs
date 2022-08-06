
namespace application;

class Program {
    static async Task Main(string[] args)
    {
        string endpoint;
        string authToken;
        string cache;
        uint hz;
        uint reportInterval;
        uint clients;
        uint concurrency;
        try
        {
            endpoint = args.Where((arg) => arg.StartsWith("--endpoint="))
                    .First()
                    .Split("=")[1];

            authToken = args.Where((arg) => arg.StartsWith("--token="))
                    .First()
                    .Split("=")[1];

            cache = args.Where((arg) => arg.StartsWith("--cache="))
                    .First()
                    .Split("=")[1];

            hz = uint.Parse(
                args.Where((arg) => arg.StartsWith("--hz="))
                    .FirstOrDefault("=100")
                    .Split("=")[1]
            );

            reportInterval = uint.Parse(
                args.Where((arg) => arg.StartsWith("--interval="))
                    .FirstOrDefault("=10")
                    .Split("=")[1]
            );

            clients = uint.Parse(
                args.Where((arg) => arg.StartsWith("--clients="))
                    .FirstOrDefault("=" + Math.Max(1, Environment.ProcessorCount - 1))
                    .Split("=")[1]
            );

            // Requests per client. Momento defaults to 100 maximum per client instance.
            concurrency = uint.Parse(
                args.Where((arg) => arg.StartsWith("--concurrency="))
                    .FirstOrDefault("=100")
                    .Split("=")[1]
            );
        }
        catch (Exception)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Momento C# Load Generator\n");
            Console.WriteLine("Usage:");
            Console.WriteLine("  --endpoint=cache.cell-us-east-1-1.prod.a.momentohq.com  [required]");
            Console.WriteLine("  --token=your_momento_token                              [required]");
            Console.WriteLine("  --cache=cache_to_test                                   [required]");
            Console.WriteLine("  --hz=2000                                               [100]");
            Console.WriteLine("    Set the request rate to attempt");
            Console.WriteLine("  --clients=5                                             [processorcount - 1]");
            Console.WriteLine("    Set the number of concurrent clients to use to attempt the configured hz");
            Console.WriteLine("  --concurrency=5                                         [100]");
            Console.WriteLine("    Set the maximum number of concurrent h2 streams per client");
            Console.WriteLine("  --interval=10                                           [10]");
            Console.WriteLine("    Set the console report interval (in seconds)");
            Environment.Exit(1);
            throw new Exception("impossible");
        }

        Console.WriteLine("Starting {0:D}hz", hz);

        var cancellationSource = new CancellationTokenSource();

        Console.CancelKeyPress += delegate {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Error.WriteLine("\nTerminating load threads...");
            Console.ResetColor();
            cancellationSource.Cancel();
        };

        await new LoadGenerator(cancellationSource, endpoint, authToken, cache).Run(hz, reportInterval, clients, concurrency);

        Console.WriteLine("All done");
    }
}
