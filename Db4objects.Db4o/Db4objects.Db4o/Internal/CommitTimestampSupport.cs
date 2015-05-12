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
using Db4objects.Db4o.Events;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal.Btree;
using Db4objects.Db4o.Internal.Handlers;
using Db4objects.Db4o.Marshall;

namespace Db4objects.Db4o.Internal
{
    public class CommitTimestampSupport
    {
        private readonly LocalObjectContainer _container;
        private BTree _idToTimestamp;
        private BTree _timestampToId;

        public CommitTimestampSupport(LocalObjectContainer container)
        {
            _container = container;
        }

        public virtual void EnsureInitialized()
        {
            if (_idToTimestamp != null)
            {
                return;
            }
            if (!_container.Config().GenerateCommitTimestamps().DefiniteYes())
            {
                return;
            }
            Initialize();
        }

        public virtual BTree IdToTimestamp()
        {
            if (_idToTimestamp != null)
            {
                return _idToTimestamp;
            }
            EnsureInitialized();
            return _idToTimestamp;
        }

        public virtual BTree TimestampToId()
        {
            if (_timestampToId != null)
            {
                return _timestampToId;
            }
            EnsureInitialized();
            return _timestampToId;
        }

        private void Initialize()
        {
            var idToTimestampIndexId = _container.SystemData().IdToTimestampIndexId();
            var timestampToIdIndexId = _container.SystemData().TimestampToIdIndexId();
            if (_container.Config().IsReadOnly())
            {
                if (idToTimestampIndexId == 0)
                {
                    return;
                }
            }
            _idToTimestamp = new BTree(_container.SystemTransaction(), idToTimestampIndexId,
                new TimestampEntryById());
            _timestampToId = new BTree(_container.SystemTransaction(), timestampToIdIndexId,
                new IdEntryByTimestamp());
            if (idToTimestampIndexId != _idToTimestamp.GetID())
            {
                StoreBtreesIds();
            }
            EventRegistryFactory.ForObjectContainer(_container).Committing += new _IEventListener4_65(this).OnEvent;
        }

        private void StoreBtreesIds()
        {
            _container.SystemData().IdToTimestampIndexId(_idToTimestamp.GetID());
            _container.SystemData().TimestampToIdIndexId(_timestampToId.GetID());
            _container.GetFileHeader().WriteVariablePart(_container);
        }

        public virtual long VersionForId(int id)
        {
            if (IdToTimestamp() == null || id == 0)
            {
                return 0;
            }
            var te = (TimestampEntry
                ) IdToTimestamp().Search(_container.SystemTransaction(), new TimestampEntry
                    (id, 0));
            if (te == null)
            {
                return 0;
            }
            return te.GetCommitTimestamp();
        }

        public virtual void Put(Transaction trans, int objectId, long version)
        {
            var te = new TimestampEntry
                (objectId, version);
            IdToTimestamp().Add(trans, te);
            TimestampToId().Add(trans, te);
        }

        private sealed class _IEventListener4_65
        {
            private readonly CommitTimestampSupport _enclosing;

            public _IEventListener4_65(CommitTimestampSupport _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void OnEvent(object sender, CommitEventArgs args)
            {
                var trans = (LocalTransaction) args.Transaction();
                var transactionTimestamp = trans.Timestamp();
                var commitTimestamp = (transactionTimestamp > 0)
                    ? transactionTimestamp
                    : _enclosing
                        ._container.GenerateTimeStampId();
                var sysTrans = trans.SystemTransaction();
                AddTimestamp(sysTrans, args.Added.GetEnumerator(), commitTimestamp
                    );
                AddTimestamp(sysTrans, args.Updated.GetEnumerator(), commitTimestamp
                    );
                AddTimestamp(sysTrans, args.Deleted.GetEnumerator(), 0);
            }

            private void AddTimestamp(Transaction trans, IEnumerator it, long commitTimestamp
                )
            {
                while (it.MoveNext())
                {
                    var objInfo = (IObjectInfo) it.Current;
                    var te = new TimestampEntry
                        ((int) objInfo.GetInternalID(), commitTimestamp);
                    var oldEntry = (TimestampEntry
                        ) _enclosing._idToTimestamp.Remove(trans, te);
                    if (oldEntry != null)
                    {
                        _enclosing._timestampToId.Remove(trans, oldEntry);
                    }
                    if (commitTimestamp != 0)
                    {
                        _enclosing._idToTimestamp.Add(trans, te);
                        _enclosing._timestampToId.Add(trans, te);
                    }
                }
            }
        }

        public class TimestampEntry : IFieldIndexKey
        {
            public readonly long commitTimestamp;
            public readonly int objectId;

            public TimestampEntry(int objectId, long commitTimestamp)
            {
                this.objectId = objectId;
                this.commitTimestamp = commitTimestamp;
            }

            public virtual int ParentID()
            {
                return objectId;
            }

            public virtual object Value()
            {
                return commitTimestamp;
            }

            public override string ToString()
            {
                return "TimestampEntry [objectId=" + objectId + ", commitTimestamp=" + commitTimestamp
                       + "]";
            }

            public virtual long GetCommitTimestamp()
            {
                return commitTimestamp;
            }
        }

        private class TimestampEntryById : IIndexable4
        {
            public virtual IPreparedComparison PrepareComparison(IContext context, object first
                )
            {
                return new _IPreparedComparison_135(first);
            }

            public virtual int LinkLength()
            {
                return Const4.IntLength + Const4.LongLength;
            }

            public virtual object ReadIndexEntry(IContext context, ByteArrayBuffer reader)
            {
                return new TimestampEntry(reader.ReadInt(), reader.ReadLong
                    ());
            }

            public virtual void WriteIndexEntry(IContext context, ByteArrayBuffer writer, object
                obj)
            {
                writer.WriteInt(((TimestampEntry) obj).ParentID());
                writer.WriteLong(((TimestampEntry) obj).GetCommitTimestamp(
                    ));
            }

            public virtual void DefragIndexEntry(DefragmentContextImpl context)
            {
                // we are storing ids in the btree, so the order will change when the ids change
                // to properly defrag the btree we need to readd all the entries
                throw new NotSupportedException();
            }

            private sealed class _IPreparedComparison_135 : IPreparedComparison
            {
                private readonly object first;

                public _IPreparedComparison_135(object first)
                {
                    this.first = first;
                }

                public int CompareTo(object second)
                {
                    return IntHandler.Compare(((TimestampEntry) first).objectId
                        , ((TimestampEntry) second).objectId);
                }
            }
        }

        private sealed class IdEntryByTimestamp : TimestampEntryById
        {
            public override IPreparedComparison PrepareComparison(IContext context, object first
                )
            {
                return new _IPreparedComparison_164(first);
            }

            private sealed class _IPreparedComparison_164 : IPreparedComparison
            {
                private readonly object first;

                public _IPreparedComparison_164(object first)
                {
                    this.first = first;
                }

                public int CompareTo(object second)
                {
                    var result = LongHandler.Compare(((TimestampEntry) first).commitTimestamp
                        , ((TimestampEntry) second).commitTimestamp);
                    if (result != 0)
                    {
                        return result;
                    }
                    return IntHandler.Compare(((TimestampEntry) first).objectId
                        , ((TimestampEntry) second).objectId);
                }
            }
        }
    }
}