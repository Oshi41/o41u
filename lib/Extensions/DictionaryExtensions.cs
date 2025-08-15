using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace lib.Extensions;

public static partial class Extensions
{
    public static bool ContainsValue(this IDictionary dict, object item, IEqualityComparer? comparer = null)
    {
        return dict.Count > 0 && dict.Values.OfType<object>().Contains(item, comparer);
    }

    public static IEnumerable<(object, object)> Entries(this IDictionary dict)
    {
        foreach (var key in dict.Keys)
        {
            yield return (key, dict[key]);
        }
    }
}