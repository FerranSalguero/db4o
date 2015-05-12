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

using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Btree;
using Sharpen;

namespace Db4objects.Db4o.Defragment
{
    /// <summary>
    ///     Second step in the defragmenting process: Fills in target file pointer slots, copies
    ///     content slots from source to target and triggers ID remapping therein by calling the
    ///     appropriate db4o/marshaller defrag() implementations.
    /// </summary>
    /// <remarks>
    ///     Second step in the defragmenting process: Fills in target file pointer slots, copies
    ///     content slots from source to target and triggers ID remapping therein by calling the
    ///     appropriate db4o/marshaller defrag() implementations. During the process, the actual address
    ///     mappings for the content slots are registered for use with string indices.
    /// </remarks>
    /// <exclude></exclude>
    internal sealed class SecondPassCommand : IPassCommand
    {
        protected readonly int _objectCommitFrequency;
        protected int _objectCount;

        public SecondPassCommand(int objectCommitFrequency)
        {
            _objectCommitFrequency = objectCommitFrequency;
        }

        /// <exception cref="Db4objects.Db4o.CorruptionException"></exception>
        /// <exception cref="System.IO.IOException"></exception>
        public void ProcessClass(DefragmentServicesImpl services, ClassMetadata classMetadata
            , int id, int classIndexID)
        {
            if (services.MappedID(id, -1) == -1)
            {
                Runtime.Err.WriteLine("MAPPING NOT FOUND: " + id);
            }
            DefragmentContextImpl.ProcessCopy(services, id, new _ISlotCopyHandler_34(classMetadata
                , classIndexID));
        }

        /// <exception cref="Db4objects.Db4o.CorruptionException"></exception>
        /// <exception cref="System.IO.IOException"></exception>
        public void ProcessObjectSlot(DefragmentServicesImpl services, ClassMetadata classMetadata
            , int id)
        {
            var sourceBuffer = services.SourceBufferByID(id);
            DefragmentContextImpl.ProcessCopy(services, id, new _ISlotCopyHandler_43(this, services
                ), sourceBuffer);
        }

        /// <exception cref="Db4objects.Db4o.CorruptionException"></exception>
        /// <exception cref="System.IO.IOException"></exception>
        public void ProcessClassCollection(DefragmentServicesImpl services)
        {
            DefragmentContextImpl.ProcessCopy(services, services.SourceClassCollectionID(), new
                _ISlotCopyHandler_59(services));
        }

        /// <exception cref="Db4objects.Db4o.CorruptionException"></exception>
        /// <exception cref="System.IO.IOException"></exception>
        public void ProcessBTree(DefragmentServicesImpl context, BTree btree)
        {
            btree.DefragBTree(context);
        }

        public void Flush(DefragmentServicesImpl context)
        {
        }

        private sealed class _ISlotCopyHandler_34 : ISlotCopyHandler
        {
            private readonly int classIndexID;
            private readonly ClassMetadata classMetadata;

            public _ISlotCopyHandler_34(ClassMetadata classMetadata, int classIndexID)
            {
                this.classMetadata = classMetadata;
                this.classIndexID = classIndexID;
            }

            public void ProcessCopy(DefragmentContextImpl context)
            {
                classMetadata.DefragClass(context, classIndexID);
            }
        }

        private sealed class _ISlotCopyHandler_43 : ISlotCopyHandler
        {
            private readonly SecondPassCommand _enclosing;
            private readonly DefragmentServicesImpl services;

            public _ISlotCopyHandler_43(SecondPassCommand _enclosing, DefragmentServicesImpl
                services)
            {
                this._enclosing = _enclosing;
                this.services = services;
            }

            public void ProcessCopy(DefragmentContextImpl context)
            {
                ClassMetadata.DefragObject(context);
                if (_enclosing._objectCommitFrequency > 0)
                {
                    _enclosing._objectCount++;
                    if (_enclosing._objectCount == _enclosing._objectCommitFrequency)
                    {
                        services.TargetCommit();
                        services.Mapping().Commit();
                        _enclosing._objectCount = 0;
                    }
                }
            }
        }

        private sealed class _ISlotCopyHandler_59 : ISlotCopyHandler
        {
            private readonly DefragmentServicesImpl services;

            public _ISlotCopyHandler_59(DefragmentServicesImpl services)
            {
                this.services = services;
            }

            public void ProcessCopy(DefragmentContextImpl context)
            {
                var acceptedClasses = 0;
                var numClassesOffset = context.TargetBuffer().Offset();
                acceptedClasses = CopyAcceptedClasses(context, acceptedClasses);
                WriteIntAt(context.TargetBuffer(), numClassesOffset, acceptedClasses);
            }

            private int CopyAcceptedClasses(DefragmentContextImpl context, int acceptedClasses
                )
            {
                var numClasses = context.ReadInt();
                for (var classIdx = 0; classIdx < numClasses; classIdx++)
                {
                    var classId = context.SourceBuffer().ReadInt();
                    if (!Accept(classId))
                    {
                        continue;
                    }
                    ++acceptedClasses;
                    context.WriteMappedID(classId);
                }
                return acceptedClasses;
            }

            private void WriteIntAt(ByteArrayBuffer target, int offset, int value)
            {
                var currentOffset = target.Offset();
                target.Seek(offset);
                target.WriteInt(value);
                target.Seek(currentOffset);
            }

            private bool Accept(int classId)
            {
                return services.Accept(services.ClassMetadataForId(classId));
            }
        }
    }
}