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
using Db4objects.Db4o.Internal.Encoding;
using Db4objects.Db4o.Internal.Handlers;

namespace Db4objects.Db4o.Internal.Marshall
{
    /// <exclude></exclude>
    public class FieldMarshaller0 : AbstractFieldMarshaller
    {
        public override int MarshalledLength(ObjectContainerBase stream, ClassAspect aspect
            )
        {
            var len = stream.StringIO().ShortLength(aspect.GetName());
            if (aspect is FieldMetadata)
            {
                var field = (FieldMetadata) aspect;
                if (field.NeedsArrayAndPrimitiveInfo())
                {
                    len += 1;
                }
                if (!(field is VirtualFieldMetadata))
                {
                    len += Const4.IdLength;
                }
            }
            return len;
        }

        protected override RawFieldSpec ReadSpec(AspectType aspectType, ObjectContainerBase
            stream, ByteArrayBuffer reader)
        {
            var name = StringHandler.ReadStringNoDebug(stream.Transaction.Context(), reader
                );
            if (!aspectType.IsFieldMetadata())
            {
                return new RawFieldSpec(aspectType, name);
            }
            if (name.IndexOf(Const4.VirtualFieldPrefix) == 0)
            {
                if (stream._handlers.VirtualFieldByName(name) != null)
                {
                    return new RawFieldSpec(aspectType, name);
                }
            }
            var fieldTypeID = reader.ReadInt();
            var attribs = reader.ReadByte();
            return new RawFieldSpec(aspectType, name, fieldTypeID, attribs);
        }

        public override sealed FieldMetadata Read(ObjectContainerBase stream, ClassMetadata
            containingClass, ByteArrayBuffer reader)
        {
            var spec = ReadSpec(stream, reader);
            return FromSpec(spec, stream, containingClass);
        }

        protected virtual FieldMetadata FromSpec(RawFieldSpec spec, ObjectContainerBase stream
            , ClassMetadata containingClass)
        {
            if (spec == null)
            {
                return null;
            }
            var name = spec.Name();
            if (spec.IsVirtualField())
            {
                return stream._handlers.VirtualFieldByName(name);
            }
            if (spec.IsTranslator())
            {
                return new TranslatedAspect(containingClass, name);
            }
            if (spec.IsField())
            {
                return new FieldMetadata(containingClass, name, spec.FieldTypeID(), spec.IsPrimitive
                    (), spec.IsArray(), spec.IsNArray());
            }
            return new UnknownTypeHandlerAspect(containingClass, name);
        }

        public override void Write(Transaction trans, ClassMetadata clazz, ClassAspect aspect
            , ByteArrayBuffer writer)
        {
            writer.WriteShortString(trans, aspect.GetName());
            if (!(aspect is FieldMetadata))
            {
                return;
            }
            var field = (FieldMetadata) aspect;
            field.Alive();
            if (field.IsVirtual())
            {
                return;
            }
            var handler = field.GetHandler();
            if (handler is StandardReferenceTypeHandler)
            {
                // TODO: ensure there is a test case, to make this happen 
                if (((StandardReferenceTypeHandler) handler).ClassMetadata().GetID() == 0)
                {
                    trans.Container().NeedsUpdate(clazz);
                }
            }
            writer.WriteInt(field.FieldTypeID());
            var bitmap = new BitMap4(3);
            bitmap.Set(0, field.IsPrimitive());
            bitmap.Set(1, Handlers4.HandlesArray(handler));
            bitmap.Set(2, Handlers4.HandlesMultidimensionalArray(handler));
            // keep the order
            writer.WriteByte(bitmap.GetByte(0));
        }

        public override void Defrag(ClassMetadata classMetadata, ClassAspect aspect, LatinStringIO
            sio, DefragmentContextImpl context)
        {
            context.IncrementStringOffset(sio);
            if (!(aspect is FieldMetadata))
            {
                return;
            }
            if (((FieldMetadata) aspect).IsVirtual())
            {
                return;
            }
            // handler ID
            context.CopyID();
            // skip primitive/array/narray attributes
            context.IncrementOffset(1);
        }
    }
}