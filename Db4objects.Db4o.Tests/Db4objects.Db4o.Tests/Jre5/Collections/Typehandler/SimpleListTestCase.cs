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
using System.Collections;
using Db4objects.Db4o.Config;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Jre5.Collections.Typehandler
{
    /// <exclude></exclude>
    public class SimpleListTestCase : AbstractDb4oTestCase
    {
        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            config.ObjectClass(typeof (Item)).CascadeOnDelete(true);
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            var item = new Item();
            item.list = new ArrayList();
            item.list.Add("zero");
            item.list.Add(new ReferenceTypeElement("one"));
            Store(item);
        }

        public virtual void TestRetrieveInstance()
        {
            var item = (Item) RetrieveOnlyInstance(typeof (
                Item));
            Assert.AreEqual(2, item.list.Count);
            Assert.AreEqual("zero", item.list[0]);
        }

        public virtual void TestCascadingActivation()
        {
            var item = (Item) RetrieveOnlyInstance(typeof (
                Item));
            var list = item.list;
            Assert.AreEqual(2, list.Count);
            var element = list[1];
            if (Db().IsActive(element))
            {
                Db().Deactivate(item, int.MaxValue);
                Assert.IsFalse(Db().IsActive(element));
                Db().Activate(item, int.MaxValue);
                Assert.IsTrue(Db().IsActive(element));
            }
        }

        public virtual void TestQuery()
        {
            var q = Db().Query();
            q.Constrain(typeof (Item));
            q.Descend("list").Constrain("zero");
            var objectSet = q.Execute();
            Assert.AreEqual(1, objectSet.Count);
            var item = (Item) objectSet.Next();
            Assert.AreEqual("zero", item.list[0]);
        }

        public virtual void TestDeletion()
        {
            AssertObjectCount(typeof (ReferenceTypeElement), 1);
            var item = (Item) RetrieveOnlyInstance(typeof (
                Item));
            Db().Delete(item);
            AssertObjectCount(typeof (ReferenceTypeElement), 0);
        }

        private void AssertObjectCount(Type clazz, int count)
        {
            Assert.AreEqual(count, Db().Query(clazz).Count);
        }

        public class Item
        {
            public IList list;
        }

        public class ReferenceTypeElement
        {
            public string name;

            public ReferenceTypeElement(string name_)
            {
                name = name_;
            }
        }
    }
}