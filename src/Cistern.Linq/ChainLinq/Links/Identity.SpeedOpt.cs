﻿namespace Cistern.Linq.ChainLinq.Links
{
    sealed partial class Identity<T>
        : Optimizations.ISkipTakeOnConsumableLinkUpdate<T, T>
        , Optimizations.ICountOnConsumableLink
    {
        public int GetCount(int count) => count;

        public Link<T, T> Skip(int toSkip) => this;
    }
}
