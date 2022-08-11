namespace momento_csharp_load_generator.load.requests
{
    public record class RequestUtil(
        Grpc.Core.Metadata Headers
    ) {
        public Grpc.Core.CallOptions GetCallOptions(CancellationToken cancellationToken)
        {
            return new Grpc.Core.CallOptions(
                cancellationToken: CancellationToken.None,
                headers: Headers,
                deadline: DateTime.UtcNow + TimeSpan.FromSeconds(1)
            );
        }
    }
}

