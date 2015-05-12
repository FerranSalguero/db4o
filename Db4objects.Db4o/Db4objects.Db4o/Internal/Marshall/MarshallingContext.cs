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
using Db4objects.Db4o.Internal.Activation;
using Db4objects.Db4o.Internal.Slots;
using Db4objects.Db4o.Marshall;
using Db4objects.Db4o.Typehandlers;

namespace Db4objects.Db4o.Internal.Marshall
{
    /// <exclude></exclude>
    public class MarshallingContext : IMarshallingInfo, IWriteContext
    {
        private const int HeaderLength = Const4.LeadingLength + Const4.IdLength + 1 + Const4
            .IntLength;

        private readonly bool _isNew;
        private readonly BitMap4 _nullBitMap;
        private readonly ObjectReference _reference;
        private readonly Transaction _transaction;
        private readonly MarshallingBuffer _writeBuffer;
        private MarshallingBuffer _currentBuffer;
        private object _currentIndexEntry;
        private object _currentMarshalledObject;
        private ByteArrayBuffer _debugPrepend;
        private int _declaredAspectCount;
        private IUpdateDepth _updateDepth;

        public MarshallingContext(Transaction trans, ObjectReference
            @ref, IUpdateDepth updateDepth, bool isNew)
        {
            // YapClass ID
            // Marshaller Version
            // number of fields
            _transaction = trans;
            _reference = @ref;
            _nullBitMap = new BitMap4(AspectCount());
            _updateDepth = ClassMetadata().AdjustUpdateDepth(trans, updateDepth);
            _isNew = isNew;
            _writeBuffer = new MarshallingBuffer();
            _currentBuffer = _writeBuffer;
        }

        public virtual ClassMetadata ClassMetadata()
        {
            return _reference.ClassMetadata();
        }

        public virtual bool IsNull(int fieldIndex)
        {
            // TODO Auto-generated method stub
            return false;
        }

        public virtual void BeginSlot()
        {
            _currentBuffer = _writeBuffer;
        }

        // FIXME: This method was just temporarily added to fulfill contract of MarshallingInfo
        //        It will go, the buffer is never needed in new marshalling. 
        public virtual IReadBuffer Buffer()
        {
            return null;
        }

        public virtual int DeclaredAspectCount()
        {
            return _declaredAspectCount;
        }

        public virtual void DeclaredAspectCount(int count)
        {
            _declaredAspectCount = count;
        }

        public virtual Transaction Transaction()
        {
            return _transaction;
        }

        public virtual IObjectContainer ObjectContainer()
        {
            return Transaction().ObjectContainer();
        }

        public virtual void WriteByte(byte b)
        {
            PreWrite();
            _currentBuffer.WriteByte(b);
            PostWrite();
        }

        public virtual void WriteBytes(byte[] bytes)
        {
            PreWrite();
            _currentBuffer.WriteBytes(bytes);
            PostWrite();
        }

        public virtual void WriteInt(int i)
        {
            PreWrite();
            _currentBuffer.WriteInt(i);
            PostWrite();
        }

        public virtual void WriteLong(long l)
        {
            PreWrite();
            _currentBuffer.WriteLong(l);
            PostWrite();
        }

        public virtual void WriteObject(object obj)
        {
            var id = Container().StoreInternal(Transaction(), obj, _updateDepth, true);
            WriteInt(id);
            _currentMarshalledObject = obj;
            _currentIndexEntry = id;
        }

        public virtual void WriteObject(ITypeHandler4 handler, object obj)
        {
            var state = CurrentState();
            WriteObjectWithCurrentState(handler, obj);
            RestoreState(state);
        }

        public virtual IReservedBuffer Reserve(int length)
        {
            PreWrite();
            var reservedBuffer = _currentBuffer.Reserve(length);
            PostWrite();
            return reservedBuffer;
        }

        private int AspectCount()
        {
            return ClassMetadata().AspectCount();
        }

        public virtual bool IsNew()
        {
            return _isNew;
        }

        public virtual void IsNull(int fieldIndex, bool flag)
        {
            _nullBitMap.Set(fieldIndex, flag);
        }

        public virtual Slot AllocateNewSlot(int length)
        {
            if (_transaction is LocalTransaction)
            {
                return LocalContainer().AllocateSlotForNewUserObject(_transaction, ObjectID(), length
                    );
            }
            return new Slot(Slot.New, length);
        }

        private Slot AllocateUpdateSlot(int length)
        {
            if (_transaction is LocalTransaction)
            {
                return LocalContainer().AllocateSlotForUserObjectUpdate(Transaction(), ObjectID()
                    , length);
            }
            return new Slot(Slot.Update, length);
        }

        private LocalObjectContainer LocalContainer()
        {
            return ((LocalTransaction) Transaction()).LocalContainer();
        }

        public virtual Pointer4 AllocateSlot()
        {
            var length = Container().BlockConverter().BlockAlignedBytes(MarshalledLength());
            var slot = IsNew() ? AllocateNewSlot(length) : AllocateUpdateSlot(length);
            return new Pointer4(ObjectID(), slot);
        }

        public virtual ByteArrayBuffer ToWriteBuffer(Pointer4 pointer)
        {
            var buffer = new ByteArrayBuffer(pointer.Length());
            _writeBuffer.MergeChildren(this, pointer.Address(), WriteBufferOffset());
            WriteObjectClassID(buffer, ClassMetadata().GetID());
            buffer.WriteByte(HandlerRegistry.HandlerVersion);
            buffer.WriteInt(AspectCount());
            buffer.WriteBitMap(_nullBitMap);
            _writeBuffer.TransferContentTo(buffer);
            return buffer;
        }

        private int WriteBufferOffset()
        {
            return HeaderLength + _nullBitMap.MarshalledLength();
        }

        public virtual int MarshalledLength()
        {
            var length = WriteBufferOffset();
            _writeBuffer.CheckBlockAlignment(this, null, new IntByRef(length));
            return length + _writeBuffer.MarshalledLength() + Const4.BracketsBytes;
        }

        public virtual int RequiredLength(MarshallingBuffer buffer, bool align)
        {
            if (!align)
            {
                return buffer.Length();
            }
            return Container().BlockConverter().BlockAlignedBytes(buffer.Length());
        }

        private void WriteObjectClassID(ByteArrayBuffer reader, int id)
        {
            reader.WriteInt(-id);
        }

        public virtual object GetObject()
        {
            return _reference.GetObject();
        }

        public virtual Config4Class ClassConfiguration()
        {
            return ClassMetadata().Config();
        }

        public virtual IUpdateDepth UpdateDepth()
        {
            return _updateDepth;
        }

        public virtual void UpdateDepth(IUpdateDepth depth)
        {
            _updateDepth = depth;
        }

        public virtual int ObjectID()
        {
            return _reference.GetID();
        }

        public virtual object CurrentIndexEntry()
        {
            // TODO Auto-generated method stub
            return null;
        }

        public virtual ObjectContainerBase Container()
        {
            return Transaction().Container();
        }

        private void PreWrite()
        {
        }

        private void PostWrite()
        {
        }

        public virtual void CreateChildBuffer(bool storeLengthInLink)
        {
            var childBuffer = _currentBuffer.AddChild(false, storeLengthInLink);
            _currentBuffer.ReserveChildLinkSpace(storeLengthInLink);
            _currentBuffer = childBuffer;
        }

        public virtual void WriteDeclaredAspectCount(int count)
        {
            _writeBuffer.WriteInt(count);
        }

        public virtual void DebugPrependNextWrite(ByteArrayBuffer prepend)
        {
        }

        public virtual void DebugWriteEnd(byte b)
        {
            _currentBuffer.WriteByte(b);
        }

        public virtual void WriteObjectWithCurrentState(ITypeHandler4 handler, object obj
            )
        {
            if (Handlers4.UseDedicatedSlot(this, handler))
            {
                WriteObject(obj);
            }
            else
            {
                if (obj == null)
                {
                    WriteNullReference(handler);
                }
                else
                {
                    CreateIndirectionWithinSlot(handler);
                    handler.Write(this, obj);
                }
            }
        }

        private void WriteNullReference(ITypeHandler4 handler)
        {
            if (IsIndirectedWithinSlot(handler))
            {
                WriteNullLink();
                return;
            }
            Handlers4.Write(handler, this, Handlers4.NullRepresentationInUntypedArrays(handler
                ));
        }

        private void WriteNullLink()
        {
            WriteInt(0);
            WriteInt(0);
        }

        public virtual void AddIndexEntry(FieldMetadata fieldMetadata, object obj)
        {
            if (!_currentBuffer.HasParent())
            {
                var indexEntry = (obj == _currentMarshalledObject) ? _currentIndexEntry : obj;
                if (_isNew || !UpdateDepth().CanSkip(_reference))
                {
                    fieldMetadata.AddIndexEntry(Transaction(), ObjectID(), indexEntry);
                }
                return;
            }
            _currentBuffer.RequestIndexEntry(fieldMetadata);
        }

        public virtual void PurgeFieldIndexEntriesOnUpdate(Transaction
            transaction, ArrayType arrayType)
        {
            if (!UpdateDepth().CanSkip(_reference))
            {
                transaction.WriteUpdateAdjustIndexes(_reference.GetID(), _reference.ClassMetadata
                    (), arrayType);
            }
        }

        public virtual ObjectReference Reference()
        {
            return _reference;
        }

        public virtual void CreateIndirectionWithinSlot(ITypeHandler4 handler)
        {
            if (IsIndirectedWithinSlot(handler))
            {
                CreateIndirectionWithinSlot();
            }
        }

        public virtual void CreateIndirectionWithinSlot()
        {
            CreateChildBuffer(true);
        }

        private bool IsIndirectedWithinSlot(ITypeHandler4 handler)
        {
            return SlotFormat.Current().IsIndirectedWithinSlot(handler);
        }

        public virtual MarshallingContextState CurrentState()
        {
            return new MarshallingContextState(_currentBuffer);
        }

        public virtual void RestoreState(MarshallingContextState state)
        {
            _currentBuffer = state._buffer;
        }
    }
}