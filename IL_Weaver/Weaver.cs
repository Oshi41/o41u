using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using lib.Extensions;
using lib.Helpers;
using Serilog;

namespace IL_Weaver;

public class IlWeaver
{
    #region Static

    private static string ValidateParameters(string filepath)
    {
        Guard.IsNotEmpty(new FileInfo(filepath));

        // Allow dynamic code if the host honors the switch (helpful under AOT-restricted hosts)
        var switchName = "System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported";
        if (!AppContext.TryGetSwitch(switchName, out var disabled) || !disabled)
        {
            Log.Information("Enabling dynamic code support ");
            AppContext.SetSwitch(switchName, true);
        }

        return Path.GetFullPath(filepath);
    }

    #endregion

    #region Fields

    private readonly Assembly _source;
    private readonly string _sourcePath;

    #endregion

    public IlWeaver(string filepath)
    {
        _sourcePath = ValidateParameters(filepath);
        _source = Assembly.LoadFrom(_sourcePath);
    }

    public IEnumerable<Type> AllTypes => _source.LoadAllTypes();

    /// <summary>
    /// Replace the body of the specified method with a default return and save to outputPath.
    /// Only supports simple methods without EH sections. Returns false with reason if unsupported.
    /// </summary>
    public bool TryReplaceMethodBodyWithDefault(string typeFullName, string methodName, string outputPath, out string? reason)
        => MethodPatcher.TryReplaceWithDefault(_sourcePath, outputPath, typeFullName, methodName, out reason);

    /// <summary>
    /// Overload accepting a System.Type (top-level types only).
    /// </summary>
    public bool TryReplaceMethodBodyWithDefault(Type type, string methodName, string outputPath, out string? reason)
    {
        var fullName = string.IsNullOrEmpty(type.Namespace) ? type.Name : type.Namespace + "." + type.Name;
        return TryReplaceMethodBodyWithDefault(fullName, methodName, outputPath, out reason);
    }
}