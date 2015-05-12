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
using Db4objects.Db4o.Typehandlers;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Jre5.Collections.Typehandler
{
    /// <exclude></exclude>
    public class SimpleMapTestCase : AbstractDb4oTestCase
    {
        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            config.RegisterTypeHandler(new SingleClassTypeHandlerPredicate(typeof (Hashtable))
                , new MapTypeHandler());
            config.ObjectClass(typeof (Item)).CascadeOnDelete(true);
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            var item = new Item();
            item.map = new Hashtable();
            item.map["zero"] = "zero";
            item.map[new ReferenceTypeElement("one")] = "one";
            Store(item);
        }

        public virtual void TestRetrieveInstance()
        {
            var item = (Item) RetrieveOnlyInstance(typeof (
                Item));
            Assert.AreEqual("zero", item.map["zero"]);
        }

        public virtual void TestQuery()
        {
            var q = Db().Query();
            q.Constrain(typeof (Item));
            q.Descend("map").Constrain("zero");
            var objectSet = q.Execute();
            Assert.AreEqual(1, objectSet.Count);
            var item = (Item) objectSet.Next();
            Assert.AreEqual("zero", item.map["zero"]);
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
            public IDictionary map;
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