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
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal.Config;
using Db4objects.Db4o.Internal.Freespace;
using Db4objects.Db4o.Internal.Slots;
using Db4oUnit;
using Db4oUnit.Extensions.Fixtures;

namespace Db4objects.Db4o.Tests.Common.Freespace
{
    public class FreespaceManagerTypeChangeTestCase : FreespaceManagerTestCaseBase, IOptOutMultiSession
        , IOptOutDefragSolo, IOptOutNonStandardBlockSize
    {
        private const bool Verbose = false;

        /// <summary>
        ///     The magic numbers for the limits were found empirically
        ///     using "what we have" and adding a reserve.
        /// </summary>
        /// <remarks>
        ///     The magic numbers for the limits were found empirically
        ///     using "what we have" and adding a reserve.
        ///     Settings may need to be higher if we add new complexity
        ///     to how our engine works.
        /// </remarks>
        private const long UsedSpaceCreepLimit = 1200;

        private const long FragmentationCreepLimit = 10;
        private const long TotalUsedSpaceCreepLimit = 12000;
        private const long TotalFragmentationCreepLimit = 100;
        private static readonly string ItemName = "one";
        private static readonly int Btree = 0;
        private static readonly int Ram = 1;
        internal int[] _fragmentation = new int[2];
        internal int[] _initialFragmentation = new int[2];
        internal int[] _initialUsedSpace = new int[2];
        internal int _maxFragmentationCreep;
        internal int _maxUsedSpaceCreep;
        internal int[] _usedSpace = new int[2];
        private IConfiguration configuration;

        public static void Main(string[] args)
        {
            new FreespaceManagerTypeChangeTestCase().RunSolo();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            base.Configure(config);
            config.Freespace().UseBTreeSystem();
            configuration = config;
            Db4oLegacyConfigurationBridge.AsIdSystemConfiguration(config).UseInMemorySystem();
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestSwitchingBackAndForth()
        {
            ProduceSomeFreeSpace();
            PrintStatus();
            Db().Commit();
            PrintStatus();
            StoreItem();
            PrintStatus();
            Db().Commit();
            for (var run = 0; run < 50; run++)
            {
                // produceSomeFreeSpace();
                // db().commit();
                PrintStatus();
                AssertFreespace(Btree, run);
                configuration.Freespace().UseRamSystem();
                Reopen();
                AssertFreespaceManagerClass(typeof (InMemoryFreespaceManager));
                AssertItemAvailable();
                DeleteItem();
                StoreItem();
                PrintStatus();
                AssertFreespace(Ram, run);
                configuration.Freespace().UseBTreeSystem();
                Reopen();
                AssertFreespaceManagerClass(typeof (BTreeFreespaceManager));
                AssertItemAvailable();
                DeleteItem();
                StoreItem();
            }
        }

        private void StoreItem()
        {
            Store(new Item(ItemName));
        }

        private void DeleteItem()
        {
            Db().Delete(((Item) RetrieveOnlyInstance(typeof (
                Item))));
        }

        private void AssertItemAvailable()
        {
            var item = (Item) RetrieveOnlyInstance(typeof (Item
                ));
            Assert.AreEqual(ItemName, item._name);
        }

        private void AssertFreespace(int system, int run)
        {
            var calculatedFreespaceSize = CalculatedFreespaceSize();
            var fileSize = FileSize();
            var usedSpace = (int) (fileSize - calculatedFreespaceSize);
            var fragmentation = FreespaceSlots().Size();
            if (run == 0)
            {
                _usedSpace[system] = usedSpace;
                _fragmentation[system] = fragmentation;
                _initialFragmentation[system] = fragmentation;
                _initialUsedSpace[system] = usedSpace;
                return;
            }
            if (usedSpace > _usedSpace[system])
            {
                var usedSpaceCreep = usedSpace - _usedSpace[system];
                _usedSpace[system] = usedSpace;
                if (usedSpaceCreep > _maxUsedSpaceCreep)
                {
                    _maxUsedSpaceCreep = usedSpaceCreep;
                }
            }
            Print("Max space CREEP " + _maxUsedSpaceCreep);
            if (fragmentation > _fragmentation[system])
            {
                var fragmentationCreep = fragmentation - _fragmentation[system];
                _fragmentation[system] = fragmentation;
                if (fragmentationCreep > _maxFragmentationCreep)
                {
                    _maxFragmentationCreep = fragmentationCreep;
                }
            }
            Print("Max Fragmentation CREEP " + _maxFragmentationCreep);
            var totalUsedSpaceCreep = usedSpace - _initialUsedSpace[system];
            var totalFragmentationCreep = fragmentation - _initialFragmentation[system];
            Print("Total space CREEP " + totalUsedSpaceCreep);
            Print("Total Fragmentation CREEP " + totalFragmentationCreep);
            Assert.IsSmaller(FragmentationCreepLimit, _maxFragmentationCreep);
            Assert.IsSmaller(TotalFragmentationCreepLimit, totalFragmentationCreep);
            Assert.IsSmaller(UsedSpaceCreepLimit, _maxUsedSpaceCreep);
            Assert.IsSmaller(TotalUsedSpaceCreepLimit, totalUsedSpaceCreep);
        }

        private void PrintStatus()
        {
            return;
            Print("fileSize " + FileSize());
            Print("slot count " + CurrentFreespaceManager().SlotCount());
            Print("current freespace " + CurrentFreespace());
            var freespaceSlots = FreespaceSlots();
            var iterator = freespaceSlots.GetEnumerator();
            while (iterator.MoveNext())
            {
                Print(iterator.Current.ToString());
            }
            Print("calculated freespace size " + CalculatedFreespaceSize());
        }

        private long FileSize()
        {
            return FileSession().FileLength();
        }

        private Collection4 FreespaceSlots()
        {
            var collectionOfSlots = new Collection4();
            CurrentFreespaceManager().Traverse(new _IVisitor4_197(collectionOfSlots));
            return collectionOfSlots;
        }

        private int CalculatedFreespaceSize()
        {
            var size = 0;
            var i = FreespaceSlots().GetEnumerator();
            while (i.MoveNext())
            {
                var slot = (Slot) i.Current;
                size += slot.Length();
            }
            return size;
        }

        private void AssertFreespaceManagerClass(Type clazz)
        {
            Assert.IsInstanceOf(clazz, CurrentFreespaceManager());
        }

        private int CurrentFreespace()
        {
            return CurrentFreespaceManager().TotalFreespace();
        }

        private static void Print(string str)
        {
        }

        public class Item
        {
            public string _name;

            public Item(string name)
            {
                _name = name;
            }
        }

        private sealed class _IVisitor4_197 : IVisitor4
        {
            private readonly Collection4 collectionOfSlots;

            public _IVisitor4_197(Collection4 collectionOfSlots)
            {
                this.collectionOfSlots = collectionOfSlots;
            }

            public void Visit(object obj)
            {
                collectionOfSlots.Add(obj);
            }
        }
    }
}