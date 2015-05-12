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
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal.Btree;
using Db4objects.Db4o.Internal.Convert;
using Db4objects.Db4o.Internal.Events;
using Db4objects.Db4o.Internal.Fileheader;
using Db4objects.Db4o.Internal.Freespace;
using Db4objects.Db4o.Internal.Ids;
using Db4objects.Db4o.Internal.Qlin;
using Db4objects.Db4o.Internal.Query.Processor;
using Db4objects.Db4o.Internal.Query.Result;
using Db4objects.Db4o.Internal.References;
using Db4objects.Db4o.Internal.Slots;
using Db4objects.Db4o.Qlin;
using Sharpen;
using Sharpen.Lang;

namespace Db4objects.Db4o.Internal
{
    /// <exclude></exclude>
    public abstract class LocalObjectContainer : ExternalObjectContainer, IInternalObjectContainer
        , IEmbeddedObjectContainer
    {
        private readonly Collection4 _dirtyClassMetadata = new Collection4();
        private readonly byte[] _pointerBuffer = new byte[Const4.PointerLength];

        protected readonly ByteArrayBuffer _pointerIo = new ByteArrayBuffer(Const4.PointerLength
            );

        private readonly Lock4 _semaphoresLock = new Lock4();
        private int _blockEndAddress;
        protected FileHeader _fileHeader;
        private IFreespaceManager _freespaceManager;
        private IIdSystem _idSystem;
        private Hashtable4 _semaphores;
        private SystemData _systemData;
        private bool i_isServer;

        internal LocalObjectContainer(IConfiguration config) : base(config)
        {
        }

        public override Db4oDatabase Identity()
        {
            return _systemData.Identity();
        }

        public override sealed void ReleaseSemaphore(string name)
        {
            ReleaseSemaphore(null, name);
        }

        public override sealed bool SetSemaphore(string name, int timeout)
        {
            return SetSemaphore(null, name, timeout);
        }

        public override ISystemInfo SystemInfo()
        {
            return new SystemInfoFileImpl(this);
        }

        public override int InstanceCount(ClassMetadata clazz, Transaction trans)
        {
            lock (Lock())
            {
                return clazz.IndexEntryCount(trans);
            }
        }

        public override IObjectContainer OpenSession()
        {
            lock (Lock())
            {
                return new ObjectContainerSession(this);
            }
        }

        public override EventRegistryImpl NewEventRegistry()
        {
            return new EventRegistryImpl();
        }

        public override Transaction NewTransaction(Transaction parentTransaction, IReferenceSystem
            referenceSystem, bool isSystemTransaction)
        {
            ITransactionalIdSystem systemIdSystem = null;
            if (!isSystemTransaction)
            {
                systemIdSystem = SystemTransaction().IdSystem();
            }
            IClosure4 idSystem = new _IClosure4_58(this);
            var transactionalIdSystem = NewTransactionalIdSystem(systemIdSystem
                , idSystem);
            return new LocalTransaction(this, parentTransaction, transactionalIdSystem, referenceSystem
                );
        }

        public virtual ITransactionalIdSystem NewTransactionalIdSystem(ITransactionalIdSystem
            systemIdSystem, IClosure4 idSystem)
        {
            return new TransactionalIdSystemImpl(new _IClosure4_69(this), idSystem, (TransactionalIdSystemImpl
                ) systemIdSystem);
        }

        public virtual IFreespaceManager FreespaceManager()
        {
            return _freespaceManager;
        }

        public virtual void BlockSizeReadFromFile(int size)
        {
            BlockSize(size);
            SetRegularEndAddress(FileLength());
        }

        public virtual void SetRegularEndAddress(long address)
        {
            _blockEndAddress = _blockConverter.BytesToBlocks(address);
        }

        protected override sealed void Close2()
        {
            try
            {
                if (!_config.IsReadOnly())
                {
                    CommitTransaction();
                    Shutdown();
                }
            }
            finally
            {
                ShutdownObjectContainer();
            }
        }

        public override void Commit1(Transaction trans)
        {
            trans.Commit();
        }

        internal virtual void ConfigureNewFile()
        {
            BlockSize(ConfigImpl.BlockSize());
            _fileHeader = FileHeader.NewCurrentFileHeader();
            SetRegularEndAddress(_fileHeader.Length());
            NewSystemData(ConfigImpl.FreespaceSystem(), ConfigImpl.IdSystemType());
            SystemData().ConverterVersion(Converter.Version);
            CreateStringIO(_systemData.StringEncoding());
            CreateIdSystem();
            InitializeClassMetadataRepository();
            InitalizeWeakReferenceSupport();
            GenerateNewIdentity();
            var blockedFreespaceManager = AbstractFreespaceManager.CreateNew
                (this);
            InstallFreespaceManager(blockedFreespaceManager);
            InitNewClassCollection();
            InitializeEssentialClasses();
            _fileHeader.InitNew(this);
            blockedFreespaceManager.Start(0);
        }

        private void NewSystemData(byte freespaceSystemType, byte idSystemType)
        {
            _systemData = new SystemData();
            _systemData.StringEncoding(ConfigImpl.Encoding());
            _systemData.FreespaceSystem(freespaceSystemType);
            _systemData.IdSystemType(idSystemType);
        }

        public override int ConverterVersion()
        {
            return _systemData.ConverterVersion();
        }

        public override long CurrentVersion()
        {
            return _timeStampIdGenerator.Last();
        }

        internal virtual void InitNewClassCollection()
        {
            // overridden in YapObjectCarrier to do nothing
            ClassCollection().InitTables(1);
        }

        public BTree CreateBTreeClassIndex(int id)
        {
            return new BTree(_transaction, id, new IDHandler());
        }

        public AbstractQueryResult NewQueryResult(Transaction trans)
        {
            return NewQueryResult(trans, Config().EvaluationMode());
        }

        public override sealed AbstractQueryResult NewQueryResult(Transaction trans, QueryEvaluationMode
            mode)
        {
            if (trans == null)
            {
                throw new ArgumentNullException();
            }
            if (mode == QueryEvaluationMode.Immediate)
            {
                return new IdListQueryResult(trans);
            }
            return new HybridQueryResult(trans, mode);
        }

        public override sealed bool Delete4(Transaction transaction, ObjectReference @ref
            , object obj, int cascade, bool userCall)
        {
            var id = @ref.GetID();
            var reader = ReadStatefulBufferById(transaction, id);
            if (reader != null)
            {
                if (obj != null)
                {
                    if ((!ShowInternalClasses()) && Const4.ClassInternal.IsAssignableFrom(obj.GetType
                        ()))
                    {
                        return false;
                    }
                }
                reader.SetCascadeDeletes(cascade);
                transaction.IdSystem().NotifySlotDeleted(id, SlotChangeFactory.UserObjects);
                var classMetadata = @ref.ClassMetadata();
                classMetadata.Delete(reader, obj);
                return true;
            }
            return false;
        }

        public abstract long FileLength();
        public abstract string FileName();

        public virtual void Free(Slot slot)
        {
            if (slot.IsNull())
            {
                return;
            }
            // TODO: This should really be an IllegalArgumentException but old database files 
            //       with index-based FreespaceManagers appear to deliver zeroed slots.
            // throw new IllegalArgumentException();
            if (_freespaceManager == null)
            {
                // Can happen on early free before freespacemanager
                // is up, during conversion.
                return;
            }
            if (DTrace.enabled)
            {
                DTrace.FileFree.LogLength(slot.Address(), slot.Length());
            }
            _freespaceManager.Free(slot);
        }

        public virtual void Free(int address, int a_length)
        {
            Free(new Slot(address, a_length));
        }

        public virtual void GenerateNewIdentity()
        {
            lock (_lock)
            {
                SetIdentity(Db4oDatabase.Generate());
            }
        }

        public override AbstractQueryResult QueryAllObjects(Transaction trans)
        {
            return GetAll(trans, Config().EvaluationMode());
        }

        public virtual AbstractQueryResult GetAll(Transaction trans, QueryEvaluationMode
            mode)
        {
            var queryResult = NewQueryResult(trans, mode);
            queryResult.LoadFromClassIndexes(ClassCollection().Iterator());
            return queryResult;
        }

        public virtual int AllocatePointerSlot()
        {
            var id = AllocateSlot(Const4.PointerLength).Address();
            if (!IsValidPointer(id))
            {
                return AllocatePointerSlot();
            }
            // write a zero pointer first
            // to prevent delete interaction trouble
            WritePointer(id, Slot.Zero);
            if (DTrace.enabled)
            {
                DTrace.GetPointerSlot.Log(id);
            }
            return id;
        }

        protected virtual bool IsValidPointer(int id)
        {
            // We have to make sure that object IDs do not collide
            // with built-in type IDs.
            return !_handlers.IsSystemHandler(id);
        }

        public virtual Slot AllocateSlot(int length)
        {
            if (length <= 0)
            {
                throw new ArgumentException();
            }
            if (_freespaceManager != null && _freespaceManager.IsStarted())
            {
                var slot = _freespaceManager.AllocateSlot(length);
                if (slot != null)
                {
                    if (DTrace.enabled)
                    {
                        DTrace.GetSlot.LogLength(slot.Address(), slot.Length());
                    }
                    return slot;
                }
                while (GrowDatabaseByConfiguredSize())
                {
                    slot = _freespaceManager.AllocateSlot(length);
                    if (slot != null)
                    {
                        if (DTrace.enabled)
                        {
                            DTrace.GetSlot.LogLength(slot.Address(), slot.Length());
                        }
                        return slot;
                    }
                }
            }
            var appendedSlot = AppendBytes(length);
            if (DTrace.enabled)
            {
                DTrace.GetSlot.LogLength(appendedSlot.Address(), appendedSlot.Length());
            }
            return appendedSlot;
        }

        private bool GrowDatabaseByConfiguredSize()
        {
            var reservedStorageSpace = ConfigImpl.DatabaseGrowthSize();
            if (reservedStorageSpace <= 0)
            {
                return false;
            }
            var reservedBlocks = _blockConverter.BytesToBlocks(reservedStorageSpace);
            var reservedBytes = _blockConverter.BlocksToBytes(reservedBlocks);
            var slot = new Slot(_blockEndAddress, reservedBlocks);
            if (Debug4.xbytes && Deploy.overwrite)
            {
                OverwriteDeletedBlockedSlot(slot);
            }
            WriteBytes(new ByteArrayBuffer(reservedBytes), _blockEndAddress, 0);
            _freespaceManager.Free(_blockConverter.ToNonBlockedLength(slot));
            _blockEndAddress += reservedBlocks;
            return true;
        }

        public Slot AppendBytes(long bytes)
        {
            var blockCount = _blockConverter.BytesToBlocks(bytes);
            var blockedStartAddress = _blockEndAddress;
            var blockedEndAddress = _blockEndAddress + blockCount;
            CheckBlockedAddress(blockedEndAddress);
            _blockEndAddress = blockedEndAddress;
            var slot = new Slot(blockedStartAddress, blockCount);
            if (Debug4.xbytes && Deploy.overwrite)
            {
                OverwriteDeletedBlockedSlot(slot);
            }
            return _blockConverter.ToNonBlockedLength(slot);
        }

        private void CheckBlockedAddress(int blockedAddress)
        {
            if (blockedAddress < 0)
            {
                SwitchToReadOnlyMode();
                throw new DatabaseMaximumSizeReachedException();
            }
        }

        private void SwitchToReadOnlyMode()
        {
            _config.ReadOnly(true);
        }

        // When a file gets opened, it uses the file size to determine where 
        // new slots can be appended. If this method would not be called, the
        // freespace system could already contain a slot that points beyond
        // the end of the file and this space could be allocated and used twice,
        // for instance if a slot was allocated and freed without ever being
        // written to file.
        internal virtual void EnsureLastSlotWritten()
        {
            if (_blockEndAddress > _blockConverter.BytesToBlocks(FileLength()))
            {
                var writer = CreateStatefulBuffer(SystemTransaction(), _blockEndAddress
                                                                       - 1, BlockSize());
                writer.Write();
            }
        }

        public virtual void SetIdentity(Db4oDatabase identity)
        {
            lock (Lock())
            {
                _systemData.Identity(identity);
                // The dirty TimeStampIdGenerator triggers writing of
                // the variable part of the systemdata. We need to
                // make it dirty here, so the new identity is persisted:
                _timeStampIdGenerator.Generate();
                _fileHeader.WriteVariablePart(this);
            }
        }

        internal override bool IsServer()
        {
            return i_isServer;
        }

        public override sealed int IdForNewUserObject(Transaction trans)
        {
            return trans.IdSystem().NewId(SlotChangeFactory.UserObjects);
        }

        public override void RaiseCommitTimestamp(long minimumVersion)
        {
            lock (Lock())
            {
                _timeStampIdGenerator.SetMinimumNext(minimumVersion);
            }
        }

        public override StatefulBuffer ReadStatefulBufferById(Transaction a_ta, int a_id)
        {
            return ReadStatefulBufferById(a_ta, a_id, false);
        }

        public override ByteArrayBuffer[] ReadSlotBuffers(Transaction transaction, int[]
            ids)
        {
            var buffers = new ByteArrayBuffer[ids.Length];
            for (var i = 0; i < ids.Length; ++i)
            {
                if (ids[i] == 0)
                {
                    buffers[i] = null;
                }
                else
                {
                    buffers[i] = ReadBufferById(transaction, ids[i]);
                }
            }
            return buffers;
        }

        public override ByteArrayBuffer ReadBufferById(Transaction trans, int id)
        {
            return ReadBufferById(trans, id, false);
        }

        public override sealed ByteArrayBuffer ReadBufferById(Transaction trans, int id,
            bool lastCommitted)
        {
            if (id <= 0)
            {
                throw new ArgumentException();
            }
            var slot = lastCommitted
                ? trans.IdSystem().CommittedSlot(id)
                : trans.IdSystem()
                    .CurrentSlot(id);
            if (DTrace.enabled)
            {
                DTrace.SlotRead.LogLength(id, slot);
            }
            return ReadBufferBySlot(slot);
        }

        public override StatefulBuffer ReadStatefulBufferById(Transaction trans, int id,
            bool lastCommitted)
        {
            if (id <= 0)
            {
                throw new ArgumentException("id=" + id);
            }
            var slot = lastCommitted
                ? trans.IdSystem().CommittedSlot(id)
                : trans.IdSystem()
                    .CurrentSlot(id);
            if (DTrace.enabled)
            {
                DTrace.SlotRead.LogLength(id, slot);
            }
            return ReadStatefulBufferBySlot(trans, id, slot);
        }

        public virtual ByteArrayBuffer ReadBufferBySlot(Slot slot)
        {
            if (Slot.IsNull(slot))
            {
                return null;
            }
            if (DTrace.enabled)
            {
                DTrace.ReadSlot.LogLength(slot.Address(), slot.Length());
            }
            var buffer = new ByteArrayBuffer(slot.Length());
            buffer.ReadEncrypt(this, slot.Address());
            return buffer;
        }

        public virtual StatefulBuffer ReadStatefulBufferBySlot(Transaction trans, int id,
            Slot slot)
        {
            if (Slot.IsNull(slot))
            {
                return null;
            }
            if (DTrace.enabled)
            {
                DTrace.ReadSlot.LogLength(slot.Address(), slot.Length());
            }
            var buffer = CreateStatefulBuffer(trans, slot.Address(), slot.Length()
                );
            buffer.SetID(id);
            buffer.ReadEncrypt(this, slot.Address());
            return buffer;
        }

        protected override bool DoFinalize()
        {
            return _fileHeader != null;
        }

        /// <exception cref="Db4objects.Db4o.Ext.OldFormatException"></exception>
        internal virtual void ReadThis()
        {
            NewSystemData(AbstractFreespaceManager.FmLegacyRam, StandardIdSystemFactory.Legacy
                );
            BlockSizeReadFromFile(1);
            _fileHeader = FileHeader.Read(this);
            if (Config().GenerateCommitTimestamps().IsUnspecified())
            {
                Config().GenerateCommitTimestamps(_systemData.IdToTimestampIndexId() != 0);
            }
            CreateStringIO(_systemData.StringEncoding());
            CreateIdSystem();
            InitializeClassMetadataRepository();
            InitalizeWeakReferenceSupport();
            SetNextTimeStampId(SystemData().LastTimeStampID());
            ClassCollection().SetID(_systemData.ClassCollectionID());
            ClassCollection().Read(SystemTransaction());
            Converter.Convert(new ConversionStage.ClassCollectionAvailableStage(this));
            _fileHeader.ReadIdentity(this);
            if (_config.IsReadOnly())
            {
                return;
            }
            if (!ConfigImpl.CommitRecoveryDisabled())
            {
                _fileHeader.CompleteInterruptedTransaction(this);
            }
            IFreespaceManager blockedFreespaceManager = AbstractFreespaceManager.CreateNew(this
                , _systemData.FreespaceSystem());
            InstallFreespaceManager(blockedFreespaceManager);
            blockedFreespaceManager.Read(this, _systemData.InMemoryFreespaceSlot());
            blockedFreespaceManager.Start(_systemData.BTreeFreespaceId());
            _fileHeader = _fileHeader.Convert(this);
            if (FreespaceMigrationRequired(blockedFreespaceManager))
            {
                MigrateFreespace(blockedFreespaceManager);
            }
            WriteHeader(true, false);
            if (Converter.Convert(new ConversionStage.SystemUpStage(this)))
            {
                _systemData.ConverterVersion(Converter.Version);
                _fileHeader.WriteVariablePart(this);
                Transaction.Commit();
            }
        }

        private void InstallFreespaceManager(IFreespaceManager blockedFreespaceManager)
        {
            _freespaceManager = BlockSize() == 1
                ? blockedFreespaceManager
                : new BlockAwareFreespaceManager
                    (blockedFreespaceManager, _blockConverter);
        }

        protected virtual void CreateIdSystem()
        {
            _idSystem = StandardIdSystemFactory.NewInstance(this);
        }

        private bool FreespaceMigrationRequired(IFreespaceManager freespaceManager)
        {
            if (freespaceManager == null)
            {
                return false;
            }
            var readSystem = _systemData.FreespaceSystem();
            var configuredSystem = ConfigImpl.FreespaceSystem();
            if (freespaceManager.SystemType() == configuredSystem)
            {
                return false;
            }
            if (configuredSystem != 0)
            {
                return true;
            }
            return AbstractFreespaceManager.MigrationRequired(readSystem);
        }

        private void MigrateFreespace(IFreespaceManager oldFreespaceManager)
        {
            IFreespaceManager newFreespaceManager = AbstractFreespaceManager.CreateNew(this,
                ConfigImpl.FreespaceSystem());
            newFreespaceManager.Start(0);
            SystemData().FreespaceSystem(ConfigImpl.FreespaceSystem());
            InstallFreespaceManager(newFreespaceManager);
            AbstractFreespaceManager.Migrate(oldFreespaceManager, newFreespaceManager);
            _fileHeader.WriteVariablePart(this);
        }

        public override sealed void ReleaseSemaphore(Transaction trans, string name)
        {
            lock (_lock)
            {
                if (_semaphores == null)
                {
                    return;
                }
            }
            _semaphoresLock.Run(new _IClosure4_574(this, trans, name));
        }

        public override void ReleaseSemaphores(Transaction trans)
        {
            if (_semaphores != null)
            {
                var semaphores = _semaphores;
                _semaphoresLock.Run(new _IClosure4_588(this, semaphores, trans));
            }
        }

        public override sealed void Rollback1(Transaction trans)
        {
            trans.Rollback();
        }

        public override sealed void SetDirtyInSystemTransaction(PersistentBase a_object)
        {
            a_object.SetStateDirty();
            a_object.CacheDirty(_dirtyClassMetadata);
        }

        public override sealed bool SetSemaphore(Transaction trans, string name, int timeout
            )
        {
            if (name == null)
            {
                throw new ArgumentNullException();
            }
            lock (_lock)
            {
                if (_semaphores == null)
                {
                    _semaphores = new Hashtable4(10);
                }
            }
            var acquired = new BooleanByRef();
            _semaphoresLock.Run(new _IClosure4_625(this, trans, name, acquired, timeout));
            return acquired.value;
        }

        public virtual void SetServer(bool flag)
        {
            i_isServer = flag;
        }

        public abstract void SyncFiles();
        public abstract void SyncFiles(IRunnable runnable);

        protected override string DefaultToString()
        {
            return FileName();
        }

        public override void Shutdown()
        {
            WriteHeader(false, true);
        }

        public void CommitTransaction()
        {
            _transaction.Commit();
        }

        public abstract void WriteBytes(ByteArrayBuffer buffer, int blockedAddress, int addressOffset
            );

        public override sealed void WriteDirtyClassMetadata()
        {
            WriteCachedDirty();
        }

        private void WriteCachedDirty()
        {
            var i = _dirtyClassMetadata.GetEnumerator();
            while (i.MoveNext())
            {
                var dirty = (PersistentBase) i.Current;
                dirty.Write(SystemTransaction());
                dirty.NotCachedDirty();
            }
            _dirtyClassMetadata.Clear();
        }

        public void WriteEncrypt(ByteArrayBuffer buffer, int address, int addressOffset)
        {
            _handlers.Encrypt(buffer);
            WriteBytes(buffer, address, addressOffset);
            _handlers.Decrypt(buffer);
        }

        public virtual void WriteHeader(bool startFileLockingThread, bool shuttingDown)
        {
            if (shuttingDown)
            {
                _freespaceManager.Write(this);
                _freespaceManager = null;
            }
            var writer = CreateStatefulBuffer(SystemTransaction(), 0, _fileHeader.
                Length());
            _fileHeader.WriteFixedPart(this, startFileLockingThread, shuttingDown, writer, BlockSize
                ());
            if (shuttingDown)
            {
                EnsureLastSlotWritten();
            }
            SyncFiles();
        }

        public override sealed void WriteNew(Transaction trans, Pointer4 pointer, ClassMetadata
            classMetadata, ByteArrayBuffer buffer)
        {
            WriteEncrypt(buffer, pointer.Address(), 0);
            if (classMetadata == null)
            {
                return;
            }
            classMetadata.AddToIndex(trans, pointer.Id());
        }

        // This is a reroute of writeBytes to write the free blocks
        // unchecked.
        public abstract void OverwriteDeletedBytes(int address, int length);

        public virtual void OverwriteDeletedBlockedSlot(Slot slot)
        {
            OverwriteDeletedBytes(slot.Address(), _blockConverter.BlocksToBytes(slot.Length()
                ));
        }

        public void WriteTransactionPointer(int pointer)
        {
            _fileHeader.WriteTransactionPointer(SystemTransaction(), pointer);
        }

        public Slot AllocateSlotForUserObjectUpdate(Transaction trans, int id, int length
            )
        {
            var slot = AllocateSlot(length);
            trans.IdSystem().NotifySlotUpdated(id, slot, SlotChangeFactory.UserObjects);
            return slot;
        }

        public Slot AllocateSlotForNewUserObject(Transaction trans, int id, int length)
        {
            var slot = AllocateSlot(length);
            trans.IdSystem().NotifySlotCreated(id, slot, SlotChangeFactory.UserObjects);
            return slot;
        }

        public override sealed void WriteUpdate(Transaction trans, Pointer4 pointer, ClassMetadata
            classMetadata, ArrayType arrayType, ByteArrayBuffer buffer)
        {
            var address = pointer.Address();
            if (address == 0)
            {
                address = AllocateSlotForUserObjectUpdate(trans, pointer.Id(), pointer.Length()).
                    Address();
            }
            WriteEncrypt(buffer, address, 0);
        }

        public virtual void SetNextTimeStampId(long val)
        {
            _timeStampIdGenerator.SetMinimumNext(val);
        }

        public virtual FileHeader GetFileHeader()
        {
            return _fileHeader;
        }

        public virtual void InstallDebugFreespaceManager(IFreespaceManager manager)
        {
            _freespaceManager = manager;
        }

        public virtual SystemData SystemData()
        {
            return _systemData;
        }

        public override long[] GetIDsForClass(Transaction trans, ClassMetadata clazz)
        {
            var ids = new IntArrayList();
            clazz.Index().TraverseAll(trans, new _IVisitor4_792(ids));
            return ids.AsLong();
        }

        public override IQueryResult ClassOnlyQuery(QQueryBase query, ClassMetadata clazz
            )
        {
            if (!clazz.HasClassIndex())
            {
                return new IdListQueryResult(query.Transaction());
            }
            var queryResult = NewQueryResult(query.Transaction());
            queryResult.LoadFromClassIndex(clazz);
            return queryResult;
        }

        public override IQueryResult ExecuteQuery(QQuery query)
        {
            var queryResult = NewQueryResult(query.Transaction());
            queryResult.LoadFromQuery(query);
            return queryResult;
        }

        public virtual LocalTransaction LocalSystemTransaction()
        {
            return (LocalTransaction) SystemTransaction();
        }

        public override bool IsDeleted(Transaction trans, int id)
        {
            return trans.IdSystem().IsDeleted(id);
        }

        public virtual void WritePointer(int id, Slot slot)
        {
            if (DTrace.enabled)
            {
                DTrace.WritePointer.Log(id);
                DTrace.WritePointer.LogLength(slot);
            }
            _pointerIo.Seek(0);
            _pointerIo.WriteInt(slot.Address());
            _pointerIo.WriteInt(slot.Length());
            WriteBytes(_pointerIo, id, 0);
        }

        public virtual Slot DebugReadPointerSlot(int id)
        {
            return null;
        }

        public Slot ReadPointerSlot(int id)
        {
            if (!IsValidId(id))
            {
                throw new InvalidIDException(id);
            }
            ReadBytes(_pointerBuffer, id, Const4.PointerLength);
            var address = (_pointerBuffer[3] & 255) | (_pointerBuffer[2] & 255) << 8 | (_pointerBuffer
                [1] & 255) << 16 | _pointerBuffer[0] << 24;
            var length = (_pointerBuffer[7] & 255) | (_pointerBuffer[6] & 255) << 8 | (_pointerBuffer
                [5] & 255) << 16 | _pointerBuffer[4] << 24;
            if (!IsValidSlot(address, length))
            {
                throw new InvalidSlotException(address, length, id);
            }
            return new Slot(address, length);
        }

        private bool IsValidId(int id)
        {
            return FileLength() >= id;
        }

        private bool IsValidSlot(int address, int length)
        {
            // just in case overflow 
            var fileLength = FileLength();
            var validAddress = fileLength >= address;
            var validLength = fileLength >= length;
            var validSlot = fileLength >= (address + length);
            return validAddress && validLength && validSlot;
        }

        protected override void CloseIdSystem()
        {
            if (_idSystem != null)
            {
                _idSystem.Close();
            }
        }

        public virtual IIdSystem IdSystem()
        {
            return _idSystem;
        }

        public virtual IRunnable CommitHook()
        {
            _systemData.LastTimeStampID(_timeStampIdGenerator.Last());
            return _fileHeader.Commit(false);
        }

        public Slot AllocateSafeSlot(int length)
        {
            var reusedSlot = FreespaceManager().AllocateSafeSlot(length);
            if (reusedSlot != null)
            {
                return reusedSlot;
            }
            return AppendBytes(length);
        }

        public virtual IQLin From(Type clazz)
        {
            return new QLinRoot(Query(), clazz);
        }

        private sealed class _IClosure4_58 : IClosure4
        {
            private readonly LocalObjectContainer _enclosing;

            public _IClosure4_58(LocalObjectContainer _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public object Run()
            {
                return _enclosing.IdSystem();
            }
        }

        private sealed class _IClosure4_69 : IClosure4
        {
            private readonly LocalObjectContainer _enclosing;

            public _IClosure4_69(LocalObjectContainer _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public object Run()
            {
                return _enclosing.FreespaceManager();
            }
        }

        private sealed class _IClosure4_574 : IClosure4
        {
            private readonly LocalObjectContainer _enclosing;
            private readonly string name;
            private readonly Transaction trans;

            public _IClosure4_574(LocalObjectContainer _enclosing, Transaction trans, string
                name)
            {
                this._enclosing = _enclosing;
                this.trans = trans;
                this.name = name;
            }

            public object Run()
            {
                var transaction = _enclosing.CheckTransaction(trans);
                if (_enclosing._semaphores != null && transaction == _enclosing._semaphores
                    .Get(name))
                {
                    _enclosing._semaphores.Remove(name);
                }
                _enclosing._semaphoresLock.Awake();
                return null;
            }
        }

        private sealed class _IClosure4_588 : IClosure4
        {
            private readonly LocalObjectContainer _enclosing;
            private readonly Hashtable4 semaphores;
            private readonly Transaction trans;

            public _IClosure4_588(LocalObjectContainer _enclosing, Hashtable4 semaphores, Transaction
                trans)
            {
                this._enclosing = _enclosing;
                this.semaphores = semaphores;
                this.trans = trans;
            }

            public object Run()
            {
                semaphores.ForEachKeyForIdentity(new _IVisitor4_589(semaphores), trans);
                _enclosing._semaphoresLock.Awake();
                return null;
            }

            private sealed class _IVisitor4_589 : IVisitor4
            {
                private readonly Hashtable4 semaphores;

                public _IVisitor4_589(Hashtable4 semaphores)
                {
                    this.semaphores = semaphores;
                }

                public void Visit(object a_object)
                {
                    semaphores.Remove(a_object);
                }
            }
        }

        private sealed class _IClosure4_625 : IClosure4
        {
            private readonly LocalObjectContainer _enclosing;
            private readonly BooleanByRef acquired;
            private readonly string name;
            private readonly int timeout;
            private readonly Transaction trans;

            public _IClosure4_625(LocalObjectContainer _enclosing, Transaction trans, string
                name, BooleanByRef acquired, int timeout)
            {
                this._enclosing = _enclosing;
                this.trans = trans;
                this.name = name;
                this.acquired = acquired;
                this.timeout = timeout;
            }

            public object Run()
            {
                try
                {
                    var transaction = _enclosing.CheckTransaction(trans);
                    var candidateTransaction = _enclosing._semaphores.Get(name);
                    if (trans == candidateTransaction)
                    {
                        acquired.value = true;
                        return null;
                    }
                    if (candidateTransaction == null)
                    {
                        _enclosing._semaphores.Put(name, transaction);
                        acquired.value = true;
                        return null;
                    }
                    var endtime = Runtime.CurrentTimeMillis() + timeout;
                    long waitTime = timeout;
                    while (waitTime > 0)
                    {
                        _enclosing._semaphoresLock.Awake();
                        _enclosing._semaphoresLock.Snooze(waitTime);
                        if (_enclosing.ClassCollection() == null)
                        {
                            acquired.value = false;
                            return null;
                        }
                        candidateTransaction = _enclosing._semaphores.Get(name);
                        if (candidateTransaction == null)
                        {
                            _enclosing._semaphores.Put(name, transaction);
                            acquired.value = true;
                            return null;
                        }
                        waitTime = endtime - Runtime.CurrentTimeMillis();
                    }
                    acquired.value = false;
                    return null;
                }
                finally
                {
                    _enclosing._semaphoresLock.Awake();
                }
            }
        }

        private sealed class _IVisitor4_792 : IVisitor4
        {
            private readonly IntArrayList ids;

            public _IVisitor4_792(IntArrayList ids)
            {
                this.ids = ids;
            }

            public void Visit(object obj)
            {
                ids.Add(((int) obj));
            }
        }
    }
}