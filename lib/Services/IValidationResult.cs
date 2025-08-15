#nullable enable
using System;
using lib.Enums;

namespace lib.Services;

public interface IValidationResult<out T>
{
    ValidationResultType Status { get; }
    T Item { get; }
    Exception? Error { get; }
}