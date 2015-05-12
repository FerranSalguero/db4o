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
    public class Slot
    {
        public const int New = -1;
        public const int Update = -2;

        public static readonly Slot Zero = new Slot
            (0, 0);

        public static int MarshalledLength = Const4.IntLength*2;
        private readonly int _address;
        private readonly int _length;

        public Slot(int address, int length)
        {
            _address = address;
            _length = length;
        }

        public virtual int Address()
        {
            return _address;
        }

        public virtual int Length()
        {
            return _length;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            if (!(obj is Slot))
            {
                return false;
            }
            var other = (Slot)
                obj;
            return (_address == other._address) && (Length() == other.Length());
        }

        public override int GetHashCode()
        {
            return _address ^ Length();
        }

        public virtual Slot SubSlot(int offset)
        {
            return new Slot(_address + offset, Length() - offset
                );
        }

        public override string ToString()
        {
            return "[A:" + _address + ",L:" + Length() + "]";
        }

        public virtual Slot Truncate(int requiredLength)
        {
            return new Slot(_address, requiredLength);
        }

        public virtual int CompareByAddress(Slot slot)
        {
            // FIXME: This is the wrong way around !!!
            // Fix here and in all referers.
            var res = slot._address - _address;
            if (res != 0)
            {
                return res;
            }
            return slot.Length() - Length();
        }

        public virtual int CompareByLength(Slot slot)
        {
            // FIXME: This is the wrong way around !!!
            // Fix here and in all referers.
            var res = slot.Length() - Length();
            if (res != 0)
            {
                return res;
            }
            return slot._address - _address;
        }

        public virtual bool IsDirectlyPreceding(Slot other
            )
        {
            return _address + Length() == other._address;
        }

        public virtual Slot Append(Slot
            slot)
        {
            return new Slot(Address(), _length + slot.Length()
                );
        }

        public virtual bool IsNull()
        {
            return Address() == 0 || Length() == 0;
        }

        public virtual bool IsNew()
        {
            return _address == New;
        }

        public virtual bool IsUpdate()
        {
            return _address == Update;
        }

        public static bool IsNull(Slot slot)
        {
            return slot == null || slot.IsNull();
        }
    }
}