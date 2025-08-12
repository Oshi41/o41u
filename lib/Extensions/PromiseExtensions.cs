using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using lib.Model;

namespace lib.Extensions;

/// <summary>
/// Extensions for Promise to support parallel execution and utilities.
/// </summary>
public static partial class Extensions
{
    /// <summary>
    /// Runs promises in parallel with the specified maximum degree of concurrency.
    /// </summary>
    /// <typeparam name="T">The result type of the promises.</typeparam>
    /// <param name="factories">A sequence of factories that produce disposable <see cref="Promise{T}"/> instances.</param>
    /// <param name="maxConcurrency">The maximum number of promises to run concurrently.</param>
    /// <returns>A promise that resolves to an array of results preserving the original order.</returns>
    public static Promise<T[]> Parallel<T>(this IEnumerable<Func<Promise<T>>> factories, int maxConcurrency = 4)
    {
        var tasks = factories.ToArray();
        return new Promise<T[]>(async () =>
        {
            var results = new T[tasks.Length];
            var semaphore = new System.Threading.SemaphoreSlim(maxConcurrency);

            var parallelTasks = tasks.Select(async (factory, index) =>
            {
                await semaphore.WaitAsync();
                try
                {
                    using var promise = factory();
                    results[index] = await promise;
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(parallelTasks);
            return results;
        });
    }

    /// <summary>
    /// Runs actions in parallel with the specified maximum degree of concurrency.
    /// </summary>
    /// <param name="factories">A sequence of factories that produce disposable <see cref="Promise"/> instances.</param>
    /// <param name="maxConcurrency">The maximum number of actions to run concurrently.</param>
    /// <returns>A promise that resolves when all actions complete.</returns>
    public static Promise Parallel(this IEnumerable<Func<Promise>> factories, int maxConcurrency = int.MaxValue)
    {
        var tasks = factories.ToArray();
        return new Promise(async () =>
        {
            var semaphore = new System.Threading.SemaphoreSlim(maxConcurrency);

            var parallelTasks = tasks.Select(async factory =>
            {
                await semaphore.WaitAsync();
                try
                {
                    using var promise = factory();
                    await promise;
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(parallelTasks);
        });
    }

    /// <summary>
    /// Waits for all promises to complete and aggregates their results.
    /// </summary>
    /// <typeparam name="T">The result type of the promises.</typeparam>
    /// <param name="promises">The promises to wait for.</param>
    /// <returns>A promise that resolves to an array of results.</returns>
    public static Promise<T[]> WhenAll<T>(params Promise<T>[] promises)
    {
        return new Promise<T[]>(async () =>
        {
            var tasks = promises.Select(p => p.Task).ToArray();
            return await Task.WhenAll(tasks);
        });
    }

    /// <summary>
    /// Waits for all promises to complete.
    /// </summary>
    /// <param name="promises">The promises to wait for.</param>
    /// <returns>A promise that resolves when all input promises complete.</returns>
    public static Promise WhenAll(params Promise[] promises)
    {
        return new Promise(async () =>
        {
            var tasks = promises.Select(p => p.Task).ToArray();
            await Task.WhenAll(tasks);
        });
    }

    /// <summary>
    /// Waits for any of the provided promises to complete and resolves with its result.
    /// </summary>
    /// <typeparam name="T">The result type of the promises.</typeparam>
    /// <param name="promises">The promises to race.</param>
    /// <returns>A promise that resolves with the first completed result.</returns>
    public static Promise<T> WhenAny<T>(params Promise<T>[] promises)
    {
        return new Promise<T>(async () =>
        {
            var tasks = promises.Select(p => p.Task).ToArray();
            var completedTask = await Task.WhenAny(tasks);
            return await completedTask;
        });
    }

    /// <summary>
    /// Adds a timeout to a promise.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="promise">The promise to wrap with a timeout.</param>
    /// <param name="timeout">The maximum time to wait before timing out.</param>
    /// <returns>A promise that resolves with the original result or throws upon timeout.</returns>
    /// <exception cref="TimeoutException">Thrown when the operation does not complete within the specified timeout.</exception>
    public static Promise<T> Timeout<T>(this Promise<T> promise, TimeSpan timeout)
    {
        return new Promise<T>(async () =>
        {
            var timeoutTask = Task.Delay(timeout);
            var completedTask = await Task.WhenAny(promise.Task, timeoutTask);

            if (completedTask == timeoutTask)
                throw new TimeoutException($"Promise timed out after {timeout}");

            return await promise.Task;
        });
    }

    /// <summary>
    /// Retries a promise-producing operation with exponential backoff.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="factory">A factory that produces a disposable <see cref="Promise{T}"/> on each attempt.</param>
    /// <param name="maxAttempts">The maximum number of attempts before failing.</param>
    /// <param name="delay">The initial delay between attempts. If null, defaults to 100 ms. Each retry doubles the delay.</param>
    /// <returns>A promise that resolves with the first successful result.</returns>
    /// <exception cref="InvalidOperationException">Thrown if all attempts fail and no last exception is available.</exception>
    public static Promise<T> Retry<T>(this Func<Promise<T>> factory, int maxAttempts = 3, TimeSpan? delay = null)
    {
        return new Promise<T>(async () =>
        {
            var currentDelay = delay ?? TimeSpan.FromMilliseconds(100);
            Exception lastException = null;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    using var promise = factory();
                    return await promise;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    if (attempt == maxAttempts) break;

                    await Task.Delay(currentDelay);
                    currentDelay = TimeSpan.FromMilliseconds(currentDelay.TotalMilliseconds * 2); // Exponential backoff
                }
            }

            throw lastException ?? new InvalidOperationException("Retry failed");
        });
    }
}