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

using System.Collections;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Slots;

namespace Db4objects.Db4o.CS.Internal.Objectexchange
{
    public class EagerObjectWriter
    {
        private readonly ObjectExchangeConfiguration _config;
        private readonly LocalTransaction _transaction;

        public EagerObjectWriter(ObjectExchangeConfiguration config, LocalTransaction transaction
            )
        {
            _config = config;
            _transaction = transaction;
        }

        public virtual ByteArrayBuffer Write(IIntIterator4 idIterator, int maxCount)
        {
            var rootIds = ReadSlots(idIterator, maxCount);
            var slots = SlotsFor(rootIds);
            var marshalledSize = MarshalledSizeFor(slots) + Const4.IntLength + rootIds.Count
                                 *Const4.IntLength;
            var buffer = new ByteArrayBuffer(marshalledSize);
            WriteIdSlotPairsTo(slots, buffer);
            WriteIds(buffer, rootIds);
            return buffer;
        }

        private void WriteIds(ByteArrayBuffer buffer, IList ids)
        {
            buffer.WriteInt(ids.Count);
            for (var idIter = ids.GetEnumerator(); idIter.MoveNext();)
            {
                var id = ((int) idIter.Current);
                buffer.WriteInt(id);
            }
        }

        private IList SlotsFor(IList ids)
        {
            return new SlotCollector(_config.prefetchDepth, new StandardReferenceCollector(_transaction
                ), new StandardSlotAccessor(_transaction)).Collect(Iterators.Take(_config.prefetchCount
                    , Iterators.Iterator(ids)));
        }

        private void WriteIdSlotPairsTo(IList slots, ByteArrayBuffer buffer)
        {
            buffer.WriteInt(slots.Count);
            for (var idSlotPairIter = slots.GetEnumerator();
                idSlotPairIter.MoveNext(
                    );)
            {
                var idSlotPair = ((Pair) idSlotPairIter.Current);
                var id = (((int) idSlotPair.first));
                var slot = ((Slot) idSlotPair.second);
                if (Slot.IsNull(slot))
                {
                    buffer.WriteInt(id);
                    buffer.WriteInt(0);
                    continue;
                }
                var slotBuffer = _transaction.LocalContainer().ReadBufferBySlot(slot);
                buffer.WriteInt(id);
                buffer.WriteInt(slot.Length());
                buffer.WriteBytes(slotBuffer._buffer);
            }
        }

        private int MarshalledSizeFor(IList slots)
        {
            var total = Const4.IntLength;
            // count
            for (var idSlotPairIter = slots.GetEnumerator();
                idSlotPairIter.MoveNext(
                    );)
            {
                var idSlotPair = ((Pair) idSlotPairIter.Current);
                total += Const4.IntLength;
                // id
                total += Const4.IntLength;
                // length
                var slot = ((Slot) idSlotPair.second);
                if (slot != null)
                {
                    total += slot.Length();
                }
            }
            return total;
        }

        private IList ReadSlots(IIntIterator4 idIterator, int maxCount)
        {
            var slots = new ArrayList();
            while (idIterator.MoveNext())
            {
                var id = idIterator.CurrentInt();
                slots.Add(id);
                if (slots.Count >= maxCount)
                {
                    break;
                }
            }
            return slots;
        }
    }
}