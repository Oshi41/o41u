using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using lib.Helpers;

namespace lib.Extensions;

/// <summary>
/// General-purpose extension methods used across the library.
/// </summary>
public static partial class Extensions
{
    /// <summary>
    /// Attempts to get a value from a dictionary by key without throwing if the key is missing.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary key.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary value.</typeparam>
    /// <param name="map">The dictionary to read from.</param>
    /// <param name="key">The key to locate.</param>
    /// <returns>The value associated with <paramref name="key"/>, or the default value for <typeparamref name="TValue"/> if not found.</returns>
    public static TValue SafeGet<TKey, TValue>(this IDictionary<TKey, TValue> map, TKey key) =>
        map.TryGetValue(key, out var val) ? val : default;

    /// <summary>
    /// Executes a function and returns its result, logging any thrown exception and returning default instead.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <returns>The function result, or default when an exception occurs.</returns>
    public static T SafeLoad<T>(Func<T> func) => SafeLoad(func, () => default);

    /// <summary>
    /// Executes a function and returns its result; if it throws, logs the error and executes <paramref name="onCatch"/>.
    /// Any exception thrown by <paramref name="onCatch"/> is also logged and default is returned.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="func">The primary function to execute.</param>
    /// <param name="onCatch">A fallback function to execute when <paramref name="func"/> throws.</param>
    /// <returns>The result of <paramref name="func"/>, or the result of <paramref name="onCatch"/>, or default if both fail.</returns>
    public static T? SafeLoad<T>(Func<T> func, Func<T> onCatch)
    {
        try
        {
            return func();
        }
        catch (Exception e)
        {
            Log.Warning("Error during func call: {e}", e);
        }

        try
        {
            return onCatch();
        }
        catch (Exception e)
        {
            Log.Warning("Error during onCatch func call: {e}", e);
            return default;
        }
    }
    
}