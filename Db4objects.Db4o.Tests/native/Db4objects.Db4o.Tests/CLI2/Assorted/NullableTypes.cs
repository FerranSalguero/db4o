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
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.CLI2.Assorted
{
    public class NullableContainer
    {
        public DateTime? dateValue;
        public int? intValue;

        public NullableContainer(int value)
        {
            intValue = value;
        }

        public NullableContainer(DateTime value)
        {
            dateValue = value;
        }
    }

    public class NullableTypes : AbstractDb4oTestCase
    {
        private static readonly DateTime TheDate = new DateTime(1983, 3, 7);

        protected override void Store()
        {
            Db().Store(new NullableContainer(42));
            Db().Store(new NullableContainer(TheDate));
        }

        public void TestGlobalQuery()
        {
            var query = NewQuery();
            query.Constrain(typeof (NullableContainer));

            var os = query.Execute();
            Assert.AreEqual(2, os.Count);

            var foundInt = false;
            var foundDate = false;
            while (os.HasNext())
            {
                var item = (NullableContainer) os.Next();
                if (item.intValue.HasValue)
                {
                    Assert.AreEqual(42, item.intValue.Value);
                    Assert.IsFalse(item.dateValue.HasValue);
                    foundInt = true;
                }
                else if (item.dateValue.HasValue)
                {
                    Assert.AreEqual(TheDate, item.dateValue.Value);
                    Assert.IsFalse(item.intValue.HasValue);
                    foundDate = true;
                }
            }

            Assert.IsTrue(foundInt);
            Assert.IsTrue(foundDate);
        }

        public void TestDateQuery()
        {
            var os = Db().QueryByExample(new NullableContainer(TheDate));
            CheckDateValueQueryResult(os);
        }

        private static void CheckDateValueQueryResult(IObjectSet os)
        {
            Assert.AreEqual(1, os.Count);
            var found = (NullableContainer) os.Next();
            Assert.AreEqual(TheDate, found.dateValue.Value);
            EnsureIsNull(found.intValue);
        }

        public void TestIntQuery()
        {
            var os = Db().QueryByExample(new NullableContainer(42));
            CheckIntValueQueryResult(os);
        }

        public void TestSodaQuery()
        {
            var q = NewQuery(typeof (NullableContainer));
            q.Descend("intValue").Constrain(42);
            CheckIntValueQueryResult(q.Execute());
        }

        public void TestSodaQueryWithNullConstrain()
        {
            var q = NewQuery(typeof (NullableContainer));
            q.Descend("intValue").Constrain(null);
            CheckDateValueQueryResult(q.Execute());
        }

        private static void CheckIntValueQueryResult(IObjectSet os)
        {
            Assert.AreEqual(1, os.Count);
            var found = (NullableContainer) os.Next();
            Assert.AreEqual(42, found.intValue.Value);
            EnsureIsNull(found.dateValue);
        }

        private static void EnsureIsNull<T>(T? value) where T : struct
        {
            Assert.IsFalse(value.HasValue, "!nullable.HasValue");
        }
    }
}