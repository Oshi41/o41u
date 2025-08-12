using System.Reflection;
using lib.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace lib.Extensions;

public static partial class Extensions
{
    /// <summary>
    /// Registers all classes in loaded assemblies that are marked with <see cref="lib.Attributes.RegistrationAttribute"/>.
    /// The registration uses the lifetime specified by the attribute and, if provided, registers the service as the given interface/type via <c>As</c>.
    /// </summary>
    /// <param name="services">The service collection to register the discovered types into.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance to allow chaining.</returns>
    public static IServiceCollection RegisterTypes(this IServiceCollection services)
    {
        foreach (var type in LoadAllTypes())
        {
            foreach (var attr in type.GetCustomAttributes<RegistrationAttribute>())
            {
                _ = attr.Lifetime switch
                {
                    Lifetime.Singleton => services.AddSingleton(type, attr.As ?? type),
                    Lifetime.Scoped => services.AddScoped(type, attr.As ?? type),
                    Lifetime.Transient => services.AddTransient(type, attr.As ?? type),
                    _ => services
                };
            }
        }

        return services;
    }
}