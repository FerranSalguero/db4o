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
using Db4objects.Db4o.TA;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.TA.Mixed
{
    public class MixedActivateTestCase : ItemTestCaseBase
    {
        private readonly int ItemDepth = 10;

        public static void Main(string[] args)
        {
            new MixedActivateTestCase().RunAll();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void AssertItemValue(object obj)
        {
            AssertActivatedItemByMethod((Item) obj, ItemDepth);
        }

        internal virtual void AssertActivatedItemByMethod(Item item
            , int level)
        {
            for (var i = 0; i < ItemDepth; i++)
            {
                Assert.AreEqual("Item " + (ItemDepth - i), item.GetName());
                Assert.AreEqual(ItemDepth - i, item.GetValue());
                if (i < ItemDepth - 1)
                {
                    Assert.IsNotNull(item.Next());
                }
                else
                {
                    Assert.IsNull(item.Next());
                }
                item = item.Next();
            }
        }

        /// <exception cref="System.Exception"></exception>
        protected override void AssertRetrievedItem(object obj)
        {
            var item = (Item) obj;
            for (var i = 0; i < ItemDepth; i++)
            {
                AssertNullItem(item, ItemDepth - i);
                item = item.Next();
            }
        }

        private void AssertNullItem(Item item, int level)
        {
            if (level%2 == 0)
            {
                Assert.IsNull(item._name);
                Assert.IsNull(item._next);
                Assert.AreEqual(0, item._value);
            }
            else
            {
                Assert.AreEqual("Item " + level, item._name);
                Assert.AreEqual(level, item._value);
                if (level == 1)
                {
                    Assert.IsNull(item._next);
                }
                else
                {
                    Assert.IsNotNull(item._next);
                }
            }
        }

        /// <exception cref="System.Exception"></exception>
        protected override object CreateItem()
        {
            var item = TAItem.NewTAITem(10);
            item._isRoot = true;
            return item;
        }

        public virtual void TestActivate()
        {
            var item = (Item) RetrieveOnlyInstance
                (typeof (TAItem));
            Assert.IsNull(item._name);
            Assert.IsNull(item._next);
            Assert.AreEqual(0, item._value);
            // depth = 0;
            Db().Activate(item, 0);
            Assert.IsNull(item._name);
            Assert.IsNull(item._next);
            Assert.AreEqual(0, item._value);
            // depth = 1;
            // item.next();
            Db().Activate(item, 1);
            AssertActivatedItemByField(item, 1);
            Db().Activate(item, 5);
            AssertActivatedItemByField(item, 5);
            Db().Activate(item, 10);
            AssertActivatedItemByField(item, 10);
        }

        internal virtual void AssertActivatedItemByField(Item item,
            int level)
        {
            for (var i = 0; i < level; i++)
            {
                Assert.AreEqual("Item " + (ItemDepth - i), item._name);
                Assert.AreEqual(ItemDepth - i, item._value);
                if (i < ItemDepth - 1)
                {
                    Assert.IsNotNull(item._next);
                }
                else
                {
                    Assert.IsNull(item._next);
                }
                item = item._next;
            }
            if (level < ItemDepth)
            {
                Assert.IsNull(item._name);
                Assert.IsNull(item._next);
                Assert.AreEqual(0, item._value);
            }
        }

        public override object RetrieveOnlyInstance(Type clazz)
        {
            var q = Db().Query();
            q.Constrain(clazz);
            q.Descend("_isRoot").Constrain(true);
            return q.Execute().Next();
        }

        public class Item
        {
            public bool _isRoot;
            public string _name;
            public Item _next;
            public int _value;

            public Item()
            {
            }

            public Item(string name, int value)
            {
                //
                _name = name;
                _value = value;
            }

            public static Item NewItem(int depth)
            {
                if (depth == 0)
                {
                    return null;
                }
                var header = new Item("Item " + depth
                    , depth);
                header._next = TAItem.NewTAITem(depth - 1);
                return header;
            }

            public virtual string GetName()
            {
                return _name;
            }

            public virtual int GetValue()
            {
                return _value;
            }

            public virtual Item Next()
            {
                return _next;
            }
        }

        public class TAItem : Item, IActivatable
        {
            [NonSerialized] private IActivator _activator;

            public TAItem(string name, int value) : base(name, value)
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
                var header = new TAItem("Item " +
                                        depth, depth);
                header._next = NewItem(depth - 1);
                return header;
            }

            public override string GetName()
            {
                Activate(ActivationPurpose.Read);
                return _name;
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