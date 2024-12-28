using HighHttpRequestCountDemo.API.Domain;
using System.Collections.Concurrent;

namespace HighHttpRequestCountDemo.Services;

internal class ConcurrentQueueStrategy(HttpClient client, string baseUrl, int concurrencyLimit = 10) : IDemoStrategy
{
    public string Name => "ConcurrentQueue Strategy";
    public string Description => "Any requests Enqueued will be dequeued and sent using circuit breaker strategy and concurrency limiting.\n" +
                                 "This strategy has the potential for very large volume.";
    private readonly ConcurrentBag<User> _responses = [];
    private readonly SemaphoreSlim _completed = new SemaphoreSlim(0);
    private int _numberOfRequests;

    public IReadOnlyList<User> Execute(int numberOfRequests)
    {
        _numberOfRequests = numberOfRequests;
        List<int> userIds = Enumerable.Range(1, numberOfRequests).ToList();
        ActionQueue<int> queue = new ActionQueue<int>(ProcessEnqueuedItem, concurrencyLimit);
        _responses.Clear();

        int firstBurstCount = (int)(userIds.Count * 0.05);
        int SecondBurstCount = numberOfRequests - firstBurstCount;

        Console.WriteLine($"Submitting {numberOfRequests:N0} requests...");

        // All at onece.
        //for ( int i = 0; i < numberOfRequests; i++)
        //{
        //    queue.Submit(userIds[i]);
        //}


        // Or, in bursts.
        int i = 0;
        Console.WriteLine($"Submitting First batch of {firstBurstCount} requests...");
        for (; i < firstBurstCount; i++)
        {
            queue.Submit(userIds[i]);
        }

        Program.WriteLine("Delaying 1 second for simulating caller's other busy work...", ConsoleColor.White);
        Thread.Sleep(1000);

        Console.WriteLine($"\nSubmitting Second batch of {SecondBurstCount} requests...");
        for (i = firstBurstCount; i < firstBurstCount + SecondBurstCount; i++)
        {
            queue.Submit(userIds[i]);
        }

        Console.WriteLine($"\nCompleted submitting all {numberOfRequests:N0} requests.");

        _completed.Wait();

        return _responses.ToList().AsReadOnly();
    }

    private async void ProcessEnqueuedItem(int userId)
    {
        _responses.Add(await client.GetUser($"{baseUrl}/user/{userId}"));

        if (_responses.Count % 50 == 0)
        {
            Console.Write($"\rResponses: {_responses.Count:N0}");
        }

        if (_responses.Count == _numberOfRequests)
        {
            _completed.Release();
        }
    }
}
