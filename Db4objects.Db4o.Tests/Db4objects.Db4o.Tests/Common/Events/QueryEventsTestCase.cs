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
using Db4objects.Db4o.Events;
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Events
{
    public class QueryEventsTestCase : AbstractDb4oTestCase
    {
        private bool queryFinished;
        private bool queryStarted;

        /// <exception cref="System.Exception"></exception>
        protected override void Db4oSetupAfterStore()
        {
            var events = EventRegistry();
            events.QueryStarted += new _IEventListener4_23(this).OnEvent;
            events.QueryFinished += new _IEventListener4_29(this).OnEvent;
        }

        public virtual void TestSodaQueryLifeCycleEvents()
        {
            var query = NewQuery(typeof (Item));
            query.Descend("id").Constrain(42);
            query.Execute();
            AssertEventsRaised();
        }

        public virtual void TestClassOnlyQueryLifeCycleEvents()
        {
            AssertClassOnlyQuery(typeof (Item));
        }

        public virtual void TestUntypedClassOnlyQueryLifeCycleEvents()
        {
            AssertClassOnlyQuery(typeof (object));
        }

        private void AssertClassOnlyQuery(Type clazz)
        {
            var query = NewQuery(clazz);
            query.Execute();
            AssertEventsRaised();
        }

        private void AssertEventsRaised()
        {
            Assert.IsTrue(queryStarted);
            Assert.IsTrue(queryFinished);
        }

        public class Item
        {
            public int id;
        }

        private sealed class _IEventListener4_23
        {
            private readonly QueryEventsTestCase _enclosing;

            public _IEventListener4_23(QueryEventsTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void OnEvent(object sender, QueryEventArgs args)
            {
                _enclosing.queryStarted = true;
            }
        }

        private sealed class _IEventListener4_29
        {
            private readonly QueryEventsTestCase _enclosing;

            public _IEventListener4_29(QueryEventsTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void OnEvent(object sender, QueryEventArgs args)
            {
                _enclosing.queryFinished = true;
            }
        }
    }
}