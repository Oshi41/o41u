#nullable enable
using System;
using System.IO;

namespace lib.Helpers;

/// <summary>
/// Provides utility methods for file operations.
/// </summary>
public static class File
{
    /// <summary>
    /// Checks if a file exists at the specified path.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>True if the file exists; otherwise, false.</returns>
    public static bool Exists(string path) => System.IO.File.Exists(path);
    
    /// <summary>
    /// Ensures a file exists at the specified path, creating it with default content if it doesn't exist.
    /// </summary>
    /// <param name="path">The file path to ensure.</param>
    /// <param name="defaultContent">The default content to write if the file doesn't exist.</param>
    /// <returns>A FileInfo object for the file, or null if an error occurs.</returns>
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