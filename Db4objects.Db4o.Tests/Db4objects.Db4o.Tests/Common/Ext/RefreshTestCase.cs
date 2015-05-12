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
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Ext
{
    public class RefreshTestCase : Db4oClientServerTestCase
    {
        public static void Main(string[] args)
        {
            new RefreshTestCase().RunAll();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            config.ObjectClass(typeof (Item)).CascadeOnUpdate(true);
        }

        protected override void Store()
        {
            var r3 = new Item("o3", null);
            var r2 = new Item("o2", r3);
            var r1 = new Item("o1", r2);
            Store(r1);
        }

        public virtual void Test()
        {
            var oc1 = OpenNewSession();
            var oc2 = OpenNewSession();
            try
            {
                var r1 = GetRoot(oc1);
                r1.name = "cc";
                oc1.Refresh(r1, 0);
                Assert.AreEqual("cc", r1.name);
                oc1.Refresh(r1, 1);
                Assert.AreEqual("o1", r1.name);
                r1.child.name = "cc";
                oc1.Refresh(r1, 1);
                Assert.AreEqual("cc", r1.child.name);
                oc1.Refresh(r1, 2);
                Assert.AreEqual("o2", r1.child.name);
                var r2 = GetRoot(oc2);
                r2.name = "o21";
                r2.child.name = "o22";
                r2.child.child.name = "o23";
                oc2.Store(r2);
                oc2.Commit();
                oc1.Refresh(r1, 3);
                Assert.AreEqual("o21", r1.name);
                Assert.AreEqual("o22", r1.child.name);
                Assert.AreEqual("o23", r1.child.child.name);
            }
            finally
            {
                oc1.Close();
                oc2.Close();
            }
        }

        private Item GetRoot(IObjectContainer oc)
        {
            return GetByName(oc, "o1");
        }

        private Item GetByName(IObjectContainer oc, string name)
        {
            var q = oc.Query();
            q.Constrain(typeof (Item));
            q.Descend("name").Constrain(name);
            var objectSet = q.Execute();
            return (Item) objectSet.Next();
        }

        public class Item
        {
            public Item child;
            public string name;

            public Item(string name, Item child)
            {
                this.name = name;
                this.child = child;
            }
        }
    }
}