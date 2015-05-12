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

using Db4objects.Db4o.Internal.Handlers;
using Db4objects.Db4o.Internal.Marshall;

namespace Db4objects.Db4o.Internal.Convert.Conversions
{
    /// <exclude></exclude>
    public class VersionNumberToCommitTimestamp_8_0 : Conversion
    {
        public const int Version = 12;
        private VersionFieldMetadata versionFieldMetadata;

        public override void Convert(ConversionStage.SystemUpStage stage)
        {
            var container = stage.File();
            if (!container.Config().GenerateCommitTimestamps().DefiniteYes())
            {
                return;
            }
            container.ClassCollection().WriteAllClasses();
            BuildCommitTimestampIndex(container);
            container.SystemTransaction().Commit();
        }

        private void BuildCommitTimestampIndex(LocalObjectContainer container)
        {
            versionFieldMetadata = container.Handlers.Indexes()._version;
            var i = container.ClassCollection().Iterator();
            while (i.MoveNext())
            {
                var clazz = i.CurrentClass();
                if (clazz.HasVersionField() && !clazz.IsStruct())
                {
                    RebuildIndexForClass(container, clazz);
                }
            }
        }

        public virtual bool RebuildIndexForClass(LocalObjectContainer container, ClassMetadata
            classMetadata)
        {
            var ids = classMetadata.GetIDs();
            for (var i = 0; i < ids.Length; i++)
            {
                RebuildIndexForObject(container, (int) ids[i]);
            }
            return ids.Length > 0;
        }

        /// <exception cref="Db4objects.Db4o.Internal.FieldIndexException"></exception>
        protected virtual void RebuildIndexForObject(LocalObjectContainer container, int
            objectId)
        {
            var writer = container.ReadStatefulBufferById(container.SystemTransaction
                (), objectId);
            if (writer != null)
            {
                RebuildIndexForWriter(container, writer, objectId);
            }
        }

        protected virtual void RebuildIndexForWriter(LocalObjectContainer container, StatefulBuffer
            buffer, int objectId)
        {
            var objectHeader = new ObjectHeader(container, buffer);
            var context = new ObjectIdContextImpl(container.SystemTransaction
                (), buffer, objectHeader, objectId);
            var classMetadata = context.ClassMetadata();
            if (classMetadata.IsStruct())
            {
                // We don't keep version information for structs.
                return;
            }
            if (classMetadata.SeekToField(container.SystemTransaction(), buffer, versionFieldMetadata
                ) != HandlerVersion.Invalid)
            {
                var version = ((long) versionFieldMetadata.Read(context));
                if (version != 0)
                {
                    var t = (LocalTransaction) container.SystemTransaction();
                    t.CommitTimestampSupport().Put(container.SystemTransaction(), objectId, version);
                }
            }
        }
    }
}