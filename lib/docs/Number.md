# Number — One type to work with any numeric

A small, single-struct abstraction over all common .NET numeric primitives. Number wraps an internal IConvertible and gives you:

- Natural arithmetic with operator overloads (+, -, *, /, %, <<, >>)
- Comparisons and equality that work across numeric kinds
- Deterministic type promotion (decimal > double > float > integral …)
- Rich math: Abs, Round, Floor, Ceiling, Truncate, Pow, Sqrt, Log/Log10, Sin/Cos/Tan(h), Asin/Acos/Atan/Atan2
- Parsing and formatting helpers (C#-style suffixes and 0x hex)
- Implicit conversions to/from all built-in numeric types
- Utilities like IsZero() with sensible epsilon handling and a NaN sentinel

Short glance
- Purpose: unify arithmetic and comparisons across int, long, float, double, decimal, and unsigned variants, without constantly re‑casting.
- Core idea: wrap any numeric as Number; operations promote to the widest/most precise type involved.
- Best for: writing generic math-y code, working with mixed numeric inputs, or piping literal-like strings through TryParse/Parse.

Quick start
```csharp
using lib.Model;

Number a = 10;        // int
Number b = 20.5f;     // float

Number sum = a + b;   // promoted to float (Value.GetType() == typeof(float))
Console.WriteLine((float)sum); // 30.5

// Mixed comparisons
Console.WriteLine(a < b);      // True
Console.WriteLine(a.Same(10.0));   // True (numeric equality across types)
Console.WriteLine(a.Equals(10.0)); // False (different underlying types)

// Math
var hyp = Number.Sqrt(3*3 + 4*4); // 5 (double)

// Parsing
var n1 = Number.Parse("42U");     // uint
var n2 = Number.Parse("0xFF");    // hex -> int (255)
var n3 = Number.Parse("3.14M");   // decimal

// Formatting back to a source-like literal
Console.WriteLine(n3.AsParsableString()); // "3.14M"
```

Key features
- Constructors and implicit conversions
  - Construct with any of: byte, sbyte, short, ushort, int, uint, long, ulong, float, double, decimal.
  - Implicit conversions to/from all these primitives. Examples:
    - Number n = 42; int i = n; double d = n; decimal m = n;
- Operator overloads
  - Unary: +, -, ++, --
  - Binary: +, -, *, /, %, <<, >>
  - Relational: <, <=, >, >=, ==, != (==/!= use Number.Equals rules; see Equality below)
- Deterministic promotion
  - When operating on two Number values, the result type is chosen by:
    - decimal > double > float > ulong > long > uint > int > ushort > short > byte > sbyte
  - Examples:
    - int + decimal -> decimal
    - double + float -> double
    - long + int -> long
- Math helpers (return a Number)
  - Abs, Negate, Round(scale[, MidpointRounding]), Floor, Ceiling, Truncate
  - Pow, Sqrt, Log(base), Log10, Sin/Sinh, Cos/Cosh, Tan/Tanh, Asin, Acos, Atan, Atan2
- Parsing and formatting
  - TryParse(string, CultureInfo, out Number): accepts C#-style suffixes and hex (0x...)
    - Integer: 42, 42L, 42U, 42UL
    - Floating: 3.14F, 3.14D, 3.14M, or 3.14 (double by default)
    - Hex: 0xFF, 0x10, etc.
  - Parse(string): same as TryParse with InvariantCulture; returns Number.NaN on failure
  - AsParsableString(): returns a source-like literal with suffix (e.g., "42U", "3.14M") when possible
- Zero/NaN helpers
  - Number.Zero, Number.MinusOne, Number.NaN
  - IsZero(): true for 0 of any numeric type, including very small float/double magnitudes within epsilon
  - IsNaN(): true when underlying double representation is NaN (e.g., double.NaN, float.NaN)

Equality, comparison, and semantics
- Same(Number other)
  - Numeric equality after appropriate conversions. Cross-type 10 and 10.0 compare "Same".
- Equals(Number other) and == operator
  - Strict: requires same underlying type (e.g., int vs double are not Equal).
  - For relational operators (<, <=, >, >=), the operands are compared after promoting to the operation target type.
- HashCode
  - Based on the underlying value’s hash code.

Caveats and gotchas
- Overflow/underflow for integral math
  - When both operands resolve to an integral target type, operations use that integral arithmetic. Example: int.MaxValue + 1 results in int.MinValue (overflow), matching .NET’s unchecked default behavior.
  - Use AsFloatingPointNumber(n) or include a floating operand to promote and avoid integral overflow where needed.
- Integer division truncation
  - If the operation target type is integral, a/b truncates toward zero. Include a floating operand to get fractional results.
- IsZero epsilon behavior
  - IsZero() treats magnitudes smaller than float/double epsilon as zero for floats/doubles. Don’t use IsZero() as a strict equality for tiny non-zero measurements.
- Equals vs Same
  - Equals (and ==/!=) consider underlying types; Same compares numeric value across types. Use Same for value equality across types.
- NaN and division by zero
  - Floating-point division by zero yields Infinity (consistent with .NET). NaN propagates through numeric operations.
- Culture in TryParse
  - TryParse honors the provided CultureInfo. Parse(string) uses invariant culture semantics via internal TryParse. If parsing user input, pass CultureInfo accordingly.
- Bit shifts
  - Only defined for integral target types; shifting with floating operands is not supported.

API sketch (selected)
- Static helpers: IsIntegralNumber(object|Number), IsFloatingPointNumber(object|Number), AsFloatingPointNumber(Number)
- Binary ops: Add, Subtract, Multiply, Divide, Modulo, ShiftLeft, ShiftRight
- Comparisons: LessThen, GreaterThen, Same, Equals, Max, Min
- Math: Abs, Negate, Round, Floor, Ceiling, Truncate, Pow, Sqrt, Log, Log10, Sin, Cos, Tan(h), Asin, Acos, Atan, Atan2
- Parsing/formatting: TryCast(IConvertible), TryParse(string, CultureInfo, out Number), Parse(string), AsParsableString()

When to use Number
- You accept inputs from mixed numeric types but want uniform behavior without a forest of casts.
- You need consistent, explicit type promotion rules across operations.
- You want convenient parsing of numeric literals (including hex and suffixes) and easy round-tripping to parsable strings.

Examples in practice
- Prevent integral overflow by promoting explicitly:
```csharp
var count = new Number(int.MaxValue);
var one = new Number(1);
var safe = Number.AsFloatingPointNumber(count) + one; // double
Console.WriteLine((double)safe); // 2147483648
```

- Cross-type equality checks:
```csharp
Number i = 10; Number d = 10.0; 
Console.WriteLine(i.Same(d));    // True
Console.WriteLine(i.Equals(d));  // False (different underlying types)
```

- Parsing user input with culture:
```csharp
Number.TryParse("1.234,56", new CultureInfo("de-DE"), out var n); // decimal 1234,56 in DE
```

Notes
- Namespace: lib.Model
- Type: public readonly struct Number : IComparable[<T>], IEquatable[<T>], IConvertible, IFormattable
- Constants: Number.Zero, Number.MinusOne, Number.NaN
