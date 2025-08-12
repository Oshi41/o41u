using System;

namespace lib.Attributes;

public enum Lifetime
{
    Singleton,
    Scoped,
    Transient,
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class RegistrationAttribute : Attribute
{
    public Type As { get; set; }
    public Lifetime Lifetime { get; set; }
}