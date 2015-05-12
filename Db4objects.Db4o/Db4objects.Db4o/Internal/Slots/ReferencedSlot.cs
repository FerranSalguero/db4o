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

namespace Db4objects.Db4o.Internal.Slots
{
    /// <exclude></exclude>
    public class ReferencedSlot : TreeInt
    {
        private int _references;
        private Slot _slot;

        public ReferencedSlot(int a_key) : base(a_key)
        {
        }

        public override object ShallowClone()
        {
            var rs = new ReferencedSlot
                (_key);
            rs._slot = _slot;
            rs._references = _references;
            return ShallowCloneInternal(rs);
        }

        public virtual void PointTo(Slot slot)
        {
            _slot = slot;
        }

        public virtual Tree Free(LocalObjectContainer file, Tree treeRoot, Slot
            slot)
        {
            file.Free(_slot.Address(), _slot.Length());
            if (RemoveReferenceIsLast())
            {
                if (treeRoot != null)
                {
                    return treeRoot.RemoveNode(this);
                }
            }
            PointTo(slot);
            return treeRoot;
        }

        public virtual bool AddReferenceIsFirst()
        {
            _references++;
            return (_references == 1);
        }

        public virtual bool RemoveReferenceIsLast()
        {
            _references--;
            return _references < 1;
        }

        public virtual Slot Slot()
        {
            return _slot;
        }
    }
}