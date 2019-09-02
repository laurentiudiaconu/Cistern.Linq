﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Cistern.Linq.ChainLinq.Consumer
{
    static class MaxGenericImpl
    {
        // Appears to help the JIT, faster FSharpList.Max in benchmarks
        public static T HeadStartExecuteEnumerable_InnerLoop<T, Accumulator, Maths, Enumerator>(Maths maths, T result, Enumerator e)
            where T : struct
            where Accumulator : struct
            where Maths : struct, Cistern.Linq.Maths.IMathsOperations<T, Accumulator>
            where Enumerator : IEnumerator<T>
        {
            while (e.MoveNext())
            {
                var t = e.Current;
                if (maths.GreaterThan(t, result))
                    result = t;
            }
            return result;
        }
    }

    abstract class MaxGeneric<T, Accumulator, Maths>
        : Consumer<T, T>
        , Optimizations.IHeadStart<T>
        , Optimizations.ITailEnd<T>
        where T : struct
        where Accumulator : struct
        where Maths : struct, Cistern.Linq.Maths.IMathsOperations<T, Accumulator>
    {
        protected bool _noData;

        public MaxGeneric() : base(default(Maths).MaxInit) =>
            _noData = true;

        public override void ChainComplete()
        {
            if (_noData)
            {
                ThrowHelper.ThrowNoElementsException();
            }
        }

        void Optimizations.IHeadStart<T>.Execute(ReadOnlySpan<T> source)
        {
            Maths maths = default;

            var result = Result;

            _noData &= source.Length == 0;
            var idx = 0;
            for (; maths.IsNaN(result) && idx < source.Length; ++idx)
            {
                result = source[idx];
            }
            for(; idx < source.Length; ++idx)
            {
                var input = source[idx];
                if (maths.GreaterThan(input, result))
                    result = input;
            }

            Result = result;
        }

        void Optimizations.IHeadStart<T>.Execute<Enumerable, Enumerator>(Enumerable source)
        {
            Maths maths = default;

            var result = Result;

            using (var e = source.GetEnumerator())
            {
                bool moveNext;
                while (moveNext = e.MoveNext())
                {
                    _noData = false;
                    result = e.Current;
                    if (!maths.IsNaN(result))
                        break;
                }
                if (moveNext)
                {
                    result = MaxGenericImpl.HeadStartExecuteEnumerable_InnerLoop<T, Accumulator, Maths, Enumerator>(maths, result, e);
                }
            }

            Result = result;
        }

        void Optimizations.ITailEnd<T>.Select<S>(ReadOnlySpan<S> source, Func<S, T> selector)
        {
            Maths maths = default;

            var result = Result;

            _noData &= source.Length == 0;
            foreach (var s in source)
            {
                var t = selector(s);
                if (maths.GreaterThan(t, result) || maths.IsNaN(result))
                    result = t;
            }

            Result = result;
        }

        ChainStatus Optimizations.ITailEnd<T>.SelectMany<TSource, TCollection>(TSource source, ReadOnlySpan<TCollection> span, Func<TSource, TCollection, T> resultSelector)
        {
            Maths maths = default;

            var result = Result;

            _noData &= span.Length == 0;
            foreach (var s in span)
            {
                var t = resultSelector(source, s);
                if (maths.GreaterThan(t, result) || maths.IsNaN(result))
                    result = t;
            }

            Result = result;

            return ChainStatus.Flow;
        }

        void Optimizations.ITailEnd<T>.Where(ReadOnlySpan<T> source, Func<T, bool> predicate)
        {
            Maths maths = default;

            var noData = _noData;
            var result = Result;

            foreach (var input in source)
            {
                if (predicate(input))
                {
                    noData = false;
                    if (maths.GreaterThan(input, result) || maths.IsNaN(result))
                        result = input;
                }
            }

            _noData = noData;
            Result = result;
        }
        void Optimizations.ITailEnd<T>.Where<Enumerable, Enumerator>(Enumerable source, Func<T, bool> predicate)
        {
            Maths maths = default;

            var noData = _noData;
            var result = Result;

            foreach (var input in source)
            {
                if (predicate(input))
                {
                    noData = false;
                    if (maths.GreaterThan(input, result) || maths.IsNaN(result))
                        result = input;
                }
            }

            _noData = noData;
            Result = result;
        }

        void Optimizations.ITailEnd<T>.WhereSelect<S>(ReadOnlySpan<S> source, Func<S, bool> predicate, Func<S, T> selector)
        {
            Maths maths = default;

            var noData = _noData;
            var result = Result;

            foreach (var s in source)
            {
                if (predicate(s))
                {
                    noData = false;
                    var t = selector(s);
                    if (maths.GreaterThan(t, result) || maths.IsNaN(result))
                        result = t;
                }
            }

            _noData = noData;
            Result = result;
        }

        void Optimizations.ITailEnd<T>.WhereSelect<Enumerable, Enumerator, S>(Enumerable source, Func<S, bool> predicate, Func<S, T> selector)
        {
            Maths maths = default;

            var noData = _noData;
            var result = Result;

            foreach (var s in source)
            {
                if (predicate(s))
                {
                    noData = false;
                    var t = selector(s);
                    if (maths.GreaterThan(t, result) || maths.IsNaN(result))
                        result = t;
                }
            }

            _noData = noData;
            Result = result;
        }

    }

    struct MaxNullableLogic<T, Accumulator, Maths> : INullableGenericLogic<T>
        where T : struct
        where Accumulator : struct
        where Maths : struct, Cistern.Linq.Maths.IMathsOperations<T, Accumulator>
    {
        bool found;
        T result;

        T? INullableGenericLogic<T>.Result { get => found ? (T?)result : null; }

        public void Init(T? result)
        {
            this.found = result.HasValue;
            this.result = result.GetValueOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Process(T? input)
        {
            var maths = default(Maths);

            if (!found)
            {
                if (input.HasValue)
                {
                    result = input.Value;
                    found = true;
                }
                return true;
            }

            if (input.HasValue)
            {
                var i = input.GetValueOrDefault();
                if (maths.GreaterThan(i, result) || maths.IsNaN(result))
                {
                    result = i;
                }
            }

            return true;
        }
    }

    sealed class MaxInt : MaxGeneric<int, int, Maths.OpsInt>
    {
        public override ChainStatus ProcessNext(int input)
        {
            _noData = false;
            if (input > Result)
            {
                Result = input;
            }
            return ChainStatus.Flow;
        }
    }

    sealed class MaxLong : MaxGeneric<long, long, Maths.OpsLong>
    {
        public override ChainStatus ProcessNext(long input)
        {
            _noData = false;
            if (input > Result)
            {
                Result = input;
            }
            return ChainStatus.Flow;
        }
    }

    sealed class MaxDouble : MaxGeneric<double, double, Maths.OpsDouble>
    {
        public override ChainStatus ProcessNext(double input)
        {
            _noData = false;
            if (input > Result || double.IsNaN(Result))
            {
                Result = input;
            }
            return ChainStatus.Flow;
        }
    }

    sealed class MaxFloat : MaxGeneric<float, double, Maths.OpsFloat>
    {
        public override ChainStatus ProcessNext(float input)
        {
            _noData = false;
            if (input > Result || float.IsNaN(Result))
            {
                Result = input;
            }
            return ChainStatus.Flow;
        }
    }

    sealed class MaxDecimal : MaxGeneric<decimal, decimal, Maths.OpsDecimal>
    {
        public override ChainStatus ProcessNext(decimal input)
        {
            _noData = false;
            if (input > Result)
            {
                Result = input;
            }
            return ChainStatus.Flow;
        }
    }

    sealed class MaxValueType<T> : Consumer<T, T>
    {
        bool _first;

        public MaxValueType() : base(default) =>
            _first = true;

        public override ChainStatus ProcessNext(T input)
        {
            if (_first)
            {
                _first = false;
                Result = input;
            }
            else if (Comparer<T>.Default.Compare(input, Result) > 0)
            {
                Result = input;
            }

            return ChainStatus.Flow;
        }

        public override void ChainComplete()
        {
            if (_first)
            {
                ThrowHelper.ThrowNoElementsException();
            }
        }
    }

    sealed class MaxRefType<T> : Consumer<T, T>
    {
        public MaxRefType() : base(default) { }

        public override ChainStatus ProcessNext(T input)
        {
            if (Result == null || (input != null && Comparer<T>.Default.Compare(input, Result) > 0))
            {
                Result = input;
            }

            return ChainStatus.Flow;
        }
    }
}
