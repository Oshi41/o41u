using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace lib.Services;

internal static class AssemblyResolver
{
    public static void Init()
    {
        try
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveFromLocalFolder;
        }
        catch
        {
            // Swallow: resolution fallback is best-effort
        }
    }

    private static Assembly ResolveFromLocalFolder(object sender, ResolveEventArgs args)
    {
        try
        {
            var requested = new AssemblyName(args.Name);

            // Determine the folder where this library resides, fallback to AppContext base dir
            var assemblyLocation = typeof(AssemblyResolver).Assembly.Location;
            var baseDir = !string.IsNullOrEmpty(assemblyLocation)
                ? Path.GetDirectoryName(assemblyLocation)
                : AppContext.BaseDirectory;

            if (string.IsNullOrEmpty(baseDir)) return null;

            // Try simple {Name}.dll next to this library
            var candidate = Path.Combine(baseDir, requested.Name + ".dll");
            if (File.Exists(candidate))
            {
                return Assembly.LoadFrom(candidate);
            }

            // Also try probing subfolders commonly used (e.g., 'runtimes', 'lib')
            var subdirs = new[]
            {
                "", "runtimes", Path.Combine("runtimes", "win"), Path.Combine("runtimes", "win-x64"), "lib"
            };

            foreach (var sub in subdirs)
            {
                var path = string.IsNullOrEmpty(sub)
                    ? candidate
                    : Path.Combine(baseDir, sub, requested.Name + ".dll");

                if (File.Exists(path))
                {
                    return Assembly.LoadFrom(path);
                }
            }
        }
        catch
        {
            // ignore, let default resolution continue
        }

        return null;
    }
}
