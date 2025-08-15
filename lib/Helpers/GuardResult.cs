using System;
using System.Runtime.CompilerServices;

namespace lib.Helpers;

public record GuardResult
{
    private string? _message = string.Empty;
    public string? GuardName { get; }
    public readonly bool Ok;

    protected GuardResult()
    {
        Ok = true;
        GuardName = string.Empty;
    }

    protected GuardResult(string? guardName, string? error = null)
    {
        Ok = false;
        GuardName = guardName;
        _message = error ?? GetDefaultErrorMessage(GuardName);
    }

    private static string GetDefaultErrorMessage(string? guard)
    {
        switch (guard)
        {
            case nameof(Guard.IsNull):
                return "Value must be null";
            
            case nameof(Guard.IsZero):
                return "Value must be o";
            
            case nameof(Guard.IsPositive):
                return "Value should be positive (> 0)";
            
            case nameof(Guard.IsNegative):
                return "Value should be negative (< 0)";
            
            case nameof(Guard.HasFraction):
                return "Value should be a fractured number";
            
            case nameof(Guard.IsEmpty):
                return "Value should be empty";
            
            case nameof(Guard.IsNotEmpty):
                return "Value should not be empty";
            
            case nameof(Guard.Same):
                return "Values must be equal";
            
            case nameof(Guard.More):
                return "Left should be bigger than right (l > r)";
            
            case nameof(Guard.Less):
                return "Left should be less than right (l < r)";
            
            case nameof(Guard.Contains):
                return "Left should contain right (l.Contains(r))";
            
            case nameof(Guard.SameType):
                return "Values should have the same type";
            
            default:
                return string.Empty;
        }
    }

    public GuardResult CheckAndThrow()
    {
        if (!Ok)
            throw new Exception(_message);
        
        return this;
    }

    #region operators

    public static implicit operator GuardResult(bool f)
    {
        if (f) return new();
        
        return new(CallerInfo.GetMemberName(2));
    }
    public static implicit operator GuardResult(string errorMessage)
    {
        return new(CallerInfo.GetMemberName(2), errorMessage);
    }
    public static implicit operator bool(GuardResult f) => f.Ok;

    #endregion
}