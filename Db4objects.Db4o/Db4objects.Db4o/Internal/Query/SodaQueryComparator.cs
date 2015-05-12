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
using System.Collections;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal.Handlers;
using Db4objects.Db4o.Internal.Marshall;

namespace Db4objects.Db4o.Internal.Query
{
    public class SodaQueryComparator : IComparer, IIntComparator
    {
        private readonly IDictionary _bufferCache = new Hashtable();
        private readonly LocalObjectContainer _container;
        private readonly ClassMetadata _extentType;
        private readonly IDictionary _fieldValueCache = new Hashtable();
        private readonly Ordering[] _orderings;
        private readonly LocalTransaction _transaction;

        public SodaQueryComparator(LocalObjectContainer container, Type extentType, Ordering
            [] orderings) : this(container, container.ProduceClassMetadata(container.Reflector
                ().ForClass(extentType)), orderings)
        {
        }

        public SodaQueryComparator(LocalObjectContainer container, ClassMetadata extent,
            Ordering[] orderings)
        {
            _container = container;
            _transaction = ((LocalTransaction) _container.Transaction);
            _extentType = extent;
            _orderings = orderings;
            ResolveFieldPaths(orderings);
        }

        public virtual int Compare(object x, object y)
        {
            return Compare(((int) x), ((int) y));
        }

        public virtual int Compare(int x, int y)
        {
            for (var orderingIndex = 0; orderingIndex < _orderings.Length; ++orderingIndex)
            {
                var ordering = _orderings[orderingIndex];
                var resolvedPath = ordering._resolvedPath;
                if (resolvedPath.Count == 0)
                {
                    continue;
                }
                var result = CompareByField(x, y, resolvedPath);
                if (result != 0)
                {
                    return ordering.Direction().Equals(Direction.Ascending)
                        ? result
                        : -result;
                }
            }
            return 0;
        }

        private void ResolveFieldPaths(Ordering[] orderings)
        {
            for (var fieldPathIndex = 0; fieldPathIndex < orderings.Length; ++fieldPathIndex)
            {
                var fieldPath = orderings[fieldPathIndex];
                fieldPath._resolvedPath = ResolveFieldPath(fieldPath.FieldPath());
            }
        }

        public virtual IList Sort(long[] ids)
        {
            var idList = ListFrom(ids);
            idList.Sort(this);
            return idList;
        }

        private ArrayList ListFrom(long[] ids)
        {
            var idList = new ArrayList(ids.Length);
            for (var idIndex = 0; idIndex < ids.Length; ++idIndex)
            {
                var id = ids[idIndex];
                idList.Add((int) id);
            }
            return idList;
        }

        private IList ResolveFieldPath(string[] fieldPath)
        {
            IList fields = new ArrayList(fieldPath.Length);
            var currentType = _extentType;
            for (var fieldNameIndex = 0; fieldNameIndex < fieldPath.Length; ++fieldNameIndex)
            {
                var fieldName = fieldPath[fieldNameIndex];
                var field = currentType.FieldMetadataForName(fieldName);
                if (field == null)
                {
                    fields.Clear();
                    break;
                }
                currentType = field.FieldType();
                fields.Add(field);
            }
            return fields;
        }

        private int CompareByField(int x, int y, IList path)
        {
            var xFieldValue = GetFieldValue(x, path);
            var yFieldValue = GetFieldValue(y, path);
            var field = ((FieldMetadata) path[path.Count - 1]);
            return field.PrepareComparison(_transaction.Context(), xFieldValue).CompareTo(yFieldValue
                );
        }

        private object GetFieldValue(int id, IList path)
        {
            for (var i = 0; i < path.Count - 1; ++i)
            {
                var obj = GetFieldValue(id, ((FieldMetadata) path[i]));
                if (null == obj)
                {
                    return null;
                }
                id = _container.GetID(_transaction, obj);
            }
            return GetFieldValue(id, ((FieldMetadata) path[path.Count - 1]));
        }

        private object GetFieldValue(int id, FieldMetadata field)
        {
            var key = new FieldValueKey(id,
                field);
            var cachedValue = _fieldValueCache[key];
            if (null != cachedValue)
            {
                return cachedValue;
            }
            var fieldValue = ReadFieldValue(id, field);
            _fieldValueCache[key] = fieldValue;
            return fieldValue;
        }

        private object ReadFieldValue(int id, FieldMetadata field)
        {
            var buffer = BufferFor(id);
            var handlerVersion = field.ContainingClass().SeekToField(_transaction,
                buffer, field);
            if (handlerVersion == HandlerVersion.Invalid)
            {
                return null;
            }
            var context = new QueryingReadContext(_transaction, handlerVersion
                ._number, buffer, id);
            return field.Read(context);
        }

        private ByteArrayBuffer BufferFor(int id)
        {
            var cachedBuffer = ((ByteArrayBuffer) _bufferCache[id]);
            if (null != cachedBuffer)
            {
                return cachedBuffer;
            }
            var buffer = _container.ReadBufferById(_transaction, id);
            _bufferCache[id] = buffer;
            return buffer;
        }

        public class Ordering
        {
            private readonly Direction _direction;
            private readonly string[] _fieldPath;

            [NonSerialized] internal IList _resolvedPath;

            public Ordering(Direction direction, string[] fieldPath)
            {
                _direction = direction;
                _fieldPath = fieldPath;
            }

            public virtual Direction Direction()
            {
                return _direction;
            }

            public virtual string[] FieldPath()
            {
                return _fieldPath;
            }
        }

        public class Direction
        {
            public static readonly Direction Ascending = new Direction
                (0);

            public static readonly Direction Descending = new Direction
                (1);

            private readonly int value;

            private Direction()
            {
            }

            private Direction(int value)
            {
                this.value = value;
            }

            public override bool Equals(object obj)
            {
                return ((Direction) obj).value == value;
            }

            public override string ToString()
            {
                return Equals(Ascending) ? "ASCENDING" : "DESCENDING";
            }
        }

        internal class FieldValueKey
        {
            private readonly FieldMetadata _field;
            private readonly int _id;

            public FieldValueKey(int id, FieldMetadata field)
            {
                _id = id;
                _field = field;
            }

            public override int GetHashCode()
            {
                return _field.GetHashCode() ^ _id;
            }

            public override bool Equals(object obj)
            {
                var other = (FieldValueKey) obj;
                return _field == other._field && _id == other._id;
            }
        }
    }
}