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

namespace Db4objects.Db4o.Foundation
{
    public class IntIterators
    {
        public static IFixedSizeIntIterator4 ForInts(int[] array, int count)
        {
            return new IntIterator4Impl(array, count);
        }

        public static IIntIterator4 ForLongs(long[] ids)
        {
            return new _IIntIterator4_10(ids);
        }

        private sealed class _IIntIterator4_10 : IIntIterator4
        {
            private readonly long[] ids;
            internal int _current;
            internal int _next;

            public _IIntIterator4_10(long[] ids)
            {
                this.ids = ids;
                _next = 0;
            }

            public int CurrentInt()
            {
                return _current;
            }

            public object Current
            {
                get { return _current; }
            }

            public bool MoveNext()
            {
                if (_next < ids.Length)
                {
                    _current = (int) ids[_next];
                    ++_next;
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }
    }
}