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

using System.Collections;
using Db4objects.Db4o.Foundation;

namespace Db4objects.Db4o.Internal.Caching
{
    /// <exclude></exclude>
    internal class LRUIntCache : IPurgeableCache4
    {
        private readonly int _maxSize;
        private readonly Hashtable4 _slots;
        private Entry _first;
        private Entry _last;
        private int _size;

        internal LRUIntCache(int size)
        {
            _maxSize = size;
            _slots = new Hashtable4(size);
        }

        public virtual object Produce(object key, IFunction4 producer, IProcedure4 finalizer
            )
        {
            var intKey = (((int) key));
            if (_last == null)
            {
                var lastValue = producer.Apply(((int) key));
                if (lastValue == null)
                {
                    return null;
                }
                _size = 1;
                var lastEntry = new Entry(intKey, lastValue);
                _slots.Put(intKey, lastEntry);
                _first = lastEntry;
                _last = lastEntry;
                return lastValue;
            }
            var entry = (Entry) _slots.Get(intKey);
            if (entry == null)
            {
                if (_size >= _maxSize)
                {
                    var oldEntry = (Entry) _slots.Remove(_last._key);
                    _last = oldEntry._previous;
                    _last._next = null;
                    if (null != finalizer)
                    {
                        finalizer.Apply(oldEntry._value);
                    }
                    _size--;
                }
                var newValue = producer.Apply(((int) key));
                if (newValue == null)
                {
                    return null;
                }
                _size++;
                var newEntry = new Entry(intKey, newValue);
                _slots.Put(intKey, newEntry);
                _first._previous = newEntry;
                newEntry._next = _first;
                _first = newEntry;
                return newValue;
            }
            if (_first == entry)
            {
                return entry._value;
            }
            var previous = entry._previous;
            entry._previous = null;
            if (_last == entry)
            {
                _last = previous;
            }
            previous._next = entry._next;
            if (previous._next != null)
            {
                previous._next._previous = previous;
            }
            _first._previous = entry;
            entry._next = _first;
            _first = entry;
            return entry._value;
        }

        public virtual IEnumerator GetEnumerator()
        {
            IEnumerator i = new _IEnumerator_108(this);
            return i;
        }

        public virtual object Purge(object key)
        {
            var intKey = (((int) key));
            var entry = (Entry) _slots.Remove(intKey);
            if (entry == null)
            {
                return null;
            }
            _size--;
            if (_first == entry)
            {
                _first = entry._next;
            }
            if (_last == entry)
            {
                _last = entry._previous;
            }
            if (entry._previous != null)
            {
                entry._previous._next = entry._next;
            }
            if (entry._next != null)
            {
                entry._next._previous = entry._previous;
            }
            return entry._value;
        }

        private class Entry
        {
            internal readonly int _key;
            internal readonly object _value;
            internal Entry _next;
            internal Entry _previous;

            public Entry(int key, object value)
            {
                _key = key;
                _value = value;
            }

            public override string ToString()
            {
                return string.Empty + _key;
            }
        }

        private sealed class _IEnumerator_108 : IEnumerator
        {
            private readonly LRUIntCache _enclosing;
            private Entry _current;
            private Entry _cursor;

            public _IEnumerator_108(LRUIntCache _enclosing)
            {
                this._enclosing = _enclosing;
                _cursor = this._enclosing._first;
            }

            public object Current
            {
                get { return _current._value; }
            }

            public bool MoveNext()
            {
                if (_cursor == null)
                {
                    _current = null;
                    return false;
                }
                _current = _cursor;
                _cursor = _cursor._next;
                return true;
            }

            public void Reset()
            {
                _cursor = _enclosing._first;
                _current = null;
            }
        }
    }
}