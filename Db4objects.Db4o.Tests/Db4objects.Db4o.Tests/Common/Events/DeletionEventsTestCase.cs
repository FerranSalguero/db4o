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
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Events;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Events
{
    public class DeletionEventsTestCase : EventsTestCaseBase
    {
        protected override void Configure(IConfiguration config)
        {
            config.ActivationDepth(1);
        }

        public virtual void TestDeletionEvents()
        {
            if (IsEmbedded())
            {
                // TODO: something wrong when embedded c/s is run as part
                // of the full test suite
                return;
            }
            var deletionLog = new EventLog();
            ServerEventRegistry().Deleting += new _IEventListener4_25(this, deletionLog).OnEvent;
            ServerEventRegistry().Deleted += new _IEventListener4_31(this, deletionLog).OnEvent;
            Db().Delete(((Item) RetrieveOnlyInstance(typeof (Item
                ))));
            Db().Commit();
            Assert.IsTrue(deletionLog.xing);
            Assert.IsTrue(deletionLog.xed);
        }

        private void AssertItemIsActive(EventArgs args)
        {
            Assert.AreEqual(1, ItemForEvent(args).id);
        }

        private Item ItemForEvent(EventArgs args)
        {
            return ((Item) ((ObjectEventArgs) args).Object);
        }

        private sealed class _IEventListener4_25
        {
            private readonly DeletionEventsTestCase _enclosing;
            private readonly EventLog deletionLog;

            public _IEventListener4_25(DeletionEventsTestCase _enclosing, EventLog
                deletionLog)
            {
                this._enclosing = _enclosing;
                this.deletionLog = deletionLog;
            }

            public void OnEvent(object sender, CancellableObjectEventArgs
                args)
            {
                deletionLog.xing = true;
                _enclosing.AssertItemIsActive(args);
            }
        }

        private sealed class _IEventListener4_31
        {
            private readonly DeletionEventsTestCase _enclosing;
            private readonly EventLog deletionLog;

            public _IEventListener4_31(DeletionEventsTestCase _enclosing, EventLog
                deletionLog)
            {
                this._enclosing = _enclosing;
                this.deletionLog = deletionLog;
            }

            public void OnEvent(object sender, ObjectInfoEventArgs args
                )
            {
                deletionLog.xed = true;
                _enclosing.AssertItemIsActive(args);
            }
        }
    }
}