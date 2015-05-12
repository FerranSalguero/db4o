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
using Db4objects.Db4o.Ext;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Handlers
{
    public class DeletionUponFormatMigrationTestCase : FormatMigrationTestCaseBase
    {
        private const int ItemsToKeepCount = 3;
        private const int IdToBeDeleted = 42;
        private const int IdToBeKept = unchecked(0xdb40);
        private static readonly string ChildToBeKept = "bar";
        private static readonly string ChildToBeDelete = "foo";

        protected override void ConfigureForTest(IConfiguration config)
        {
            config.ObjectClass(typeof (Item)).CascadeOnDelete
                (true);
        }

        protected override void AssertObjectsAreReadable(IExtObjectContainer objectContainer
            )
        {
            if (Db4oMajorVersion() < 5 || (Db4oMajorVersion() == 5 && Db4oMinorVersion() < 4))
            {
                return;
            }
            AssertChildItem(objectContainer, ChildToBeDelete, false);
            AssertChildItem(objectContainer, ChildToBeKept, true);
            AssertReferenceToDeletedObjectSetToNull(objectContainer);
            AssertCascadeDeletionOnArrays(objectContainer);
        }

        private void AssertCascadeDeletionOnArrays(IExtObjectContainer objectContainer)
        {
            var keptItems = ItemByIdGreaterThan(objectContainer, IdToBeKept);
            Assert.AreEqual(0, keptItems.Count);
        }

        private void AssertReferenceToDeletedObjectSetToNull(IExtObjectContainer objectContainer
            )
        {
            var item = ItemById(objectContainer, IdToBeKept
                );
            Assert.IsNotNull(item);
            Assert.AreEqual(1, item._array.Length);
            Assert.IsNull(item._array[0]);
        }

        protected override void AssertObjectDeletion(IExtObjectContainer objectContainer)
        {
            var item = ItemById(objectContainer, IdToBeDeleted
                );
            Assert.IsNotNull(item._child);
            Assert.IsNotNull(item._array[0]);
            objectContainer.Delete(item);
        }

        private void AssertChildItem(IExtObjectContainer objectContainer, string name, bool
            expectToBeFound)
        {
            var query = objectContainer.Query();
            query.Constrain(typeof (ChildItem));
            query.Descend("_name").Constrain(name);
            var result = query.Execute();
            Assert.AreEqual(expectToBeFound, result.HasNext(), name);
            if (expectToBeFound)
            {
                var childItem = (ChildItem
                    ) result.Next();
                Assert.AreEqual(name, childItem._name);
            }
        }

        private Item ItemById(IExtObjectContainer objectContainer
            , int id)
        {
            var query = objectContainer.Query();
            query.Constrain(typeof (Item));
            query.Descend("_id").Constrain(id);
            var result = query.Execute();
            Assert.AreEqual(1, result.Count);
            return (Item) result.Next();
        }

        private IObjectSet ItemByIdGreaterThan(IExtObjectContainer objectContainer, int id
            )
        {
            var query = objectContainer.Query();
            query.Constrain(typeof (Item));
            query.Descend("_id").Constrain(id).Greater();
            return query.Execute();
        }

        protected override string FileNamePrefix()
        {
            return "deletion-tests";
        }

        protected override void Store(IObjectContainerAdapter objectContainer)
        {
            var item1 = new Item
                (IdToBeDeleted, new ChildItem(ChildToBeDelete
                    ), ItemsToKeep());
            objectContainer.Store(item1, 10);
            var item2 = new Item
                (IdToBeKept, new ChildItem(ChildToBeKept), new[] {item1});
            objectContainer.Store(item2, 10);
        }

        private Item[] ItemsToKeep()
        {
            var items = new Item
                [ItemsToKeepCount];
            for (var i = 1; i <= items.Length; i++)
            {
                items[i - 1] = new Item(IdToBeKept + i);
            }
            return items;
        }

        public class Item
        {
            public Item[] _array;
            public object _child;
            public int _id;

            public Item(int id, ChildItem child, Item
                [] items) : this(id)
            {
                _child = child;
                _array = items;
            }

            public Item(int id)
            {
                _id = id;
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                {
                    return false;
                }
                if (!obj.GetType().Equals(typeof (Item)))
                {
                    return false;
                }
                var other = (Item
                    ) obj;
                return other._id == _id;
            }
        }

        public class ChildItem
        {
            public string _name;

            public ChildItem(string name)
            {
                _name = name;
            }
        }
    }
}