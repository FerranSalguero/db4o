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
using System.Collections.Generic;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal.Activation;
using Db4objects.Db4o.Internal.Btree;
using Db4objects.Db4o.Marshall;

namespace Db4objects.Db4o.Internal.Collections
{
    /// <exclude></exclude>
    public partial class BigSet<E> : Db4o.Collections.ISet<E>, IBigSetPersistence
    {
        private BTree _bTree;
        private Transaction _transaction;

        public BigSet(LocalObjectContainer db)
        {
            if (db == null)
            {
                return;
            }
            _transaction = db.Transaction;
            _bTree = BTreeManager().NewBTree();
        }

        public virtual void Write(IWriteContext context)
        {
            var id = BTree().GetID();
            if (id == 0)
            {
                BTree().Write(SystemTransaction());
            }
            context.WriteInt(BTree().GetID());
        }

        public virtual void Read(IReadContext context)
        {
            var id = context.ReadInt();
            if (_bTree != null)
            {
                AssertCurrentBTreeId(id);
                return;
            }
            _transaction = context.Transaction();
            _bTree = BTreeManager().ProduceBTree(id);
        }

        public virtual void Invalidate()
        {
            _bTree = null;
        }

        public virtual bool AddAll(IEnumerable<E> iterable)
        {
            var result = false;
            foreach (var element in iterable)
            {
                if (Add(element))
                {
                    result = true;
                }
            }
            return result;
        }

        public virtual void Clear()
        {
            lock (Lock())
            {
                BTreeForUpdate().Clear(Transaction());
            }
        }

        public virtual bool IsEmpty
        {
            get { return Count == 0; }
        }

        public virtual int Count
        {
            get
            {
                lock (Lock())
                {
                    return BTree().Size(Transaction());
                }
            }
        }

        private ObjectContainerBase Container()
        {
            return Transaction().Container();
        }

        public virtual bool Add(E obj)
        {
            lock (Lock())
            {
                var id = GetID(obj);
                if (id == 0)
                {
                    Add(Store(obj));
                    return true;
                }
                if (Contains(id))
                {
                    return false;
                }
                Add(id);
                return true;
            }
        }

        private int Store(E obj)
        {
            return Container().Store(_transaction, obj, Container().UpdateDepthProvider().Unspecified
                (NullModifiedObjectQuery.Instance));
        }

        private void Add(int id)
        {
            BTreeForUpdate().Add(_transaction, id);
        }

        private int GetID(object obj)
        {
            return (int) Container().GetID(obj);
        }

        public virtual bool Contains(object obj)
        {
            var id = GetID(obj);
            if (id == 0)
            {
                return false;
            }
            return Contains(id);
        }

        private bool Contains(int id)
        {
            lock (Lock())
            {
                var range = BTree().SearchRange(Transaction(), id);
                return !range.IsEmpty();
            }
        }

        private IEnumerator BTreeIterator()
        {
            return new SynchronizedIterator4(BTree().Iterator(Transaction()), Lock());
        }

        public virtual bool Remove(object obj)
        {
            lock (Lock())
            {
                if (!Contains(obj))
                {
                    return false;
                }
                var id = GetID(obj);
                BTreeForUpdate().Remove(Transaction(), id);
                return true;
            }
        }

        public virtual object[] ToArray()
        {
            throw new NotSupportedException();
        }

        public virtual T[] ToArray<T>(T[] a)
        {
            throw new NotSupportedException();
        }

        private BigSetBTreeManager BTreeManager()
        {
            return new BigSetBTreeManager(_transaction);
        }

        private void AssertCurrentBTreeId(int id)
        {
            if (id != _bTree.GetID())
            {
                throw new InvalidOperationException();
            }
        }

        private Transaction Transaction()
        {
            return _transaction;
        }

        private Transaction SystemTransaction()
        {
            return Container().SystemTransaction();
        }

        private BTree BTree()
        {
            if (_bTree == null)
            {
                throw new InvalidOperationException();
            }
            return _bTree;
        }

        private BTree BTreeForUpdate()
        {
            var bTree = BTree();
            BTreeManager().EnsureIsManaged(bTree);
            return bTree;
        }

        private object Element(int id)
        {
            var obj = Container().GetByID(Transaction(), id);
            Container().Activate(obj);
            return obj;
        }

        private object Lock()
        {
            return Container().Lock();
        }
    }
}