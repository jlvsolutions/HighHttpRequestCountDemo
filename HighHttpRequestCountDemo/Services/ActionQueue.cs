using System.Collections.Concurrent;

namespace HighHttpRequestCountDemo.Services;

/// <summary>
/// A concurrency queue which applies the given action for each item.</summary>
internal class ActionQueue<T>(Action<T> action, int concurrencyLimit = 4)
{
    private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(concurrencyLimit, concurrencyLimit);
    private readonly Action<T> _action = action;
    private bool _isProcessing = false;

    private void StartProcessing()
    {
        //Console.WriteLine($"StartProcessing() called.  {_isProcessing}, SubmittedCount: {submittedCnt}");

        if (_isProcessing)
        {
            return;
        }

        while (!_queue.IsEmpty)
        {
            _isProcessing = true;

            _ = _queue.TryDequeue(out T? item); // TODO:  Wrap with error handling.

            Task.Run(async () =>
            {
                try
                {
                    await _semaphore.WaitAsync();

                    item = item ?? default;
                    _action(item!);
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

        //Console.WriteLine("Exiting StartProcessing().");
    }

    int submittedCnt = 0;
    /// <summary>Queues an item for the action specified in the constructor. </summary>
    public void Submit(T item)
    {
        _queue.Enqueue(item);

        submittedCnt++;

        // Ensure that processing is taking place
        StartProcessing();
    }
}
