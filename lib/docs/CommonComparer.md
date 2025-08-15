# CommonComparer — Rich comparisons for mixed types

A flexible comparer that works across common .NET types.

Exposed via `lib.Extensions.Extensions.CommonComparer` and interfaces `IRichComparer`/`IRichComparer<T>`.

- Compare objects, dictionaries, enumerables, and strings.
- Test for containment across these types.
- Cast to a typed comparer when needed.

## Description

CommonComparer provides consistent Compare/Equals/Contains logic when working with heterogeneous objects.

It knows how to:

- Compare IComparable values directly.
- Compare dictionaries by keys, counts, and values.
- Compare enumerables by count and set-difference.
- Compare strings and check substring containment.

## Usage

```csharp
using lib.Extensions; // Extensions.CommonComparer

var cmp = Extensions.CommonComparer; // IRichComparer

// Null ordering
cmp.Compare(null, 5); // 1 (null considered greater than non-null)
cmp.Compare(5, null); // -1

// Dictionaries
IDictionary x = new Dictionary<string, int> { {"a",1 }, {"b", 2} };
IDictionary y = new Dictionary<string, int> { {"a",1 }, {"b", 2} };
cmp.Compare(x, y); // -1 with current implementation for equal non-empty dictionaries

// Enumerable vs Enumerable (as sets)
IEnumerable a = new List<int>{1,2,3};
IEnumerable b = new List<int>{3,2,1};
cmp.Compare(a, b); // 0 (same set)

// Containment
cmp.Contains(x, new Dictionary<string,int>{{"a",1}}); // true
cmp.Contains(a, 2); // true
cmp.Contains("hello world", "ell"); // true

// Typed adapter
var intComparer = Extensions.CommonComparer.Cast<int>();
intComparer.Compare(10, 5); // > 0
```

## Reasoning why it's useful

- Real-world code compares collections, dictionaries, and simple values.
- Standard IComparer/IEqualityComparer do not provide unified semantics across these types.
- CommonComparer gives you one tool with sensible defaults for comparisons and containment checks.

## Examples

- Dictionary vs IEnumerable keys:
```csharp
IDictionary dict = new Dictionary<string,int>{{"a",1},{"b",2}};
IEnumerable keys = new List<string>{"a","b"};
Extensions.CommonComparer.Compare(dict, keys); // 0 or small difference (no missing keys)
Extensions.CommonComparer.Contains(dict, keys); // true
```

- Detect differences between two sets:
```csharp
IEnumerable left = new[]{1,2,3};
IEnumerable right = new[]{1,2};
Extensions.CommonComparer.Compare(left, right); // 1 (count difference)
```

Notes
- Namespace: lib.Helpers (implementation), lib.Services (interfaces)
- Entry point: lib.Extensions.Extensions.CommonComparer