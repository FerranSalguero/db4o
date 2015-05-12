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
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Handlers
{
    public class ByteHandlerTestCase : TypeHandlerTestCaseBase
    {
        public static void Main(string[] args)
        {
            new ByteHandlerTestCase().RunSolo();
        }

        private ByteHandler ByteHandler()
        {
            return new ByteHandler();
        }

        public virtual void TestReadWrite()
        {
            var writeContext = new MockWriteContext(Db());
            byte expected = unchecked(0x61);
            ByteHandler().Write(writeContext, expected);
            var readContext = new MockReadContext(writeContext);
            var byteValue = (byte) ByteHandler().Read(readContext);
            Assert.AreEqual(expected, byteValue);
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestStoreObject()
        {
            var storedItem = new Item(5, 6);
            DoTestStoreObject(storedItem);
        }

        public class Item
        {
            public byte _byte;
            public byte _byteWrapper;

            public Item(byte b, byte wrapper)
            {
                _byte = b;
                _byteWrapper = wrapper;
            }

            public override bool Equals(object obj)
            {
                if (obj == this)
                {
                    return true;
                }
                if (!(obj is Item))
                {
                    return false;
                }
                var other = (Item) obj;
                return (other._byte == _byte) && _byteWrapper.Equals(other._byteWrapper
                    );
            }

            public override string ToString()
            {
                return "[" + _byte + "," + _byteWrapper + "]";
            }
        }
    }
}