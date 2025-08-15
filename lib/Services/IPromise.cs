using System;
using System.Threading;
using System.Threading.Tasks;

namespace lib.Services;

/// <summary>
/// Common interface for cancellable promises with composition helpers.
/// </summary>
/// <remarks>
/// Promises can be awaited directly and can propagate completion to child promises.
/// </remarks>
public interface IPromise : IDisposable
{
    /// <summary>
    /// Underlying task representing the asynchronous operation.
    /// </summary>
    Task Task { get; }

    /// <summary>
    /// Cancellation token associated with this promise.
    /// </summary>
    CancellationToken CancellationToken { get; }

    /// <summary>
    /// Indicates whether the promise has completed (successfully, faulted, or canceled).
    /// </summary>
    bool IsCompleted { get; }

    /// <summary>
    /// Indicates whether the promise was canceled.
    /// </summary>
    bool IsCancelled { get; }

    /// <summary>
    /// Immediately completes the promise successfully (void result for non-generic variants).
    /// </summary>
    void Return();

    /// <summary>
    /// Immediately completes the promise with an exception.
    /// </summary>
    void Throw(Exception exception);

    /// <summary>
    /// Adds a child promise that must complete before this promise finalizes its state.
    /// </summary>
    void AddChild(IPromise child);

    /// <summary>
    /// Requests cancellation of this promise (and typically its children).
    /// </summary>
    void Cancel();
}
