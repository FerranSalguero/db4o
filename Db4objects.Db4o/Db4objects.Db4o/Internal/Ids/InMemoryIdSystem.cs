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
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal.Slots;

namespace Db4objects.Db4o.Internal.Ids
{
    /// <exclude></exclude>
    public class InMemoryIdSystem : IStackableIdSystem
    {
        private readonly LocalObjectContainer _container;
        private readonly SequentialIdGenerator _idGenerator;
        private int _childId;
        private IdSlotTree _ids;
        private Slot _slot;

        /// <summary>for testing purposes only.</summary>
        /// <remarks>for testing purposes only.</remarks>
        public InMemoryIdSystem(LocalObjectContainer container, int maxValidId)
        {
            _container = container;
            _idGenerator = new SequentialIdGenerator(new _IFunction4_32(this, maxValidId), _container
                .Handlers.LowestValidId(), maxValidId);
        }

        public InMemoryIdSystem(LocalObjectContainer container) : this(container, int.MaxValue
            )
        {
            ReadThis();
        }

        public virtual void Close()
        {
        }

        // do nothing
        public virtual void Commit(IVisitable slotChanges, FreespaceCommitter freespaceCommitter
            )
        {
            var oldSlot = _slot;
            var reservedSlot = AllocateSlot(false, EstimatedSlotLength(EstimateMappingCount(
                slotChanges)));
            // No more operations against the FreespaceManager.
            // Time to free old slots.
            freespaceCommitter.Commit();
            slotChanges.Accept(new _IVisitor4_69(this));
            WriteThis(reservedSlot);
            FreeSlot(oldSlot);
        }

        public virtual Slot CommittedSlot(int id)
        {
            var idSlotMapping = (IdSlotTree) Tree.Find(_ids, new TreeInt(id));
            if (idSlotMapping == null)
            {
                throw new InvalidIDException(id);
            }
            return idSlotMapping.Slot();
        }

        public virtual void CompleteInterruptedTransaction(int address, int length)
        {
        }

        // do nothing
        public virtual int NewId()
        {
            var id = _idGenerator.NewId();
            _ids = ((IdSlotTree) Tree.Add(_ids, new IdSlotTree(id, Slot.Zero)));
            return id;
        }

        public virtual void ReturnUnusedIds(IVisitable visitable)
        {
            visitable.Accept(new _IVisitor4_233(this));
        }

        public virtual int ChildId()
        {
            return _childId;
        }

        public virtual void ChildId(int id)
        {
            _childId = id;
        }

        public virtual void TraverseOwnSlots(IProcedure4 block)
        {
            block.Apply(Pair.Of(0, _slot));
        }

        private void ReadThis()
        {
            var systemData = _container.SystemData();
            _slot = systemData.IdSystemSlot();
            if (!Slot.IsNull(_slot))
            {
                var buffer = _container.ReadBufferBySlot(_slot);
                _childId = buffer.ReadInt();
                _idGenerator.Read(buffer);
                _ids = (IdSlotTree) new TreeReader(buffer, new IdSlotTree(0, null)).Read();
            }
        }

        private Slot AllocateSlot(bool appendToFile, int slotLength)
        {
            if (!appendToFile)
            {
                var slot = _container.FreespaceManager().AllocateSafeSlot(slotLength);
                if (slot != null)
                {
                    return slot;
                }
            }
            return _container.AppendBytes(slotLength);
        }

        private int EstimateMappingCount(IVisitable slotChanges)
        {
            var count = new IntByRef();
            count.value = _ids == null ? 0 : _ids.Size();
            slotChanges.Accept(new _IVisitor4_103(count));
            return count.value;
        }

        private void WriteThis(Slot reservedSlot)
        {
            // We need a little dance here to keep filling free slots
            // with X bytes. The FreespaceManager would do it immediately
            // upon the free call, but then our CrashSimulatingTestCase
            // fails because we have the Xses in the file before flushing.
            Slot xByteSlot = null;
            var slotLength = SlotLength();
            if (reservedSlot.Length() >= slotLength)
            {
                _slot = reservedSlot;
                reservedSlot = null;
            }
            else
            {
                _slot = AllocateSlot(true, slotLength);
            }
            var buffer = new ByteArrayBuffer(_slot.Length());
            buffer.WriteInt(_childId);
            _idGenerator.Write(buffer);
            TreeInt.Write(buffer, _ids);
            _container.WriteBytes(buffer, _slot.Address(), 0);
            _container.SystemData().IdSystemSlot(_slot);
            var commitHook = _container.CommitHook();
            _container.SyncFiles(commitHook);
            FreeSlot(reservedSlot);
        }

        private void FreeSlot(Slot slot)
        {
            if (Slot.IsNull(slot))
            {
                return;
            }
            var freespaceManager = _container.FreespaceManager();
            if (freespaceManager == null)
            {
                return;
            }
            freespaceManager.FreeSafeSlot(slot);
        }

        private int SlotLength()
        {
            return TreeInt.MarshalledLength(_ids) + _idGenerator.MarshalledLength() + Const4.
                IdLength;
        }

        private int EstimatedSlotLength(int estimatedCount)
        {
            var template = _ids;
            if (template == null)
            {
                template = new IdSlotTree(0, new Slot(0, 0));
            }
            return template.MarshalledLength(estimatedCount) + _idGenerator.MarshalledLength(
                ) + Const4.IdLength;
        }

        private int FindFreeId(int start, int end)
        {
            if (_ids == null)
            {
                return start;
            }
            var lastId = new IntByRef();
            var freeId = new IntByRef();
            Tree.Traverse(_ids, new TreeInt(start), new _ICancellableVisitor4_204(lastId, start
                , freeId));
            if (freeId.value > 0)
            {
                return freeId.value;
            }
            if (lastId.value < end)
            {
                return Math.Max(start, lastId.value + 1);
            }
            return 0;
        }

        private sealed class _IFunction4_32 : IFunction4
        {
            private readonly InMemoryIdSystem _enclosing;
            private readonly int maxValidId;

            public _IFunction4_32(InMemoryIdSystem _enclosing, int maxValidId)
            {
                this._enclosing = _enclosing;
                this.maxValidId = maxValidId;
            }

            public object Apply(object start)
            {
                return _enclosing.FindFreeId((((int) start)), maxValidId);
            }
        }

        private sealed class _IVisitor4_69 : IVisitor4
        {
            private readonly InMemoryIdSystem _enclosing;

            public _IVisitor4_69(InMemoryIdSystem _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Visit(object slotChange)
            {
                if (!((SlotChange) slotChange).SlotModified())
                {
                    return;
                }
                if (((SlotChange) slotChange).RemoveId())
                {
                    _enclosing._ids = (IdSlotTree) Tree.RemoveLike(_enclosing._ids, new TreeInt
                        (((TreeInt) slotChange)._key));
                    return;
                }
                if (DTrace.enabled)
                {
                    DTrace.SlotCommitted.LogLength(((TreeInt) slotChange)._key, ((SlotChange) slotChange
                        ).NewSlot());
                }
                _enclosing._ids = ((IdSlotTree) Tree.Add(_enclosing._ids, new IdSlotTree
                    (((TreeInt) slotChange)._key, ((SlotChange) slotChange).NewSlot())));
            }
        }

        private sealed class _IVisitor4_103 : IVisitor4
        {
            private readonly IntByRef count;

            public _IVisitor4_103(IntByRef count)
            {
                this.count = count;
            }

            public void Visit(object slotChange)
            {
                if (!((SlotChange) slotChange).SlotModified() || ((SlotChange) slotChange).RemoveId
                    ())
                {
                    return;
                }
                count.value++;
            }
        }

        private sealed class _ICancellableVisitor4_204 : ICancellableVisitor4
        {
            private readonly IntByRef freeId;
            private readonly IntByRef lastId;
            private readonly int start;

            public _ICancellableVisitor4_204(IntByRef lastId, int start, IntByRef freeId)
            {
                this.lastId = lastId;
                this.start = start;
                this.freeId = freeId;
            }

            public bool Visit(object node)
            {
                var id = ((TreeInt) node)._key;
                if (lastId.value == 0)
                {
                    if (id > start)
                    {
                        freeId.value = start;
                        return false;
                    }
                    lastId.value = id;
                    return true;
                }
                if (id > lastId.value + 1)
                {
                    freeId.value = lastId.value + 1;
                    return false;
                }
                lastId.value = id;
                return true;
            }
        }

        private sealed class _IVisitor4_233 : IVisitor4
        {
            private readonly InMemoryIdSystem _enclosing;

            public _IVisitor4_233(InMemoryIdSystem _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Visit(object obj)
            {
                _enclosing._ids = (IdSlotTree) Tree.RemoveLike(_enclosing._ids, new TreeInt
                    ((((int) obj))));
            }
        }
    }
}