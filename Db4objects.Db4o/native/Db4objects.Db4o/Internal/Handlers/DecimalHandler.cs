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
using Db4objects.Db4o.Marshall;

namespace Db4objects.Db4o.Internal.Handlers
{
    public class DecimalHandler : IntegralTypeHandler
    {
        public override object DefaultValue()
        {
            return (decimal) 0;
        }

        public override object Read(byte[] bytes, int offset)
        {
            var ints = new int[4];
            offset += 3;
            for (var i = 0; i < 4; i ++)
            {
                ints[i] = (bytes[offset] & 255 | (bytes[--offset] & 255) << 8 | (bytes[--offset] & 255) << 16 |
                           bytes[--offset] << 24);
                offset += 7;
            }
            return new decimal(ints);
        }

        public override int TypeID()
        {
            return 21;
        }

        public override void Write(object obj, byte[] bytes, int offset)
        {
            var dec = (decimal) obj;
            var ints = decimal.GetBits(dec);
            offset += 4;
            for (var i = 0; i < 4; i ++)
            {
                bytes[--offset] = (byte) ints[i];
                bytes[--offset] = (byte) (ints[i] >>= 8);
                bytes[--offset] = (byte) (ints[i] >>= 8);
                bytes[--offset] = (byte) (ints[i] >>= 8);
                offset += 8;
            }
        }

        public override object Read(IReadContext context)
        {
            var bytes = new byte[16];
            var ints = new int[4];
            var offset = 4;
            context.ReadBytes(bytes);
            for (var i = 0; i < 4; i++)
            {
                ints[i] = (
                    bytes[--offset] & 255 |
                    (bytes[--offset] & 255) << 8 |
                    (bytes[--offset] & 255) << 16 |
                    (bytes[--offset] & 255) << 24
                    );
                offset += 8;
            }
            return new decimal(ints);
        }

        public override void Write(IWriteContext context, object obj)
        {
            var dec = (decimal) obj;
            var bytes = new byte[16];
            var offset = 4;
            var ints = decimal.GetBits(dec);
            for (var i = 0; i < 4; i++)
            {
                bytes[--offset] = (byte) ints[i];
                bytes[--offset] = (byte) (ints[i] >>= 8);
                bytes[--offset] = (byte) (ints[i] >>= 8);
                bytes[--offset] = (byte) (ints[i] >>= 8);
                offset += 8;
            }
            context.WriteBytes(bytes);
        }

        public override IPreparedComparison InternalPrepareComparison(object obj)
        {
            return new PreparedComparisonFor<decimal>(((decimal) obj));
        }
    }
}