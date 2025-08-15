using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace lib.Extensions;

public static partial class Extensions
{
    /// <summary>
    /// Searches for files in directory
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="searchGlobs">Glob search pattern
    /// *.exe, **/*, dir/**/*.exe
    /// </param>
    /// 
    /// <returns></returns>
    public static List<FileInfo> ListFiles(this DirectoryInfo dir, params string[] searchGlobs)
    {
        var matcher = new Matcher();
        searchGlobs.ForEach(x => matcher.AddInclude(x));
        
        return matcher
            .Execute(new DirectoryInfoWrapper(dir))
            .Files
            .Select(x => Path.Combine(dir.FullName, x.Path))
            .Distinct()
            .OrderBy(x => x)
            .Select(x => new FileInfo(x))
            .ToList();
    }

    public static string? ReadAllText(this FileInfo file)
    {
        if (file?.Exists == true)
        {
            try
            {
                return File.ReadAllText(file.FullName);
            }
            catch (Exception e)
            {
                Log.Warning("Error during file content reading: {e}", e);
            }
        }

        return null;
    }
}