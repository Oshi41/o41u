using System.Collections;
using System.Linq;

namespace lib.Extensions;

public static partial class Extensions
{
    public static bool IsNullOrEmpty(this string s) => string.IsNullOrEmpty(s);

    public static string Join(this IEnumerable s, string separator) => string.Join(separator, s.OfType<object>().Select(x => x.ToString()).Where(x => x != null));
}