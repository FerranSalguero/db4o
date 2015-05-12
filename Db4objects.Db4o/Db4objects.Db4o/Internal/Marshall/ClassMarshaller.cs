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
using Db4objects.Db4o.Internal.Encoding;

namespace Db4objects.Db4o.Internal.Marshall
{
    /// <exclude></exclude>
    public abstract class ClassMarshaller
    {
        public MarshallerFamily _family;

        public virtual RawClassSpec ReadSpec(Transaction trans, ByteArrayBuffer reader)
        {
            var nameBytes = ReadName(trans, reader);
            var className = trans.Container().StringIO().Read(nameBytes);
            ReadMetaClassID(reader);
            // skip
            var ancestorID = reader.ReadInt();
            reader.IncrementOffset(Const4.IntLength);
            // index ID
            var numFields = reader.ReadInt();
            return new RawClassSpec(className, ancestorID, numFields);
        }

        public virtual void Write(Transaction trans, ClassMetadata clazz, ByteArrayBuffer
            writer)
        {
            writer.WriteShortString(trans, clazz.NameToWrite());
            var intFormerlyKnownAsMetaClassID = 0;
            writer.WriteInt(intFormerlyKnownAsMetaClassID);
            writer.WriteIDOf(trans, clazz._ancestor);
            WriteIndex(trans, clazz, writer);
            writer.WriteInt(clazz.DeclaredAspectCount());
            clazz.TraverseDeclaredAspects(new _IProcedure4_39(this, trans, clazz, writer));
        }

        protected virtual void WriteIndex(Transaction trans, ClassMetadata clazz, ByteArrayBuffer
            writer)
        {
            var indexID = clazz.Index().Write(trans);
            writer.WriteInt(IndexIDForWriting(indexID));
        }

        protected abstract int IndexIDForWriting(int indexID);

        public byte[] ReadName(Transaction trans, ByteArrayBuffer reader)
        {
            return ReadName(trans.Container().StringIO(), reader);
        }

        public int ReadMetaClassID(ByteArrayBuffer reader)
        {
            return reader.ReadInt();
        }

        private byte[] ReadName(LatinStringIO sio, ByteArrayBuffer reader)
        {
            var nameBytes = sio.Bytes(reader);
            reader.IncrementOffset(nameBytes.Length);
            nameBytes = Platform4.UpdateClassName(nameBytes);
            return nameBytes;
        }

        public void Read(ObjectContainerBase stream, ClassMetadata clazz, ByteArrayBuffer
            reader)
        {
            clazz.SetAncestor(stream.ClassMetadataForID(reader.ReadInt()));
            //        if(clazz.callConstructor()){
            //            // The logic further down checks the ancestor YapClass, whether
            //            // or not it is allowed, not to call constructors. The ancestor
            //            // YapClass may possibly have not been loaded yet.
            //            clazz.createConstructor(true);
            //        }
            clazz.CheckType();
            ReadIndex(stream, clazz, reader);
            clazz._aspects = ReadAspects(stream, reader, clazz);
        }

        protected abstract void ReadIndex(ObjectContainerBase stream, ClassMetadata clazz
            , ByteArrayBuffer reader);

        private ClassAspect[] ReadAspects(ObjectContainerBase stream, ByteArrayBuffer reader
            , ClassMetadata clazz)
        {
            var aspects = new ClassAspect[reader.ReadInt()];
            for (var i = 0; i < aspects.Length; i++)
            {
                aspects[i] = _family._field.Read(stream, clazz, reader);
                aspects[i].SetHandle(i);
            }
            return aspects;
        }

        public virtual int MarshalledLength(ObjectContainerBase stream, ClassMetadata clazz
            )
        {
            var len = new IntByRef(stream.StringIO().ShortLength(clazz.NameToWrite()) +
                                   Const4.ObjectLength + (Const4.IntLength*2) + (Const4.IdLength));
            len.value += clazz.Index().OwnLength();
            clazz.TraverseDeclaredAspects(new _IProcedure4_108(this, len, stream));
            return len.value;
        }

        public virtual void Defrag(ClassMetadata classMetadata, LatinStringIO sio, DefragmentContextImpl
            context, int classIndexID)
        {
            ReadName(sio, context.SourceBuffer());
            ReadName(sio, context.TargetBuffer());
            var metaClassID = 0;
            context.WriteInt(metaClassID);
            // ancestor ID
            context.CopyID();
            context.WriteInt((classMetadata.HasClassIndex()
                ? IndexIDForWriting(classIndexID)
                : 0));
            var aspectCount = context.ReadInt();
            if (aspectCount > classMetadata.DeclaredAspectCount())
            {
                throw new InvalidOperationException();
            }
            var processedAspectCount = new IntByRef(0);
            classMetadata.TraverseDeclaredAspects(new _IProcedure4_136(this, processedAspectCount
                , aspectCount, classMetadata, sio, context));
        }

        private sealed class _IProcedure4_39 : IProcedure4
        {
            private readonly ClassMarshaller _enclosing;
            private readonly ClassMetadata clazz;
            private readonly Transaction trans;
            private readonly ByteArrayBuffer writer;

            public _IProcedure4_39(ClassMarshaller _enclosing, Transaction trans, ClassMetadata
                clazz, ByteArrayBuffer writer)
            {
                this._enclosing = _enclosing;
                this.trans = trans;
                this.clazz = clazz;
                this.writer = writer;
            }

            public void Apply(object arg)
            {
                _enclosing._family._field.Write(trans, clazz, (ClassAspect) arg, writer);
            }
        }

        private sealed class _IProcedure4_108 : IProcedure4
        {
            private readonly ClassMarshaller _enclosing;
            private readonly IntByRef len;
            private readonly ObjectContainerBase stream;

            public _IProcedure4_108(ClassMarshaller _enclosing, IntByRef len, ObjectContainerBase
                stream)
            {
                this._enclosing = _enclosing;
                this.len = len;
                this.stream = stream;
            }

            public void Apply(object arg)
            {
                len.value += _enclosing._family._field.MarshalledLength(stream, (ClassAspect
                    ) arg);
            }
        }

        private sealed class _IProcedure4_136 : IProcedure4
        {
            private readonly ClassMarshaller _enclosing;
            private readonly int aspectCount;
            private readonly ClassMetadata classMetadata;
            private readonly DefragmentContextImpl context;
            private readonly IntByRef processedAspectCount;
            private readonly LatinStringIO sio;

            public _IProcedure4_136(ClassMarshaller _enclosing, IntByRef processedAspectCount
                , int aspectCount, ClassMetadata classMetadata, LatinStringIO sio, DefragmentContextImpl
                    context)
            {
                this._enclosing = _enclosing;
                this.processedAspectCount = processedAspectCount;
                this.aspectCount = aspectCount;
                this.classMetadata = classMetadata;
                this.sio = sio;
                this.context = context;
            }

            public void Apply(object arg)
            {
                if (processedAspectCount.value >= aspectCount)
                {
                    return;
                }
                var aspect = (ClassAspect) arg;
                _enclosing._family._field.Defrag(classMetadata, aspect, sio, context);
                processedAspectCount.value++;
            }
        }
    }
}