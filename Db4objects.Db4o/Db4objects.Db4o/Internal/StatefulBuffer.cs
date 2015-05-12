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
using Db4objects.Db4o.Internal.Slots;

namespace Db4objects.Db4o.Internal
{
    /// <summary>
    ///     public for .NET conversion reasons
    ///     TODO: Split this class for individual usecases.
    /// </summary>
    /// <remarks>
    ///     public for .NET conversion reasons
    ///     TODO: Split this class for individual usecases. Only use the member
    ///     variables needed for the respective usecase.
    /// </remarks>
    /// <exclude></exclude>
    public sealed class StatefulBuffer : ByteArrayBuffer
    {
        private int _address;
        private int _addressOffset;
        private int _cascadeDelete;
        private int _id;
        private int _length;
        internal Transaction _trans;

        public StatefulBuffer(Transaction trans, int initialBufferSize
            )
        {
            _trans = trans;
            _length = initialBufferSize;
            _buffer = new byte[_length];
        }

        public StatefulBuffer(Transaction trans, int address, int
            length) : this(trans, length)
        {
            _address = address;
        }

        public StatefulBuffer(Transaction trans, Slot
            slot) : this(trans, slot.Address(), slot.Length())
        {
        }

        public StatefulBuffer(Transaction trans, Pointer4 pointer
            ) : this(trans, pointer._slot)
        {
            _id = pointer._id;
        }

        public void DebugCheckBytes()
        {
        }

        // Db4o.log("!!! YapBytes.debugCheckBytes not all bytes used");
        // This is normal for writing The FreeSlotArray, becauce one
        // slot is possibly reserved by it's own pointer.
        public int GetAddress()
        {
            return _address;
        }

        public int GetID()
        {
            return _id;
        }

        public override int Length()
        {
            return _length;
        }

        public ObjectContainerBase Container()
        {
            return _trans.Container();
        }

        public LocalObjectContainer File()
        {
            return ((LocalTransaction) _trans).LocalContainer();
        }

        public Transaction Transaction()
        {
            return _trans;
        }

        public byte[] GetWrittenBytes()
        {
            var bytes = new byte[_offset];
            Array.Copy(_buffer, 0, bytes, 0, _offset);
            return bytes;
        }

        /// <exception cref="Db4objects.Db4o.Ext.Db4oIOException"></exception>
        public void Read()
        {
            Container().ReadBytes(_buffer, _address, _addressOffset, _length);
        }

        public StatefulBuffer ReadStatefulBuffer()
        {
            var length = ReadInt();
            if (length == 0)
            {
                return null;
            }
            var yb = new StatefulBuffer
                (_trans, length);
            Array.Copy(_buffer, _offset, yb._buffer, 0, length);
            _offset += length;
            return yb;
        }

        public void RemoveFirstBytes(int aLength)
        {
            _length -= aLength;
            var temp = new byte[_length];
            Array.Copy(_buffer, aLength, temp, 0, _length);
            _buffer = temp;
            _offset -= aLength;
            if (_offset < 0)
            {
                _offset = 0;
            }
        }

        public void Address(int address)
        {
            _address = address;
        }

        public void SetID(int id)
        {
            _id = id;
        }

        public void SetTransaction(Transaction aTrans)
        {
            _trans = aTrans;
        }

        public void UseSlot(int adress)
        {
            _address = adress;
            _offset = 0;
        }

        // FIXME: FB remove
        public void UseSlot(int address, int length)
        {
            UseSlot(new Slot(address, length));
        }

        public void UseSlot(Slot slot)
        {
            _address = slot.Address();
            _offset = 0;
            if (slot.Length() > _buffer.Length)
            {
                _buffer = new byte[slot.Length()];
            }
            _length = slot.Length();
        }

        // FIXME: FB remove
        public void UseSlot(int id, int adress, int length)
        {
            _id = id;
            UseSlot(adress, length);
        }

        public void Write()
        {
            File().WriteBytes(this, _address, _addressOffset);
        }

        public void WriteEncrypt()
        {
            File().WriteEncrypt(this, _address, _addressOffset);
        }

        public ByteArrayBuffer ReadPayloadWriter(int offset, int length)
        {
            var payLoad = new StatefulBuffer
                (_trans, 0, length);
            Array.Copy(_buffer, offset, payLoad._buffer, 0, length);
            TransferPayLoadAddress(payLoad, offset);
            return payLoad;
        }

        private void TransferPayLoadAddress(StatefulBuffer toWriter
            , int offset)
        {
            var blockedOffset = offset/Container().BlockSize();
            toWriter._address = _address + blockedOffset;
            toWriter._id = toWriter._address;
            toWriter._addressOffset = _addressOffset;
        }

        public void MoveForward(int length)
        {
            _addressOffset += length;
        }

        public override string ToString()
        {
            return "id " + _id + " adr " + _address + " len " + _length;
        }

        public Slot Slot()
        {
            return new Slot(_address, _length);
        }

        public Pointer4 Pointer()
        {
            return new Pointer4(_id, Slot());
        }

        public int CascadeDeletes()
        {
            return _cascadeDelete;
        }

        public void SetCascadeDeletes(int depth)
        {
            _cascadeDelete = depth;
        }
    }
}