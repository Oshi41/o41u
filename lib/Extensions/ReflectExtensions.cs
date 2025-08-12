using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace lib.Extensions;

public static partial class Extensions
{
    /// <summary>
    /// Safely loads all available types from the assemblies currently loaded into the AppDomain.
    /// </summary>
    /// <returns>An enumerable of all successfully loaded <see cref="Type"/> instances.
    /// In case of partial failures (e.g., <see cref="ReflectionTypeLoadException"/>), the types that were loaded successfully are included.
    /// Any errors are logged and the method continues processing remaining assemblies.</returns>
    public static IEnumerable<Type> LoadAllTypes()
    {
        var assemblies = SafeLoad(AppDomain.CurrentDomain.GetAssemblies);

        foreach (var assembly in assemblies)
        {
            var types = new List<Type>();
            try
            {
                types.AddRange(assembly.GetTypes());
            }
            catch (ReflectionTypeLoadException ex)
            {
                Log.Warning("Error during obtaining the types: {e}", ex);
                // Add the types that were successfully loaded
                types.AddRange(ex.Types.Where(t => t != null));
            }
            catch (Exception e)
            {
                Log.Warning("Error during obtaining the types: {e}", e);
                // Skip assemblies that can't be loaded or reflected on
                continue;
            }

            foreach (var type in types)
            {
                yield return type;
            }
        }
    }

    
    public static IEnumerable<Type> GetDependants(this Type type)
    {
        if (type == null) yield break;

        if (type.IsEnum && type.GetEnumUnderlyingType() is {} enumType)
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