using HighHttpRequestCountDemo.API.Domain;
using System.Threading.Tasks.Dataflow;

namespace HighHttpRequestCountDemo.Services;

internal class TransformBlockStrategy(HttpClient client, string baseUrl, int concurrencyLimit = 10) : IDemoStrategy
{
    public string Name => "TransformBlock Strategy";
    public string Description => @"Uses the TPL to act like a queue as a form of a circuit breaker strategy in order to limit concurrent http requests.";

    public IReadOnlyList<User> Execute(int numberOfRequests)
    {
        List<int> userIds = Enumerable.Range(1, numberOfRequests).ToList();
       
        TransformBlock<string, User> transform = new TransformBlock<string, User>(
            client.GetUser,
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = concurrencyLimit } );

        BufferBlock<User> buffer = new BufferBlock<User>();
        
        transform.LinkTo(buffer);

        Console.WriteLine($"Subitting {numberOfRequests:N0} requests...");
        userIds.ForEach(async userId =>
        {
            await transform.SendAsync($"{baseUrl}/user/{userId}");
        });
        Console.WriteLine($"Requests submission completed.");

        transform.Complete();

        Console.WriteLine("Completing processing...");
        transform.Completion.Wait();

        return buffer.TryReceiveAll(out var users)
            ? users.AsReadOnly()
            : throw new Exception("Error when trying to retrieve buffer");
    }
}
