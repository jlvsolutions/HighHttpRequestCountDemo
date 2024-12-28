using HighHttpRequestCountDemo.API.Domain;
using System.Collections.Concurrent;

namespace HighHttpRequestCountDemo.Services;

internal class BlockingCollectionStrategy(HttpClient client, string baseUrl, int concurrencyLimit = 10) : IDemoStrategy
{
    public string Name => "BlockingCollection Strategy";
    public string Description => "This strategy leverages BlockingCollection's blocking and bounding functionality\n" +
                                 "to throttle concurrent http requests.";
    BlockingActionQueue<int> _queue = null!;
    private readonly ConcurrentBag<User> _responses = [];
    private readonly SemaphoreSlim _completed = new SemaphoreSlim(0);
    private int _numberOfRequests;


    public IReadOnlyList<User> Execute(int numberOfRequests)
    {
        _numberOfRequests = numberOfRequests;
        List<int> userIds = Enumerable.Range(1, numberOfRequests).ToList();
        _queue = new BlockingActionQueue<int>(ProcessUserRequest, concurrencyLimit);
        _responses.Clear();

        Console.WriteLine($"Submitting {numberOfRequests:N0} requests...");
        userIds.ForEach(_queue.Submit);
        Console.WriteLine($"\nCompleted submitting {numberOfRequests:N0} requests.");

        Console.WriteLine("Waiting for responses to complete...");

        _completed.Wait();

        return _responses.ToList().AsReadOnly();
    }

    private async void ProcessUserRequest(int userId)
    {
        _responses.Add(await client.GetUser($"{baseUrl}/user/{userId}"));

        if (_responses.Count % 50 == 0)
        {
            Console.Write($"\rResponses: {_responses.Count:N0}");
        }

        if (_responses.Count == _numberOfRequests)
        {
            _queue.Stop();
            _completed.Release();
        }
    }
}
