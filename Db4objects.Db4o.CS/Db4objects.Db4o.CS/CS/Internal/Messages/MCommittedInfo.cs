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
using Sharpen.IO;
using Sharpen.Lang;

namespace Db4objects.Db4o.CS.Internal.Messages
{
    /// <exclude></exclude>
    public class MCommittedInfo : MsgD, IClientSideMessage
    {
        public virtual bool ProcessAtClient()
        {
            var @is = new ByteArrayInputStream(_payLoad._buffer);
            var dispatcherID = PrimitiveCodec.ReadInt(@is);
            var callbackInfos = Decode(@is);
            Container().ThreadPool().Start(ReflectPlatform.SimpleName(GetType()) + ": calling commit callbacks thread"
                , new _IRunnable_111(this, callbackInfos, dispatcherID));
            return true;
        }

        public virtual MCommittedInfo Encode(CallbackObjectInfoCollections callbackInfo,
            int dispatcherID)
        {
            var os = new ByteArrayOutputStream();
            PrimitiveCodec.WriteInt(os, dispatcherID);
            var bytes = EncodeInfo(callbackInfo, os);
            var committedInfo = (MCommittedInfo) GetWriterForLength(Transaction(),
                bytes.Length + Const4.IntLength);
            committedInfo._payLoad.Append(bytes);
            return committedInfo;
        }

        private byte[] EncodeInfo(CallbackObjectInfoCollections callbackInfo, ByteArrayOutputStream
            os)
        {
            EncodeObjectInfoCollection(os, callbackInfo.added, new InternalIDEncoder
                (this));
            EncodeObjectInfoCollection(os, callbackInfo.deleted, new FrozenObjectInfoEncoder
                (this));
            EncodeObjectInfoCollection(os, callbackInfo.updated, new InternalIDEncoder
                (this));
            return os.ToByteArray();
        }

        private void EncodeObjectInfoCollection(ByteArrayOutputStream os, IObjectInfoCollection
            collection, IObjectInfoEncoder encoder)
        {
            var iter = collection.GetEnumerator();
            while (iter.MoveNext())
            {
                var obj = (IObjectInfo) iter.Current;
                encoder.Encode(os, obj);
            }
            PrimitiveCodec.WriteLong(os, -1);
        }

        public virtual CallbackObjectInfoCollections Decode(ByteArrayInputStream @is)
        {
            var added = DecodeObjectInfoCollection(@is, new InternalIDEncoder
                (this));
            var deleted = DecodeObjectInfoCollection(@is, new FrozenObjectInfoEncoder
                (this));
            var updated = DecodeObjectInfoCollection(@is, new InternalIDEncoder
                (this));
            return new CallbackObjectInfoCollections(added, updated, deleted);
        }

        private IObjectInfoCollection DecodeObjectInfoCollection(ByteArrayInputStream @is
            , IObjectInfoEncoder encoder)
        {
            var collection = new Collection4();
            while (true)
            {
                var info = encoder.Decode(@is);
                if (null == info)
                {
                    break;
                }
                collection.Add(info);
            }
            return new ObjectInfoCollectionImpl(collection);
        }

        /// <exception cref="System.IO.IOException"></exception>
        protected virtual void WriteByteArray(ByteArrayOutputStream os, byte[] signaturePart
            )
        {
            PrimitiveCodec.WriteLong(os, signaturePart.Length);
            os.Write(signaturePart);
        }

        private sealed class FrozenObjectInfoEncoder : IObjectInfoEncoder
        {
            private readonly MCommittedInfo _enclosing;

            internal FrozenObjectInfoEncoder(MCommittedInfo _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Encode(ByteArrayOutputStream os, IObjectInfo info)
            {
                PrimitiveCodec.WriteLong(os, info.GetInternalID());
                var sourceDatabaseId = ((FrozenObjectInfo) info).SourceDatabaseId(_enclosing
                    .Transaction());
                PrimitiveCodec.WriteLong(os, sourceDatabaseId);
                PrimitiveCodec.WriteLong(os, ((FrozenObjectInfo) info).UuidLongPart());
                PrimitiveCodec.WriteLong(os, info.GetCommitTimestamp());
            }

            public IObjectInfo Decode(ByteArrayInputStream @is)
            {
                var id = PrimitiveCodec.ReadLong(@is);
                if (id == -1)
                {
                    return null;
                }
                var sourceDatabaseId = PrimitiveCodec.ReadLong(@is);
                Db4oDatabase sourceDatabase = null;
                if (sourceDatabaseId > 0)
                {
                    sourceDatabase = (Db4oDatabase) _enclosing.Container().GetByID(_enclosing
                        .Transaction(), sourceDatabaseId);
                }
                var uuidLongPart = PrimitiveCodec.ReadLong(@is);
                var version = PrimitiveCodec.ReadLong(@is);
                return new FrozenObjectInfo(null, id, sourceDatabase, uuidLongPart, version);
            }
        }

        private sealed class InternalIDEncoder : IObjectInfoEncoder
        {
            private readonly MCommittedInfo _enclosing;

            internal InternalIDEncoder(MCommittedInfo _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Encode(ByteArrayOutputStream os, IObjectInfo info)
            {
                PrimitiveCodec.WriteLong(os, info.GetInternalID());
            }

            public IObjectInfo Decode(ByteArrayInputStream @is)
            {
                var id = PrimitiveCodec.ReadLong(@is);
                if (id == -1)
                {
                    return null;
                }
                return new LazyObjectReference(_enclosing.Transaction(), (int) id);
            }
        }

        internal interface IObjectInfoEncoder
        {
            void Encode(ByteArrayOutputStream os, IObjectInfo info);
            IObjectInfo Decode(ByteArrayInputStream @is);
        }

        private sealed class _IRunnable_111 : IRunnable
        {
            private readonly MCommittedInfo _enclosing;
            private readonly CallbackObjectInfoCollections callbackInfos;
            private readonly int dispatcherID;

            public _IRunnable_111(MCommittedInfo _enclosing, CallbackObjectInfoCollections callbackInfos
                , int dispatcherID)
            {
                this._enclosing = _enclosing;
                this.callbackInfos = callbackInfos;
                this.dispatcherID = dispatcherID;
            }

            public void Run()
            {
                if (_enclosing.Container().IsClosed())
                {
                    return;
                }
                _enclosing.Container().Callbacks().CommitOnCompleted(_enclosing.Transaction
                    (), callbackInfos, dispatcherID == ((ClientObjectContainer) _enclosing.Container
                        ()).ServerSideID());
            }
        }
    }
}