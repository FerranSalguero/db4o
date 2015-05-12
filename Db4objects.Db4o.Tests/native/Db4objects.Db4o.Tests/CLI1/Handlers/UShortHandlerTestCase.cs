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
    public class UShortHandlerTestCase : TypeHandlerTestCaseBase
    {
        protected override void Configure(IConfiguration config)
        {
            config.ExceptionsOnNotStorable(false);
        }

        public virtual void TestReadWrite()
        {
            var writeContext = new MockWriteContext(Db());
            ushort expected = 0x1122;
            UShortHandler().Write(writeContext, expected);
            var readContext = new MockReadContext(writeContext);
            var ushortValue = (ushort) UShortHandler().Read(readContext);
            Assert.AreEqual(expected, ushortValue);
        }

        public virtual void TestStoreObject()
        {
            var storedItem = new ULongHandlerTestCase.Item(0x1122, 0x8877);
            DoTestStoreObject(storedItem);
        }

        private UShortHandler UShortHandler()
        {
            return new UShortHandler();
        }

        public class Item
        {
            public ushort _ushort;
            public ushort _ushortWrapper;

            public Item(ushort u, ushort wrapper)
            {
                _ushort = u;
                _ushortWrapper = wrapper;
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
                return (other._ushort == _ushort) && _ushortWrapper.Equals(other._ushortWrapper
                    );
            }

            public override string ToString()
            {
                return "[" + _ushort + "," + _ushortWrapper + "]";
            }
        }
    }
}