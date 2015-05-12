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

namespace Db4objects.Db4o.Tests.Common.Defragment
{
    public class DefragmentUntypedPrimitiveArrayTestCase : AbstractDb4oTestCase
    {
        private const int ItemSize = 42;

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            Store(new Item(ItemSize));
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestDefragment()
        {
            AssertItemSizes();
            Defragment();
            AssertItemSizes();
        }

        private void AssertItemSizes()
        {
            var item = (Item) RetrieveOnlyInstance(typeof (Item
                ));
            Assert.AreEqual(ItemSize, item._id);
            Assert.AreEqual(ItemSize, ((int[]) item._intData).Length);
            Assert.AreEqual(ItemSize - 1, ((int[]) item._intData)[ItemSize - 1]);
            Assert.AreEqual(ItemSize, ((byte[]) item._byteData).Length);
            Assert.AreEqual(ItemSize - 1, ((byte[]) item._byteData)[ItemSize - 1]);
            Assert.AreEqual(ItemSize.ToString(), item._name);
        }

        public class Item
        {
            public object _byteData;
            public int _id;
            public object _intData;
            public string _name;

            public Item(int size)
            {
                _id = size;
                _intData = new int[size];
                _byteData = new byte[size];
                for (var idx = 0; idx < size; idx++)
                {
                    ((int[]) _intData)[idx] = idx;
                    ((byte[]) _byteData)[idx] = (byte) idx;
                }
                _name = size.ToString();
            }
        }
    }
}