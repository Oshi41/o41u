using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using lib.Extensions;
using lib.Helpers;
using lib.Helpers.Assertions;
using Serilog;

namespace IL_Weaver;

public class Weaver
{
    #region Static

    private static string ValidateParameters(string filepath)
    {
        Guard.IsNotEmpty(filepath);

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

    #endregion

    public Weaver(string filepath)
    {
        _source = Assembly.LoadFrom(ValidateParameters(filepath));
    }

    public IEnumerable<Type> AllTypes => _source.LoadAllTypes();
}