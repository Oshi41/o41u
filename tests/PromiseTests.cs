using System;
using System.Threading.Tasks;
using lib.Extensions;
using lib.Model;

namespace tests;

public class PromiseTests
{
    [Test]
    public async Task Promise_BasicReturn_WorksCorrectly()
    {
        var promise = new Promise<int>(() => 42);
        var result = await promise;

        Assert.That(result, Is.EqualTo(42));
        Assert.That(promise.IsCompleted, Is.True);
    }

    [Test]
    public async Task Promise_BasicThrow_WorksCorrectly()
    {
        var promise = new Promise<int>(new Func<int>(() => throw new InvalidOperationException("Test error")));

        var ex = await AssertThrowsAsync<InvalidOperationException>(() => promise.Task);
        Assert.That(ex.Message, Is.EqualTo("Test error"));
    }

    [Test]
    public async Task Promise_ImmediateReturn_WorksCorrectly()
    {
        var promise = new Promise<string>();
        promise.Return("Hello World");

        var result = await promise;
        Assert.That(result, Is.EqualTo("Hello World"));
    }

    [Test]
    public async Task Promise_ImmediateThrow_WorksCorrectly()
    {
        var promise = new Promise<int>();
        promise.Throw(new ArgumentException("Test"));

        var ex = await AssertThrowsAsync<ArgumentException>(() => promise.Task);
        Assert.That(ex.Message, Is.EqualTo("Test"));
    }

    [Test]
    public async Task Promise_Then_ChainsCorrectly()
    {
        var promise = new Promise<int>(() => 10);
        var chained = promise.Then(x => x * 2);

        var result = await chained;
        Assert.That(result, Is.EqualTo(20));
    }

    [Test]
    public async Task Promise_Catch_HandlesExceptions()
    {
        var promise = new Promise<int>(new Func<int>(() => throw new InvalidOperationException("Error")));
        var caught = promise.Catch(ex => -1);

        var result = await caught;
        Assert.That(result, Is.EqualTo(-1));
    }

    [Test]
    public void Promise_Cancel_CancelsCorrectly()
    {
        var promise = new Promise<int>();
        promise.Cancel();

        Assert.That(promise.IsCancelled, Is.True);
        Assert.That(promise.IsCompleted, Is.True);
    }

    [Test]
    public async Task PromiseContext_Current_WorksCorrectly()
    {
        string result = null;

        var promise = PromiseContext.Run(() =>
        {
            result = "Started";
            PromiseContext.Return();
            result = "Should not reach here";
        });

        await promise;
        Assert.That(result, Is.EqualTo("Started"));
    }

    [Test]
    public async Task PromiseContext_ReturnWithValue_WorksCorrectly()
    {
        var promise = PromiseContext.Run(() =>
        {
            PromiseContext.Return(42);
            return 0; // Should not be reached
        });

        var result = await promise;
        Assert.That(result, Is.EqualTo(42));
    }

    [Test]
    public async Task PromiseContext_Throw_WorksCorrectly()
    {
        var promise = PromiseContext.Run(() =>
        {
            PromiseContext.Throw(new InvalidOperationException("Context throw"));
            return 42; // Should not be reached
        });

        var ex = await AssertThrowsAsync<InvalidOperationException>(() => promise.Task);
        Assert.That(ex.Message, Is.EqualTo("Context throw"));
    }

    [Test]
    public async Task Promise_WhenAll_WorksCorrectly()
    {
        var promise1 = new Promise<int>(() => 1);
        var promise2 = new Promise<int>(() => 2);
        var promise3 = new Promise<int>(() => 3);

        var all = Extensions.WhenAll(promise1, promise2, promise3);
        var results = await all;

        Assert.That(results, Is.EquivalentTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public async Task Promise_Parallel_WorksWithConcurrency()
    {
        var counter = 0;
        var factories = Enumerable.Range(0, 10)
            .Select(i => new Func<Promise<int>>(() => new Promise<int>(() =>
            {
                Interlocked.Increment(ref counter);
                Task.Delay(100).Wait(); // Simulate work
                return i;
            })));

        var parallel = factories.Parallel(maxConcurrency: 3);
        var results = await parallel;

        Assert.That(results.Length, Is.EqualTo(10));
        Assert.That(results.Sum(), Is.EqualTo(45)); // 0+1+2+...+9 = 45
    }

    [Test]
    public async Task Promise_Timeout_ThrowsOnTimeout()
    {
        var promise = new Promise<int>(async () =>
        {
            await Task.Delay(1000); // Long operation
            return 42;
        });

        var timeoutPromise = promise.Timeout(TimeSpan.FromMilliseconds(100));

        var ex = await AssertThrowsAsync<TimeoutException>(() => timeoutPromise.Task);
        Assert.That(ex.Message, Contains.Substring("timed out"));
    }

    [Test]
    public async Task Promise_Retry_RetriesOnFailure()
    {
        var attempts = 0;
        var factory = new Func<Promise<int>>(() => new Promise<int>(() =>
        {
            attempts++;
            if (attempts < 3)
                throw new InvalidOperationException("Retry me");
            return 42;
        }));

        var retryPromise = factory.Retry(maxAttempts: 3, delay: TimeSpan.FromMilliseconds(10));
        var result = await retryPromise;

        Assert.That(result, Is.EqualTo(42));
        Assert.That(attempts, Is.EqualTo(3));
    }

    [Test]
    public async Task Promise_AddChild_WaitsForChildren()
    {
        var parent = new Promise<int>();
        var child = new Promise();

        parent.AddChild(child);

        var completed = false;
        var parentTask = parent.Task.ContinueWith(_ => completed = true);

        // Parent should not complete until child completes
        await Task.Delay(50);
        Assert.That(completed, Is.False);

        parent.Return(42);
        await Task.Delay(50);
        Assert.That(completed, Is.False);

        child.Return();
        await parentTask;
        Assert.That(completed, Is.True);
    }

    private static async Task<TException> AssertThrowsAsync<TException>(Func<Task> action) 
        where TException : Exception
    {
        try
        {
            await action();
            Assert.Fail($"Expected {typeof(TException).Name} to be thrown");
            return null;
        }
        catch (TException ex)
        {
            return ex;
        }
        catch (Exception ex)
        {
            Assert.Fail($"Expected {typeof(TException).Name}, but got {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }
}
