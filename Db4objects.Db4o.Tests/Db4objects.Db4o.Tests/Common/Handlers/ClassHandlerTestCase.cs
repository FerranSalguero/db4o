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

namespace Db4objects.Db4o.Tests.Common.Handlers
{
    public class ClassHandlerTestCase : AbstractDb4oTestCase
    {
        public static void Main(string[] args)
        {
            new ClassHandlerTestCase().RunSolo();
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestStoreObject()
        {
            var expectedItem = new Item("parent",
                new Item("child", null));
            Db().Store(expectedItem);
            Db().Purge(expectedItem);
            var q = Db().Query();
            q.Constrain(typeof (Item));
            q.Descend("_name").Constrain("parent");
            var objectSet = q.Execute();
            var readItem = (Item) objectSet.Next();
            Assert.AreNotSame(expectedItem, readItem);
            AssertAreEqual(expectedItem, readItem);
        }

        private void AssertAreEqual(Item expectedItem, Item
            readItem)
        {
            Assert.AreEqual(expectedItem._name, readItem._name);
            Assert.AreEqual(expectedItem._child._name, readItem._child._name);
        }

        public class Item
        {
            public Item _child;
            public string _name;

            public Item(string name, Item child)
            {
                _name = name;
                _child = child;
            }
        }
    }
}