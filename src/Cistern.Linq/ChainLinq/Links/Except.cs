﻿using System.Collections.Generic;

namespace Cistern.Linq.ChainLinq.Links
{
    sealed class Except<T> : Link<T, T>
    {
        private readonly IEqualityComparer<T> _comparer;
        private readonly IEnumerable<T> _second;

        public Except(IEqualityComparer<T> comparer, IEnumerable<T> second) =>
            (_comparer, _second) = (comparer, second);

        public override Chain<T> Compose(Chain<T> activity) =>
            new Activity(_comparer, _second, activity);

        sealed class Activity : Activity<T, T>
        {
            private Set<T> _seen;

            public Activity(IEqualityComparer<T> comparer, IEnumerable<T> second, Chain<T> next) : base(next)
            {
                _seen = Utils.Consume(second, new Consumer.CreateSet<T>(comparer));
            }

            public override ChainStatus ProcessNext(T input) =>
                _seen.Add(input) ? Next(input) : ChainStatus.Filter;
        }
    }

    sealed class ExceptDefaultComparer<T> : Link<T, T>
    {
        private readonly IEnumerable<T> _second;

        public ExceptDefaultComparer(IEnumerable<T> second) =>
            _second = second;

        public override Chain<T> Compose(Chain<T> activity) =>
            new Activity(_second, activity);

        sealed class Activity : Activity<T, T>
        {
            private SetDefaultComparer<T> _seen;

            public Activity(IEnumerable<T> second, Chain<T> next) : base(next)
            {
                _seen = Utils.Consume(second, new Consumer.CreateSetDefaultComparer<T>());
            }

            public override ChainStatus ProcessNext(T input) =>
                _seen.Add(input) ? Next(input) : ChainStatus.Filter;
        }
    }

}
