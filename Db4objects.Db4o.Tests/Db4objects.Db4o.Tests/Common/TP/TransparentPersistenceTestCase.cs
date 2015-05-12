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
using Db4objects.Db4o.Events;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.TA;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.TP
{
    public class TransparentPersistenceTestCase : AbstractDb4oTestCase
    {
        public static void Main(string[] args)
        {
            new TransparentPersistenceTestCase().RunAll();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            config.Add(new TransparentPersistenceSupport());
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            Store(new Item("Foo"));
            Store(new Item("Bar"));
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestActivateOnWrite()
        {
            var foo = ItemByName("Foo");
            foo.SetName("Foo*");
            Assert.AreEqual("Foo*", foo.GetName());
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestConcurrentClientModification()
        {
            if (!IsMultiSession())
            {
                return;
            }
            var client1 = Db();
            var client2 = OpenNewSession();
            try
            {
                var foo1 = ItemByName(client1, "Foo");
                var foo2 = ItemByName(client2, "Foo");
                foo1.SetName("Foo*");
                foo2.SetName("Foo**");
                AssertUpdatedObjects(client1, foo1);
                AssertUpdatedObjects(client2, foo2);
                client1.Refresh(foo1, 1);
                Assert.AreEqual(foo2.GetName(), foo1.GetName());
            }
            finally
            {
                client2.Close();
            }
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestObjectGoneAfterUpdateAndDeletion()
        {
            var foo = ItemByName("Foo");
            foo.SetName("Foo*");
            Db().Delete(foo);
            Reopen();
            Assert.IsNull(ItemByName("Foo"));
            Assert.IsNull(ItemByName("Foo*"));
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestTransparentUpdate()
        {
            var foo = ItemByName("Foo");
            var bar = ItemByName("Bar");
            Assert.AreEqual("Bar", bar.GetName());
            // accessed but not changed
            foo.SetName("Bar");
            // changing more than once shouldn't be a problem
            foo.SetName("Foo*");
            AssertUpdatedObjects(foo);
            Reopen();
            Assert.IsNotNull(ItemByName("Foo*"));
            Assert.IsNull(ItemByName("Foo"));
            Assert.IsNotNull(ItemByName("Bar"));
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestChangedAfterCommit()
        {
            var item = ItemByName("Foo");
            item.SetName("Bar");
            AssertUpdatedObjects(item);
            item.SetName("Foo");
            AssertUpdatedObjects(item);
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestUpdateAfterActivation()
        {
            var foo = ItemByName("Foo");
            Assert.AreEqual("Foo", foo.GetName());
            foo.SetName("Foo*");
            AssertUpdatedObjects(foo);
        }

        private void AssertUpdatedObjects(Item expected)
        {
            AssertUpdatedObjects(Db(), expected);
        }

        private void AssertUpdatedObjects(IExtObjectContainer container, Item expected)
        {
            var updated = CommitCapturingUpdatedObjects(container);
            Assert.AreEqual(1, updated.Size(), updated.ToString());
            Assert.AreSame(expected, updated.SingleElement());
        }

        private Collection4 CommitCapturingUpdatedObjects(IExtObjectContainer container)
        {
            var updated = new Collection4();
            EventRegistryFor(container).Updated += new _IEventListener4_117(updated).OnEvent;
            container.Commit();
            return updated;
        }

        private Item ItemByName(string name)
        {
            return ItemByName(Db(), name);
        }

        private Item ItemByName(IExtObjectContainer container, string name)
        {
            var q = NewQuery(container, typeof (Item));
            q.Descend("name").Constrain(name);
            var result = q.Execute();
            if (result.HasNext())
            {
                return (Item) result.Next();
            }
            return null;
        }

        private sealed class _IEventListener4_117
        {
            private readonly Collection4 updated;

            public _IEventListener4_117(Collection4 updated)
            {
                this.updated = updated;
            }

            public void OnEvent(object sender, ObjectInfoEventArgs args
                )
            {
                ObjectEventArgs objectArgs = args;
                updated.Add(objectArgs.Object);
            }
        }
    }
}