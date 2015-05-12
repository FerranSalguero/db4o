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
using Db4objects.Db4o.Diagnostic;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Querying
{
    public class NullConstraintQueryTestCase : AbstractDb4oTestCase
    {
        public static void Main(string[] args)
        {
            new NullConstraintQueryTestCase().RunAll();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            config.Diagnostic().AddListener(new LoadedFromClassIndexListener
                (this));
            config.ObjectClass(typeof (ObjectItem)).ObjectField("_child"
                ).Indexed(true);
            config.ObjectClass(typeof (StringItem)).ObjectField("_name"
                ).Indexed(true);
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            var childItem = new ObjectItem
                (null, "child");
            var parentItem = new ObjectItem
                (childItem, "parent");
            Store(parentItem);
            Store(new StringItem(null));
            Store(new StringItem(null));
            Store(new StringItem("one"));
            Store(new StringItem("two"));
        }

        public virtual void TestQueryForNullChild()
        {
            var q = NewQuery(typeof (ObjectItem));
            q.Descend("_child").Constrain(null);
            var objectSet = q.Execute();
            Assert.AreEqual(1, objectSet.Count);
            var item = ((ObjectItem
                ) objectSet.Next());
            Assert.AreEqual("child", item._name);
        }

        public virtual void TestQueryForNullString()
        {
            var q = NewQuery(typeof (StringItem));
            q.Descend("_name").Constrain(null);
            var objectSet = q.Execute();
            Assert.AreEqual(2, objectSet.Count);
            var item = ((StringItem
                ) objectSet.Next());
            Assert.IsNull(item._name);
            item = ((StringItem) objectSet.Next());
            Assert.IsNull(item._name);
        }

        private sealed class LoadedFromClassIndexListener : IDiagnosticListener
        {
            private readonly NullConstraintQueryTestCase _enclosing;

            internal LoadedFromClassIndexListener(NullConstraintQueryTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void OnDiagnostic(IDiagnostic d)
            {
                if (d is LoadedFromClassIndex)
                {
                    Assert.Fail("Query should not be loaded from class index");
                }
            }
        }

        public class ObjectItem
        {
            public ObjectItem _child;
            public string _name;

            public ObjectItem(ObjectItem child, string name)
            {
                _child = child;
                _name = name;
            }
        }

        public class StringItem
        {
            public string _name;

            public StringItem(string name)
            {
                _name = name;
            }
        }
    }
}