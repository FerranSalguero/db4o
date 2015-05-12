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
using Db4objects.Db4o.Events;
using Db4objects.Db4o.Foundation;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Events
{
    public class ExceptionPropagationInEventsTestUnit : EventsTestCaseBase
    {
        private readonly Hashtable _eventFirer = new Hashtable();

        public ExceptionPropagationInEventsTestUnit()
        {
            _eventFirer["insert"] = NewObjectInserter();
            _eventFirer["query"] = NewQueryRunner();
            _eventFirer["update"] = NewObjectUpdater();
            _eventFirer["delete"] = NewObjectDeleter();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Store()
        {
            Store(new Item(1));
            Store(new Item(2));
        }

        public virtual void TestEvents()
        {
            var @event = EventToTest();
            if (IsEmbedded())
            {
                return;
            }
            if (IsNetworking() && !@event.IsClientServerEvent())
            {
                return;
            }
            AssertEventThrows(@event.EventFirerName(), ((ICodeBlock) _eventFirer[@event.EventFirerName
                ()]), @event.ListenerSetter());
        }

        private EventInfo EventToTest()
        {
            return (EventInfo) ExceptionPropagationInEventsTestVariables.EventSelector.Value;
        }

        private void AssertEventThrows(string eventName, ICodeBlock codeBlock, IProcedure4
            listenerSetter)
        {
            var eventRegistry = EventRegistryFactory.ForObjectContainer(Db());
            listenerSetter.Apply(eventRegistry);
            Assert.Expect(typeof (EventException), typeof (NotImplementedException), codeBlock,
                eventName);
        }

        private ICodeBlock NewObjectUpdater()
        {
            return new _ICodeBlock_50(this);
        }

        private ICodeBlock NewObjectDeleter()
        {
            return new _ICodeBlock_64(this);
        }

        private ICodeBlock NewQueryRunner()
        {
            return new _ICodeBlock_73(this);
        }

        private ICodeBlock NewObjectInserter()
        {
            return new _ICodeBlock_81(this);
        }

        private Item RetrieveItem(int id)
        {
            var query = NewQuery(typeof (Item));
            query.Descend("id").Constrain(id);
            var results = query.Execute();
            Assert.AreEqual(1, results.Count);
            var found = ((Item) results.Next());
            Assert.AreEqual(id, found.id);
            return found;
        }

        private sealed class _ICodeBlock_50 : ICodeBlock
        {
            private readonly ExceptionPropagationInEventsTestUnit _enclosing;

            public _ICodeBlock_50(ExceptionPropagationInEventsTestUnit _enclosing)
            {
                this._enclosing = _enclosing;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                var item = _enclosing.RetrieveItem(1);
                item.id = 10;
                _enclosing.Db().Store(item);
                _enclosing.Db().Commit();
            }
        }

        private sealed class _ICodeBlock_64 : ICodeBlock
        {
            private readonly ExceptionPropagationInEventsTestUnit _enclosing;

            public _ICodeBlock_64(ExceptionPropagationInEventsTestUnit _enclosing)
            {
                this._enclosing = _enclosing;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                _enclosing.Db().Delete(_enclosing.RetrieveItem(1));
                _enclosing.Db().Commit();
            }
        }

        private sealed class _ICodeBlock_73 : ICodeBlock
        {
            private readonly ExceptionPropagationInEventsTestUnit _enclosing;

            public _ICodeBlock_73(ExceptionPropagationInEventsTestUnit _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Run()
            {
                _enclosing.RetrieveItem(1);
            }
        }

        private sealed class _ICodeBlock_81 : ICodeBlock
        {
            private readonly ExceptionPropagationInEventsTestUnit _enclosing;

            public _ICodeBlock_81(ExceptionPropagationInEventsTestUnit _enclosing)
            {
                this._enclosing = _enclosing;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                _enclosing.Db().Store(new Item());
                _enclosing.Db().Commit();
            }
        }
    }
}