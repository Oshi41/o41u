using System;
using System.Collections;
using System.Collections.Generic;

namespace lib.Services;

/// <summary>
/// Extends standard comparer/equality contracts with containment checks and safe generic casting.
/// </summary>
public interface IRichComparer: IComparer, IEqualityComparer
{
    /// <summary>
    /// Determines whether <paramref name="x"/> contains <paramref name="y"/>.
    /// </summary>
    /// <remarks>
    /// Supported scenarios are implementation-specific, but typically include:
    /// - Dictionaries: whether all keys/values of <paramref name="y"/> are present in <paramref name="x"/>.
    /// - Sequences: whether <paramref name="y"/> element(s) appear within <paramref name="x"/>.
    /// - Strings: substring check.
    /// </remarks>
    bool Contains(object? x, object? y);
    
    /// <summary>
    /// Returns a typed adapter over this comparer for <typeparamref name="T1"/>.
    /// </summary>
    IRichComparer<T1> Cast<T1>();
}

/// <summary>
/// Typed variant of <see cref="IRichComparer"/>.
/// </summary>
public interface IRichComparer<in T> : IRichComparer, IComparer<T>, IEqualityComparer<T>
{
    /// <summary>
    /// Determines whether <paramref name="source"/> contains <paramref name="target"/> using type-aware rules.
    /// </summary>
    bool Contains(T? source, T? target);
}
