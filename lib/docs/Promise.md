# Promise — Controlled async flow with context

Minimal promise-like API inspired by etask, built on top of Task.

- Promise<T> and Promise for void
- Chaining: Then, ThenAsync, Catch
- Control: Return, Throw, Cancel, AddChild
- Context: PromiseContext.Current with helpers Return/Throw/Cancel/AddChild
- Utilities: WhenAll/Any, Parallel, Timeout, Retry (via extensions)

## Description

Promise wraps Task and exposes a small, composable API. You can immediately Return/Throw, cancel the whole chain, and attach child promises that must complete.

PromiseContext provides a thread-local Current promise to interact with the active promise without passing it around.

Extensions add high-level utilities for parallelism, timeouts, and retry with backoff.

## Usage

```csharp
using lib.Model;
using lib.Extensions; // WhenAll, Parallel, Timeout, Retry

// 1) Basic creation and chaining
var p = new Promise<int>(async () => {
  await Task.Delay(10);
  return 42;
});

var chained = p.Then(x => x + 1)          // 43
               .ThenAsync(async x => { await Task.Delay(1); return x * 2; }); // 86

int result = await chained;

// 2) Catch exceptions
var handled = new Promise<int>(() => throw new InvalidOperationException())
  .Catch(ex => 0); // returns 0 on error

// 3) Cancel
var cancellable = new Promise<int>(async () => {
  await Task.Delay(1000);
  return 1;
});
cancellable.Cancel(); // cancels task and marks completed

// 4) Context: early return / throw from inside work
var viaContext = PromiseContext.Run(() => {
  // do something and return early
  PromiseContext.Return(); // completes the current promise
});
await viaContext;

// 5) Parallel and WhenAll
var all = Extensions.WhenAll(
  new Promise<int>(async () => { await Task.Delay(5); return 1; }),
  new Promise<int>(async () => { await Task.Delay(5); return 2; })
);
int[] values = await all; // [1, 2]

// Limit concurrency
var results = new Func<Promise<int>>[] {
  () => new Promise<int>(async () => { await Task.Delay(10); return 1; }),
  () => new Promise<int>(async () => { await Task.Delay(10); return 2; }),
}.Parallel(maxConcurrency: 1);

// 6) Timeout and Retry
var withTimeout = new Promise<int>(async () => { await Task.Delay(1000); return 7; })
  .Timeout(TimeSpan.FromMilliseconds(10)); // throws TimeoutException

var retried = new Func<Promise<int>>(() => new Promise<int>(() => throw new Exception()))
  .Retry(maxAttempts: 3);
```

## Reasoning why it's useful

- Early Return/Throw is simpler than plumbing cancellation tokens and exceptions through many layers.
- PromiseContext enables concise APIs where the context is implicit during execution.
- Extensions cover common coordination needs (parallel, whenAll, timeouts, retries) without verbose Task code.

## Examples

- Add child work that must finish:
```csharp
var parent = new Promise(async () => {
  var child = new Promise(async () => { await Task.Delay(5); });
  PromiseContext.AddChild(child);
});
await parent; // waits for child completion internally
```

- Use WhenAny to take the first result:
```csharp
var a = new Promise<int>(async () => { await Task.Delay(50); return 1; });
var b = new Promise<int>(async () => { await Task.Delay(10); return 2; });
var first = Extensions.WhenAny(a, b);
int v = await first; // 2
```

Notes
- Namespaces: lib.Model (Promise, Promise<T>, PromiseContext), lib.Extensions (extensions)
- Features in tests: chaining, catch, cancel, context current, whenAll, parallel, timeout, retry, add child