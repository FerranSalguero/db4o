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
using Db4objects.Db4o.Internal.Activation;
using Db4objects.Db4o.Internal.Callbacks;
using Db4objects.Db4o.Internal.Ids;
using Db4objects.Db4o.Internal.References;

namespace Db4objects.Db4o.Internal
{
    /// <exclude></exclude>
    public class LocalTransaction : Transaction
    {
        private readonly ICommittedCallbackDispatcher _committedCallbackDispatcher;
        protected readonly LocalObjectContainer _file;
        private readonly ITransactionalIdSystem _idSystem;
        private readonly IdentitySet4 _participants = new IdentitySet4();
        private CommitTimestampSupport _commitTimestampSupport;
        private IList _concurrentReplicationTimestamps;
        private long _timestamp;
        internal Tree _writtenUpdateAdjustedIndexes;

        public LocalTransaction(ObjectContainerBase container, Transaction parentTransaction
            , ITransactionalIdSystem idSystem, IReferenceSystem referenceSystem) : base(container
                , parentTransaction, referenceSystem)
        {
            _file = (LocalObjectContainer) container;
            _committedCallbackDispatcher = new _ICommittedCallbackDispatcher_39(this);
            _idSystem = idSystem;
        }

        public virtual Config4Impl Config()
        {
            return Container().Config();
        }

        public virtual LocalObjectContainer LocalContainer()
        {
            return _file;
        }

        public override void Commit()
        {
            Commit(_committedCallbackDispatcher);
        }

        public virtual void Commit(ICommittedCallbackDispatcher dispatcher)
        {
            lock (Container().Lock())
            {
                CommitListeners();
                DispatchCommittingCallback();
                if (!DoCommittedCallbacks(dispatcher))
                {
                    CommitImpl();
                    CommitClearAll();
                }
                else
                {
                    var deleted = CollectCommittedCallbackDeletedInfo();
                    CommitImpl();
                    var committedInfo = CollectCommittedCallbackInfo(deleted
                        );
                    CommitClearAll();
                    dispatcher.DispatchCommitted(CallbackObjectInfoCollections.Emtpy == committedInfo
                        ? committedInfo
                        : new CallbackObjectInfoCollections(committedInfo.added, committedInfo
                            .updated, new ObjectInfoCollectionImpl(deleted)));
                }
            }
        }

        private void DispatchCommittingCallback()
        {
            if (DoCommittingCallbacks())
            {
                Callbacks().CommitOnStarted(this, CollectCommittingCallbackInfo());
            }
        }

        private bool DoCommittedCallbacks(ICommittedCallbackDispatcher dispatcher)
        {
            if (IsSystemTransaction())
            {
                return false;
            }
            return dispatcher.WillDispatchCommitted();
        }

        private bool DoCommittingCallbacks()
        {
            if (IsSystemTransaction())
            {
                return false;
            }
            return Callbacks().CaresAboutCommitting();
        }

        public virtual void Enlist(ITransactionParticipant participant)
        {
            if (null == participant)
            {
                throw new ArgumentNullException();
            }
            CheckSynchronization();
            if (!_participants.Contains(participant))
            {
                _participants.Add(participant);
            }
        }

        private void CommitImpl()
        {
            if (DTrace.enabled)
            {
                DTrace.TransCommit.LogInfo("server == " + Container().IsServer() + ", systemtrans == "
                                           + IsSystemTransaction());
            }
            CommitClassMetadata();
            CommitParticipants();
            Container().WriteDirtyClassMetadata();
            IdSystem().Commit(new FreespaceCommitter(LocalContainer().FreespaceManager()));
        }

        private void CommitListeners()
        {
            CommitParentListeners();
            CommitTransactionListeners();
        }

        private void CommitParentListeners()
        {
            if (_systemTransaction != null)
            {
                ParentLocalTransaction().CommitListeners();
            }
        }

        private void CommitParticipants()
        {
            if (ParentLocalTransaction() != null)
            {
                ParentLocalTransaction().CommitParticipants();
            }
            var iterator = _participants.GetEnumerator();
            while (iterator.MoveNext())
            {
                ((ITransactionParticipant) iterator.Current).Commit(this);
            }
        }

        private void CommitClassMetadata()
        {
            Container().ProcessPendingClassUpdates();
            Container().WriteDirtyClassMetadata();
            Container().ClassCollection().Write(Container().SystemTransaction());
        }

        private LocalTransaction ParentLocalTransaction()
        {
            return (LocalTransaction) _systemTransaction;
        }

        private void CommitClearAll()
        {
            if (_systemTransaction != null)
            {
                ParentLocalTransaction().CommitClearAll();
            }
            ClearAll();
        }

        protected override void Clear()
        {
            IdSystem().Clear();
            DisposeParticipants();
            _participants.Clear();
        }

        private void DisposeParticipants()
        {
            var iterator = _participants.ValuesIterator();
            while (iterator.MoveNext())
            {
                ((ITransactionParticipant) iterator.Current).Dispose(this);
            }
        }

        public override void Rollback()
        {
            lock (Container().Lock())
            {
                RollbackParticipants();
                IdSystem().Rollback();
                RollBackTransactionListeners();
                ClearAll();
            }
        }

        private void RollbackParticipants()
        {
            var iterator = _participants.ValuesIterator();
            while (iterator.MoveNext())
            {
                ((ITransactionParticipant) iterator.Current).Rollback(this);
            }
        }

        public virtual void FlushFile()
        {
            if (DTrace.enabled)
            {
                DTrace.TransFlush.Log();
            }
            _file.SyncFiles();
        }

        public override void ProcessDeletes()
        {
            if (_delete == null)
            {
                _writtenUpdateAdjustedIndexes = null;
                return;
            }
            while (_delete != null)
            {
                var delete = _delete;
                _delete = null;
                delete.Traverse(new _IVisitor4_224(this));
            }
            // if the object has been deleted
            // We need to hold a hard reference here, otherwise we can get 
            // intermediate garbage collection kicking in.
            // This means the object was gc'd.
            // Let's try to read it again, but this may fail in
            // CS mode if another transaction has deleted it. 
            _writtenUpdateAdjustedIndexes = null;
        }

        public override void WriteUpdateAdjustIndexes(int id, ClassMetadata clazz, ArrayType
            typeInfo)
        {
            new WriteUpdateProcessor(this, id, clazz, typeInfo).Run();
        }

        private ICallbacks Callbacks()
        {
            return Container().Callbacks();
        }

        private Collection4 CollectCommittedCallbackDeletedInfo()
        {
            var deleted = new Collection4();
            CollectCallBackInfo(new _ICallbackInfoCollector_274(this, deleted));
            return deleted;
        }

        private CallbackObjectInfoCollections CollectCommittedCallbackInfo(Collection4 deleted
            )
        {
            if (!IdSystem().IsDirty())
            {
                return CallbackObjectInfoCollections.Emtpy;
            }
            var added = new Collection4();
            var updated = new Collection4();
            CollectCallBackInfo(new _ICallbackInfoCollector_297(this, added, updated));
            return NewCallbackObjectInfoCollections(added, updated, deleted);
        }

        private CallbackObjectInfoCollections CollectCommittingCallbackInfo()
        {
            if (!IdSystem().IsDirty())
            {
                return CallbackObjectInfoCollections.Emtpy;
            }
            var added = new Collection4();
            var deleted = new Collection4();
            var updated = new Collection4();
            CollectCallBackInfo(new _ICallbackInfoCollector_320(this, added, updated, deleted
                ));
            return NewCallbackObjectInfoCollections(added, updated, deleted);
        }

        private CallbackObjectInfoCollections NewCallbackObjectInfoCollections(Collection4
            added, Collection4 updated, Collection4 deleted)
        {
            return new CallbackObjectInfoCollections(new ObjectInfoCollectionImpl(added), new
                ObjectInfoCollectionImpl(updated), new ObjectInfoCollectionImpl(deleted));
        }

        private void CollectCallBackInfo(ICallbackInfoCollector collector)
        {
            IdSystem().CollectCallBackInfo(collector);
        }

        public override ITransactionalIdSystem IdSystem()
        {
            return _idSystem;
        }

        public virtual IObjectInfo FrozenReferenceFor(int id)
        {
            var @ref = ReferenceForId(id);
            if (@ref != null)
            {
                if (IsStruct(@ref))
                {
                    return null;
                }
                return new FrozenObjectInfo(this, @ref, true);
            }
            @ref = Container().PeekReference(SystemTransaction(), id, new FixedActivationDepth
                (0), true);
            if (@ref == null || @ref.GetObject() == null || IsStruct(@ref))
            {
                return null;
            }
            return new FrozenObjectInfo(SystemTransaction(), @ref, true);
        }

        private bool IsStruct(ObjectReference @ref)
        {
            return @ref.ClassMetadata().IsStruct();
        }

        public virtual LazyObjectReference LazyReferenceFor(int id)
        {
            return new LazyObjectReference(this, id);
        }

        public override long VersionForId(int id)
        {
            return CommitTimestampSupport().VersionForId(id);
        }

        public virtual CommitTimestampSupport CommitTimestampSupport
            ()
        {
            if (!IsSystemTransaction())
            {
                throw new InvalidOperationException();
            }
            if (_commitTimestampSupport == null)
            {
                _commitTimestampSupport = new CommitTimestampSupport(LocalContainer
                    ());
            }
            return _commitTimestampSupport;
        }

        public override long GenerateTransactionTimestamp(long forcedTimeStamp)
        {
            if (forcedTimeStamp > 0)
            {
                _timestamp = forcedTimeStamp;
            }
            else
            {
                _timestamp = LocalContainer().GenerateTimeStampId();
            }
            return _timestamp;
        }

        public override void UseDefaultTransactionTimestamp()
        {
            _timestamp = 0;
            _concurrentReplicationTimestamps = null;
        }

        public virtual long Timestamp()
        {
            return _timestamp;
        }

        public virtual void NotifyAboutOtherReplicationCommit(long replicationVersion, IList
            concurrentTimestamps)
        {
            if (Timestamp() == 0)
            {
                return;
            }
            if (_concurrentReplicationTimestamps == null)
            {
                _concurrentReplicationTimestamps = new ArrayList();
            }
            _concurrentReplicationTimestamps.Add(replicationVersion);
            concurrentTimestamps.Add(Timestamp());
        }

        public virtual IList ConcurrentReplicationTimestamps()
        {
            if (_concurrentReplicationTimestamps != null)
            {
                return _concurrentReplicationTimestamps;
            }
            return new ArrayList();
        }

        public override void PostOpen()
        {
            base.PostOpen();
            if (IsSystemTransaction())
            {
                CommitTimestampSupport().EnsureInitialized();
            }
        }

        private sealed class _ICommittedCallbackDispatcher_39 : ICommittedCallbackDispatcher
        {
            private readonly LocalTransaction _enclosing;

            public _ICommittedCallbackDispatcher_39(LocalTransaction _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public bool WillDispatchCommitted()
            {
                return _enclosing.Callbacks().CaresAboutCommitted();
            }

            public void DispatchCommitted(CallbackObjectInfoCollections committedInfo)
            {
                _enclosing.Callbacks().CommitOnCompleted(_enclosing, committedInfo, false
                    );
            }
        }

        private sealed class _IVisitor4_224 : IVisitor4
        {
            private readonly LocalTransaction _enclosing;

            public _IVisitor4_224(LocalTransaction _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Visit(object a_object)
            {
                var info = (DeleteInfo) a_object;
                if (_enclosing.LocalContainer().IsDeleted(_enclosing, info._key))
                {
                    return;
                }
                object obj = null;
                if (info._reference != null)
                {
                    obj = info._reference.GetObject();
                }
                if (obj == null || info._reference.GetID() < 0)
                {
                    var hardRef = _enclosing.Container().GetHardObjectReferenceById
                        (_enclosing, info._key);
                    if (hardRef == HardObjectReference.Invalid)
                    {
                        return;
                    }
                    info._reference = hardRef._reference;
                    info._reference.FlagForDelete(_enclosing.Container().TopLevelCallId());
                    obj = info._reference.GetObject();
                }
                _enclosing.Container().Delete3(_enclosing, info._reference, obj, info._cascade
                    , false);
            }
        }

        private sealed class _ICallbackInfoCollector_274 : ICallbackInfoCollector
        {
            private readonly LocalTransaction _enclosing;
            private readonly Collection4 deleted;

            public _ICallbackInfoCollector_274(LocalTransaction _enclosing, Collection4 deleted
                )
            {
                this._enclosing = _enclosing;
                this.deleted = deleted;
            }

            public void Deleted(int id)
            {
                var @ref = _enclosing.FrozenReferenceFor(id);
                if (@ref != null)
                {
                    deleted.Add(@ref);
                }
            }

            public void Updated(int id)
            {
            }

            public void Added(int id)
            {
            }
        }

        private sealed class _ICallbackInfoCollector_297 : ICallbackInfoCollector
        {
            private readonly LocalTransaction _enclosing;
            private readonly Collection4 added;
            private readonly Collection4 updated;

            public _ICallbackInfoCollector_297(LocalTransaction _enclosing, Collection4 added
                , Collection4 updated)
            {
                this._enclosing = _enclosing;
                this.added = added;
                this.updated = updated;
            }

            public void Added(int id)
            {
                added.Add(_enclosing.LazyReferenceFor(id));
            }

            public void Updated(int id)
            {
                updated.Add(_enclosing.LazyReferenceFor(id));
            }

            public void Deleted(int id)
            {
            }
        }

        private sealed class _ICallbackInfoCollector_320 : ICallbackInfoCollector
        {
            private readonly LocalTransaction _enclosing;
            private readonly Collection4 added;
            private readonly Collection4 deleted;
            private readonly Collection4 updated;

            public _ICallbackInfoCollector_320(LocalTransaction _enclosing, Collection4 added
                , Collection4 updated, Collection4 deleted)
            {
                this._enclosing = _enclosing;
                this.added = added;
                this.updated = updated;
                this.deleted = deleted;
            }

            public void Added(int id)
            {
                added.Add(_enclosing.LazyReferenceFor(id));
            }

            public void Updated(int id)
            {
                updated.Add(_enclosing.LazyReferenceFor(id));
            }

            public void Deleted(int id)
            {
                var @ref = _enclosing.FrozenReferenceFor(id);
                if (@ref != null)
                {
                    deleted.Add(@ref);
                }
            }
        }
    }
}