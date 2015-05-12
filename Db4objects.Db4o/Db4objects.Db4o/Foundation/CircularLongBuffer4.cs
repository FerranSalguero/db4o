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

namespace Db4objects.Db4o.Foundation
{
    /// <summary>
    ///     A fixed size double ended queue with O(1) complexity for addFirst, removeFirst and removeLast operations.
    /// </summary>
    /// <remarks>
    ///     A fixed size double ended queue with O(1) complexity for addFirst, removeFirst and removeLast operations.
    /// </remarks>
    public class CircularLongBuffer4 : IEnumerable
    {
        private const int Empty = -1;
        private readonly long[] _buffer;
        private int _head;
        private int _tail;

        public CircularLongBuffer4(int size)
        {
            _buffer = new long[size + 1];
        }

        public virtual IEnumerator GetEnumerator()
        {
            var tail = Index(_tail);
            var head = Index(_head);
            // TODO: detect concurrent modification and throw IllegalStateException
            return new _IEnumerator_122(this, head, tail);
        }

        public virtual int Size()
        {
            return Index(_tail - _head);
        }

        public virtual void AddFirst(long value)
        {
            var newHead = CircularIndex(_head - 1);
            if (newHead == _tail)
            {
                throw new InvalidOperationException();
            }
            _head = newHead;
            _buffer[Index(_head)] = value;
        }

        private int CircularIndex(int index)
        {
            return index%_buffer.Length;
        }

        private int Index(int i)
        {
            return i < 0 ? _buffer.Length + i : i;
        }

        public virtual long RemoveLast()
        {
            AssertNotEmpty();
            _tail = CircularIndex(_tail - 1);
            return Erase(_tail);
        }

        private void AssertNotEmpty()
        {
            if (IsEmpty())
            {
                throw new InvalidOperationException();
            }
        }

        public virtual bool IsEmpty()
        {
            return Index(_head) == Index(_tail);
        }

        public virtual bool IsFull()
        {
            return CircularIndex(_head - 1) == _tail;
        }

        public virtual long RemoveFirst()
        {
            AssertNotEmpty();
            var erased = Erase(_head);
            _head = CircularIndex(_head + 1);
            return erased;
        }

        private long Erase(int index)
        {
            var bufferIndex = Index(index);
            var erasedValue = _buffer[bufferIndex];
            _buffer[bufferIndex] = Empty;
            return erasedValue;
        }

        public virtual bool Remove(long value)
        {
            var idx = IndexOf(value);
            if (idx >= 0)
            {
                RemoveAt(idx);
                return true;
            }
            return false;
        }

        public virtual bool Contains(long value)
        {
            return IndexOf(value) >= 0;
        }

        private int IndexOf(long value)
        {
            var current = Index(_head);
            var tail = Index(_tail);
            while (current != tail)
            {
                if (value == _buffer[current])
                {
                    break;
                }
                current = CircularIndex(current + 1);
            }
            return (current == tail ? -1 : current);
        }

        private void RemoveAt(int index)
        {
            if (Index(_tail - 1) == index)
            {
                RemoveLast();
                return;
            }
            if (index == Index(_head))
            {
                RemoveFirst();
                return;
            }
            var current = index;
            var tail = Index(_tail);
            while (current != tail)
            {
                var next = CircularIndex(current + 1);
                _buffer[current] = _buffer[next];
                current = next;
            }
            _tail = CircularIndex(_tail - 1);
        }

        private sealed class _IEnumerator_122 : IEnumerator
        {
            private readonly CircularLongBuffer4 _enclosing;
            private readonly int head;
            private readonly int tail;
            private object _current;
            private int _index;

            public _IEnumerator_122(CircularLongBuffer4 _enclosing, int head, int tail)
            {
                this._enclosing = _enclosing;
                this.head = head;
                this.tail = tail;
                _index = head;
                _current = Iterators.NoElement;
            }

            public object Current
            {
                get
                {
                    if (_current == Iterators.NoElement)
                    {
                        throw new InvalidOperationException();
                    }
                    return _current;
                }
            }

            public bool MoveNext()
            {
                if (_index == tail)
                {
                    return false;
                }
                _current = _enclosing._buffer[_index];
                _index = _enclosing.CircularIndex(_index + 1);
                return true;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }
    }
}