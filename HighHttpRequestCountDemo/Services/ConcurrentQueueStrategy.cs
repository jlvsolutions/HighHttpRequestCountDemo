using HighHttpRequestCountDemo.API.Domain;
using System.Collections.Concurrent;

namespace HighHttpRequestCountDemo.Services;

internal class ConcurrentQueueStrategy(HttpClient client, string baseUrl, int concurrencyLimit = 10) : IDemoStrategy
{
    public string Name => "ConcurrentQueue Strategy";
    public string Description => "Any requests Enqueued will be dequeued and sent using circuit breaker strategy and concurrency limiting.";
    private readonly ConcurrentBag<User> _responses = [];
    private readonly SemaphoreSlim Completed = new SemaphoreSlim(1);
    private int _numberOfRequests;

    public IReadOnlyList<User> Execute(int numberOfRequests)
    {
        List<int> userIds = Enumerable.Range(1, numberOfRequests).ToList();
        ActionQueue<int> queue = new(ProcessEnqueuedItem, concurrencyLimit);

        int firstBurstCount = (int)(userIds.Count * 0.05);
        int SecondBurstCount = numberOfRequests - firstBurstCount;
        _numberOfRequests = numberOfRequests;

        Completed.Wait();

        Console.WriteLine($"Submitting {numberOfRequests:N0} requests...");
        for ( int i = 0; i < numberOfRequests; i++)
        {
            queue.Submit(userIds[i]);
        }

        //int i = 0;
        //Console.WriteLine($"Submitting First batch of {firstBurstCount} requests...");
        //for (; i < firstBurstCount; i++)
        //{
        //    queue.Submit(userIds[i]);
        //}

        //Console.WriteLine("Delaying for simulated other busy work...");
        //for (int j = 0; j < 500_000_000; j++)
        //{
        //    int four = 2 + 2;
        //}

        //Console.WriteLine($"\nSubmitting Second batch of {SecondBurstCount} requests...");
        //for (i = firstBurstCount; i < firstBurstCount + SecondBurstCount; i++)
        //{
        //    queue.Submit(userIds[i]);
        //}

        Console.WriteLine($"\nCompleted submitting all {userIds.Count:N0} requests.");

        Completed.Wait();

        return _responses.ToList().AsReadOnly();
    }

    private async void ProcessEnqueuedItem(int userId)
    {
        _responses.Add(await client.GetUser($"{baseUrl}/user/{userId}"));

        if (_responses.Count % 100 == 0)
        {
            Console.Write($"\rResponses: {_responses.Count:N0}");
        }

        if (_responses.Count == _numberOfRequests)
        {
            Completed.Release();
        }
    }
}
