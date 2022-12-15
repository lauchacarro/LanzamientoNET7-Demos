
using Grpc.Health.V1;
using Grpc.Net.Client;

var channel = GrpcChannel.ForAddress("http://localhost:5076");
var client = new Health.HealthClient(channel);

var response = await client.CheckAsync(new HealthCheckRequest());
var status = response.Status;

Console.WriteLine(status);
// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
Console.ReadLine();
