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
using Db4objects.Db4o.Marshall;

namespace Db4objects.Db4o.Internal.Btree
{
    /// <exclude></exclude>
    public class FieldIndexKeyHandler : IIndexable4
    {
        private readonly IDHandler _parentIdHandler;
        private readonly IIndexable4 _valueHandler;

        public FieldIndexKeyHandler(IIndexable4 delegate_)
        {
            _parentIdHandler = new IDHandler();
            _valueHandler = delegate_;
        }

        public virtual int LinkLength()
        {
            return _valueHandler.LinkLength() + Const4.IntLength;
        }

        public virtual object ReadIndexEntry(IContext context, ByteArrayBuffer a_reader)
        {
            // TODO: could read int directly here with a_reader.readInt()
            var parentID = ReadParentID(context, a_reader);
            var objPart = _valueHandler.ReadIndexEntry(context, a_reader);
            if (parentID < 0)
            {
                objPart = null;
                parentID = -parentID;
            }
            return new FieldIndexKeyImpl(parentID, objPart);
        }

        public virtual void WriteIndexEntry(IContext context, ByteArrayBuffer writer, object
            obj)
        {
            var composite = (IFieldIndexKey) obj;
            var parentID = composite.ParentID();
            var value = composite.Value();
            if (value == null)
            {
                parentID = -parentID;
            }
            _parentIdHandler.Write(parentID, writer);
            _valueHandler.WriteIndexEntry(context, writer, composite.Value());
        }

        public virtual void DefragIndexEntry(DefragmentContextImpl context)
        {
            _parentIdHandler.DefragIndexEntry(context);
            _valueHandler.DefragIndexEntry(context);
        }

        public virtual IPreparedComparison PrepareComparison(IContext context, object fieldIndexKey
            )
        {
            var source = (IFieldIndexKey) fieldIndexKey;
            var preparedValueComparison = _valueHandler.PrepareComparison(context
                , source.Value());
            var preparedParentIdComparison = _parentIdHandler.NewPrepareCompare
                (source.ParentID());
            return new _IPreparedComparison_67(preparedValueComparison, preparedParentIdComparison
                );
        }

        private int ReadParentID(IContext context, ByteArrayBuffer a_reader)
        {
            return ((int) _parentIdHandler.ReadIndexEntry(context, a_reader));
        }

        public virtual IIndexable4 ValueHandler()
        {
            return _valueHandler;
        }

        private sealed class _IPreparedComparison_67 : IPreparedComparison
        {
            private readonly IPreparedComparison preparedParentIdComparison;
            private readonly IPreparedComparison preparedValueComparison;

            public _IPreparedComparison_67(IPreparedComparison preparedValueComparison, IPreparedComparison
                preparedParentIdComparison)
            {
                this.preparedValueComparison = preparedValueComparison;
                this.preparedParentIdComparison = preparedParentIdComparison;
            }

            public int CompareTo(object obj)
            {
                var target = (IFieldIndexKey) obj;
                try
                {
                    var delegateResult = preparedValueComparison.CompareTo(target.Value());
                    if (delegateResult != 0)
                    {
                        return delegateResult;
                    }
                }
                catch (IllegalComparisonException)
                {
                }
                // can happen, is expected
                return preparedParentIdComparison.CompareTo(target.ParentID());
            }
        }
    }
}