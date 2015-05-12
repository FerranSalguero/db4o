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
using Db4objects.Db4o.Query;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Querying
{
    public class IndexedQueriesTestCase : AbstractDb4oTestCase
    {
        public static void Main(string[] arguments)
        {
            new IndexedQueriesTestCase().RunSolo();
        }

        protected override void Configure(IConfiguration config)
        {
            IndexField(config, "_name");
            IndexField(config, "_int");
            IndexField(config, "_integer");
        }

        private void IndexField(IConfiguration config, string fieldName)
        {
            IndexField(config, typeof (IndexedQueriesItem), fieldName);
        }

        protected override void Store()
        {
            string[] strings = {"a", "c", "b", "f", "e"};
            for (var i = 0; i < strings.Length; i++)
            {
                Db().Store(new IndexedQueriesItem(strings[i]));
            }
            int[] ints = {1, 5, 7, 3, 2, 3};
            for (var i = 0; i < ints.Length; i++)
            {
                Db().Store(new IndexedQueriesItem(ints[i]));
            }
        }

        public virtual void TestIntQuery()
        {
            AssertInts(5);
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestStringQuery()
        {
            AssertNullNameCount(6);
            Db().Store(new IndexedQueriesItem("d"));
            AssertQuery(1, "b");
            UpdateB();
            Db().Store(new IndexedQueriesItem("z"));
            Db().Store(new IndexedQueriesItem("y"));
            Reopen();
            AssertQuery(1, "b");
            AssertInts(8);
        }

        private void AssertIntegers()
        {
            var q = NewQuery();
            q.Descend("_integer").Constrain(4).Greater().Equal();
            AssertIntsFound(new[] {5, 7}, q);
            q = NewQuery();
            q.Descend("_integer").Constrain(4).Smaller();
            AssertIntsFound(new[] {1, 2, 3, 3}, q);
        }

        private void AssertInts(int expectedZeroSize)
        {
            var q = NewQuery();
            q.Descend("_int").Constrain(0);
            var zeroSize = q.Execute().Count;
            Assert.AreEqual(expectedZeroSize, zeroSize);
            q = NewQuery();
            q.Descend("_int").Constrain(4).Greater().Equal();
            AssertIntsFound(new[] {5, 7}, q);
            q = NewQuery();
            q.Descend("_int").Constrain(4).Greater();
            AssertIntsFound(new[] {5, 7}, q);
            q = NewQuery();
            q.Descend("_int").Constrain(3).Greater();
            AssertIntsFound(new[] {5, 7}, q);
            q = NewQuery();
            q.Descend("_int").Constrain(3).Greater().Equal();
            AssertIntsFound(new[] {3, 3, 5, 7}, q);
            q = NewQuery();
            q.Descend("_int").Constrain(2).Greater().Equal();
            AssertIntsFound(new[] {2, 3, 3, 5, 7}, q);
            q = NewQuery();
            q.Descend("_int").Constrain(2).Greater();
            AssertIntsFound(new[] {3, 3, 5, 7}, q);
            q = NewQuery();
            q.Descend("_int").Constrain(1).Greater().Equal();
            AssertIntsFound(new[] {1, 2, 3, 3, 5, 7}, q);
            q = NewQuery();
            q.Descend("_int").Constrain(1).Greater();
            AssertIntsFound(new[] {2, 3, 3, 5, 7}, q);
            q = NewQuery();
            q.Descend("_int").Constrain(4).Smaller();
            AssertIntsFound(new[] {1, 2, 3, 3}, expectedZeroSize, q);
            q = NewQuery();
            q.Descend("_int").Constrain(4).Smaller().Equal();
            AssertIntsFound(new[] {1, 2, 3, 3}, expectedZeroSize, q);
            q = NewQuery();
            q.Descend("_int").Constrain(3).Smaller();
            AssertIntsFound(new[] {1, 2}, expectedZeroSize, q);
            q = NewQuery();
            q.Descend("_int").Constrain(3).Smaller().Equal();
            AssertIntsFound(new[] {1, 2, 3, 3}, expectedZeroSize, q);
            q = NewQuery();
            q.Descend("_int").Constrain(2).Smaller().Equal();
            AssertIntsFound(new[] {1, 2}, expectedZeroSize, q);
            q = NewQuery();
            q.Descend("_int").Constrain(2).Smaller();
            AssertIntsFound(new[] {1}, expectedZeroSize, q);
            q = NewQuery();
            q.Descend("_int").Constrain(1).Smaller().Equal();
            AssertIntsFound(new[] {1}, expectedZeroSize, q);
            q = NewQuery();
            q.Descend("_int").Constrain(1).Smaller();
            AssertIntsFound(new int[] {}, expectedZeroSize, q);
        }

        private void AssertIntsFound(int[] ints, int zeroSize, IQuery q)
        {
            var res = q.Execute();
            Assert.AreEqual((ints.Length + zeroSize), res.Count);
            while (res.HasNext())
            {
                var ci = (IndexedQueriesItem
                    ) res.Next();
                for (var i = 0; i < ints.Length; i++)
                {
                    if (ints[i] == ci._int)
                    {
                        ints[i] = 0;
                        break;
                    }
                }
            }
            for (var i = 0; i < ints.Length; i++)
            {
                Assert.AreEqual(0, ints[i]);
            }
        }

        private void AssertIntsFound(int[] ints, IQuery q)
        {
            AssertIntsFound(ints, 0, q);
        }

        private void AssertQuery(int count, string @string)
        {
            var res = QueryForName(@string);
            Assert.AreEqual(count, res.Count);
            var item = (IndexedQueriesItem
                ) res.Next();
            Assert.AreEqual("b", item._name);
        }

        private void AssertNullNameCount(int count)
        {
            var res = QueryForName(null);
            Assert.AreEqual(count, res.Count);
            while (res.HasNext())
            {
                var ci = (IndexedQueriesItem
                    ) res.Next();
                Assert.IsNull(ci._name);
            }
        }

        private void UpdateB()
        {
            var res = QueryForName("b");
            var ci = (IndexedQueriesItem
                ) res.Next();
            ci._name = "j";
            Db().Store(ci);
            res = QueryForName("b");
            Assert.AreEqual(0, res.Count);
            res = QueryForName("j");
            Assert.AreEqual(1, res.Count);
            ci._name = "b";
            Db().Store(ci);
            AssertQuery(1, "b");
        }

        private IObjectSet QueryForName(string n)
        {
            var q = NewQuery();
            q.Descend("_name").Constrain(n);
            return q.Execute();
        }

        protected override IQuery NewQuery()
        {
            var q = base.NewQuery();
            q.Constrain(typeof (IndexedQueriesItem));
            return q;
        }

        public class IndexedQueriesItem
        {
            public int _int;
            public int _integer;
            public string _name;

            public IndexedQueriesItem()
            {
            }

            public IndexedQueriesItem(string name)
            {
                _name = name;
            }

            public IndexedQueriesItem(int int_)
            {
                _int = int_;
                _integer = int_;
            }
        }
    }
}