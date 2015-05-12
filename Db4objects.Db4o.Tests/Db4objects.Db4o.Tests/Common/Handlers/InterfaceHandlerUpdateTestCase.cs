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

using Db4objects.Db4o.Ext;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Handlers
{
    public class InterfaceHandlerUpdateTestCase : HandlerUpdateTestCaseBase
    {
        protected override object[] CreateValues()
        {
            return new object[] {ItemContainer.CreateNew()};
        }

        protected override object CreateArrays()
        {
            return ItemContainer.CreateNew();
        }

        protected override void AssertArrays(IExtObjectContainer objectContainer, object
            obj)
        {
            if (Db4oMajorVersion() == 4)
            {
                return;
            }
            AssertItemInterfaceArrays(StoredItemName(), obj);
        }

        private void AssertItemInterfaceArrays(string name, object itemContainerObject)
        {
            var itemContainer = (ItemContainer
                ) itemContainerObject;
            AssertItemInterfaceArray(name, itemContainer._items);
            AssertItemInterfaceArray(name, itemContainer._objects);
            AssertItemInterfaceArray(name, (object[]) itemContainer._object);
        }

        protected override void AssertValues(IExtObjectContainer objectContainer, object[]
            values)
        {
            if (Db4oMajorVersion() == 4)
            {
                return;
            }
            AssertItem(StoredItemName(), ItemFromValues(values));
        }

        protected override void UpdateValues(object[] values)
        {
            if (Db4oMajorVersion() == 4)
            {
                return;
            }
            UpdateItem(ItemFromValues(values));
        }

        private void UpdateItem(Item item)
        {
            item._name = UpdatedItemName();
        }

        private string UpdatedItemName()
        {
            return "updated";
        }

        protected override void AssertUpdatedValues(object[] values)
        {
            if (Db4oMajorVersion() == 4)
            {
                return;
            }
            AssertItem(UpdatedItemName(), ItemFromValues(values));
        }

        protected override void UpdateArrays(object obj)
        {
            if (Db4oMajorVersion() == 4)
            {
                return;
            }
            var itemContainer = (ItemContainer
                ) obj;
            UpdateItemInterfaceArray(itemContainer._items);
            UpdateItemInterfaceArray(itemContainer._objects);
            UpdateItemInterfaceArray((object[]) itemContainer._object);
        }

        protected override void AssertUpdatedArrays(object obj)
        {
            if (Db4oMajorVersion() == 4)
            {
                return;
            }
            AssertItemInterfaceArrays(UpdatedItemName(), obj);
        }

        private Item ItemFromValues(object[] values)
        {
            var itemContainer = (ItemContainer
                ) values[0];
            var item = itemContainer._item;
            return (Item) item;
        }

        private void AssertItem(string name, object item)
        {
            Assert.IsInstanceOf(typeof (Item), item);
            Assert.AreEqual(name, ((Item) item)._name);
        }

        private void AssertItemInterfaceArray(string itemName, object[] items)
        {
            AssertItem(itemName, items[0]);
        }

        private void UpdateItemInterfaceArray(object[] items)
        {
            UpdateItem((Item) items[0]);
        }

        protected override string TypeName()
        {
            return "interface";
        }

        public static Item StoredItem()
        {
            return new Item(StoredItemName());
        }

        private static string StoredItemName()
        {
            return "stored";
        }

        public interface IItemInterface
        {
        }

        public class ItemContainer
        {
            internal IItemInterface _item;
            internal IItemInterface[] _items;
            internal object _object;
            internal object[] _objects;

            public static ItemContainer CreateNew()
            {
                var itemContainer = new ItemContainer
                    ();
                itemContainer._item = StoredItem();
                itemContainer._items = NewItemInterfaceArray();
                itemContainer._objects = NewItemInterfaceArray();
                itemContainer._object = NewItemInterfaceArray();
                return itemContainer;
            }

            private static IItemInterface[] NewItemInterfaceArray
                ()
            {
                return new IItemInterface[] {StoredItem()};
            }
        }

        public class Item : IItemInterface
        {
            public string _name;

            public Item(string name)
            {
                _name = name;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Item))
                {
                    return false;
                }
                return _name.Equals(((Item) obj)._name);
            }

            public override string ToString()
            {
                return "Item " + _name;
            }
        }
    }
}