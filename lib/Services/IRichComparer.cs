using System;
using System.Collections;
using System.Collections.Generic;

namespace lib.Services;

public interface IRichComparer: IComparer, IEqualityComparer
{
    bool Contains(object? x, object? y);
    
    IRichComparer<T1> Cast<T1>();
}

public interface IRichComparer<in T> : IRichComparer, IComparer<T>, IEqualityComparer<T>
{
    bool Contains(T? source, T? target);
}
