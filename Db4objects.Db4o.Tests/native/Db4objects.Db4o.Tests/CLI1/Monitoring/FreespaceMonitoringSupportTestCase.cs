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
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal.Slots;
using Db4objects.Db4o.Monitoring;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.CLI1.Monitoring
{
    public class FreespaceMonitoringSupportTestCase : PerformanceCounterTestCaseBase
    {
        protected override void Configure(IConfiguration config)
        {
            config.Add(new FreespaceMonitoringSupport());
        }

        public void Test()
        {
            // ensure client is fully connected to the server already
            Db().Commit();
            AssertMonitoredFreespaceIsCorrect();
            var item = new Item();
            Store(item);
            Db().Commit();
            AssertMonitoredFreespaceIsCorrect();
            Db().Delete(item);
            Db().Commit();
            AssertMonitoredFreespaceIsCorrect();
        }

        private void AssertMonitoredFreespaceIsCorrect()
        {
            var freespaceManager = FileSession().FreespaceManager();
            var visitor = new FreespaceCountingVisitor();
            freespaceManager.Traverse(visitor);
            var freespace = visitor.TotalFreespace;
            var slotCount = visitor.SlotCount;
            var averageSlotSize = slotCount == 0 ? 0 : freespace/slotCount;
            Assert.AreEqual(freespace, TotalFreespace());
            Assert.AreEqual(slotCount, SlotCount());
            Assert.AreEqual(averageSlotSize, AverageSlotSize());
        }

        private int TotalFreespace()
        {
            return (int) PerformanceCounterSpec.TotalFreespace.PerformanceCounter(FileSession()).RawValue;
        }

        private int SlotCount()
        {
            return (int) PerformanceCounterSpec.FreespaceSlotCount.PerformanceCounter(FileSession()).RawValue;
        }

        private int AverageSlotSize()
        {
            return (int) PerformanceCounterSpec.FreespaceAverageSlotSize.PerformanceCounter(FileSession()).RawValue;
        }

        public class Item
        {
        }

        public class FreespaceCountingVisitor : IVisitor4
        {
            public int TotalFreespace { get; private set; }
            public int SlotCount { get; private set; }

            public void Visit(object obj)
            {
                var slot = obj as Slot;
                TotalFreespace += slot.Length();
                SlotCount++;
            }
        }
    }
}

#endif