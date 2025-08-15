using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using lib.Model;

namespace lib.Helpers;

/// <summary>
/// Provides validation and comparison operations for objects with fluent API and static helper methods.
/// </summary>
public class Guard
{
    /// <summary>
    /// Gets the item being validated.
    /// </summary>
    public object? Item { get; }

    /// <summary>
    /// Initializes a new instance of the Guard class with the specified item.
    /// </summary>
    /// <param name="item">The item to validate.</param>
    protected Guard(object? item)
    {
        Item = item;
    }

    #region Unary

    /// <summary>
    /// Checks if the item is null.
    /// </summary>
    /// <returns>A GuardResult indicating whether the item is null.</returns>
    public GuardResult IsNull() => Item == null;

    /// <summary>
    /// Checks if the item is a Number with zero value.
    /// </summary>
    /// <returns>A GuardResult indicating whether the item is zero.</returns>
    public GuardResult IsZero() => Item is Number n && n.IsZero();
    
    /// <summary>
    /// Checks if the item is a positive Number.
    /// </summary>
    /// <returns>A GuardResult indicating whether the item is positive.</returns>
    public GuardResult IsPositive() => Item is Number n && !n.IsNaN() && n > 0;
    
    /// <summary>
    /// Checks if the item is a negative Number.
    /// </summary>
    /// <returns>A GuardResult indicating whether the item is negative.</returns>
    public GuardResult IsNegative() => Item is Number n && !n.IsNaN() && n < 0;

    /// <summary>
    /// Checks if the item is a Number with a fractional part.
    /// </summary>
    /// <returns>A GuardResult indicating whether the item has a fraction.</returns>
    public GuardResult HasFraction() => Item is Number n
                                        && !n.IsNaN()
                                        && Number.IsFloatingPointNumber(n)
                                        && n.Truncate() != n;

    /// <summary>
    /// Checks if the item is empty (null, empty string, empty collection, zero number, etc.).
    /// </summary>
    /// <returns>A GuardResult indicating whether the item is empty.</returns>
    public GuardResult IsEmpty()
    {
        if (IsNull()) return true;

        if (Guid.Empty.Equals(Item)) return true;

        if (Item is string { Length: 0 }) return true;

        if (Item is FileSystemInfo { Exists: false }) return true;

        if (Item is FileInfo f) return f.ReadAllText().IsNullOrEmpty();

        if (Item is DirectoryInfo dir) return !dir.ListFiles().Any();

        if (Item is Number num) return num.IsZero();

        if (Item is IEnumerable e)
            return !e.OfType<object>().Any();

        return $"Object type is not supported: {Item!.GetType()}";
    }

    public GuardResult IsNotEmpty() => !IsEmpty();

    #endregion

    #region Compare

    private GuardResult? Check(Guard? other)
    {
        if (ReferenceEquals(this, other)) return true;
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(Item, other.Item)) return true;
        if (Item == null || other.Item == null) return false;

        return null;
    }

    public GuardResult Same(Guard? other)
    {
        if (Check(other) is { } r)
            return r;
        
        return CommonComparer.Equals(Item, other.Item);
    }

    public GuardResult More(Guard? other)
    {
        if (Check(other) is { }) return false;
        
        return CommonComparer.Compare(Item, other!.Item) > 0;
    }

    public GuardResult Less(Guard? other)
    {
        if (Check(other) is { }) return false;

        return CommonComparer.Compare(Item, other!.Item) < 0;
    }

    public GuardResult Contains(Guard? other)
    {
        if (Check(other) is { }) return false;
        
        return CommonComparer.Contains(Item, other!.Item);
    }

    public GuardResult SameType(Guard? other) => Item?.GetType() == other?.Item?.GetType();

    public override bool Equals(object? obj) => obj is Guard g && Same(g);

    public override int GetHashCode() => 0;

    #endregion

    #region operands

    public static GuardResult operator ==(Guard? left, Guard? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;

        return left.Same(right);
    }

    public static GuardResult operator !=(Guard? left, Guard? right)
    {
        if (ReferenceEquals(left, right)) return false;
        if (left is null || right is null) return true;

        return !left.Same(right);
    }

    public static GuardResult operator <(Guard? left, Guard? right)
    {
        return left is not null && right is not null && left.Less(right);
    }

    public static GuardResult operator <=(Guard? left, Guard? right)
    {
        return left == right || left < right;
    }

    public static GuardResult operator >(Guard? left, Guard? right)
    {
        return left is not null && right is not null && left.More(right);
    }

    public static GuardResult operator >=(Guard? left, Guard? right)
    {
        return left == right || left > right;
    }

    #endregion

    #region static helper

    public static GuardResult Null(object? parameter) => new Guard(parameter).IsNull().CheckAndThrow();
    public static GuardResult IsZero(object? parameter) => new Guard(parameter).IsZero().CheckAndThrow();
    public static GuardResult IsPositive(object? parameter) => new Guard(parameter).IsPositive().CheckAndThrow();
    public static GuardResult IsNegative(object? parameter) => new Guard(parameter).IsNegative().CheckAndThrow();
    public static GuardResult HasFraction(object? parameter) => new Guard(parameter).HasFraction().CheckAndThrow();
    public static GuardResult IsEmpty(object? parameter) => new Guard(parameter).IsEmpty().CheckAndThrow();
    public static GuardResult IsNotEmpty(object? parameter) => new Guard(parameter).IsNotEmpty().CheckAndThrow();


    public static GuardResult Same(object? left, object? right) => new Guard(left).Same(new Guard(right)).CheckAndThrow();
    public static GuardResult More(object? left, object? right) => new Guard(left).More(new Guard(right)).CheckAndThrow();
    public static GuardResult Less(object? left, object? right) => new Guard(left).Less(new Guard(right)).CheckAndThrow();

    public static GuardResult Contains(string? src, string? text)
    {
        return new Guard(src).Contains(new Guard(text)).CheckAndThrow();
    }
    public static GuardResult Contains<T>(IEnumerable<T>? src, T? item)
    {
        return new Guard(src).Contains(new Guard(item)).CheckAndThrow();
    }

    #endregion
}