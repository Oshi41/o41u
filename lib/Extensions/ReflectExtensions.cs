using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace lib.Extensions;

public static partial class Extensions
{
    // ReSharper disable once InconsistentNaming
    private static readonly BindingFlags ALL = BindingFlags.Public |
                                               BindingFlags.Static |
                                               BindingFlags.Instance |
                                               BindingFlags.NonPublic;

    public static bool TryGetProp<T>(this object owner, string propertyName, out T result)
    {
        result = default;

        if (owner?.GetType() is { } type)
        {
            var propInfo = SafeLoad(() => type.GetProperty(propertyName, ALL));
            if (propInfo is { CanRead: true } prop)
            {
                object val;
                try
                {
                    val = prop.GetValue(owner);
                }
                catch (Exception ex)
                {
                    Log.Warning("Error during property getter: {ex}", ex);
                    return false;
                }

                if (val is T converted)
                {
                    result = converted;
                    return true;
                }
            }
        }

        return false;
    }

    public static IEnumerable<Type> LoadAllTypes(this Module mod)
    {
        if (mod != null)
        {
            try
            {
                return mod.GetTypes().Where(x => x != null).ToList();
            }
            catch (ReflectionTypeLoadException ex)
            {
                Log.Warning("Error during obtaining the types: {e}", ex);
                // Add the types that were successfully loaded
                return ex.Types.Where(t => t != null).ToList();
            }
            catch (Exception e)
            {
                // Skip assemblies that can't be loaded or reflected on
                Log.Warning("Error during obtaining the types: {e}", e);
            }
        }

        return [];
    }

    public static IEnumerable<Type> LoadAllTypes(this Assembly assembly)
    {
        if (assembly != null)
        {
            foreach (var mod in SafeLoad(assembly.GetModules))
            {
                foreach (var t in LoadAllTypes(mod))
                {
                    yield return t;
                }
            }
        }
    }

    public static IEnumerable<Type> LoadAllTypes(this AppDomain domain)
    {
        if (domain != null)
        {
            foreach (var assembly in SafeLoad(domain.GetAssemblies).Where(x => x != null))
            {
                foreach (var t in LoadAllTypes(assembly))
                {
                    yield return t;
                }
            }
        }
    }

    public static IEnumerable<Type> GetDependants(this Type type)
    {
        if (type == null) yield break;

        if (type.IsEnum && type.GetEnumUnderlyingType() is { } enumType)
        {
            foreach (var t in IterateThroughItem(enumType))
            {
                yield return t;
            }
        }

        if (type is { BaseType: { } baseType })
        {
            foreach (var t in IterateThroughItem(baseType))
            {
                yield return t;
            }
        }

        foreach (var @interface in type.GetInterfaces())
        {
        }


        IEnumerable<Type> IterateThroughItem(Type item)
        {
            if (item != null)
                yield return item;

            foreach (var child in GetDependants(item))
            {
                yield return child;
            }
        }
    }
}