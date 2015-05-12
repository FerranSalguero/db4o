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

using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Querying
{
    /// <exclude></exclude>
    public class OrderedQueryTestCase : AbstractDb4oTestCase
    {
        public static void Main(string[] args)
        {
            new OrderedQueryTestCase().RunSolo();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            Db().Store(new Item(1));
            Db().Store(new Item(3));
            Db().Store(new Item(2));
        }

        public virtual void TestOrderAscending()
        {
            var query = NewQuery(typeof (Item));
            query.Descend("value").OrderAscending();
            AssertQuery(new[] {1, 2, 3}, query.Execute());
        }

        public virtual void TestOrderDescending()
        {
            var query = NewQuery(typeof (Item));
            query.Descend("value").OrderDescending();
            AssertQuery(new[] {3, 2, 1}, query.Execute());
        }

        public virtual void _testCOR1212()
        {
            Store(new Item2("Item 2"));
            var query = NewQuery();
            query.Constrain(typeof (Item)).Or(query.Constrain(typeof (Item2
                )));
            query.Descend("value").OrderAscending();
            var result = query.Execute();
            AssertQuery(new[] {1, 2, 3}, result);
        }

        private void AssertQuery(int[] expected, IObjectSet actual)
        {
            for (var i = 0; i < expected.Length; i++)
            {
                Assert.IsTrue(actual.HasNext());
                Assert.AreEqual(expected[i], ((Item) actual.Next()).value);
            }
            Assert.IsFalse(actual.HasNext());
        }

        public sealed class Item
        {
            public int value;

            public Item(int value)
            {
                this.value = value;
            }
        }

        public class Item2
        {
            public string _name;

            public Item2(string name)
            {
                _name = name;
            }
        }
    }
}