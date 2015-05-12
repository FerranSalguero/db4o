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
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.TA.TA
{
    public class TAActivateTestCase : TAItemTestCaseBase
    {
        private readonly int ItemDepth = 10;

        public static void Main(string[] args)
        {
            new TAActivateTestCase().RunAll();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void AssertItemValue(object obj)
        {
            var taItem = (TAItem) obj;
            for (var i = 0; i < ItemDepth - 1; i++)
            {
                Assert.AreEqual("TAItem " + (ItemDepth - i), taItem.GetName());
                Assert.AreEqual(ItemDepth - i, taItem.GetValue());
                Assert.IsNotNull(taItem.Next());
                taItem = taItem.Next();
            }
            Assert.AreEqual("TAItem 1", taItem.GetName());
            Assert.AreEqual(1, taItem.GetValue());
            Assert.IsNull(taItem.Next());
        }

        /// <exception cref="System.Exception"></exception>
        protected override void AssertRetrievedItem(object obj)
        {
            var taItem = (TAItem) obj;
            AssertNullItem(taItem);
            // depth = 0, no effect
            Db().Activate(taItem, 0);
            AssertNullItem(taItem);
            // depth = 1
            Db().Activate(taItem, 1);
            AssertActivatedItem(taItem, 0, 1);
            // depth = 5
            Db().Activate(taItem, 5);
            AssertActivatedItem(taItem, 0, 5);
            Db().Activate(taItem, ItemDepth + 100);
            AssertActivatedItem(taItem, 0, ItemDepth);
        }

        private void AssertActivatedItem(TAItem item, int from, int depth
            )
        {
            if (depth > ItemDepth)
            {
                throw new ArgumentException("depth should not be greater than ITEM_DEPTH.");
            }
            var next = item;
            for (var i = from; i < depth; i++)
            {
                Assert.AreEqual("TAItem " + (ItemDepth - i), next._name);
                Assert.AreEqual(ItemDepth - i, next._value);
                if (i < ItemDepth - 1)
                {
                    Assert.IsNotNull(next._next);
                }
                next = next._next;
            }
            if (depth < ItemDepth)
            {
                AssertNullItem(next);
            }
        }

        private void AssertNullItem(TAItem taItem)
        {
            Assert.IsNull(taItem._name);
            Assert.IsNull(taItem._next);
            Assert.AreEqual(0, taItem._value);
        }

        public override object RetrieveOnlyInstance(Type clazz)
        {
            var q = Db().Query();
            q.Constrain(clazz);
            q.Descend("_isRoot").Constrain(true);
            return q.Execute().Next();
        }

        /// <exception cref="System.Exception"></exception>
        protected override object CreateItem()
        {
            var taItem = TAItem.NewTAItem(ItemDepth);
            taItem._isRoot = true;
            return taItem;
        }

        public class TAItem : ActivatableImpl
        {
            public bool _isRoot;
            public string _name;
            public TAItem _next;
            public int _value;

            public static TAItem NewTAItem(int depth)
            {
                if (depth == 0)
                {
                    return null;
                }
                var root = new TAItem();
                root._name = "TAItem " + depth;
                root._value = depth;
                root._next = NewTAItem(depth - 1);
                return root;
            }

            public virtual string GetName()
            {
                Activate(ActivationPurpose.Read);
                return _name;
            }

            public virtual int GetValue()
            {
                Activate(ActivationPurpose.Read);
                return _value;
            }

            public virtual TAItem Next()
            {
                Activate(ActivationPurpose.Read);
                return _next;
            }
        }
    }
}