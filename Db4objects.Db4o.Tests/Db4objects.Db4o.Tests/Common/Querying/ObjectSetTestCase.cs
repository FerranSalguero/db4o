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
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Internal;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Querying
{
    /// <exclude></exclude>
    public class ObjectSetTestCase : AbstractDb4oTestCase
    {
        public static void Main(string[] args)
        {
            new ObjectSetTestCase().RunNetworking();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            config.Queries().EvaluationMode(QueryEvaluationMode.Lazy);
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            Db().Store(new Item("foo"));
            Db().Store(new Item("bar"));
            Db().Store(new Item("baz"));
        }

        public virtual void _testObjectsCantBeSeenAfterDelete()
        {
            var trans1 = NewTransaction();
            var trans2 = NewTransaction();
            DeleteItemAndCommit(trans2, "foo");
            var os = QueryItems(trans1);
            AssertItems(new[] {"bar", "baz"}, os);
        }

        public virtual void _testAccessOrder()
        {
            var result = NewQuery(typeof (Item)).Execute();
            for (var i = 0; i < result.Count; ++i)
            {
                Assert.IsTrue(result.HasNext());
                Assert.AreSame(result.Ext()[i], result.Next());
            }
            Assert.IsFalse(result.HasNext());
        }

        public virtual void TestInvalidNext()
        {
            var query = NewQuery(typeof (Item));
            query.Descend("name").Constrain("foo");
            var result = query.Execute();
            Assert.Expect(typeof (InvalidOperationException), new _ICodeBlock_69(result));
        }

        private void AssertItems(string[] expectedNames, IObjectSet actual)
        {
            for (var i = 0; i < expectedNames.Length; i++)
            {
                Assert.IsTrue(actual.HasNext());
                Assert.AreEqual(expectedNames[i], ((Item) actual.Next()).name);
            }
            Assert.IsFalse(actual.HasNext());
        }

        private void DeleteItemAndCommit(Transaction trans, string name)
        {
            Container().Delete(trans, QueryItem(trans, name));
            trans.Commit();
        }

        private Item QueryItem(Transaction trans, string name)
        {
            var q = NewQuery(trans, typeof (Item));
            q.Descend("name").Constrain(name);
            return (Item) q.Execute().Next();
        }

        private IObjectSet QueryItems(Transaction trans)
        {
            var q = NewQuery(trans, typeof (Item));
            q.Descend("name").OrderAscending();
            return q.Execute();
        }

        public class Item
        {
            public string name;

            public Item()
            {
            }

            public Item(string name_)
            {
                name = name_;
            }

            public override string ToString()
            {
                return "Item(\"" + name + "\")";
            }
        }

        private sealed class _ICodeBlock_69 : ICodeBlock
        {
            private readonly IObjectSet result;

            public _ICodeBlock_69(IObjectSet result)
            {
                this.result = result;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                while (true)
                {
                    result.HasNext();
                    result.Next();
                }
            }
        }
    }
}