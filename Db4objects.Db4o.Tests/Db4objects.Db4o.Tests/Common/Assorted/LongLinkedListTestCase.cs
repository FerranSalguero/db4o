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

namespace Db4objects.Db4o.Tests.Common.Assorted
{
    public class LongLinkedListTestCase : AbstractDb4oTestCase
    {
        private const int Count = 1000;

        private static LinkedList NewLongCircularList()
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

        /// <exception cref="System.Exception"></exception>
        public static void Main(string[] args)
        {
            new LongLinkedListTestCase().RunSolo();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            Store(NewLongCircularList());
        }

        public virtual void Test()
        {
            var q = NewQuery(typeof (LinkedList));
            q.Descend("_depth").Constrain(0);
            var objectSet = q.Execute();
            Assert.AreEqual(1, objectSet.Count);
            var head = (LinkedList) objectSet
                .Next();
            Db().Activate(head, int.MaxValue);
            AssertListIsComplete(head);
            Db().Deactivate(head, int.MaxValue);
            Db().Activate(head, int.MaxValue);
            AssertListIsComplete(head);
            Db().Deactivate(head, int.MaxValue);
            Db().Refresh(head, int.MaxValue);
            AssertListIsComplete(head);
        }

        // TODO: The following produces a stack overflow. That's OK for now, peekPersisted is rarely
        //		 used and users can control behaviour with the depth parameter. 
        // 		 
        //		LinkedList peeked = (LinkedList) db().ext().peekPersisted(head, Integer.MAX_VALUE, true);
        //		assertListIsComplete(peeked);
        private void AssertListIsComplete(LinkedList head)
        {
            var count = 1;
            var tail = head._next;
            while (tail != head)
            {
                count++;
                tail = tail._next;
            }
            Assert.AreEqual(Count, count);
        }

        public class LinkedList
        {
            public int _depth;
            public LinkedList _next;
        }
    }
}