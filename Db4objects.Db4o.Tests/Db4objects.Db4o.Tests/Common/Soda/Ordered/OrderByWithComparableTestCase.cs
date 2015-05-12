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

namespace Db4objects.Db4o.Tests.Common.Soda.Ordered
{
    public class OrderByWithComparableTestCase : AbstractDb4oTestCase
    {
        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            Store(new Item(1, new ItemComparable
                (1)));
            Store(new Item(2, null));
            Store(new Item(3, new ItemComparable
                (2)));
            Store(new Item(4, null));
        }

        public virtual void TestOrderByWithEnums()
        {
            var query = NewQuery();
            query.Constrain(typeof (Item));
            query.Descend("_id").Constrain(1).Or(query.Descend("_id").Constrain(3));
            query.Descend("_itemCmp").OrderAscending();
            var result = query.Execute();
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(1, ((Item) result.Next()).ItemCmp().
                Id());
            Assert.AreEqual(2, ((Item) result.Next()).ItemCmp().
                Id());
        }

        public virtual void TestOrderByWithNullValues()
        {
            var query = NewQuery();
            query.Constrain(typeof (Item));
            query.Descend("_itemCmp").OrderAscending();
            var result = query.Execute();
            Assert.AreEqual(4, result.Count);
            Assert.IsNull(((Item) result.Next()).ItemCmp());
            Assert.IsNull(((Item) result.Next()).ItemCmp());
            Assert.AreEqual(1, ((Item) result.Next()).ItemCmp().
                Id());
            Assert.AreEqual(2, ((Item) result.Next()).ItemCmp().
                Id());
        }

        public class ItemComparable : IComparable
        {
            public int _id;

            public ItemComparable(int id)
            {
                _id = id;
            }

            public virtual int CompareTo(object other)
            {
                var cmp = (ItemComparable
                    ) other;
                if (_id == cmp._id)
                {
                    return 0;
                }
                return _id < cmp._id ? -1 : 1;
            }

            public virtual int Id()
            {
                return _id;
            }
        }

        public class Item
        {
            public int _id;
            public ItemComparable _itemCmp;

            public Item(int id, ItemComparable itemCmp)
            {
                _id = id;
                _itemCmp = itemCmp;
            }

            public virtual ItemComparable ItemCmp()
            {
                return _itemCmp;
            }
        }
    }
}