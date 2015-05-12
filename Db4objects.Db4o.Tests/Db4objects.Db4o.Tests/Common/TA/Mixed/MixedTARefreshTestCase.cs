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
using Db4objects.Db4o.Activation;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.TA;
using Db4oUnit;
using Db4oUnit.Extensions.Fixtures;

namespace Db4objects.Db4o.Tests.Common.TA.Mixed
{
    public class MixedTARefreshTestCase : TransparentActivationTestCaseBase, IOptOutSolo
    {
        private const int ItemDepth = 10;

        public static void Main(string[] args)
        {
            new MixedTARefreshTestCase().RunNetworking();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            var item = Item.NewItem(ItemDepth
                );
            item._isRoot = true;
            Store(item);
        }

        public virtual void TestRefresh()
        {
            var client1 = OpenNewSession();
            var client2 = OpenNewSession();
            var item1 = RetrieveInstance(client1);
            var item2 = RetrieveInstance(client2);
            var next1 = item1;
            var value = 10;
            while (next1 != null)
            {
                Assert.AreEqual(value, next1.GetValue());
                next1 = next1.Next();
                value--;
            }
            var next2 = item2;
            value = 10;
            while (next2 != null)
            {
                Assert.AreEqual(value, next2.GetValue());
                next2 = next2.Next();
                value--;
            }
            item1.SetValue(100);
            item1.Next().SetValue(200);
            client1.Store(item1, 2);
            client1.Commit();
            Assert.AreEqual(100, item1.GetValue());
            Assert.AreEqual(200, item1.Next().GetValue());
            Assert.AreEqual(10, item2.GetValue());
            Assert.AreEqual(9, item2.Next().GetValue());
            //refresh 0
            client2.Refresh(item2, 0);
            Assert.AreEqual(10, item2.GetValue());
            Assert.AreEqual(9, item2.Next().GetValue());
            //refresh 1
            client2.Refresh(item2, 1);
            Assert.AreEqual(100, item2.GetValue());
            Assert.AreEqual(9, item2.Next().GetValue());
            //refresh 2
            client2.Refresh(item2, 2);
            Assert.AreEqual(100, item2.GetValue());
            //FIXME: maybe a bug
            //Assert.areEqual(200, item2.next().getValue());
            next1 = item1;
            value = 1000;
            while (next1 != null)
            {
                next1.SetValue(value);
                next1 = next1.Next();
                value++;
            }
            client1.Store(item1, 5);
            client1.Commit();
            client2.Refresh(item2, 5);
            next2 = item2;
            for (var i = 1000; i < 1005; i++)
            {
                Assert.AreEqual(i, next2.GetValue());
                next2 = next2.Next();
            }
            client1.Close();
            client2.Close();
        }

        private Item RetrieveInstance(IExtObjectContainer client)
        {
            var query = client.Query();
            query.Constrain(typeof (Item));
            query.Descend("_isRoot").Constrain(true);
            return (Item) query.Execute().Next();
        }

        public class Item
        {
            public bool _isRoot;
            public Item _next;
            public int _value;

            public Item()
            {
            }

            public Item(int value)
            {
                //
                _value = value;
            }

            public static Item NewItem(int depth)
            {
                if (depth == 0)
                {
                    return null;
                }
                var header = new Item(depth);
                header._next = TAItem.NewTAITem(depth - 1);
                return header;
            }

            public virtual int GetValue()
            {
                return _value;
            }

            public virtual void SetValue(int value)
            {
                _value = value;
            }

            public virtual Item Next()
            {
                return _next;
            }
        }

        public class TAItem : Item, IActivatable
        {
            [NonSerialized] private IActivator _activator;

            public TAItem(int value) : base(value)
            {
            }

            public virtual void Activate(ActivationPurpose purpose)
            {
                if (_activator == null)
                {
                    return;
                }
                _activator.Activate(purpose);
            }

            public virtual void Bind(IActivator activator)
            {
                _activator = activator;
            }

            public static TAItem NewTAITem(int depth)
            {
                if (depth == 0)
                {
                    return null;
                }
                var header = new TAItem(depth);
                header._next = NewItem(depth - 1);
                return header;
            }

            public override int GetValue()
            {
                Activate(ActivationPurpose.Read);
                return _value;
            }

            public override Item Next()
            {
                Activate(ActivationPurpose.Read);
                return _next;
            }
        }
    }
}