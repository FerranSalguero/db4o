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

using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Store
{
    public class DeepSetClientServerTestCase : Db4oClientServerTestCase
    {
        public static void Main(string[] args)
        {
            new DeepSetClientServerTestCase().RunAll();
        }

        protected override void Store()
        {
            var item = new Item();
            item.name = "1";
            item.child = new Item();
            item.child.name = "2";
            item.child.child = new Item();
            item.child.child.name = "3";
            Store(item);
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void Test()
        {
            var oc1 = OpenNewSession();
            var oc2 = OpenNewSession();
            var oc3 = OpenNewSession();
            var example = new Item();
            example.name = "1";
            try
            {
                var item1 = (Item) oc1.QueryByExample
                    (example).Next();
                Assert.AreEqual("1", item1.name);
                Assert.AreEqual("2", item1.child.name);
                Assert.AreEqual("3", item1.child.child.name);
                var item2 = (Item) oc2.QueryByExample
                    (example).Next();
                Assert.AreEqual("1", item2.name);
                Assert.AreEqual("2", item2.child.name);
                Assert.AreEqual("3", item2.child.child.name);
                item1.child.name = "12";
                item1.child.child.name = "13";
                oc1.Store(item1, 2);
                oc1.Commit();
                // check result
                var item = (Item) oc1.QueryByExample
                    (example).Next();
                Assert.AreEqual("1", item.name);
                Assert.AreEqual("12", item.child.name);
                Assert.AreEqual("13", item.child.child.name);
                item = (Item) oc2.QueryByExample(example).Next();
                oc2.Refresh(item, 3);
                Assert.AreEqual("1", item.name);
                Assert.AreEqual("12", item.child.name);
                Assert.AreEqual("3", item.child.child.name);
                item = (Item) oc3.QueryByExample(example).Next();
                Assert.AreEqual("1", item.name);
                Assert.AreEqual("12", item.child.name);
                Assert.AreEqual("3", item.child.child.name);
            }
            finally
            {
                oc1.Close();
                oc2.Close();
                oc3.Close();
            }
        }

        public class Item
        {
            public Item child;
            public string name;
        }
    }
}