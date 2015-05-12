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

namespace Db4objects.Db4o.Tests.Common.Assorted
{
    public class CascadedDeleteReadTestCase : AbstractDb4oTestCase
    {
        public static void Main(string[] args)
        {
            new CascadedDeleteReadTestCase().RunSoloAndClientServer();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            base.Configure(config);
            config.ObjectClass(typeof (Item)).ObjectField("_child1"
                ).CascadeOnDelete(true);
            config.ObjectClass(typeof (Item)).ObjectField("_child2"
                ).CascadeOnDelete(true);
            config.ObjectClass(typeof (Item)).ObjectField("_child1"
                ).CascadeOnUpdate(true);
            config.ObjectClass(typeof (Item)).ObjectField("_child2"
                ).CascadeOnUpdate(true);
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            Store(new Item(new Item("1"
                ), null, "parent"));
        }

        public virtual void Test()
        {
            var item = ParentItem();
            item._child2 = item._child1;
            item._child1 = null;
            Store(item);
            Db().Delete(item);
            AssertItemCount(0);
        }

        private Item ParentItem()
        {
            var q = Db().Query();
            q.Constrain(typeof (Item));
            q.Descend("_name").Constrain("parent");
            return (Item) q.Execute().Next();
        }

        private void AssertItemCount(int count)
        {
            var q = Db().Query();
            q.Constrain(typeof (Item));
            var objectSet = q.Execute();
            Assert.AreEqual(count, objectSet.Count);
        }

        public class Item
        {
            public Item _child1;
            public Item _child2;
            public string _name;

            public Item()
            {
            }

            public Item(string name)
            {
                _name = name;
            }

            public Item(Item child1, Item
                child2, string name)
            {
                _child1 = child1;
                _child2 = child2;
                _name = name;
            }
        }
    }
}