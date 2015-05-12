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
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal.Activation;
using Db4objects.Db4o.Internal.Btree;
using Db4objects.Db4o.Internal.Delete;
using Db4objects.Db4o.Internal.Handlers;
using Db4objects.Db4o.Internal.Marshall;
using Db4objects.Db4o.Internal.Slots;
using Db4objects.Db4o.Marshall;

namespace Db4objects.Db4o.Internal
{
    /// <exclude></exclude>
    public class UUIDFieldMetadata : VirtualFieldMetadata
    {
        internal UUIDFieldMetadata() : base(Handlers4.LongId, new LongHandler())
        {
            SetName(Const4.VirtualFieldPrefix + "uuid");
        }

        /// <exception cref="Db4objects.Db4o.Internal.FieldIndexException"></exception>
        public override void AddFieldIndex(ObjectIdContextImpl context)
        {
            var transaction = (LocalTransaction) context.Transaction();
            var localContainer = (LocalObjectContainer) transaction.Container
                ();
            var oldSlot = transaction.IdSystem().CommittedSlot(context.ObjectId());
            var savedOffset = context.Offset();
            var db4oDatabaseIdentityID = context.ReadInt();
            var uuid = context.ReadLong();
            context.Seek(savedOffset);
            var isnew = (oldSlot.IsNull());
            if ((uuid == 0 || db4oDatabaseIdentityID == 0) && context.ObjectId() > 0 && !isnew)
            {
                var identityAndUUID = ReadDatabaseIdentityIDAndUUID
                    (localContainer, context.ClassMetadata(), oldSlot, false);
                db4oDatabaseIdentityID = identityAndUUID.databaseIdentityID;
                uuid = identityAndUUID.uuid;
            }
            if (db4oDatabaseIdentityID == 0)
            {
                db4oDatabaseIdentityID = localContainer.Identity().GetID(transaction);
            }
            if (uuid == 0)
            {
                uuid = localContainer.GenerateTimeStampId();
            }
            var writer = (StatefulBuffer) context.Buffer();
            writer.WriteInt(db4oDatabaseIdentityID);
            writer.WriteLong(uuid);
            if (isnew)
            {
                AddIndexEntry(writer, uuid);
            }
        }

        /// <exception cref="Db4objects.Db4o.Ext.Db4oIOException"></exception>
        private DatabaseIdentityIDAndUUID ReadDatabaseIdentityIDAndUUID
            (ObjectContainerBase container, ClassMetadata classMetadata, Slot oldSlot, bool
                checkClass)
        {
            if (DTrace.enabled)
            {
                DTrace.RereadOldUuid.LogLength(oldSlot.Address(), oldSlot.Length());
            }
            var reader = container.DecryptedBufferByAddress(oldSlot.Address(), oldSlot
                .Length());
            if (checkClass)
            {
                var realClass = ClassMetadata.ReadClass(container, reader);
                if (realClass != classMetadata)
                {
                    return null;
                }
            }
            if (classMetadata.SeekToField(container.Transaction, reader, this) == HandlerVersion
                .Invalid)
            {
                return null;
            }
            return new DatabaseIdentityIDAndUUID(reader.ReadInt(), reader.ReadLong
                ());
        }

        public override void Delete(DeleteContextImpl context, bool isUpdate)
        {
            if (isUpdate)
            {
                context.Seek(context.Offset() + LinkLength(context));
                return;
            }
            context.Seek(context.Offset() + Const4.IntLength);
            var longPart = context.ReadLong();
            if (longPart > 0)
            {
                if (context.Container().MaintainsIndices())
                {
                    RemoveIndexEntry(context.Transaction(), context.ObjectId(), longPart);
                }
            }
        }

        public override bool HasIndex()
        {
            return true;
        }

        public override BTree GetIndex(Transaction transaction)
        {
            EnsureIndex(transaction);
            return base.GetIndex(transaction);
        }

        /// <exception cref="Db4objects.Db4o.Internal.FieldIndexException"></exception>
        protected override void RebuildIndexForObject(LocalObjectContainer container, ClassMetadata
            classMetadata, int objectId)
        {
            var slot = container.SystemTransaction().IdSystem().CurrentSlot(objectId);
            var data = ReadDatabaseIdentityIDAndUUID(
                container, classMetadata, slot, true);
            if (null == data)
            {
                return;
            }
            AddIndexEntry(container.LocalSystemTransaction(), objectId, data.uuid);
        }

        private void EnsureIndex(Transaction transaction)
        {
            if (null == transaction)
            {
                throw new ArgumentNullException();
            }
            if (null != base.GetIndex(transaction))
            {
                return;
            }
            var file = ((LocalObjectContainer) transaction.Container());
            var sd = file.SystemData();
            if (sd == null)
            {
                // too early, in new file, try again later.
                return;
            }
            InitIndex(transaction, sd.UuidIndexId());
            if (sd.UuidIndexId() == 0)
            {
                sd.UuidIndexId(base.GetIndex(transaction).GetID());
                file.GetFileHeader().WriteVariablePart(file);
            }
        }

        internal override void Instantiate1(ObjectReferenceContext context)
        {
            var dbID = context.ReadInt();
            var trans = context.Transaction();
            var container = trans.Container();
            container.ShowInternalClasses(true);
            try
            {
                var db = (Db4oDatabase) container.GetByID2(trans, dbID);
                if (db != null && db.i_signature == null)
                {
                    container.Activate(trans, db, new FixedActivationDepth(2));
                }
                var va = context.ObjectReference().VirtualAttributes();
                va.i_database = db;
                va.i_uuid = context.ReadLong();
            }
            finally
            {
                container.ShowInternalClasses(false);
            }
        }

        public override int LinkLength(IHandlerVersionContext context)
        {
            return Const4.LongLength + Const4.IdLength;
        }

        internal override void Marshall(Transaction trans, ObjectReference @ref, IWriteBuffer
            buffer, bool isMigrating, bool isNew)
        {
            var attr = @ref.VirtualAttributes();
            var container = trans.Container();
            var doAddIndexEntry = isNew && container.MaintainsIndices();
            var dbID = 0;
            var linkToDatabase = (attr != null && attr.i_database == null) ? true : !isMigrating;
            if (linkToDatabase)
            {
                var db = ((IInternalObjectContainer) container).Identity();
                if (db == null)
                {
                    // can happen on early classes like Metaxxx, no problem
                    attr = null;
                }
                else
                {
                    if (attr.i_database == null)
                    {
                        attr.i_database = db;
                        // TODO: Should be check for ! client instead of instanceof
                        if (container is LocalObjectContainer)
                        {
                            attr.i_uuid = container.GenerateTimeStampId();
                            doAddIndexEntry = true;
                        }
                    }
                    db = attr.i_database;
                    if (db != null)
                    {
                        dbID = db.GetID(trans);
                    }
                }
            }
            else
            {
                if (attr != null)
                {
                    dbID = attr.i_database.GetID(trans);
                }
            }
            buffer.WriteInt(dbID);
            if (attr == null)
            {
                buffer.WriteLong(0);
                return;
            }
            buffer.WriteLong(attr.i_uuid);
            if (doAddIndexEntry)
            {
                AddIndexEntry(trans, @ref.GetID(), attr.i_uuid);
            }
        }

        internal override void MarshallIgnore(IWriteBuffer buffer)
        {
            buffer.WriteInt(0);
            buffer.WriteLong(0);
        }

        public HardObjectReference GetHardObjectReferenceBySignature(Transaction transaction
            , long longPart, byte[] signature)
        {
            var range = Search(transaction, longPart);
            var keys = range.Keys();
            while (keys.MoveNext())
            {
                var current = (IFieldIndexKey) keys.Current;
                var hardRef = GetHardObjectReferenceById(transaction, current.ParentID
                    (), signature);
                if (null != hardRef)
                {
                    return hardRef;
                }
            }
            return HardObjectReference.Invalid;
        }

        protected HardObjectReference GetHardObjectReferenceById(Transaction transaction,
            int parentId, byte[] signature)
        {
            var hardRef = transaction.Container().GetHardObjectReferenceById(
                transaction, parentId);
            if (hardRef._reference == null)
            {
                return null;
            }
            var vad = hardRef._reference.VirtualAttributes(transaction, false);
            if (!Arrays4.Equals(signature, vad.i_database.i_signature))
            {
                return null;
            }
            return hardRef;
        }

        public override void DefragAspect(IDefragmentContext context)
        {
            // database id
            context.CopyID();
            // uuid
            context.IncrementOffset(Const4.LongLength);
        }

        internal class DatabaseIdentityIDAndUUID
        {
            public int databaseIdentityID;
            public long uuid;

            public DatabaseIdentityIDAndUUID(int databaseIdentityID_, long uuid_)
            {
                databaseIdentityID = databaseIdentityID_;
                uuid = uuid_;
            }
        }
    }
}