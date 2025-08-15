#nullable enable
using System;
using System.IO;

namespace lib.Helpers;

public static class Directory
{
    public static bool Exists(string path) => System.IO.Directory.Exists(path);
    public static System.IO.DirectoryInfo? Ensure(string? path = null)
    {
        try
        {
            path ??= System.IO.Directory.GetCurrentDirectory();
            path = Path.GetFullPath(path);
            
            if (!Exists(path)) System.IO.Directory.CreateDirectory(path);
            
            return new DirectoryInfo(path);
        }
        catch (Exception e)
        {
            Log.Warning("Error during file path checking: {0}", e);
        }

        return null;
    }
    public static DirectoryInfo Current => new(System.IO.Directory.GetCurrentDirectory());
}