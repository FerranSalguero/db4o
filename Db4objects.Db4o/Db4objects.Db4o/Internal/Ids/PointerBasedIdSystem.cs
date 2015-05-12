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
using Db4objects.Db4o.Internal.Slots;
using Db4objects.Db4o.Internal.Transactionlog;

namespace Db4objects.Db4o.Internal.Ids
{
    /// <exclude></exclude>
    public sealed class PointerBasedIdSystem : IIdSystem
    {
        private readonly LocalObjectContainer _container;
        internal readonly TransactionLogHandler _transactionLogHandler;

        public PointerBasedIdSystem(LocalObjectContainer container)
        {
            _container = container;
            _transactionLogHandler = NewTransactionLogHandler(container);
        }

        public int NewId()
        {
            return _container.AllocatePointerSlot();
        }

        public Slot CommittedSlot(int id)
        {
            return _container.ReadPointerSlot(id);
        }

        public void Commit(IVisitable slotChanges, FreespaceCommitter freespaceCommitter)
        {
            var reservedSlot = _transactionLogHandler.AllocateSlot(false, CountSlotChanges(slotChanges
                ));
            freespaceCommitter.Commit();
            _transactionLogHandler.ApplySlotChanges(slotChanges, CountSlotChanges(slotChanges
                ), reservedSlot);
        }

        public void ReturnUnusedIds(IVisitable visitable)
        {
            visitable.Accept(new _IVisitor4_51(this));
        }

        public void Close()
        {
            _transactionLogHandler.Close();
        }

        public void CompleteInterruptedTransaction(int transactionId1, int transactionId2
            )
        {
            _transactionLogHandler.CompleteInterruptedTransaction(transactionId1, transactionId2
                );
        }

        public void TraverseOwnSlots(IProcedure4 block)
        {
        }

        private int CountSlotChanges(IVisitable slotChanges)
        {
            var slotChangeCount = new IntByRef();
            slotChanges.Accept(new _IVisitor4_40(slotChangeCount));
            return slotChangeCount.value;
        }

        private TransactionLogHandler NewTransactionLogHandler(LocalObjectContainer container
            )
        {
            var fileBased = container.Config().FileBasedTransactionLog() && container is IoAdaptedObjectContainer;
            if (!fileBased)
            {
                return new EmbeddedTransactionLogHandler(container);
            }
            var fileName = ((IoAdaptedObjectContainer) container).FileName();
            return new FileBasedTransactionLogHandler(container, fileName);
        }

        private sealed class _IVisitor4_40 : IVisitor4
        {
            private readonly IntByRef slotChangeCount;

            public _IVisitor4_40(IntByRef slotChangeCount)
            {
                this.slotChangeCount = slotChangeCount;
            }

            public void Visit(object slotChange)
            {
                if (((SlotChange) slotChange).SlotModified())
                {
                    slotChangeCount.value++;
                }
            }
        }

        private sealed class _IVisitor4_51 : IVisitor4
        {
            private readonly PointerBasedIdSystem _enclosing;

            public _IVisitor4_51(PointerBasedIdSystem _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Visit(object id)
            {
                _enclosing._container.Free((((int) id)), Const4.PointerLength);
            }
        }
    }
}