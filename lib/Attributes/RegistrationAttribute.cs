using System;

namespace lib.Attributes;

/// <summary>
/// Service registration lifetime, similar to common DI containers.
/// </summary>
public enum Lifetime
{
    /// <summary>Single instance for the entire application lifetime.</summary>
    Singleton,
    /// <summary>One instance per logical scope.</summary>
    Scoped,
    /// <summary>New instance per request/resolve.</summary>
    Transient,
}

/// <summary>
/// Marks a class for registration in a DI container.
/// </summary>
/// <remarks>
/// Usage example:
/// <code>
/// [Registration(As = typeof(IMyService), Lifetime = Lifetime.Singleton)]
/// public class MyService : IMyService { }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class RegistrationAttribute : Attribute
{
    /// <summary>The service contract type to register the class as.</summary>
    public Type As { get; set; }
    /// <summary>The desired registration lifetime.</summary>
    public Lifetime Lifetime { get; set; }
}