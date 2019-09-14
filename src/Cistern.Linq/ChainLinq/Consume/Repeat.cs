﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace Cistern.Linq.ChainLinq.Consume
{
    static class Repeat
    {
        public struct RepeatEnumerator<T> : IEnumerator<T>
        {
            private readonly int Count;

            private int Index;

            public RepeatEnumerator(T element, int count)
            {
                Current = element;
                Count = count;
                Index = 0;
            }

            public T Current { get; }

            object IEnumerator.Current => Current;

            public void Dispose() { }

            public bool MoveNext()
            {
                if (Index >= Count)
                    return false;

                Index++;
                return true;

            }

            public void Reset() => throw new System.NotImplementedException();
        }

        public struct RepeatEnumerable<T>
            : Optimizations.ITypedEnumerable<T, RepeatEnumerable<T>, RepeatEnumerator<T>>
        {
            private readonly int Count;
            private readonly T Element;

            public RepeatEnumerable(T element, int count)
            {
                Count = count;
                Element = element;
            }

            public IEnumerable<T> Source => null;

            public int? TryLength => Count;

            public RepeatEnumerator<T> GetEnumerator() => new RepeatEnumerator<T>(Element, Count);

            public bool TryGetSourceAsSpan(out ReadOnlySpan<T> readOnlySpan)
            {
                readOnlySpan = default;
                return false;
            }
            public bool TrySkip(int count, out RepeatEnumerable<T> skipped)
            {
                skipped = new RepeatEnumerable<T>(Element, Math.Max(0, Count));
                return true;
            }
        }

        const int MinSizeToCoverExecuteCosts = 10; // from some random testing (depends on pipeline length)

        public static void Invoke<T>(T element, int count, Chain<T> chain)
        {
            try
            {
                if (count > MinSizeToCoverExecuteCosts && chain is Optimizations.IHeadStart<T> optimized)
                {
                    optimized.Execute<RepeatEnumerable<T>, RepeatEnumerator<T>>(new RepeatEnumerable<T>(element, count));
                }
                else
                {
                    Pipeline(element, count, chain);
                }
                chain.ChainComplete();
            }
            finally
            {
                chain.ChainDispose();
            }
        }

        public static void Invoke<T, V>(T element, int count, ILink<T, V> composition, Chain<V> consumer) =>
            Invoke(element, count, composition.Compose(consumer));

        private static void Pipeline<T>(T element, int count, Chain<T> chain)
        {
            for(var i=0; i < count; ++i)
            {
                var state = chain.ProcessNext(element);
                if (state.IsStopped())
                    break;
            }
        }

    }
}
