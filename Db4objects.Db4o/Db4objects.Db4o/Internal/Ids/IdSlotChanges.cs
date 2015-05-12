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

using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal.Freespace;
using Db4objects.Db4o.Internal.Slots;

namespace Db4objects.Db4o.Internal.Ids
{
    public class IdSlotChanges
    {
        private readonly IClosure4 _freespaceManager;
        private readonly TransactionalIdSystemImpl _idSystem;
        private readonly LockedTree _slotChanges = new LockedTree();
        private TreeInt _prefetchedIDs;

        public IdSlotChanges(TransactionalIdSystemImpl idSystem, IClosure4 freespaceManager
            )
        {
            _idSystem = idSystem;
            _freespaceManager = freespaceManager;
        }

        public void AccumulateFreeSlots(FreespaceCommitter freespaceCommitter, bool forFreespace
            , bool traverseMutable)
        {
            IVisitor4 visitor = new _IVisitor4_27(this, freespaceCommitter, forFreespace);
            if (traverseMutable)
            {
                _slotChanges.TraverseMutable(visitor);
            }
            else
            {
                _slotChanges.TraverseLocked(visitor);
            }
        }

        public virtual void Clear()
        {
            _slotChanges.Clear();
        }

        public virtual void Rollback()
        {
            _slotChanges.TraverseLocked(new _IVisitor4_44(this));
        }

        public virtual bool IsDeleted(int id)
        {
            var slot = FindSlotChange(id);
            if (slot == null)
            {
                return false;
            }
            return slot.IsDeleted();
        }

        public virtual SlotChange ProduceSlotChange(int id, SlotChangeFactory slotChangeFactory
            )
        {
            if (DTrace.enabled)
            {
                DTrace.ProduceSlotChange.Log(id);
            }
            var slot = slotChangeFactory.NewInstance(id);
            _slotChanges.Add(slot);
            return (SlotChange) slot.AddedOrExisting();
        }

        public SlotChange FindSlotChange(int id)
        {
            return (SlotChange) _slotChanges.Find(id);
        }

        public virtual void TraverseSlotChanges(IVisitor4 visitor)
        {
            _slotChanges.TraverseLocked(visitor);
        }

        public virtual bool IsDirty()
        {
            return !_slotChanges.IsEmpty();
        }

        public virtual void ReadSlotChanges(ByteArrayBuffer buffer)
        {
            _slotChanges.Read(buffer, new SlotChange(0));
        }

        public virtual void AddPrefetchedID(int id)
        {
            _prefetchedIDs = ((TreeInt) Tree.Add(_prefetchedIDs, new TreeInt(id)));
        }

        public virtual void PrefetchedIDConsumed(int id)
        {
            _prefetchedIDs = ((TreeInt) _prefetchedIDs.RemoveLike(new TreeInt(id)));
        }

        internal void FreePrefetchedIDs(IIdSystem idSystem)
        {
            if (_prefetchedIDs == null)
            {
                return;
            }
            idSystem.ReturnUnusedIds(_prefetchedIDs);
            _prefetchedIDs = null;
        }

        public virtual void NotifySlotCreated(int id, Slot slot, SlotChangeFactory slotChangeFactory
            )
        {
            ProduceSlotChange(id, slotChangeFactory).NotifySlotCreated(slot);
        }

        internal virtual void NotifySlotUpdated(int id, Slot slot, SlotChangeFactory slotChangeFactory
            )
        {
            ProduceSlotChange(id, slotChangeFactory).NotifySlotUpdated(FreespaceManager(), slot
                );
        }

        public virtual void NotifySlotDeleted(int id, SlotChangeFactory slotChangeFactory
            )
        {
            ProduceSlotChange(id, slotChangeFactory).NotifyDeleted(FreespaceManager());
        }

        private IFreespaceManager FreespaceManager()
        {
            return ((IFreespaceManager) _freespaceManager.Run());
        }

        private sealed class _IVisitor4_27 : IVisitor4
        {
            private readonly IdSlotChanges _enclosing;
            private readonly bool forFreespace;
            private readonly FreespaceCommitter freespaceCommitter;

            public _IVisitor4_27(IdSlotChanges _enclosing, FreespaceCommitter freespaceCommitter
                , bool forFreespace)
            {
                this._enclosing = _enclosing;
                this.freespaceCommitter = freespaceCommitter;
                this.forFreespace = forFreespace;
            }

            public void Visit(object obj)
            {
                ((SlotChange) obj).AccumulateFreeSlot(_enclosing._idSystem, freespaceCommitter
                    , forFreespace);
            }
        }

        private sealed class _IVisitor4_44 : IVisitor4
        {
            private readonly IdSlotChanges _enclosing;

            public _IVisitor4_44(IdSlotChanges _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Visit(object slotChange)
            {
                ((SlotChange) slotChange).Rollback(_enclosing.FreespaceManager());
            }
        }
    }
}