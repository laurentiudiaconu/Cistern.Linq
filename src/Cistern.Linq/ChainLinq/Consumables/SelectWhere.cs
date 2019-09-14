﻿using System;
using System.Collections.Generic;

namespace Cistern.Linq.ChainLinq.Consumables
{
    sealed partial class SelectWhereArray<T, U>
        : ConsumableEnumerator<U>
        , Optimizations.IMergeWhere<U>
    {
        internal T[] Underlying { get; }
        internal Func<T, U> Selector { get; }
        internal Func<U, bool> Predicate { get; }

        int _idx;

        public SelectWhereArray(T[] array, Func<T, U> selector, Func<U, bool> predicate) =>
            (Underlying, Selector, Predicate) = (array, selector, predicate);

        public override void Consume(Consumer<U> consumer) =>
            ChainLinq.Consume.ReadOnlySpan.Invoke(Underlying, new Links.SelectWhere<T, U>(Selector, Predicate), consumer);

        internal override ConsumableEnumerator<U> Clone() =>
            new SelectWhereArray<T, U>(Underlying, Selector, Predicate);

        public override bool MoveNext()
        {
            if (_state != 1)
                return false;

            while (_idx < Underlying.Length)
            {
                var current = Selector(Underlying[_idx++]);
                if (Predicate(current))
                {
                    _current = current;
                    return true;
                }
            }

            _current = default(U);
            return false;
        }

        public override object TailLink => this; // for IMergeWhere

        public override Consumable<V> ReplaceTailLink<Unknown, V>(ILink<Unknown, V> newLink) =>
            throw new NotImplementedException();

        public override Consumable<U> AddTail(ILink<U, U> transform) =>
            new Array<T, U>(Underlying, 0, Underlying.Length, Links.Composition.Create(new Links.SelectWhere<T, U>(Selector, Predicate), transform));

        public override Consumable<V> AddTail<V>(ILink<U, V> transform) =>
            new Array<T, V>(Underlying, 0, Underlying.Length, Links.Composition.Create(new Links.SelectWhere<T, U>(Selector, Predicate), transform));

        Consumable<U> Optimizations.IMergeWhere<U>.MergeWhere(ConsumableCons<U> _, Func<U, bool> predicate) =>
            new SelectWhereArray<T, U>(Underlying, Selector, x => Predicate(x) && predicate(x));
    }

    sealed partial class SelectWhereEnumerable<TEnumerable, TEnumerator, T, U>
        : ConsumableEnumerator<U>
        , Optimizations.IMergeWhere<U>
        where TEnumerable : Optimizations.ITypedEnumerable<T, TEnumerable, TEnumerator>
        where TEnumerator : IEnumerator<T>
    {
        internal TEnumerable Underlying { get; }
        internal Func<T, U> Selector { get; }
        internal Func<U, bool> Predicate { get; }

        TEnumerator _enumerator;

        public SelectWhereEnumerable(TEnumerable enumerable, Func<T, U> selector, Func<U, bool> predicate) =>
            (Underlying, Selector, Predicate) = (enumerable, selector, predicate);

        public override void Consume(Consumer<U> consumer)
        {
            if (Underlying.TryGetSourceAsSpan(out var span))
                ChainLinq.Consume.ReadOnlySpan.Invoke(span, new Links.SelectWhere<T, U>(Selector, Predicate), consumer);
            else
                ChainLinq.Consume.Enumerable.Invoke<TEnumerable, TEnumerator, T, U>(Underlying, new Links.SelectWhere<T, U>(Selector, Predicate), consumer);
        }

        internal override ConsumableEnumerator<U> Clone() =>
            new SelectWhereEnumerable<TEnumerable, TEnumerator, T, U>(Underlying, Selector, Predicate);

        public override void Dispose()
        {
            if (_enumerator != null)
            {
                _enumerator.Dispose();
                _enumerator = default;
            }
            base.Dispose();
        }

        public override bool MoveNext()
        {
            switch (_state)
            {
                case 1:
                    _enumerator = Underlying.GetEnumerator();
                    _state = 2;
                    goto case 2;

                case 2:
                    while (_enumerator.MoveNext())
                    {
                        var current = Selector(_enumerator.Current);
                        if (Predicate(current))
                        {
                            _current = current;
                            return true;
                        }
                    }
                    _state = int.MaxValue;
                    goto default;

                default:
                    _current = default(U);
                    if (_enumerator != null)
                    {
                        _enumerator.Dispose();
                        _enumerator = default;
                    }
                    return false;
            }
        }

        public override object TailLink => this; // for IMergeWhere

        public override Consumable<V> ReplaceTailLink<Unknown, V>(ILink<Unknown, V> newLink) =>
            throw new NotImplementedException();

        public override Consumable<U> AddTail(ILink<U, U> transform) =>
            new Enumerable<TEnumerable, TEnumerator, T, U>(Underlying, Links.Composition.Create(new Links.SelectWhere<T, U>(Selector, Predicate), transform));

        public override Consumable<V> AddTail<V>(ILink<U, V> transform) =>
            new Enumerable<TEnumerable, TEnumerator, T, V>(Underlying, Links.Composition.Create(new Links.SelectWhere<T, U>(Selector, Predicate), transform));
        Consumable<U> Optimizations.IMergeWhere<U>.MergeWhere(ConsumableCons<U> _, Func<U, bool> predicate) =>
            new SelectWhereEnumerable<TEnumerable, TEnumerator, T, U>(Underlying, Selector, x => Predicate(x) && predicate(x));
    }
}
