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

namespace Db4objects.Db4o.Foundation
{
    /// <exclude></exclude>
    public class HashtableBase
    {
        private const float Fill = 0.5F;
        public int _mask;
        public int _maximumSize;
        public int _size;
        public HashtableIntEntry[] _table;
        public int _tableSize;

        public HashtableBase(int size)
        {
            // FIELDS ARE PUBLIC SO THEY CAN BE REFLECTED ON IN JDKs <= 1.1
            size = NewSize(size);
            // legacy for .NET conversion
            _tableSize = 1;
            while (_tableSize < size)
            {
                _tableSize = _tableSize << 1;
            }
            _mask = _tableSize - 1;
            _maximumSize = (int) (_tableSize*Fill);
            _table = new HashtableIntEntry[_tableSize];
        }

        public HashtableBase() : this(1)
        {
        }

        /// <param name="cloneOnlyCtor"></param>
        protected HashtableBase(IDeepClone cloneOnlyCtor)
        {
        }

        public virtual void Clear()
        {
            _size = 0;
            Arrays4.Fill(_table, null);
        }

        private int NewSize(int size)
        {
            return (int) (size/Fill);
        }

        public virtual int Size()
        {
            return _size;
        }

        protected virtual HashtableIntEntry FindWithSameKey(HashtableIntEntry newEntry)
        {
            var existing = _table[EntryIndex(newEntry)];
            while (null != existing)
            {
                if (existing.SameKeyAs(newEntry))
                {
                    return existing;
                }
                existing = existing._next;
            }
            return null;
        }

        protected virtual int EntryIndex(HashtableIntEntry entry)
        {
            return entry._key & _mask;
        }

        protected virtual void PutEntry(HashtableIntEntry newEntry)
        {
            var existing = FindWithSameKey(newEntry);
            if (null != existing)
            {
                Replace(existing, newEntry);
            }
            else
            {
                Insert(newEntry);
            }
        }

        private void Insert(HashtableIntEntry newEntry)
        {
            _size++;
            if (_size > _maximumSize)
            {
                IncreaseSize();
            }
            var index = EntryIndex(newEntry);
            newEntry._next = _table[index];
            _table[index] = newEntry;
        }

        private void Replace(HashtableIntEntry existing, HashtableIntEntry newEntry)
        {
            newEntry._next = existing._next;
            var entry = _table[EntryIndex(existing)];
            if (entry == existing)
            {
                _table[EntryIndex(existing)] = newEntry;
            }
            else
            {
                while (entry._next != existing)
                {
                    entry = entry._next;
                }
                entry._next = newEntry;
            }
        }

        private void IncreaseSize()
        {
            _tableSize = _tableSize << 1;
            _maximumSize = _maximumSize << 1;
            _mask = _tableSize - 1;
            var temp = _table;
            _table = new HashtableIntEntry[_tableSize];
            for (var i = 0; i < temp.Length; i++)
            {
                Reposition(temp[i]);
            }
        }

        protected virtual HashtableIterator HashtableIterator(
            )
        {
            return new HashtableIterator(_table);
        }

        private void Reposition(HashtableIntEntry entry)
        {
            var currentEntry = entry;
            HashtableIntEntry nextEntry = null;
            while (currentEntry != null)
            {
                nextEntry = currentEntry._next;
                currentEntry._next = _table[EntryIndex(currentEntry)];
                _table[EntryIndex(currentEntry)] = currentEntry;
                currentEntry = nextEntry;
            }
        }

        public virtual IEnumerator Keys()
        {
            return Iterators.Map(HashtableIterator(), new _IFunction4_133());
        }

        public virtual IEnumerable Values()
        {
            return new _IEnumerable_141(this);
        }

        /// <summary>Iterates through all the values.</summary>
        /// <remarks>Iterates through all the values.</remarks>
        /// <returns>value iterator</returns>
        public virtual IEnumerator ValuesIterator()
        {
            return Iterators.Map(HashtableIterator(), new _IFunction4_154());
        }

        public override string ToString()
        {
            return Iterators.Join(HashtableIterator(), "{", "}", ", ");
        }

        protected virtual void RemoveEntry(HashtableIntEntry predecessor, HashtableIntEntry
            entry)
        {
            if (predecessor != null)
            {
                predecessor._next = entry._next;
            }
            else
            {
                _table[EntryIndex(entry)] = entry._next;
            }
            _size--;
        }

        protected virtual object RemoveObjectEntry(int intKey, object objectKey)
        {
            var entry = (HashtableObjectEntry) _table[intKey & _mask];
            HashtableObjectEntry predecessor = null;
            while (entry != null)
            {
                if (entry._key == intKey && entry.HasKey(objectKey))
                {
                    RemoveEntry(predecessor, entry);
                    return entry._object;
                }
                predecessor = entry;
                entry = (HashtableObjectEntry) entry._next;
            }
            return null;
        }

        protected virtual object RemoveLongEntry(int intKey, long longKey)
        {
            var entry = (HashtableLongEntry) _table[intKey & _mask];
            HashtableLongEntry predecessor = null;
            while (entry != null)
            {
                if (entry._key == intKey && entry._longKey == longKey)
                {
                    RemoveEntry(predecessor, entry);
                    return entry._object;
                }
                predecessor = entry;
                entry = (HashtableLongEntry) entry._next;
            }
            return null;
        }

        protected virtual object RemoveIntEntry(int key)
        {
            var entry = _table[key & _mask];
            HashtableIntEntry predecessor = null;
            while (entry != null)
            {
                if (entry._key == key)
                {
                    RemoveEntry(predecessor, entry);
                    return entry._object;
                }
                predecessor = entry;
                entry = entry._next;
            }
            return null;
        }

        private sealed class _IFunction4_133 : IFunction4
        {
            public object Apply(object current)
            {
                return ((IEntry4) current).Key();
            }
        }

        private sealed class _IEnumerable_141 : IEnumerable
        {
            private readonly HashtableBase _enclosing;

            public _IEnumerable_141(HashtableBase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public IEnumerator GetEnumerator()
            {
                return _enclosing.ValuesIterator();
            }
        }

        private sealed class _IFunction4_154 : IFunction4
        {
            public object Apply(object current)
            {
                return ((IEntry4) current).Value();
            }
        }
    }
}