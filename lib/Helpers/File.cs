#nullable enable
using System;
using System.IO;

namespace lib.Helpers;

public static class File
{
    public static bool Exists(string path) => System.IO.File.Exists(path);
    
    public static FileInfo? Ensure(string path, string defaultContent = "")
    {
        try
        {
            var fp = Path.GetFullPath(path);
            Directory.Ensure(Path.GetDirectoryName(fp));
            
            if (!System.IO.File.Exists(fp))
                System.IO.File.WriteAllText(fp, defaultContent);
            
            return new FileInfo(fp);
        }
        catch (Exception e)
        {
            Log.Warning("Error during file path checking: {0}", e);
            return null;
        }
    }
}