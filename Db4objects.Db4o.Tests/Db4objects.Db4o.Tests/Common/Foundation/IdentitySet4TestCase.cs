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
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Foundation
{
    public class IdentitySet4TestCase : ITestCase
    {
        public virtual void TestByIdentity()
        {
            var table = new IdentitySet4(2);
            var item1 = new Item(1);
            Assert.IsFalse(table.Contains(item1));
            table.Add(item1);
            Assert.IsTrue(table.Contains(item1));
            var item2 = new Item(2);
            Assert.IsFalse(table.Contains(item2));
            table.Add(item2);
            Assert.IsTrue(table.Contains(item2));
            Assert.AreEqual(2, table.Size());
            var size = 0;
            var i = table.GetEnumerator();
            while (i.MoveNext())
            {
                size++;
            }
            Assert.AreEqual(2, size);
        }

        public virtual void TestRemove()
        {
            var set = new IdentitySet4();
            var obj = new object();
            set.Add(obj);
            Assert.IsTrue(set.Contains(obj));
            set.Remove(obj);
            Assert.IsFalse(set.Contains(obj));
        }

        public virtual void TestIterator()
        {
            var set = new IdentitySet4();
            var o1 = new object();
            var o2 = new object();
            set.Add(o1);
            set.Add(o2);
            Iterator4Assert.SameContent(Iterators.Iterate(new[] {o1, o2}), set.GetEnumerator
                ());
        }

        public class Item
        {
            internal int _id;

            public Item(int id)
            {
                _id = id;
            }

            public override int GetHashCode()
            {
                return _id;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Item))
                {
                    return false;
                }
                var other = (Item) obj;
                return _id == other._id;
            }
        }
    }
}