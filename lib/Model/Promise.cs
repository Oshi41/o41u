using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using lib.Services;

namespace lib.Model;

/// <summary>
/// Cancellable promise with etask-like functionality
/// </summary>
public class Promise<T> : IPromise
{
    private readonly TaskCompletionSource<T> _taskCompletionSource;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly List<IPromise> _children = new();
    private readonly object _lock = new();
    private bool _disposed;
    private Exception _exception;
    private T _result;
    private bool _isCompleted;

    public Task<T> Task => _taskCompletionSource.Task;
    Task IPromise.Task => _taskCompletionSource.Task;
    public CancellationToken CancellationToken => _cancellationTokenSource.Token;
    public bool IsCompleted => _isCompleted;
    public bool IsCancelled => _cancellationTokenSource.Token.IsCancellationRequested;

    public Promise()
    {
        _taskCompletionSource = new TaskCompletionSource<T>();
        _cancellationTokenSource = new CancellationTokenSource();
        PromiseContext.SetCurrent(this);
    }

    public Promise(Func<T> work) : this()
    {
        try
        {
            var result = work();
            Return(result);
        }
        catch (Exception ex)
        {
            Throw(ex);
        }
    }

    public Promise(Func<Task<T>> asyncWork) : this()
    {
        _ = System.Threading.Tasks.Task.Run(async () =>
        {
            try
            {
                var result = await asyncWork();
                Return(result);
            }
            catch (Exception ex)
            {
                Throw(ex);
            }
        });
    }

    /// <summary>
    /// Immediately return a value and complete the promise
    /// </summary>
    public void Return(T value)
    {
        lock (_lock)
        {
            if (_isCompleted) return;
            _isCompleted = true;
            _result = value;
            _taskCompletionSource.SetResult(value);
            CompleteChildren();
        }
    }

    /// <summary>
    /// Immediately return with default value and complete the promise (IPromise implementation)
    /// </summary>
    void IPromise.Return()
    {
        Return(default(T));
    }

    /// <summary>
    /// Immediately throw an exception and complete the promise
    /// </summary>
    public void Throw(Exception exception)
    {
        lock (_lock)
        {
            if (_isCompleted) return;
            _isCompleted = true;
            _exception = exception;
            _taskCompletionSource.SetException(exception);
            CompleteChildren();
        }
    }

    /// <summary>
    /// Add a child promise that must complete before this promise can complete
    /// </summary>
    public void AddChild(IPromise child)
    {
        lock (_lock)
        {
            if (_isCompleted) return;
            _children.Add(child);
        }
    }

    /// <summary>
    /// Wait for all children to complete
    /// </summary>
    private void CompleteChildren()
    {
        if (_children.Count == 0) return;

        _ = System.Threading.Tasks.Task.Run(async () =>
        {
            try
            {
                await System.Threading.Tasks.Task.WhenAll(_children.Select(c => c.Task));
            }
            catch
            {
                // Children exceptions are handled by their own promises
            }
        });
    }

    /// <summary>
    /// Cancel the promise and all children
    /// </summary>
    public void Cancel()
    {
        lock (_lock)
        {
            if (_isCompleted) return;
            _cancellationTokenSource.Cancel();
            foreach (var child in _children)
            {
                child.Cancel();
            }

            _taskCompletionSource.SetCanceled();
            _isCompleted = true;
        }
    }

    /// <summary>
    /// Catch and handle exceptions
    /// </summary>
    public Promise<T> Catch(Func<Exception, T> handler)
    {
        return new Promise<T>(async () =>
        {
            try
            {
                return await Task;
            }
            catch (Exception ex)
            {
                return handler(ex);
            }
        });
    }

    /// <summary>
    /// Transform the result
    /// </summary>
    public Promise<TResult> Then<TResult>(Func<T, TResult> transform)
    {
        return new Promise<TResult>(async () =>
        {
            var result = await Task;
            return transform(result);
        });
    }

    /// <summary>
    /// Transform the result asynchronously
    /// </summary>
    public Promise<TResult> ThenAsync<TResult>(Func<T, Task<TResult>> transform)
    {
        return new Promise<TResult>(async () =>
        {
            var result = await Task;
            return await transform(result);
        });
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        lock (_lock)
        {
            foreach (var child in _children)
            {
                child?.Dispose();
            }

            _children.Clear();
        }

        _cancellationTokenSource?.Dispose();
        PromiseContext.Clear();
    }

    /// <summary>
    /// Get the awaiter for direct awaiting of the promise
    /// </summary>
    public System.Runtime.CompilerServices.TaskAwaiter<T> GetAwaiter()
    {
        return Task.GetAwaiter();
    }

    public static implicit operator Task<T>(Promise<T> promise) => promise.Task;
    public static implicit operator Promise<T>(T value) => new Promise<T>(() => value);
}

/// <summary>
/// Non-generic promise for void operations
/// </summary>
public class Promise : IPromise
{
    private readonly Promise<object> _inner;

    public Task Task => _inner.Task.ContinueWith(t => { }, TaskContinuationOptions.ExecuteSynchronously);
    public CancellationToken CancellationToken => _inner.CancellationToken;
    public bool IsCompleted => _inner.IsCompleted;
    public bool IsCancelled => _inner.IsCancelled;

    public Promise()
    {
        _inner = new Promise<object>();
    }

    public Promise(Action work) : this()
    {
        try
        {
            work();
            Return();
        }
        catch (Exception ex)
        {
            Throw(ex);
        }
    }

    public Promise(Func<Task> asyncWork) : this()
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await asyncWork();
                Return();
            }
            catch (Exception ex)
            {
                Throw(ex);
            }
        });
    }

    public void Return() => _inner.Return(null);
    public void Throw(Exception exception) => _inner.Throw(exception);
    public void AddChild(IPromise child) => _inner.AddChild(child);
    public void Cancel() => _inner.Cancel();

    public Promise Catch(Action<Exception> handler)
    {
        return new Promise(async () =>
        {
            try
            {
                await Task;
            }
            catch (Exception ex)
            {
                handler(ex);
            }
        });
    }

    public Promise Then(Action action)
    {
        return new Promise(async () =>
        {
            await Task;
            action();
        });
    }

    public Promise<T> Then<T>(Func<T> func)
    {
        return new Promise<T>(async () =>
        {
            await Task;
            return func();
        });
    }

    /// <summary>
    /// Get the awaiter for direct awaiting of the promise
    /// </summary>
    public System.Runtime.CompilerServices.TaskAwaiter GetAwaiter()
    {
        return Task.GetAwaiter();
    }

    public void Dispose() => _inner?.Dispose();

    public static implicit operator Task(Promise promise) => promise.Task;
    public static implicit operator Promise(Task task) => new Promise(() => task);
}