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

namespace Db4objects.Db4o.Tests.Common.Assorted
{
    public class PlainObjectTestCase : AbstractDb4oTestCase
    {
        public static void Main(string[] args)
        {
            new PlainObjectTestCase().RunAll();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            config.ObjectClass(typeof (Item)).CascadeOnDelete(true);
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            var plainObject = new object();
            var item = new Item("one", plainObject);
            Store(item);
            Assert.IsTrue(Db().IsStored(item._plainObject));
            Store(new Item("two", plainObject));
        }

        public virtual void TestRetrieve()
        {
            var itemOne = RetrieveItem("one");
            Assert.IsNotNull(itemOne._plainObject);
            Assert.IsTrue(Db().IsStored(itemOne._plainObject));
            var itemTwo = RetrieveItem("two");
            Assert.AreSame(itemOne._plainObject, itemTwo._plainObject);
        }

        public virtual void TestDelete()
        {
            var itemOne = RetrieveItem("one");
            Db().Delete(itemOne);
        }

        public virtual void _testEvaluationQuery()
        {
            // The evaluation doesn't work in C/S mode
            // because TransportObjectContainer#storeInteral  
            // never gets a chance to intercept.
            var itemOne = RetrieveItem("one");
            var plainObject = itemOne._plainObject;
            var q = NewQuery(typeof (Item));
            q.Constrain(new _IEvaluation_64(plainObject));
            var objectSet = q.Execute();
            Assert.AreEqual(2, objectSet.Count);
        }

        public virtual void TestIdentityQuery()
        {
            var itemOne = RetrieveItem("one");
            var plainObject = itemOne._plainObject;
            var q = NewQuery(typeof (Item));
            q.Descend("_plainObject").Constrain(plainObject).Identity();
            var objectSet = q.Execute();
            Assert.AreEqual(2, objectSet.Count);
        }

        private Item RetrieveItem(string name)
        {
            var query = NewQuery(typeof (Item));
            query.Descend("_name").Constrain(name);
            var objectSet = query.Execute();
            Assert.AreEqual(1, objectSet.Count);
            return (Item) objectSet.Next();
        }

        public class Item
        {
            public string _name;
            public object _plainObject;

            public Item(string name, object plainObject)
            {
                _name = name;
                _plainObject = plainObject;
            }
        }

        private sealed class _IEvaluation_64 : IEvaluation
        {
            private readonly object plainObject;

            public _IEvaluation_64(object plainObject)
            {
                this.plainObject = plainObject;
            }

            public void Evaluate(ICandidate candidate)
            {
                var item = (Item) candidate.GetObject();
                candidate.Include(item._plainObject == plainObject);
            }
        }
    }
}