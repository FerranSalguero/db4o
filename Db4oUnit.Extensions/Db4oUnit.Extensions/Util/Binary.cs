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

namespace Db4oUnit.Extensions.Util
{
    /// <exclude></exclude>
    public class Binary
    {
        public static long LongForBits(long bits)
        {
            return (long) ((Math.Pow(2, bits)) - 1);
        }

        public static int NumberOfBits(long l)
        {
            if (l < 0)
            {
                throw new ArgumentException();
            }
            long bit = 1;
            var counter = 0;
            for (var i = 0; i < 64; i++)
            {
                if ((l & bit) == 0)
                {
                    counter++;
                }
                else
                {
                    counter = 0;
                }
                bit = bit << 1;
            }
            return 64 - counter;
        }
    }
}