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

using Db4objects.Db4o.Internal.Activation;
using Db4objects.Db4o.Internal.Marshall;
using Db4objects.Db4o.Internal.Slots;
using Db4objects.Db4o.Marshall;

namespace Db4objects.Db4o.Internal.Handlers.Versions
{
    public class OpenTypeHandler7 : OpenTypeHandler
    {
        public OpenTypeHandler7(ObjectContainerBase container) : base(container)
        {
        }

        public override object Read(IReadContext readContext)
        {
            var context = (IInternalReadContext) readContext;
            var payloadOffset = context.ReadInt();
            if (payloadOffset == 0)
            {
                context.NotifyNullReferenceSkipped();
                return null;
            }
            var savedOffSet = context.Offset();
            try
            {
                var typeHandler = ReadTypeHandler(context, payloadOffset);
                if (typeHandler == null)
                {
                    return null;
                }
                if (IsPlainObject(typeHandler))
                {
                    return ReadPlainObject(readContext);
                }
                SeekSecondaryOffset(context, typeHandler);
                return context.ReadAtCurrentSeekPosition(typeHandler);
            }
            finally
            {
                context.Seek(savedOffSet);
            }
        }

        public override void Defragment(IDefragmentContext context)
        {
            var payLoadOffSet = context.ReadInt();
            if (payLoadOffSet == 0)
            {
                return;
            }
            var savedOffSet = context.Offset();
            context.Seek(payLoadOffSet);
            var classMetadataId = context.CopyIDReturnOriginalID();
            var typeHandler = CorrectTypeHandlerVersionFor(context, classMetadataId
                );
            if (typeHandler != null)
            {
                if (IsPlainObject(typeHandler))
                {
                    context.CopySlotlessID();
                }
                else
                {
                    SeekSecondaryOffset(context, typeHandler);
                    context.Defragment(typeHandler);
                }
            }
            context.Seek(savedOffSet);
        }

        private object ReadPlainObject(IReadContext context)
        {
            var id = context.ReadInt();
            var transaction = context.Transaction();
            var obj = transaction.ObjectForIdFromCache(id);
            if (obj != null)
            {
                return obj;
            }
            obj = new object();
            AddReference(context, obj, id);
            return obj;
        }

        private void AddReference(IContext context, object obj, int id)
        {
            var transaction = context.Transaction();
            ObjectReference @ref = new _ObjectReference_74(id);
            @ref.ClassMetadata(transaction.Container().ClassMetadataForID(Handlers4.UntypedId
                ));
            @ref.SetObjectWeak(transaction.Container(), obj);
            transaction.AddNewReference(@ref);
        }

        private sealed class _ObjectReference_74 : ObjectReference
        {
            internal bool _firstUpdate;

            public _ObjectReference_74(int baseArg1) : base(baseArg1)
            {
                _firstUpdate = true;
            }

            public override void WriteUpdate(Transaction transaction, IUpdateDepth updatedepth
                )
            {
                if (!_firstUpdate)
                {
                    base.WriteUpdate(transaction, updatedepth);
                    return;
                }
                _firstUpdate = false;
                var container = transaction.Container();
                SetStateClean();
                var context = new MarshallingContext(transaction, this, updatedepth
                    , false);
                Handlers4.Write(ClassMetadata().TypeHandler(), context, GetObject());
                var length = Container().BlockConverter().BlockAlignedBytes(context.MarshalledLength
                    ());
                var slot = context.AllocateNewSlot(length);
                var pointer = new Pointer4(GetID(), slot);
                var buffer = context.ToWriteBuffer(pointer);
                container.WriteUpdate(transaction, pointer, ClassMetadata(), ArrayType.None,
                    buffer);
                if (IsActive())
                {
                    SetStateClean();
                }
            }
        }
    }
}