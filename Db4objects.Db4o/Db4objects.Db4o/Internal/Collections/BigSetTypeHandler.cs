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

using Db4objects.Db4o.Defragment;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal.Btree;
using Db4objects.Db4o.Internal.Delete;
using Db4objects.Db4o.Internal.Marshall;
using Db4objects.Db4o.Marshall;
using Db4objects.Db4o.Typehandlers;

namespace Db4objects.Db4o.Internal.Collections
{
    /// <exclude></exclude>
    public class BigSetTypeHandler : IReferenceTypeHandler, ICascadingTypeHandler
    {
        public virtual void CascadeActivation(IActivationContext context)
        {
        }

        // TODO Auto-generated method stub
        public virtual void CollectIDs(QueryingReadContext context)
        {
        }

        // TODO Auto-generated method stub
        public virtual ITypeHandler4 ReadCandidateHandler(QueryingReadContext context)
        {
            // TODO Auto-generated method stub
            return null;
        }

        public virtual void Defragment(IDefragmentContext context)
        {
            var pos = context.Offset();
            var id = context.ReadInt();
            var bTree = NewBTree(context, id);
            var services = (DefragmentServicesImpl) context.Services();
            var collector = new IDMappingCollector();
            services.RegisterBTreeIDs(bTree, collector);
            collector.Flush(services);
            context.Seek(pos);
            context.CopyID();
            bTree.DefragBTree(services);
        }

        /// <exception cref="Db4objects.Db4o.Ext.Db4oIOException"></exception>
        public virtual void Delete(IDeleteContext context)
        {
            InvalidBigSet(context);
            var id = context.ReadInt();
            FreeBTree(context, id);
        }

        public virtual void Write(IWriteContext context, object obj)
        {
            var bigSet = (IBigSetPersistence) obj;
            bigSet.Write(context);
        }

        public virtual void Activate(IReferenceActivationContext context)
        {
            var bigSet = (IBigSetPersistence) context.PersistentObject();
            bigSet.Read(context);
        }

        private void InvalidBigSet(IDeleteContext context)
        {
            var bigSet = (IBigSetPersistence) context.Transaction().ObjectForIdFromCache
                (context.ObjectId());
            if (bigSet != null)
            {
                bigSet.Invalidate();
            }
        }

        private void FreeBTree(IDeleteContext context, int id)
        {
            var bTree = NewBTree(context, id);
            bTree.Free(SystemTransaction(context));
            bTree = null;
        }

        private static LocalTransaction SystemTransaction(IContext context)
        {
            return (LocalTransaction) context.Transaction().SystemTransaction();
        }

        private BTree NewBTree(IContext context, int id)
        {
            return new BTree(SystemTransaction(context), id, new IDHandler());
        }

        public virtual IPreparedComparison PrepareComparison(IContext context, object obj
            )
        {
            // TODO Auto-generated method stub
            return null;
        }
    }
}