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
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Updatedepth
{
    public class UpdateDepthWithCascadingDeleteTestCase : AbstractDb4oTestCase
    {
        private const int ChildId = 2;
        private const int RootId = 1;

        protected override void Configure(IConfiguration config)
        {
            config.ObjectClass(typeof (Item)).CascadeOnDelete
                (true);
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            Store(new Item(RootId, new Item
                (ChildId, null)));
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestUpdateDepth()
        {
            var item = QueryItemByID(RootId);
            var changedRootID = 42;
            item._id = changedRootID;
            item._item._id = 43;
            Store(item);
            Reopen();
            var changed = QueryItemByID(changedRootID
                );
            Assert.AreEqual(ChildId, changed._item._id);
        }

        private Item QueryItemByID(int id)
        {
            var query = NewQuery(typeof (Item));
            query.Descend("_id").Constrain(id);
            var result = query.Execute();
            Assert.IsTrue(result.HasNext());
            var item = ((Item
                ) result.Next());
            return item;
        }

        public class Item
        {
            public int _id;
            public Item _item;

            public Item(int id, Item item)
            {
                _id = id;
                _item = item;
            }
        }
    }
}