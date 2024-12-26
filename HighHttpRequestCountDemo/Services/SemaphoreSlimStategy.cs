using HighHttpRequestCountDemo.API.Domain;
using System.Collections.Concurrent;

namespace HighHttpRequestCountDemo.Services;

internal class SemaphoreSlimStategy(HttpClient client, string baseUrl) : IDemoStrategy
{
    public string Name => "SemaphoreSlim Strategy";

    public string Description => "Uses a SemaphoreSlim to implement a circuit breaker strategy in order to limit concurrent http requests.";

    public IReadOnlyList<User> Execute(int numberOfRequests)
    {
        List<int> userIds = Enumerable.Range(1, numberOfRequests).ToList();
        SemaphoreSlim semaphoreSlim = new SemaphoreSlim(initialCount: 10, maxCount: 10);
        ConcurrentBag<User> responses = new ConcurrentBag<User>();

        IEnumerable<Task> tasks = userIds.Select(async userId =>
        {
            await semaphoreSlim.WaitAsync();

            try
            {
                responses.Add(await client.GetUser($"{baseUrl}/user/{userId}"));

                if (userId % (int)(userIds.Count * .05) == 0)
                {
                    Console.Write($"\r{userId}"); // Give a sense of progress for UX.  Not exactly accurate but enough for demo.
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        });

        Task.WhenAll(tasks).Wait();
        Console.Write("\r");

        return [.. responses];
    }
}
