using lib.Helpers;
using lib.Services;

namespace lib.Extensions;

public static partial class Extensions
{
    public static IRichComparer CommonComparer { get; } = new CommonComparer<object>();
}