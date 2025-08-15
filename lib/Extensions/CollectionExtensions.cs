using System.Collections.Generic;
using System.Linq;

namespace lib.Extensions;

public static partial class Extensions
{
    public static T? Shift<T>(this ICollection<T> src)
    {
        if (src.Any())
        {
            var item = src.ElementAt(0);
            src.Remove(item);
            return item;
        }

        return default(T);
    }
    
    public static T? Pop<T>(this ICollection<T> src)
    {
        if (src.Any())
        {
            var item = src.Last();
            src.Remove(item);
            return item;
        }

        return default(T);
    }
}