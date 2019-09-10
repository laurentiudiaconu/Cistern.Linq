﻿using System;

namespace Cistern.Linq.ChainLinq.Links
{
    sealed class WhereIndexed<T>
        : Link<T, T>
    {
        public Func<T, int, bool> Predicate { get; }

        public WhereIndexed(Func<T, int, bool> predicate) =>
            Predicate = predicate;

        public override Chain<T> Compose(Chain<T> activity) =>
            new Activity(Predicate, activity);

        sealed class Activity : Activity<T, T>
        {
            private readonly Func<T, int, bool> _predicate;
            private int _index;

            public Activity(Func<T, int, bool> predicate, Chain<T> next) : base(next) =>
                (_predicate, _index) = (predicate, -1);

            public override ChainStatus ProcessNext(T input)
            {
                checked
                {
                    _index++;
                }

                return _predicate(input, _index) ? Next(input) : ChainStatus.Filter;
            }
                
        }
    }
}
