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

using Db4objects.Db4o.Activation;
using Db4objects.Db4o.Ext;
using Db4oUnit;
using Db4oUnit.Extensions.Fixtures;

namespace Db4objects.Db4o.Tests.Common.TA.TA
{
    public class TARefreshTestCase : TransparentActivationTestCaseBase, IOptOutSolo
    {
        private const int ItemDepth = 10;

        public static void Main(string[] args)
        {
            new TARefreshTestCase().RunNetworking();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            var item = TAItem.NewGraph(ItemDepth);
            Store(item);
        }

        public virtual void TestRefresh()
        {
            var client1 = OpenNewSession();
            var client2 = OpenNewSession();
            var item1 = QueryRoot(client1);
            var item2 = QueryRoot(client2);
            var next1 = item1;
            var value = 10;
            while (next1 != null)
            {
                Assert.AreEqual(value, next1.Value());
                next1 = next1.Next();
                value--;
            }
            var next2 = item2;
            value = 10;
            while (next2 != null)
            {
                Assert.AreEqual(value, next2.Value());
                next2 = next2.Next();
                value--;
            }
            //update depth = 1
            item1.Value(100);
            item1.Next().Value(200);
            client1.Store(item1, 2);
            client1.Commit();
            AssertItemValue(100, item1);
            AssertItemValue(200, item1.Next());
            AssertItemValue(10, item2);
            AssertItemValue(9, item2.Next());
            //refresh 0
            client2.Refresh(item2, 0);
            AssertItemValue(10, item2);
            AssertItemValue(9, item2.Next());
            //refresh 1
            client2.Refresh(item2, 1);
            AssertItemValue(100, item2);
            AssertItemValue(9, item2.Next());
            //refresh 2
            client2.Refresh(item2, 2);
            AssertItemValue(100, item2);
            AssertItemValue(200, item2.Next());
            next1 = item1;
            value = 1000;
            while (next1 != null)
            {
                next1.Value(value);
                next1 = next1.Next();
                value++;
            }
            client1.Store(item1, 5);
            client1.Commit();
            client2.Refresh(item2, 5);
            next2 = item2;
            for (var i = 1000; i < 1005; i++)
            {
                AssertItemValue(i, next2);
                next2 = next2.Next();
            }
            client1.Close();
            client2.Close();
        }

        private void AssertItemValue(int expectedValue, TAItem item)
        {
            Assert.AreEqual(expectedValue, item.PassThroughValue());
            Assert.AreEqual(expectedValue, item.Value());
        }

        private TAItem QueryRoot(IExtObjectContainer client)
        {
            var query = client.Query();
            query.Constrain(typeof (TAItem));
            query.Descend("_isRoot").Constrain(true);
            return (TAItem) query.Execute().Next();
        }

        public class TAItem : ActivatableImpl
        {
            public bool _isRoot;
            public TAItem _next;
            public int _value;

            public static TAItem NewGraph(int depth)
            {
                var item = NewTAItem(depth);
                item._isRoot = true;
                return item;
            }

            private static TAItem NewTAItem(int depth)
            {
                if (depth == 0)
                {
                    return null;
                }
                var root = new TAItem();
                root._value = depth;
                root._next = NewTAItem(depth - 1);
                return root;
            }

            public virtual int PassThroughValue()
            {
                return _value;
            }

            public virtual int Value()
            {
                Activate(ActivationPurpose.Read);
                return _value;
            }

            public virtual void Value(int value)
            {
                _value = value;
            }

            public virtual TAItem Next()
            {
                Activate(ActivationPurpose.Read);
                return _next;
            }
        }
    }
}