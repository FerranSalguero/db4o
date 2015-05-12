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
using System.IO;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Internal.Mapping;
using Db4objects.Db4o.Internal.Marshall;
using Db4objects.Db4o.Marshall;
using Db4objects.Db4o.Typehandlers;

namespace Db4objects.Db4o.Internal.Handlers.Versions
{
    /// <exclude></exclude>
    public class OpenTypeHandler0 : OpenTypeHandler2
    {
        public OpenTypeHandler0(ObjectContainerBase container) : base(container)
        {
        }

        public override object Read(IReadContext context)
        {
            return context.ReadObject();
        }

        public override ITypeHandler4 ReadCandidateHandler(QueryingReadContext context)
        {
            var id = 0;
            var offset = context.Offset();
            try
            {
                id = context.ReadInt();
            }
            catch (Exception)
            {
            }
            context.Seek(offset);
            if (id != 0)
            {
                var reader = context.Container().ReadStatefulBufferById(context.Transaction
                    (), id);
                if (reader != null)
                {
                    var oh = new ObjectHeader(context.Container(), reader);
                    try
                    {
                        if (oh.ClassMetadata() != null)
                        {
                            context.Buffer(reader);
                            return oh.ClassMetadata().SeekCandidateHandler(context);
                        }
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
            // TODO: Check Exception Types
            // Errors typically occur, if classes don't match
            return null;
        }

        public override ObjectID ReadObjectID(IInternalReadContext context)
        {
            var id = context.ReadInt();
            return id == 0 ? ObjectID.IsNull : new ObjectID(id);
        }

        public override void Defragment(IDefragmentContext context)
        {
            var sourceId = context.SourceBuffer().ReadInt();
            if (sourceId == 0)
            {
                context.TargetBuffer().WriteInt(0);
                return;
            }
            var targetId = 0;
            try
            {
                targetId = context.MappedID(sourceId);
            }
            catch (MappingNotFoundException)
            {
                targetId = CopyDependentSlot(context, sourceId);
            }
            context.TargetBuffer().WriteInt(targetId);
        }

        private int CopyDependentSlot(IDefragmentContext context, int sourceId)
        {
            try
            {
                var sourceBuffer = context.SourceBufferById(sourceId);
                var targetPayloadSlot = context.AllocateTargetSlot(sourceBuffer.Length());
                var targetId = context.Services().TargetNewId();
                context.Services().MapIDs(sourceId, targetId, false);
                context.Services().Mapping().MapId(targetId, targetPayloadSlot);
                var payloadContext = new DefragmentContextImpl(sourceBuffer, (DefragmentContextImpl
                    ) context);
                var clazzId = payloadContext.CopyIDReturnOriginalID();
                var payloadHandler = payloadContext.TypeHandlerForId(clazzId);
                var versionedPayloadHandler = HandlerRegistry.CorrectHandlerVersion(payloadContext
                    , payloadHandler);
                versionedPayloadHandler.Defragment(payloadContext);
                payloadContext.WriteToTarget(targetPayloadSlot.Address());
                return targetId;
            }
            catch (IOException ioexc)
            {
                throw new Db4oIOException(ioexc);
            }
        }
    }
}