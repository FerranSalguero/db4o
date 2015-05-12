/* This file is part of the db4o object database http://www.db4o.com

Copyright (C) 2004 - 2011  Versant Corporation http://www.versant.com

db4o is free software; you can redistribute it and/or modify it under
the terms of version 3 of the GNU General Public License as published
by the Free Software Foundation.

db4o is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or
FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License
for more details.

You should have received a copy of the GNU General Public License along
with this program.  If not, see http://www.gnu.org/licenses/. */

using System;
using System.Collections;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal.Query.Result;
using Sharpen.Lang;

namespace Db4objects.Db4o.CS.Internal
{
    /// <exclude></exclude>
    internal class ClientQueryResultIterator : IEnumerator
    {
        private static readonly IPrefetchingStrategy _prefetchingStrategy = SingleMessagePrefetchingStrategy
            .Instance;

        private readonly AbstractQueryResult _client;
        private readonly IIntIterator4 _ids;
        private object[] _prefetchedObjects;
        private int _prefetchRight;
        private int _remainingObjects;

        internal ClientQueryResultIterator(AbstractQueryResult client)
        {
            _client = client;
            _ids = client.IterateIDs();
        }

        public virtual object Current
        {
            get
            {
                lock (StreamLock())
                {
                    return _client.Activate(PrefetchedCurrent());
                }
            }
        }

        public virtual void Reset()
        {
            _remainingObjects = 0;
            _ids.Reset();
        }

        public virtual bool MoveNext()
        {
            lock (StreamLock())
            {
                if (_remainingObjects > 0)
                {
                    --_remainingObjects;
                    return SkipNulls();
                }
                Prefetch();
                --_remainingObjects;
                if (_remainingObjects < 0)
                {
                    return false;
                }
                return SkipNulls();
            }
        }

        private object StreamLock()
        {
            return _client.Lock();
        }

        private bool SkipNulls()
        {
            // skip nulls (deleted objects)
            if (PrefetchedCurrent() == null)
            {
                return MoveNext();
            }
            return true;
        }

        private void Prefetch()
        {
            _client.Stream().WithEnvironment(new _IRunnable_67(this));
        }

        private int PrefetchCount()
        {
            return Math.Max(Stream().Config().PrefetchObjectCount(), 1);
        }

        private ClientObjectContainer Stream()
        {
            return (ClientObjectContainer) _client.Stream();
        }

        private object PrefetchedCurrent()
        {
            return _prefetchedObjects[_prefetchRight - _remainingObjects - 1];
        }

        // TODO: open this as an external tuning interface in ExtObjectSet
        //	public void prefetch(int count){
        //		if(count < 1){
        //			count = 1;
        //		}
        //		i_prefetchCount = count;
        //		Object[] temp = new Object[i_prefetchCount];
        //		if(i_remainingObjects > 0){
        //			// Potential problem here: 
        //			// On reducing the prefetch size, this will crash.
        //			System.arraycopy(i_prefetched, 0, temp, 0, i_remainingObjects);
        //		}
        //		i_prefetched = temp;
        //	}
        private void EnsureObjectCacheAllocated(int prefetchObjectCount)
        {
            if (_prefetchedObjects == null)
            {
                _prefetchedObjects = new object[prefetchObjectCount];
                return;
            }
            if (prefetchObjectCount > _prefetchedObjects.Length)
            {
                var newPrefetchedObjects = new object[prefetchObjectCount];
                Array.Copy(_prefetchedObjects, 0, newPrefetchedObjects, 0, _prefetchedObjects
                    .Length);
                _prefetchedObjects = newPrefetchedObjects;
            }
        }

        private sealed class _IRunnable_67 : IRunnable
        {
            private readonly ClientQueryResultIterator _enclosing;

            public _IRunnable_67(ClientQueryResultIterator _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Run()
            {
                _enclosing.EnsureObjectCacheAllocated(_enclosing.PrefetchCount());
                _enclosing._remainingObjects = _prefetchingStrategy.PrefetchObjects(_enclosing.Stream(), _enclosing.
                    _client.Transaction(), _enclosing._ids, _enclosing._prefetchedObjects,
                    _enclosing.PrefetchCount());
                _enclosing._prefetchRight = _enclosing._remainingObjects;
            }
        }
    }
}