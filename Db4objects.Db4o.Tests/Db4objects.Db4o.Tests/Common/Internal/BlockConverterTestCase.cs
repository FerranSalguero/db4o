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
using Db4objects.Db4o.Internal.Slots;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Internal
{
    /// <exclude></exclude>
    public class BlockConverterTestCase : ITestCase
    {
        private readonly BlockSizeBlockConverter blockConverter = new BlockSizeBlockConverter
            (8);

        public virtual void TestBlocksToBytes()
        {
            int[] blocks = {0, 1, 8, 9};
            int[] bytes = {0, 8, 64, 72};
            for (var i = 0; i < blocks.Length; i++)
            {
                Assert.AreEqual(bytes[i], blockConverter.BlocksToBytes(blocks[i]));
            }
        }

        public virtual void TestBytesToBlocks()
        {
            int[] bytes = {0, 1, 2, 7, 8, 9, 16, 17, 799, 800, 801};
            int[] blocks = {0, 1, 1, 1, 1, 2, 2, 3, 100, 100, 101};
            for (var i = 0; i < blocks.Length; i++)
            {
                Assert.AreEqual(blocks[i], blockConverter.BytesToBlocks(bytes[i]));
            }
        }

        public virtual void TestBlockAlignedBytes()
        {
            int[] bytes = {0, 1, 2, 7, 8, 9, 16, 17, 799, 800, 801};
            int[] aligned = {0, 8, 8, 8, 8, 16, 16, 24, 800, 800, 808};
            for (var i = 0; i < bytes.Length; i++)
            {
                Assert.AreEqual(aligned[i], blockConverter.BlockAlignedBytes(bytes[i]));
            }
        }

        public virtual void TestToBlockedLength()
        {
            int[] bytes = {0, 1, 2, 7, 8, 9, 16, 17, 799, 800, 801};
            int[] blocks = {0, 1, 1, 1, 1, 2, 2, 3, 100, 100, 101};
            for (var i = 0; i < bytes.Length; i++)
            {
                var byteSlot = new Slot(0, bytes[i]);
                var blockedSlot = new Slot(0, blocks[i]);
                Assert.AreEqual(blockedSlot, blockConverter.ToBlockedLength(byteSlot));
            }
        }

        public virtual void TestToNonBlockedLength()
        {
            int[] blocks = {0, 1, 8, 9};
            int[] bytes = {0, 8, 64, 72};
            for (var i = 0; i < blocks.Length; i++)
            {
                var blockedSlot = new Slot(0, blocks[i]);
                var byteSlot = new Slot(0, bytes[i]);
                Assert.AreEqual(byteSlot, blockConverter.ToNonBlockedLength(blockedSlot));
            }
        }
    }
}