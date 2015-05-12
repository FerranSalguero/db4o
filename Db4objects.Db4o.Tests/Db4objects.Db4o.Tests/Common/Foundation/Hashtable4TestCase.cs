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
    public class Hashtable4TestCase : ITestCase
    {
        public static void Main(string[] args)
        {
            new ConsoleTestRunner(typeof (Hashtable4TestCase)).Run();
        }

        public virtual void TestClear()
        {
            var table = new Hashtable4();
            for (var i = 0; i < 2; ++i)
            {
                table.Clear();
                Assert.AreEqual(0, table.Size());
                table.Put("foo", "bar");
                Assert.AreEqual(1, table.Size());
                AssertIterator(table, new object[] {"foo"});
            }
        }

        public virtual void TestToString()
        {
            var table = new Hashtable4();
            table.Put("foo", "bar");
            table.Put("bar", "baz");
            Assert.AreEqual(Iterators.Join(table.Iterator(), "{", "}", ", "), table.ToString(
                ));
        }

        public virtual void TestContainsKey()
        {
            var table = new Hashtable4();
            Assert.IsFalse(table.ContainsKey(null));
            Assert.IsFalse(table.ContainsKey("foo"));
            table.Put("foo", null);
            Assert.IsTrue(table.ContainsKey("foo"));
            table.Put("bar", "baz");
            Assert.IsTrue(table.ContainsKey("bar"));
            Assert.IsFalse(table.ContainsKey("baz"));
            Assert.IsTrue(table.ContainsKey("foo"));
            table.Remove("foo");
            Assert.IsTrue(table.ContainsKey("bar"));
            Assert.IsFalse(table.ContainsKey("foo"));
        }

        public virtual void TestByteArrayKeys()
        {
            byte[] key1 = {1, 2, 3};
            byte[] key2 = {3, 2, 1};
            byte[] key3 = {3, 2, 1};
            // same values as key2
            var table = new Hashtable4(2);
            table.Put(key1, "foo");
            table.Put(key2, "bar");
            Assert.AreEqual("foo", table.Get(key1));
            Assert.AreEqual("bar", table.Get(key2));
            Assert.AreEqual(2, CountKeys(table));
            Assert.AreEqual(2, table.Size());
            table.Put(key3, "baz");
            Assert.AreEqual("foo", table.Get(key1));
            Assert.AreEqual("baz", table.Get(key2));
            Assert.AreEqual(2, CountKeys(table));
            Assert.AreEqual(2, table.Size());
            Assert.AreEqual("baz", table.Remove(key2));
            Assert.AreEqual(1, CountKeys(table));
            Assert.AreEqual(1, table.Size());
            Assert.AreEqual("foo", table.Remove(key1));
            Assert.AreEqual(0, CountKeys(table));
            Assert.AreEqual(0, table.Size());
        }

        public virtual void TestIterator()
        {
            AssertIsIteratable(new object[0]);
            AssertIsIteratable(new object[] {"one"});
            AssertIsIteratable(new object[] {1, 3, 2});
            AssertIsIteratable(new object[] {"one", "three", "two"});
            AssertIsIteratable(new object[]
            {
                new Key(1), new Key
                    (3),
                new Key(2)
            });
        }

        public virtual void TestSameKeyTwice()
        {
            var key = 1;
            var table = new Hashtable4();
            table.Put(key, "foo");
            table.Put(key, "bar");
            Assert.AreEqual("bar", table.Get(key));
            Assert.AreEqual(1, CountKeys(table));
        }

        public virtual void TestSameHashCodeIterator()
        {
            var keys = CreateKeys(1, 5);
            AssertIsIteratable(keys);
        }

        private Key[] CreateKeys(int begin, int end)
        {
            var factor = 10;
            var count = (end - begin);
            var keys = new Key[count*factor];
            for (var i = 0; i < count; ++i)
            {
                var baseIndex = i*factor;
                for (var j = 0; j < factor; ++j)
                {
                    keys[baseIndex + j] = new Key(begin + i);
                }
            }
            return keys;
        }

        public virtual void TestDifferentKeysSameHashCode()
        {
            var key1 = new Key(1);
            var key2 = new Key(1);
            var key3 = new Key(2);
            var table = new Hashtable4(2);
            table.Put(key1, "foo");
            table.Put(key2, "bar");
            Assert.AreEqual("foo", table.Get(key1));
            Assert.AreEqual("bar", table.Get(key2));
            Assert.AreEqual(2, CountKeys(table));
            table.Put(key2, "baz");
            Assert.AreEqual("foo", table.Get(key1));
            Assert.AreEqual("baz", table.Get(key2));
            Assert.AreEqual(2, CountKeys(table));
            table.Put(key1, "spam");
            Assert.AreEqual("spam", table.Get(key1));
            Assert.AreEqual("baz", table.Get(key2));
            Assert.AreEqual(2, CountKeys(table));
            table.Put(key3, "eggs");
            Assert.AreEqual("spam", table.Get(key1));
            Assert.AreEqual("baz", table.Get(key2));
            Assert.AreEqual("eggs", table.Get(key3));
            Assert.AreEqual(3, CountKeys(table));
            table.Put(key2, "mice");
            Assert.AreEqual("spam", table.Get(key1));
            Assert.AreEqual("mice", table.Get(key2));
            Assert.AreEqual("eggs", table.Get(key3));
            Assert.AreEqual(3, CountKeys(table));
        }

        private int CountKeys(Hashtable4 table)
        {
            var count = 0;
            var i = table.Iterator();
            while (i.MoveNext())
            {
                count++;
            }
            return count;
        }

        public virtual void AssertIsIteratable(object[] keys)
        {
            var table = TableFromKeys(keys);
            AssertIterator(table, keys);
        }

        private void AssertIterator(Hashtable4 table, object[] keys)
        {
            var iter = table.Iterator();
            var expected = new Collection4(keys);
            while (iter.MoveNext())
            {
                var entry = (IEntry4) iter.Current;
                var removedOK = expected.Remove(entry.Key());
                Assert.IsTrue(removedOK);
            }
            Assert.IsTrue(expected.IsEmpty(), expected.ToString());
        }

        private Hashtable4 TableFromKeys(object[] keys)
        {
            var ht = new Hashtable4();
            for (var i = 0; i < keys.Length; i++)
            {
                ht.Put(keys[i], keys[i]);
            }
            return ht;
        }

        internal class KeyCount
        {
            public int keys;
        }

        internal class Key
        {
            private readonly int _hashCode;

            public Key(int hashCode)
            {
                _hashCode = hashCode;
            }

            public override int GetHashCode()
            {
                return _hashCode;
            }
        }
    }
}