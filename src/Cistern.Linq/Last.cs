﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace Cistern.Linq
{
    public static partial class Enumerable
    {
        public static TSource Last<TSource>(this IEnumerable<TSource> source) =>
            GetLast(source, false);

        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source) =>
            GetLast(source, true);

        public static TSource Last<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
            GetLast(source, predicate, false);

        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
            GetLast(source, predicate, true);

        private static TSource GetLast<TSource>(IEnumerable<TSource> source, bool orDefault)
        {
            if (source == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.source);
            }

            var consumable = ChainLinq.Utils.AsConsumable(source);

            var last = new ChainLinq.Consumer.Last<TSource>(orDefault);
            consumable.Consume(last);
            return last.Result;
        }

        private static TSource GetLast<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate, bool orDefault)
        {
            if (source == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.source);
            }

            if (predicate == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.predicate);
            }

            if (source is IList<TSource> list)
            {
                for (int i = list.Count - 1; i >= 0; --i)
                {
                    TSource result = list[i];
                    if (predicate(result))
                    {
                        return result;
                    }
                }

                if (orDefault)
                {
                    return default(TSource);
                }

                ThrowHelper.ThrowNoElementsException();
            }

            return GetLast(source.Where(predicate), orDefault);
        }
    }
}
