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
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal.Slots;

namespace Db4objects.Db4o.Internal.Freespace
{
    public abstract class AbstractFreespaceManager : IFreespaceManager
    {
        public const byte FmDebug = 127;
        public const byte FmDefault = 0;
        public const byte FmLegacyRam = 1;
        public const byte FmRam = 2;
        public const byte FmIx = 3;
        public const byte FmBtree = 4;
        private const int IntsInSlot = 12;
        public const int RemainderSizeLimit = 20;
        private readonly int _discardLimit;
        private readonly int _remainderSizeLimit;
        protected IProcedure4 _slotFreedCallback;

        public AbstractFreespaceManager(IProcedure4 slotFreedCallback, int discardLimit,
            int remainderSizeLimit)
        {
            _slotFreedCallback = slotFreedCallback;
            _discardLimit = discardLimit;
            _remainderSizeLimit = remainderSizeLimit;
        }

        public virtual void MigrateTo(IFreespaceManager fm)
        {
            Traverse(new _IVisitor4_74(fm));
        }

        public virtual int TotalFreespace()
        {
            var mint = new IntByRef();
            Traverse(new _IVisitor4_99(mint));
            return mint.value;
        }

        public virtual void SlotFreed(Slot slot)
        {
            if (_slotFreedCallback == null)
            {
                return;
            }
            _slotFreedCallback.Apply(slot);
        }

        public abstract Slot AllocateSafeSlot(int arg1);
        public abstract Slot AllocateSlot(int arg1);
        public abstract Slot AllocateTransactionLogSlot(int arg1);
        public abstract void BeginCommit();
        public abstract void Commit();
        public abstract void EndCommit();
        public abstract void Free(Slot arg1);
        public abstract void FreeSafeSlot(Slot arg1);
        public abstract void FreeSelf();
        public abstract bool IsStarted();
        public abstract void Listener(IFreespaceListener arg1);
        public abstract void Read(LocalObjectContainer arg1, Slot arg2);
        public abstract int SlotCount();
        public abstract void Start(int arg1);
        public abstract byte SystemType();
        public abstract void Traverse(IVisitor4 arg1);
        public abstract void Write(LocalObjectContainer arg1);

        public static byte CheckType(byte systemType)
        {
            if (systemType == FmDefault)
            {
                return FmRam;
            }
            return systemType;
        }

        public static AbstractFreespaceManager CreateNew
            (LocalObjectContainer file)
        {
            return CreateNew(file, file.SystemData().FreespaceSystem());
        }

        public static AbstractFreespaceManager CreateNew
            (LocalObjectContainer file, byte systemType)
        {
            systemType = CheckType(systemType);
            var unblockedDiscardLimit = file.ConfigImpl.DiscardFreeSpace();
            var blockedDiscardLimit = unblockedDiscardLimit == int.MaxValue
                ? unblockedDiscardLimit
                : file.BlockConverter().BytesToBlocks(unblockedDiscardLimit);
            var remainderSizeLimit = file.BlockConverter().BytesToBlocks(RemainderSizeLimit);
            IProcedure4 slotFreedCallback = new _IProcedure4_52(file);
            switch (systemType)
            {
                case FmIx:
                {
                    return new FreespaceManagerIx(blockedDiscardLimit, remainderSizeLimit);
                }

                case FmBtree:
                {
                    return new BTreeFreespaceManager(file, slotFreedCallback, blockedDiscardLimit, remainderSizeLimit
                        );
                }

                default:
                {
                    return new InMemoryFreespaceManager(slotFreedCallback, blockedDiscardLimit, remainderSizeLimit
                        );
                    break;
                }
            }
        }

        public static int InitSlot(LocalObjectContainer file)
        {
            var address = file.AllocateSlot(SlotLength()).Address();
            SlotEntryToZeroes(file, address);
            return address;
        }

        internal static void SlotEntryToZeroes(LocalObjectContainer file, int address)
        {
            var writer = new StatefulBuffer(file.SystemTransaction(), address, SlotLength
                ());
            for (var i = 0; i < IntsInSlot; i++)
            {
                writer.WriteInt(0);
            }
            writer.WriteEncrypt();
        }

        internal static int SlotLength()
        {
            return Const4.IntLength*IntsInSlot;
        }

        protected virtual int DiscardLimit()
        {
            return _discardLimit;
        }

        protected bool SplitRemainder(int length)
        {
            if (CanDiscard(length))
            {
                return false;
            }
            return length > _remainderSizeLimit;
        }

        internal bool CanDiscard(int length)
        {
            return length == 0 || length < DiscardLimit();
        }

        public static void Migrate(IFreespaceManager oldFM, IFreespaceManager newFM)
        {
            oldFM.MigrateTo(newFM);
            oldFM.FreeSelf();
        }

        public virtual void DebugCheckIntegrity()
        {
            var lastStart = new IntByRef();
            var lastEnd = new IntByRef();
            Traverse(new _IVisitor4_131(lastEnd, lastStart));
        }

        public static bool MigrationRequired(byte systemType)
        {
            return systemType == FmLegacyRam || systemType == FmIx;
        }

        private sealed class _IProcedure4_52 : IProcedure4
        {
            private readonly LocalObjectContainer file;

            public _IProcedure4_52(LocalObjectContainer file)
            {
                this.file = file;
            }

            public void Apply(object slot)
            {
                file.OverwriteDeletedBlockedSlot(((Slot) slot));
            }
        }

        private sealed class _IVisitor4_74 : IVisitor4
        {
            private readonly IFreespaceManager fm;

            public _IVisitor4_74(IFreespaceManager fm)
            {
                this.fm = fm;
            }

            public void Visit(object obj)
            {
                fm.Free((Slot) obj);
            }
        }

        private sealed class _IVisitor4_99 : IVisitor4
        {
            private readonly IntByRef mint;

            public _IVisitor4_99(IntByRef mint)
            {
                this.mint = mint;
            }

            public void Visit(object obj)
            {
                var slot = (Slot) obj;
                mint.value += slot.Length();
            }
        }

        private sealed class _IVisitor4_131 : IVisitor4
        {
            private readonly IntByRef lastEnd;
            private readonly IntByRef lastStart;

            public _IVisitor4_131(IntByRef lastEnd, IntByRef lastStart)
            {
                this.lastEnd = lastEnd;
                this.lastStart = lastStart;
            }

            public void Visit(object obj)
            {
                var slot = (Slot) obj;
                if (slot.Address() <= lastEnd.value)
                {
                    throw new InvalidOperationException();
                }
                lastStart.value = slot.Address();
                lastEnd.value = slot.Address() + slot.Length();
            }
        }
    }
}