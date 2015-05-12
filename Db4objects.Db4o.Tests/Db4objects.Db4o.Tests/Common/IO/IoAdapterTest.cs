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
    public class IoAdapterTest : IoAdapterTestUnitBase
    {
        public static void Main(string[] args)
        {
            new ConsoleTestRunner(typeof (IoAdapterTest)).Run();
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestReadWrite()
        {
            _adapter.Seek(0);
            var count = 1024*8 + 10;
            var data = new byte[count];
            for (var i = 0; i < count; ++i)
            {
                data[i] = (byte) (i%256);
            }
            _adapter.Write(data);
            _adapter.Seek(0);
            var readBytes = new byte[count];
            _adapter.Read(readBytes);
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
                _adapter.Write(data);
            }
            var readBuffer = new byte[dataSize];
            for (var i = 0; i < 64; ++i)
            {
                _adapter.Seek(dataSize*(63 - i));
                _adapter.Read(readBuffer);
                ArrayAssert.AreEqual(data, readBuffer);
            }
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestSeek()
        {
            var count = 1024*2 + 10;
            var data = NewDataArray(count);
            _adapter.Write(data);
            var readBytes = new byte[count];
            _adapter.Seek(0);
            _adapter.Read(readBytes);
            for (var i = 0; i < count; i++)
            {
                Assert.AreEqual(data[i], readBytes[i]);
            }
            _adapter.Seek(20);
            _adapter.Read(readBytes);
            for (var i = 0; i < count - 20; i++)
            {
                Assert.AreEqual(data[i + 20], readBytes[i]);
            }
            var writtenData = new byte[10];
            for (var i = 0; i < writtenData.Length; ++i)
            {
                writtenData[i] = (byte) i;
            }
            _adapter.Seek(1000);
            _adapter.Write(writtenData);
            _adapter.Seek(1000);
            var readCount = _adapter.Read(readBytes, 10);
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
                "short string", "this is a really long string, just to make sure that all IoAdapters work correctly. "
            };
            for (var j = 0; j < strs.Length; j++)
            {
                AssertReadWriteString(_adapter, strs[j]);
            }
        }

        /// <exception cref="System.Exception"></exception>
        private void AssertReadWriteString(IoAdapter adapter, string str)
        {
            var data = Runtime.GetBytesForString(str);
            var read = new byte[2048];
            adapter.Seek(0);
            adapter.Write(data);
            adapter.Seek(0);
            adapter.Read(read);
            Assert.AreEqual(str, Runtime.GetStringForBytes(read, 0, data.Length));
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void _testReadWriteAheadFileEnd()
        {
            var str = "this is a really long string, just to make sure that all IoAdapters work correctly. ";
            AssertReadWriteAheadFileEnd(_adapter, str);
        }

        /// <exception cref="System.Exception"></exception>
        private void AssertReadWriteAheadFileEnd(IoAdapter adapter, string str)
        {
            var data = Runtime.GetBytesForString(str);
            var read = new byte[2048];
            adapter.Seek(10);
            var readBytes = adapter.Read(data);
            Assert.AreEqual(-1, readBytes);
            Assert.AreEqual(0, adapter.GetLength());
            adapter.Seek(0);
            readBytes = adapter.Read(data);
            Assert.AreEqual(-1, readBytes);
            Assert.AreEqual(0, adapter.GetLength());
            adapter.Seek(10);
            adapter.Write(data);
            Assert.AreEqual(10 + data.Length, adapter.GetLength());
            adapter.Seek(0);
            readBytes = adapter.Read(read);
            Assert.AreEqual(10 + data.Length, readBytes);
            adapter.Seek(20 + data.Length);
            readBytes = adapter.Read(read);
            Assert.AreEqual(-1, readBytes);
            adapter.Seek(1024 + data.Length);
            readBytes = adapter.Read(read);
            Assert.AreEqual(-1, readBytes);
            adapter.Seek(1200);
            adapter.Write(data);
            adapter.Seek(0);
            readBytes = adapter.Read(read);
            Assert.AreEqual(1200 + data.Length, readBytes);
        }
    }
}