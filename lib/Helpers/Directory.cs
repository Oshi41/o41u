#nullable enable
using System;
using System.IO;

namespace lib.Helpers;

/// <summary>
/// Provides utility methods for directory operations.
/// </summary>
public static class Directory
{
    /// <summary>
    /// Checks if a directory exists at the specified path.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>True if the directory exists; otherwise, false.</returns>
    public static bool Exists(string path) => System.IO.Directory.Exists(path);
    
    /// <summary>
    /// Ensures a directory exists at the specified path, creating it if it doesn't exist.
    /// </summary>
    /// <param name="path">The directory path to ensure. If null, uses current directory.</param>
    /// <returns>A DirectoryInfo object for the directory, or null if an error occurs.</returns>
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
    
    /// <summary>
    /// Gets a DirectoryInfo object representing the current working directory.
    /// </summary>
    public static DirectoryInfo Current => new(System.IO.Directory.GetCurrentDirectory());
}