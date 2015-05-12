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

using Db4objects.Db4o.IO;
using Db4oUnit;
using Sharpen;

namespace Db4objects.Db4o.Tests.Common.IO
{
    public class BinTest : StorageTestUnitBase
    {
        public static void Main(string[] args)
        {
            new ConsoleTestRunner(typeof (BinTest)).Run();
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestReadWrite()
        {
            var count = 1024*8 + 10;
            var data = new byte[count];
            for (var i = 0; i < count; ++i)
            {
                data[i] = (byte) (i%256);
            }
            _bin.Write(0, data, data.Length);
            _bin.Sync();
            var readBytes = new byte[count];
            _bin.Read(0, readBytes, readBytes.Length);
            for (var i = 0; i < count; i++)
            {
                Assert.AreEqual(data[i], readBytes[i]);
            }
        }

        public virtual void TestHugeFile()
        {
            var dataSize = 1024*2;
            var data = NewDataArray(dataSize);
            for (var i = 0; i < 64; ++i)
            {
                _bin.Write(i*data.Length, data, data.Length);
            }
            var readBuffer = new byte[dataSize];
            for (var i = 0; i < 64; ++i)
            {
                _bin.Read(dataSize*(63 - i), readBuffer, readBuffer.Length);
                ArrayAssert.AreEqual(data, readBuffer);
            }
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestSeek()
        {
            var count = 1024*2 + 10;
            var data = NewDataArray(count);
            _bin.Write(0, data, data.Length);
            var readBytes = new byte[count];
            _bin.Read(0, readBytes, readBytes.Length);
            for (var i = 0; i < count; i++)
            {
                Assert.AreEqual(data[i], readBytes[i]);
            }
            _bin.Read(20, readBytes, readBytes.Length);
            for (var i = 0; i < count - 20; i++)
            {
                Assert.AreEqual(data[i + 20], readBytes[i]);
            }
            var writtenData = new byte[10];
            for (var i = 0; i < writtenData.Length; ++i)
            {
                writtenData[i] = (byte) i;
            }
            _bin.Write(1000, writtenData, writtenData.Length);
            var readCount = _bin.Read(1000, readBytes, 10);
            Assert.AreEqual(10, readCount);
            for (var i = 0; i < readCount; ++i)
            {
                Assert.AreEqual(i, readBytes[i]);
            }
        }

        private byte[] NewDataArray(int count)
        {
            var data = new byte[count];
            for (var i = 0; i < data.Length; ++i)
            {
                data[i] = (byte) (i%256);
            }
            return data;
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestReadWriteBytes()
        {
            string[] strs =
            {
                "short string",
                "this is a really long string, just to make sure all Storage implementations work correctly. "
            };
            for (var j = 0; j < strs.Length; j++)
            {
                AssertReadWriteString(_bin, strs[j]);
            }
        }

        /// <exception cref="System.Exception"></exception>
        private void AssertReadWriteString(IBin adapter, string str)
        {
            var data = Runtime.GetBytesForString(str);
            var read = new byte[2048];
            adapter.Write(0, data, data.Length);
            adapter.Read(0, read, read.Length);
            Assert.AreEqual(str, Runtime.GetStringForBytes(read, 0, data.Length));
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void _testReadWriteAheadFileEnd()
        {
            var str =
                "this is a really long string, just to make sure that all Storage implementations work correctly. ";
            AssertReadWriteAheadFileEnd(_bin, str);
        }

        /// <exception cref="System.Exception"></exception>
        private void AssertReadWriteAheadFileEnd(IBin adapter, string str)
        {
            var data = Runtime.GetBytesForString(str);
            var read = new byte[2048];
            var readBytes = adapter.Read(10, data, data.Length);
            Assert.AreEqual(-1, readBytes);
            Assert.AreEqual(0, adapter.Length());
            readBytes = adapter.Read(0, data, data.Length);
            Assert.AreEqual(-1, readBytes);
            Assert.AreEqual(0, adapter.Length());
            adapter.Write(10, data, data.Length);
            Assert.AreEqual(10 + data.Length, adapter.Length());
            readBytes = adapter.Read(0, read, read.Length);
            Assert.AreEqual(10 + data.Length, readBytes);
            readBytes = adapter.Read(20 + data.Length, read, read.Length);
            Assert.AreEqual(-1, readBytes);
            readBytes = adapter.Read(1024 + data.Length, read, read.Length);
            Assert.AreEqual(-1, readBytes);
            adapter.Write(1200, data, data.Length);
            readBytes = adapter.Read(0, read, read.Length);
            Assert.AreEqual(1200 + data.Length, readBytes);
        }
    }
}