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

using Db4objects.Db4o.Internal.Freespace;
using Db4objects.Db4o.Internal.Ids;

namespace Db4objects.Db4o.Internal.Slots
{
    /// <exclude></exclude>
    public class SlotChange : TreeInt
    {
        private SlotChangeOperation _currentOperation;
        private SlotChangeOperation _firstOperation;
        protected Slot _newSlot;

        public SlotChange(int id) : base(id)
        {
        }

        public override object ShallowClone()
        {
            var sc = new SlotChange(0);
            sc.NewSlot(_newSlot);
            return ShallowCloneInternal(sc);
        }

        public virtual void AccumulateFreeSlot(TransactionalIdSystemImpl idSystem, FreespaceCommitter
            freespaceCommitter, bool forFreespace)
        {
            if (ForFreespace() != forFreespace)
            {
                return;
            }
            if (_firstOperation == SlotChangeOperation.create)
            {
                return;
            }
            if (_currentOperation == SlotChangeOperation.update || _currentOperation
                == SlotChangeOperation.delete)
            {
                var slot = ModifiedSlotInParentIdSystem(idSystem);
                if (Slot.IsNull(slot))
                {
                    slot = idSystem.CommittedSlot(_key);
                }
                // No old slot at all can be the case if the object
                // has been deleted by another transaction and we add it again.
                if (!Slot.IsNull(slot))
                {
                    freespaceCommitter.DelayedFree(slot, FreeToSystemFreespaceSystem());
                }
            }
        }

        protected virtual bool ForFreespace()
        {
            return false;
        }

        protected virtual Slot ModifiedSlotInParentIdSystem(TransactionalIdSystemImpl idSystem
            )
        {
            return idSystem.ModifiedSlotInParentIdSystem(_key);
        }

        public virtual bool IsDeleted()
        {
            return SlotModified() && _newSlot.IsNull();
        }

        public virtual bool IsNew()
        {
            return _firstOperation == SlotChangeOperation.create;
        }

        private bool IsFreeOnRollback()
        {
            return !Slot.IsNull(_newSlot);
        }

        public bool SlotModified()
        {
            return _newSlot != null;
        }

        /// <summary>FIXME:	Check where pointers should be freed on commit.</summary>
        /// <remarks>
        ///     FIXME:	Check where pointers should be freed on commit.
        ///     This should be triggered in this class.
        /// </remarks>
        public virtual Slot NewSlot()
        {
            //	private final boolean isFreePointerOnCommit() {
            //		return isBitSet(FREE_POINTER_ON_COMMIT_BIT);
            //	}
            return _newSlot;
        }

        public override object Read(ByteArrayBuffer reader)
        {
            var change = new SlotChange(reader.ReadInt());
            var newSlot = new Slot(reader.ReadInt(), reader.ReadInt());
            change.NewSlot(newSlot);
            return change;
        }

        public virtual void Rollback(IFreespaceManager freespaceManager)
        {
            if (IsFreeOnRollback())
            {
                freespaceManager.Free(_newSlot);
            }
        }

        public override void Write(ByteArrayBuffer writer)
        {
            if (SlotModified())
            {
                writer.WriteInt(_key);
                writer.WriteInt(_newSlot.Address());
                writer.WriteInt(_newSlot.Length());
            }
        }

        public void WritePointer(LocalObjectContainer container)
        {
            if (SlotModified())
            {
                container.WritePointer(_key, _newSlot);
            }
        }

        private void NewSlot(Slot slot)
        {
            _newSlot = slot;
        }

        public virtual void NotifySlotUpdated(IFreespaceManager freespaceManager, Slot slot
            )
        {
            if (DTrace.enabled)
            {
                DTrace.NotifySlotUpdated.LogLength(_key, slot);
            }
            FreePreviouslyModifiedSlot(freespaceManager);
            _newSlot = slot;
            Operation(SlotChangeOperation.update);
        }

        protected virtual void FreePreviouslyModifiedSlot(IFreespaceManager freespaceManager
            )
        {
            if (Slot.IsNull(_newSlot))
            {
                return;
            }
            Free(freespaceManager, _newSlot);
            _newSlot = null;
        }

        protected virtual void Free(IFreespaceManager freespaceManager, Slot slot)
        {
            if (slot.IsNull())
            {
                return;
            }
            if (freespaceManager == null)
            {
                return;
            }
            freespaceManager.Free(slot);
        }

        private void Operation(SlotChangeOperation operation)
        {
            if (_firstOperation == null)
            {
                _firstOperation = operation;
            }
            _currentOperation = operation;
        }

        public virtual void NotifySlotCreated(Slot slot)
        {
            if (DTrace.enabled)
            {
                DTrace.NotifySlotCreated.Log(_key);
                DTrace.NotifySlotCreated.LogLength(slot);
            }
            Operation(SlotChangeOperation.create);
            _newSlot = slot;
        }

        public virtual void NotifyDeleted(IFreespaceManager freespaceManager)
        {
            if (DTrace.enabled)
            {
                DTrace.NotifySlotDeleted.Log(_key);
            }
            Operation(SlotChangeOperation.delete);
            FreePreviouslyModifiedSlot(freespaceManager);
            _newSlot = Slot.Zero;
        }

        public virtual bool RemoveId()
        {
            return false;
        }

        public override string ToString()
        {
            var str = "id: " + _key;
            if (_newSlot != null)
            {
                str += " newSlot: " + _newSlot;
            }
            return str;
        }

        protected virtual bool FreeToSystemFreespaceSystem()
        {
            return false;
        }

        private class SlotChangeOperation
        {
            internal static readonly SlotChangeOperation create = new SlotChangeOperation
                ("create");

            internal static readonly SlotChangeOperation update = new SlotChangeOperation
                ("update");

            internal static readonly SlotChangeOperation delete = new SlotChangeOperation
                ("delete");

            private readonly string _type;

            public SlotChangeOperation(string type)
            {
                _type = type;
            }

            public override string ToString()
            {
                return _type;
            }
        }
    }
}