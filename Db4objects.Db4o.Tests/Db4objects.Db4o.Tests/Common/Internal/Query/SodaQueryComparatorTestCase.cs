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

using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal.Query;
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;

namespace Db4objects.Db4o.Tests.Common.Internal.Query
{
    public class SodaQueryComparatorTestCase : AbstractDb4oTestCase, IOptOutMultiSession
    {
        internal readonly IFunction4 oidByItemId;

        public SodaQueryComparatorTestCase()
        {
            oidByItemId = new _IFunction4_121(this);
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            StoreItem(1, "bb", "ca");
            StoreItem(2, "aa", "cb");
        }

        public virtual void TestNullInThePath()
        {
            Store(new Item(3, "cc", null));
            int[] expectedItemIds = {3, 1, 2};
            AssertQuery(expectedItemIds, new[] {Ascending(new[] {"child", "name"})});
        }

        public virtual void TestFirstLevelAscending()
        {
            int[] expectedItems = {2, 1};
            AssertQuery(expectedItems, new[] {Ascending(new[] {"name"})});
        }

        public virtual void TestSecondLevelAscending()
        {
            int[] expectedItems = {1, 2};
            AssertQuery(expectedItems, new[] {Ascending(new[] {"child", "name"})});
        }

        public virtual void TestFirstLevelThenSecondLevel()
        {
            StoreItem(3, "aa", "cc");
            StoreItem(4, "bb", "cc");
            int[] expectedItems = {2, 3, 1, 4};
            AssertQuery(expectedItems, new[] {Ascending(new[] {"name"}), Ascending(new[] {"child", "name"})});
        }

        public virtual void TestSecondLevelThenFirstLevel()
        {
            StoreItem(3, "cc", "ca");
            StoreItem(4, "cc", "ce");
            int[] expectedItems = {1, 3, 2, 4};
            AssertQuery(expectedItems, new[] {Ascending(new[] {"child", "name"}), Ascending(new[] {"name"})});
        }

        public virtual void TestFirstLevelDescending()
        {
            int[] expectedItems = {1, 2};
            AssertQuery(expectedItems, new[] {Descending(new[] {"name"})});
        }

        public virtual void TestSecondLevelDescending()
        {
            int[] expectedItems = {2, 1};
            AssertQuery(expectedItems, new[] {Descending(new[] {"child", "name"})});
        }

        public virtual void TestFirstLevelThenSecondLevelDescending()
        {
            StoreItem(3, "aa", "cc");
            StoreItem(4, "bb", "cc");
            int[] expectedItems = {4, 1, 3, 2};
            AssertQuery(expectedItems, new[] {Descending(new[] {"name"}), Descending(new[] {"child", "name"})});
        }

        public virtual void TestSecondLevelThenFirstLevelDescending()
        {
            StoreItem(3, "cc", "ca");
            StoreItem(4, "cc", "ce");
            int[] expectedItems = {4, 2, 3, 1};
            AssertQuery(expectedItems, new[] {Descending(new[] {"child", "name"}), Descending(new[] {"name"})});
        }

        public virtual void TestFirstLevelAscendingThenSecondLevelDescending()
        {
            StoreItem(3, "aa", "cc");
            StoreItem(4, "bb", "cc");
            int[] expectedItems = {3, 2, 4, 1};
            AssertQuery(expectedItems, new[] {Ascending(new[] {"name"}), Descending(new[] {"child", "name"})});
        }

        public virtual void TestSecondLevelAscendingThenFirstLevelDescending()
        {
            StoreItem(3, "cc", "ca");
            StoreItem(4, "cc", "ce");
            int[] expectedItems = {3, 1, 2, 4};
            AssertQuery(expectedItems, new[] {Ascending(new[] {"child", "name"}), Descending(new[] {"name"})});
        }

        private SodaQueryComparator.Ordering Ascending(string[] fieldPath)
        {
            return new SodaQueryComparator.Ordering(SodaQueryComparator.Direction.Ascending,
                fieldPath);
        }

        private SodaQueryComparator.Ordering Descending(string[] fieldPath)
        {
            return new SodaQueryComparator.Ordering(SodaQueryComparator.Direction.Descending,
                fieldPath);
        }

        private void StoreItem(int id, string name, string childName)
        {
            Store(new Item(id, name, new ItemChild
                (childName)));
        }

        private void AssertQuery(int[] expectedItemIds, SodaQueryComparator.Ordering[] orderings
            )
        {
            var ids = NewQuery(typeof (Item)).Execute().Ext().GetIDs
                ();
            var sorted = new SodaQueryComparator(FileSession(), typeof (Item
                ), orderings).Sort(ids);
            Iterator4Assert.AreEqual(Iterators.Map(expectedItemIds, oidByItemId), Iterators.Iterator
                (sorted));
        }

        private int ItemByName(int id)
        {
            var query = NewQuery(typeof (Item));
            query.Descend("id").Constrain(id);
            return (int) query.Execute().Ext().GetIDs()[0];
        }

        private sealed class _IFunction4_121 : IFunction4
        {
            private readonly SodaQueryComparatorTestCase _enclosing;

            public _IFunction4_121(SodaQueryComparatorTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public object Apply(object id)
            {
                var oid = _enclosing.ItemByName(((int) id));
                //			System.out.println(id + " -> " + oid);
                return oid;
            }
        }

        public class Item
        {
            public ItemChild child;
            public int id;
            public string name;

            public Item(int id, string name, ItemChild child)
            {
                this.id = id;
                this.name = name;
                this.child = child;
            }
        }

        public class ItemChild
        {
            public string name;

            public ItemChild(string name)
            {
                this.name = name;
            }
        }
    }
}