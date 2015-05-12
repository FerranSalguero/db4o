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

using Db4objects.Db4o.Events;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Events
{
    public class UpdateInCallbackThrowsTestCase : AbstractDb4oTestCase
    {
        public static void Main(string[] args)
        {
            new UpdateInCallbackThrowsTestCase().RunAll();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            Store(new Item("foo", new Item
                ("bar")));
        }

        public virtual void TestUpdatingInDeletingCallback()
        {
            var isNetworking = IsNetworking();
            EventRegistryFor(FileSession()).Deleting += new _IEventListener4_42(isNetworking).OnEvent;
            Db().Delete(ItemByName("foo"));
            Assert.IsNotNull(ItemByName("bar*"));
        }

        public virtual void TestReentrantUpdateAfterActivationThrows()
        {
            var foo = ItemByName("foo");
            Db().Deactivate(foo);
            EventRegistry().Activated += new _IEventListener4_71(this).OnEvent;
            Db().Activate(foo, 1);
        }

        private Item ItemByName(string name)
        {
            return ((Item) QueryItemsByName(name).Next());
        }

        public virtual void TestReentrantUpdateThrows()
        {
            var updatedTriggered = new ByRef();
            updatedTriggered.value = false;
            var registry = EventRegistryFactory.ForObjectContainer(Db());
            registry.Updated += new _IEventListener4_102(this, updatedTriggered).OnEvent;
            var items = QueryItemsByName("foo");
            Assert.AreEqual(1, items.Count);
            Assert.IsFalse((((bool) updatedTriggered.value)));
            Store(items.Next());
            Assert.IsTrue((((bool) updatedTriggered.value)));
        }

        private IObjectSet QueryItemsByName(string name)
        {
            var query = NewQuery(typeof (Item));
            query.Descend("_name").Constrain(name);
            return query.Execute();
        }

        public class Item
        {
            public Item _child;
            public string _name;

            public Item(string name) : this(name, null)
            {
            }

            public Item(string name, Item child)
            {
                _name = name;
                _child = child;
            }
        }

        private sealed class _IEventListener4_42
        {
            private readonly bool isNetworking;

            public _IEventListener4_42(bool isNetworking)
            {
                this.isNetworking = isNetworking;
            }

            public void OnEvent(object sender, CancellableObjectEventArgs
                args)
            {
                var obj = args.Object;
                if (!(obj is Item))
                {
                    return;
                }
                var transaction = (Transaction) args.Transaction
                    ();
                var container = transaction.ObjectContainer();
                var foo = (Item) obj;
                var child = foo._child;
                if (isNetworking)
                {
                    container.Activate(child, 1);
                }
                child._name += "*";
                container.Store(child);
            }
        }

        private sealed class _IEventListener4_71
        {
            private readonly UpdateInCallbackThrowsTestCase _enclosing;

            public _IEventListener4_71(UpdateInCallbackThrowsTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void OnEvent(object sender, ObjectInfoEventArgs args
                )
            {
                var obj = args.Object;
                if (!(obj is Item))
                {
                    return;
                }
                var item = (Item) obj;
                if (!item._name.Equals("foo"))
                {
                    return;
                }
                Assert.Expect(typeof (Db4oIllegalStateException), new _ICodeBlock_83(this, item));
            }

            private sealed class _ICodeBlock_83 : ICodeBlock
            {
                private readonly _IEventListener4_71 _enclosing;
                private readonly Item item;

                public _ICodeBlock_83(_IEventListener4_71 _enclosing, Item
                    item)
                {
                    this._enclosing = _enclosing;
                    this.item = item;
                }

                public void Run()
                {
                    item._child = new Item("baz");
                    _enclosing._enclosing.Store(item);
                }
            }
        }

        private sealed class _IEventListener4_102
        {
            private readonly UpdateInCallbackThrowsTestCase _enclosing;
            private readonly ByRef updatedTriggered;

            public _IEventListener4_102(UpdateInCallbackThrowsTestCase _enclosing, ByRef updatedTriggered
                )
            {
                this._enclosing = _enclosing;
                this.updatedTriggered = updatedTriggered;
            }

            public void OnEvent(object sender, ObjectInfoEventArgs args
                )
            {
                var obj = args.Object;
                if (!(obj is Item))
                {
                    return;
                }
                var item = (Item) obj;
                if (!item._name.Equals("foo"))
                {
                    return;
                }
                updatedTriggered.value = true;
                Assert.Expect(typeof (Db4oIllegalStateException), new _ICodeBlock_116(this, item));
            }

            private sealed class _ICodeBlock_116 : ICodeBlock
            {
                private readonly _IEventListener4_102 _enclosing;
                private readonly Item item;

                public _ICodeBlock_116(_IEventListener4_102 _enclosing, Item
                    item)
                {
                    this._enclosing = _enclosing;
                    this.item = item;
                }

                public void Run()
                {
                    item._child = new Item("baz");
                    _enclosing._enclosing.Store(item);
                }
            }
        }
    }
}