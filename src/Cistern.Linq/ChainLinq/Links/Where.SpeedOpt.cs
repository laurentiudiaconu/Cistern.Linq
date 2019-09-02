﻿using System;
using System.Collections.Generic;

namespace Cistern.Linq.ChainLinq.Links
{
    internal partial class Where<T>
        : Optimizations.IMergeSelect<T>
        , Optimizations.IMergeWhere<T>
    {
        public Consumable<U> MergeSelect<U>(ConsumableCons<T> consumable, Func<T, U> selector) =>
            consumable.ReplaceTailLink(new WhereSelect<T, U>(Predicate, selector));

        public virtual Consumable<T> MergeWhere(ConsumableCons<T> consumable, Func<T, bool> second) =>
            consumable.ReplaceTailLink(new Where2<T>(Predicate, second));

        sealed partial class Activity
            : Optimizations.IHeadStart<T>
        {
            void Optimizations.IHeadStart<T>.Execute(ReadOnlySpan<T> source)
            {
                if (next is Optimizations.ITailEnd<T> optimized)
                {
                    optimized.Where(source, _predicate);
                }
                else
                {
                    foreach (var item in source)
                    {
                        if (_predicate(item))
                        {
                            var state = Next(item);
                            if (state.IsStopped())
                                break;
                        }
                    }
                }
            }

            void Optimizations.IHeadStart<T>.Execute<Enumerable, Enumerator>(Enumerable source)
            {
                if (next is Optimizations.ITailEnd<T> optimized)
                {
                    optimized.Where(source, _predicate);
                }
                else
                {
                    foreach (var item in source)
                    {
                        if (_predicate(item))
                        {
                            var state = Next(item);
                            if (state.IsStopped())
                                break;
                        }
                    }
                }
            }
        }
    }

    sealed class Where2<T> : Where<T>
    {
        private readonly Func<T, bool> _first;
        private readonly Func<T, bool> _second;

        public Where2(Func<T, bool> first, Func<T, bool> second) : base(t => first(t) && second(t)) =>
            (_first, _second) = (first, second);

        public override Consumable<T> MergeWhere(ConsumableCons<T> consumable, Func<T, bool> third) =>
            consumable.ReplaceTailLink(new Where3<T>(_first, _second, third));
    }

    sealed class Where3<T> : Where<T>
    {
        private readonly Func<T, bool> _first;
        private readonly Func<T, bool> _second;
        private readonly Func<T, bool> _third;

        public Where3(Func<T, bool> first, Func<T, bool> second, Func<T, bool> third) : base(t => first(t) && second(t) && third(t)) =>
            (_first, _second, _third) = (first, second, third);

        public override Consumable<T> MergeWhere(ConsumableCons<T> consumable, Func<T, bool> forth) =>
            consumable.ReplaceTailLink(new Where4<T>(_first, _second, _third, forth));
    }

    sealed class Where4<T> : Where<T>
    {
        public Where4(Func<T, bool> first, Func<T, bool> second, Func<T, bool> third, Func<T, bool> forth)
            : base(t => first(t) && second(t) && third(t) && forth(t)) { }
    }
}
