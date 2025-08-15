using System;
using System.Runtime.CompilerServices;

namespace lib.Helpers;

public record GuardResult
{
    private string? _message = string.Empty;
    public string? GuardName { get; }
    public readonly bool Ok;

    // success
    public GuardResult([CallerMemberName] string? guardName = null) 
        : this("", guardName)
    {
    }

    // Error occurred
    public GuardResult(string err, [CallerMemberName] string? guardName = null)
    {
        GuardName = guardName;
        Ok = string.IsNullOrEmpty(err);
        _message  = err;
    }

    public GuardResult CheckAndThrow()
    {
        if (!Ok)
            throw new Exception(_message);
        
        return this;
    }


    #region operators

    public static implicit operator GuardResult(bool f) => new();
    public static implicit operator GuardResult(string error) => new(error);
    public static implicit operator bool(GuardResult f) => f.Ok;

    #endregion
}