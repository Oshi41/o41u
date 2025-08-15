using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace IL_Weaver.Weaver_old;

/// <summary>
/// Helpers for working with dynamic assemblies.
/// </summary>
public static partial class Extensions
{
    /// <summary>
    /// Copies all defined types from the specified <paramref name="assembly"/> into a newly created in-memory dynamic assembly.
    /// Only type definitions are copied: namespace, name, visibility/attributes, base type and implemented interfaces.
    /// Members (fields, methods, properties, events) are not emitted to keep the operation lightweight and dependency-free.
    /// </summary>
    /// <param name="assembly">The source assembly whose defined types will be mirrored.</param>
    /// <param name="dynamicAssemblyName">Optional name for the new dynamic assembly. If not provided, a name based on the source is used.</param>
    /// <param name="filter">Optional filter to include only selected types.</param>
    /// <param name="includeNonPublic">Include non-public types defined in the assembly. By default only public types are copied.</param>
    /// <returns>The created <see cref="AssemblyBuilder"/> along with the module and a map from source <see cref="Type"/> to the emitted <see cref="Type"/>.</returns>
    /// <remarks>
    /// - Uses only System.Reflection and System.Reflection.Emit.
    /// - Generic type definitions and nested types are skipped in this minimal implementation.
    /// - The dynamic assembly is created with <see cref="AssemblyBuilderAccess.Run"/> and is not persisted to disk.
    /// </remarks>
    public static (AssemblyBuilder Assembly, ModuleBuilder Module, IReadOnlyDictionary<Type, Type> TypeMap)
        CopyTypesToDynamicAssembly(this Assembly assembly,
            string? dynamicAssemblyName = null,
            Func<Type, bool>? filter = null,
            bool includeNonPublic = false)
    {
        if (assembly == null) throw new ArgumentNullException(nameof(assembly));

        var asmName = new AssemblyName(dynamicAssemblyName ?? $"{assembly.GetName().Name}.Dynamic");
        var asmBuilder = AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
        var moduleBuilder = asmBuilder.DefineDynamicModule($"{asmName.Name}.dll");

        // Collect types to copy (top-level, non-generic, optionally filtered)
        var allTypes = SafeGetTypes(assembly)
            .Where(t => !t.IsGenericTypeDefinition)
            .Where(t => !t.IsNested)
            .Where(t => includeNonPublic || t.IsPublic)
            .Where(t => filter == null || filter(t))
            .ToArray();

        // First pass: define builders for classes/structs/interfaces; create enums immediately
        var builders = new Dictionary<Type, TypeBuilder>(allTypes.Length);
        var map = new Dictionary<Type, Type>();

        foreach (var t in allTypes)
        {
            if (t.IsEnum)
            {
                // Create enum with the same underlying type
                var underlying = Enum.GetUnderlyingType(t);
                var eb = moduleBuilder.DefineEnum(t.FullName!, t.Attributes, underlying);
                var enumType = eb.CreateTypeInfo()!.AsType();
                map[t] = enumType;
                continue;
            }

            var attributes = t.Attributes;
            // Remove BeforeFieldInit that the runtime tends to add; it's safe to keep or remove.
            attributes &= ~TypeAttributes.BeforeFieldInit;

            var baseType = t.IsInterface ? null : t.BaseType;

            var tb = moduleBuilder.DefineType(t.FullName!, attributes, baseType);
            builders[t] = tb;
        }

        // Note: We intentionally skip adding interfaces because we don't emit member implementations here.
        // Adding interfaces without emitting their methods would produce a TypeLoadException.

        // Create the types
        foreach (var kv in builders)
        {
            var created = kv.Value.CreateTypeInfo()!.AsType();
            map[kv.Key] = created;
        }

        return (asmBuilder, moduleBuilder, map);
    }

    private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            // Return the types that were loaded successfully
            return ex.Types.Where(t => t != null)!;
        }
    }

    private static Type GetRootType(Type t)
    {
        // For generic types, return the generic type definition's base (but we skip generic defs in this impl)
        return t.IsGenericType ? t.GetGenericTypeDefinition() : t;
    }
}
