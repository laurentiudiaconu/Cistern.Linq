﻿using System;
using System.Collections.Generic;

namespace Cistern.Linq.ChainLinq.Optimizations
{
    interface IHeadStart<T>
    {
        void Execute(ReadOnlySpan<T> source);
        void Execute<Enumerable, Enumerator>(Enumerable source)
            where Enumerable : ITypedEnumerable<T, Enumerator>
            where Enumerator : IEnumerator<T>;
    }

    interface ITailEnd<T>
    {
        void Select<S>(ReadOnlySpan<S> source, Func<S, T> selector);

        void Where(ReadOnlySpan<T> source, Func<T, bool> predicate);
        void Where<Enumerable, Enumerator>(Enumerable source, Func<T, bool> predicate)
            where Enumerable : ITypedEnumerable<T, Enumerator>
            where Enumerator : IEnumerator<T>;

        ChainStatus SelectMany<TSource, TCollection>(TSource source, ReadOnlySpan<TCollection> span, Func<TSource, TCollection, T> resultSelector);

        void WhereSelect<S>(ReadOnlySpan<S> source, Func<S, bool> predicate, Func<S, T> selector);
        void WhereSelect<Enumerable, Enumerator, S>(Enumerable source, Func<S, bool> predicate, Func<S, T> selector)
            where Enumerable : ITypedEnumerable<S, Enumerator>
            where Enumerator : IEnumerator<S>;

    }
}
