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
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Foundation
{
    public class BufferTestCase : ITestCase
    {
        private const int Readerlength = 64;

        public virtual void TestCopy()
        {
            var from = new ByteArrayBuffer(Readerlength);
            for (var i = 0; i < Readerlength; i++)
            {
                from.WriteByte((byte) i);
            }
            var to = new ByteArrayBuffer(Readerlength - 1);
            from.CopyTo(to, 1, 2, 10);
            Assert.AreEqual(0, to.ReadByte());
            Assert.AreEqual(0, to.ReadByte());
            for (var i = 1; i <= 10; i++)
            {
                Assert.AreEqual((byte) i, to.ReadByte());
            }
            for (var i = 12; i < Readerlength - 1; i++)
            {
                Assert.AreEqual(0, to.ReadByte());
            }
        }
    }
}