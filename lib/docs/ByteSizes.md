# ByteSizes

An immutable type representing a data size with convenient factory methods, arithmetic and comparison operators, and pretty printing.

- Base: binary (powers of 1024)
- Storage: internally stored as total bytes
- Output: formatted as a compact human-friendly string (only non-zero parts)

## Units (binary)

| Unit | Relation |
|------|----------|
| KB   | 1 KB = 1024 B |
| MB   | 1 MB = 1024 KB |
| GB   | 1 GB = 1024 MB |
| TB   | 1 TB = 1024 GB |
| PB   | 1 PB = 1024 TB |
| EB   | 1 EB = 1024 PB |

## Quick start

```csharp
using lib.Model;

var a = ByteSizes.FromMb(1.5); // 1 MB and 512 KB
var b = ByteSizes.FromKb(256);  // 256 KB

var sum = a + b;                // 1 MB, 768 KB
var bigger = 2 * sum;           // scaling

Console.WriteLine(sum);         // "1 MB, 768 KB"
Console.WriteLine(bigger > sum);// True
```

## Creating sizes

```csharp
ByteSizes s1 = ByteSizes.FromB(100);
ByteSizes s2 = ByteSizes.FromKb(2);
ByteSizes s3 = ByteSizes.FromMb(3);
ByteSizes s4 = ByteSizes.FromGb(4);
ByteSizes s5 = ByteSizes.FromTb(5);
ByteSizes s6 = ByteSizes.FromPb(6);
ByteSizes s7 = ByteSizes.FromEb(7);
```

## Arithmetic and comparisons

```csharp
var a = ByteSizes.FromKb(2);
var b = ByteSizes.FromB(100);

var add = a + b; // add.TotalBytes == 2*1024 + 100
var sub = a - b;
var mul = a * 1.5; // or 1.5 * a

bool gt = a > b;
bool lt = a < b;
bool eq = a == b;
```

## Components and totals

Every instance provides both the decomposed components and the totals:

- Components: Eb, Pb, Tb, Gb, Mb, Kb, Bytes (each in range 0..1023)
- Totals: TotalEb, TotalPb, TotalTb, TotalGb, TotalMb, TotalKb, TotalBytes

```csharp
var size = ByteSizes.FromMb(1.5); // 1 MB, 512 KB
Console.WriteLine(size.Mb);       // 1
Console.WriteLine(size.Kb);       // 512
Console.WriteLine(size.TotalKb);  // 1536
Console.WriteLine((ulong)size);   // implicit cast => TotalBytes
```

## Formatting

ToString() returns a concise, pretty string consisting of non-zero parts separated by commas:

- Example: "1 GB, 200 MB, 10 KB"
- Zero is printed as an empty string (no units displayed)

```csharp
var s = ByteSizes.FromGb(1) + ByteSizes.FromMb(200) + ByteSizes.FromKb(10);
Console.WriteLine(s.ToString()); // "1 GB, 200 MB, 10 KB"
```
