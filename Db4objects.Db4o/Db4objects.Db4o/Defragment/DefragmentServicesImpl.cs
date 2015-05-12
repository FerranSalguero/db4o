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

using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Btree;
using Db4objects.Db4o.Internal.Classindex;
using Db4objects.Db4o.Internal.Encoding;
using Db4objects.Db4o.Internal.Ids;
using Db4objects.Db4o.Internal.Mapping;
using Db4objects.Db4o.Internal.Marshall;
using Db4objects.Db4o.Internal.Slots;
using Db4objects.Db4o.IO;
using Db4objects.Db4o.Typehandlers;

namespace Db4objects.Db4o.Defragment
{
    /// <exclude></exclude>
    public class DefragmentServicesImpl : IDefragmentServices
    {
        public static readonly DbSelector Sourcedb = new _DbSelector_39
            ();

        public static readonly DbSelector Targetdb = new _DbSelector_45
            ();

        private readonly Hashtable4 _classIndices = new Hashtable4(16);
        private readonly DefragmentConfig _defragConfig;
        private readonly IDefragmentListener _listener;
        private readonly IIdMapping _mapping;
        private readonly LocalObjectContainer _sourceDb;
        private readonly LocalObjectContainer _targetDb;
        private readonly IQueue4 _unindexed = new NonblockingQueue();

        /// <exception cref="System.IO.IOException"></exception>
        public DefragmentServicesImpl(DefragmentConfig defragConfig, IDefragmentListener
            listener)
        {
            _listener = listener;
            var originalConfig = (Config4Impl) defragConfig.Db4oConfig();
            var storage = defragConfig.BackupStorage();
            if (defragConfig.ReadOnly())
            {
                storage = new NonFlushingStorage(storage);
            }
            var sourceConfig = PrepareConfig(originalConfig, storage, defragConfig.ReadOnly
                ());
            _sourceDb = (LocalObjectContainer) Db4oFactory.OpenFile(sourceConfig, defragConfig
                .TempPath()).Ext();
            _sourceDb.ShowInternalClasses(true);
            defragConfig.Db4oConfig().BlockSize(_sourceDb.BlockSize());
            if (!originalConfig.GenerateCommitTimestamps().DefiniteNo())
            {
                defragConfig.Db4oConfig().GenerateCommitTimestamps(_sourceDb.Config().GenerateCommitTimestamps
                    ().DefiniteYes());
            }
            _targetDb = FreshTargetFile(defragConfig);
            _mapping = defragConfig.Mapping();
            _mapping.Open();
            _defragConfig = defragConfig;
        }

        /// <exception cref="Db4objects.Db4o.Internal.Mapping.MappingNotFoundException"></exception>
        public virtual int StrictMappedID(int oldID)
        {
            var mapped = InternalMappedID(oldID);
            if (mapped == 0)
            {
                throw new MappingNotFoundException(oldID);
            }
            return mapped;
        }

        public virtual int MappedID(int id)
        {
            if (id == 0)
            {
                return 0;
            }
            var mapped = InternalMappedID(id);
            if (mapped == 0)
            {
                _listener.NotifyDefragmentInfo(new DefragmentInfo("No mapping found for ID " + id
                    ));
                return Const4.InvalidObjectId;
            }
            return mapped;
        }

        public virtual void MapIDs(int oldID, int newID, bool isClassID)
        {
            _mapping.MapId(oldID, newID, isClassID);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual ByteArrayBuffer SourceBufferByAddress(int address, int length)
        {
            return BufferByAddress(Sourcedb, address, length);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual ByteArrayBuffer TargetBufferByAddress(int address, int length)
        {
            return BufferByAddress(Targetdb, address, length);
        }

        public virtual Slot AllocateTargetSlot(int length)
        {
            return _targetDb.AllocateSlot(length);
        }

        public virtual void TargetWriteBytes(DefragmentContextImpl context, int address)
        {
            context.Write(_targetDb, address);
        }

        public virtual void TargetWriteBytes(ByteArrayBuffer reader, int address)
        {
            _targetDb.WriteBytes(reader, address, 0);
        }

        public virtual void TraverseAllIndexSlots(BTree btree, IVisitor4 command)
        {
            var slotIDIter = btree.AllNodeIds(Sourcedb.Transaction(this));
            while (slotIDIter.MoveNext())
            {
                command.Visit(slotIDIter.Current);
            }
        }

        public virtual void RegisterBTreeIDs(BTree btree, IDMappingCollector collector)
        {
            collector.CreateIDMapping(this, btree.GetID(), false);
            TraverseAllIndexSlots(btree, new _IVisitor4_244(this, collector));
        }

        public virtual Transaction SystemTrans()
        {
            return Sourcedb.Transaction(this);
        }

        public virtual ByteArrayBuffer SourceBufferByID(int sourceID)
        {
            return BufferByID(Sourcedb, sourceID);
        }

        public virtual ClassMetadata ClassMetadataForId(int id)
        {
            return _sourceDb.ClassMetadataForID(id);
        }

        public virtual void RegisterUnindexed(int id)
        {
            _unindexed.Add(id);
        }

        public virtual IdSource UnindexedIDs()
        {
            return new IdSource(_unindexed);
        }

        public virtual int SourceAddressByID(int sourceID)
        {
            return CommittedSlot(Sourcedb, sourceID).Address();
        }

        public virtual int TargetAddressByID(int sourceID)
        {
            return _mapping.AddressForId(sourceID);
        }

        public virtual int TargetNewId()
        {
            return _targetDb.IdSystem().NewId();
        }

        public virtual IIdMapping Mapping()
        {
            return _mapping;
        }

        public virtual void CommitIds()
        {
            var freespaceCommitter = new FreespaceCommitter(_targetDb.FreespaceManager
                ());
            freespaceCommitter.TransactionalIdSystem(SystemTrans().IdSystem());
            _targetDb.IdSystem().Commit(Mapping().SlotChanges(), freespaceCommitter);
            freespaceCommitter.Commit();
        }

        private Config4Impl PrepareConfig(Config4Impl originalConfig, IStorage storage, bool
            readOnly)
        {
            var sourceConfig = (Config4Impl) originalConfig.DeepClone(null);
            sourceConfig.WeakReferences(false);
            sourceConfig.Storage = storage;
            sourceConfig.ReadOnly(readOnly);
            return sourceConfig;
        }

        /// <exception cref="System.IO.IOException"></exception>
        internal static LocalObjectContainer FreshTempFile(string fileName, int blockSize
            )
        {
            var storage = new FileStorage();
            storage.Delete(fileName);
            var db4oConfig = DefragmentConfig.VanillaDb4oConfig(blockSize);
            db4oConfig.ObjectClass(typeof (IdSlotMapping)).ObjectField("_id").Indexed(true);
            db4oConfig.Storage = storage;
            return (LocalObjectContainer) Db4oFactory.OpenFile(db4oConfig, fileName).Ext();
        }

        /// <exception cref="System.IO.IOException"></exception>
        internal static LocalObjectContainer FreshTargetFile(DefragmentConfig config)
        {
            config.Db4oConfig().Storage.Delete(config.OrigPath());
            return (LocalObjectContainer) Db4oFactory.OpenFile(config.ClonedDb4oConfig(), config
                .OrigPath());
        }

        public virtual int MappedID(int oldID, int defaultID)
        {
            var mapped = InternalMappedID(oldID);
            return (mapped != 0 ? mapped : defaultID);
        }

        /// <exception cref="Db4objects.Db4o.Internal.Mapping.MappingNotFoundException"></exception>
        private int InternalMappedID(int oldID)
        {
            if (oldID == 0)
            {
                return 0;
            }
            var mappedId = _mapping.MappedId(oldID);
            if (mappedId == 0 && _sourceDb.Handlers.IsSystemHandler(oldID))
            {
                return oldID;
            }
            return mappedId;
        }

        public virtual void Close()
        {
            _sourceDb.Close();
            _targetDb.Close();
            _mapping.Close();
        }

        public virtual ByteArrayBuffer BufferByID(DbSelector selector
            , int id)
        {
            var slot = CommittedSlot(selector, id);
            return BufferByAddress(selector, slot.Address(), slot.Length());
        }

        private Slot CommittedSlot(DbSelector selector, int id)
        {
            return selector.Db(this).IdSystem().CommittedSlot(id);
        }

        public virtual ByteArrayBuffer BufferByAddress(DbSelector
            selector, int address, int length)
        {
            return selector.Db(this).DecryptedBufferByAddress(address, length);
        }

        /// <exception cref="System.ArgumentException"></exception>
        public virtual StatefulBuffer TargetStatefulBufferByAddress(int address, int length
            )
        {
            return _targetDb.ReadWriterByAddress(Targetdb.Transaction(this), address, length);
        }

        public virtual IStoredClass[] StoredClasses(DbSelector selector
            )
        {
            var db = selector.Db(this);
            db.ShowInternalClasses(true);
            try
            {
                return db.ClassCollection().StoredClasses();
            }
            finally
            {
                db.ShowInternalClasses(false);
            }
        }

        public virtual LatinStringIO StringIO()
        {
            return _sourceDb.StringIO();
        }

        public virtual void TargetCommit()
        {
            _targetDb.Commit();
        }

        public virtual ITypeHandler4 SourceHandler(int id)
        {
            return _sourceDb.TypeHandlerForClassMetadataID(id);
        }

        public virtual int SourceClassCollectionID()
        {
            return _sourceDb.ClassCollection().GetID();
        }

        public virtual int ClassIndexID(ClassMetadata classMetadata)
        {
            return ClassIndex(classMetadata).Id();
        }

        public virtual void TraverseAll(ClassMetadata classMetadata, IVisitor4 command)
        {
            if (!classMetadata.HasClassIndex())
            {
                return;
            }
            classMetadata.Index().TraverseAll(Sourcedb.Transaction(this), command);
        }

        public virtual void TraverseAllIndexSlots(ClassMetadata classMetadata, IVisitor4
            command)
        {
            var slotIDIter = classMetadata.Index().AllSlotIDs(Sourcedb.Transaction(this
                ));
            while (slotIDIter.MoveNext())
            {
                command.Visit(slotIDIter.Current);
            }
        }

        public virtual int DatabaseIdentityID(DbSelector selector)
        {
            var db = selector.Db(this);
            var identity = db.Identity();
            if (identity == null)
            {
                return 0;
            }
            return identity.GetID(selector.Transaction(this));
        }

        private IClassIndexStrategy ClassIndex(ClassMetadata classMetadata)
        {
            var classIndex = (IClassIndexStrategy) _classIndices.Get(classMetadata
                );
            if (classIndex == null)
            {
                classIndex = new BTreeClassIndexStrategy(classMetadata);
                _classIndices.Put(classMetadata, classIndex);
                classIndex.Initialize(_targetDb);
            }
            return classIndex;
        }

        public virtual void CopyIdentity()
        {
            _targetDb.SetIdentity(_sourceDb.Identity());
        }

        public virtual void ReplaceClassMetadataRepository()
        {
            var systemTransaction = _targetDb.SystemTransaction
                ();
            // Can't use strictMappedID because the repository ID can
            // be lower than HandlerRegisrtry _highestBuiltinTypeID and
            // the ClassRepository ID would be treated as a system handler
            // and the unmapped ID would be returned.
            var newRepositoryId = _mapping.MappedId(SourceClassCollectionID());
            var sourceIdentityID = DatabaseIdentityID(Sourcedb);
            var targetIdentityID = _mapping.MappedId(sourceIdentityID);
            var targetUuidIndexID = _mapping.MappedId(SourceUuidIndexID());
            var oldIdentityId = _targetDb.SystemData().Identity().GetID(systemTransaction);
            var oldRepositoryId = _targetDb.ClassCollection().GetID();
            var oldRepository = _targetDb.ClassCollection();
            var newRepository = new ClassMetadataRepository(systemTransaction
                );
            newRepository.SetID(newRepositoryId);
            newRepository.Read(systemTransaction);
            newRepository.InitOnUp(systemTransaction);
            _targetDb.SystemData().ClassCollectionID(newRepositoryId);
            _targetDb.ReplaceClassMetadataRepository(newRepository);
            _targetDb.SystemData().UuidIndexId(targetUuidIndexID);
            var identity = (Db4oDatabase) _targetDb.GetByID(systemTransaction, targetIdentityID
                );
            _targetDb.SetIdentity(identity);
            var iterator = oldRepository.Iterator();
            while (iterator.MoveNext())
            {
                var classMetadata = iterator.CurrentClass();
                var index = (BTreeClassIndexStrategy) classMetadata.Index();
                index.Btree().Free(_targetDb.LocalSystemTransaction());
                FreeById(classMetadata.GetID());
            }
            FreeById(oldIdentityId);
            FreeById(oldRepositoryId);
        }

        public virtual void DefragIdToTimestampBtree()
        {
            if (_sourceDb.SystemData().IdToTimestampIndexId() == 0)
            {
                return;
            }
            var targetTransaction = (LocalTransaction) _targetDb.SystemTransaction
                ();
            var sourceTransaction = (LocalTransaction) _sourceDb.SystemTransaction
                ();
            var target = targetTransaction.CommitTimestampSupport();
            var source = sourceTransaction.CommitTimestampSupport();
            if (source.IdToTimestamp() == null)
            {
                return;
            }
            source.IdToTimestamp().TraverseKeys(sourceTransaction, new _IVisitor4_336(this, target
                , targetTransaction));
        }

        private void FreeById(int id)
        {
            _targetDb.SystemTransaction().IdSystem().NotifySlotDeleted(id, SlotChangeFactory.
                SystemObjects);
        }

        public virtual BTree SourceUuidIndex()
        {
            if (SourceUuidIndexID() == 0)
            {
                return null;
            }
            return _sourceDb.UUIDIndex().GetIndex(SystemTrans());
        }

        public virtual void TargetUuidIndexID(int id)
        {
            _targetDb.SystemData().UuidIndexId(id);
        }

        public virtual int SourceUuidIndexID()
        {
            return _sourceDb.SystemData().UuidIndexId();
        }

        public virtual int SourceIdToTimestampIndexID()
        {
            return _sourceDb.SystemData().IdToTimestampIndexId();
        }

        public virtual ObjectHeader SourceObjectHeader(ByteArrayBuffer buffer)
        {
            return new ObjectHeader(_sourceDb, buffer);
        }

        public virtual int BlockSize()
        {
            return _sourceDb.BlockSize();
        }

        public virtual bool Accept(IStoredClass klass)
        {
            return _defragConfig.StoredClassFilter().Accept(klass);
        }

        public abstract class DbSelector
        {
            internal DbSelector()
            {
            }

            internal abstract LocalObjectContainer Db(DefragmentServicesImpl context);

            internal virtual Transaction Transaction(DefragmentServicesImpl
                context)
            {
                return Db(context).SystemTransaction();
            }
        }

        private sealed class _DbSelector_39 : DbSelector
        {
            internal override LocalObjectContainer Db(DefragmentServicesImpl context)
            {
                return context._sourceDb;
            }
        }

        private sealed class _DbSelector_45 : DbSelector
        {
            internal override LocalObjectContainer Db(DefragmentServicesImpl context)
            {
                return context._targetDb;
            }
        }

        private sealed class _IVisitor4_244 : IVisitor4
        {
            private readonly DefragmentServicesImpl _enclosing;
            private readonly IDMappingCollector collector;

            public _IVisitor4_244(DefragmentServicesImpl _enclosing, IDMappingCollector collector
                )
            {
                this._enclosing = _enclosing;
                this.collector = collector;
            }

            public void Visit(object obj)
            {
                var id = ((int) obj);
                collector.CreateIDMapping(_enclosing, id, false);
            }
        }

        private sealed class _IVisitor4_336 : IVisitor4
        {
            private readonly DefragmentServicesImpl _enclosing;
            private readonly CommitTimestampSupport target;
            private readonly LocalTransaction targetTransaction;

            public _IVisitor4_336(DefragmentServicesImpl _enclosing, CommitTimestampSupport target
                , LocalTransaction targetTransaction)
            {
                this._enclosing = _enclosing;
                this.target = target;
                this.targetTransaction = targetTransaction;
            }

            public void Visit(object te)
            {
                var mappedID = _enclosing.MappedID(((CommitTimestampSupport.TimestampEntry) te
                    ).ParentID());
                target.Put(targetTransaction, mappedID, ((CommitTimestampSupport.TimestampEntry) te
                    ).GetCommitTimestamp());
            }
        }
    }
}