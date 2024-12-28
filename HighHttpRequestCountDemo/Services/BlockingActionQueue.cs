using System.Collections.Concurrent;

namespace HighHttpRequestCountDemo.Services;

internal class BlockingActionQueue<T>
{
    private readonly Action<T> _action;
    private readonly BlockingCollection<T> _queue;
    private readonly SemaphoreSlim _actionSem = new SemaphoreSlim(1);

    public BlockingActionQueue(Action<T> action, int concurrencyLimit)
    {
        _action = action;
        _queue = new BlockingCollection<T>(concurrencyLimit);
        //_queue = new BlockingCollection<T>(new ConcurrentQueue<T>(), concurrencyLimit);

        Process();
    }

    private void Process()
    {
        Task.Factory.StartNew(() =>
        {
            T? item = default;
            while (!_queue.IsCompleted)
            {
                _actionSem.Wait();
                try
                {
                    item = _queue.Take();
                    //Console.WriteLine($"Took item: {item}");
                }
                catch (InvalidOperationException ex)
                {
                    // Can happen if one thread calls CompleteAdding after the IsCompleted test in this loop.
                    //Program.WriteLine(ex.Message, ConsoleColor.Red);
                }

                if (item != null)
                {
                    _action(item);
                }
                _actionSem.Release();
            }
        }, TaskCreationOptions.LongRunning);
    }
    internal void Stop()
    {
        if (!_queue.IsCompleted)
        {
            Console.WriteLine($"\nCompleting the queue.");
            _queue.CompleteAdding();
        }
    }

    internal void Submit(T item)
    {
        Task.Run(() =>
        {
            // Will block when bounded capacity met.
            _queue.Add(item);
            //Console.WriteLine($"Added item: {item}");
        });
    }
}
