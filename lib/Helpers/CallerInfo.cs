using System.Diagnostics;
using System.Reflection;

namespace lib.Helpers;

/// <summary>
/// Utilities for retrieving caller information beyond what CallerMemberName can provide.
/// </summary>
public static class CallerInfo
{
    /// <summary>
    /// Returns the member name N frames up the call stack relative to the method that calls this API.
    /// For example, framesUp = 0 -> the immediate caller; framesUp = 1 -> caller of the caller ("one frame ago").
    /// </summary>
    /// <param name="framesUp">How many frames above the immediate caller to inspect. 1 means caller of the caller.</param>
    /// <returns>The member name if available; otherwise null.</returns>
    public static string? GetMemberName(int framesUp = 1)
    {
        // Skip this method frame (1) plus the requested framesUp.
        var skip = 1 + (framesUp < 0 ? 0 : framesUp);
        var st = new StackTrace(skipFrames: skip, fNeedFileInfo: false);
        var mb = st.GetFrame(0)?.GetMethod();
        return GetMethodDisplayName(mb);
    }

    private static string? GetMethodDisplayName(MethodBase? method)
    {
        if (method == null) return null;
        // Use simple Name to match CallerMemberName behavior; do not include type by default.
        return method.Name;
    }
}
