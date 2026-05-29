using Microsoft.UI.Dispatching;

namespace PussyCats.App.Configuration;

/// <summary>
/// Holds the UI thread's DispatcherQueue so ViewModels can marshal property-change
/// notifications back to the UI thread without taking a UI dependency directly.
/// Set once from App() before any ViewModel is created.
/// </summary>
public static class UIDispatcher
{
    public static DispatcherQueue? Queue { get; set; }

    public static void Enqueue(Action action)
    {
        if (Queue is null || Queue.HasThreadAccess)
            action();
        else
            Queue.TryEnqueue(new DispatcherQueueHandler(action));
    }

    public static Task EnqueueAsync(Action action)
    {
        if (Queue is null || Queue.HasThreadAccess)
        {
            action();
            return Task.CompletedTask;
        }

        var completion = new TaskCompletionSource();
        var queued = Queue.TryEnqueue(() =>
        {
            try
            {
                action();
                completion.SetResult();
            }
            catch (Exception exception)
            {
                completion.SetException(exception);
            }
        });

        if (!queued)
        {
            completion.SetException(new InvalidOperationException("Could not enqueue work on the UI dispatcher."));
        }

        return completion.Task;
    }
}
