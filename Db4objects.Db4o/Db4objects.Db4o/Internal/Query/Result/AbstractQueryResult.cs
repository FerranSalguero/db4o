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
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal.Query.Processor;
using Db4objects.Db4o.Query;

namespace Db4objects.Db4o.Internal.Query.Result
{
    /// <exclude></exclude>
    public abstract class AbstractQueryResult : IQueryResult
    {
        protected readonly Transaction _transaction;

        public AbstractQueryResult(Transaction transaction)
        {
            _transaction = transaction;
        }

        public virtual object Lock()
        {
            var stream = Stream();
            stream.CheckClosed();
            return stream.Lock();
        }

        public virtual IExtObjectContainer ObjectContainer()
        {
            return Transaction().ObjectContainer().Ext();
        }

        public virtual IEnumerator GetEnumerator()
        {
            return new _MappingIterator_56(this, IterateIDs());
        }

        public virtual int Size()
        {
            throw new NotImplementedException();
        }

        public virtual void Sort(IQueryComparator cmp)
        {
            throw new NotImplementedException();
        }

        public virtual void SortIds(IIntComparator cmp)
        {
            throw new NotImplementedException();
        }

        public virtual object Get(int index)
        {
            throw new NotImplementedException();
        }

        public virtual int IndexOf(int id)
        {
            throw new NotImplementedException();
        }

        public abstract IIntIterator4 IterateIDs();

        public object Activate(object obj)
        {
            Stream().Activate(_transaction, obj);
            return obj;
        }

        public object ActivatedObject(int id)
        {
            var stream = Stream();
            var ret = stream.GetActivatedObjectFromCache(_transaction, id);
            if (ret != null)
            {
                return ret;
            }
            return stream.ReadActivatedObjectNotInCache(_transaction, id);
        }

        public virtual ObjectContainerBase Stream()
        {
            return _transaction.Container();
        }

        public virtual Transaction Transaction()
        {
            return _transaction;
        }

        public virtual AbstractQueryResult SupportSize
            ()
        {
            return this;
        }

        public virtual AbstractQueryResult SupportSort
            ()
        {
            return this;
        }

        public virtual AbstractQueryResult SupportElementAccess
            ()
        {
            return this;
        }

        protected virtual int KnownSize()
        {
            return Size();
        }

        public virtual AbstractQueryResult ToIdList
            ()
        {
            var res = new IdListQueryResult(Transaction(), KnownSize());
            var i = IterateIDs();
            while (i.MoveNext())
            {
                res.Add(i.CurrentInt());
            }
            return res;
        }

        protected virtual AbstractQueryResult ToIdTree
            ()
        {
            return new IdTreeQueryResult(Transaction(), IterateIDs());
        }

        public virtual Config4Impl Config()
        {
            return Stream().Config();
        }

        /// <param name="i"></param>
        public virtual int GetId(int i)
        {
            throw new NotImplementedException();
        }

        /// <param name="c"></param>
        public virtual void LoadFromClassIndex(ClassMetadata c)
        {
            throw new NotImplementedException();
        }

        /// <param name="i"></param>
        public virtual void LoadFromClassIndexes(ClassMetadataIterator i)
        {
            throw new NotImplementedException();
        }

        /// <param name="ids"></param>
        public virtual void LoadFromIdReader(IEnumerator ids)
        {
            throw new NotImplementedException();
        }

        /// <param name="q"></param>
        public virtual void LoadFromQuery(QQuery q)
        {
            throw new NotImplementedException();
        }

        private sealed class _MappingIterator_56 : MappingIterator
        {
            private readonly AbstractQueryResult _enclosing;

            public _MappingIterator_56(AbstractQueryResult _enclosing, IEnumerator baseArg1) :
                base(baseArg1)
            {
                this._enclosing = _enclosing;
            }

            protected override object Map(object current)
            {
                if (current == null)
                {
                    return Iterators.Skip;
                }
                lock (_enclosing.Lock())
                {
                    var obj = _enclosing.ActivatedObject(((int) current));
                    if (obj == null)
                    {
                        return Iterators.Skip;
                    }
                    return obj;
                }
            }
        }
    }
}