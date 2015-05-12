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

#if !SILVERLIGHT
using System;
using System.Collections;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Btree;
using Db4objects.Db4o.Internal.Classindex;
using Db4objects.Db4o.Internal.Fileheader;
using Db4objects.Db4o.Internal.Freespace;
using Db4objects.Db4o.Internal.Ids;
using Db4objects.Db4o.Internal.Slots;
using Sharpen;
using Sharpen.IO;

namespace Db4objects.Db4o.Filestats
{
    /// <summary>
    ///     Collects database file usage statistics and prints them
    ///     to the console.
    /// </summary>
    /// <remarks>
    ///     Collects database file usage statistics and prints them
    ///     to the console.
    /// </remarks>
    public partial class FileUsageStatsCollector
    {
        private readonly IBlockConverter _blockConverter;
        private readonly LocalObjectContainer _db;
        private readonly ISlotMap _slots;
        private readonly IDictionary MiscCollectors;
        private FileUsageStats _stats;

        public FileUsageStatsCollector(IObjectContainer db, bool collectSlots)
        {
            MiscCollectors = new Hashtable();
            RegisterBigSetCollector();
            _db = (LocalObjectContainer) db;
            var blockSize = _db.BlockSize();
            _blockConverter = blockSize > 1
                ? new BlockSizeBlockConverter(blockSize
                    )
                : (IBlockConverter) new DisabledBlockConverter();
            _slots = collectSlots
                ? new SlotMapImpl(_db.FileLength())
                : (ISlotMap) new
                    NullSlotMap();
        }

        /// <summary>Usage: FileUsageStatsCollector <db path> [<collect gaps ( true| false)>]</summary>
        public static void Main(string[] args)
        {
            var dbPath = args[0];
            var collectSlots = args.Length > 1 && "true".Equals(args[1]);
            Runtime.Out.WriteLine(dbPath + ": " + new File(dbPath).Length(
                ));
            var stats = RunStats(dbPath, collectSlots);
            Runtime.Out.WriteLine(stats);
        }

        public static FileUsageStats RunStats(string dbPath)
        {
            return RunStats(dbPath, false);
        }

        public static FileUsageStats RunStats(string dbPath, bool collectSlots)
        {
            return RunStats(dbPath, collectSlots, Db4oEmbedded.NewConfiguration());
        }

        public static FileUsageStats RunStats(string dbPath, bool collectSlots, IEmbeddedConfiguration
            config)
        {
            var db = Db4oEmbedded.OpenFile(config, dbPath);
            try
            {
                return new FileUsageStatsCollector(db, collectSlots).CollectStats
                    ();
            }
            finally
            {
                db.Close();
            }
        }

        public virtual FileUsageStats CollectStats()
        {
            _stats = new FileUsageStats(_db.FileLength(), FileHeaderUsage(), IdSystemUsage(),
                Freespace(), ClassMetadataUsage(), FreespaceUsage(), UuidUsage(), _slots, CommitTimestampUsage
                    ());
            var classRoots = ClassNode.BuildHierarchy(_db.ClassCollection());
            for (var classRootIter = classRoots.GetEnumerator();
                classRootIter.MoveNext
                    ();)
            {
                var classRoot = ((ClassNode) classRootIter.Current);
                CollectClassSlots(classRoot.ClassMetadata());
                CollectClassStats(_stats, classRoot);
            }
            return _stats;
        }

        private long CollectClassStats(FileUsageStats stats, ClassNode classNode)
        {
            long subClassSlotUsage = 0;
            for (var curSubClassIter = classNode.SubClasses().GetEnumerator();
                curSubClassIter
                    .MoveNext();)
            {
                var curSubClass = ((ClassNode) curSubClassIter.Current);
                subClassSlotUsage += CollectClassStats(stats, curSubClass);
            }
            var clazz = classNode.ClassMetadata();
            long classIndexUsage = 0;
            if (clazz.HasClassIndex())
            {
                classIndexUsage = BTreeUsage(((BTreeClassIndexStrategy) clazz.Index()).Btree());
            }
            var fieldIndexUsage = FieldIndexUsage(clazz);
            var instanceUsage = ClassSlotUsage(clazz);
            var totalSlotUsage = instanceUsage.slotUsage;
            var ownSlotUsage = totalSlotUsage - subClassSlotUsage;
            var classStats = new ClassUsageStats(clazz.GetName(), ownSlotUsage, classIndexUsage
                , fieldIndexUsage, instanceUsage.miscUsage);
            stats.AddClassStats(classStats);
            return totalSlotUsage;
        }

        private long FieldIndexUsage(ClassMetadata classMetadata)
        {
            var usage = new LongByRef();
            classMetadata.TraverseDeclaredFields(new _IProcedure4_125(this, usage));
            return usage.value;
        }

        private long BTreeUsage(BTree btree)
        {
            return BTreeUsage(_db, btree, _slots);
        }

        internal static long BTreeUsage(LocalObjectContainer db, BTree btree, ISlotMap slotMap
            )
        {
            return BTreeUsage(db.SystemTransaction(), db.IdSystem(), btree, slotMap);
        }

        private static long BTreeUsage(Transaction transaction, IIdSystem idSystem, BTree
            btree, ISlotMap slotMap)
        {
            var nodeIter = btree.AllNodeIds(transaction);
            var btreeSlot = idSystem.CommittedSlot(btree.GetID
                ());
            slotMap.Add(btreeSlot);
            long usage = btreeSlot.Length();
            while (nodeIter.MoveNext())
            {
                var curNodeId = ((int) nodeIter.Current);
                var slot = idSystem.CommittedSlot(curNodeId);
                slotMap.Add(slot);
                usage += slot.Length();
            }
            return usage;
        }

        private InstanceUsage ClassSlotUsage(ClassMetadata clazz)
        {
            if (!clazz.HasClassIndex())
            {
                return new InstanceUsage(0, 0);
            }
            var miscCollector = ((IMiscCollector) MiscCollectors[clazz.GetName()]);
            var slotUsage = new LongByRef();
            var miscUsage = new LongByRef();
            var index = (BTreeClassIndexStrategy) clazz.Index();
            index.TraverseAll(_db.SystemTransaction(), new _IVisitor4_166(this, slotUsage, miscCollector
                , miscUsage));
            return new InstanceUsage(slotUsage.value, miscUsage.value
                );
        }

        private void CollectClassSlots(ClassMetadata clazz)
        {
            if (!clazz.HasClassIndex())
            {
                return;
            }
            var index = (BTreeClassIndexStrategy) clazz.Index();
            index.TraverseAll(_db.SystemTransaction(), new _IVisitor4_182(this));
        }

        private long Freespace()
        {
            _db.FreespaceManager().Traverse(new _IVisitor4_190(this));
            return _db.FreespaceManager().TotalFreespace();
        }

        private long FreespaceUsage()
        {
            return FreespaceUsage(_db.FreespaceManager());
        }

        private long FreespaceUsage(IFreespaceManager fsm)
        {
            if (fsm is InMemoryFreespaceManager)
            {
                return 0;
            }
            if (fsm is BTreeFreespaceManager)
            {
                return BTreeUsage((BTree) FieldValue(fsm, "_slotsByAddress")) + BTreeUsage((BTree)
                    FieldValue(fsm, "_slotsByLength"));
            }
            if (fsm is BlockAwareFreespaceManager)
            {
                return FreespaceUsage((IFreespaceManager) FieldValue(fsm, "_delegate"));
            }
            throw new InvalidOperationException("Unknown freespace manager: " + fsm);
        }

        private long IdSystemUsage()
        {
            var usage = new IntByRef();
            _db.IdSystem().TraverseOwnSlots(new _IProcedure4_217(this, usage));
            return usage.value;
        }

        private long ClassMetadataUsage()
        {
            var classRepositorySlot = Slot(_db.ClassCollection
                ().GetID());
            _slots.Add(classRepositorySlot);
            long usage = classRepositorySlot.Length();
            var classIdIter = _db.ClassCollection().Ids();
            while (classIdIter.MoveNext())
            {
                var curClassId = (((int) classIdIter.Current));
                var classSlot = Slot(curClassId);
                _slots.Add(classSlot);
                usage += classSlot.Length();
            }
            return usage;
        }

        private long FileHeaderUsage()
        {
            var headerLength = _db.GetFileHeader().Length();
            var usage = _blockConverter.BlockAlignedBytes(headerLength);
            var variablePart = (FileHeaderVariablePart2) FieldValue(_db.GetFileHeader
                (), "_variablePart");
            usage += _blockConverter.BlockAlignedBytes(variablePart.MarshalledLength());
            _slots.Add(new Slot(0, headerLength));
            _slots.Add(new Slot(variablePart.Address(), variablePart
                .MarshalledLength()));
            return usage;
        }

        private long UuidUsage()
        {
            if (_db.SystemData().UuidIndexId() <= 0)
            {
                return 0;
            }
            var index = _db.UUIDIndex().GetIndex(_db.SystemTransaction());
            return index == null ? 0 : BTreeUsage(index);
        }

        private long CommitTimestampUsage()
        {
            var st = (LocalTransaction) _db.SystemTransaction();
            var commitTimestampSupport = st.CommitTimestampSupport();
            if (commitTimestampSupport == null)
            {
                return 0;
            }
            var idToTimestampBtree = commitTimestampSupport.IdToTimestamp();
            var idToTimestampBTreeSize = idToTimestampBtree == null
                ? 0
                : BTreeUsage(idToTimestampBtree
                    );
            var timestampToIdBtree = commitTimestampSupport.TimestampToId();
            var timestampToIdBTreeSize = timestampToIdBtree == null
                ? 0
                : BTreeUsage(timestampToIdBtree
                    );
            return idToTimestampBTreeSize + timestampToIdBTreeSize;
        }

        private int SlotSizeForId(int id)
        {
            return Slot(id).Length();
        }

        private static object FieldValue(object parent, string fieldName)
        {
            return Reflection4.GetFieldValue(parent, fieldName);
        }

        private Slot Slot(int id)
        {
            return _db.IdSystem().CommittedSlot(id);
        }

        private sealed class _IProcedure4_125 : IProcedure4
        {
            private readonly FileUsageStatsCollector _enclosing;
            private readonly LongByRef usage;

            public _IProcedure4_125(FileUsageStatsCollector _enclosing, LongByRef usage)
            {
                this._enclosing = _enclosing;
                this.usage = usage;
            }

            public void Apply(object field)
            {
                if (((FieldMetadata) field).IsVirtual() || !((FieldMetadata) field).HasIndex())
                {
                    return;
                }
                usage.value += _enclosing.BTreeUsage(((FieldMetadata) field).GetIndex(_enclosing
                    ._db.SystemTransaction()));
            }
        }

        private sealed class _IVisitor4_166 : IVisitor4
        {
            private readonly FileUsageStatsCollector _enclosing;
            private readonly IMiscCollector miscCollector;
            private readonly LongByRef miscUsage;
            private readonly LongByRef slotUsage;

            public _IVisitor4_166(FileUsageStatsCollector _enclosing, LongByRef slotUsage, IMiscCollector
                miscCollector, LongByRef miscUsage)
            {
                this._enclosing = _enclosing;
                this.slotUsage = slotUsage;
                this.miscCollector = miscCollector;
                this.miscUsage = miscUsage;
            }

            public void Visit(object id)
            {
                slotUsage.value += _enclosing.SlotSizeForId((((int) id)));
                if (miscCollector != null)
                {
                    miscUsage.value += miscCollector.CollectFor(_enclosing._db, (((int) id)), _enclosing._slots);
                }
            }
        }

        private sealed class _IVisitor4_182 : IVisitor4
        {
            private readonly FileUsageStatsCollector _enclosing;

            public _IVisitor4_182(FileUsageStatsCollector _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Visit(object id)
            {
                _enclosing._slots.Add(_enclosing.Slot((((int) id))));
            }
        }

        private sealed class _IVisitor4_190 : IVisitor4
        {
            private readonly FileUsageStatsCollector _enclosing;

            public _IVisitor4_190(FileUsageStatsCollector _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Visit(object slot)
            {
                _enclosing._slots.Add(((Slot) slot));
            }
        }

        private sealed class _IProcedure4_217 : IProcedure4
        {
            private readonly FileUsageStatsCollector _enclosing;
            private readonly IntByRef usage;

            public _IProcedure4_217(FileUsageStatsCollector _enclosing, IntByRef usage)
            {
                this._enclosing = _enclosing;
                this.usage = usage;
            }

            public void Apply(object idSlot)
            {
                var slot = ((Slot)
                    ((Pair) idSlot).second);
                usage.value += slot.Length();
                _enclosing._slots.Add(slot);
            }
        }

        private class InstanceUsage
        {
            public readonly long miscUsage;
            public readonly long slotUsage;

            public InstanceUsage(long slotUsage, long miscUsage)
            {
                this.slotUsage = slotUsage;
                this.miscUsage = miscUsage;
            }
        }
    }
}

#endif // !SILVERLIGHT