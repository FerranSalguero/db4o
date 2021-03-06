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

#if !CF && !SILVERLIGHT

using Db4objects.Db4o.Config;
using Db4objects.Db4o.Monitoring;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.CLI1.Monitoring
{
    public class ReferenceSystemMonitoringSupportTestCase : PerformanceCounterTestCaseBase
    {
        protected override void Configure(IConfiguration config)
        {
            config.Add(new ReferenceSystemMonitoringSupport());
        }

        public void TestObjectReferenceCount()
        {
            var objectCount = 10;
            var items = new Item[objectCount];
            for (var i = 0; i < objectCount; i++)
            {
                Assert.AreEqual(ReferenceCountForDb4oDatabase() + i, ObjectReferenceCount());
                items[i] = new Item();
                Store(items[i]);
            }
            Db().Purge(items[0]);
            Assert.AreEqual(ReferenceCountForDb4oDatabase() + objectCount - 1, ObjectReferenceCount());
        }

        private int ObjectReferenceCount()
        {
            return (int) PerformanceCounterSpec.ObjectReferenceCount.PerformanceCounter(MonitoredContainer()).RawValue;
        }

        private int ReferenceCountForDb4oDatabase()
        {
            if (IsNetworking())
            {
                return 0;
            }
            return 1;
        }

        public class Item
        {
        }
    }
}

#endif