﻿using System;
using System.Collections.Generic;

namespace Cistern.Linq.ChainLinq.Consumables
{
    sealed partial class SelectArray<T, U>
        : ConsumableEnumerator<U>
        , Optimizations.IMergeSelect<U>
        , Optimizations.IMergeWhere<U>
    {
        internal T[] Underlying { get; }
        internal Func<T, U> Selector { get; }

        int _idx;

        public SelectArray(T[] array, Func<T, U> selector) =>
            (Underlying, Selector) = (array, selector);

        public override void Consume(Consumer<U> consumer)
        {
            if (consumer is Optimizations.ITailEnd<U> optimized)
            {
                try
                {
                    optimized.Select(Underlying, Selector);
                    consumer.ChainComplete();
                }
                finally
                {
                    consumer.ChainDispose();
                }
            }
            else
            {
                ChainLinq.Consume.ReadOnlySpan.Invoke(Underlying, new Links.Select<T, U>(Selector), consumer);
            }
        }

        internal override ConsumableEnumerator<U> Clone() =>
            new SelectArray<T, U>(Underlying, Selector);

        public override bool MoveNext()
        {
            if (_state != 1 || _idx >= Underlying.Length)
            {
                _current = default(U);
                return false;
            }

            _current = Selector(Underlying[_idx++]);

            return true;
        }

        public override object TailLink => this; // for IMergeSelect & IMergeWhere;

        public override Consumable<V> ReplaceTailLink<Unknown, V>(ILink<Unknown, V> newLink) => throw new NotImplementedException();

        public override Consumable<U> AddTail(ILink<U, U> transform) =>
            new Array<T, U>(Underlying, 0, Underlying.Length, Links.Composition.Create(new Links.Select<T, U>(Selector), transform));

        public override Consumable<V> AddTail<V>(ILink<U, V> transform) =>
            new Array<T, V>(Underlying, 0, Underlying.Length, Links.Composition.Create(new Links.Select<T, U>(Selector), transform));

        Consumable<V> Optimizations.IMergeSelect<U>.MergeSelect<V>(ConsumableCons<U> _, Func<U, V> u2v) =>
            new SelectArray<T, V>(Underlying, t => u2v(Selector(t)));

        Consumable<U> Optimizations.IMergeWhere<U>.MergeWhere(ConsumableCons<U> _, Func<U, bool> predicate) =>
            new SelectWhereArray<T, U>(Underlying, Selector, predicate);
    }

    sealed partial class SelectEnumerable<TEnumerable, TEnumerator, T, U>
        : ConsumableEnumerator<U>
        , Optimizations.IMergeSelect<U>
        , Optimizations.IMergeWhere<U>
        where TEnumerable : Optimizations.ITypedEnumerable<T, TEnumerable, TEnumerator>
        where TEnumerator : IEnumerator<T>
    {
        internal TEnumerable Underlying { get; }
        internal Func<T, U> Selector { get; }

        TEnumerator _enumerator;

        public SelectEnumerable(TEnumerable enumerable, Func<T, U> selector) =>
            (Underlying, Selector) = (enumerable, selector);

        public override void Consume(Consumer<U> consumer)
        {
            if (consumer is Optimizations.ITailEnd<U> optimized)
            {
                try
                {
                    if (Underlying.TryGetSourceAsSpan(out var span))
                        optimized.Select(span, Selector);
                    else
                        optimized.Select<TEnumerable, TEnumerator, T>(Underlying, Selector);

                    consumer.ChainComplete();
                }
                finally
                {
                    consumer.ChainDispose();
                }
            }
            else
            {
                if (Underlying.TryGetSourceAsSpan(out var span))
                    ChainLinq.Consume.ReadOnlySpan.Invoke(span, new Links.Select<T, U>(Selector), consumer);
                else
                    ChainLinq.Consume.Enumerable.Invoke<TEnumerable, TEnumerator, T, U>(Underlying, new Links.Select<T, U>(Selector), consumer);
            }
        }

        internal override ConsumableEnumerator<U> Clone() =>
            new SelectEnumerable<TEnumerable, TEnumerator, T, U>(Underlying, Selector);

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
                    if (!_enumerator.MoveNext())
                    {
                        _state = int.MaxValue;
                        goto default;
                    }
                    _current = Selector(_enumerator.Current);
                    return true;

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

        public override object TailLink => this; // for IMergeSelect & IMergeWhere

        public override Consumable<V1> ReplaceTailLink<Unknown, V1>(ILink<Unknown, V1> newLink) =>
            throw new NotImplementedException();

        public override Consumable<U> AddTail(ILink<U, U> transform) =>
            new Enumerable<TEnumerable, TEnumerator, T, U>(Underlying, Links.Composition.Create(new Links.Select<T, U>(Selector), transform));

        public override Consumable<V> AddTail<V>(ILink<U, V> transform) =>
            new Enumerable<TEnumerable, TEnumerator, T, V>(Underlying, Links.Composition.Create(new Links.Select<T, U>(Selector), transform));

        Consumable<V> Optimizations.IMergeSelect<U>.MergeSelect<V>(ConsumableCons<U> _, Func<U, V> u2v) =>
            new SelectEnumerable<TEnumerable, TEnumerator, T, V>(Underlying, t => u2v(Selector(t)));

        Consumable<U> Optimizations.IMergeWhere<U>.MergeWhere(ConsumableCons<U> _, Func<U, bool> predicate) =>
            new SelectWhereEnumerable<TEnumerable, TEnumerator, T, U>(Underlying, Selector, predicate);
    }
}
