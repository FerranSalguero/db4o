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
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Btree;
using Db4objects.Db4o.Internal.Handlers;
using Db4objects.Db4o.Internal.Slots;
using Sharpen;

namespace Db4oUnit.Extensions
{
    public class BTreeAssert
    {
        public static void TraverseKeys(IBTreeRange result, IVisitor4 visitor)
        {
            var i = result.Keys();
            while (i.MoveNext())
            {
                visitor.Visit(i.Current);
            }
        }

        public static void AssertKeys(Transaction transaction, BTree btree, int[] keys)
        {
            var visitor = ExpectingVisitor.CreateExpectingVisitor(keys);
            btree.TraverseKeys(transaction, visitor);
            visitor.AssertExpectations();
        }

        public static void AssertEmpty(Transaction transaction, BTree tree)
        {
            var visitor = new ExpectingVisitor(new object[0]);
            tree.TraverseKeys(transaction, visitor);
            visitor.AssertExpectations();
            Assert.AreEqual(0, tree.Size(transaction));
        }

        public static void DumpKeys(Transaction trans, BTree tree)
        {
            tree.TraverseKeys(trans, new _IVisitor4_37());
        }

        public static int FillSize(BTree btree)
        {
            return btree.NodeSize() + 1;
        }

        public static int[] NewBTreeNodeSizedArray(BTree btree, int value)
        {
            return IntArrays4.Fill(new int[FillSize(btree)], value);
        }

        public static void AssertRange(int[] expectedKeys, IBTreeRange range)
        {
            Assert.IsNotNull(range);
            var visitor = ExpectingVisitor.CreateSortedExpectingVisitor(expectedKeys
                );
            TraverseKeys(range, visitor);
            visitor.AssertExpectations();
        }

        public static BTree CreateIntKeyBTree(ObjectContainerBase container, int id, int
            nodeSize)
        {
            return new BTree(container.SystemTransaction(), id, new IntHandler(), nodeSize);
        }

        public static void AssertSingleElement(Transaction trans, BTree btree, object element
            )
        {
            Assert.AreEqual(1, btree.Size(trans));
            var result = btree.SearchRange(trans, element);
            var expectingVisitor = new ExpectingVisitor(new[] {element}
                );
            TraverseKeys(result, expectingVisitor);
            expectingVisitor.AssertExpectations();
            expectingVisitor = new ExpectingVisitor(new[] {element});
            btree.TraverseKeys(trans, expectingVisitor);
            expectingVisitor.AssertExpectations();
        }

        /// <exception cref="System.Exception"></exception>
        public static void AssertAllSlotsFreed(LocalTransaction trans, BTree bTree, ICodeBlock
            block)
        {
            var container = (LocalObjectContainer) trans.Container();
            var idSystem = trans.IdSystem();
            var allSlotIDs = bTree.AllNodeIds(trans.SystemTransaction());
            var allSlots = new Collection4();
            while (allSlotIDs.MoveNext())
            {
                var slotID = ((int) allSlotIDs.Current);
                var slot = idSystem.CurrentSlot(slotID);
                allSlots.Add(slot);
            }
            var bTreeSlot = idSystem.CurrentSlot(bTree.GetID());
            allSlots.Add(bTreeSlot);
            var freedSlots = new Collection4();
            var freespaceManager = container.FreespaceManager();
            container.InstallDebugFreespaceManager(new FreespaceManagerForDebug(new _ISlotListener_99
                (freedSlots)));
            block.Run();
            container.InstallDebugFreespaceManager(freespaceManager);
            Assert.IsTrue(freedSlots.ContainsAll(allSlots.GetEnumerator()));
        }

        private sealed class _IVisitor4_37 : IVisitor4
        {
            public void Visit(object obj)
            {
                Runtime.Out.WriteLine(obj);
            }
        }

        private sealed class _ISlotListener_99 : ISlotListener
        {
            private readonly Collection4 freedSlots;

            public _ISlotListener_99(Collection4 freedSlots)
            {
                this.freedSlots = freedSlots;
            }

            public void OnFree(Slot slot)
            {
                freedSlots.Add(slot);
            }
        }
    }
}