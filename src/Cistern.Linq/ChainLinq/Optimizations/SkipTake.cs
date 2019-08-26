﻿namespace Cistern.Linq.ChainLinq.Optimizations
{
    interface ISkipTakeOnConsumable<T>
    {
        Consumable<T> Skip(int toSkip);
        Consumable<T> Take(int toTake);
        T Last(bool orDefault);
    }

    interface ISkipTakeOnConsumableLinkUpdate<T, U>
    {
        Link<T, U> Skip(int toSkip);
    }

    interface IMergeSkip<T>
    {
        Consumable<T> MergeSkip(ConsumableCons<T> consumable, int count);
    }
}
