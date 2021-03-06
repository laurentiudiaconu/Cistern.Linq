﻿using System;
using System.Collections.Generic;

namespace Cistern.Linq.Consumables
{
    sealed partial class Repeat<T, U> 
        : Consumable<T, U>
        , Optimizations.IConsumableFastCount
        , Optimizations.IMergeSelect<U>
        , Optimizations.IMergeWhere<U>
    {
        private readonly T _element;
        private readonly int _count;

        public Repeat(T element, int count, ILink<T, U> first) : base(first) =>
            (_element, _count) = (element, count);

        public override IConsumable<U> Create   (ILink<T, U> first) => new Repeat<T, U>(_element, _count, first);
        public override IConsumable<V> Create<V>(ILink<T, V> first) => new Repeat<T, V>(_element, _count, first);

        public override IEnumerator<U> GetEnumerator() =>
            Cistern.Linq.GetEnumerator.Repeat.Get(_element, _count, Link);

        public override void Consume(Consumer<U> consumer) =>
            Cistern.Linq.Consume.Repeat.Invoke(_element, _count, Link, consumer);

        public int? TryFastCount(bool asCountConsumer) =>
            Optimizations.Count.TryGetCount(this, LinkOrNull, asCountConsumer);

        public int? TryRawCount(bool asCountConsumer) => _count;

        public override object TailLink => IsIdentity ? this : base.TailLink;

        IConsumable<V> Optimizations.IMergeSelect<U>.MergeSelect<V>(Consumable<U> _, Func<U, V> selector) =>
            (IConsumable<V>)(object)new SelectEnumerable<Consume.Repeat.RepeatEnumerable<T>, Consume.Repeat.RepeatEnumerator<T>, T, V>(new Consume.Repeat.RepeatEnumerable<T>(_element, _count), (Func<T, V>)(object)selector);

        public IConsumable<U> MergeWhere(Consumable<U> _, Func<U, bool> predicate) =>
            (IConsumable<U>)(object)new WhereEnumerable<Consume.Repeat.RepeatEnumerable<T>, Consume.Repeat.RepeatEnumerator<T>, T>(new Consume.Repeat.RepeatEnumerable<T>(_element, _count), (Func<T, bool>)(object)predicate);
    }
}
