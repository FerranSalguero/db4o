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
using Db4objects.Db4o.Internal.Btree;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Btree
{
    public class BTreeNodeTestCase : BTreeTestCaseBase
    {
        private readonly int[] keys =
        {
            -5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 7, 9
        };

        public static void Main(string[] args)
        {
            new BTreeNodeTestCase().RunSolo();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Db4oSetupAfterStore()
        {
            base.Db4oSetupAfterStore();
            Add(keys);
            Commit();
        }

        public virtual void TestLastKeyIndex()
        {
            var node = Node(3);
            Assert.AreEqual(1, node.LastKeyIndex(Trans()));
            var trans = NewTransaction();
            _btree.Add(trans, 5);
            Assert.AreEqual(1, node.LastKeyIndex(Trans()));
            _btree.Commit(trans);
            Assert.AreEqual(2, node.LastKeyIndex(Trans()));
        }

        private BTreeNode Node(int value)
        {
            var range = Search(value);
            var i = range.Pointers();
            i.MoveNext();
            var firstPointer = (BTreePointer) i.Current;
            var node = firstPointer.Node();
            node.DebugLoadFully(SystemTrans());
            return node;
        }

        public virtual void TestLastPointer()
        {
            var node = Node(3);
            var lastPointer = node.LastPointer(Trans());
            AssertPointerKey(4, lastPointer);
        }

        public virtual void TestTransactionalSize()
        {
            var node = Node(3);
            AssertTransactionalSize(node);
            var id = node.GetID();
            var readNode = new BTreeNode(id, _btree);
            AssertTransactionalSize(readNode);
        }

        private void AssertTransactionalSize(BTreeNode node)
        {
            var otherTrans = NewTransaction();
            var originalSize = node.Size(Trans());
            Assert.IsGreater(0, originalSize);
            for (var i = originalSize - 1; i > 0; i--)
            {
                var key = node.Key(Trans(), i);
                node.Remove(Trans(), PrepareComparison(key), key, i);
            }
            Assert.AreEqual(1, node.Size(Trans()));
            Assert.AreEqual(originalSize, node.Size(otherTrans));
            node.Commit(Trans());
            Assert.AreEqual(1, node.Size(otherTrans));
            var newKey = node.Key(Trans(), 0);
            node.Add(Trans(), PrepareComparison(newKey), newKey);
            Assert.AreEqual(2, node.Size(Trans()));
            Assert.AreEqual(1, node.Size(otherTrans));
            node.Commit(Trans());
            Assert.AreEqual(2, node.Size(Trans()));
            Assert.AreEqual(2, node.Size(otherTrans));
            node.Remove(Trans(), PrepareComparison(newKey), newKey, 1);
            Assert.AreEqual(1, node.Size(Trans()));
            Assert.AreEqual(2, node.Size(otherTrans));
            node.Add(Trans(), PrepareComparison(newKey), newKey);
            Assert.AreEqual(2, node.Size(Trans()));
            Assert.AreEqual(2, node.Size(otherTrans));
        }

        private IPreparedComparison PrepareComparison(object key)
        {
            return _btree.KeyHandler().PrepareComparison(Context(), key);
        }
    }
}