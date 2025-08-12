using System.Threading;
using lib.Services;

namespace lib.Model;

/// <summary>
/// Context for accessing current promise similar to NUnit's RunContext.Current
/// </summary>
public static class PromiseContext
{
    private static readonly ThreadLocal<IPromise> _current = new();

    /// <summary>
    /// Get the current promise context
    /// </summary>
    public static IPromise Current => _current.Value;

    /// <summary>
    /// Set the current promise context
    /// </summary>
    internal static void SetCurrent(IPromise promise)
    {
        _current.Value = promise;
    }

    /// <summary>
    /// Clear the current promise context
    /// </summary>
    internal static void Clear()
    {
        _current.Value = null;
    }

    /// <summary>
    /// Execute work within a promise context
    /// </summary>
    public static Promise Run(System.Action work)
    {
        var promise = new Promise();
        var previousContext = _current.Value;

        try
        {
            _current.Value = promise;
            work();
            if (!promise.IsCompleted)
                promise.Return();
        }
        catch (System.Exception ex)
        {
            promise.Throw(ex);
        }
        finally
        {
            _current.Value = previousContext;
        }

        return promise;
    }

    /// <summary>
    /// Execute work within a promise context with return value
    /// </summary>
    public static Promise<T> Run<T>(System.Func<T> work)
    {
        var promise = new Promise<T>();
        var previousContext = _current.Value;

        try
        {
            _current.Value = promise;
            var result = work();
            if (!promise.IsCompleted)
                promise.Return(result);
        }
        catch (System.Exception ex)
        {
            promise.Throw(ex);
        }
        finally
        {
            _current.Value = previousContext;
        }

        return promise;
    }

    /// <summary>
    /// Immediately return from current promise context
    /// </summary>
    public static void Return()
    {
        Current?.Return();
    }

    /// <summary>
    /// Immediately return value from current promise context
    /// </summary>
    public static void Return<T>(T value)
    {
        if (Current is Promise<T> typedPromise)
            typedPromise.Return(value);
    }

    /// <summary>
    /// Immediately throw from current promise context
    /// </summary>
    public static void Throw(System.Exception exception)
    {
        Current?.Throw(exception);
    }

    /// <summary>
    /// Add child to current promise context
    /// </summary>
    public static void AddChild(IPromise child)
    {
        Current?.AddChild(child);
    }

    /// <summary>
    /// Cancel current promise context
    /// </summary>
    public static void Cancel()
    {
        Current?.Cancel();
    }
}
