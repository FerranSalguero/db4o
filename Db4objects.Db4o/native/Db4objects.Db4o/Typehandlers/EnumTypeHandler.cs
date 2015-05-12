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
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Delete;
using Db4objects.Db4o.Internal.Marshall;
using Db4objects.Db4o.Marshall;
using Db4objects.Db4o.Reflect;
using Db4objects.Db4o.Reflect.Net;

namespace Db4objects.Db4o.Typehandlers
{
    public class EnumTypeHandler : IValueTypeHandler, ITypeFamilyTypeHandler, IIndexableTypeHandler
    {
        public IPreparedComparison PrepareComparison(IContext context, object obj)
        {
            return new PreparedEnumComparison(obj);
        }

        public object ReadIndexEntry(IContext context, ByteArrayBuffer reader)
        {
            return new IndexEntry(reader.ReadInt(), reader.ReadLong());
        }

        public void WriteIndexEntry(IContext context, ByteArrayBuffer writer, object obj)
        {
            var indexEntry = obj as IndexEntry;
            if (indexEntry == null)
            {
                indexEntry = new IndexEntry(ClassMetadataIdFor(context, obj), Convert.ToInt64(obj));
            }
            writer.WriteInt(indexEntry.ClassMetadataId);
            writer.WriteLong(indexEntry.EnumValue);
        }

        public void DefragIndexEntry(DefragmentContextImpl context)
        {
            context.IncrementOffset(Const4.LongLength);
        }

        public object IndexEntryToObject(IContext context, object indexEntry)
        {
            var entry = (IndexEntry) indexEntry;
            return ToEnum(context, entry.ClassMetadataId, entry.EnumValue);
        }

        public object ReadIndexEntryFromObjectSlot(MarshallerFamily mf, StatefulBuffer statefulBuffer)
        {
            return new IndexEntry(statefulBuffer.ReadInt(), statefulBuffer.ReadLong());
        }

        public object ReadIndexEntry(IObjectIdContext context)
        {
            return new IndexEntry(context.ReadInt(), context.ReadLong());
        }

        public bool DescendsIntoMembers()
        {
            return false;
        }

        public int LinkLength()
        {
            return Const4.IdLength + Const4.LongLength;
        }

        public void Delete(IDeleteContext context)
        {
            var offset = context.Offset() + Const4.IdLength + Const4.LongLength;
            context.Seek(offset);
        }

        public void Defragment(IDefragmentContext context)
        {
            context.CopyID();
            context.IncrementOffset(Const4.LongLength);
        }

        public object Read(IReadContext context)
        {
            var classId = context.ReadInt();
            var enumValue = context.ReadLong();

            return ToEnum(context, classId, enumValue);
        }

        public void Write(IWriteContext context, object obj)
        {
            var classId = ClassMetadataIdFor(context, obj);

            context.WriteInt(classId);
            context.WriteLong(Convert.ToInt64(obj));
        }

        private static int ClassMetadataIdFor(IContext context, object obj)
        {
            var claxx = Container(context).Reflector().ForObject(obj);
            var clazz = Container(context).ProduceClassMetadata(claxx);

            //TODO: Handle clazz == null!! Must not happen!

            return clazz.GetID();
        }

        private static ITypeHandler4 StringTypeHandler(IContext context)
        {
            return
                Container(context)
                    .Handlers.TypeHandlerForClass(Container(context).Ext().Reflector().ForClass(typeof (string)));
        }

        private static ObjectContainerBase Container(IContext context)
        {
            return ((IInternalObjectContainer) context.ObjectContainer()).Container;
        }

        private static object ToEnum(IContext context, int classId, long enumValue)
        {
            var clazz = Container(context).ClassMetadataForID(classId);

            var enumType = NetReflector.ToNative(clazz.ClassReflector());
            return Enum.ToObject(enumType, enumValue);
        }

        private class PreparedEnumComparison : IPreparedComparison
        {
            private readonly long _enumValue;

            public PreparedEnumComparison(object obj)
            {
                if (obj is TransactionContext)
                {
                    obj = ((TransactionContext) obj)._object;
                }

                if (obj == null) return;

                _enumValue = ToLong(obj);
            }

            public int CompareTo(object obj)
            {
                if (obj is TransactionContext)
                {
                    obj = ((TransactionContext) obj)._object;
                }

                if (obj == null) return 1;

                var other = ToLong(obj);
                if (_enumValue == other) return 0;
                if (_enumValue < other) return -1;

                return 1;
            }

            private static long ToLong(object obj)
            {
                if (obj is IndexEntry)
                {
                    return ((IndexEntry) obj).EnumValue;
                }

                return Convert.ToInt64(obj);
            }
        }

        private class IndexEntry
        {
            private readonly int _classMetadataId;
            private readonly long _enumValue;

            internal IndexEntry(int classMetadataId, long enumValue)
            {
                _classMetadataId = classMetadataId;
                _enumValue = enumValue;
            }

            internal long EnumValue
            {
                get { return _enumValue; }
            }

            internal int ClassMetadataId
            {
                get { return _classMetadataId; }
            }
        }
    }

    public class EnumTypeHandlerPredicate : ITypeHandlerPredicate
    {
        public bool Match(IReflectClass classReflector)
        {
            var type = NetReflector.ToNative(classReflector);
            if (type == null)
            {
                return false;
            }
            return type.IsEnum;
        }
    }
}