#nullable enable
namespace lib.Enums;

/// <summary>
/// Represents the outcome of a validation routine.
/// </summary>
public enum ValidationResultType
{
    /// <summary>Validation result is not specified.</summary>
    Unknown,
    /// <summary>Validation failed due to unmet conditions (expected but not exceptional).</summary>
    Failure,
    /// <summary>Validation passed successfully.</summary>
    Success,
    /// <summary>Validation failed due to an unexpected error (exception).</summary>
    Error,
}
