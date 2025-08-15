using System;
using System.Collections;
using System.Collections.Generic;

namespace lib.Helpers;

public delegate bool EqualsDelegate<in T>(T? x, T? y);

public delegate int CompareDelegate<in T>(T? x, T? y);

internal class DelegateComparer<T, TKey> : IEqualityComparer<T>, IComparer<T>
{
    public Func<T, TKey>? KeySelector { get; set; }
    public EqualsDelegate<TKey>? EqDelegate { get; set; }
    public CompareDelegate<TKey>? CompareDelegate { get; set; }

    public bool Equals(T? x, T? y)
    {
        if (x is null && y is null) return true;
        if (x is null || y is null) return false;

        if (KeySelector != null)
        {
            var left = KeySelector!(x!);
            var right = KeySelector!(y!);

            if (left is null && right is null) return true;
            if (left is null || right is null) return false;


            if (EqDelegate != null)
                return EqDelegate(left, right);

            if (CompareDelegate != null)
                return CompareDelegate(left, right) == 0;
            
            return Equals(left, right);
        }


        return Equals(x, y);
    }

    public int GetHashCode(T? obj) => 0;

    public int Compare(T? x, T? y)
    {
        if (x is null && y is null) return 0;
        if (x is null) return 1;
        if (y is null) return -1;

        if (KeySelector != null)
        {
            var left = KeySelector!(x!);
            var right = KeySelector!(y!);

            if (left is null && right is null) return 0;
            if (left is null) return 1;
            if (right is null) return -1;

            if (CompareDelegate != null)
                return CompareDelegate(left, right);

            if (EqDelegate != null && EqDelegate(left, right))
            {
                return 0;
            }
            
            return System.Collections.Comparer.Default.Compare(left, right);
        }

        return System.Collections.Comparer.Default.Compare(x, y);
    }
}