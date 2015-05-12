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

using System.Collections;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Btree;
using Db4objects.Db4o.Internal.Classindex;
using Db4objects.Db4o.Internal.Ids;
using Db4objects.Db4o.Internal.Slots;
using Sharpen;

namespace Db4objects.Db4o.Consistency
{
    public class ConsistencyChecker
    {
        private readonly IList _bogusSlots = new ArrayList();
        private readonly LocalObjectContainer _db;
        private readonly OverlapMap _overlaps;

        public ConsistencyChecker(IObjectContainer db)
        {
            _db = (LocalObjectContainer) db;
            _overlaps = new OverlapMap(_db.BlockConverter());
        }

        public static void Main(string[] args)
        {
            var db = Db4oEmbedded.OpenFile(args[0]);
            try
            {
                Runtime.Out.WriteLine(new ConsistencyChecker(
                    db).CheckSlotConsistency());
            }
            finally
            {
                db.Close();
            }
        }

        public virtual ConsistencyReport CheckSlotConsistency()
        {
            return ((ConsistencyReport) _db.SyncExec(new _IClosure4_38(this)));
        }

        private IList CheckClassIndices()
        {
            IList invalidIds = new ArrayList();
            var idSystem = _db.IdSystem();
            if (!(idSystem is BTreeIdSystem))
            {
                return invalidIds;
            }
            var clazzIter = _db.ClassCollection().Iterator();
            while (clazzIter.MoveNext())
            {
                var clazz = clazzIter.CurrentClass();
                if (!clazz.HasClassIndex())
                {
                    continue;
                }
                var index = (BTreeClassIndexStrategy) clazz.Index();
                index.TraverseAll(_db.SystemTransaction(), new _IVisitor4_64(this, invalidIds, clazz
                    ));
            }
            return invalidIds;
        }

        private IList CheckFieldIndices()
        {
            IList invalidIds = new ArrayList();
            var clazzIter = _db.ClassCollection().Iterator();
            while (clazzIter.MoveNext())
            {
                var clazz = clazzIter.CurrentClass();
                clazz.TraverseDeclaredFields(new _IProcedure4_80(this, invalidIds, clazz));
            }
            return invalidIds;
        }

        private bool IdIsValid(int id)
        {
            try
            {
                return !Slot.IsNull(_db.IdSystem().CommittedSlot(id));
            }
            catch (InvalidIDException)
            {
                return false;
            }
        }

        private void MapFreespace()
        {
            _db.FreespaceManager().Traverse(new _IVisitor4_110(this));
        }

        private void MapIdSystem()
        {
            var idSystem = _db.IdSystem();
            if (!(idSystem is BTreeIdSystem))
            {
                Runtime.Err.WriteLine("No btree id system found - not mapping ids.");
                return;
            }
            ((BTreeIdSystem) idSystem).TraverseIds(new _IVisitor4_127(this));
            idSystem.TraverseOwnSlots(new _IProcedure4_138(this));
        }

        private bool IsBogusSlot(int address, int length)
        {
            return address < 0 || (long) address + length > _db.FileLength();
        }

        private sealed class _IClosure4_38 : IClosure4
        {
            private readonly ConsistencyChecker _enclosing;

            public _IClosure4_38(ConsistencyChecker _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public object Run()
            {
                _enclosing.MapIdSystem();
                _enclosing.MapFreespace();
                return new ConsistencyReport(_enclosing._bogusSlots, _enclosing._overlaps
                    , _enclosing.CheckClassIndices(), _enclosing.CheckFieldIndices());
            }
        }

        private sealed class _IVisitor4_64 : IVisitor4
        {
            private readonly ConsistencyChecker _enclosing;
            private readonly ClassMetadata clazz;
            private readonly IList invalidIds;

            public _IVisitor4_64(ConsistencyChecker _enclosing, IList invalidIds, ClassMetadata
                clazz)
            {
                this._enclosing = _enclosing;
                this.invalidIds = invalidIds;
                this.clazz = clazz;
            }

            public void Visit(object id)
            {
                if (!_enclosing.IdIsValid((((int) id))))
                {
                    invalidIds.Add(new Pair(clazz.GetName(), ((int) id)));
                }
            }
        }

        private sealed class _IProcedure4_80 : IProcedure4
        {
            private readonly ConsistencyChecker _enclosing;
            private readonly ClassMetadata clazz;
            private readonly IList invalidIds;

            public _IProcedure4_80(ConsistencyChecker _enclosing, IList invalidIds, ClassMetadata
                clazz)
            {
                this._enclosing = _enclosing;
                this.invalidIds = invalidIds;
                this.clazz = clazz;
            }

            public void Apply(object field)
            {
                if (!((FieldMetadata) field).HasIndex())
                {
                    return;
                }
                var fieldIndex = ((FieldMetadata) field).GetIndex(_enclosing._db.SystemTransaction
                    ());
                fieldIndex.TraverseKeys(_enclosing._db.SystemTransaction(), new _IVisitor4_86
                    (this, invalidIds, clazz, field));
            }

            private sealed class _IVisitor4_86 : IVisitor4
            {
                private readonly _IProcedure4_80 _enclosing;
                private readonly ClassMetadata clazz;
                private readonly object field;
                private readonly IList invalidIds;

                public _IVisitor4_86(_IProcedure4_80 _enclosing, IList invalidIds, ClassMetadata
                    clazz, object field)
                {
                    this._enclosing = _enclosing;
                    this.invalidIds = invalidIds;
                    this.clazz = clazz;
                    this.field = field;
                }

                public void Visit(object fieldIndexKey)
                {
                    var parentID = ((IFieldIndexKey) fieldIndexKey).ParentID();
                    if (!_enclosing._enclosing.IdIsValid(parentID))
                    {
                        invalidIds.Add(new Pair(clazz.GetName() + "#" + ((FieldMetadata) field).GetName(),
                            parentID));
                    }
                }
            }
        }

        private sealed class _IVisitor4_110 : IVisitor4
        {
            private readonly ConsistencyChecker _enclosing;

            public _IVisitor4_110(ConsistencyChecker _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Visit(object slot)
            {
                var detail = new FreespaceSlotDetail(((Slot) slot));
                if (_enclosing.IsBogusSlot(((Slot) slot).Address(), ((Slot) slot).Length()))
                {
                    _enclosing._bogusSlots.Add(detail);
                }
                _enclosing._overlaps.Add(detail);
            }
        }

        private sealed class _IVisitor4_127 : IVisitor4
        {
            private readonly ConsistencyChecker _enclosing;

            public _IVisitor4_127(ConsistencyChecker _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Visit(object mapping)
            {
                SlotDetail detail = new IdObjectSlotDetail(((IdSlotMapping) mapping)._id, ((IdSlotMapping
                    ) mapping).Slot());
                if (_enclosing.IsBogusSlot(((IdSlotMapping) mapping)._address, ((IdSlotMapping
                    ) mapping)._length))
                {
                    _enclosing._bogusSlots.Add(detail);
                }
                if (((IdSlotMapping) mapping)._address > 0)
                {
                    _enclosing._overlaps.Add(detail);
                }
            }
        }

        private sealed class _IProcedure4_138 : IProcedure4
        {
            private readonly ConsistencyChecker _enclosing;

            public _IProcedure4_138(ConsistencyChecker _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void Apply(object idSlot)
            {
                var id = (((int) ((Pair) idSlot).first));
                var slot = ((Slot) ((Pair) idSlot).second);
                var detail = id > 0
                    ? new IdObjectSlotDetail(id, slot)
                    : (SlotDetail
                        ) new RawObjectSlotDetail(slot);
                if (_enclosing.IsBogusSlot(((Slot) ((Pair) idSlot).second).Address(), ((Slot) (
                    (Pair) idSlot).second).Length()))
                {
                    _enclosing._bogusSlots.Add(detail);
                }
                _enclosing._overlaps.Add(detail);
            }
        }
    }
}