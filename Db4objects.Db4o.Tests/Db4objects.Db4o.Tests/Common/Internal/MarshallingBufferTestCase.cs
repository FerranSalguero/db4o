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

namespace Db4objects.Db4o.Tests.Common.Internal
{
    public class MarshallingBufferTestCase : ITestCase
    {
        private const int Data1 = 111;
        private const byte Data2 = 2;
        private const int Data3 = 333;
        private const int Data4 = 444;
        private const int Data5 = 55;

        public virtual void TestWrite()
        {
            var buffer = new MarshallingBuffer();
            buffer.WriteInt(Data1);
            buffer.WriteByte(Data2);
            var content = InspectContent(buffer);
            Assert.AreEqual(Data1, content.ReadInt());
            Assert.AreEqual(Data2, content.ReadByte());
        }

        public virtual void TestTransferLastWrite()
        {
            var buffer = new MarshallingBuffer();
            buffer.WriteInt(Data1);
            var lastOffset = Offset(buffer);
            buffer.WriteByte(Data2);
            var other = new MarshallingBuffer();
            buffer.TransferLastWriteTo(other, true);
            Assert.AreEqual(lastOffset, Offset(buffer));
            var content = InspectContent(other);
            Assert.AreEqual(Data2, content.ReadByte());
        }

        private int Offset(MarshallingBuffer buffer)
        {
            return buffer.TestDelegate().Offset();
        }

        private ByteArrayBuffer InspectContent(MarshallingBuffer buffer)
        {
            var bufferDelegate = buffer.TestDelegate();
            bufferDelegate.Seek(0);
            return bufferDelegate;
        }

        public virtual void TestChildren()
        {
            var buffer = new MarshallingBuffer();
            buffer.WriteInt(Data1);
            buffer.WriteByte(Data2);
            var child = buffer.AddChild();
            child.WriteInt(Data3);
            child.WriteInt(Data4);
            buffer.MergeChildren(null, 0, 0);
            var content = InspectContent(buffer);
            Assert.AreEqual(Data1, content.ReadInt());
            Assert.AreEqual(Data2, content.ReadByte());
            var address = content.ReadInt();
            content.Seek(address);
            Assert.AreEqual(Data3, content.ReadInt());
            Assert.AreEqual(Data4, content.ReadInt());
        }

        public virtual void TestGrandChildren()
        {
            var buffer = new MarshallingBuffer();
            buffer.WriteInt(Data1);
            buffer.WriteByte(Data2);
            var child = buffer.AddChild();
            child.WriteInt(Data3);
            child.WriteInt(Data4);
            var grandChild = child.AddChild();
            grandChild.WriteInt(Data5);
            buffer.MergeChildren(null, 0, 0);
            var content = InspectContent(buffer);
            Assert.AreEqual(Data1, content.ReadInt());
            Assert.AreEqual(Data2, content.ReadByte());
            var address = content.ReadInt();
            content.Seek(address);
            Assert.AreEqual(Data3, content.ReadInt());
            Assert.AreEqual(Data4, content.ReadInt());
            address = content.ReadInt();
            content.Seek(address);
            Assert.AreEqual(Data5, content.ReadInt());
        }

        public virtual void TestLinkOffset()
        {
            var linkOffset = 7;
            var buffer = new MarshallingBuffer();
            buffer.WriteInt(Data1);
            buffer.WriteByte(Data2);
            var child = buffer.AddChild();
            child.WriteInt(Data3);
            child.WriteInt(Data4);
            var grandChild = child.AddChild();
            grandChild.WriteInt(Data5);
            buffer.MergeChildren(null, 0, linkOffset);
            var content = InspectContent(buffer);
            var extendedBuffer = new ByteArrayBuffer(content.Length() + linkOffset
                );
            content.CopyTo(extendedBuffer, 0, linkOffset, content.Length());
            extendedBuffer.Seek(linkOffset);
            Assert.AreEqual(Data1, extendedBuffer.ReadInt());
            Assert.AreEqual(Data2, extendedBuffer.ReadByte());
            var address = extendedBuffer.ReadInt();
            extendedBuffer.Seek(address);
            Assert.AreEqual(Data3, extendedBuffer.ReadInt());
            Assert.AreEqual(Data4, extendedBuffer.ReadInt());
            address = extendedBuffer.ReadInt();
            extendedBuffer.Seek(address);
            Assert.AreEqual(Data5, extendedBuffer.ReadInt());
        }

        public virtual void TestLateChildrenWrite()
        {
            var buffer = new MarshallingBuffer();
            buffer.WriteInt(Data1);
            var child = buffer.AddChild(true, true);
            child.WriteInt(Data3);
            buffer.WriteByte(Data2);
            child.WriteInt(Data4);
            buffer.MergeChildren(null, 0, 0);
            var content = InspectContent(buffer);
            Assert.AreEqual(Data1, content.ReadInt());
            var address = content.ReadInt();
            content.ReadInt();
            // length
            Assert.AreEqual(Data2, content.ReadByte());
            content.Seek(address);
            Assert.AreEqual(Data3, content.ReadInt());
            Assert.AreEqual(Data4, content.ReadInt());
        }
    }
}