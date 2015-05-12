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
using Db4objects.Db4o.Consistency;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Ids;
using Db4objects.Db4o.Internal.Slots;
using Db4objects.Db4o.IO;
using Db4oUnit;
using Db4oUnit.Fixtures;

namespace Db4objects.Db4o.Tests.Optional
{
    public class ConsistencyCheckerTestSuite : FixtureBasedTestSuite
    {
        private static readonly FixtureVariable BlockSize = FixtureVariable.NewInstance("blockSize"
            );

        public static void Main(string[] args)
        {
            new ConsoleTestRunner(typeof (ConsistencyCheckerTestSuite)).Run();
        }

        public override Type[] TestUnits()
        {
            return new[]
            {
                typeof (ConsistencyCheckerTestUnit
                    )
            };
        }

        public override IFixtureProvider[] FixtureProviders()
        {
            return new IFixtureProvider[]
            {
                new SimpleFixtureProvider(BlockSize, new[]
                {
                    new BlockSizeSpec(1), new BlockSizeSpec
                        (7),
                    new BlockSizeSpec(9), new BlockSizeSpec
                        (13),
                    new BlockSizeSpec(17), new BlockSizeSpec
                        (19)
                })
            };
        }

        public class BlockSizeSpec : ILabeled
        {
            private readonly int _blockSize;

            public BlockSizeSpec(int blockSize)
            {
                _blockSize = blockSize;
            }

            public virtual string Label()
            {
                return _blockSize.ToString();
            }

            public virtual int BlockSize()
            {
                return _blockSize;
            }
        }

        public class Item
        {
            internal byte[] bytes = new byte[((BlockSizeSpec) BlockSize
                .Value).BlockSize()];
        }

        public class ConsistencyCheckerTestUnit : ITestLifeCycle
        {
            private LocalObjectContainer _db;

            /// <exception cref="System.Exception"></exception>
            public virtual void SetUp()
            {
                var config = Db4oEmbedded.NewConfiguration();
                config.File.Storage = new MemoryStorage();
                config.File.BlockSize = ((BlockSizeSpec) BlockSize.Value
                    ).BlockSize();
                _db = (LocalObjectContainer) Db4oEmbedded.OpenFile(config, "inmem.db4o");
            }

            /// <exception cref="System.Exception"></exception>
            public virtual void TearDown()
            {
                _db.Close();
            }

            public virtual void TestFreeUsedSlot()
            {
                AssertInconsistencyDetected(new _IProcedure4_66(this));
            }

            public virtual void TestFreeShiftedUsedSlot()
            {
                AssertInconsistencyDetected(new _IProcedure4_76(this));
            }

            public virtual void TestNegativeAddressSlot()
            {
                AssertBogusSlotDetected(-1, 10);
            }

            public virtual void TestExceedsFileLengthSlot()
            {
                AssertBogusSlotDetected(int.MaxValue - 1, 1);
            }

            private void AssertBogusSlotDetected(int address, int length)
            {
                AssertInconsistencyDetected(new _IProcedure4_94(this, address, length));
            }

            private void AssertInconsistencyDetected(IProcedure4 proc)
            {
                var item = new Item();
                _db.Store(item);
                _db.Commit();
                Assert.IsTrue(new ConsistencyChecker(_db).CheckSlotConsistency().Consistent());
                proc.Apply(item);
                _db.Commit();
                Assert.IsFalse(new ConsistencyChecker(_db).CheckSlotConsistency().Consistent());
            }

            private sealed class _IProcedure4_66 : IProcedure4
            {
                private readonly ConsistencyCheckerTestUnit _enclosing;

                public _IProcedure4_66(ConsistencyCheckerTestUnit _enclosing)
                {
                    this._enclosing = _enclosing;
                }

                public void Apply(object item)
                {
                    var id = (int) _enclosing._db.GetID(((Item) item));
                    var slot = _enclosing._db.IdSystem().CommittedSlot(id);
                    _enclosing._db.FreespaceManager().Free(slot);
                }
            }

            private sealed class _IProcedure4_76 : IProcedure4
            {
                private readonly ConsistencyCheckerTestUnit _enclosing;

                public _IProcedure4_76(ConsistencyCheckerTestUnit _enclosing)
                {
                    this._enclosing = _enclosing;
                }

                public void Apply(object item)
                {
                    var id = (int) _enclosing._db.GetID(((Item) item));
                    var slot = _enclosing._db.IdSystem().CommittedSlot(id);
                    _enclosing._db.FreespaceManager().Free(new Slot(slot.Address() + 1, slot.Length
                        ()));
                }
            }

            private sealed class _IProcedure4_94 : IProcedure4
            {
                private readonly ConsistencyCheckerTestUnit _enclosing;
                private readonly int address;
                private readonly int length;

                public _IProcedure4_94(ConsistencyCheckerTestUnit _enclosing, int address, int length
                    )
                {
                    this._enclosing = _enclosing;
                    this.address = address;
                    this.length = length;
                }

                public void Apply(object item)
                {
                    var id = (int) _enclosing._db.GetID(((Item) item));
                    _enclosing._db.IdSystem().Commit(new _IVisitable_97(id, address, length), FreespaceCommitter
                        .DoNothing);
                }

                private sealed class _IVisitable_97 : IVisitable
                {
                    private readonly int address;
                    private readonly int id;
                    private readonly int length;

                    public _IVisitable_97(int id, int address, int length)
                    {
                        this.id = id;
                        this.address = address;
                        this.length = length;
                    }

                    public void Accept(IVisitor4 visitor)
                    {
                        var slotChange = new SlotChange(id);
                        slotChange.NotifySlotCreated(new Slot(address, length));
                        visitor.Visit(slotChange);
                    }
                }
            }
        }
    }
}