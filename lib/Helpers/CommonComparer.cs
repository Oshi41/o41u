using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using lib.Services;

namespace lib.Helpers;

/// <summary>
/// Provides comprehensive comparison and containment operations for various data types including dictionaries, enumerables, and primitives.
/// </summary>
/// <typeparam name="T">The type of objects to compare.</typeparam>
internal class CommonComparer<T> : IRichComparer<T>
{
    /// <summary>
    /// Gets a singleton instance of the CommonComparer.
    /// </summary>
    public static IRichComparer Instance { get; } = new CommonComparer<T>();
    
    /// <summary>
    /// Compares two objects and returns an integer indicating their relative order.
    /// Supports dictionaries, enumerables, and IComparable objects.
    /// </summary>
    /// <param name="x">The first object to compare.</param>
    /// <param name="y">The second object to compare.</param>
    /// <returns>Less than zero if x is less than y; zero if equal; greater than zero if x is greater than y.</returns>
    public virtual int Compare(object? x, object? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (ReferenceEquals(null, y)) return -1;
        if (ReferenceEquals(x, null)) return 1;

        var count = 0;

        if (x is IComparable xC && y is IComparable yC)
        {
            count = xC.CompareTo(yC);
            if (count != 0) return count;
        }

        if (x is IDictionary { Count: > 0 } xDict)
        {
            if (y is IDictionary { Count: > 0 } yDict)
            {
                count = xDict.Count - yDict.Count;
                if (count != 0) return count;

                count = xDict.Keys.Except(yDict.Keys, (IEqualityComparer)this).OfType<object>().Count();
                if (count != 0) return count;
                
                foreach (var (key, xValue) in xDict.Entries())
                {
                    count = Compare(xValue, yDict[key]);
                    if (count != 0) return count;
                }

                return -1;
            }

            if (y is IEnumerable arr && arr.OfType<object>().ToList() is { Count: > 0 } list)
            {
                count = list.Except(xDict.Keys, this).Count();
                return count;
            }

            return -1;
        }

        if (x is IEnumerable xarr && xarr.OfType<object>().ToList() is { Count: > 0 } xList)
        {
            if (y is IEnumerable arr && arr.OfType<object>().ToList() is { Count: > 0 } ylist)
            {
                count = xList.Count - ylist.Count;
                if (count != 0) return count;
                
                count = xList.Except(ylist, this).Count();
                return count;
            }

            return -1;
        }

        if (x.Equals(y)) return 0;

        Log.Warning("Type Does not support comparing: {e}", x.GetType());
        return x.GetHashCode() - y.GetHashCode();
    }
    
    /// <summary>
    /// Determines whether the first object contains or includes the second object.
    /// Supports containment checks for dictionaries, enumerables, and strings.
    /// </summary>
    /// <param name="x">The container object to check.</param>
    /// <param name="y">The object to look for within the container.</param>
    /// <returns>True if x contains y; otherwise, false.</returns>
    public virtual bool Contains(object? x, object? y)
    {
        if (x is IDictionary { Count: > 0 } xDict && y is IDictionary { Count: > 0 } yDict)
        {
            if (yDict.Keys.Except(xDict.Keys, (IEqualityComparer)this).OfType<object>().Any())
            {
                return false;
            }

            foreach (var (key, yVal) in yDict.Entries())
            {
                if (!Equals(xDict[key], yVal))
                    return false;
            }

            return true;
        }

        if (x is IDictionary { Count: > 0 } xDict1 
            && y is IEnumerable e
            && e.OfType<object>().ToList() is {Count: > 0} yList)
        {
            return !yList.Except(xDict1.Keys, this).Any();
        }

        if (x is IEnumerable e1
            && e1.OfType<object>().ToList() is { Count: > 0 } xList)
        {
            var yList1 = y is IEnumerable yArr
                ? yArr.OfType<object>().ToList()
                : [y];
            
            return yList1.Any() && !yList1.Except(xList, this).Any(); 
        }

        if (x is string { Length: > 0 } xStr && y is not null)
        {
            return xStr.IndexOf(y.ToString(), StringComparison.CurrentCulture) >= 0;
        }
        
        return false;
    }


    #region Overrided

    public new bool Equals(object? x, object? y) => Compare(x, y) == 0;
    public int GetHashCode(object? obj) => 0;
    public int Compare(T? x, T? y) => Compare(x as object, y);
    public bool Equals(T? x, T? y) => Equals(x as object, y);
    public int GetHashCode(T obj) => GetHashCode(obj as object);
    public bool Contains(T? source, T? target) => Contains(source as object, target);
    public virtual IRichComparer<T1> Cast<T1>() => new CastAdapter<T1>(this);

    #endregion
    

    class CastAdapter<T1>(IRichComparer comparer) : CommonComparer<T1>
    {
        public override int Compare(object? x, object? y)
        {
            return comparer.Compare(x, y);
        }

        public override bool Contains(object? x, object? y)
        {
            return comparer.Contains(x, y);
        }

        public override IRichComparer<T11> Cast<T11>()
        {
            return new CastAdapter<T11>(comparer);
        }
    }

}