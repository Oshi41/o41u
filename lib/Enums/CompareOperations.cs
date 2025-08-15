using System;

namespace lib.Enums;

/// <summary>
/// Describes the result or expectation of a comparison between two values.
/// </summary>
/// <remarks>
/// Although marked with <see cref="FlagsAttribute"/>, values are not powers of two. Treat it as a simple tri-state.
/// </remarks>
[Flags]
public enum CompareOperations
{
    /// <summary>Left equals right.</summary>
    Equals,
    /// <summary>Left is greater than right.</summary>
    More,
    /// <summary>Left is less than right.</summary>
    Less,
}