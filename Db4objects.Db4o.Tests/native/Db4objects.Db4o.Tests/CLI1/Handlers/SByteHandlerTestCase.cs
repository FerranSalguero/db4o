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

using Db4objects.Db4o.Config;
using Db4objects.Db4o.Internal.Handlers;
using Db4objects.Db4o.Tests.Common.Handlers;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.CLI1.Handlers
{
    public class SByteHandlerTestCase : TypeHandlerTestCaseBase
    {
        protected override void Configure(IConfiguration config)
        {
            config.ExceptionsOnNotStorable(false);
        }

        public virtual void TestReadWrite()
        {
            var writeContext = new MockWriteContext(Db());
            sbyte expected = 0x11;
            SByteHandler().Write(writeContext, expected);
            var readContext = new MockReadContext(writeContext);
            var sbyteValue = (sbyte) SByteHandler().Read(readContext);
            Assert.AreEqual(expected, sbyteValue);
        }

        public virtual void TestStoreObject()
        {
            var storedItem = new Item(0x11, 0x22);
            DoTestStoreObject(storedItem);
        }

        private SByteHandler SByteHandler()
        {
            return new SByteHandler();
        }

        public class Item
        {
            public sbyte _sbyte;
            public sbyte _sbyteWrapper;

            public Item(sbyte s, sbyte wrapper)
            {
                _sbyte = s;
                _sbyteWrapper = wrapper;
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
                return (other._sbyte == _sbyte) && _sbyteWrapper.Equals(other._sbyteWrapper
                    );
            }

            public override string ToString()
            {
                return "[" + _sbyte + "," + _sbyteWrapper + "]";
            }
        }
    }
}