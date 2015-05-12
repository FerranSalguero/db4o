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
    public class InstantiationEventsTestCase : EventsTestCaseBase
    {
        protected override void Configure(IConfiguration config)
        {
            config.ActivationDepth(0);
        }

        public virtual void TestInstantiationEvents()
        {
            var instantiatedLog = new EventLog();
            EventRegistry().Instantiated += new _IEventListener4_20(this, instantiatedLog).OnEvent;
            RetrieveOnlyInstance(typeof (Item));
            Assert.IsFalse(instantiatedLog.xing);
            Assert.IsTrue(instantiatedLog.xed);
        }

        private sealed class _IEventListener4_20
        {
            private readonly InstantiationEventsTestCase _enclosing;
            private readonly EventLog instantiatedLog;

            public _IEventListener4_20(InstantiationEventsTestCase _enclosing, EventLog
                instantiatedLog)
            {
                this._enclosing = _enclosing;
                this.instantiatedLog = instantiatedLog;
            }

            public void OnEvent(object sender, ObjectInfoEventArgs args
                )
            {
                _enclosing.AssertClientTransaction(args);
                instantiatedLog.xed = true;
                var obj = args.Object;
                var objectReference = _enclosing.Trans().ReferenceSystem().ReferenceForObject
                    (obj);
                Assert.IsNotNull(objectReference);
                Assert.AreSame(objectReference, args.Info);
            }
        }
    }
}