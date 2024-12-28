using System.Collections.Concurrent;

namespace HighHttpRequestCountDemo.Services;

internal class BlockingActionQueue<T>
{
    private readonly Action<T> _action;
    private readonly BlockingCollection<T> _queue;

    public BlockingActionQueue(Action<T> action, int concurrencyLimit = 4)
    {
        _action = action;
        _queue = new BlockingCollection<T>(new ConcurrentQueue<T>(), concurrencyLimit);

        Process();
    }

    private void Process()
    {
        Task.Factory.StartNew(() =>
        {
            T? item = default;
            while (!_queue.IsCompleted)
            {
                try
                {
                    item = _queue.Take();
                }
                // Can happen if one thread calls CompleteAdding after the IsCompleted test.
                catch (InvalidOperationException) { }

                if (item != null)
                {
                    Task.Run(() =>
                    {
                        _action(item);
                    });
                }
            }
        }, TaskCreationOptions.LongRunning);
    }

    internal void Stop()
    {
        Console.WriteLine($"\nCompleting the queue.");
        _queue.CompleteAdding();
    }

    internal void Submit(T item)
    {
        Task.Run(() =>
        {
            // Will block when threshold met.
            _queue.Add(item);
        });
    }
}
