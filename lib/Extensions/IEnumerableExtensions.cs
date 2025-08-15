using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace lib.Extensions;

public static partial class Extensions
{
    #region ForEach

    public static void ForEach<T>(this IEnumerable<T>? src, Action<T, int>? action)
    {
        if (src == null || action == null)
            return;

        var index = 0;
        foreach (var item in src)
        {
            action(item, index);
            index++;
        }
    }

    public static void ForEach<T>(this IEnumerable? src, Action<T, int> action) => src?.OfType<T>().ForEach(action);

    public static void ForEach(this IEnumerable? src, Action<object, int> action) =>
        src?.OfType<object>().ForEach(action);

    public static void ForEach<T>(this IEnumerable<T>? src, Action<T> action) =>
        src?.ForEach((arg1, _) => action(arg1));

    public static void ForEach(this IEnumerable? src, Action<object> action) =>
        src?.OfType<object>()?.ForEach((arg1, _) => action(arg1));

    public static void ForEach<T>(this IEnumerable? src, Action<T> action) =>
        src?.OfType<T>().ForEach((arg1, _) => action(arg1));

    #endregion

    #region Except

    public static IEnumerable<T> Except<T>(this IEnumerable<T>? src, IEnumerable<T>? other,
        IEqualityComparer<T>? comparer = null)
    {
        return src != null && other != null
            ? System.Linq.Enumerable.Except(src, other, comparer)
            : [];
    }

    public static IEnumerable<T> Except<T>(this IEnumerable<T>? src, IEnumerable<T>? other,
        IEqualityComparer? comparer = null)
    {
        return Except(src, other, (IEqualityComparer<T>)CommonComparer.Cast<T>());
    }

    public static IEnumerable<T> Except<T>(this IEnumerable<T>? src, IEnumerable? other,
        IEqualityComparer<T>? comparer = null)
    {
        return Except(src, other?.OfType<T>(), comparer);
    }

    public static IEnumerable<T> Except<T>(this IEnumerable<T>? src, IEnumerable? other,
        IEqualityComparer? comparer = null)
    {
        return Except(src, other?.OfType<T>(), CommonComparer);
    }

    public static IEnumerable<T> Except<T>(this IEnumerable? src, IEnumerable? other,
        IEqualityComparer? comparer = null)
    {
        return Except(src?.OfType<T>(), other?.OfType<T>(), CommonComparer);
    }

    public static IEnumerable<T> Except<T>(this IEnumerable? src, IEnumerable? other,
        IEqualityComparer<T>? comparer = null)
    {
        return Except(src?.OfType<T>(), other?.OfType<T>(), comparer);
    }

    public static IEnumerable Except(this IEnumerable? src, IEnumerable? other, IEqualityComparer? comparer = null)
    {
        return Except(src?.OfType<object>(), other?.OfType<object>(), comparer);
    }

    #endregion

    #region ExceptBy

    public static IEnumerable<T> ExceptBy<T, TKey>(this IEnumerable<T>? src, IEnumerable<T>? other,
        Func<T, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
    {
        if (src == null || other == null)
            yield break;

        var mapped = other.Select(keySelector).ToImmutableHashSet(comparer);

        foreach (var item in src)
        {
            var key = keySelector(item);
            if (!mapped.Contains(key))
                yield return item;
        }
    }

    public static IEnumerable ExceptBy(this IEnumerable? src, IEnumerable? other, Func<object, object> keySelector,
        IEqualityComparer? comparer = null)
    {
        return ExceptBy(src?.OfType<object>(), other?.OfType<object>(), keySelector, CommonComparer.Cast<object>());
    }

    public static IEnumerable<T> ExceptBy<T, TKey>(this IEnumerable? src, IEnumerable? other, Func<T, TKey> keySelector,
        IEqualityComparer? comparer = null)
    {
        Func<object, object?> keySelectorFixed = o => o is T item ? keySelector(item) : 0;
        return ExceptBy(src, other, keySelectorFixed, comparer).OfType<T>();
    }

    public static IEnumerable<T> ExceptBy<T>(this IEnumerable? src, IEnumerable? other,
        Func<object, object> keySelector, IEqualityComparer? comparer = null)
    {
        return ExceptBy(src?.OfType<object>(), other?.OfType<object>(), keySelector, CommonComparer.Cast<object>())
            .OfType<T>();
    }

    public static IEnumerable ExceptBy<T>(this IEnumerable<T>? src, IEnumerable? other,
        Func<object, object> keySelector, IEqualityComparer? comparer = null)
    {
        return ExceptBy(src?.OfType<object>(), other?.OfType<object>(), keySelector, CommonComparer.Cast<object>())
            .OfType<T>();
    }

    public static IEnumerable ExceptBy<T>(this IEnumerable? src, IEnumerable<T>? other,
        Func<object, object> keySelector, IEqualityComparer? comparer = null)
    {
        return ExceptBy(src?.OfType<object>(), other?.OfType<object>(), keySelector, CommonComparer.Cast<object>())
            .OfType<T>();
    }

    public static IEnumerable ExceptBy<T>(this IEnumerable? src, IEnumerable? other, Func<T, object> keySelector,
        IEqualityComparer? comparer = null)
    {
        Func<object, object> selector = o => o is T item ? keySelector(item) : 0;

        return ExceptBy(src?.OfType<object>(), other?.OfType<object>(), selector, CommonComparer.Cast<object>())
            .OfType<T>();
    }

    public static IEnumerable ExceptBy<T>(this IEnumerable? src, IEnumerable? other, Func<object, object> keySelector,
        IEqualityComparer<T>? comparer = null)
    {
        Func<object, object> selector = o => o is T item ? keySelector(item) : 0;

        return ExceptBy(src?.OfType<object>(), other?.OfType<object>(), selector, CommonComparer.Cast<object>())
            .OfType<T>();
    }

    #endregion

    #region Any

    public static bool Any(this IEnumerable? src, Func<object, bool>? predicate = null)
    {
        return src != null && Enumerable.Any(src.OfType<object>(), predicate ?? (_ => true));
    }

    public static bool Any<T>(this IEnumerable? src, Func<T, bool>? predicate = null)
    {
        return src.Any(Predicate);

        bool Predicate(object arg)
        {
            return predicate == null || (arg is T argument && predicate(argument));
        }
    }

    #endregion

    #region ToDictionary

    public static IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<TValue>? src, Func<TValue, TKey> keySelector)
    {
        var result = new Dictionary<TKey, TValue>();

        src?.ForEach(item =>
        {
            if (keySelector(item) is not { } key)
            {
                Log.Warning("Null key during ToDictionary call: {e}", item);
                return;
            }

            if (result.TryGetValue(key, out var oldValue))
            {
                Log.Warning("Overwriting existing key [{key}]: {old}->{cur}", key, oldValue, item);
            }

            result[key] = item;
        });

        return result;
    }

    #endregion

    public static bool Contains<T>(this IEnumerable<T> src, object item, IEqualityComparer? compare = null)
    {
        compare ??= CommonComparer;
        return src.Any(x => compare.Equals(x, item));
    }
}