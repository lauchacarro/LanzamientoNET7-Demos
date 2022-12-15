


using Grpc.Core;

namespace GrpcDemo7;

public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger _logger;

    public GreeterService(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<GreeterService>();
    }

    /// <summary>
    /// Say hello.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        _logger.LogInformation($"Sending hello to {request.Name}");
        return Task.FromResult(new HelloReply { Message = $"Hello {request.Name}" });
    }

}