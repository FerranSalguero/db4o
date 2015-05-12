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
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Util;

namespace Db4objects.Db4o.Tests.Common.Events
{
    public class ClassRegistrationEventsTestCase : AbstractDb4oTestCase
    {
        public virtual void TestClassRegistrationEvents()
        {
            var eventFlag = new EventFlag
                ();
            var registry = EventRegistryFactory.ForObjectContainer(Db());
            registry.ClassRegistered += new _IEventListener4_23(eventFlag).OnEvent;
            Store(new Data());
            Assert.IsTrue(eventFlag.eventOccurred);
        }

        public class Data
        {
        }

        private class EventFlag
        {
            public bool eventOccurred;
        }

        private sealed class _IEventListener4_23
        {
            private readonly EventFlag eventFlag;

            public _IEventListener4_23(EventFlag eventFlag)
            {
                this.eventFlag = eventFlag;
            }

            public void OnEvent(object sender, ClassEventArgs args)
            {
                var classEventArgs = args;
                Assert.AreEqual(typeof (Data).FullName, CrossPlatformServices
                    .SimpleName(classEventArgs.ClassMetadata().GetName()));
                eventFlag.eventOccurred = true;
            }
        }
    }
}