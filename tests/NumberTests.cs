using System;
using System.Globalization;
using lib.Model;

namespace tests;

public class NumberTests
{
    [Test]
    public void Constructors_CreateCorrectNumbers()
    {
        Assert.That(new Number(42).ToInt32(null), Is.EqualTo(42));
        Assert.That(new Number(42L).ToInt64(null), Is.EqualTo(42L));
        Assert.That(new Number(42u).ToUInt32(null), Is.EqualTo(42u));
        Assert.That(new Number(42ul).ToUInt64(null), Is.EqualTo(42ul));
        Assert.That(new Number(42.5f).ToSingle(null), Is.EqualTo(42.5f));
        Assert.That(new Number(42.5d).ToDouble(null), Is.EqualTo(42.5d));
        Assert.That(new Number(42.5m).ToDecimal(null), Is.EqualTo(42.5m));
        Assert.That(new Number((byte)42).ToByte(null), Is.EqualTo((byte)42));
        Assert.That(new Number((sbyte)42).ToSByte(null), Is.EqualTo((sbyte)42));
        Assert.That(new Number((short)42).ToInt16(null), Is.EqualTo((short)42));
        Assert.That(new Number((ushort)42).ToUInt16(null), Is.EqualTo((ushort)42));
    }

    [Test]
    public void Constructor_WithNumberParameter_UnwrapsValue()
    {
        var original = new Number(42);
        var wrapped = new Number((int)original);

        Assert.That(wrapped.ToInt32(null), Is.EqualTo(42));
        Assert.That(wrapped.Value.GetType(), Is.EqualTo(typeof(int)));
    }

    [Test]
    public void Constants_HaveCorrectValues()
    {
        Assert.That(Number.Zero.ToInt32(null), Is.EqualTo(0));
        Assert.That(Number.MinusOne.ToInt32(null), Is.EqualTo(-1));
        Assert.That(Number.NaN.IsNaN(), Is.True);
    }

    [Test]
    public void IsNaN_DetectsNaNValues()
    {
        Assert.That(Number.NaN.IsNaN(), Is.True);
        Assert.That(new Number(double.NaN).IsNaN(), Is.True);
        Assert.That(new Number(float.NaN).IsNaN(), Is.True);
        Assert.That(new Number(42).IsNaN(), Is.False);
        Assert.That(new Number(42.5).IsNaN(), Is.False);
    }

    [Test]
    public void IsFloatingPointNumber_DetectsFloatingTypes()
    {
        Assert.That(Number.IsFloatingPointNumber(42.5f), Is.True);
        Assert.That(Number.IsFloatingPointNumber(42.5d), Is.True);
        Assert.That(Number.IsFloatingPointNumber(42.5m), Is.True);
        Assert.That(Number.IsFloatingPointNumber(42), Is.False);
        Assert.That(Number.IsFloatingPointNumber(42L), Is.False);

        Assert.That(Number.IsFloatingPointNumber(new Number(42.5f)), Is.True);
        Assert.That(Number.IsFloatingPointNumber(new Number(42)), Is.False);
    }

    [Test]
    public void IsIntegralNumber_DetectsIntegralTypes()
    {
        Assert.That(Number.IsIntegralNumber(42), Is.True);
        Assert.That(Number.IsIntegralNumber(42L), Is.True);
        Assert.That(Number.IsIntegralNumber((byte)42), Is.True);
        Assert.That(Number.IsIntegralNumber((short)42), Is.True);
        Assert.That(Number.IsIntegralNumber(42.5f), Is.False);
        Assert.That(Number.IsIntegralNumber(42.5d), Is.False);

        Assert.That(Number.IsIntegralNumber(new Number(42)), Is.True);
        Assert.That(Number.IsIntegralNumber(new Number(42.5)), Is.False);
    }

    [Test]
    public void Addition_WorksWithSameTypes()
    {
        var a = new Number(10);
        var b = new Number(20);
        var result = a.Add(b);

        Assert.That(result.ToInt32(null), Is.EqualTo(30));
    }

    [Test]
    public void Addition_WorksWithDifferentTypes()
    {
        var a = new Number(10);
        var b = new Number(20.5f);
        var result = a.Add(b);

        Assert.That(result.ToSingle(null), Is.EqualTo(30.5f));
    }

    [Test]
    public void Addition_HandlesOverflow()
    {
        var a = new Number(int.MaxValue);
        var b = new Number(1);
        var result = a.Add(b);

        // Should overflow to negative value
        Assert.That(result.ToInt32(null), Is.EqualTo(int.MinValue));
    }

    [Test]
    public void Subtraction_PreservesTypes()
    {
        var a = new Number(20);
        var b = new Number(10);
        var result = a.Subtract(b);

        Assert.That(result.ToInt32(null), Is.EqualTo(10));
    }

    [Test]
    public void Multiplication_PreservesFractions()
    {
        var a = new Number(2.5m);
        var b = new Number(4.2m);
        var result = a.Multiply(b);

        Assert.That(result.ToDecimal(null), Is.EqualTo(10.5m));
    }

    [Test]
    public void Division_PreservesPrecision()
    {
        var a = new Number(10.0);
        var b = new Number(3.0);
        var result = a.Divide(b);

        Assert.That(Math.Abs(result.ToDouble(null) - 3.333333333333333), Is.LessThan(0.000000000000001));
    }

    [Test]
    public void Division_ByZero_ReturnsInfinity()
    {
        var a = new Number(10.0);
        var b = new Number(0.0);
        var result = a.Divide(b);

        Assert.That(double.IsInfinity(result.ToDouble(null)), Is.True);
    }

    [Test]
    public void Modulo_WorksCorrectly()
    {
        var a = new Number(17);
        var b = new Number(5);
        var result = a.Modulo(b);

        Assert.That(result.ToInt32(null), Is.EqualTo(2));
    }

    [Test]
    public void ShiftOperations_WorkWithIntegers()
    {
        var a = new Number(8);
        var b = new Number(2);

        var leftShift = a.ShiftLeft(b);
        var rightShift = a.ShiftRight(b);

        Assert.That(leftShift.ToInt32(null), Is.EqualTo(32));
        Assert.That(rightShift.ToInt32(null), Is.EqualTo(2));
    }

    [Test]
    public void Comparison_WorksWithSameTypes()
    {
        var a = new Number(10);
        var b = new Number(20);
        var c = new Number(10);

        Assert.That(a.LessThen(b), Is.True);
        Assert.That(b.GreaterThen(a), Is.True);
        Assert.That(a.Same(c), Is.True);
        Assert.That(a.Equals(c), Is.True);
    }

    [Test]
    public void Comparison_WorksWithDifferentTypes()
    {
        var a = new Number(10);
        var b = new Number(10.0);

        Assert.That(a.Same(b), Is.True);
        Assert.That(a.Equals(b), Is.False); // Different types
    }

    [Test]
    public void MathematicalFunctions_WorkCorrectly()
    {
        var num = new Number(16.0);

        Assert.That(num.Sqrt().ToDouble(null), Is.EqualTo(4.0));
        Assert.That(num.Log(new Number(2.0)).ToDouble(null), Is.EqualTo(4.0).Within(0.0001));
        Assert.That(new Number(2.0).Pow(new Number(3.0)).ToDouble(null), Is.EqualTo(8.0));
    }

    [Test]
    public void TrigonometricFunctions_WorkCorrectly()
    {
        var piOver2 = new Number(Math.PI / 2);
        var zero = new Number(0.0);

        Assert.That(zero.Sin().ToDouble(null), Is.EqualTo(0.0).Within(0.0001));
        Assert.That(piOver2.Sin().ToDouble(null), Is.EqualTo(1.0).Within(0.0001));
        Assert.That(zero.Cos().ToDouble(null), Is.EqualTo(1.0).Within(0.0001));
    }

    [Test]
    public void Rounding_WorksWithDecimals()
    {
        var num = new Number(3.14159m);
        var rounded = num.Round(new Number(2));

        Assert.That(rounded.ToDecimal(null), Is.EqualTo(3.14m));
    }

    [Test]
    public void FloorCeilingTruncate_WorkCorrectly()
    {
        var positive = new Number(3.7);
        var negative = new Number(-3.7);

        Assert.That(positive.Floor().ToDouble(null), Is.EqualTo(3.0));
        Assert.That(positive.Ceiling().ToDouble(null), Is.EqualTo(4.0));
        Assert.That(positive.Truncate().ToDouble(null), Is.EqualTo(3.0));

        Assert.That(negative.Floor().ToDouble(null), Is.EqualTo(-4.0));
        Assert.That(negative.Ceiling().ToDouble(null), Is.EqualTo(-3.0));
        Assert.That(negative.Truncate().ToDouble(null), Is.EqualTo(-3.0));
    }

    [Test]
    public void AbsoluteValue_WorksWithDifferentTypes()
    {
        Assert.That(new Number(-42).Abs().ToInt32(null), Is.EqualTo(42));
        Assert.That(new Number(-42.5f).Abs().ToSingle(null), Is.EqualTo(42.5f));
        Assert.That(new Number(-42.5).Abs().ToDouble(null), Is.EqualTo(42.5));
        Assert.That(new Number(-42.5m).Abs().ToDecimal(null), Is.EqualTo(42.5m));
    }

    [Test]
    public void Negate_ChangesSign()
    {
        Assert.That(new Number(42).Negate().ToInt32(null), Is.EqualTo(-42));
        Assert.That(new Number(-42).Negate().ToInt32(null), Is.EqualTo(42));
        Assert.That(new Number(42.5).Negate().ToDouble(null), Is.EqualTo(-42.5));
    }

    [Test]
    public void MinMax_WorksCorrectly()
    {
        var a = new Number(10);
        var b = new Number(20);

        Assert.That(a.Max(b).ToInt32(null), Is.EqualTo(20));
        Assert.That(a.Min(b).ToInt32(null), Is.EqualTo(10));
    }

    [Test]
    public void Parse_HandlesIntegerLiterals()
    {
        Assert.That(Number.Parse("42").ToInt32(null), Is.EqualTo(42));
        Assert.That(Number.Parse("-42").ToInt32(null), Is.EqualTo(-42));
        Assert.That(Number.Parse("42L").ToInt64(null), Is.EqualTo(42L));
        Assert.That(Number.Parse("42U").ToUInt32(null), Is.EqualTo(42U));
        Assert.That(Number.Parse("42UL").ToUInt64(null), Is.EqualTo(42UL));
    }

    [Test]
    public void Parse_HandlesFloatingPointLiterals()
    {
        Assert.That(Number.Parse("42.5F").ToSingle(null), Is.EqualTo(42.5f));
        Assert.That(Number.Parse("42.5D").ToDouble(null), Is.EqualTo(42.5d));
        Assert.That(Number.Parse("42.5M").ToDecimal(null), Is.EqualTo(42.5m));
        Assert.That(Number.Parse("42.5").ToDouble(null), Is.EqualTo(42.5d));
    }

    [Test]
    public void Parse_HandlesHexadecimalLiterals()
    {
        Assert.That(Number.Parse("0x10").ToInt32(null), Is.EqualTo(16));
        Assert.That(Number.Parse("0xFF").ToInt32(null), Is.EqualTo(255));
        Assert.That(Number.Parse("0xA").ToInt32(null), Is.EqualTo(10));
    }

    [Test]
    public void Parse_HandlesInvalidInput()
    {
        Assert.That(Number.Parse("invalid").IsNaN(), Is.True);
        Assert.That(Number.Parse("").IsNaN(), Is.True);
        Assert.That(Number.Parse("12.34.56").IsNaN(), Is.True);
    }

    [Test]
    public void TryParse_ReturnsCorrectResults()
    {
        Assert.That(Number.TryParse("42", CultureInfo.InvariantCulture, out var result), Is.True);
        Assert.That(result.ToInt32(null), Is.EqualTo(42));

        Assert.That(Number.TryParse("invalid", CultureInfo.InvariantCulture, out var invalidResult), Is.False);
        Assert.That(invalidResult.IsNaN(), Is.True);
    }

    [Test]
    public void AsParsableString_GeneratesCorrectLiterals()
    {
        Assert.That(new Number(42).AsParsableString(), Is.EqualTo("42"));
        Assert.That(new Number(42L).AsParsableString(), Is.EqualTo("42L"));
        Assert.That(new Number(42U).AsParsableString(), Is.EqualTo("42U"));
        Assert.That(new Number(42.5f).AsParsableString(), Is.EqualTo("42.5F"));
        Assert.That(new Number(42.5).AsParsableString(), Is.EqualTo("42.5D"));
        Assert.That(new Number(42.5m).AsParsableString(), Is.EqualTo("42.5M"));
    }

    [Test]
    public void OperatorOverloads_WorkCorrectly()
    {
        Number a = 10;
        Number b = 20;

        Assert.That((a + b).ToInt32(null), Is.EqualTo(30));
        Assert.That((b - a).ToInt32(null), Is.EqualTo(10));
        Assert.That((a * 3).ToInt32(null), Is.EqualTo(30));
        Assert.That((b / 2).ToInt32(null), Is.EqualTo(10));
        Assert.That((b % 3).ToInt32(null), Is.EqualTo(2));

        Assert.That(a < b, Is.True);
        Assert.That(b > a, Is.True);
        Assert.That(a <= a, Is.True);
        Assert.That(b >= b, Is.True);
        Assert.That(a == new Number(10), Is.True);
        Assert.That(a != b, Is.True);
    }

    [Test]
    public void ImplicitConversions_WorkBothWays()
    {
        Number num = 42;
        int intVal = num;

        Assert.That(intVal, Is.EqualTo(42));

        Number floatNum = 42.5f;
        float floatVal = floatNum;

        Assert.That(floatVal, Is.EqualTo(42.5f));
    }

    [Test]
    public void GetOperationTargetType_ChoosesCorrectType()
    {
        // Test decimal takes precedence
        var a = new Number(10);
        var b = new Number(5.5m);
        var result = a.Add(b);
        Assert.That(result.Value.GetType(), Is.EqualTo(typeof(decimal)));

        // Test double takes precedence over float
        var c = new Number(10.0);
        var d = new Number(5.5f);
        var result2 = c.Add(d);
        Assert.That(result2.Value.GetType(), Is.EqualTo(typeof(double)));

        // Test long takes precedence over int
        var e = new Number(10);
        var f = new Number(5L);
        var result3 = e.Add(f);
        Assert.That(result3.Value.GetType(), Is.EqualTo(typeof(long)));
    }

    [Test]
    public void AsFloatingPointNumber_ConvertsIntegralsCorrectly()
    {
        var intNum = new Number(42);
        var floatResult = Number.AsFloatingPointNumber(intNum);

        Assert.That(Number.IsFloatingPointNumber(floatResult), Is.True);
        Assert.That(floatResult.ToDouble(null), Is.EqualTo(42.0));

        // Already floating point should return same
        var doubleNum = new Number(42.5);
        var sameResult = Number.AsFloatingPointNumber(doubleNum);

        Assert.That(sameResult.ToDouble(null), Is.EqualTo(42.5));
    }

    [Test]
    public void TryCast_WorksWithIConvertibleTypes()
    {
        Assert.That(Number.TryCast(42, out var result), Is.True);
        Assert.That(result.HasValue, Is.True);
        Assert.That(result.Value.ToInt32(null), Is.EqualTo(42));

        Assert.That(Number.TryCast(42.5, out var floatResult), Is.True);
        Assert.That(floatResult.HasValue, Is.True);
        Assert.That(floatResult.Value.ToDouble(null), Is.EqualTo(42.5));
    }

    [Test]
    public void HashCode_IsConsistent()
    {
        var a = new Number(42);
        var b = new Number(42);
        var c = new Number(43);

        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        Assert.That(a.GetHashCode(), Is.Not.EqualTo(c.GetHashCode()));
    }

    [Test]
    public void IsZero_DetectsZeroValues()
    {
        // Integer zeros
        Assert.That(new Number(0).IsZero(), Is.True);
        Assert.That(new Number(0L).IsZero(), Is.True);
        Assert.That(new Number(0u).IsZero(), Is.True);
        Assert.That(new Number(0ul).IsZero(), Is.True);
        Assert.That(new Number((short)0).IsZero(), Is.True);
        Assert.That(new Number((ushort)0).IsZero(), Is.True);
        Assert.That(new Number((byte)0).IsZero(), Is.True);
        Assert.That(new Number((sbyte)0).IsZero(), Is.True);

        // Floating point zeros
        Assert.That(new Number(0.0f).IsZero(), Is.True);
        Assert.That(new Number(0.0d).IsZero(), Is.True);
        Assert.That(new Number(0.0m).IsZero(), Is.True);

        // Negative zeros
        Assert.That(new Number(-0.0f).IsZero(), Is.True);
        Assert.That(new Number(-0.0d).IsZero(), Is.True);

        // Very small numbers (should be considered zero due to epsilon)
        Assert.That(new Number(float.Epsilon / 2).IsZero(), Is.True);
        Assert.That(new Number(double.Epsilon / 2).IsZero(), Is.True);
    }

    [Test]
    public void IsZero_DetectsNonZeroValues()
    {
        // Non-zero integers
        Assert.That(new Number(1).IsZero(), Is.False);
        Assert.That(new Number(-1).IsZero(), Is.False);
        Assert.That(new Number(42L).IsZero(), Is.False);

        // Non-zero floating point
        Assert.That(new Number(0.1f).IsZero(), Is.False);
        Assert.That(new Number(0.1d).IsZero(), Is.False);
        Assert.That(new Number(0.1m).IsZero(), Is.False);

        // NaN should not be zero
        Assert.That(Number.NaN.IsZero(), Is.False);
        Assert.That(new Number(float.NaN).IsZero(), Is.False);
        Assert.That(new Number(double.NaN).IsZero(), Is.False);

        // Infinity should not be zero
        Assert.That(new Number(float.PositiveInfinity).IsZero(), Is.False);
        Assert.That(new Number(double.NegativeInfinity).IsZero(), Is.False);
    }

    [Test]
    public void IsZero_StaticMethod_WorksCorrectly()
    {
        Assert.That(Number.IsZero(Number.Zero), Is.True);
        Assert.That(Number.IsZero(new Number(0.0)), Is.True);
        Assert.That(Number.IsZero(new Number(42)), Is.False);
        Assert.That(Number.IsZero(Number.NaN), Is.False);
    }

    [Test]
    public void IsZero_WithArithmeticResults()
    {
        // Result of 5 - 5 should be zero
        var result1 = new Number(5).Subtract(new Number(5));
        Assert.That(result1.IsZero(), Is.True);

        // Result of 0 * any number should be zero
        var result2 = Number.Zero.Multiply(new Number(999));
        Assert.That(result2.IsZero(), Is.True);

        // Result of any number - itself should be zero
        var result3 = new Number(3.14159).Subtract(new Number(3.14159));
        Assert.That(result3.IsZero(), Is.True);
    }

    [Test]
    public void Zero_Constant_IsActuallyZero()
    {
        Assert.That(Number.Zero.IsZero(), Is.True);
        Assert.That(Number.Zero.ToInt32(null), Is.EqualTo(0));
        Assert.That(Number.Zero.ToDouble(null), Is.EqualTo(0.0));
    }
}
