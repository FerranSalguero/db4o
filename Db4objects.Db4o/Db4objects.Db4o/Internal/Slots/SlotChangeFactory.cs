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

namespace Db4objects.Db4o.Internal.Slots
{
    /// <exclude></exclude>
    public class SlotChangeFactory
    {
        public static readonly SlotChangeFactory UserObjects
            = new SlotChangeFactory();

        public static readonly SlotChangeFactory SystemObjects
            = new _SlotChangeFactory_20();

        public static readonly SlotChangeFactory IdSystem =
            new _SlotChangeFactory_26();

        public static readonly SlotChangeFactory FreeSpace
            = new _SlotChangeFactory_32();

        private SlotChangeFactory()
        {
        }

        public virtual SlotChange NewInstance(int id)
        {
            return new SlotChange(id);
        }

        private sealed class _SlotChangeFactory_20 : SlotChangeFactory
        {
            public override SlotChange NewInstance(int id)
            {
                return new SystemSlotChange(id);
            }
        }

        private sealed class _SlotChangeFactory_26 : SlotChangeFactory
        {
            public override SlotChange NewInstance(int id)
            {
                return new IdSystemSlotChange(id);
            }
        }

        private sealed class _SlotChangeFactory_32 : SlotChangeFactory
        {
            public override SlotChange NewInstance(int id)
            {
                return new FreespaceSlotChange(id);
            }
        }
    }
}