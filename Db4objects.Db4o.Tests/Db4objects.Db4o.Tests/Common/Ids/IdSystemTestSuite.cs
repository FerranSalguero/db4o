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
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Config;
using Db4objects.Db4o.Internal.Freespace;
using Db4objects.Db4o.Internal.Ids;
using Db4objects.Db4o.Internal.Slots;
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;
using Db4oUnit.Fixtures;

namespace Db4objects.Db4o.Tests.Common.Ids
{
    public class IdSystemTestSuite : FixtureBasedTestSuite
    {
        private const int SlotLength = 10;
        private static readonly int MaxValidId = 1000;
        private static readonly FixtureVariable _fixture = FixtureVariable.NewInstance("IdSystem");

        public override IFixtureProvider[] FixtureProviders()
        {
            return new IFixtureProvider[]
            {
                new Db4oFixtureProvider(), new SimpleFixtureProvider
                    (_fixture, new IIdSystemProvider[]
                    {
                        new _IIdSystemProvider_236
                            (),
                        new _IIdSystemProvider_253(), new _IIdSystemProvider_270()
                    })
            };
        }

        public override Type[] TestUnits()
        {
            return new[]
            {
                typeof (IdSystemTestUnit), typeof (IdOverflowTestUnit
                    )
            };
        }

        public class IdSystemTestUnit : AbstractDb4oTestCase, IOptOutMultiSession, IDb4oTestCase
        {
            /// <exception cref="System.Exception"></exception>
            protected override void Configure(IConfiguration config)
            {
                var idSystemConfiguration = Db4oLegacyConfigurationBridge.AsIdSystemConfiguration
                    (config);
                ((IIdSystemProvider) _fixture.Value).Apply(idSystemConfiguration
                    );
            }

            public virtual void TestSlotForNewIdDoesNotExist()
            {
                var newId = IdSystem().NewId();
                Slot oldSlot = null;
                try
                {
                    oldSlot = IdSystem().CommittedSlot(newId);
                }
                catch (InvalidIDException)
                {
                }
                Assert.IsFalse(IsValid(oldSlot));
            }

            public virtual void TestSingleNewSlot()
            {
                var id = IdSystem().NewId();
                Assert.AreEqual(AllocateNewSlot(id), IdSystem().CommittedSlot(id));
            }

            public virtual void TestSingleSlotUpdate()
            {
                var id = IdSystem().NewId();
                AllocateNewSlot(id);
                var slotChange = SlotChangeFactory.UserObjects.NewInstance(id);
                var updatedSlot = LocalContainer().AllocateSlot(SlotLength);
                slotChange.NotifySlotUpdated(FreespaceManager(), updatedSlot);
                Commit(new[] {slotChange});
                Assert.AreEqual(updatedSlot, IdSystem().CommittedSlot(id));
            }

            public virtual void TestSingleSlotDelete()
            {
                var id = IdSystem().NewId();
                AllocateNewSlot(id);
                var slotChange = SlotChangeFactory.UserObjects.NewInstance(id);
                slotChange.NotifyDeleted(FreespaceManager());
                Commit(new[] {slotChange});
                Assert.IsFalse(IsValid(IdSystem().CommittedSlot(id)));
            }

            public virtual void TestReturnUnusedIds()
            {
                var id = IdSystem().NewId();
                var slot = IdSystem().CommittedSlot(id);
                Assert.AreEqual(Slot.Zero, slot);
                IdSystem().ReturnUnusedIds(new _IVisitable_83(id));
                if (IdSystem() is PointerBasedIdSystem)
                {
                    slot = IdSystem().CommittedSlot(id);
                    Assert.AreEqual(Slot.Zero, slot);
                }
                else
                {
                    Assert.Expect(typeof (InvalidIDException), new _ICodeBlock_93(this, id));
                }
            }

            private Slot AllocateNewSlot(int newId)
            {
                var slotChange = SlotChangeFactory.UserObjects.NewInstance(newId);
                var allocatedSlot = LocalContainer().AllocateSlot(SlotLength);
                slotChange.NotifySlotCreated(allocatedSlot);
                Commit(new[] {slotChange});
                return allocatedSlot;
            }

            private void Commit(SlotChange[] slotChanges)
            {
                IdSystem().Commit(new _IVisitable_112(slotChanges), FreespaceCommitter.DoNothing);
            }

            private LocalObjectContainer LocalContainer()
            {
                return (LocalObjectContainer) Container();
            }

            private bool IsValid(Slot slot)
            {
                return !Slot.IsNull(slot);
            }

            private IFreespaceManager FreespaceManager()
            {
                return LocalContainer().FreespaceManager();
            }

            private IIdSystem IdSystem()
            {
                return LocalContainer().IdSystem();
            }

            private sealed class _IVisitable_83 : IVisitable
            {
                private readonly int id;

                public _IVisitable_83(int id)
                {
                    this.id = id;
                }

                public void Accept(IVisitor4 visitor)
                {
                    visitor.Visit(id);
                }
            }

            private sealed class _ICodeBlock_93 : ICodeBlock
            {
                private readonly IdSystemTestUnit _enclosing;
                private readonly int id;

                public _ICodeBlock_93(IdSystemTestUnit _enclosing, int id)
                {
                    this._enclosing = _enclosing;
                    this.id = id;
                }

                /// <exception cref="System.Exception"></exception>
                public void Run()
                {
                    _enclosing.IdSystem().CommittedSlot(id);
                }
            }

            private sealed class _IVisitable_112 : IVisitable
            {
                private readonly SlotChange[] slotChanges;

                public _IVisitable_112(SlotChange[] slotChanges)
                {
                    this.slotChanges = slotChanges;
                }

                public void Accept(IVisitor4 visitor)
                {
                    for (var slotChangeIndex = 0; slotChangeIndex < slotChanges.Length; ++slotChangeIndex)
                    {
                        var slotChange = slotChanges[slotChangeIndex];
                        visitor.Visit(slotChange);
                    }
                }
            }
        }

        public class IdOverflowTestUnit : AbstractDb4oTestCase, IOptOutMultiSession, IDb4oTestCase
        {
            public virtual void TestNewIdOverflow()
            {
                if (!((IIdSystemProvider) _fixture.Value).SupportsIdOverflow())
                {
                    return;
                }
                var container = (LocalObjectContainer) Container();
                var idSystem = ((IIdSystemProvider) _fixture.Value).NewInstance
                    (container);
                var allFreeIds = AllocateAllAvailableIds(idSystem);
                AssertNoMoreIdAvailable(idSystem);
                IList subSetOfIds = new ArrayList();
                var counter = 0;
                for (var currentIdIter = allFreeIds.GetEnumerator();
                    currentIdIter.MoveNext
                        ();)
                {
                    var currentId = ((int) currentIdIter.Current);
                    counter++;
                    if (counter%3 == 0)
                    {
                        subSetOfIds.Add(currentId);
                    }
                }
                AssertFreeAndReallocate(idSystem, subSetOfIds);
                AssertFreeAndReallocate(idSystem, allFreeIds);
            }

            private void AssertFreeAndReallocate(IIdSystem idSystem, IList ids)
            {
                // Boundary condition: Last ID. Produced a bug when implementing. 
                if (!ids.Contains(MaxValidId))
                {
                    ids.Add(MaxValidId);
                }
                Assert.IsGreater(0, ids.Count);
                idSystem.ReturnUnusedIds(new _IVisitable_184(ids));
                var freedCount = ids.Count;
                for (var i = 0; i < freedCount; i++)
                {
                    var newId = idSystem.NewId();
                    Assert.IsTrue(ids.Contains(newId));
                    ids.Remove(newId);
                }
                Assert.IsTrue(ids.Count == 0);
                AssertNoMoreIdAvailable(idSystem);
            }

            private IList AllocateAllAvailableIds(IIdSystem idSystem)
            {
                IList ids = new ArrayList();
                var newId = 0;
                do
                {
                    newId = idSystem.NewId();
                    ids.Add(newId);
                } while (newId < MaxValidId);
                return ids;
            }

            private void AssertNoMoreIdAvailable(IIdSystem idSystem)
            {
                Assert.Expect(typeof (Db4oFatalException), new _ICodeBlock_219(idSystem));
            }

            private sealed class _IVisitable_184 : IVisitable
            {
                private readonly IList ids;

                public _IVisitable_184(IList ids)
                {
                    this.ids = ids;
                }

                public void Accept(IVisitor4 visitor)
                {
                    for (var expectedFreeIdIter = ids.GetEnumerator();
                        expectedFreeIdIter.MoveNext
                            ();)
                    {
                        var expectedFreeId = ((int) expectedFreeIdIter.Current);
                        visitor.Visit(expectedFreeId);
                    }
                }
            }

            private sealed class _ICodeBlock_219 : ICodeBlock
            {
                private readonly IIdSystem idSystem;

                public _ICodeBlock_219(IIdSystem idSystem)
                {
                    this.idSystem = idSystem;
                }

                /// <exception cref="System.Exception"></exception>
                public void Run()
                {
                    idSystem.NewId();
                }
            }
        }

        private sealed class _IIdSystemProvider_236 : IIdSystemProvider
        {
            public void Apply(IIdSystemConfiguration idSystemConfiguration)
            {
                idSystemConfiguration.UsePointerBasedSystem();
            }

            public IIdSystem NewInstance(LocalObjectContainer container)
            {
                return null;
            }

            public bool SupportsIdOverflow()
            {
                return false;
            }

            public string Label()
            {
                return "PointerBased";
            }
        }

        private sealed class _IIdSystemProvider_253 : IIdSystemProvider
        {
            public void Apply(IIdSystemConfiguration idSystemConfiguration)
            {
                idSystemConfiguration.UseInMemorySystem();
            }

            public IIdSystem NewInstance(LocalObjectContainer container)
            {
                return new InMemoryIdSystem(container, MaxValidId);
            }

            public bool SupportsIdOverflow()
            {
                return true;
            }

            public string Label()
            {
                return "InMemory";
            }
        }

        private sealed class _IIdSystemProvider_270 : IIdSystemProvider
        {
            public void Apply(IIdSystemConfiguration idSystemConfiguration)
            {
                idSystemConfiguration.UseStackedBTreeSystem();
            }

            public IIdSystem NewInstance(LocalObjectContainer container)
            {
                return new BTreeIdSystem(container, new InMemoryIdSystem(container), MaxValidId);
            }

            public bool SupportsIdOverflow()
            {
                // FIXME: implement next
                return false;
            }

            public string Label()
            {
                return "BTree";
            }
        }

        private interface IIdSystemProvider : ILabeled
        {
            void Apply(IIdSystemConfiguration idSystemConfiguration);
            bool SupportsIdOverflow();
            IIdSystem NewInstance(LocalObjectContainer container);
        }
    }
}