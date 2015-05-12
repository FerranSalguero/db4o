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
using Db4objects.Db4o.Foundation;
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;

namespace Db4objects.Db4o.Tests.Common.Events
{
    public class EventCountTestCase : AbstractDb4oTestCase
    {
        private const int MaxChecks = 10;
        private const long WaitTime = 10;

        private readonly SafeCounter _activated = new SafeCounter
            ();

        private readonly SafeCounter _committed = new SafeCounter
            ();

        private readonly SafeCounter _created = new SafeCounter
            ();

        private readonly SafeCounter _deleted = new SafeCounter
            ();

        private readonly SafeCounter _updated = new SafeCounter
            ();

        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            new EventCountTestCase().RunAll();
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestEventRegistryCounts()
        {
            RegisterEventHandlers();
            for (var i = 0; i < 1000; i++)
            {
                var item = new Item(i);
                Db().Store(item);
                Assert.IsTrue(Db().IsStored(item));
                if (((i + 1)%100) == 0)
                {
                    Db().Commit();
                }
            }
            AssertCount(_created, 1000, "created");
            AssertCount(_committed, 10, "commit");
            ReopenAndRegister();
            var items = NewQuery(typeof (Item)).Execute();
            Assert.AreEqual(1000, items.Count, "Wrong number of objects retrieved");
            while (items.HasNext())
            {
                var item = (Item) items.Next();
                item._value++;
                Store(item);
            }
            AssertCount(_activated, 1000, "activated");
            AssertCount(_updated, 1000, "updated");
            items.Reset();
            while (items.HasNext())
            {
                var item = items.Next();
                Db().Delete(item);
                Assert.IsFalse(Db().IsStored(item));
            }
            AssertCount(_deleted, 1000, "deleted");
        }

        /// <exception cref="System.Exception"></exception>
        private void AssertCount(SafeCounter actual, int expected, string
            name)
        {
            actual.AssertEquals(expected, MaxChecks);
        }

        /// <exception cref="System.Exception"></exception>
        private void ReopenAndRegister()
        {
            Reopen();
            RegisterEventHandlers();
        }

        private void RegisterEventHandlers()
        {
            IObjectContainer deletionEventSource = Db();
            if (Fixture() is IDb4oClientServerFixture)
            {
                var clientServerFixture = (IDb4oClientServerFixture) Fixture(
                    );
                deletionEventSource = clientServerFixture.Server().Ext().ObjectContainer();
            }
            var eventRegistry = EventRegistryFactory.ForObjectContainer(Db());
            var deletionEventRegistry = EventRegistryFactory.ForObjectContainer(deletionEventSource
                );
            // No dedicated IncrementListener class due to sharpen event semantics
            deletionEventRegistry.Deleted += new _IEventListener4_91(this).OnEvent;
            eventRegistry.Activated += new _IEventListener4_96(this).OnEvent;
            eventRegistry.Committed += new _IEventListener4_101(this).OnEvent;
            eventRegistry.Created += new _IEventListener4_106(this).OnEvent;
            eventRegistry.Updated += new _IEventListener4_111(this).OnEvent;
        }

        private sealed class _IEventListener4_91
        {
            private readonly EventCountTestCase _enclosing;

            public _IEventListener4_91(EventCountTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void OnEvent(object sender, ObjectInfoEventArgs args
                )
            {
                _enclosing._deleted.Increment();
            }
        }

        private sealed class _IEventListener4_96
        {
            private readonly EventCountTestCase _enclosing;

            public _IEventListener4_96(EventCountTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void OnEvent(object sender, ObjectInfoEventArgs args
                )
            {
                _enclosing._activated.Increment();
            }
        }

        private sealed class _IEventListener4_101
        {
            private readonly EventCountTestCase _enclosing;

            public _IEventListener4_101(EventCountTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void OnEvent(object sender, CommitEventArgs args)
            {
                _enclosing._committed.Increment();
            }
        }

        private sealed class _IEventListener4_106
        {
            private readonly EventCountTestCase _enclosing;

            public _IEventListener4_106(EventCountTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void OnEvent(object sender, ObjectInfoEventArgs args
                )
            {
                _enclosing._created.Increment();
            }
        }

        private sealed class _IEventListener4_111
        {
            private readonly EventCountTestCase _enclosing;

            public _IEventListener4_111(EventCountTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void OnEvent(object sender, ObjectInfoEventArgs args
                )
            {
                _enclosing._updated.Increment();
            }
        }

        public class Item
        {
            public int _value;

            public Item(int i)
            {
                _value = i;
            }
        }

        private class SafeCounter
        {
            private readonly Lock4 _lock = new Lock4();
            private int _value;

            public virtual void Increment()
            {
                _lock.Run(new _IClosure4_131(this));
            }

            public virtual void AssertEquals(int expected, int maxChecks)
            {
                var ret = new IntByRef();
                for (var checkCount = 0;
                    checkCount < MaxChecks && ret.value != expected;
                    checkCount
                        ++)
                {
                    _lock.Run(new _IClosure4_140(this, expected, ret));
                }
                Assert.AreEqual(expected, ret.value);
            }

            private sealed class _IClosure4_131 : IClosure4
            {
                private readonly SafeCounter _enclosing;

                public _IClosure4_131(SafeCounter _enclosing)
                {
                    this._enclosing = _enclosing;
                }

                public object Run()
                {
                    _enclosing._value++;
                    return null;
                }
            }

            private sealed class _IClosure4_140 : IClosure4
            {
                private readonly SafeCounter _enclosing;
                private readonly int expected;
                private readonly IntByRef ret;

                public _IClosure4_140(SafeCounter _enclosing, int expected, IntByRef ret)
                {
                    this._enclosing = _enclosing;
                    this.expected = expected;
                    this.ret = ret;
                }

                public object Run()
                {
                    if (_enclosing._value != expected)
                    {
                        _enclosing._lock.Snooze(WaitTime);
                    }
                    ret.value = _enclosing._value;
                    return null;
                }
            }
        }
    }
}