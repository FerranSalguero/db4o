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
    public class DecimalHandlerTestCase : TypeHandlerTestCaseBase
    {
        protected override void Configure(IConfiguration config)
        {
            config.ExceptionsOnNotStorable(false);
        }

        public virtual void TestReadWrite()
        {
            var writeContext = new MockWriteContext(Db());
            var expected = decimal.MaxValue;
            DecimalHandler().Write(writeContext, expected);
            var readContext = new MockReadContext(writeContext);
            var decimalValue = (decimal) DecimalHandler().Read(readContext);
            Assert.AreEqual(expected, decimalValue);
        }

        public virtual void TestStoreObject()
        {
            var storedItem = new Item(decimal.MaxValue, decimal.MinValue);
            DoTestStoreObject(storedItem);
        }

        private DecimalHandler DecimalHandler()
        {
            return new DecimalHandler();
        }

        public class Item
        {
            public decimal _decimal;
            public decimal _decimalWrapper;

            public Item(decimal d, decimal wrapper)
            {
                _decimal = d;
                _decimalWrapper = wrapper;
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
                return (other._decimal == _decimal) && _decimalWrapper.Equals(other._decimalWrapper);
            }

            public override string ToString()
            {
                return "[" + _decimal + "," + _decimalWrapper + "]";
            }
        }
    }
}