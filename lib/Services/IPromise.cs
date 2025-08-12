using System;
using System.Threading;
using System.Threading.Tasks;

namespace lib.Services;

/// <summary>
/// Common interface for all promise types
/// </summary>
public interface IPromise : IDisposable
{
    Task Task { get; }
    CancellationToken CancellationToken { get; }
    bool IsCompleted { get; }
    bool IsCancelled { get; }

    void Return();
    void Throw(Exception exception);
    void AddChild(IPromise child);
    void Cancel();
}
