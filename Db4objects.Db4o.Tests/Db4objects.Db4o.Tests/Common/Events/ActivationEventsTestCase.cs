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
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Events
{
    public class ActivationEventsTestCase : EventsTestCaseBase
    {
        protected override void Configure(IConfiguration config)
        {
            config.ActivationDepth(1);
        }

        public virtual void TestActivationEvents()
        {
            var activationLog = new EventLog();
            EventRegistry().Activating += new _IEventListener4_19(this, activationLog).OnEvent;
            EventRegistry().Activated += new _IEventListener4_25(this, activationLog).OnEvent;
            RetrieveOnlyInstance(typeof (Item));
            Assert.IsTrue(activationLog.xing);
            Assert.IsTrue(activationLog.xed);
        }

        private sealed class _IEventListener4_19
        {
            private readonly ActivationEventsTestCase _enclosing;
            private readonly EventLog activationLog;

            public _IEventListener4_19(ActivationEventsTestCase _enclosing, EventLog
                activationLog)
            {
                this._enclosing = _enclosing;
                this.activationLog = activationLog;
            }

            public void OnEvent(object sender, CancellableObjectEventArgs
                args)
            {
                _enclosing.AssertClientTransaction(args);
                activationLog.xing = true;
            }
        }

        private sealed class _IEventListener4_25
        {
            private readonly ActivationEventsTestCase _enclosing;
            private readonly EventLog activationLog;

            public _IEventListener4_25(ActivationEventsTestCase _enclosing, EventLog
                activationLog)
            {
                this._enclosing = _enclosing;
                this.activationLog = activationLog;
            }

            public void OnEvent(object sender, ObjectInfoEventArgs args
                )
            {
                _enclosing.AssertClientTransaction(args);
                activationLog.xed = true;
            }
        }
    }
}