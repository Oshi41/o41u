# Guard — Fluent checks with exceptions on failure

Quick, readable validations with a small API and rich results.

- Static helpers: Guard.IsEmpty, IsNotEmpty, Null, IsZero, IsPositive, IsNegative, HasFraction, Same, Less, More, Contains
- Rich result type: GuardResult with implicit conversions and CheckAndThrow()
- Works with strings, collections, files/directories, numbers (via Number), and general objects

## Description

Guard provides simple, fluent-style validations that return a GuardResult. Use CheckAndThrow() to raise an exception with a sensible message when a check fails, or use the boolean conversion to branch without throwing.

Supported cases include:
- Null and emptiness for strings, collections, FileInfo/DirectoryInfo
- Numbers: zero, positive, negative, fractional part (via lib.Model.Number)
- Comparisons and containment using CommonComparer semantics

## Usage

```csharp
using lib.Helpers;
using lib.Model;

Guard.Null(null).Ok; // true
Guard.IsEmpty("").Ok; // true
Guard.IsNotEmpty("abc").Ok; // true

Guard.Contains("hello", "ell").Ok; // true
Guard.Same("a", "a").Ok; // true
Guard.Less(1, 2).CheckAndThrow(); // passes

// Numbers (via Number)
Number n = 0;
Guard.IsZero(n).Ok; // true
Guard.IsPositive(new Number(3)).Ok; // true
Guard.HasFraction(new Number(1.5M)).Ok; // true

// Use GuardResult as bool
GuardResult ok = true; // implicit
if (ok) { /* ... */ }

// Throwing with default message
try { Guard.Same("x", "y").CheckAndThrow(); }
catch (Exception ex) { /* "Values must be equal" */ }
```

## Reasoning why it's useful

- Reduces boilerplate around common checks.
- Centralizes messages and semantics; consistent across the codebase.
- Fluent API with implicit conversions makes validations concise and readable.

## Examples

- Files and directories:
```csharp
var fi = new FileInfo("path/to/file.txt");
Guard.IsEmpty(fi).Ok; // true if file is missing or has empty content
var di = new DirectoryInfo("path/to/folder");
Guard.IsEmpty(di).Ok; // true if directory is missing or has no files
```

- Comparisons with automatic semantics via CommonComparer:
```csharp
Guard.Less("a", "b").Ok;  // true
Guard.More(3, 2).Ok;        // true
Guard.Contains(new[]{1,2,3}, 2).Ok; // true
```

Notes
- Namespace: lib.Helpers
- Types: Guard, GuardResult
- Related: lib.Extensions.Extensions.CommonComparer