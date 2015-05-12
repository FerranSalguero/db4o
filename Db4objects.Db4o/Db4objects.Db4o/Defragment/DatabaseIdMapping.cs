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

using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Btree;
using Db4objects.Db4o.Internal.Ids;
using Db4objects.Db4o.Internal.Mapping;
using Db4objects.Db4o.Internal.Slots;

namespace Db4objects.Db4o.Defragment
{
    /// <summary>Database based mapping for IDs during a defragmentation run.</summary>
    /// <remarks>
    ///     Database based mapping for IDs during a defragmentation run.
    ///     Use this mapping to keep memory consumption lower than when
    ///     using the
    ///     <see cref="InMemoryIdMapping">InMemoryIdMapping</see>
    ///     .
    /// </remarks>
    /// <seealso cref="Defragment">Defragment</seealso>
    public class DatabaseIdMapping : AbstractIdMapping
    {
        private readonly int _commitFrequency;
        private readonly string _fileName;
        private readonly BTreeSpec _treeSpec;
        private MappedIDPair _cache = new MappedIDPair(0, 0);
        private int _idInsertCount;
        private BTree _idTree;
        private LocalObjectContainer _mappingDb;
        private int _slotInsertCount;
        private BTree _slotTree;

        /// <summary>
        ///     Will maintain the ID mapping as a BTree in the file with the given path.
        /// </summary>
        /// <remarks>
        ///     Will maintain the ID mapping as a BTree in the file with the given path.
        ///     If a file exists in this location, it will be DELETED.
        ///     Node size and cache height of the tree will be the default values used by
        ///     the BTree implementation. The tree will never commit.
        /// </remarks>
        /// <param name="fileName">The location where the BTree file should be created.</param>
        public DatabaseIdMapping(string fileName) : this(fileName, null, 0)
        {
        }

        /// <summary>
        ///     Will maintain the ID mapping as a BTree in the file with the given path.
        /// </summary>
        /// <remarks>
        ///     Will maintain the ID mapping as a BTree in the file with the given path.
        ///     If a file exists in this location, it will be DELETED.
        /// </remarks>
        /// <param name="fileName">The location where the BTree file should be created.</param>
        /// <param name="nodeSize">The size of a BTree node</param>
        /// <param name="commitFrequency">
        ///     The number of inserts after which a commit should be issued (&lt;=0: never commit)
        /// </param>
        public DatabaseIdMapping(string fileName, int nodeSize, int commitFrequency) : this
            (fileName, new BTreeSpec(nodeSize), commitFrequency)
        {
        }

        private DatabaseIdMapping(string fileName, BTreeSpec treeSpec,
            int commitFrequency)
        {
            // <=0 : never commit
            _fileName = fileName;
            _treeSpec = treeSpec;
            _commitFrequency = commitFrequency;
        }

        public override int MappedId(int oldID)
        {
            if (_cache.Orig() == oldID)
            {
                return _cache.Mapped();
            }
            var classID = MappedClassID(oldID);
            if (classID != 0)
            {
                return classID;
            }
            var range = _idTree.SearchRange(Trans(), new MappedIDPair(oldID, 0));
            var pointers = range.Pointers();
            if (pointers.MoveNext())
            {
                var pointer = (BTreePointer) pointers.Current;
                _cache = (MappedIDPair) pointer.Key();
                return _cache.Mapped();
            }
            return 0;
        }

        protected override void MapNonClassIDs(int origID, int mappedID)
        {
            _cache = new MappedIDPair(origID, mappedID);
            _idTree.Add(Trans(), _cache);
            if (_commitFrequency > 0)
            {
                _idInsertCount++;
                if (_commitFrequency == _idInsertCount)
                {
                    _idTree.Commit(Trans());
                    _idInsertCount = 0;
                }
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Open()
        {
            _mappingDb = DefragmentServicesImpl.FreshTempFile(_fileName, 1);
            _idTree = (_treeSpec == null
                ? new BTree(Trans(), 0, new MappedIDPairHandler())
                : new BTree(Trans(), 0, new MappedIDPairHandler(), _treeSpec.NodeSize()));
            _slotTree = (_treeSpec == null
                ? new BTree(Trans(), 0, new BTreeIdSystem.IdSlotMappingHandler
                    ())
                : new BTree(Trans(), 0, new BTreeIdSystem.IdSlotMappingHandler(), _treeSpec.
                    NodeSize()));
        }

        public override void Close()
        {
            _mappingDb.Close();
        }

        private Transaction Trans()
        {
            return _mappingDb.SystemTransaction();
        }

        public override void MapId(int id, Slot slot)
        {
            _slotTree.Add(Trans(), new IdSlotMapping(id, slot.Address(), slot.Length()));
            if (_commitFrequency > 0)
            {
                _slotInsertCount++;
                if (_commitFrequency == _slotInsertCount)
                {
                    _slotTree.Commit(Trans());
                    _slotInsertCount = 0;
                }
            }
        }

        public override IVisitable SlotChanges()
        {
            return new _IVisitable_137(this);
        }

        public override int AddressForId(int id)
        {
            var range = _slotTree.SearchRange(Trans(), new IdSlotMapping(id, 0, 0));
            var pointers = range.Pointers();
            if (pointers.MoveNext())
            {
                var pointer = (BTreePointer) pointers.Current;
                return ((IdSlotMapping) pointer.Key())._address;
            }
            return 0;
        }

        public override void Commit()
        {
            _mappingDb.Commit();
        }

        private class BTreeSpec
        {
            private readonly int _nodeSize;

            public BTreeSpec(int nodeSize)
            {
                _nodeSize = nodeSize;
            }

            public virtual int NodeSize()
            {
                return _nodeSize;
            }
        }

        private sealed class _IVisitable_137 : IVisitable
        {
            private readonly DatabaseIdMapping _enclosing;

            public _IVisitable_137(DatabaseIdMapping _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Accept(IVisitor4 outSideVisitor)
            {
                _enclosing._slotTree.TraverseKeys(_enclosing.Trans(), new _IVisitor4_139
                    (outSideVisitor));
            }

            private sealed class _IVisitor4_139 : IVisitor4
            {
                private readonly IVisitor4 outSideVisitor;

                public _IVisitor4_139(IVisitor4 outSideVisitor)
                {
                    this.outSideVisitor = outSideVisitor;
                }

                public void Visit(object idSlotMapping)
                {
                    var slotChange = new SlotChange(((IdSlotMapping) idSlotMapping)._id);
                    slotChange.NotifySlotCreated(((IdSlotMapping) idSlotMapping).Slot());
                    outSideVisitor.Visit(slotChange);
                }
            }
        }
    }
}