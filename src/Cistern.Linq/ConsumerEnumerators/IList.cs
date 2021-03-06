﻿using System.Collections.Generic;

namespace Cistern.Linq.ConsumerEnumerators
{
    internal sealed class IList<T, TResult> : ConsumerEnumerator<TResult>
    {
        private IList<T> _list;
        private readonly int _finalIdx;
        private int _idx;
        private Chain<T> _chain = null;
        private bool _completed;

        internal override Chain StartOfChain => _chain;

        public IList(IList<T> list, int start, int count, ILink<T, TResult> factory)
        {
            _completed = false;
            _list = list;
            _idx = start;
            checked { _finalIdx = start + count; }
            _chain = factory.Compose(this);
        }

        public override void ChainDispose()
        {
            _list = null;
            _chain = null;
        }

        public override bool MoveNext()
        {
        tryAgain:
            if (_idx >= _finalIdx || status.IsStopped())
            {
                if (!_completed && _chain.ChainComplete(status & ~ChainStatus.Flow).NotStoppedAndFlowing())
                {
                    _completed = true;
                    return true;
                }
                Result = default;
                return false;
            }

            status = _chain.ProcessNext(_list[_idx++]);
            if (!status.IsFlowing())
                goto tryAgain;

            return true;
        }
    }
}
