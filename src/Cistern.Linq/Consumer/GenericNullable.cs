﻿using System;

namespace Cistern.Linq.Consumer
{
    interface INullableGenericLogic<T>
        where T : struct
    {
        void Init(T? result);
        bool Process(T? input);
        T? Result { get; }
    }

    sealed class GenericNullable<T, Logic>
        : Consumer<T?, T?>
        , Optimizations.IHeadStart<T?>
        , Optimizations.ITailEnd<T?>
        where T : struct
        where Logic : INullableGenericLogic<T>
    {
        public GenericNullable() : base(null) { }

        public override ChainStatus ProcessNext(T? input)
        {
            Logic logic = default; logic.Init(Result);
            var status = logic.Process(input) ? ChainStatus.Flow : ChainStatus.Stop;
            Result = logic.Result;

            return status;
        }

        ChainStatus Optimizations.IHeadStart<T?>.Execute(ReadOnlySpan<T?> source)
        {
            Logic logic = default; logic.Init(Result);
            var chainStatus = ChainStatus.Flow;

            foreach (var input in source)
            {
                if (!logic.Process(input))
                {
                    chainStatus = ChainStatus.Stop;
                    break;
                }
            }

            Result = logic.Result;
            return chainStatus;
        }

        ChainStatus Optimizations.IHeadStart<T?>.Execute<Enumerable, Enumerator>(Enumerable source)
        {
            Logic logic = default; logic.Init(Result);
            var chainStatus = ChainStatus.Flow;

            foreach (var input in source)
            {
                if (!logic.Process(input))
                {
                    chainStatus = ChainStatus.Stop;
                    break;
                }
            }

            Result = logic.Result;
            return chainStatus;
        }

        ChainStatus Optimizations.ITailEnd<T?>.Select<S>(ReadOnlySpan<S> source, Func<S, T?> selector)
        {
            Logic logic = default; logic.Init(Result);
            var chainStatus = ChainStatus.Flow;

            foreach (var input in source)
            {
                if (!logic.Process(selector(input)))
                {
                    chainStatus = ChainStatus.Stop;
                    break;
                }
            }

            Result = logic.Result;
            return chainStatus;
        }

        ChainStatus Optimizations.ITailEnd<T?>.Select<Enumerable, Enumerator, S>(Enumerable source, Func<S, T?> selector)
        {
            Logic logic = default; logic.Init(Result);
            var chainStatus = ChainStatus.Flow;

            foreach (var input in source)
            {
                if (!logic.Process(selector(input)))
                {
                    chainStatus = ChainStatus.Stop;
                    break;
                }
            }

            Result = logic.Result;
            return chainStatus;
        }

        ChainStatus Optimizations.ITailEnd<T?>.SelectMany<TSource, TCollection>(TSource source, ReadOnlySpan<TCollection> span, Func<TSource, TCollection, T?> resultSelector)
        {
            Logic logic = default; logic.Init(Result);
            ChainStatus status = ChainStatus.Flow;

            foreach (var input in span)
            {
                if (!logic.Process(resultSelector(source, input)))
                {
                    status = ChainStatus.Stop;
                    break;
                }
            }

            Result = logic.Result;

            return status;
        }

        ChainStatus Optimizations.ITailEnd<T?>.Where(ReadOnlySpan<T?> source, Func<T?, bool> predicate)
        {
            Logic logic = default; logic.Init(Result);
            ChainStatus chainStatus = ChainStatus.Flow;

            foreach (var input in source)
            {
                if (predicate(input))
                {
                    if (!logic.Process(input))
                    {
                        chainStatus = ChainStatus.Stop;
                        break;
                    }
                }
            }

            Result = logic.Result;
            return chainStatus;
        }

        ChainStatus Optimizations.ITailEnd<T?>.Where<Enumerable, Enumerator>(Enumerable source, Func<T?, bool> predicate)
        {
            Logic logic = default; logic.Init(Result);
            var chainStatus = ChainStatus.Flow;

            foreach (var input in source)
            {
                if (predicate(input))
                {
                    if (!logic.Process(input))
                    {
                        chainStatus = ChainStatus.Stop;
                        break;
                    }
                }
            }

            Result = logic.Result;
            return chainStatus;
        }

        ChainStatus Optimizations.ITailEnd<T?>.WhereSelect<S>(ReadOnlySpan<S> source, Func<S, bool> predicate, Func<S, T?> selector)
        {
            Logic logic = default; logic.Init(Result);
            var chainStatus = ChainStatus.Flow;
            foreach (var input in source)
            {
                if (predicate(input))
                {
                    if (!logic.Process(selector(input)))
                    {
                        chainStatus = ChainStatus.Stop;
                        break;
                    }
                }
            }

            Result = logic.Result;
            return chainStatus;
        }

        ChainStatus Optimizations.ITailEnd<T?>.WhereSelect<Enumerable, Enumerator, S>(Enumerable source, Func<S, bool> predicate, Func<S, T?> selector)
        {
            Logic logic = default; logic.Init(Result);
            var chainStatus = ChainStatus.Flow;
            foreach (var input in source)
            {
                if (predicate(input))
                {
                    if (!logic.Process(selector(input)))
                    {
                        chainStatus = ChainStatus.Stop;
                        break;
                    }
                }
            }

            Result = logic.Result;
            return chainStatus;
        }
    }
}
