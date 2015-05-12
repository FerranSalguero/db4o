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
using Db4objects.Db4o.Types;

namespace Db4objects.Db4o.Foundation
{
    /// <summary>Fast linked list for all usecases.</summary>
    /// <remarks>Fast linked list for all usecases.</remarks>
    /// <exclude></exclude>
    public class Collection4 : ISequence4, IEnumerable, IDeepClone, IUnversioned
    {
        private List4 _first;
        private List4 _last;
        private int _size;
        private int _version;

        public Collection4()
        {
        }

        public Collection4(object[] elements)
        {
            AddAll(elements);
        }

        public Collection4(IEnumerable other)
        {
            AddAll(other);
        }

        public Collection4(IEnumerator iterator)
        {
            AddAll(iterator);
        }

        public virtual object DeepClone(object newParent)
        {
            var col = new Collection4
                ();
            object element = null;
            var i = InternalIterator();
            while (i.MoveNext())
            {
                element = i.Current;
                if (element is IDeepClone)
                {
                    col.Add(((IDeepClone) element).DeepClone(newParent));
                }
                else
                {
                    col.Add(element);
                }
            }
            return col;
        }

        /// <summary>Adds an element to the end of this collection.</summary>
        /// <remarks>Adds an element to the end of this collection.</remarks>
        /// <param name="element"></param>
        public bool Add(object element)
        {
            DoAdd(element);
            Changed();
            return true;
        }

        public void AddAll(IEnumerable other)
        {
            AssertNotNull(other);
            AddAll(other.GetEnumerator());
        }

        public void Clear()
        {
            _first = null;
            _last = null;
            _size = 0;
            Changed();
        }

        public bool Contains(object element)
        {
            return Find(element) != null;
        }

        public virtual bool ContainsAll(IEnumerable iter)
        {
            return ContainsAll(iter.GetEnumerator());
        }

        /// <summary>
        ///     Iterates through the collection in reversed insertion order which happens
        ///     to be the fastest.
        /// </summary>
        /// <remarks>
        ///     Iterates through the collection in reversed insertion order which happens
        ///     to be the fastest.
        /// </remarks>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return _first == null
                ? Iterators.EmptyIterator
                : new Collection4Iterator(this, _first
                    );
        }

        public virtual object Get(int index)
        {
            if (index < 0)
            {
                throw new ArgumentException();
            }
            var cur = _first;
            while (index > 0 && cur != null)
            {
                cur = cur._next;
                index--;
            }
            if (cur == null)
            {
                throw new ArgumentException();
            }
            return cur._element;
        }

        /// <summary>
        ///     removes an object from the Collection equals() comparison returns the
        ///     removed object or null, if none found
        /// </summary>
        public virtual bool Remove(object a_object)
        {
            List4 previous = null;
            var current = _first;
            while (current != null)
            {
                if (current.Holds(a_object))
                {
                    _size--;
                    AdjustOnRemoval(previous, current);
                    Changed();
                    return true;
                }
                previous = current;
                current = current._next;
            }
            return false;
        }

        public int Size()
        {
            return _size;
        }

        public bool IsEmpty()
        {
            return _size == 0;
        }

        /// <summary>This is a non reflection implementation for more speed.</summary>
        /// <remarks>
        ///     This is a non reflection implementation for more speed. In contrast to
        ///     the JDK behaviour, the passed array has to be initialized to the right
        ///     length.
        /// </remarks>
        public object[] ToArray(object[] array)
        {
            var j = 0;
            var i = InternalIterator();
            while (i.MoveNext())
            {
                array[j++] = i.Current;
            }
            return array;
        }

        public object[] ToArray()
        {
            var j = 0;
            var array = new object[Size()];
            var i = InternalIterator();
            while (i.MoveNext())
            {
                array[j++] = i.Current;
            }
            return array;
        }

        public virtual object SingleElement()
        {
            if (Size() != 1)
            {
                throw new InvalidOperationException();
            }
            return _first._element;
        }

        public void Prepend(object element)
        {
            DoPrepend(element);
            Changed();
        }

        private void DoPrepend(object element)
        {
            if (_first == null)
            {
                DoAdd(element);
            }
            else
            {
                _first = new List4(_first, element);
                _size++;
            }
        }

        private void DoAdd(object element)
        {
            if (_last == null)
            {
                _first = new List4(element);
                _last = _first;
            }
            else
            {
                _last._next = new List4(element);
                _last = _last._next;
            }
            _size++;
        }

        public void AddAll(object[] elements)
        {
            AssertNotNull(elements);
            for (var i = 0; i < elements.Length; i++)
            {
                Add(elements[i]);
            }
        }

        public void AddAll(IEnumerator iterator)
        {
            AssertNotNull(iterator);
            while (iterator.MoveNext())
            {
                Add(iterator.Current);
            }
        }

        public virtual bool ContainsAll(IEnumerator iter)
        {
            AssertNotNull(iter);
            while (iter.MoveNext())
            {
                if (!Contains(iter.Current))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>tests if the object is in the Collection.</summary>
        /// <remarks>tests if the object is in the Collection. == comparison.</remarks>
        public bool ContainsByIdentity(object element)
        {
            var i = InternalIterator();
            while (i.MoveNext())
            {
                var current = i.Current;
                if (current == element)
                {
                    return true;
                }
            }
            return false;
        }

        private List4 Find(object obj)
        {
            var current = _first;
            while (current != null)
            {
                if (current.Holds(obj))
                {
                    return current;
                }
                current = current._next;
            }
            return null;
        }

        private List4 FindByIdentity(object obj)
        {
            var current = _first;
            while (current != null)
            {
                if (current._element == obj)
                {
                    return current;
                }
                current = current._next;
            }
            return null;
        }

        /// <summary>
        ///     returns the first object found in the Collections that equals() the
        ///     passed object
        /// </summary>
        public object Get(object element)
        {
            var holder = Find(element);
            return holder == null ? null : holder._element;
        }

        /// <summary>makes sure the passed object is in the Collection.</summary>
        /// <remarks>makes sure the passed object is in the Collection. equals() comparison.</remarks>
        public object Ensure(object element)
        {
            var list = Find(element);
            if (list == null)
            {
                Add(element);
                return element;
            }
            return list._element;
        }

        /// <summary>
        ///     Removes all the elements from this collection that are returned by
        ///     iterable.
        /// </summary>
        /// <remarks>
        ///     Removes all the elements from this collection that are returned by
        ///     iterable.
        /// </remarks>
        /// <param name="iterable"></param>
        public virtual void RemoveAll(IEnumerable iterable)
        {
            RemoveAll(iterable.GetEnumerator());
        }

        /// <summary>
        ///     Removes all the elements from this collection that are returned by
        ///     iterator.
        /// </summary>
        /// <remarks>
        ///     Removes all the elements from this collection that are returned by
        ///     iterator.
        /// </remarks>
        public virtual void RemoveAll(IEnumerator iterator)
        {
            while (iterator.MoveNext())
            {
                Remove(iterator.Current);
            }
        }

        public virtual void Replace(object oldObject, object newObject)
        {
            var list = Find(oldObject);
            if (list != null)
            {
                list._element = newObject;
            }
        }

        public virtual void ReplaceByIdentity(object oldObject, object newObject)
        {
            var list = FindByIdentity(oldObject);
            if (list != null)
            {
                list._element = newObject;
            }
        }

        private void AdjustOnRemoval(List4 previous, List4 removed)
        {
            if (removed == _first)
            {
                _first = removed._next;
            }
            else
            {
                previous._next = removed._next;
            }
            if (removed == _last)
            {
                _last = previous;
            }
        }

        public virtual int IndexOf(object obj)
        {
            var index = 0;
            var current = _first;
            while (current != null)
            {
                if (current.Holds(obj))
                {
                    return index;
                }
                index++;
                current = current._next;
            }
            return -1;
        }

        public override string ToString()
        {
            return Iterators.ToString(InternalIterator());
        }

        private void Changed()
        {
            ++_version;
        }

        internal virtual int Version()
        {
            return _version;
        }

        private void AssertNotNull(object element)
        {
            if (element == null)
            {
                throw new ArgumentNullException();
            }
        }

        /// <summary>
        ///     Leaner iterator for faster iteration (but unprotected against
        ///     concurrent modifications).
        /// </summary>
        /// <remarks>
        ///     Leaner iterator for faster iteration (but unprotected against
        ///     concurrent modifications).
        /// </remarks>
        private IEnumerator InternalIterator()
        {
            return new Iterator4Impl(_first);
        }
    }
}