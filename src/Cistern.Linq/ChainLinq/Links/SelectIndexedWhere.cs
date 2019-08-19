﻿using System;

namespace Cistern.Linq.ChainLinq.Links
{
    internal sealed class SelectIndexedWhere<T, U>
        : Link<T, U>
        , Optimizations.IMergeWhere<U>
    {
        public Func<T, int, U> Selector { get; }
        public Func<U, bool> Predicate { get; }

        public SelectIndexedWhere(Func<T, int, U> selector, Func<U, bool> predicate) : base(LinkType.SelectWhere) =>
            (Selector, Predicate) = (selector, predicate);

        public override Chain<T> Compose(Chain<U> activity) =>
            new Activity(Selector, Predicate, 0, activity);

        public Consumable<U> MergeWhere(ConsumableForMerging<U> consumable, Func<U, bool> second) =>
            consumable.ReplaceTailLink(new SelectIndexedWhere<T, U>(Selector, t => Predicate(t) && second(t)));

        sealed class Activity
            : Activity<T, U>
            , Optimizations.IHeadStart<T>
        {
            private readonly Func<T, int, U> _selector;
            private readonly Func<U, bool> _predicate;

            private int _index;

            public Activity(Func<T, int, U> selector, Func<U, bool> predicate, int startIndex, Chain<U> next) : base(next)
            {
                (_selector, _predicate) = (selector, predicate);
                checked
                {
                    _index = startIndex - 1;
                }
            }

            public override ChainStatus ProcessNext(T input)
            {
                checked
                {
                    var item = _selector(input, ++_index);
                    return _predicate(item) ? Next(item) : ChainStatus.Filter;
                }
            }

            void Optimizations.IHeadStart<T>.Execute(ReadOnlySpan<T> memory)
            {
                checked
                {
                    foreach (var t in memory)
                    {
                        var u = _selector(t, ++_index);
                        if (_predicate(u))
                        {
                            var state = Next(u);
                            if (state.IsStopped())
                                break;
                        }
                    }
                }
            }

            void Optimizations.IHeadStart<T>.Execute<Enumerator>(Optimizations.ITypedEnumerable<T, Enumerator> source)
            {
                checked
                {
                    foreach (var t in source)
                    {
                        var u = _selector(t, ++_index);
                        if (_predicate(u))
                        {
                            var state = Next(u);
                            if (state.IsStopped())
                                break;
                        }
                    }
                }
            }
        }
    }
}
