namespace lib.Enums;

/// <summary>
/// Specifies how to combine multiple boolean results.
/// </summary>
public enum MultipleBoolOperations
{
    /// <summary>All conditions must be true.</summary>
    And,
    /// <summary>At least one condition must be true.</summary>
    Or,
}