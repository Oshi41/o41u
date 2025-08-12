using System;
using System.Globalization;

namespace lib.Model;

public readonly struct Number :
    IComparable,
    IComparable<Number>,
    IComparable<ulong>,
    IComparable<long>,
    IComparable<uint>,
    IComparable<int>,
    IComparable<ushort>,
    IComparable<short>,
    IComparable<byte>,
    IComparable<sbyte>,
    IComparable<decimal>,
    IComparable<double>,
    IComparable<float>,
    IConvertible,
    IFormattable,
    IEquatable<Number>,
    IEquatable<ulong>,
    IEquatable<long>,
    IEquatable<uint>,
    IEquatable<int>,
    IEquatable<ushort>,
    IEquatable<short>,
    IEquatable<byte>,
    IEquatable<sbyte>,
    IEquatable<decimal>,
    IEquatable<double>,
    IEquatable<float>
{
    public IConvertible Value => _value;

    public static readonly Number Zero = new(0);
    public static readonly Number MinusOne = new(-1);
    public static readonly Number NaN = new(double.NaN);
    private readonly IConvertible _value;

    #region Constructors

    internal Number(IConvertible fullNumber) => _value = fullNumber is Number nr ? nr._value : fullNumber;

    public Number(long fullNumber) => _value = fullNumber;
    public Number(ulong fullNumber) => _value = fullNumber;
    public Number(int fullNumber) => _value = fullNumber;
    public Number(uint fullNumber) => _value = fullNumber;
    public Number(byte fullNumber) => _value = fullNumber;
    public Number(sbyte fullNumber) => _value = fullNumber;
    public Number(short fullNumber) => _value = fullNumber;
    public Number(ushort fullNumber) => _value = fullNumber;
    public Number(float fullNumber) => _value = fullNumber;
    public Number(double fullNumber) => _value = fullNumber;
    public Number(decimal fullNumber) => _value = fullNumber;

    #endregion

    #region Static helpers

    public static bool IsFloatingPointNumber(object number) => number switch
    {
        Number nr => IsFloatingPointNumber(nr._value),
        Type nrType => nrType == typeof(decimal) || nrType == typeof(double) || nrType == typeof(float),
        decimal or double or float => true,
        _ => false
    };

    public static bool IsIntegralNumber(object number) => number switch
    {
        Number nr => IsIntegralNumber(nr._value),
        Type nrType => nrType == typeof(ulong) || nrType == typeof(long) || nrType == typeof(uint) ||
                       nrType == typeof(int) || nrType == typeof(ushort) || nrType == typeof(short) ||
                       nrType == typeof(byte) || nrType == typeof(sbyte),
        ulong or long or uint or int or ushort or short or byte or sbyte => true,
        _ => false
    };

    private static Type GetOperationTargetType(Number numberLeft, Number numberRight)
    {
        //if any value is a floating point number the result must always be a floating point number
        if (numberLeft._value is decimal || numberRight._value is decimal)
        {
            return typeof(decimal);
        }

        if (numberLeft._value is double || numberRight._value is double)
        {
            return typeof(double);
        }

        if (numberLeft._value is float || numberRight._value is float)
        {
            return typeof(float);
        }

        //if non of the types are floating point numbers check in order of most to least precision
        if (numberLeft._value is ulong || numberRight._value is ulong)
        {
            return typeof(ulong);
        }

        if (numberLeft._value is long || numberRight._value is long)
        {
            return typeof(long);
        }

        if (numberLeft._value is uint || numberRight._value is uint)
        {
            return typeof(uint);
        }

        if (numberLeft._value is int || numberRight._value is int)
        {
            return typeof(int);
        }

        if (numberLeft._value is ushort || numberRight._value is ushort)
        {
            return typeof(ushort);
        }

        if (numberLeft._value is short || numberRight._value is short)
        {
            return typeof(short);
        }

        if (numberLeft._value is byte || numberRight._value is byte)
        {
            return typeof(byte);
        }

        if (numberLeft._value is sbyte || numberRight._value is sbyte)
        {
            return typeof(sbyte);
        }

        throw new InvalidOperationException("Cannot determinate the numbers type");
    }

    public static bool IsIntegralNumber(Number number) => IsIntegralNumber(number._value);
    public static bool IsFloatingPointNumber(Number number) => IsFloatingPointNumber(number._value);

    public static Number AsFloatingPointNumber(Number number)
    {
        if (IsFloatingPointNumber(number._value))
        {
            return number;
        }

        switch (number._value)
        {
            case ulong or long:
                break;
            case uint or int:
                return new Number(number.ToDouble(null));
            case short or ushort:
                return new Number(number.ToSingle(null));
            case byte or sbyte:
                return new Number(number.ToSingle(null));
        }

        return new Number(number.ToDecimal(null));
    }

    public static Number Add(Number left, Number right) => left.Add(right);

    public static Number Subtract(Number left, Number right) => left.Subtract(right);

    public static Number Multiply(Number left, Number right) => left.Multiply(right);

    public static Number Divide(Number left, Number right) => left.Divide(right);

    public static Number Modulo(Number left, Number right) => left.Modulo(right);

    public static Number ShiftLeft(Number left, Number right) => left.ShiftLeft(right);

    public static Number ShiftRight(Number left, Number right) => left.ShiftRight(right);

    public static bool LessThen(Number left, Number right) => left.LessThen(right);

    public static bool SmallerOrEquals(Number left, Number right) => left.LessThen(right) || left.Same(right);

    public static bool GreaterThen(Number left, Number right) => left.GreaterThen(right);

    public static bool GreaterOrEquals(Number left, Number right) => left.GreaterThen(right) || left.Same(right);

    public static bool Equals(Number left, Number right) => left.Equals(right);

    public static bool UnEquals(Number left, Number right) => !left.Equals(right);

    public static bool Same(Number left, Number right) => left.Same(right);

    public static Number Abs(Number left) => left.Abs();

    public static Number Round(Number left, Number right) => left.Round(right);

    public static Number Negate(Number left) => left.Negate();

    public static Number Log(Number left, Number right) => left.Log(right);

    public static Number Max(Number left, Number right) => left.Max(right);

    public static Number Min(Number left, Number right) => left.Min(right);

    public static Number Pow(Number left, Number right) => left.Pow(right);

    public static bool IsNaN(Number left) => left.IsNaN();

    public static bool IsZero(Number left) => left.IsZero();

    public static Number Floor(Number left) => left.Floor();

    public static Number Ceiling(Number left) => left.Ceiling();

    public static Number Truncate(Number left) => left.Truncate();

    public static Number Atan2(Number left, Number right) => left.Atan2(right);

    public static Number Asin(Number left) => left.Asin();

    public static Number Cosh(Number left) => left.Cosh();

    public static Number Cos(Number left) => left.Cos();

    public static Number Acos(Number left) => left.Acos();

    public static Number Tanh(Number left) => left.Tanh();

    public static Number Sqrt(Number left) => left.Sqrt();

    public static Number Sinh(Number left) => left.Sinh();

    public static Number Sin(Number left) => left.Sin();

    public static Number Log10(Number left) => left.Log10();
    public static Number Parse(string text) => TryParse(text, null, out var nr) ? nr : NaN;

    #endregion

    #region Helper Methods

    private Number PerformBinaryOperation(Number other, Type targetType) => targetType switch
    {
        var t when t == typeof(decimal) => new Number(ToDecimal(null) + other.ToDecimal(null)),
        var t when t == typeof(double) => new Number(ToDouble(null) + other.ToDouble(null)),
        var t when t == typeof(float) => new Number(ToSingle(null) + other.ToSingle(null)),
        var t when t == typeof(ulong) => new Number(ToUInt64(null) + other.ToUInt64(null)),
        var t when t == typeof(long) => new Number(ToInt64(null) + other.ToInt64(null)),
        var t when t == typeof(uint) => new Number(ToUInt32(null) + other.ToUInt32(null)),
        var t when t == typeof(int) => new Number(ToInt32(null) + other.ToInt32(null)),
        var t when t == typeof(ushort) => new Number(ToUInt16(null) + other.ToUInt16(null)),
        var t when t == typeof(short) => new Number(ToInt16(null) + other.ToInt16(null)),
        var t when t == typeof(byte) => new Number(ToByte(null) + other.ToByte(null)),
        var t when t == typeof(sbyte) => new Number(ToSByte(null) + other.ToSByte(null)),
        _ => throw new InvalidCastException($"Cannot convert {other._value} ({other._value?.GetType()}) or {_value} ({_value?.GetType()}) to a numeric type")
    };

    private Number PerformBinaryOperationSub(Number other, Type targetType) => targetType switch
    {
        var t when t == typeof(decimal) => new Number(ToDecimal(null) - other.ToDecimal(null)),
        var t when t == typeof(double) => new Number(ToDouble(null) - other.ToDouble(null)),
        var t when t == typeof(float) => new Number(ToSingle(null) - other.ToSingle(null)),
        var t when t == typeof(ulong) => new Number(ToUInt64(null) - other.ToUInt64(null)),
        var t when t == typeof(long) => new Number(ToInt64(null) - other.ToInt64(null)),
        var t when t == typeof(uint) => new Number(ToUInt32(null) - other.ToUInt32(null)),
        var t when t == typeof(int) => new Number(ToInt32(null) - other.ToInt32(null)),
        var t when t == typeof(ushort) => new Number(ToUInt16(null) - other.ToUInt16(null)),
        var t when t == typeof(short) => new Number(ToInt16(null) - other.ToInt16(null)),
        var t when t == typeof(byte) => new Number(ToByte(null) - other.ToByte(null)),
        var t when t == typeof(sbyte) => new Number(ToSByte(null) - other.ToSByte(null)),
        _ => throw new InvalidCastException($"Cannot convert {other._value} ({other._value?.GetType()}) or {_value} ({_value?.GetType()}) to a numeric type")
    };

    private Number PerformBinaryOperationMul(Number other, Type targetType) => targetType switch
    {
        var t when t == typeof(decimal) => new Number(ToDecimal(null) * other.ToDecimal(null)),
        var t when t == typeof(double) => new Number(ToDouble(null) * other.ToDouble(null)),
        var t when t == typeof(float) => new Number(ToSingle(null) * other.ToSingle(null)),
        var t when t == typeof(ulong) => new Number(ToUInt64(null) * other.ToUInt64(null)),
        var t when t == typeof(long) => new Number(ToInt64(null) * other.ToInt64(null)),
        var t when t == typeof(uint) => new Number(ToUInt32(null) * other.ToUInt32(null)),
        var t when t == typeof(int) => new Number(ToInt32(null) * other.ToInt32(null)),
        var t when t == typeof(ushort) => new Number(ToUInt16(null) * other.ToUInt16(null)),
        var t when t == typeof(short) => new Number(ToInt16(null) * other.ToInt16(null)),
        var t when t == typeof(byte) => new Number(ToByte(null) * other.ToByte(null)),
        var t when t == typeof(sbyte) => new Number(ToSByte(null) * other.ToSByte(null)),
        _ => throw new InvalidCastException($"Cannot convert {other._value} ({other._value?.GetType()}) or {_value} ({_value?.GetType()}) to a numeric type")
    };

    private Number PerformBinaryOperationDiv(Number other, Type targetType) => targetType switch
    {
        var t when t == typeof(decimal) => new Number(ToDecimal(null) / other.ToDecimal(null)),
        var t when t == typeof(double) => new Number(ToDouble(null) / other.ToDouble(null)),
        var t when t == typeof(float) => new Number(ToSingle(null) / other.ToSingle(null)),
        var t when t == typeof(ulong) => new Number(ToUInt64(null) / other.ToUInt64(null)),
        var t when t == typeof(long) => new Number(ToInt64(null) / other.ToInt64(null)),
        var t when t == typeof(uint) => new Number(ToUInt32(null) / other.ToUInt32(null)),
        var t when t == typeof(int) => new Number(ToInt32(null) / other.ToInt32(null)),
        var t when t == typeof(ushort) => new Number(ToUInt16(null) / other.ToUInt16(null)),
        var t when t == typeof(short) => new Number(ToInt16(null) / other.ToInt16(null)),
        var t when t == typeof(byte) => new Number(ToByte(null) / other.ToByte(null)),
        var t when t == typeof(sbyte) => new Number(ToSByte(null) / other.ToSByte(null)),
        _ => throw new InvalidCastException($"Cannot convert {other._value} ({other._value?.GetType()}) or {_value} ({_value?.GetType()}) to a numeric type")
    };

    private Number PerformBinaryOperationMod(Number other, Type targetType) => targetType switch
    {
        var t when t == typeof(decimal) => new Number(ToDecimal(null) % other.ToDecimal(null)),
        var t when t == typeof(double) => new Number(ToDouble(null) % other.ToDouble(null)),
        var t when t == typeof(float) => new Number(ToSingle(null) % other.ToSingle(null)),
        var t when t == typeof(ulong) => new Number(ToUInt64(null) % other.ToUInt64(null)),
        var t when t == typeof(long) => new Number(ToInt64(null) % other.ToInt64(null)),
        var t when t == typeof(uint) => new Number(ToUInt32(null) % other.ToUInt32(null)),
        var t when t == typeof(int) => new Number(ToInt32(null) % other.ToInt32(null)),
        var t when t == typeof(ushort) => new Number(ToUInt16(null) % other.ToUInt16(null)),
        var t when t == typeof(short) => new Number(ToInt16(null) % other.ToInt16(null)),
        var t when t == typeof(byte) => new Number(ToByte(null) % other.ToByte(null)),
        var t when t == typeof(sbyte) => new Number(ToSByte(null) % other.ToSByte(null)),
        _ => throw new InvalidCastException($"Cannot convert {other._value} ({other._value?.GetType()}) or {_value} ({_value?.GetType()}) to a numeric type")
    };

    #endregion

    #region Number Operations

    public bool IsNaN() => double.IsNaN(ToDouble(null));
    public bool IsZero() => _value switch
    {
        null => false,
        0 or 0L or 0u or 0ul or (short)0 or (ushort)0 or (byte)0 or (sbyte)0 => true,
        0.0f or 0.0d or 0.0m => true,
        float f when f == 0.0f || (Math.Abs(f) < float.Epsilon) => true,
        double d when d == 0.0 || (Math.Abs(d) < double.Epsilon) => true,
        decimal dec when dec == 0m => true,
        _ => false
    };
    public Number Max(Number other) => this > other ? this : other;
    public Number Min(Number other) => this < other ? this : other;
    public Number Pow(Number other) => Math.Pow(ToDouble(null), other.ToDouble(null));
    public Number Log(Number other) => Math.Log(ToDouble(null), other.ToDouble(null));
    public Number Log() => Log(Math.E);
    public Number Log10() => Log(10D);
    public Number Sin() => Math.Sin(ToDouble(null));
    public Number Sinh() => Math.Sinh(ToDouble(null));
    public Number Sqrt() => Math.Sqrt(ToDouble(null));
    public Number Tanh() => Math.Tanh(ToDouble(null));
    public Number Cos() => Math.Cos(ToDouble(null));
    public Number Cosh() => Math.Cosh(ToDouble(null));
    public Number Acos() => Math.Acos(ToDouble(null));
    public Number Asin() => Math.Asin(ToDouble(null));
    public Number Atan() => Math.Atan(ToDouble(null));
    public Number Atan2(Number x) => Math.Atan2(ToDouble(null), x.ToDouble(null));

    public Number Truncate() => _value is decimal
        ? Math.Truncate(ToDecimal(null))
        : Math.Truncate(ToDouble(null));

    public Number Ceiling() => _value is decimal
        ? Math.Ceiling(ToDecimal(null))
        : Math.Ceiling(ToDouble(null));

    public Number Floor() => _value is decimal
        ? Math.Floor(ToDecimal(null))
        : Math.Floor(ToDouble(null));

    public Number Negate()
    {
        var targetType = _value?.GetType() ?? typeof(object);
        return targetType switch
        {
            var t when t == typeof(decimal) => new Number(-ToDecimal(null)),
            var t when t == typeof(double) => new Number(-ToDouble(null)),
            var t when t == typeof(float) => new Number(-ToSingle(null)),
            var t when t == typeof(long) => new Number(-ToInt64(null)),
            var t when t == typeof(uint) => new Number(-ToDouble(null)),
            var t when t == typeof(int) => new Number(-ToInt32(null)),
            var t when t == typeof(ushort) => new Number(-ToUInt16(null)),
            var t when t == typeof(short) => new Number(-ToInt16(null)),
            var t when t == typeof(byte) => new Number(-ToByte(null)),
            var t when t == typeof(sbyte) => new Number(-ToSByte(null)),
            _ => throw new InvalidCastException($"Cannot negate value for {_value} ({_value?.GetType()})")
        };
    }

    public Number Abs()
    {
        var targetType = _value?.GetType() ?? typeof(object);
        return targetType switch
        {
            var t when t == typeof(decimal) => new Number(Math.Abs(ToDecimal(null))),
            var t when t == typeof(double) => new Number(Math.Abs(ToDouble(null))),
            var t when t == typeof(float) => new Number(Math.Abs(ToSingle(null))),
            var t when t == typeof(long) => new Number(Math.Abs(ToInt64(null))),
            var t when t == typeof(uint) => new Number(ToUInt32(null)),
            var t when t == typeof(int) => new Number(Math.Abs(ToInt32(null))),
            var t when t == typeof(ushort) => new Number(ToUInt16(null)),
            var t when t == typeof(short) => new Number(Math.Abs(ToInt16(null))),
            var t when t == typeof(byte) => new Number(ToByte(null)),
            var t when t == typeof(sbyte) => new Number(Math.Abs(ToSByte(null))),
            _ => throw new InvalidCastException($"Cannot get absolute value for {_value} ({_value?.GetType()})")
        };
    }

    /// <summary>
    ///		Rounds a decimal value to a specified number of fractional digits.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public Number Round(Number other) => Round(other, MidpointRounding.ToEven);

    /// <summary>
    ///		Rounds a decimal value to a specified number of fractional digits.
    /// </summary>
    public Number Round(Number other, MidpointRounding mode) => _value is decimal
        ? Math.Round(_value.ToDecimal(null), other.ToInt32(null), mode)
        : Math.Round(_value.ToDouble(null), other.ToInt32(null), mode);

    public Number Add(Number other) => PerformBinaryOperation(other, GetOperationTargetType(this, other));

    public Number Subtract(Number other) => PerformBinaryOperationSub(other, GetOperationTargetType(this, other));

    public Number Multiply(Number other) => PerformBinaryOperationMul(other, GetOperationTargetType(this, other));

    public Number Divide(Number other) => PerformBinaryOperationDiv(other, GetOperationTargetType(this, other));

    public Number Modulo(Number other) => PerformBinaryOperationMod(other, GetOperationTargetType(this, other));

    public Number ShiftRight(Number other)
    {
        var targetType = GetOperationTargetType(this, other);
        return targetType switch
        {
            var t when t == typeof(ulong) => new Number(ToUInt64(null) >> other.ToInt32(null)),
            var t when t == typeof(long) => new Number(ToInt64(null) >> other.ToInt32(null)),
            var t when t == typeof(uint) => new Number(ToUInt32(null) >> other.ToInt32(null)),
            var t when t == typeof(int) => new Number(ToInt32(null) >> other.ToInt32(null)),
            var t when t == typeof(ushort) => new Number(ToUInt16(null) >> other.ToInt32(null)),
            var t when t == typeof(short) => new Number(ToInt16(null) >> other.ToInt32(null)),
            var t when t == typeof(byte) => new Number(ToByte(null) >> other.ToInt32(null)),
            var t when t == typeof(sbyte) => new Number(ToSByte(null) >> other.ToInt32(null)),
            _ => throw new InvalidCastException($"Cannot convert {other._value} ({other._value?.GetType()}) or {_value} ({_value?.GetType()}) to a numeric type")
        };
    }

    public Number ShiftLeft(Number other)
    {
        var targetType = GetOperationTargetType(this, other);
        return targetType switch
        {
            var t when t == typeof(ulong) => new Number(ToUInt64(null) << other.ToInt32(null)),
            var t when t == typeof(long) => new Number(ToInt64(null) << other.ToInt32(null)),
            var t when t == typeof(uint) => new Number(ToUInt32(null) << other.ToInt32(null)),
            var t when t == typeof(int) => new Number(ToInt32(null) << other.ToInt32(null)),
            var t when t == typeof(ushort) => new Number(ToUInt16(null) << other.ToInt32(null)),
            var t when t == typeof(short) => new Number(ToInt16(null) << other.ToInt32(null)),
            var t when t == typeof(byte) => new Number(ToByte(null) << other.ToInt32(null)),
            var t when t == typeof(sbyte) => new Number(ToSByte(null) << other.ToInt32(null)),
            _ => throw new InvalidCastException($"Cannot convert {other._value} ({other._value?.GetType()}) or {_value} ({_value?.GetType()}) to a numeric type")
        };
    }

    public bool GreaterThen(Number other)
    {
        var targetType = GetOperationTargetType(this, other);
        return targetType switch
        {
            var t when t == typeof(decimal) => ToDecimal(null) > other.ToDecimal(null),
            var t when t == typeof(double) => ToDouble(null) > other.ToDouble(null),
            var t when t == typeof(float) => ToSingle(null) > other.ToSingle(null),
            var t when t == typeof(ulong) => ToUInt64(null) > other.ToUInt64(null),
            var t when t == typeof(long) => ToInt64(null) > other.ToInt64(null),
            var t when t == typeof(uint) => ToUInt32(null) > other.ToUInt32(null),
            var t when t == typeof(int) => ToInt32(null) > other.ToInt32(null),
            var t when t == typeof(ushort) => ToUInt16(null) > other.ToUInt16(null),
            var t when t == typeof(short) => ToInt16(null) > other.ToInt16(null),
            var t when t == typeof(byte) => ToByte(null) > other.ToByte(null),
            var t when t == typeof(sbyte) => ToSByte(null) > other.ToSByte(null),
            _ => throw new InvalidCastException($"Cannot convert {other._value} ({other._value?.GetType()}) or {_value} ({_value?.GetType()}) to a numeric type")
        };
    }

    public bool LessThen(Number other)
    {
        var targetType = GetOperationTargetType(this, other);
        return targetType switch
        {
            var t when t == typeof(decimal) => ToDecimal(null) < other.ToDecimal(null),
            var t when t == typeof(double) => ToDouble(null) < other.ToDouble(null),
            var t when t == typeof(float) => ToSingle(null) < other.ToSingle(null),
            var t when t == typeof(ulong) => ToUInt64(null) < other.ToUInt64(null),
            var t when t == typeof(long) => ToInt64(null) < other.ToInt64(null),
            var t when t == typeof(uint) => ToUInt32(null) < other.ToUInt32(null),
            var t when t == typeof(int) => ToInt32(null) < other.ToInt32(null),
            var t when t == typeof(ushort) => ToUInt16(null) < other.ToUInt16(null),
            var t when t == typeof(short) => ToInt16(null) < other.ToInt16(null),
            var t when t == typeof(byte) => ToByte(null) < other.ToByte(null),
            var t when t == typeof(sbyte) => ToSByte(null) < other.ToSByte(null),
            _ => throw new InvalidCastException($"Cannot convert {other._value} ({other._value?.GetType()}) or {_value} ({_value?.GetType()}) to a numeric type")
        };
    }

    public bool Equals(Number other) => other._value?.GetType() == _value?.GetType() && Same(other);

    public bool Same(Number other) => Equals(other._value, _value) || (_value?.GetType() switch
    {
        var t when t == typeof(decimal) => ToDecimal(null) == other.ToDecimal(null),
        var t when t == typeof(double) => ToDouble(null) == other.ToDouble(null),
        var t when t == typeof(float) => ToSingle(null) == other.ToSingle(null),
        var t when t == typeof(ulong) => ToUInt64(null) == other.ToUInt64(null),
        var t when t == typeof(long) => ToInt64(null) == other.ToInt64(null),
        var t when t == typeof(uint) => ToUInt32(null) == other.ToUInt32(null),
        var t when t == typeof(int) => ToInt32(null) == other.ToInt32(null),
        var t when t == typeof(ushort) => ToUInt16(null) == other.ToUInt16(null),
        var t when t == typeof(short) => ToInt16(null) == other.ToInt16(null),
        var t when t == typeof(byte) => ToByte(null) == other.ToByte(null),
        var t when t == typeof(sbyte) => ToSByte(null) == other.ToSByte(null),
        _ => false
    });

    public static bool TryCast(IConvertible convertable, out Number? number)
    {
        number = convertable switch
        {
            ulong noulong => new Number(noulong),
            long nolong => new Number(nolong),
            uint nouint => new Number(nouint),
            int noint => new Number(noint),
            ushort noushort => new Number(noushort),
            short noshort => new Number(noshort),
            byte nobyte => new Number(nobyte),
            sbyte nosbyte => new Number(nosbyte),
            decimal nodecimal => new Number(nodecimal),
            double nodouble => new Number(nodouble),
            float nofloat => new Number(nofloat),
            _ => null
        };
        return number.HasValue;
    }

    public static bool TryParse(string input, CultureInfo culture, out Number number)
    {
        if (input.Length == 0)
        {
            number = NaN;
            return false;
        }

        if (input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            input = input.TrimStart('0', 'x', 'X');

            if (int.TryParse(input, NumberStyles.HexNumber, culture, out var hexIntVal))
            {
                number = new Number(hexIntVal);
                return true;
            }

            number = default;
            return false;
        }

        if (char.IsLetter(input[input.Length - 1]))
        {
            //according to MSDN folloring literals are allowed

            if (input.EndsWith("u", StringComparison.CurrentCultureIgnoreCase))
            {
                input = input.TrimEnd('u', 'U');

                //its an unsigned number
                //evaluate of which type it is
                if (uint.TryParse(input, NumberStyles.Integer, culture, out var uIntVal))
                {
                    number = new Number(uIntVal);
                    return true;
                }

                if (ushort.TryParse(input, NumberStyles.Integer, culture, out var ushortVal))
                {
                    number = new Number(ushortVal);
                    return true;
                }

                if (ulong.TryParse(input, NumberStyles.Integer, culture, out var uLongVal))
                {
                    number = new Number(uLongVal);
                    return true;
                }

                if (byte.TryParse(input, NumberStyles.Integer, culture, out var byteVal))
                {
                    number = new Number(byteVal);
                    return true;
                }

                number = default;
                return false;
            }

            if (input.EndsWith("m", StringComparison.CurrentCultureIgnoreCase))
            {
                input = input.TrimEnd('m', 'M');

                //its an unsigned number
                //evaluate of which type it is
                if (decimal.TryParse(input, NumberStyles.Number, culture, out var uIntVal))
                {
                    number = new Number(uIntVal);
                    return true;
                }

                number = default;
                return false;
            }

            if (input.EndsWith("l", StringComparison.OrdinalIgnoreCase))
            {
                //its an long
                if (input.EndsWith("ul", StringComparison.OrdinalIgnoreCase))
                {
                    input = input.TrimEnd('u', 'U', 'l', 'L');

                    //its unsigned
                    if (ulong.TryParse(input, NumberStyles.Integer, culture, out var uLongVal))
                    {
                        number = new Number(uLongVal);
                        return true;
                    }

                    number = default;
                    return false;
                }

                input = input.TrimEnd('l', 'L');

                //its signed
                if (long.TryParse(input, NumberStyles.Integer, culture, out var explLongVal))
                {
                    number = new Number(explLongVal);
                    return true;
                }

                number = default;
                return false;
            }

            if (input.EndsWith("f", StringComparison.OrdinalIgnoreCase))
            {
                //its an float
                input = input.TrimEnd('f', 'F');

                if (float.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, culture, out var floatVal))
                {
                    number = new Number(floatVal);
                    return true;
                }

                number = default;
                return false;
            }

            if (input.EndsWith("d", StringComparison.OrdinalIgnoreCase))
            {
                //its an float
                input = input.TrimEnd('d', 'D');

                if (double.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, culture,
                        out var doubleVal))
                {
                    number = new Number(doubleVal);
                    return true;
                }

                number = default;
                return false;
            }
        }

        //we start with parsing an int as its the default for any number in msbuild
        if (int.TryParse(input, NumberStyles.Integer, culture, out var intVal))
        {
            number = new Number(intVal);
            return true;
        }

        //if its bigger then an int it is most likely an long
        if (long.TryParse(input, NumberStyles.Integer, culture, out var longVal))
        {
            number = new Number(longVal);
            return true;
        }
        //if (uint.TryParse(input, out var impliUIntVal))
        //{
        //	number = new Number(impliUIntVal);
        //	return true;
        //}
        //if (ulong.TryParse(input, out var impliULongVal))
        //{
        //	number = new Number(impliULongVal);
        //	return true;
        //}

        if (double.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, culture, out var impliDoubleVal))
        {
            number = new Number(impliDoubleVal);
            return true;
        }

        if (sbyte.TryParse(input, NumberStyles.Integer, culture, out var sByteVal))
        {
            number = new Number(sByteVal);
            return true;
        }

        if (ushort.TryParse(input, NumberStyles.Integer, culture, out var shortVal))
        {
            number = new Number(shortVal);
            return true;
        }

        if (decimal.TryParse(input, NumberStyles.Number, culture, out var decimalVal))
        {
            number = new Number(decimalVal);
            return true;
        }

        number = NaN;
        return false;
    }

    public string AsParsableString() => _value?.GetType() switch
    {
        var t when t == typeof(decimal) => _value + "M",
        var t when t == typeof(double) => _value + "D",
        var t when t == typeof(float) => _value + "F",
        var t when t == typeof(ulong) => _value + "UL",
        var t when t == typeof(long) => _value + "L",
        var t when t == typeof(uint) => _value + "U",
        var t when t == typeof(int) => _value.ToString(null),
        var t when t == typeof(ushort) => _value.ToString(null) + "U",
        var t when t == typeof(short) => _value.ToString(null),
        var t when t == typeof(byte) => "0x" + _value,
        var t when t == typeof(sbyte) => "0x" + _value + "u",
        _ => null
    };

    #endregion

    public int CompareTo(ulong other) => CompareTo((object)other);
    public int CompareTo(long other) => CompareTo((object)other);
    public int CompareTo(uint other) => CompareTo((object)other);
    public int CompareTo(int other) => CompareTo((object)other);
    public int CompareTo(ushort other) => CompareTo((object)other);
    public int CompareTo(short other) => CompareTo((object)other);
    public int CompareTo(byte other) => CompareTo((object)other);
    public int CompareTo(sbyte other) => CompareTo((object)other);
    public int CompareTo(decimal other) => CompareTo((object)other);
    public int CompareTo(double other) => CompareTo((object)other);
    public int CompareTo(float other) => CompareTo((object)other);

    public bool Equals(ulong other) => Equals((object)other);
    public bool Equals(long other) => Equals((object)other);
    public bool Equals(uint other) => Equals((object)other);
    public bool Equals(int other) => Equals((object)other);
    public bool Equals(ushort other) => Equals((object)other);
    public bool Equals(short other) => Equals((object)other);
    public bool Equals(byte other) => Equals((object)other);
    public bool Equals(sbyte other) => Equals((object)other);
    public bool Equals(decimal other) => Equals((object)other);
    public bool Equals(double other) => Equals((object)other);
    public bool Equals(float other) => Equals((object)other);

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        return (obj is Number other && Equals(other))
               || ((IsIntegralNumber(obj) || IsFloatingPointNumber(obj)) &&
                   obj is IConvertible objCom && new Number(objCom).Same(this));
    }

    public override int GetHashCode() => (_value != null ? _value.GetHashCode() : 0);

    #region IConvertable

    public TypeCode GetTypeCode() => _value?.GetTypeCode() ?? TypeCode.Empty;

    public bool ToBoolean(IFormatProvider provider) => _value switch
    {
        bool realVal => realVal,
        not null when IsZero() => false,
        not null => _value.ToBoolean(provider),
        _ => false,
    };

    public char ToChar(IFormatProvider provider) => _value switch
    {
        char realVal => realVal,
        not null => _value.ToChar(provider),
        _ => '\0',
    };
    public sbyte ToSByte(IFormatProvider provider) => _value switch
    {
        sbyte realVal => realVal,
        not null => _value.ToSByte(provider),
        _ => Zero.ToSByte(provider),
    };

    public byte ToByte(IFormatProvider provider) => _value switch
    {
        byte realVal => realVal,
        not null => _value.ToByte(provider),
        _ => Zero.ToByte(provider),
    };

    public short ToInt16(IFormatProvider provider) => _value switch
    {
        short realVal => realVal,
        not null => _value.ToInt16(provider),
        _ => Zero.ToInt16(provider),
    };

    public ushort ToUInt16(IFormatProvider provider) => _value switch
    {
        ushort realVal => realVal,
        not null => _value.ToUInt16(provider),
        _ => Zero.ToUInt16(provider),
    };

    public int ToInt32(IFormatProvider provider) => _value switch
    {
        int realVal => realVal,
        not null => _value.ToInt32(provider),
        _ => Zero.ToInt32(provider),
    };

    public uint ToUInt32(IFormatProvider provider) => _value switch
    {
        uint realVal => realVal,
        not null => _value.ToUInt32(provider),
        _ => Zero.ToUInt32(provider),
    };

    public long ToInt64(IFormatProvider provider) => _value switch
    {
        long realVal => realVal,
        not null => _value.ToInt64(provider),
        _ => Zero.ToInt64(provider),
    };

    public ulong ToUInt64(IFormatProvider provider) => _value switch
    {
        ulong realVal => realVal,
        not null => _value.ToUInt64(provider),
        _ => Zero.ToUInt64(provider),
    };
    public float ToSingle(IFormatProvider provider) => _value switch
    {
        float realVal => realVal,
        not null => _value.ToSingle(provider),
        _ => float.NaN,
    };
    public double ToDouble(IFormatProvider provider) => _value switch
    {
        double realVal => realVal,
        not null => _value.ToDouble(provider),
        _ => double.NaN,
    };
    public decimal ToDecimal(IFormatProvider provider) => _value switch
    {
        decimal realVal => realVal,
        not null => _value.ToDecimal(provider),
        _ => Zero.ToDecimal(provider),
    };

    public DateTime ToDateTime(IFormatProvider provider) => _value switch
    {
        DateTime realVal => realVal,
        not null => _value.ToDateTime(provider),
        _ => DateTime.MinValue,
    };

    public string ToString(IFormatProvider provider) => _value switch
    {
        not null => _value.ToString(provider),
        _ => "NaN",
    };

    public object ToType(Type conversionType, IFormatProvider provider)
    {
        if (_value is null)
        {
            if (conversionType == typeof(string))
                return "NaN";
            if (conversionType.IsValueType && Nullable.GetUnderlyingType(conversionType) == null)
                return Activator.CreateInstance(conversionType);
            return null;
        }

        if (_value.GetType() == conversionType)
        {
            return _value;
        }

        try
        {
            return _value.ToType(conversionType, provider);
        }
        catch (InvalidCastException)
        {
            throw new InvalidCastException($"Cannot convert from {_value.GetType()} to {conversionType}");
        }
    }

    public string ToString(string format, IFormatProvider formatProvider) => _value switch
    {
        IFormattable formattable => formattable.ToString(format, formatProvider),
        not null => _value.ToString(formatProvider),
        _ => "NaN",
    };

    public override string ToString() => _value?.ToString(CultureInfo.CurrentCulture) ?? "NaN";
    public int CompareTo(object obj)
    {
        if (_value is null && obj is null) return 0;
        if (_value is null) return -1;
        if (obj is null) return 1;

        if (!(_value is IComparable comparable))
        {
            throw new ArgumentException($"Cannot compare '{obj}' with '{_value}'");
        }

        try
        {
            return comparable.CompareTo(obj);
        }
        catch (ArgumentException)
        {
            // Try converting obj to our type for comparison
            if (obj is IConvertible convertible)
            {
                try
                {
                    var converted = convertible.ToType(_value.GetType(), CultureInfo.InvariantCulture);
                    return comparable.CompareTo(converted);
                }
                catch
                {
                    throw new ArgumentException($"Cannot compare '{obj}' with '{_value}'");
                }
            }

            throw;
        }
    }

    public int CompareTo(Number obj)
    {
        if (_value is null && obj._value is null) return 0;
        if (_value is null) return -1;
        if (obj._value is null) return 1;

        if (!(_value is IComparable comparable))
        {
            throw new ArgumentException($"Cannot compare '{obj}' with '{_value}'");
        }

        try
        {
            return comparable.CompareTo(obj._value);
        }
        catch (ArgumentException)
        {
            // Try using common comparison logic
            var targetType = GetOperationTargetType(this, obj);

            if (targetType == typeof(decimal))
                return ToDecimal(null).CompareTo(obj.ToDecimal(null));
            if (targetType == typeof(double))
                return ToDouble(null).CompareTo(obj.ToDouble(null));
            if (targetType == typeof(float))
                return ToSingle(null).CompareTo(obj.ToSingle(null));
            if (targetType == typeof(long))
                return ToInt64(null).CompareTo(obj.ToInt64(null));

            throw new ArgumentException($"Cannot compare '{obj}' with '{_value}'");
        }
    }

    #endregion

    #region Operator Overloading

    public static Number operator +(Number a) => a;
    public static Number operator ++(Number a) => a.Add(1);
    public static Number operator -(Number a) => a * -1;
    public static Number operator --(Number a) => a.Subtract(1);
    public static Number operator <<(Number a, int b) => a.ShiftLeft(b);
    public static Number operator >> (Number a, int b) => a.ShiftRight(b);
    public static bool operator ==(Number a, Number b) => a.Equals(b);
    public static bool operator !=(Number a, Number b) => !a.Equals(b);
    public static bool operator <(Number a, Number b) => a.LessThen(b);
    public static bool operator >(Number a, Number b) => a.GreaterThen(b);
    public static bool operator <=(Number a, Number b) => a.Equals(b) || a.LessThen(b);
    public static bool operator >=(Number a, Number b) => a.Equals(b) || a.GreaterThen(b);

    public static Number operator +(Number a, Number b) => a.Add(b);
    public static Number operator -(Number a, Number b) => a.Subtract(b);
    public static Number operator *(Number a, Number b) => a.Multiply(b);
    public static Number operator /(Number a, Number b) => a.Divide(b);
    public static Number operator %(Number a, Number b) => a.Modulo(b);

    public static Number operator +(Number a, decimal b) => a.Add(b);
    public static Number operator -(Number a, decimal b) => a.Subtract(b);
    public static Number operator *(Number a, decimal b) => a.Multiply(b);
    public static Number operator /(Number a, decimal b) => a.Divide(b);
    public static Number operator %(Number a, decimal b) => a.Modulo(b);

    public static Number operator +(Number a, double b) => a.Add(b);
    public static Number operator -(Number a, double b) => a.Subtract(b);
    public static Number operator *(Number a, double b) => a.Multiply(b);
    public static Number operator /(Number a, double b) => a.Divide(b);
    public static Number operator %(Number a, double b) => a.Modulo(b);

    public static Number operator +(Number a, float b) => a.Add(b);
    public static Number operator -(Number a, float b) => a.Subtract(b);
    public static Number operator *(Number a, float b) => a.Multiply(b);
    public static Number operator /(Number a, float b) => a.Divide(b);
    public static Number operator %(Number a, float b) => a.Modulo(b);

    public static Number operator +(Number a, ulong b) => a.Add(b);
    public static Number operator -(Number a, ulong b) => a.Subtract(b);
    public static Number operator *(Number a, ulong b) => a.Multiply(b);
    public static Number operator /(Number a, ulong b) => a.Divide(b);
    public static Number operator %(Number a, ulong b) => a.Modulo(b);

    public static Number operator +(Number a, long b) => a.Add(b);
    public static Number operator -(Number a, long b) => a.Subtract(b);
    public static Number operator *(Number a, long b) => a.Multiply(b);
    public static Number operator /(Number a, long b) => a.Divide(b);
    public static Number operator %(Number a, long b) => a.Modulo(b);

    public static Number operator +(Number a, uint b) => a.Add(b);
    public static Number operator -(Number a, uint b) => a.Subtract(b);
    public static Number operator *(Number a, uint b) => a.Multiply(b);
    public static Number operator /(Number a, uint b) => a.Divide(b);
    public static Number operator %(Number a, uint b) => a.Modulo(b);

    public static Number operator +(Number a, int b) => a.Add(b);
    public static Number operator -(Number a, int b) => a.Subtract(b);
    public static Number operator *(Number a, int b) => a.Multiply(b);
    public static Number operator /(Number a, int b) => a.Divide(b);
    public static Number operator %(Number a, int b) => a.Modulo(b);

    public static Number operator +(Number a, ushort b) => a.Add(b);
    public static Number operator -(Number a, ushort b) => a.Subtract(b);
    public static Number operator *(Number a, ushort b) => a.Multiply(b);
    public static Number operator /(Number a, ushort b) => a.Divide(b);
    public static Number operator %(Number a, ushort b) => a.Modulo(b);

    public static Number operator +(Number a, short b) => a.Add(b);
    public static Number operator -(Number a, short b) => a.Subtract(b);
    public static Number operator *(Number a, short b) => a.Multiply(b);
    public static Number operator /(Number a, short b) => a.Divide(b);
    public static Number operator %(Number a, short b) => a.Modulo(b);

    public static Number operator +(Number a, byte b) => a.Add(b);
    public static Number operator -(Number a, byte b) => a.Subtract(b);
    public static Number operator *(Number a, byte b) => a.Multiply(b);
    public static Number operator /(Number a, byte b) => a.Divide(b);
    public static Number operator %(Number a, byte b) => a.Modulo(b);

    public static Number operator +(Number a, sbyte b) => a.Add(b);
    public static Number operator -(Number a, sbyte b) => a.Subtract(b);
    public static Number operator *(Number a, sbyte b) => a.Multiply(b);
    public static Number operator /(Number a, sbyte b) => a.Divide(b);
    public static Number operator %(Number a, sbyte b) => a.Modulo(b);

    public static implicit operator Number(decimal d) => new(d);
    public static implicit operator Number(double d) => new(d);
    public static implicit operator Number(float d) => new(d);
    public static implicit operator Number(ulong d) => new(d);
    public static implicit operator Number(long d) => new(d);
    public static implicit operator Number(uint d) => new(d);
    public static implicit operator Number(int d) => new(d);
    public static implicit operator Number(ushort d) => new(d);
    public static implicit operator Number(short d) => new(d);
    public static implicit operator Number(byte d) => new(d);
    public static implicit operator Number(sbyte d) => new(d);

    public static implicit operator decimal(Number d) => d.ToDecimal(null);
    public static implicit operator double(Number d) => d.ToDouble(null);
    public static implicit operator float(Number d) => d.ToSingle(null);
    public static implicit operator ulong(Number d) => d.ToUInt64(null);
    public static implicit operator long(Number d) => d.ToInt64(null);
    public static implicit operator uint(Number d) => d.ToUInt32(null);
    public static implicit operator int(Number d) => d.ToInt32(null);
    public static implicit operator ushort(Number d) => d.ToUInt16(null);
    public static implicit operator short(Number d) => d.ToInt16(null);
    public static implicit operator byte(Number d) => d.ToByte(null);
    public static implicit operator sbyte(Number d) => d.ToSByte(null);

    #endregion
}