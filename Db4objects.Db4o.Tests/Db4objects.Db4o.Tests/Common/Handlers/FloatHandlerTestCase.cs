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
    public class FloatHandlerTestCase : TypeHandlerTestCaseBase
    {
        public static void Main(string[] args)
        {
            new FloatHandlerTestCase().RunSolo();
        }

        private FloatHandler FloatHandler()
        {
            return new FloatHandler();
        }

        public virtual void TestReadWrite()
        {
            var writeContext = new MockWriteContext(Db());
            var expected = float.MaxValue;
            FloatHandler().Write(writeContext, expected);
            var readContext = new MockReadContext(writeContext);
            var f = (float) FloatHandler().Read(readContext);
            Assert.AreEqual(expected, f);
        }

        public virtual void TestStoreObject()
        {
            var storedItem = new Item(1.23456789f,
                1.23456789f);
            DoTestStoreObject(storedItem);
        }

        public class Item
        {
            public float _float;
            public float _floatWrapper;

            public Item(float f, float wrapper)
            {
                _float = f;
                _floatWrapper = wrapper;
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
                return (other._float == _float) && _floatWrapper.Equals(other._floatWrapper
                    );
            }

            public override string ToString()
            {
                return "[" + _float + "," + _floatWrapper + "]";
            }
        }
    }
}