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
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Internal.Handlers;
using Db4objects.Db4o.Tests.Common.Handlers;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.CLI1.Handlers
{
    public class DateTimeHandlerTestCase : TypeHandlerTestCaseBase
    {
        protected override void Configure(IConfiguration config)
        {
            config.ExceptionsOnNotStorable(false);
        }

        public virtual void TestReadWrite()
        {
            var writeContext = new MockWriteContext(Db());
            var expected = new DateTime();
            DateTimeHandler().Write(writeContext, expected);
            var readContext = new MockReadContext(writeContext);
            var actual = (DateTime) DateTimeHandler().Read(readContext);
            Assert.AreEqual(expected, actual);
        }

        public virtual void TestStoreObject()
        {
            var storedItem = new Item(new DateTime());
            DoTestStoreObject(storedItem);
        }

        public void TestKind()
        {
            var kind = DateTimeKind.Utc;
            var storedDateTime = DateTime.SpecifyKind(new DateTime(), kind);
            var storedItem = new Item(storedDateTime);
            Store(storedItem);
            Reopen();
            var retrievedItem = (Item) RetrieveOnlyInstance(typeof (Item));
            Assert.AreEqual(kind, retrievedItem._dateTime.Kind);
        }

        private DateTimeHandler DateTimeHandler()
        {
            return new DateTimeHandler();
        }

        public class Item
        {
            public DateTime _dateTime;

            public Item(DateTime wrapper)
            {
                _dateTime = wrapper;
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
                return (other._dateTime == _dateTime);
            }

            public override string ToString()
            {
                return "[" + _dateTime + "]";
            }
        }
    }
}