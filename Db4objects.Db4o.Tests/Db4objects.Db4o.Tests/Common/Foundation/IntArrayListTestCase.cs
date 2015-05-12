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
    /// <exclude></exclude>
    public class IntArrayListTestCase : ITestCase
    {
        public static void Main(string[] args)
        {
            new ConsoleTestRunner(typeof (IntArrayListTestCase)).Run();
        }

        public virtual void TestIteratorGoesForwards()
        {
            var list = new IntArrayList();
            AssertIterator(new int[] {}, list.IntIterator());
            list.Add(1);
            AssertIterator(new[] {1}, list.IntIterator());
            list.Add(2);
            AssertIterator(new[] {1, 2}, list.IntIterator());
        }

        private void AssertIterator(int[] expected, IIntIterator4 iterator)
        {
            for (var i = 0; i < expected.Length; ++i)
            {
                Assert.IsTrue(iterator.MoveNext());
                Assert.AreEqual(expected[i], iterator.CurrentInt());
                Assert.AreEqual(expected[i], iterator.Current);
            }
            Assert.IsFalse(iterator.MoveNext());
        }

        //test mthod add(int,int)
        public virtual void TestAddAtIndex()
        {
            var list = new IntArrayList();
            for (var i = 0; i < 10; i++)
            {
                list.Add(i);
            }
            list.Add(3, 100);
            Assert.AreEqual(100, list.Get(3));
            for (var i = 4; i < 11; i++)
            {
                Assert.AreEqual(i - 1, list.Get(i));
            }
        }
    }
}