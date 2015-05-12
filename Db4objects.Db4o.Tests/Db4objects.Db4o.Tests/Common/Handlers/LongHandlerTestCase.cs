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

using Db4objects.Db4o.Internal.Handlers;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Handlers
{
    public class LongHandlerTestCase : TypeHandlerTestCaseBase
    {
        public static void Main(string[] args)
        {
            new LongHandlerTestCase().RunSolo();
        }

        private LongHandler LongHandler()
        {
            return new LongHandler();
        }

        public virtual void TestReadWrite()
        {
            var writeContext = new MockWriteContext(Db());
            var expected = unchecked(0x1020304050607080l);
            LongHandler().Write(writeContext, expected);
            var readContext = new MockReadContext(writeContext);
            var longValue = (long) LongHandler().Read(readContext);
            Assert.AreEqual(expected, longValue);
        }

        public virtual void TestStoreObject()
        {
            var storedItem = new Item(unchecked(0x1020304050607080l), unchecked(0x1122334455667788l));
            DoTestStoreObject(storedItem);
        }

        public class Item
        {
            public long _long;
            public long _longWrapper;

            public Item(long l, long wrapper)
            {
                _long = l;
                _longWrapper = wrapper;
            }

            public override bool Equals(object obj)
            {
                if (obj == this)
                {
                    return true;
                }
                if (!(obj is Item))
                {
                    return false;
                }
                var other = (Item) obj;
                return (other._long == _long) && _longWrapper.Equals(other._longWrapper
                    );
            }

            public override string ToString()
            {
                return "[" + _long + "," + _longWrapper + "]";
            }
        }
    }
}