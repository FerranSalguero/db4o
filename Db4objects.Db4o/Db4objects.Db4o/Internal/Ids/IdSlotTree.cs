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

namespace Db4objects.Db4o.Internal.Ids
{
    /// <exclude></exclude>
    public class IdSlotTree : TreeInt
    {
        private readonly Slot _slot;

        public IdSlotTree(int id, Slot slot) : base(id)
        {
            _slot = slot;
        }

        public virtual Slot Slot()
        {
            return _slot;
        }

        public override Tree OnAttemptToAddDuplicate(Tree oldNode)
        {
            _preceding = oldNode._preceding;
            _subsequent = oldNode._subsequent;
            _size = oldNode._size;
            return this;
        }

        public override int OwnLength()
        {
            return Const4.IntLength*3;
        }

        // _key, _slot._address, _slot._length 
        public override object Read(ByteArrayBuffer buffer)
        {
            var id = buffer.ReadInt();
            var slot = new Slot
                (buffer.ReadInt(), buffer.ReadInt());
            return new IdSlotTree(id, slot);
        }

        public override void Write(ByteArrayBuffer buffer)
        {
            buffer.WriteInt(_key);
            buffer.WriteInt(_slot.Address());
            buffer.WriteInt(_slot.Length());
        }
    }
}