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

using System;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Querying
{
    public class QueryByExampleTestCase : AbstractDb4oTestCase
    {
        internal const int Count = 10;

        internal static LinkedList list = LinkedList
            .NewLongCircularList();

        public static void Main(string[] args)
        {
            new QueryByExampleTestCase().RunAll();
        }

        protected override void Store()
        {
            Store(list);
        }

        public virtual void TestDefaultQueryModeIsIdentity()
        {
            var itemOne = new Item("one");
            var itemTwo = new Item("two");
            Store(itemOne);
            Store(itemTwo);
            // Change the name of the "sample"
            itemOne._name = "two";
            // Query by Identity
            var q = Db().Query();
            q.Constrain(itemOne);
            var objectSet = q.Execute();
            AssertItem(objectSet, itemOne);
        }

        public virtual void TestConstrainByExample()
        {
            var itemOne = new Item("one");
            var itemTwo = new Item("two");
            Store(itemOne);
            Store(itemTwo);
            // Change the name of the "sample"
            itemOne._name = "two";
            // Query by Example
            var q = Db().Query();
            q.Constrain(itemOne).ByExample();
            var objectSet = q.Execute();
            // Expect to get the other 
            AssertItem(objectSet, itemTwo);
        }

        private void AssertItem(IObjectSet objectSet, Item item)
        {
            Assert.AreEqual(1, objectSet.Count);
            var retrievedItem = (Item) objectSet
                .Next();
            Assert.AreSame(item, retrievedItem);
        }

        public virtual void TestQueryByExample()
        {
            var itemOne = new Item("one");
            var itemTwo = new Item("two");
            Store(itemOne);
            Store(itemTwo);
            // Change the name of the "sample"
            itemOne._name = "two";
            // Query by Example
            var objectSet = Db().QueryByExample(itemOne);
            AssertItem(objectSet, itemTwo);
        }

        public virtual void TestQueryByExampleNoneFound()
        {
            var itemOne = new Item("one");
            var itemTwo = new Item("two");
            Store(itemOne);
            Store(itemTwo);
            // Change the name of the "sample"
            itemOne._name = "three";
            var objectSet = Db().QueryByExample(itemOne);
            Assert.AreEqual(0, objectSet.Count);
        }

        public virtual void TestByExample()
        {
            var q = Db().Query();
            q.Constrain(list).ByExample();
            var result = q.Execute();
            Assert.AreEqual(Count, result.Count);
        }

        public virtual void TestByIdentity()
        {
            var q = Db().Query();
            q.Constrain(typeof (LinkedList));
            var result = q.Execute();
            Assert.AreEqual(Count, result.Count);
            while (result.HasNext())
            {
                Db().Delete(result.Next());
            }
            q = Db().Query();
            q.Constrain(typeof (LinkedList));
            result = q.Execute();
            Assert.AreEqual(0, result.Count);
            var newList = LinkedList.NewLongCircularList
                ();
            Db().Store(newList);
            q = Db().Query();
            q.Constrain(newList);
            result = q.Execute();
            Assert.AreEqual(1, result.Count);
        }

        public virtual void TestClassConstraint()
        {
            var q = Db().Query();
            q.Constrain(typeof (LinkedList));
            var result = q.Execute();
            Assert.AreEqual(Count, result.Count);
            q = Db().Query();
            q.Constrain(typeof (LinkedList)).ByExample();
            result = q.Execute();
            Assert.AreEqual(Count, result.Count);
        }

        public class Item
        {
            public string _name;

            public Item(string name)
            {
                _name = name;
            }
        }

        public class LinkedList
        {
            [NonSerialized] public int _depth;

            public LinkedList _next;

            public static LinkedList NewLongCircularList()
            {
                var head = new LinkedList();
                var tail = head;
                for (var i = 1; i < Count; i++)
                {
                    tail._next = new LinkedList();
                    tail = tail._next;
                    tail._depth = i;
                }
                tail._next = head;
                return head;
            }

            public override string ToString()
            {
                return "List[" + _depth + "]";
            }
        }
    }
}