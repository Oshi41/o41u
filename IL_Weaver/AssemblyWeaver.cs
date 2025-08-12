using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace lib.Model;

public class AssemblyWeaver 
{
    private readonly Assembly _original;
    
    private readonly AssemblyName _name;
    private readonly AssemblyBuilder _builder;
    private readonly ModuleBuilder _module;


    public AssemblyWeaver(string assemblyPath)
    {
        // AppDomain.CurrentDomain.TypeResolve += HandleTypeResolve;
        // AppDomain.CurrentDomain.AssemblyResolve += ResolveFromLocalFolder;
        //
        // _original = AppDomain.CurrentDomain.Load(assemblyPath);
        //
        // AppDomain.CurrentDomain.AssemblyResolve += ResolveFromLocalFolder;
        // AppDomain.CurrentDomain.TypeResolve -= HandleTypeResolve;
        
        AppDomain.CurrentDomain.SetData("loadFromRemoteSources", "True");
        
        _original = Assembly.LoadFile(assemblyPath);
        _name = new AssemblyName(_original.FullName!);

        _builder = AssemblyBuilder.DefineDynamicAssembly(_name, AssemblyBuilderAccess.RunAndCollect);
        _module = _builder.DefineDynamicModule(_original.ManifestModule.Name);

    }

    // private Assembly HandleTypeResolve(object sender, ResolveEventArgs args)
    // {
    //     Log.Debug("Resolving Type {0}", args.Name);
    //     
    //     return args.RequestingAssembly;
    // }
    //
    // private Assembly ResolveFromLocalFolder(object sender, ResolveEventArgs args)
    // {
    //     if (File.Exists(args.Name))
    //     {
    //         
    //         var assembly = Assembly.LoadFrom(args.Name);
    //         return assembly;
    //     }
    //     
    //     
    //     return args.RequestingAssembly;
    //
    //     // try
    //     // {
    //     //     var requested = new AssemblyName(args.Name);
    //     //
    //     //     // Determine the folder where this library resides, fallback to AppContext base dir
    //     //     var assemblyLocation = Assembly.GetExecutingAssembly().Location;
    //     //     var baseDir = !string.IsNullOrEmpty(assemblyLocation)
    //     //         ? Path.GetDirectoryName(assemblyLocation)
    //     //         : AppContext.BaseDirectory;
    //     //
    //     //     if (string.IsNullOrEmpty(baseDir)) return null;
    //     //
    //     //     // Try simple {Name}.dll next to this library
    //     //     var candidate = Path.Combine(baseDir, requested.Name + ".dll");
    //     //     if (File.Exists(candidate))
    //     //     {
    //     //         return Assembly.LoadFrom(candidate);
    //     //     }
    //     //
    //     //     // Also try probing subfolders commonly used (e.g., 'runtimes', 'lib')
    //     //     var subdirs = new[]
    //     //     {
    //     //         "", "runtimes", Path.Combine("runtimes", "win"), Path.Combine("runtimes", "win-x64"), "lib"
    //     //     };
    //     //
    //     //     foreach (var sub in subdirs)
    //     //     {
    //     //         var path = string.IsNullOrEmpty(sub)
    //     //             ? candidate
    //     //             : Path.Combine(baseDir, sub, requested.Name + ".dll");
    //     //
    //     //         if (File.Exists(path))
    //     //         {
    //     //             return Assembly.LoadFrom(path);
    //     //         }
    //     //     }
    //     // }
    //     // catch
    //     // {
    //     //     // ignore, let default resolution continue
    //     // }
    //     //
    //     // return null;
    // }
}