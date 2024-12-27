using System.Collections.Concurrent;

namespace HighHttpRequestCountDemo.Services;

/// <summary>
/// A concurrency queue which applies the given action for each item.</summary>
internal class ActionQueue<T>(Action<T> action, int concurrencyLimit = 4)
{
    private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(concurrencyLimit);
    private readonly Action<T> _action = action;
    private bool _isProcessing = false;

    private void StartProcessing()
    {
        if (_isProcessing)
        {
            return;
        }

        int dequeuTries = 0;
        while (!_queue.IsEmpty)
        {
            _isProcessing = true;

            if (!_queue.TryDequeue(out T? item))
            {
                if (++dequeuTries == 3)
                {
                    throw new Exception("Exeeded dequeue retries.");
                }
                continue;
            }
            dequeuTries = 0;

            Task.Run(async () =>
            {
                try
                {
                    await _semaphore.WaitAsync();

                    _action(item);
                }
                catch (Exception ex)
                {
                    Program.WriteLine($"Error: {ex}", ConsoleColor.Red);
                }
                finally
                {
                    _semaphore.Release();
                }
            });
        }
        _isProcessing = false;
    }

    /// <summary>Queues an item for the action specified in the constructor. </summary>
    public void Submit(T item)
    {
        _queue.Enqueue(item);

        // Ensure that processing is taking place
        StartProcessing();
    }
}
